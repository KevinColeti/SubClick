using SubClick.Core;

namespace SubClick.App;

internal sealed class FirstRunForm : Form
{
    private readonly ComboBox languages = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
    public string SelectedLanguage => ((SubtitleLanguage)languages.SelectedItem!).Id;

    public FirstRunForm(IReadOnlyList<SubtitleLanguage> items, bool recovered)
    {
        Text = "SubClick";
        Font = new Font("Segoe UI", 10);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(510, 230);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false; StartPosition = FormStartPosition.CenterScreen;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 4, ColumnCount = 1 };
        layout.RowStyles.Add(new(SizeType.Absolute, 55));
        layout.RowStyles.Add(new(SizeType.Absolute, 38));
        layout.RowStyles.Add(new(SizeType.Percent, 100));
        layout.RowStyles.Add(new(SizeType.Absolute, 38));
        layout.Controls.Add(new Label { Text = Strings.Get(recovered ? "SettingsRecovered" : "FirstRun"), Dock = DockStyle.Fill, AutoSize = false });
        languages.BindingContext = new BindingContext();
        languages.DataSource = items.ToList();
        languages.SelectedItem = items.FirstOrDefault(l => l.Id == "pob") ?? items[0];
        languages.AccessibleName = Strings.Get("SubtitleLanguage");
        layout.Controls.Add(languages);
        layout.Controls.Add(new Label { Text = Strings.Get("RememberExplanation"), Dock = DockStyle.Fill });
        var start = new Button { Text = Strings.Get("SaveAndSearch"), DialogResult = DialogResult.OK, AutoSize = true, Anchor = AnchorStyles.Right };
        layout.Controls.Add(start); AcceptButton = start;
        Controls.Add(layout);
    }
}
