using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace SubClick.Core;

public static class SubtitleFiles
{
    public const int MaxBytes = 8 * 1024 * 1024;
    static SubtitleFiles() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    public static string Destination(string video, string languageId)
    {
        if (!LanguageCatalog.ValidId(languageId)) throw new SubClickException("InvalidLanguage");
        // Keep the provider's distinct language IDs, including pob versus por.
        return Path.Combine(Path.GetDirectoryName(video)!, Path.GetFileNameWithoutExtension(video) + "." + languageId + ".srt");
    }

    public static byte[] Extract(byte[] archive, Subtitle subtitle)
    {
        if (archive.Length > MaxBytes) throw new SubClickException("TooLarge");
        using var memory = new MemoryStream(archive);
        using var zip = new ZipArchive(memory, ZipArchiveMode.Read);
        if (zip.Entries.Count > 256 || subtitle.Parts > 1) throw new SubClickException("MultipartSubtitle");
        var entries = zip.Entries.Where(e => e.Name.EndsWith(".srt", StringComparison.OrdinalIgnoreCase)).ToArray();
        var matches = entries.Where(e => e.Name.Equals(Path.GetFileName(subtitle.FileName), StringComparison.OrdinalIgnoreCase)).ToArray();
        var entry = matches.Length == 1 ? matches[0] : entries.Length == 1 ? entries[0] : null;
        if (entry is null) throw new SubClickException("MultipartSubtitle");
        if (entry.Length is <= 0 or > MaxBytes) throw new SubClickException("TooLarge");
        using var source = entry.Open();
        using var output = new MemoryStream();
        var buffer = new byte[16384];
        int count;
        while ((count = source.Read(buffer)) > 0)
        {
            if (output.Length + count > MaxBytes) throw new SubClickException("TooLarge");
            output.Write(buffer, 0, count);
        }
        string text = Decode(output.ToArray(), subtitle.Encoding).TrimStart('\uFEFF');
        Validate(text);
        return Encoding.UTF8.GetBytes(text);
    }

    public static string Decode(byte[] bytes, string charset)
    {
        if (bytes.Length >= 4 && bytes[0] == 255 && bytes[1] == 254 && bytes[2] == 0 && bytes[3] == 0)
            return new UTF32Encoding(false, true, true).GetString(bytes);
        if (bytes.Length >= 4 && bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 254 && bytes[3] == 255)
            return new UTF32Encoding(true, true, true).GetString(bytes);
        if (bytes.Length >= 2 && bytes[0] == 255 && bytes[1] == 254) return new UnicodeEncoding(false, true, true).GetString(bytes);
        if (bytes.Length >= 2 && bytes[0] == 254 && bytes[1] == 255) return new UnicodeEncoding(true, true, true).GetString(bytes);
        try { return new UTF8Encoding(false, true).GetString(bytes); }
        catch (DecoderFallbackException)
        {
            // Never guess a Western code page for an unknown non-Latin subtitle.
            if (string.IsNullOrWhiteSpace(charset)) throw new SubClickException("UnknownEncoding");
            try { return Encoding.GetEncoding(charset, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback).GetString(bytes); }
            catch (Exception ex) when (ex is ArgumentException or DecoderFallbackException or NotSupportedException)
            { throw new SubClickException("UnknownEncoding", ex); }
        }
    }

    public static void Validate(string text)
    {
        if (Regex.IsMatch(text, @"(?is)become.{0,50}vip.{0,50}member|opensubtitles.{0,80}(?:download limit|quota exceeded|vip membership required)"))
            throw new SubClickException("DownloadLimit");
        if (text.Contains('\0') || Regex.IsMatch(text, @"(?i)<\s*(?:!doctype|html)\b") ||
            !Regex.IsMatch(text, @"\d{1,3}:\d{2}:\d{2}[,.]\d{3}\s*-->\s*\d{1,3}:\d{2}:\d{2}[,.]\d{3}"))
            throw new SubClickException("InvalidSubtitle");
    }

    public static void ProbeDirectory(string destination)
    {
        string probe = Path.Combine(Path.GetDirectoryName(destination)!, ".subclick-" + Guid.NewGuid().ToString("N") + ".tmp");
        using var stream = new FileStream(probe, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1, FileOptions.DeleteOnClose);
    }

    public static void Save(string destination, byte[] utf8)
    {
        if (utf8.Length is <= 0 or > MaxBytes) throw new SubClickException("TooLarge");
        string text = new UTF8Encoding(false, true).GetString(utf8).TrimStart('\uFEFF');
        Validate(text);
        string temporary = Path.Combine(Path.GetDirectoryName(destination)!, ".subclick-" + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false))) writer.Write(text);
            File.Move(temporary, destination); // No overwrite, including a competing download.
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}
