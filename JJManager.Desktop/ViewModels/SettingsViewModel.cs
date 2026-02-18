using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JJManager.Desktop.Services;
using System.Collections.ObjectModel;
using JJManager.Core.Interfaces.Services;
using System.IO;

namespace JJManager.Desktop.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IAppConfigService _appConfigService;
    private readonly ILocalizationService _localizationService;
    private readonly IThemeService _themeService;

    // Theme settings
    [ObservableProperty]
    private ThemeModeInfo? _selectedTheme;

    [ObservableProperty]
    private ObservableCollection<ThemeModeInfo> _availableThemes = new();

    [ObservableProperty]
    private AccentColorInfo? _selectedAccentColor;

    [ObservableProperty]
    private ObservableCollection<AccentColorInfo> _availableAccentColors = new();

    // Language settings
    [ObservableProperty]
    private LanguageInfo? _selectedLanguage;

    [ObservableProperty]
    private ObservableCollection<LanguageInfo> _availableLanguages = new();

    // Behavior settings
    [ObservableProperty]
    private bool _startWithWindows;

    [ObservableProperty]
    private bool _minimizeToTray = true;

    [ObservableProperty]
    private bool _autoConnectDevices = true;

    // Data paths
    [ObservableProperty]
    private string _databasePath = string.Empty;

    [ObservableProperty]
    private string _logPath = string.Empty;

    // Logs settings
    [ObservableProperty]
    private bool _logsEnabled = true;

    [ObservableProperty]
    private ObservableCollection<LogFileInfo> _logFiles = new();

    [ObservableProperty]
    private bool _hasLogFiles;

    public SettingsViewModel(IAppConfigService appConfigService, ILocalizationService localizationService, IThemeService themeService)
    {
        _localizationService = localizationService;
        _themeService = themeService;
        _appConfigService = appConfigService;

        // Load available themes
        foreach (var theme in _themeService.AvailableThemes)
        {
            AvailableThemes.Add(theme);
        }

        // Load available accent colors
        foreach (var color in _themeService.AvailableAccentColors)
        {
            AvailableAccentColors.Add(color);
        }

        // Load available languages
        foreach (var lang in _localizationService.AvailableLanguages)
        {
            AvailableLanguages.Add(lang);
        }

        LoadSettings();
    }

    private async void LoadSettings()
    {
        // Load theme
        var savedTheme = await _appConfigService.GetConfigAsync("theme_mode");
        var themeMode = !string.IsNullOrEmpty(savedTheme) && Enum.TryParse<ThemeMode>(savedTheme, out var tm)
            ? tm
            : _themeService.CurrentTheme;
        SelectedTheme = AvailableThemes.FirstOrDefault(t => t.Mode == themeMode) ?? AvailableThemes.First();

        // Load accent color
        var savedAccent = await _appConfigService.GetConfigAsync("accent_color");
        var accentColor = !string.IsNullOrEmpty(savedAccent) && Enum.TryParse<AccentColor>(savedAccent, out var ac)
            ? ac
            : _themeService.CurrentAccentColor;
        SelectedAccentColor = AvailableAccentColors.FirstOrDefault(c => c.Color == accentColor) ?? AvailableAccentColors.First();

        // Load language
        var savedLanguage = await _appConfigService.GetConfigAsync("language");
        var langCode = !string.IsNullOrEmpty(savedLanguage) ? savedLanguage : _localizationService.CurrentLanguage;
        SelectedLanguage = AvailableLanguages.FirstOrDefault(l => l.Code == langCode) ?? AvailableLanguages.First();

        // Load behavior settings
        var autoStart = await _appConfigService.GetConfigAsync("auto_start");
        StartWithWindows = autoStart == "true";

        var minimizeToTray = await _appConfigService.GetConfigAsync("minimize_to_tray");
        MinimizeToTray = minimizeToTray != "false";

        var autoConnect = await _appConfigService.GetConfigAsync("auto_connect_devices");
        AutoConnectDevices = autoConnect != "false";

        // Load paths
        DatabasePath = _appConfigService.GetDatabaseFolder();
        LogPath = Path.Combine(DatabasePath, "logs");

        // Load logs enabled setting
        var logsEnabled = await _appConfigService.GetConfigAsync("logs_enabled");
        LogsEnabled = logsEnabled != "false";

        // Load log files
        RefreshLogFiles();
    }

    partial void OnLogsEnabledChanged(bool value)
    {
        SaveSetting("logs_enabled", value.ToString().ToLower());
    }

    [RelayCommand]
    private void RefreshLogFiles()
    {
        LogFiles.Clear();

        if (!Directory.Exists(LogPath))
        {
            HasLogFiles = false;
            return;
        }

        var files = Directory.GetFiles(LogPath, "*.txt")
            .Concat(Directory.GetFiles(LogPath, "*.log"))
            .OrderByDescending(f => new FileInfo(f).LastWriteTime);

        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            LogFiles.Add(new LogFileInfo
            {
                FileName = fileInfo.Name,
                FilePath = fileInfo.FullName,
                Size = FormatFileSize(fileInfo.Length),
                LastModified = fileInfo.LastWriteTime.ToString("dd/MM/yyyy HH:mm"),
                SizeBytes = fileInfo.Length
            });
        }

        HasLogFiles = LogFiles.Count > 0;
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }

    [RelayCommand]
    private void OpenLogFile(LogFileInfo? logFile)
    {
        if (logFile == null || !File.Exists(logFile.FilePath)) return;

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = logFile.FilePath,
                UseShellExecute = true
            });
        }
        catch
        {
            // Ignore errors opening file
        }
    }

    [RelayCommand]
    private void DeleteLogFile(LogFileInfo? logFile)
    {
        if (logFile == null || !File.Exists(logFile.FilePath)) return;

        try
        {
            File.Delete(logFile.FilePath);
            LogFiles.Remove(logFile);
            HasLogFiles = LogFiles.Count > 0;
        }
        catch
        {
            // Ignore errors deleting file
        }
    }

    [RelayCommand]
    private void DeleteAllLogFiles()
    {
        if (!Directory.Exists(LogPath)) return;

        try
        {
            foreach (var logFile in LogFiles.ToList())
            {
                if (File.Exists(logFile.FilePath))
                {
                    File.Delete(logFile.FilePath);
                }
            }
            LogFiles.Clear();
            HasLogFiles = false;
        }
        catch
        {
            // Refresh to show remaining files
            RefreshLogFiles();
        }
    }

    partial void OnSelectedThemeChanged(ThemeModeInfo? value)
    {
        if (value != null)
        {
            SaveSetting("theme_mode", value.Mode.ToString());
            _themeService.SetTheme(value.Mode);
        }
    }

    partial void OnSelectedAccentColorChanged(AccentColorInfo? value)
    {
        if (value != null)
        {
            SaveSetting("accent_color", value.Color.ToString());
            _themeService.SetAccentColor(value.Color);
        }
    }

    async partial void OnSelectedLanguageChanged(LanguageInfo? value)
    {
        if (value != null)
        {
            SaveSetting("language", value.Code);
            await _localizationService.SetLanguageAsync(value.Code);
        }
    }

    partial void OnStartWithWindowsChanged(bool value)
    {
        SaveSetting("auto_start", value.ToString().ToLower());
        // TODO: Register/unregister startup
    }

    partial void OnMinimizeToTrayChanged(bool value)
    {
        SaveSetting("minimize_to_tray", value.ToString().ToLower());
    }

    partial void OnAutoConnectDevicesChanged(bool value)
    {
        SaveSetting("auto_connect_devices", value.ToString().ToLower());
    }

    private async void SaveSetting(string key, string value)
    {
        await _appConfigService.SetConfigAsync(key, value);
    }

    [RelayCommand]
    private void OpenDatabaseFolder()
    {
        try
        {
            var folder = _appConfigService.GetDatabaseFolder();
            if (System.IO.Directory.Exists(folder))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = folder,
                    UseShellExecute = true
                });
            }
        }
        catch
        {
            // Ignore errors opening folder
        }
    }

    [RelayCommand]
    private void OpenLogFolder()
    {
        try
        {
            if (!System.IO.Directory.Exists(LogPath))
            {
                System.IO.Directory.CreateDirectory(LogPath);
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = LogPath,
                UseShellExecute = true
            });
        }
        catch
        {
            // Ignore errors opening folder
        }
    }

    [RelayCommand]
    private void SelectAccentColor(AccentColorInfo? colorInfo)
    {
        if (colorInfo != null)
        {
            SelectedAccentColor = colorInfo;
        }
    }

    [RelayCommand]
    private void ResetSettings()
    {
        SelectedTheme = AvailableThemes.FirstOrDefault(t => t.Mode == ThemeMode.Dark);
        SelectedAccentColor = AvailableAccentColors.FirstOrDefault(c => c.Color == AccentColor.Purple);
        SelectedLanguage = AvailableLanguages.FirstOrDefault(l => l.Code == "pt-BR");
        StartWithWindows = false;
        MinimizeToTray = true;
        AutoConnectDevices = true;
        LogsEnabled = true;
    }
}

/// <summary>
/// Represents a log file in the logs folder
/// </summary>
public class LogFileInfo
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string LastModified { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
}
