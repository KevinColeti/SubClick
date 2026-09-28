using System.Text;
using SubClick.App;
using SubClick.Core;

namespace SubClick.UiTests;

internal static class Program
{
    private static int failures;
    [STAThread]
    private static int Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        string root = Path.GetFullPath(args.Length > 0 ? args[0] : Path.Combine(Path.GetTempPath(), "SubClick.UiTests." + Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(root);
        try
        {
            Strings.SetCulture("pt-BR");
            var settings = new SettingsStore(Path.Combine(root, "profile-" + Guid.NewGuid().ToString("N")));
            var catalog = new LanguageCatalog(settings.DirectoryPath);
            using (var first = new FirstRunForm(catalog.Load(), false))
            {
                first.Show(); Application.DoEvents();
                Check(first.SelectedLanguage == "pob", "first run suggests Brazilian Portuguese");
                Capture(first, Path.Combine(root, "first-run.png"));
                first.Close();
            }
            settings.Update(s => s with { SubtitleLanguageId = "jpn", InterfaceLanguage = "pt-BR" });
            string video = Path.Combine(settings.DirectoryPath, "Série & ação [teste].S01E01.mkv"); File.WriteAllBytes(video, new byte[32]);
            var provider = new DemoProvider();
            using var form = new SearchForm(video, settings, catalog, provider);
            var timer = new System.Windows.Forms.Timer { Interval = 300 };
            int step = 0, ticks = 0;
            timer.Tick += (_, _) =>
            {
                try
                {
                    if (++ticks > 50) throw new Exception("UI test timeout");
                    var buttons = Descendants(form).OfType<Button>().ToArray();
                    var combos = Descendants(form).OfType<ComboBox>().ToArray();
                    var languages = combos.Single(c => c.DataSource is not null);
                    var interfaceLanguage = combos.Single(c => c.DataSource is null);
                    var list = Descendants(form).OfType<ListView>().Single();
                    if (step == 0 && list.Items.Count > 0)
                    {
                        Check(provider.LastLanguage == "jpn", "saved default starts automatic search");
                        Check(Descendants(form).Single(c => c.Name == "Status").Height >= 35, "status remains readable when progress is hidden");
                        Capture(form, Path.Combine(root, "search-pt-BR.png"));
                        languages.SelectedItem = ((IEnumerable<SubtitleLanguage>)languages.DataSource!).Single(l => l.Id == "eng");
                        Check(list.Items.Count == 0 && settings.Load().SubtitleLanguageId == "jpn", "temporary language change clears stale results and preserves default");
                        buttons.Single(b => b.Text == Strings.Get("UseDefault")).PerformClick();
                        Check(settings.Load().SubtitleLanguageId == "eng", "explicit default action persists");
                        interfaceLanguage.SelectedIndex = 2;
                        Check(settings.Load().InterfaceLanguage == "en" && form.Text == "SubClick — Find subtitles", "interface translation persists immediately");
                        buttons.Single(b => b.Text == Strings.Get("FindName")).PerformClick();
                        step = 1;
                    }
                    else if (step == 1 && list.Items.Count > 0)
                    {
                        Check(provider.LastLanguage == "eng", "next search uses selected language");
                        Capture(form, Path.Combine(root, "search-en.png"));
                        list.Items[0].Selected = true;
                        buttons.Single(b => b.Text == Strings.Get("Download")).PerformClick();
                        step = 2;
                    }
                    else if (step == 2 && File.Exists(SubtitleFiles.Destination(video, "eng")))
                    {
                        Check(File.ReadAllText(SubtitleFiles.Destination(video, "eng")).Contains("日本語"), "download saves Unicode next to selected video");
                        Capture(form, Path.Combine(root, "download-en.png"));
                        timer.Stop(); form.Close();
                    }
                }
                catch (Exception ex) { failures++; Console.WriteLine("FAIL " + ex); timer.Stop(); form.Close(); }
            };
            form.Shown += (_, _) => timer.Start();
            Application.Run(form); timer.Dispose();
            Console.WriteLine($"UI failures: {failures}");
            return failures == 0 ? 0 : 1;
        }
        catch (Exception ex) { Console.WriteLine(ex); return 1; }
    }

    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception(name);
        Console.WriteLine("PASS " + name);
    }
    private static IEnumerable<Control> Descendants(Control parent) => parent.Controls.Cast<Control>().SelectMany(c => new[] { c }.Concat(Descendants(c)));
    private static void Capture(Form form, string path)
    {
        using var bitmap = new Bitmap(form.Width, form.Height);
        form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size)); bitmap.Save(path);
    }
}

internal sealed class DemoProvider : ISubtitleProvider
{
    public string LastLanguage { get; private set; } = "";
    public Task<IReadOnlyList<SubtitleLanguage>> GetLanguagesAsync(CancellationToken ct) => Task.FromResult<IReadOnlyList<SubtitleLanguage>>([new("pob", "Português brasileiro"), new("eng", "English"), new("jpn", "Japanese")]);
    public async Task<IReadOnlyList<Subtitle>> SearchAsync(SearchRequest request, CancellationToken ct)
    {
        await Task.Delay(50, ct); LastLanguage = request.LanguageId;
        return [new("fixture", request.LanguageId, "Exemplo.S01E01.1080p.WEB-DL", "example.srt", "Synthetic UI fixture", 120, "23.976", false, false, "https://dl.opensubtitles.org/fixture", "UTF-8", 1)];
    }
    public Task<byte[]> DownloadAsync(Subtitle subtitle, CancellationToken ct) => Task.FromResult(Encoding.UTF8.GetBytes("1\n00:00:01,000 --> 00:00:03,000\nOlá — 日本語\n"));
}
