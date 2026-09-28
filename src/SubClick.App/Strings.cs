using System.Globalization;
using System.Resources;
using SubClick.Core;

namespace SubClick.App;

internal static class Strings
{
    private static readonly ResourceManager Resources = new("SubClick.App.Resources.Strings", typeof(Strings).Assembly);
    public static void SetCulture(string value)
    {
        string chosen = value == "auto" ? (CultureInfo.InstalledUICulture.TwoLetterISOLanguageName == "pt" ? "pt-BR" : "en") : value;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(chosen);
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentUICulture;
    }
    public static string Get(string key) => Resources.GetString(key) ?? key;
    public static string Format(string key, params object[] values) => string.Format(CultureInfo.CurrentCulture, Get(key), values);
    public static string Error(Exception ex) => ex switch
    {
        SubClickException app => Get(app.Code),
        OperationCanceledException => Get("TimedOut"),
        UnauthorizedAccessException => Get("AccessDenied"),
        HttpRequestException => Get("NetworkError"),
        IOException => Get("FileError"),
        _ => Get("UnexpectedError")
    };
}
