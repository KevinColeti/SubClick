using System.Text.Json;
using System.Text.RegularExpressions;

namespace SubClick.Core;

public sealed class LanguageCatalog(string directory)
{
    private string CachePath => Path.Combine(directory, "languages.json");
    public static bool ValidId(string id) => Regex.IsMatch(id, "^[a-z]{2,3}$", RegexOptions.CultureInvariant);

    public IReadOnlyList<SubtitleLanguage> Load()
    {
        try
        {
            if (File.Exists(CachePath))
            {
                var cached = Validate(JsonSerializer.Deserialize<SubtitleLanguage[]>(File.ReadAllText(CachePath)) ?? []);
                if (cached.Count > 0) return cached;
            }
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException) { }
        using var stream = typeof(LanguageCatalog).Assembly.GetManifestResourceStream("SubClick.Core.Resources.languages.json")!;
        return Validate(JsonSerializer.Deserialize<SubtitleLanguage[]>(stream)!);
    }

    public async Task<IReadOnlyList<SubtitleLanguage>> RefreshAsync(ISubtitleProvider provider, CancellationToken ct)
    {
        var values = Validate(await provider.GetLanguagesAsync(ct).ConfigureAwait(false));
        if (values.Count == 0) throw new SubClickException("InvalidResponse");
        Directory.CreateDirectory(directory);
        SettingsStore.AtomicWrite(CachePath, JsonSerializer.Serialize(values));
        return values;
    }

    private static IReadOnlyList<SubtitleLanguage> Validate(IEnumerable<SubtitleLanguage> values) => values
        .Where(v => v is not null && v.Id is not null && ValidId(v.Id) && !string.IsNullOrWhiteSpace(v.Name))
        .DistinctBy(v => v.Id).OrderBy(v => v.Name, StringComparer.OrdinalIgnoreCase).ToArray();
}
