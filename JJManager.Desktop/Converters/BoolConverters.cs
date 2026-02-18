using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Material.Icons;

namespace JJManager.Desktop.Converters;

/// <summary>
/// Converts a boolean to one of two string values
/// Usage: {Binding IsConnected, Converter={x:Static converters:BoolToStringConverter.Instance}, ConverterParameter='Connected,Disconnected'}
/// </summary>
public class BoolToStringConverter : IValueConverter
{
    public static readonly BoolToStringConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string paramString)
        {
            var parts = paramString.Split(',');
            if (parts.Length >= 2)
            {
                return boolValue ? parts[0] : parts[1];
            }
        }
        return value?.ToString() ?? string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a boolean to one of two colors
/// Usage with resource keys: {Binding IsConnected, Converter={StaticResource BoolToColor}, ConverterParameter='SuccessColor,NeutralColor'}
/// Usage with hex colors: {Binding IsConnected, Converter={StaticResource BoolToColor}, ConverterParameter='#4CAF50,#757575'}
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public static readonly BoolToColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string paramString)
        {
            var parts = paramString.Split(',');
            if (parts.Length >= 2)
            {
                var colorKey = boolValue ? parts[0].Trim() : parts[1].Trim();

                // Try to get color from application resources first
                if (Application.Current?.Resources != null &&
                    Application.Current.Resources.TryGetResource(colorKey, Application.Current.ActualThemeVariant, out var resourceValue))
                {
                    if (resourceValue is Color color)
                        return color;
                    if (resourceValue is SolidColorBrush brush)
                        return brush.Color;
                }

                // Fallback to parsing as hex color
                if (colorKey.StartsWith("#") && Color.TryParse(colorKey, out var parsedColor))
                {
                    return parsedColor;
                }
            }
        }
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Inverts a boolean value
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public static readonly InverseBoolConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }
}

/// <summary>
/// Converts a string to MaterialIconKind enum
/// Usage: {Binding DeviceIconKind, Converter={StaticResource StringToMaterialIconKind}}
/// </summary>
public class StringToMaterialIconKindConverter : IValueConverter
{
    public static readonly StringToMaterialIconKindConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string iconName && !string.IsNullOrEmpty(iconName))
        {
            if (Enum.TryParse<MaterialIconKind>(iconName, true, out var iconKind))
            {
                return iconKind;
            }
        }
        return MaterialIconKind.Chip; // Default fallback
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is MaterialIconKind iconKind)
        {
            return iconKind.ToString();
        }
        return string.Empty;
    }
}

/// <summary>
/// Converts an avares:// URI string to a Bitmap image
/// Usage: {Binding ImageResourcePath, Converter={StaticResource StringToImage}}
/// </summary>
public class StringToImageConverter : IValueConverter
{
    public static readonly StringToImageConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string path && !string.IsNullOrEmpty(path))
        {
            try
            {
                var uri = new Uri(path);
                var assets = Avalonia.Platform.AssetLoader.Open(uri);
                return new Bitmap(assets);
            }
            catch
            {
                return null;
            }
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
