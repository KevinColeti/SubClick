// Anonymous XML-RPC flow based on VLSub 0.11.1. See THIRD-PARTY-NOTICES.md.
using System.Globalization;
using System.Net;
using System.Text;

namespace SubClick.Core;

public sealed class OpenSubtitlesProvider : ISubtitleProvider, IDisposable
{
    private const string ClientIdentifier = "VLSub 0.11.1";
    private readonly HttpClient client;
    private readonly TimeSpan timeout;

    public OpenSubtitlesProvider(HttpMessageHandler? handler = null, TimeSpan? timeout = null)
    {
        client = new HttpClient(handler ?? new HttpClientHandler
        {
            AllowAutoRedirect = false, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        }) { Timeout = Timeout.InfiniteTimeSpan };
        this.timeout = timeout ?? TimeSpan.FromSeconds(40);
    }

    private async Task<byte[]> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
        deadline.CancelAfter(timeout);
        request.Headers.TryAddWithoutValidation("User-Agent", ClientIdentifier);
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, deadline.Token).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) throw new SubClickException("ServiceError");
        return await ReadLimitedAsync(response.Content, deadline.Token).ConfigureAwait(false);
    }

    private static async Task<byte[]> ReadLimitedAsync(HttpContent content, CancellationToken ct)
    {
        if (content.Headers.ContentLength > SubtitleFiles.MaxBytes) throw new SubClickException("TooLarge");
        await using var input = await content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        using var output = new MemoryStream();
        var buffer = new byte[16384];
        int count;
        while ((count = await input.ReadAsync(buffer, ct).ConfigureAwait(false)) > 0)
        {
            if (output.Length + count > SubtitleFiles.MaxBytes) throw new SubClickException("TooLarge");
            output.Write(buffer, 0, count);
        }
        return output.ToArray();
    }

    private async Task<object> RpcAsync(string method, CancellationToken ct, params object[] args)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.opensubtitles.org/xml-rpc")
        {
            Content = new StringContent(XmlRpc.Request(method, args), Encoding.UTF8, "text/xml")
        };
        var data = XmlRpc.ParseResponse(Encoding.UTF8.GetString(await SendAsync(request, ct).ConfigureAwait(false)));
        string status = XmlRpc.Text(data, "status");
        // GetSubLanguages returns a catalog without a status field on the live legacy service.
        if (!(method == "GetSubLanguages" && status.Length == 0) && !status.StartsWith("200 ", StringComparison.Ordinal))
            throw new SubClickException("ServiceError");
        return data;
    }

    public async Task<IReadOnlyList<SubtitleLanguage>> GetLanguagesAsync(CancellationToken ct)
    {
        var data = await RpcAsync("GetSubLanguages", ct, "en").ConfigureAwait(false);
        return XmlRpc.Items(XmlRpc.Get(data, "data"))
            .Select(row => new SubtitleLanguage(XmlRpc.Text(row, "SubLanguageID"), XmlRpc.Text(row, "LanguageName")))
            .Where(l => LanguageCatalog.ValidId(l.Id) && !string.IsNullOrWhiteSpace(l.Name)).DistinctBy(l => l.Id).ToArray();
    }

    public async Task<IReadOnlyList<Subtitle>> SearchAsync(SearchRequest request, CancellationToken ct)
    {
        if (!LanguageCatalog.ValidId(request.LanguageId)) throw new SubClickException("InvalidLanguage");
        var login = await RpcAsync("LogIn", ct, "", "", "en", ClientIdentifier).ConfigureAwait(false);
        string token = XmlRpc.Text(login, "token");
        if (token.Length == 0) throw new SubClickException("ServiceError");
        try
        {
            var parameters = request.Hash is null ? VideoFiles.QueryParameters(request.Query) : new Dictionary<string, object>
            {
                ["moviehash"] = request.Hash,
                ["moviebytesize"] = request.FileSize.ToString(CultureInfo.InvariantCulture)
            };
            parameters["sublanguageid"] = request.LanguageId;
            var data = await RpcAsync("SearchSubtitles", ct, token, new object[] { parameters }).ConfigureAwait(false);
            return ParseSubtitles(data, request.LanguageId, request.Hash);
        }
        finally
        {
            // Always attempt to release the anonymous session, with its own short deadline.
            using var logoutTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            try { await RpcAsync("LogOut", logoutTimeout.Token, token).ConfigureAwait(false); }
            catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or SubClickException or IOException) { }
        }
    }

    public static IReadOnlyList<Subtitle> ParseSubtitles(object data, string languageId, string? hash)
    {
        var values = new List<Subtitle>();
        foreach (var row in XmlRpc.Items(XmlRpc.Get(data, "data")))
        {
            if (XmlRpc.Text(row, "SubLanguageID") != languageId ||
                !XmlRpc.Text(row, "SubFormat").Equals("srt", StringComparison.OrdinalIgnoreCase) ||
                XmlRpc.Text(row, "SubForeignPartsOnly") == "1") continue;
            string url = XmlRpc.Text(row, "ZipDownloadLink");
            if (url.Length == 0) continue;
            long.TryParse(XmlRpc.Text(row, "SubDownloadsCnt"), out long downloads);
            int.TryParse(XmlRpc.Text(row, "SubSumCD"), out int parts);
            values.Add(new(XmlRpc.Text(row, "IDSubtitleFile"), languageId, XmlRpc.Text(row, "MovieReleaseName"),
                XmlRpc.Text(row, "SubFileName"), XmlRpc.Text(row, "MovieName"), downloads,
                XmlRpc.Text(row, "MovieFPS"), hash is not null && hash.Equals(XmlRpc.Text(row, "MovieHash"), StringComparison.OrdinalIgnoreCase),
                XmlRpc.Text(row, "SubHearingImpaired") == "1", url, XmlRpc.Text(row, "SubEncoding"), parts));
        }
        return values.DistinctBy(s => s.Id).OrderByDescending(s => s.ExactMatch).ThenByDescending(s => s.Downloads).ToArray();
    }

    public static Uri ValidateDownloadUri(string raw)
    {
        if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri) || uri.UserInfo.Length != 0 ||
            !(uri.Host.Equals("opensubtitles.org", StringComparison.OrdinalIgnoreCase) || uri.Host.EndsWith(".opensubtitles.org", StringComparison.OrdinalIgnoreCase)) ||
            uri.Scheme is not ("http" or "https") || !uri.IsDefaultPort)
            throw new SubClickException("UnsafeDownload");
        return uri.Scheme == "http" ? new UriBuilder(uri) { Scheme = "https", Port = 443 }.Uri : uri;
    }

    public async Task<byte[]> DownloadAsync(Subtitle subtitle, CancellationToken ct)
    {
        var uri = ValidateDownloadUri(subtitle.ZipUrl);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
        deadline.CancelAfter(timeout);
        for (int redirects = 0; redirects < 5; redirects++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.TryAddWithoutValidation("User-Agent", ClientIdentifier);
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, deadline.Token).ConfigureAwait(false);
            if ((int)response.StatusCode is >= 300 and < 400 && response.Headers.Location is not null)
            {
                uri = ValidateDownloadUri(new Uri(uri, response.Headers.Location).AbsoluteUri);
                continue;
            }
            if (!response.IsSuccessStatusCode) throw new SubClickException("ServiceError");
            return SubtitleFiles.Extract(await ReadLimitedAsync(response.Content, deadline.Token).ConfigureAwait(false), subtitle);
        }
        throw new SubClickException("UnsafeDownload");
    }

    public void Dispose() => client.Dispose();
}
