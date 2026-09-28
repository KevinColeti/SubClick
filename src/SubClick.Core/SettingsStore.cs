using System.Text;
using System.Text.Json;

namespace SubClick.Core;

public sealed record AppSettings
{
    public int SchemaVersion { get; init; } = 1;
    public string? SubtitleLanguageId { get; init; }
    public string InterfaceLanguage { get; init; } = "auto";
}

public sealed class SettingsStore(string directory)
{
    public string DirectoryPath { get; } = Path.GetFullPath(directory);
    public bool RecoveredInvalidFile { get; private set; }
    public AppSettings Load()
    {
        RecoveredInvalidFile = false;
        string path = Path.Combine(DirectoryPath, "settings.json");
        if (!File.Exists(path)) return new();
        try
        {
            var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path));
            if (settings is null || settings.SchemaVersion != 1 ||
                (settings.SubtitleLanguageId is not null && !LanguageCatalog.ValidId(settings.SubtitleLanguageId)) ||
                settings.InterfaceLanguage is not ("auto" or "pt-BR" or "en")) throw new JsonException();
            return settings;
        }
        catch (JsonException) { RecoveredInvalidFile = true; return new(); }
    }

    public AppSettings Update(Func<AppSettings, AppSettings> update)
    {
        string id = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(DirectoryPath.ToUpperInvariant())));
        using var mutex = new Mutex(false, "SubClick.Settings." + id);
        bool taken;
        try { taken = mutex.WaitOne(TimeSpan.FromSeconds(5)); }
        catch (AbandonedMutexException) { taken = true; }
        if (!taken) throw new IOException("Settings busy");
        try
        {
            var current = Load();
            Directory.CreateDirectory(DirectoryPath);
            string path = Path.Combine(DirectoryPath, "settings.json");
            if (RecoveredInvalidFile) File.Copy(path, path + ".invalid-" + DateTime.UtcNow.Ticks, false);
            var next = update(current);
            AtomicWrite(path, JsonSerializer.Serialize(next, new JsonSerializerOptions { WriteIndented = true }));
            return next;
        }
        finally { mutex.ReleaseMutex(); }
    }

    internal static void AtomicWrite(string path, string text)
    {
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporary, text, new UTF8Encoding(false));
            File.Move(temporary, path, true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}
