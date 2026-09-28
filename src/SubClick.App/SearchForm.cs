using System.Globalization;
using SubClick.Core;

namespace SubClick.App;

internal sealed class SearchForm : Form
{
    private readonly string video;
    private readonly SettingsStore settings;
    private readonly LanguageCatalog catalog;
    private readonly ISubtitleProvider provider;
    private readonly CancellationTokenSource lifetime = new();
    private readonly TextBox query = new() { Dock = DockStyle.Fill };
    private readonly ComboBox language = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
    private readonly ComboBox uiLanguage = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
    private readonly ListView results = new() { View = View.Details, FullRowSelect = true, MultiSelect = false, HideSelection = false, ShowItemToolTips = true, Dock = DockStyle.Fill };
    private readonly Label status = new() { Name = "Status", Dock = DockStyle.Fill, AutoEllipsis = true, UseMnemonic = false };
    private readonly Label preference = new() { AutoSize = true, Anchor = AnchorStyles.Left };
    private readonly Label languageLabel = new() { AutoSize = true, Anchor = AnchorStyles.Left };
    private readonly Label uiLabel = new() { AutoSize = true, Anchor = AnchorStyles.Left };
    private readonly Button findName = new() { AutoSize = true }, findHash = new() { AutoSize = true }, makeDefault = new() { AutoSize = true }, download = new() { AutoSize = true };
    private readonly ProgressBar progress = new() { Style = ProgressBarStyle.Marquee, Dock = DockStyle.Fill, Visible = false };
    private bool busy, translating;
    private bool Alive => !IsDisposed && !Disposing && !lifetime.IsCancellationRequested;

    public SearchForm(string video, SettingsStore settings, LanguageCatalog catalog, ISubtitleProvider provider)
    {
        this.video = video; this.settings = settings; this.catalog = catalog; this.provider = provider;
        Font = new Font("Segoe UI", 10); AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(980, 570); MinimumSize = new Size(850, 480);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), RowCount = 7, ColumnCount = 1 };
        foreach (float height in new float[] { 32, 43, 43 }) layout.RowStyles.Add(new(SizeType.Absolute, height));
        layout.RowStyles.Add(new(SizeType.Percent, 100));
        layout.RowStyles.Add(new(SizeType.Absolute, 10));
        layout.RowStyles.Add(new(SizeType.Absolute, 52));
        layout.RowStyles.Add(new(SizeType.Absolute, 38));
        layout.Controls.Add(new Label { Text = Path.GetFileName(video), UseMnemonic = false, AutoEllipsis = true, Dock = DockStyle.Fill, Font = new Font(Font, FontStyle.Bold) }, 0, 0);
        var queryRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3 };
        queryRow.ColumnStyles.Add(new(SizeType.Percent, 100)); queryRow.ColumnStyles.Add(new(SizeType.AutoSize)); queryRow.ColumnStyles.Add(new(SizeType.AutoSize));
        query.Text = VideoFiles.SearchName(video); queryRow.Controls.Add(query); queryRow.Controls.Add(findName); queryRow.Controls.Add(findHash);
        layout.Controls.Add(queryRow, 0, 1);
        var languageRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4 };
        languageRow.ColumnStyles.Add(new(SizeType.AutoSize)); languageRow.ColumnStyles.Add(new(SizeType.Percent, 100));
        languageRow.ColumnStyles.Add(new(SizeType.AutoSize)); languageRow.ColumnStyles.Add(new(SizeType.AutoSize));
        languageRow.Controls.Add(languageLabel); languageRow.Controls.Add(language); languageRow.Controls.Add(makeDefault); languageRow.Controls.Add(preference);
        layout.Controls.Add(languageRow, 0, 2);
        results.Columns.Add("", 470); results.Columns.Add("", 95); results.Columns.Add("", 95); results.Columns.Add("", 80); results.Columns.Add("", 70);
        layout.Controls.Add(results, 0, 3); layout.Controls.Add(progress, 0, 4); layout.Controls.Add(status, 0, 5);
        var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4 };
        footer.ColumnStyles.Add(new(SizeType.AutoSize)); footer.ColumnStyles.Add(new(SizeType.AutoSize)); footer.ColumnStyles.Add(new(SizeType.Percent, 100)); footer.ColumnStyles.Add(new(SizeType.AutoSize));
        footer.Controls.Add(uiLabel); footer.Controls.Add(uiLanguage); footer.Controls.Add(new Label()); footer.Controls.Add(download);
        layout.Controls.Add(footer, 0, 6); Controls.Add(layout);

        ApplyLanguages(catalog.Load(), settings.Load().SubtitleLanguageId!);
        Translate(); SetBusy(false);
        Shown += async (_, _) => await Task.WhenAll(FindAsync(true), RefreshLanguagesAsync());
        findName.Click += async (_, _) => await FindAsync(false);
        findHash.Click += async (_, _) => await FindAsync(true);
        query.KeyDown += async (_, e) => { if (e.KeyCode == Keys.Enter && !busy) { e.SuppressKeyPress = true; await FindAsync(false); } };
        language.SelectedIndexChanged += (_, _) => { if (!translating) { results.Items.Clear(); download.Enabled = false; status.Text = Strings.Get("LanguageChanged"); } };
        makeDefault.Click += (_, _) => SaveDefault();
        download.Click += async (_, _) => await SaveAsync();
        results.DoubleClick += async (_, _) => await SaveAsync();
        results.SelectedIndexChanged += (_, _) => download.Enabled = !busy && results.SelectedItems.Count == 1;
        uiLanguage.SelectedIndexChanged += (_, _) => ChangeUiLanguage();
        FormClosing += (_, _) => lifetime.Cancel();
    }

    private void ApplyLanguages(IReadOnlyList<SubtitleLanguage> items, string selected)
    {
        translating = true;
        var all = items.ToList();
        if (!all.Any(l => l.Id == selected)) all.Add(new(selected, selected));
        language.DataSource = all;
        language.SelectedItem = all.First(l => l.Id == selected);
        translating = false;
    }

    private async Task RefreshLanguagesAsync()
    {
        try
        {
            var items = await catalog.RefreshAsync(provider, lifetime.Token);
            if (Alive) ApplyLanguages(items, ((SubtitleLanguage)language.SelectedItem!).Id);
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or SubClickException or OperationCanceledException or UnauthorizedAccessException)
        { /* The bundled/cache catalog remains usable when refresh is unavailable. */ }
    }

    private void Translate()
    {
        translating = true;
        Text = Strings.Get("WindowTitle"); findName.Text = Strings.Get("FindName"); findHash.Text = Strings.Get("FindHash");
        makeDefault.Text = Strings.Get("UseDefault"); download.Text = Strings.Get("Download");
        languageLabel.Text = Strings.Get("SubtitleLanguage"); uiLabel.Text = Strings.Get("InterfaceLanguage");
        query.AccessibleName = Strings.Get("SearchTitle"); language.AccessibleName = languageLabel.Text; uiLanguage.AccessibleName = uiLabel.Text;
        string[] columns = ["Release", "ExactHash", "Downloads", "Fps", "Sdh"];
        for (int i = 0; i < columns.Length; i++) results.Columns[i].Text = Strings.Get(columns[i]);
        foreach (ListViewItem row in results.Items)
        {
            var item = (Subtitle)row.Tag!;
            row.SubItems[1].Text = item.ExactMatch ? Strings.Get("Yes") : "—";
            row.SubItems[4].Text = Strings.Get(item.HearingImpaired ? "Yes" : "No");
        }
        uiLanguage.Items.Clear(); uiLanguage.Items.AddRange([Strings.Get("Automatic"), "Português brasileiro", "English"]);
        var saved = settings.Load();
        uiLanguage.SelectedIndex = saved.InterfaceLanguage switch { "pt-BR" => 1, "en" => 2, _ => 0 };
        preference.Text = Strings.Format("DefaultLabel", saved.SubtitleLanguageId ?? "");
        translating = false;
    }

    private void ChangeUiLanguage()
    {
        if (translating || uiLanguage.SelectedIndex < 0) return;
        try
        {
            string value = uiLanguage.SelectedIndex switch { 1 => "pt-BR", 2 => "en", _ => "auto" };
            settings.Update(s => s with { InterfaceLanguage = value });
            Strings.SetCulture(value); Translate(); status.Text = Strings.Get("Ready");
        }
        catch (Exception ex) { status.Text = Strings.Error(ex); }
    }

    private void SaveDefault()
    {
        try
        {
            string id = ((SubtitleLanguage)language.SelectedItem!).Id;
            settings.Update(s => s with { SubtitleLanguageId = id });
            preference.Text = Strings.Format("DefaultLabel", id); status.Text = Strings.Get("PreferenceSaved");
        }
        catch (Exception ex) { status.Text = Strings.Error(ex); }
    }

    private void SetBusy(bool value)
    {
        busy = value; progress.Visible = value;
        query.Enabled = language.Enabled = findName.Enabled = findHash.Enabled = makeDefault.Enabled = uiLanguage.Enabled = results.Enabled = !value;
        download.Enabled = !value && results.SelectedItems.Count == 1;
    }

    private async Task FindAsync(bool byHash)
    {
        if (busy || !Alive) return;
        string id = ((SubtitleLanguage)language.SelectedItem!).Id;
        SetBusy(true); results.Items.Clear();
        try
        {
            var report = new Progress<SearchStage>(stage => { if (Alive) status.Text = Strings.Get(stage.ToString()); });
            var found = await new SearchService(provider).SearchAsync(video, query.Text.Trim(), id, byHash, report, lifetime.Token);
            if (!Alive) return;
            foreach (var item in found.Items)
            {
                var row = new ListViewItem([string.IsNullOrWhiteSpace(item.Release) ? item.FileName : item.Release,
                    item.ExactMatch ? Strings.Get("Yes") : "—", item.Downloads.ToString(CultureInfo.CurrentCulture), item.Fps,
                    Strings.Get(item.HearingImpaired ? "Yes" : "No")]) { Tag = item, ToolTipText = item.Title + "\n" + item.FileName };
                results.Items.Add(row);
            }
            if (results.Items.Count > 0) results.Items[0].Selected = true;
            status.Text = found.Items.Count == 0 ? Strings.Get("NoResults") : Strings.Format(found.ByHash ? "ResultsHash" : "ResultsName", found.Items.Count);
        }
        catch (Exception ex) { if (Alive) status.Text = Strings.Error(ex); }
        finally { if (Alive) SetBusy(false); }
    }

    private async Task SaveAsync()
    {
        if (busy || !Alive || results.SelectedItems.Count != 1) return;
        var subtitle = (Subtitle)results.SelectedItems[0].Tag!;
        string destination = SubtitleFiles.Destination(video, subtitle.LanguageId);
        if (File.Exists(destination))
        {
            using var picker = new SaveFileDialog
            {
                Title = Strings.Get("ChooseAlternative"), InitialDirectory = Path.GetDirectoryName(video),
                FileName = Path.GetFileNameWithoutExtension(destination) + ".alternative.srt", Filter = "SRT|*.srt", DefaultExt = "srt", AddExtension = true
            };
            if (picker.ShowDialog(this) != DialogResult.OK) return;
            destination = picker.FileName;
            if (File.Exists(destination)) { status.Text = Strings.Get("ExistingPreserved"); return; }
        }
        SetBusy(true);
        try
        {
            SubtitleFiles.ProbeDirectory(destination);
            status.Text = Strings.Get("Downloading");
            byte[] bytes = await provider.DownloadAsync(subtitle, lifetime.Token);
            lifetime.Token.ThrowIfCancellationRequested();
            await Task.Run(() => { lifetime.Token.ThrowIfCancellationRequested(); SubtitleFiles.Save(destination, bytes); }, lifetime.Token);
            if (Alive) status.Text = Strings.Format("Saved", destination);
        }
        catch (Exception ex) { if (Alive) status.Text = Strings.Error(ex); }
        finally { if (Alive) SetBusy(false); }
    }
}
