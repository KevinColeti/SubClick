namespace SubClick.Core;

public sealed record SubtitleLanguage(string Id, string Name)
{
    public override string ToString() => $"{Name} ({Id})";
}

public sealed record Subtitle(
    string Id, string LanguageId, string Release, string FileName, string Title,
    long Downloads, string Fps, bool ExactMatch, bool HearingImpaired,
    string ZipUrl, string Encoding, int Parts);

public sealed record SearchRequest(string Query, string LanguageId, string? Hash, long FileSize);
public sealed record SearchResult(IReadOnlyList<Subtitle> Items, bool ByHash);
public enum SearchStage { Hashing, SearchingHash, SearchingName }

public interface ISubtitleProvider
{
    Task<IReadOnlyList<SubtitleLanguage>> GetLanguagesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Subtitle>> SearchAsync(SearchRequest request, CancellationToken cancellationToken);
    Task<byte[]> DownloadAsync(Subtitle subtitle, CancellationToken cancellationToken);
}

// Stable error codes are translated only by the presentation layer.
public sealed class SubClickException(string code, Exception? inner = null) : Exception(code, inner)
{
    public string Code { get; } = code;
}
