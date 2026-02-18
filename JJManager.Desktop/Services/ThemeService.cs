using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

namespace JJManager.Desktop.Services;

/// <summary>
/// Service for managing application themes and accent colors
/// </summary>
public class ThemeService : IThemeService
{
    private static ThemeService? _instance;
    public static ThemeService Instance => _instance ??= new ThemeService();

    private ThemeMode _currentTheme = ThemeMode.Dark;
    private AccentColor _currentAccentColor = AccentColor.Purple;

    public event EventHandler? ThemeChanged;

    public IReadOnlyList<ThemeModeInfo> AvailableThemes { get; } = new List<ThemeModeInfo>
    {
        new(ThemeMode.Dark, "Settings_Theme_Dark", "Escuro"),
        new(ThemeMode.Light, "Settings_Theme_Light", "Claro"),
        new(ThemeMode.System, "Settings_Theme_System", "Sistema")
    };

    public IReadOnlyList<AccentColorInfo> AvailableAccentColors { get; } = new List<AccentColorInfo>
    {
        new(AccentColor.Purple, "Settings_Color_Purple", "#6200EE", "#BB86FC"),
        new(AccentColor.Blue, "Settings_Color_Blue", "#2196F3", "#82B1FF"),
        new(AccentColor.Green, "Settings_Color_Green", "#4CAF50", "#69F0AE"),
        new(AccentColor.Orange, "Settings_Color_Orange", "#FF9800", "#FFAB40"),
        new(AccentColor.Red, "Settings_Color_Red", "#F44336", "#FF5252"),
        new(AccentColor.Pink, "Settings_Color_Pink", "#E91E63", "#FF4081"),
        new(AccentColor.Teal, "Settings_Color_Teal", "#009688", "#64FFDA")
    };

    public ThemeMode CurrentTheme => _currentTheme;
    public AccentColor CurrentAccentColor => _currentAccentColor;

    public ThemeService()
    {
        _instance = this;
    }

    /// <summary>
    /// Initialize theme service with saved preferences
    /// </summary>
    public void Initialize(string? themeMode = null, string? accentColor = null)
    {
        if (!string.IsNullOrEmpty(themeMode) && Enum.TryParse<ThemeMode>(themeMode, out var theme))
        {
            _currentTheme = theme;
        }

        if (!string.IsNullOrEmpty(accentColor) && Enum.TryParse<AccentColor>(accentColor, out var accent))
        {
            _currentAccentColor = accent;
        }

        // ApplyTheme already calls ApplyThemeColors and ApplyAccentColor
        ApplyTheme();
    }

    /// <summary>
    /// Set the application theme
    /// </summary>
    public void SetTheme(ThemeMode theme)
    {
        if (_currentTheme != theme)
        {
            _currentTheme = theme;
            ApplyTheme();
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Set the accent color
    /// </summary>
    public void SetAccentColor(AccentColor color)
    {
        if (_currentAccentColor != color)
        {
            _currentAccentColor = color;
            ApplyAccentColor();
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ApplyTheme()
    {
        if (Application.Current == null) return;

        var requestedTheme = _currentTheme switch
        {
            ThemeMode.Dark => ThemeVariant.Dark,
            ThemeMode.Light => ThemeVariant.Light,
            ThemeMode.System => ThemeVariant.Default,
            _ => ThemeVariant.Dark
        };

        Application.Current.RequestedThemeVariant = requestedTheme;

        // Apply all theme colors
        ApplyThemeColors();

        // Re-apply accent colors (they depend on theme)
        ApplyAccentColor();
    }

    private void ApplyThemeColors()
    {
        if (Application.Current?.Resources == null) return;

        var isDark = _currentTheme == ThemeMode.Dark ||
                    (_currentTheme == ThemeMode.System && Application.Current.ActualThemeVariant == ThemeVariant.Dark);

        // Define theme colors
        var appBackgroundHex = isDark ? "#1E1E1E" : "#F5F5F5";
        var cardBackgroundHex = isDark ? "#2D2D2D" : "#FFFFFF";
        var cardBackgroundAltHex = isDark ? "#252525" : "#FAFAFA";
        var borderHex = isDark ? "#444444" : "#E0E0E0";
        var borderAltHex = isDark ? "#333333" : "#EEEEEE";

        // Semantic colors
        var successHex = isDark ? "#4CAF50" : "#2E7D32";
        var neutralHex = isDark ? "#757575" : "#9E9E9E";
        var textSecondaryHex = isDark ? "#AAFFFFFF" : "#99000000";
        var overlayHex = "#1A000000";

        // Apply App Background
        if (Color.TryParse(appBackgroundHex, out var appBgColor))
        {
            Application.Current.Resources["AppBackgroundColor"] = appBgColor;
            Application.Current.Resources["AppBackgroundBrush"] = new SolidColorBrush(appBgColor);
        }

        // Apply Card Background
        if (Color.TryParse(cardBackgroundHex, out var cardBgColor))
        {
            Application.Current.Resources["CardBackgroundColor"] = cardBgColor;
            Application.Current.Resources["CardBackgroundBrush"] = new SolidColorBrush(cardBgColor);
        }

        // Apply Card Background Alt
        if (Color.TryParse(cardBackgroundAltHex, out var cardBgAltColor))
        {
            Application.Current.Resources["CardBackgroundAltColor"] = cardBgAltColor;
            Application.Current.Resources["CardBackgroundAltBrush"] = new SolidColorBrush(cardBgAltColor);
        }

        // Apply Border
        if (Color.TryParse(borderHex, out var borderColor))
        {
            Application.Current.Resources["BorderColor"] = borderColor;
            Application.Current.Resources["BorderBrush"] = new SolidColorBrush(borderColor);
        }

        // Apply Border Alt
        if (Color.TryParse(borderAltHex, out var borderAltColor))
        {
            Application.Current.Resources["BorderAltColor"] = borderAltColor;
            Application.Current.Resources["BorderAltBrush"] = new SolidColorBrush(borderAltColor);
        }

        // Apply Success
        if (Color.TryParse(successHex, out var successColor))
        {
            Application.Current.Resources["SuccessColor"] = successColor;
            Application.Current.Resources["SuccessBrush"] = new SolidColorBrush(successColor);
        }

        // Apply Neutral
        if (Color.TryParse(neutralHex, out var neutralColor))
        {
            Application.Current.Resources["NeutralColor"] = neutralColor;
            Application.Current.Resources["NeutralBrush"] = new SolidColorBrush(neutralColor);
        }

        // Apply Text Secondary
        if (Color.TryParse(textSecondaryHex, out var textSecondaryColor))
        {
            Application.Current.Resources["TextSecondaryColor"] = textSecondaryColor;
            Application.Current.Resources["TextSecondaryBrush"] = new SolidColorBrush(textSecondaryColor);
        }

        // Apply Overlay
        if (Color.TryParse(overlayHex, out var overlayColor))
        {
            Application.Current.Resources["OverlayColor"] = overlayColor;
            Application.Current.Resources["OverlayBrush"] = new SolidColorBrush(overlayColor);
        }
    }

    private void ApplyAccentColor()
    {
        if (Application.Current?.Resources == null) return;

        var colorInfo = AvailableAccentColors.FirstOrDefault(c => c.Color == _currentAccentColor)
                       ?? AvailableAccentColors.First();

        var isDark = _currentTheme == ThemeMode.Dark ||
                    (_currentTheme == ThemeMode.System && Application.Current.ActualThemeVariant == ThemeVariant.Dark);

        var accentHex = isDark ? colorInfo.DarkHex : colorInfo.LightHex;

        if (Color.TryParse(accentHex, out var accentColor))
        {
            // Custom accent resources
            Application.Current.Resources["AccentColor"] = accentColor;
            Application.Current.Resources["AccentBrush"] = new SolidColorBrush(accentColor);

            // Calculate hover color (lighter)
            var hoverColor = LightenColor(accentColor, 0.2);
            Application.Current.Resources["AccentHoverColor"] = hoverColor;
            Application.Current.Resources["AccentHoverBrush"] = new SolidColorBrush(hoverColor);

            // Calculate pressed color (darker)
            var pressedColor = DarkenColor(accentColor, 0.2);
            Application.Current.Resources["AccentPressedColor"] = pressedColor;
            Application.Current.Resources["AccentPressedBrush"] = new SolidColorBrush(pressedColor);

            // Update Avalonia FluentTheme system accent colors
            Application.Current.Resources["SystemAccentColor"] = accentColor;
            Application.Current.Resources["SystemAccentColorLight1"] = LightenColor(accentColor, 0.15);
            Application.Current.Resources["SystemAccentColorLight2"] = LightenColor(accentColor, 0.30);
            Application.Current.Resources["SystemAccentColorLight3"] = LightenColor(accentColor, 0.45);
            Application.Current.Resources["SystemAccentColorDark1"] = DarkenColor(accentColor, 0.15);
            Application.Current.Resources["SystemAccentColorDark2"] = DarkenColor(accentColor, 0.30);
            Application.Current.Resources["SystemAccentColorDark3"] = DarkenColor(accentColor, 0.45);
        }

        // Also update header background
        if (Color.TryParse(colorInfo.LightHex, out var headerColor))
        {
            Application.Current.Resources["HeaderBackground"] = new SolidColorBrush(headerColor);
        }
    }

    /// <summary>
    /// Lighten a color by the specified amount (0-1)
    /// </summary>
    private static Color LightenColor(Color color, double amount)
    {
        var r = (byte)Math.Min(255, color.R + (255 - color.R) * amount);
        var g = (byte)Math.Min(255, color.G + (255 - color.G) * amount);
        var b = (byte)Math.Min(255, color.B + (255 - color.B) * amount);
        return Color.FromArgb(color.A, r, g, b);
    }

    /// <summary>
    /// Darken a color by the specified amount (0-1)
    /// </summary>
    private static Color DarkenColor(Color color, double amount)
    {
        var r = (byte)(color.R * (1 - amount));
        var g = (byte)(color.G * (1 - amount));
        var b = (byte)(color.B * (1 - amount));
        return Color.FromArgb(color.A, r, g, b);
    }

    /// <summary>
    /// Get the current accent color as a brush
    /// </summary>
    public IBrush GetAccentBrush()
    {
        var colorInfo = AvailableAccentColors.FirstOrDefault(c => c.Color == _currentAccentColor)
                       ?? AvailableAccentColors.First();

        var isDark = _currentTheme == ThemeMode.Dark;
        var accentHex = isDark ? colorInfo.DarkHex : colorInfo.LightHex;

        return Color.TryParse(accentHex, out var color)
            ? new SolidColorBrush(color)
            : new SolidColorBrush(Colors.Purple);
    }
}

/// <summary>
/// Theme mode options
/// </summary>
public enum ThemeMode
{
    Dark,
    Light,
    System
}

/// <summary>
/// Accent color options
/// </summary>
public enum AccentColor
{
    Purple,
    Blue,
    Green,
    Orange,
    Red,
    Pink,
    Teal
}

/// <summary>
/// Information about a theme mode
/// </summary>
public record ThemeModeInfo(ThemeMode Mode, string ResourceKey, string DefaultName);

/// <summary>
/// Information about an accent color
/// </summary>
public record AccentColorInfo(AccentColor Color, string ResourceKey, string LightHex, string DarkHex);

/// <summary>
/// Interface for theme service
/// </summary>
public interface IThemeService
{
    ThemeMode CurrentTheme { get; }
    AccentColor CurrentAccentColor { get; }
    IReadOnlyList<ThemeModeInfo> AvailableThemes { get; }
    IReadOnlyList<AccentColorInfo> AvailableAccentColors { get; }
    event EventHandler? ThemeChanged;

    void Initialize(string? themeMode = null, string? accentColor = null);
    void SetTheme(ThemeMode theme);
    void SetAccentColor(AccentColor color);
    IBrush GetAccentBrush();
}
