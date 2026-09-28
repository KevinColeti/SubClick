using SubClick.Core;

namespace SubClick.App;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        Strings.SetCulture("auto");
        try
        {
            using var running = new Mutex(false, "SubClick.Running");
            if (args.Length > 1) throw new SubClickException("SingleVideo");
            var settings = new SettingsStore(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SubClick"));
            var current = settings.Load();
            Strings.SetCulture(current.InterfaceLanguage);
            string? video = args.Length == 1 ? Path.GetFullPath(args[0]) : null;
            if (video is null)
            {
                using var picker = new OpenFileDialog
                {
                    Title = Strings.Get("SelectVideo"), CheckFileExists = true, Multiselect = false,
                    Filter = Strings.Get("Videos") + "|" + string.Join(';', VideoFiles.Extensions.Select(e => "*" + e))
                };
                if (picker.ShowDialog() != DialogResult.OK) return 0;
                video = picker.FileName;
            }
            if (!File.Exists(video) || !VideoFiles.IsVideo(video)) throw new SubClickException("InvalidVideo");
            var catalog = new LanguageCatalog(settings.DirectoryPath);
            if (current.SubtitleLanguageId is null)
            {
                using var setup = new FirstRunForm(catalog.Load(), settings.RecoveredInvalidFile);
                if (setup.ShowDialog() != DialogResult.OK) return 0;
                current = settings.Update(s => s with { SubtitleLanguageId = setup.SelectedLanguage });
            }
            using var provider = new OpenSubtitlesProvider();
            using var form = new SearchForm(video, settings, catalog, provider);
            Application.Run(form);
            return 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show(Strings.Error(ex), "SubClick", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }
    }
}
