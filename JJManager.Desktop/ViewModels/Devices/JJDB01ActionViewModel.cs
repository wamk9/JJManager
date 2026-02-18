using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JJManager.Core.Connections.SimHub;
using JJManager.Core.Others;
using JJManager.Core.Profile;
using JJManager.Desktop.Services;
using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading.Tasks;

namespace JJManager.Desktop.ViewModels.Devices;

public partial class JJDB01ActionViewModel : ViewModelBase
{
    private readonly Action<LedActionResult?> _closeCallback;
    private readonly int _ledIndex;
    private readonly int? _editingOutputIndex;
    private readonly DispatcherTimer _simHubValueTimer;
    private SimHubWebsocket? _simHubWebsocket;
    private bool _isConnectingToSimHub;

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<SimHubPropertyItem> _simHubProperties = new();

    [ObservableProperty]
    private SimHubPropertyItem? _selectedProperty;

    [ObservableProperty]
    private Color _selectedColor = Color.FromRgb(255, 0, 0);

    [ObservableProperty]
    private string _hexColor = "#FF0000";

    [ObservableProperty]
    private ObservableCollection<ComparativeItem> _comparatives = new();

    [ObservableProperty]
    private ComparativeItem? _selectedComparative;

    [ObservableProperty]
    private string _activationValue = "true";

    [ObservableProperty]
    private ObservableCollection<string> _suggestedValues = new();

    [ObservableProperty]
    private bool _hasSuggestedValues;

    [ObservableProperty]
    private string? _selectedSuggestedValue;

    [ObservableProperty]
    private bool _isCustomValueMode;

    [ObservableProperty]
    private string _customValue = "";

    [ObservableProperty]
    private byte _colorR = 255;

    [ObservableProperty]
    private byte _colorG = 0;

    [ObservableProperty]
    private byte _colorB = 0;

    [ObservableProperty]
    private ObservableCollection<LedModeItem> _ledModes = new();

    [ObservableProperty]
    private LedModeItem? _selectedLedMode;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private string _currentPropertyValueDisplay = "";

    [ObservableProperty]
    private bool _hasCurrentValue;

    #endregion

    #region Computed Properties

    private static ILocalizationService Localization => LocalizationService.Instance;

    public string WindowTitle => IsEditMode
        ? Localization.GetString("JJDB01_Action_EditTitle")
        : Localization.GetString("JJDB01_Action_AddTitle");

    public string LedLabel => $"LED {_ledIndex + 1}";

    public string CustomOptionText => Localization.GetString("JJDB01_Action_CustomValue");

    public string CurrentValueLabel => Localization.GetString("JJDB01_Action_CurrentValue");

    #endregion

    #region Constructor

    public JJDB01ActionViewModel(int ledIndex, Action<LedActionResult?> closeCallback, LedConfiguration? existingConfig = null, int? editingOutputIndex = null)
    {
        _ledIndex = ledIndex;
        _closeCallback = closeCallback;
        _isEditMode = existingConfig != null;
        _editingOutputIndex = editingOutputIndex;

        // Initialize timer for SimHub value updates
        _simHubValueTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _simHubValueTimer.Tick += OnSimHubValueTimerTick;
        _simHubValueTimer.Start();

        InitializeSimHubProperties();
        InitializeComparatives();
        InitializeLedModes();

        if (existingConfig != null)
        {
            LoadExistingConfig(existingConfig);
        }
    }

    #endregion

    #region Initialization

    private void InitializeSimHubProperties()
    {
        // Load only properties that can be used as LED triggers from the service
        var properties = SimHubPropertiesService.Instance.GetLedTriggerProperties();

        foreach (var prop in properties)
        {
            SimHubProperties.Add(prop);
        }

        SelectedProperty = SimHubProperties.Count > 0 ? SimHubProperties[0] : null;
    }

    private void InitializeComparatives()
    {
        // Comparatives are now dynamically updated based on the selected property
        // This method is called for initial setup, but the actual population
        // happens in OnSelectedPropertyChanged when a property is selected
    }

    private void InitializeLedModes()
    {
        LedModes.Add(new LedModeItem(0, Localization.GetString("LedMode_Off")));
        LedModes.Add(new LedModeItem(1, Localization.GetString("LedMode_On")));
        LedModes.Add(new LedModeItem(2, Localization.GetString("LedMode_Blink")));
        LedModes.Add(new LedModeItem(3, Localization.GetString("LedMode_Pulse")));

        SelectedLedMode = LedModes[1]; // Default to "On"
    }

    private void LoadExistingConfig(LedConfiguration config)
    {
        // Store values to load after property selection triggers list updates
        var targetComparative = config.Comparative;
        var targetValue = config.ValueToActivate ?? "true";

        // Find and select the property (this triggers OnSelectedPropertyChanged which updates Comparatives and SuggestedValues)
        foreach (var prop in SimHubProperties)
        {
            if (prop.PropertyKey == config.Property)
            {
                SelectedProperty = prop;
                break;
            }
        }

        // Load color
        if (!string.IsNullOrEmpty(config.Color))
        {
            try
            {
                var color = Color.Parse(config.Color);
                ColorR = color.R;
                ColorG = color.G;
                ColorB = color.B;
                SelectedColor = color;
            }
            catch { }
        }

        // Load comparative (after Comparatives list was populated by OnSelectedPropertyChanged)
        foreach (var comp in Comparatives)
        {
            if (comp.Type == targetComparative)
            {
                SelectedComparative = comp;
                break;
            }
        }

        // Load activation value
        ActivationValue = targetValue;

        // Check if value is in suggested values list
        if (SuggestedValues.Contains(targetValue))
        {
            SelectedSuggestedValue = targetValue;
            IsCustomValueMode = false;
        }
        else if (SuggestedValues.Count > 0)
        {
            // Value not in list, use custom mode
            IsCustomValueMode = true;
            CustomValue = targetValue;
            SelectedSuggestedValue = CustomOptionText;
        }

        // Load LED mode
        foreach (var mode in LedModes)
        {
            if (mode.Value == config.ModeIfEnabled)
            {
                SelectedLedMode = mode;
                break;
            }
        }
    }

    #endregion

    #region Property Changed

    partial void OnSelectedPropertyChanged(SimHubPropertyItem? value)
    {
        UpdateComparativesForProperty(value);
        UpdateSuggestedValuesForProperty(value);
        _ = UpdateCurrentPropertyValueAsync();
    }

    private async void OnSimHubValueTimerTick(object? sender, EventArgs e)
    {
        await UpdateCurrentPropertyValueAsync();
    }

    private async Task UpdateCurrentPropertyValueAsync()
    {
        if (SelectedProperty == null)
        {
            HasCurrentValue = false;
            CurrentPropertyValueDisplay = "";
            return;
        }

        HasCurrentValue = true;

        // Try to connect to SimHub if not connected
        if (_simHubWebsocket == null || !_simHubWebsocket.IsConnected)
        {
            if (!_isConnectingToSimHub)
            {
                _isConnectingToSimHub = true;
                CurrentPropertyValueDisplay = Localization.GetString("JJDB01_Action_WaitingSimHub");

                try
                {
                    _simHubWebsocket?.Dispose();
                    _simHubWebsocket = new SimHubWebsocket(2920, "JJDB01_ActionPreview");
                    await _simHubWebsocket.ConnectAsync();
                }
                catch
                {
                    // Connection failed, will retry on next tick
                }
                finally
                {
                    _isConnectingToSimHub = false;
                }
            }
            return;
        }

        // Request data from SimHub
        try
        {
            var (success, _) = await _simHubWebsocket.RequestDataAsync();

            if (success && _simHubWebsocket.LastValues.ContainsKey(SelectedProperty.PropertyKey))
            {
                var jsonValue = _simHubWebsocket.LastValues[SelectedProperty.PropertyKey];
                var currentValue = ExtractValue(jsonValue);
                CurrentPropertyValueDisplay = FormatCurrentValue(currentValue, SelectedProperty);
            }
            else
            {
                CurrentPropertyValueDisplay = Localization.GetString("JJDB01_Action_WaitingSimHub");
            }
        }
        catch
        {
            CurrentPropertyValueDisplay = Localization.GetString("JJDB01_Action_WaitingSimHub");
        }
    }

    private object? ExtractValue(System.Text.Json.Nodes.JsonNode? node)
    {
        if (node == null) return null;

        try
        {
            var kind = node.GetValueKind();
            return kind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Number => node.GetValue<double>(),
                JsonValueKind.String => node.GetValue<string>(),
                _ => node.ToString()
            };
        }
        catch
        {
            return node.ToString();
        }
    }

    private string FormatCurrentValue(object? value, SimHubPropertyItem property)
    {
        if (value == null)
        {
            return Localization.GetString("JJDB01_Action_WaitingSimHub");
        }

        try
        {
            // Handle different types
            if (value is bool boolVal)
            {
                return boolVal ? "true" : "false";
            }

            if (value is double doubleVal)
            {
                // Format based on property type
                if (property.PropertyKey.Contains("Percent") || property.PropertyKey.Contains("Rpm"))
                {
                    return doubleVal.ToString("N0");
                }
                else if (property.PropertyKey.Contains("Time"))
                {
                    var ts = TimeSpan.FromSeconds(doubleVal);
                    return ts.ToString(@"mm\:ss\.fff");
                }
                else if (property.PropertyKey.Contains("Temperature") || property.PropertyKey.Contains("Pressure"))
                {
                    return doubleVal.ToString("N1");
                }
                else if (property.PropertyKey.Contains("Speed"))
                {
                    return doubleVal.ToString("N0") + " km/h";
                }
                else
                {
                    return doubleVal.ToString("N2");
                }
            }

            if (value is int intVal)
            {
                return intVal.ToString();
            }

            return value.ToString() ?? "";
        }
        catch
        {
            return value.ToString() ?? "";
        }
    }

    public void StopTimer()
    {
        _simHubValueTimer.Stop();
        _simHubWebsocket?.Dispose();
        _simHubWebsocket = null;
    }

    private void UpdateComparativesForProperty(SimHubPropertyItem? property)
    {
        if (property == null) return;

        var previousSelection = SelectedComparative?.Type;
        Comparatives.Clear();

        if (property.SupportedComparatives != null)
        {
            foreach (var compType in property.SupportedComparatives)
            {
                var symbol = compType switch
                {
                    ComparativeType.Equals => "=",
                    ComparativeType.NotEquals => "≠",
                    ComparativeType.GreaterThan => ">",
                    ComparativeType.GreaterOrEquals => "≥",
                    ComparativeType.LessThan => "<",
                    ComparativeType.LessOrEquals => "≤",
                    _ => "="
                };

                var locKey = $"Comparative_{compType}";
                Comparatives.Add(new ComparativeItem(compType, symbol, Localization.GetString(locKey)));
            }
        }

        // Try to restore previous selection, or default to first
        if (previousSelection.HasValue)
        {
            foreach (var comp in Comparatives)
            {
                if (comp.Type == previousSelection.Value)
                {
                    SelectedComparative = comp;
                    return;
                }
            }
        }

        SelectedComparative = Comparatives.Count > 0 ? Comparatives[0] : null;
    }

    private void UpdateSuggestedValuesForProperty(SimHubPropertyItem? property)
    {
        SuggestedValues.Clear();
        IsCustomValueMode = false;
        CustomValue = "";

        if (property?.SuggestedValues != null && property.SuggestedValues.Count > 0)
        {
            foreach (var val in property.SuggestedValues)
            {
                SuggestedValues.Add(val);
            }
            // Add "Custom..." option at the end
            SuggestedValues.Add(CustomOptionText);
            HasSuggestedValues = true;

            // Set default activation value based on field type
            if (property.FieldType == SimHubFieldType.Boolean)
            {
                ActivationValue = "true";
                SelectedSuggestedValue = "true";
            }
            else if (SuggestedValues.Count > 1) // > 1 because we added Custom option
            {
                ActivationValue = SuggestedValues[0];
                SelectedSuggestedValue = SuggestedValues[0];
            }
        }
        else
        {
            HasSuggestedValues = false;

            // Set default activation value based on field type
            if (property?.FieldType == SimHubFieldType.Boolean)
            {
                ActivationValue = "true";
            }
            else if (property?.FieldType == SimHubFieldType.Number)
            {
                ActivationValue = "0";
            }
            else
            {
                ActivationValue = "";
            }
        }
    }

    partial void OnSelectedSuggestedValueChanged(string? value)
    {
        if (value == CustomOptionText)
        {
            IsCustomValueMode = true;
            ActivationValue = CustomValue;
        }
        else if (value != null)
        {
            IsCustomValueMode = false;
            ActivationValue = value;
        }
    }

    partial void OnCustomValueChanged(string value)
    {
        if (IsCustomValueMode)
        {
            ActivationValue = value;
        }
    }

    partial void OnSelectedColorChanged(Color value)
    {
        // Update hex and RGB values without triggering recursion
        _hexColor = $"#{value.R:X2}{value.G:X2}{value.B:X2}";
        _colorR = value.R;
        _colorG = value.G;
        _colorB = value.B;
        OnPropertyChanged(nameof(HexColor));
        OnPropertyChanged(nameof(ColorR));
        OnPropertyChanged(nameof(ColorG));
        OnPropertyChanged(nameof(ColorB));
    }

    partial void OnHexColorChanged(string value)
    {
        try
        {
            if (!string.IsNullOrEmpty(value) && value.StartsWith("#") && value.Length == 7)
            {
                var newColor = Color.Parse(value);
                if (newColor != SelectedColor)
                {
                    SelectedColor = newColor;
                }
            }
        }
        catch { }
    }

    partial void OnColorRChanged(byte value)
    {
        UpdateColorFromRgb();
    }

    partial void OnColorGChanged(byte value)
    {
        UpdateColorFromRgb();
    }

    partial void OnColorBChanged(byte value)
    {
        UpdateColorFromRgb();
    }

    private void UpdateColorFromRgb()
    {
        var newColor = Color.FromRgb(ColorR, ColorG, ColorB);
        if (newColor != SelectedColor)
        {
            SelectedColor = newColor;
        }
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void Save()
    {
        if (SelectedProperty == null || SelectedComparative == null || SelectedLedMode == null)
            return;

        StopTimer();

        var result = new LedActionResult
        {
            EditingOutputIndex = _editingOutputIndex,
            LedIndex = _ledIndex,
            Property = SelectedProperty.PropertyKey,
            PropertyName = SelectedProperty.DisplayName,
            Color = HexColor,
            Comparative = SelectedComparative.Type,
            ValueToActivate = ActivationValue,
            ModeIfEnabled = SelectedLedMode.Value
        };

        _closeCallback(result);
    }

    [RelayCommand]
    private void Cancel()
    {
        StopTimer();
        _closeCallback(null);
    }

    [RelayCommand]
    private void SetPresetColor(string hexColor)
    {
        HexColor = hexColor;
        try
        {
            SelectedColor = Color.Parse(hexColor);
        }
        catch { }
    }

    #endregion
}

#region Supporting Classes
public class LedModeItem
{
    public int Value { get; }
    public string DisplayName { get; }

    public LedModeItem(int value, string displayName)
    {
        Value = value;
        DisplayName = displayName;
    }

    public override string ToString() => DisplayName;
}

public class LedActionResult
{
    public int? EditingOutputIndex { get; set; } // Null = new action, Value = editing existing
    public int LedIndex { get; set; }
    public string Property { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public string Color { get; set; } = "#FF0000";
    public ComparativeType Comparative { get; set; }
    public string ValueToActivate { get; set; } = "true";
    public int ModeIfEnabled { get; set; } = 1;
}

#endregion
