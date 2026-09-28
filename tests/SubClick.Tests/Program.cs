using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using SubClick.Core;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
string root = Path.Combine(Path.GetTempPath(), "SubClick.Tests." + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
int failures = 0, passed = 0;
async Task Test(string name, Func<Task> run)
{
    try { await run(); Console.WriteLine("PASS " + name); passed++; }
    catch (Exception ex) { failures++; Console.WriteLine("FAIL " + name + ": " + ex); }
}
void Assert(bool condition) { if (!condition) throw new Exception("Assertion failed"); }
async Task Reject<T>(Func<Task> action) where T : Exception
{
    try { await action(); } catch (T) { return; } throw new Exception("Expected " + typeof(T).Name);
}
Subtitle Sample(string encoding = "UTF-8", string file = "sample.srt") => new("1", "jpn", "release", file, "title", 5, "24", false, false, "https://dl.opensubtitles.org/a.zip", encoding, 1);
byte[] Zip(byte[] bytes, string name = "sample.srt")
{
    using var output = new MemoryStream();
    using (var zip = new ZipArchive(output, ZipArchiveMode.Create, true))
    using (var stream = zip.CreateEntry(name).Open()) stream.Write(bytes);
    return output.ToArray();
}
string Cue(string text) => "1\n00:00:01,000 --> 00:00:03,000\n" + text + "\n";
string Response(object value) => new XElement("methodResponse", new XElement("params", new XElement("param", XmlRpc.Value(value)))).ToString();
Dictionary<string, object> Row(string lang, string id, string hash = "") => new()
{
    ["SubLanguageID"] = lang, ["SubFormat"] = "srt", ["IDSubtitleFile"] = id, ["MovieHash"] = hash,
    ["ZipDownloadLink"] = "https://dl.opensubtitles.org/a.zip", ["SubDownloadsCnt"] = "12"
};

try
{
    await Test("hash: little endian, overflow, file size and special path", async () =>
    {
        string path = Path.Combine(root, "Ação & [teste] ' $ nome.mkv");
        await File.WriteAllBytesAsync(path, new byte[131072]);
        Assert(await VideoFiles.HashAsync(path, default) == "0000000000020000");
        await File.WriteAllBytesAsync(path, Enumerable.Repeat((byte)255, 131072).ToArray());
        Assert(await VideoFiles.HashAsync(path, default) == "000000000001c000");
        var bytes = new byte[131080]; bytes[0] = 1; bytes[131072] = 2;
        await File.WriteAllBytesAsync(path, bytes);
        Assert(await VideoFiles.HashAsync(path, default) == "000000000002000b");
        await Reject<OperationCanceledException>(() => VideoFiles.HashAsync(path, new CancellationToken(true)));
    });
    await Test("small videos and title parsing", async () =>
    {
        string path = Path.Combine(root, "small.mp4"); await File.WriteAllBytesAsync(path, new byte[1]);
        Assert(await VideoFiles.HashAsync(path, default) is null);
        Assert(VideoFiles.SearchName("My.Series.S02E03.1080p.WEB-DL.mkv") == "My Series S02E03");
        var query = VideoFiles.QueryParameters("My Series S02E03");
        Assert((int)query["season"] == 2 && (int)query["episode"] == 3 && (string)query["query"] == "My Series");
        Assert((string)VideoFiles.QueryParameters("Sintel 2010")["query"] == "Sintel");
    });
    await Test("settings: persistence, updates and recovery", () =>
    {
        var store = new SettingsStore(Path.Combine(root, "settings")); Assert(store.Load().SubtitleLanguageId is null);
        store.Update(s => s with { SubtitleLanguageId = "jpn" });
        new SettingsStore(store.DirectoryPath).Update(s => s with { InterfaceLanguage = "pt-BR" });
        Assert(store.Load() == new AppSettings { SubtitleLanguageId = "jpn", InterfaceLanguage = "pt-BR" });
        File.WriteAllText(Path.Combine(store.DirectoryPath, "settings.json"), "broken");
        Assert(store.Load().SubtitleLanguageId is null && store.RecoveredInvalidFile);
        store.Update(s => s with { SubtitleLanguageId = "eng" });
        Assert(Directory.GetFiles(store.DirectoryPath, "*.invalid-*").Length == 1);
        return Task.CompletedTask;
    });
    await Test("catalog: bundled languages, fresh cache and corrupted cache", async () =>
    {
        var catalog = new LanguageCatalog(Path.Combine(root, "catalog"));
        Assert(catalog.Load().Count >= 90 && catalog.Load().Any(l => l.Id == "pob") && catalog.Load().Any(l => l.Id == "por"));
        var fake = new FakeProvider { Languages = [new("jpn", "Japanese"), new("../../bad", "Invalid")] };
        await catalog.RefreshAsync(fake, default); Assert(catalog.Load().Single().Id == "jpn");
        File.WriteAllText(Path.Combine(root, "catalog", "languages.json"), "oops"); Assert(catalog.Load().Count >= 90);
    });
    await Test("search: exact hash wins; fallback keeps chosen language", async () =>
    {
        string video = Path.Combine(root, "search.mkv"); await File.WriteAllBytesAsync(video, new byte[131072]);
        var fake = new FakeProvider { Search = r => r.Hash is not null ? [Sample() with { ExactMatch = true }] : [] };
        var search = new SearchService(fake);
        Assert((await search.SearchAsync(video, "test", "jpn", true, null, default)).ByHash);
        Assert(fake.Requests.Count == 1 && fake.Requests[0].LanguageId == "jpn");
        fake.Requests.Clear(); fake.Search = r => r.Hash is null ? [Sample()] : [];
        Assert(!(await search.SearchAsync(video, "test", "ara", true, null, default)).ByHash);
        Assert(fake.Requests.Count == 2 && fake.Requests.All(r => r.LanguageId == "ara"));
        fake.Requests.Clear();
        await search.SearchAsync(video, "test", "eng", false, null, default);
        Assert(fake.Requests.Single().Hash is null);
    });
    await Test("XML-RPC language filter, escaping and XXE rejection", async () =>
    {
        var rows = new Dictionary<string, object> { ["status"] = "200 OK", ["data"] = new object[] { Row("eng", "1"), Row("pob", "2"), Row("jpn", "3", "abc") } };
        var data = XmlRpc.ParseResponse(Response(rows));
        Assert(OpenSubtitlesProvider.ParseSubtitles(data, "jpn", "abc").Single().ExactMatch);
        Assert(OpenSubtitlesProvider.ParseSubtitles(data, "eng", null).Single().Id == "1");
        Assert(XDocument.Parse(XmlRpc.Request("Test", "A&B <é>")).Descendants("string").Single().Value == "A&B <é>");
        await Reject<SubClickException>(() => Task.Run(() => XmlRpc.ParseResponse("<!DOCTYPE x [<!ENTITY a SYSTEM 'file:///secret'>]><methodResponse>&a;</methodResponse>")));
    });
    foreach (var example in new[] { ("windows-1252", "Olá, ação!"), ("windows-1251", "Привет"), ("shift_jis", "こんにちは"), ("windows-1256", "مرحبا"), ("gb18030", "你好") })
    {
        await Test("encoding roundtrip: " + example.Item1, () =>
        {
            var bytes = Encoding.GetEncoding(example.Item1).GetBytes(Cue(example.Item2));
            var decoded = SubtitleFiles.Extract(Zip(bytes), Sample(example.Item1));
            Assert(Encoding.UTF8.GetString(decoded) == Cue(example.Item2)); return Task.CompletedTask;
        });
    }
    await Test("UTF-8 and UTF-16 BOM decoding", () =>
    {
        foreach (var encoding in new Encoding[] { new UTF8Encoding(true), Encoding.Unicode, Encoding.BigEndianUnicode, Encoding.UTF32 })
        {
            var bytes = encoding.GetPreamble().Concat(encoding.GetBytes(Cue("日本語 — العربية"))).ToArray();
            Assert(Encoding.UTF8.GetString(SubtitleFiles.Extract(Zip(bytes), Sample())) == Cue("日本語 — العربية"));
        }
        return Task.CompletedTask;
    });
    await Test("subtitle validation, path isolation and no overwrite", async () =>
    {
        byte[] text = Encoding.UTF8.GetBytes(Cue("Olá"));
        var bytes = SubtitleFiles.Extract(Zip(text, "../../sample.srt"), Sample());
        string target = Path.Combine(root, "saved.eng.srt"); SubtitleFiles.Save(target, bytes);
        await Reject<IOException>(() => Task.Run(() => SubtitleFiles.Save(target, Encoding.UTF8.GetBytes(Cue("different")))));
        Assert(File.ReadAllBytes(target).SequenceEqual(text));
        await Reject<SubClickException>(() => Task.Run(() => SubtitleFiles.Extract(Zip(Encoding.UTF8.GetBytes("<html>error</html>")), Sample())));
        await Reject<SubClickException>(() => Task.Run(() => SubtitleFiles.Extract(Zip(text), Sample() with { Parts = 2 })));
        await Reject<SubClickException>(() => Task.Run(() => SubtitleFiles.Save(target, new byte[SubtitleFiles.MaxBytes + 1])));
        Assert(!Directory.GetFiles(root, ".subclick-*.tmp").Any());
    });
    await Test("download URLs: HTTPS only and trusted host on each redirect", async () =>
    {
        Assert(OpenSubtitlesProvider.ValidateDownloadUri("http://dl.opensubtitles.org/sub.zip").Scheme == "https");
        foreach (string url in new[] { "https://opensubtitles.org.evil.test/a", "file:///c:/secret", "https://user@dl.opensubtitles.org/a", "https://dl.opensubtitles.org:444/a" })
            await Reject<SubClickException>(() => Task.Run(() => OpenSubtitlesProvider.ValidateDownloadUri(url)));
        using var provider = new OpenSubtitlesProvider(new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.Redirect)
        { Headers = { Location = new Uri("https://evil.test/a") } }));
        await Reject<SubClickException>(() => provider.DownloadAsync(Sample(), default));
    });
    await Test("provider: anonymous login, selected language and logout after error", async () =>
    {
        var methods = new List<string>();
        var handler = new FakeHandler(request =>
        {
            var doc = XDocument.Parse(request.Content!.ReadAsStringAsync().GetAwaiter().GetResult());
            string method = doc.Root!.Element("methodName")!.Value; methods.Add(method);
            if (method == "SearchSubtitles")
            {
                Assert(doc.Descendants("member").Single(m => m.Element("name")!.Value == "sublanguageid").Element("value")!.Value == "ara");
                return new(HttpStatusCode.ServiceUnavailable);
            }
            return new(HttpStatusCode.OK) { Content = new StringContent(Response(new Dictionary<string, object> { ["status"] = "200 OK", ["token"] = "test-only" })) };
        });
        using var provider = new OpenSubtitlesProvider(handler);
        await Reject<SubClickException>(() => provider.SearchAsync(new("test", "ara", null, 1), default));
        Assert(methods.SequenceEqual(new[] { "LogIn", "SearchSubtitles", "LogOut" }));
    });
    await Test("provider cancellation and streamed response limit", async () =>
    {
        using var provider = new OpenSubtitlesProvider(new FakeHandler(_ => new(HttpStatusCode.OK) { Content = new ByteArrayContent(new byte[SubtitleFiles.MaxBytes + 1]) }));
        await Reject<SubClickException>(() => provider.DownloadAsync(Sample(), default));
        await Reject<OperationCanceledException>(() => provider.GetLanguagesAsync(new CancellationToken(true)));
    });
    await Test("provider catalog accepts the service's missing status field", async () =>
    {
        using var provider = new OpenSubtitlesProvider(new FakeHandler(_ => new(HttpStatusCode.OK)
        {
            Content = new StringContent(Response(new Dictionary<string, object>
            {
                ["data"] = new object[] { new Dictionary<string, object> { ["SubLanguageID"] = "ara", ["LanguageName"] = "Arabic" } }
            }))
        }));
        Assert((await provider.GetLanguagesAsync(default)).Single().Id == "ara");
    });
    await Test("unwritable destination never consumes a download", async () =>
    {
        string directoryAsFile = Path.Combine(root, "not-a-directory"); File.WriteAllText(directoryAsFile, "fixture");
        await Reject<IOException>(() => Task.Run(() => SubtitleFiles.ProbeDirectory(Path.Combine(directoryAsFile, "a.srt"))));
    });
    int recordedIndex = Array.IndexOf(args, "--recorded");
    if (recordedIndex >= 0 && recordedIndex + 1 < args.Length)
    {
        await Test("RECORDED: actual catalog, response and ZIP through production parser", () =>
        {
            string fixtures = args[recordedIndex + 1];
            var data = XmlRpc.ParseResponse(File.ReadAllText(Path.Combine(fixtures, "live-SearchSubtitles.xml")));
            var rows = OpenSubtitlesProvider.ParseSubtitles(data, "pob", null); Assert(rows.Count > 0);
            var bytes = SubtitleFiles.Extract(File.ReadAllBytes(Path.Combine(fixtures, "live-subtitle.zip")), rows[0]);
            SubtitleFiles.Validate(Encoding.UTF8.GetString(bytes));
            Console.WriteLine($"RECORDED results={rows.Count}; UTF8 bytes={bytes.Length}");
            return Task.CompletedTask;
        });
    }
    if (args.Contains("--live"))
    {
        await Test("LIVE: anonymous catalog, search and SRT download", async () =>
        {
            using var provider = new OpenSubtitlesProvider();
            var langs = await provider.GetLanguagesAsync(default); Assert(langs.Count > 50);
            var results = await provider.SearchAsync(new("Sintel", "pob", null, 0), default);
            Assert(results.Count > 0);
            byte[] bytes = await provider.DownloadAsync(results[0], default);
            SubtitleFiles.Validate(Encoding.UTF8.GetString(bytes));
            Console.WriteLine($"LIVE catalog={langs.Count}; results={results.Count}; bytes={bytes.Length}");
        });
    }
}
finally { Directory.Delete(root, true); }
Console.WriteLine($"{passed} passed; {failures} failed");
return failures == 0 ? 0 : 1;

sealed class FakeProvider : ISubtitleProvider
{
    public IReadOnlyList<SubtitleLanguage> Languages { get; set; } = [];
    public Func<SearchRequest, IReadOnlyList<Subtitle>> Search { get; set; } = _ => [];
    public List<SearchRequest> Requests { get; } = [];
    public Task<IReadOnlyList<SubtitleLanguage>> GetLanguagesAsync(CancellationToken ct) => Task.FromResult(Languages);
    public Task<IReadOnlyList<Subtitle>> SearchAsync(SearchRequest request, CancellationToken ct) { Requests.Add(request); return Task.FromResult(Search(request)); }
    public Task<byte[]> DownloadAsync(Subtitle subtitle, CancellationToken ct) => throw new NotImplementedException();
}

sealed class FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    { ct.ThrowIfCancellationRequested(); return Task.FromResult(respond(request)); }
}
