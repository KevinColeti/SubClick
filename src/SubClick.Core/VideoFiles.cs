using System.Buffers.Binary;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SubClick.Core;

public static class VideoFiles
{
    public static IReadOnlyList<string> Extensions { get; } = new[]
    {
        ".mkv", ".mp4", ".avi", ".mov", ".wmv", ".m4v", ".webm", ".mpg", ".mpeg",
        ".ts", ".m2ts", ".mts", ".vob", ".ogv", ".flv", ".divx", ".3gp", ".3g2", ".f4v", ".asf", ".m2v"
    };

    public static bool IsVideo(string path) => Extensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);

    // OpenSubtitles/VLSub: file length + first and last 64 KiB as little-endian uint64.
    public static async Task<string?> HashAsync(string path, CancellationToken ct)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            65536, FileOptions.Asynchronous | FileOptions.RandomAccess);
        if (stream.Length < 131072) return null;
        ulong sum = (ulong)stream.Length;
        var block = new byte[65536];
        foreach (long offset in new long[] { 0, stream.Length - block.Length })
        {
            ct.ThrowIfCancellationRequested();
            stream.Position = offset;
            await stream.ReadExactlyAsync(block, ct).ConfigureAwait(false);
            for (int i = 0; i < block.Length; i += 8)
                sum = unchecked(sum + BinaryPrimitives.ReadUInt64LittleEndian(block.AsSpan(i, 8)));
        }
        return sum.ToString("x16", CultureInfo.InvariantCulture);
    }

    public static string SearchName(string path)
    {
        string name = Regex.Replace(Path.GetFileNameWithoutExtension(path), @"[._]+", " ");
        name = Regex.Replace(name, @"\s+(?:2160p|1080[pi]|720p|480p|bluray|blu-ray|brrip|bdrip|webrip|web-dl|hdtv|dvdrip|x264|x265|h264|h265|hevc)\b.*$", "", RegexOptions.IgnoreCase);
        return Regex.Replace(name, @"\s+", " ").Trim(' ', '-', '[', ']');
    }

    public static Dictionary<string, object> QueryParameters(string query)
    {
        var values = new Dictionary<string, object>();
        var episode = Regex.Match(query, @"(?i)\bS(\d{1,2})E(\d{1,3})\b|\b(\d{1,2})x(\d{1,3})\b");
        if (episode.Success)
        {
            values["season"] = int.Parse(episode.Groups[1].Success ? episode.Groups[1].Value : episode.Groups[3].Value, CultureInfo.InvariantCulture);
            values["episode"] = int.Parse(episode.Groups[2].Success ? episode.Groups[2].Value : episode.Groups[4].Value, CultureInfo.InvariantCulture);
            values["query"] = query[..episode.Index].Trim(' ', '-', '(');
        }
        else
        {
            var year = Regex.Match(query, @"\b(?:19|20)\d{2}\b");
            values["query"] = year.Success && year.Index > 0 ? query[..year.Index].Trim(' ', '-', '(', '[') : query;
        }
        if (string.IsNullOrWhiteSpace((string)values["query"])) values["query"] = query;
        return values;
    }
}
