using System.Globalization;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;

namespace JJManager.Desktop.Services;

/// <summary>
/// Service for handling internationalization (i18n) with JSON resource files
/// </summary>
public class LocalizationService : ILocalizationService
{
    private static LocalizationService? _instance;
    public static LocalizationService Instance => _instance ??= new LocalizationService();

    private Dictionary<string, string> _strings = new();
    private string _currentLanguage = "pt-BR";

    public event EventHandler? LanguageChanged;

    public IReadOnlyList<LanguageInfo> AvailableLanguages { get; } = new List<LanguageInfo>
    {
        new("pt-BR", "Português (Brasil)", "Português (Brasil)"),
        new("en-US", "English (US)", "English (US)"),
        new("es-ES", "Spanish", "Español")
    };

    public string CurrentLanguage => _currentLanguage;

    public LocalizationService()
    {
        _instance = this;
    }

    /// <summary>
    /// Initialize the localization service with the specified language
    /// </summary>
    public async Task InitializeAsync(string? languageCode = null)
    {
        // Use system language if not specified
        if (string.IsNullOrEmpty(languageCode))
        {
            languageCode = CultureInfo.CurrentCulture.Name;

            // Fallback to pt-BR if system language not supported
            if (!AvailableLanguages.Any(l => l.Code == languageCode))
            {
                // Try matching just the language part (e.g., "en" matches "en-US")
                var langPart = languageCode.Split('-')[0];
                var match = AvailableLanguages.FirstOrDefault(l => l.Code.StartsWith(langPart));
                languageCode = match?.Code ?? "pt-BR";
            }
        }

        await SetLanguageAsync(languageCode);
    }

    /// <summary>
    /// Change the current language
    /// </summary>
    public async Task SetLanguageAsync(string languageCode)
    {
        if (!AvailableLanguages.Any(l => l.Code == languageCode))
        {
            languageCode = "pt-BR";
        }

        _currentLanguage = languageCode;
        await LoadStringsAsync(languageCode);
        UpdateResourceDictionary();
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Get a localized string by key
    /// </summary>
    public string GetString(string key)
    {
        return _strings.TryGetValue(key, out var value) ? value : $"[{key}]";
    }

    /// <summary>
    /// Get a localized string with format parameters
    /// </summary>
    public string GetString(string key, params object[] args)
    {
        var template = GetString(key);
        try
        {
            return string.Format(template, args);
        }
        catch
        {
            return template;
        }
    }

    /// <summary>
    /// Indexer for easy access to strings
    /// </summary>
    public string this[string key] => GetString(key);

    private async Task LoadStringsAsync(string languageCode)
    {
        try
        {
            var assembly = typeof(LocalizationService).Assembly;
            var resourceName = $"JJManager.Desktop.Assets.i18n.{languageCode}.json";

            // Try to load from embedded resource
            using var stream = assembly.GetManifestResourceStream(resourceName);

            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                ParseJsonStrings(json);
                return;
            }

            // Fallback: try to load from file
            var basePath = AppContext.BaseDirectory;
            var filePath = Path.Combine(basePath, "Assets", "i18n", $"{languageCode}.json");

            if (File.Exists(filePath))
            {
                var json = await File.ReadAllTextAsync(filePath);
                ParseJsonStrings(json);
                return;
            }

            // Last resort: use default strings
            LoadDefaultStrings();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load language {languageCode}: {ex.Message}");
            LoadDefaultStrings();
        }
    }

    private void ParseJsonStrings(string json)
    {
        _strings.Clear();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        foreach (var property in root.EnumerateObject())
        {
            // Skip metadata properties
            if (property.Name.StartsWith("_"))
                continue;

            if (property.Value.ValueKind == JsonValueKind.String)
            {
                _strings[property.Name] = property.Value.GetString() ?? string.Empty;
            }
        }
    }

    private void LoadDefaultStrings()
    {
        _strings = new Dictionary<string, string>
        {
            ["App_Title"] = "JJManager",
            ["Devices_Title"] = "Dispositivos",
            ["Devices_Subtitle"] = "Gerencie seus dispositivos JohnJohn3D conectados",
            ["Profiles_Title"] = "Perfis",
            ["Updates_Title"] = "Atualizações",
            ["Settings_Title"] = "Configurações"
        };
    }

    private void UpdateResourceDictionary()
    {
        if (Application.Current?.Resources == null)
            return;

        var resources = Application.Current.Resources;

        foreach (var kvp in _strings)
        {
            resources[kvp.Key] = kvp.Value;
        }
    }
}

/// <summary>
/// Information about an available language
/// </summary>
public record LanguageInfo(string Code, string Name, string NativeName);

/// <summary>
/// Interface for localization service
/// </summary>
public interface ILocalizationService
{
    string CurrentLanguage { get; }
    IReadOnlyList<LanguageInfo> AvailableLanguages { get; }
    event EventHandler? LanguageChanged;

    Task InitializeAsync(string? languageCode = null);
    Task SetLanguageAsync(string languageCode);
    string GetString(string key);
    string GetString(string key, params object[] args);
    string this[string key] { get; }
}
