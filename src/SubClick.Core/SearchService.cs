namespace SubClick.Core;

public sealed class SearchService(ISubtitleProvider provider)
{
    public async Task<SearchResult> SearchAsync(string video, string query, string languageId, bool byHash,
        IProgress<SearchStage>? progress, CancellationToken ct)
    {
        if (!File.Exists(video) || !VideoFiles.IsVideo(video)) throw new SubClickException("InvalidVideo");
        if (string.IsNullOrWhiteSpace(query)) throw new SubClickException("EmptyQuery");
        string? hash = null;
        if (byHash)
        {
            progress?.Report(SearchStage.Hashing);
            hash = await VideoFiles.HashAsync(video, ct).ConfigureAwait(false);
        }
        long size = new FileInfo(video).Length;
        if (hash is not null)
        {
            progress?.Report(SearchStage.SearchingHash);
            var found = await provider.SearchAsync(new(query, languageId, hash, size), ct).ConfigureAwait(false);
            var exact = found.Where(s => s.ExactMatch).ToArray();
            if (exact.Length > 0) return new(exact, true);
        }
        progress?.Report(SearchStage.SearchingName);
        var named = await provider.SearchAsync(new(query, languageId, null, size), ct).ConfigureAwait(false);
        return new(named, false);
    }
}
