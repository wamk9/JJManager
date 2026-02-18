using Avalonia.Data.Converters;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JJManager.Core.Devices;
using JJManager.Core.Devices.JJDB01;
using JJManager.Core.Others;
using JJManager.Core.Profile;
using JJManager.Desktop.Services;
using JJManager.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace JJManager.Desktop.ViewModels.Devices;

public partial class JJDB01ViewModel : ViewModelBase
{
    private readonly JJDB01Device _device;
    private readonly Action _closeAction;
    private readonly IDeviceProfileService _profileService;

    private const int LED_COUNT = 16;

    public string IconName => _device.IconName;

    #region Observable Properties - Device State

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private bool _isSimHubConnected;

    [ObservableProperty]
    private int _brightness = 50;

    [ObservableProperty]
    private int _selectedLedIndex = -1;

    [ObservableProperty]
    private bool _hasSelectedLed;

    [ObservableProperty]
    private string _selectedLedLabel = string.Empty;

    [ObservableProperty]
    private bool _isSelectingDuplicateTarget;

    [ObservableProperty]
    private LedActionViewModel? _actionToDuplicate;

    #endregion

    #region Observable Properties - LED Colors

    [ObservableProperty]
    private ObservableCollection<LedViewModel> _leds = new();

    #endregion

    #region Observable Properties - Profile Management

    [ObservableProperty]
    private ObservableCollection<DeviceProfile> _profiles = new();

    [ObservableProperty]
    private DeviceProfile? _selectedProfile;

    /// <summary>
    /// Gets the Id of the selected profile (for reference/lookup)
    /// </summary>
    public Guid? SelectedProfileId => SelectedProfile?.Id;

    [ObservableProperty]
    private string _newProfileName = string.Empty;

    [ObservableProperty]
    private bool _canDeleteProfile;

    [ObservableProperty]
    private bool _canRenameProfile;

    [ObservableProperty]
    private bool _isDuplicatingProfile;

    [ObservableProperty]
    private string _duplicateProfileName = string.Empty;

    #endregion

    #region Observable Properties - LED Actions

    [ObservableProperty]
    private ObservableCollection<LedActionViewModel> _ledActions = new();

    #endregion

    #region Computed Properties

    private static ILocalizationService Localization => LocalizationService.Instance;

    public string ConnectionStatus => IsConnected
        ? Localization.GetString("Device_Connected")
        : Localization.GetString("Device_Disconnected");

    public string ConnectionTooltip => IsConnected
        ? Localization.GetString("Device_ConnectedTooltip")
        : Localization.GetString("Device_DisconnectedTooltip");

    public string SimHubConnectionStatus => IsSimHubConnected
        ? Localization.GetString("SimHub_Connected")
        : Localization.GetString("SimHub_Disconnected");

    public string SimHubTooltip
    {
        get
        {
            if (!IsConnected)
                return Localization.GetString("SimHub_DeviceNotConnectedTooltip");
            return IsSimHubConnected
                ? Localization.GetString("SimHub_ConnectedTooltip")
                : Localization.GetString("SimHub_DisconnectedTooltip");
        }
    }

    /// <summary>
    /// Color for SimHub status indicator:
    /// Green = connected, Gray = disconnected (device connected), Yellow = device not connected
    /// </summary>
    public Color SimHubStatusColor
    {
        get
        {
            if (!IsConnected)
                return Color.Parse("#FFC107"); // Yellow - device not connected
            return IsSimHubConnected
                ? Color.Parse("#4CAF50")  // Green - SimHub connected
                : Color.Parse("#757575"); // Gray - SimHub disconnected
        }
    }

    public string ConnectionButtonText => IsConnected
        ? Localization.GetString("Devices_Btn_Disconnect")
        : Localization.GetString("Devices_Btn_Connect");

    public bool CanEditConfiguration => IsConnected;

    public string BrightnessDisplayText => $"{Brightness} / 255 ({(Brightness * 100 / 255)}%)";

    public string FirmwareVersion => _device.FirmwareVersion?.ToString() ?? "N/A";

    #endregion

    #region Events

    public event EventHandler? LedsUpdated;
    public event EventHandler<int>? LedSelected;
    public event EventHandler<OpenActionWindowEventArgs>? OpenActionWindowRequested;

    #endregion

    #region Converters

    public static IValueConverter BoolToColorConverter { get; } = new FuncValueConverter<bool, Color>(
        value => value ? Color.Parse("#4CAF50") : Color.Parse("#757575"));

    public static IValueConverter SimHubColorConverter { get; } = new FuncValueConverter<bool, Color>(
        value => value ? Color.Parse("#4CAF50") : Color.Parse("#757575"));

    #endregion

    #region Constructor

    public JJDB01ViewModel(IDeviceProfileService profileService, JJDB01Device device, Action closeAction)
    {
        _device = device;
        _closeAction = closeAction;
        _isConnected = device.IsConnected;
        _brightness = device.Brightness;
        _profileService = profileService;

        // Initialize 16 LEDs
        for (int i = 0; i < LED_COUNT; i++)
        {
            var (r, g, b) = device.GetLedColor(i);
            Leds.Add(new LedViewModel(i, r, g, b));
        }

        // Subscribe to device events
        device.DataUpdated += OnDeviceDataUpdated;
        device.PropertyChanged += OnDevicePropertyChanged;

        // Initialize profiles and load from database
        InitializeProfilesAsync();
    }

    private async void InitializeProfilesAsync()
    {
        try
        {
            _device.Profile = await _profileService.GetDefaultProfileAsync(_device.ProductId, _device.ConnId);

            if (_device?.Profile != null)
            {
                await LoadProfiles();

                // Set selected profile (find in collection to ensure reference equality)
                _selectedProfile = Profiles.FirstOrDefault(p => p.Id == _device.Profile.Id);
                OnPropertyChanged(nameof(SelectedProfile));
                OnPropertyChanged(nameof(SelectedProfileId));

                LoadProfileData();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[JJDB01 VM] Error in InitializeProfilesAsync: {ex.Message}");
        }
    }

    #endregion

    #region Event Handlers

    private void OnDeviceDataUpdated(object? sender, JJDB01DataEventArgs e)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            Brightness = e.Brightness;

            // Update SimHub connection status
            IsSimHubConnected = _device.IsSimHubConnected;
            OnPropertyChanged(nameof(SimHubConnectionStatus));
            OnPropertyChanged(nameof(SimHubTooltip));

            // Update LED colors
            for (int i = 0; i < LED_COUNT && i < Leds.Count; i++)
            {
                int offset = i * 3;
                if (offset + 2 < e.LedColors.Length)
                {
                    Leds[i].SetColor(e.LedColors[offset], e.LedColors[offset + 1], e.LedColors[offset + 2]);
                }
            }

            LedsUpdated?.Invoke(this, EventArgs.Empty);
        });
    }

    private void OnDevicePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(JJDevice.IsConnected))
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                IsConnected = _device.IsConnected;
                OnPropertyChanged(nameof(ConnectionStatus));
                OnPropertyChanged(nameof(ConnectionTooltip));
                OnPropertyChanged(nameof(ConnectionButtonText));
                OnPropertyChanged(nameof(CanEditConfiguration));
                // SimHub status depends on device connection
                OnPropertyChanged(nameof(SimHubStatusColor));
                OnPropertyChanged(nameof(SimHubTooltip));
            });
        }
        else if (e.PropertyName == nameof(JJDB01Device.IsSimHubConnected))
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                IsSimHubConnected = _device.IsSimHubConnected;
                OnPropertyChanged(nameof(SimHubConnectionStatus));
                OnPropertyChanged(nameof(SimHubTooltip));
                OnPropertyChanged(nameof(SimHubStatusColor));
            });
        }
    }

    #endregion

    #region Public Methods

    public void SelectLed(int index)
    {
        if (index < 0 || index >= LED_COUNT)
        {
            DeselectLed();
            return;
        }

        // If selecting duplicate target
        if (IsSelectingDuplicateTarget && ActionToDuplicate != null)
        {
            CompleteDuplicateAction(index);
            return;
        }

        // Toggle: if clicking on same LED, deselect it
        if (SelectedLedIndex == index)
        {
            DeselectLed();
            return;
        }

        SelectedLedIndex = index;
        HasSelectedLed = true;
        SelectedLedLabel = $"LED {index + 1}";

        // Update selected state in LEDs
        for (int i = 0; i < Leds.Count; i++)
        {
            Leds[i].IsSelected = (i == index);
        }

        // Select LED on device (blink effect is handled in DataLoop)
        _device.SelectLed(index);

        // Load actions for this LED
        LoadLedActions(index);

        LedSelected?.Invoke(this, index);
    }

    public void DeselectLed()
    {
        SelectedLedIndex = -1;
        HasSelectedLed = false;
        SelectedLedLabel = string.Empty;
        LedActions.Clear();

        // Clear selected state in all LEDs
        for (int i = 0; i < Leds.Count; i++)
        {
            Leds[i].IsSelected = false;
        }

        _device.UnselectAllLeds();
        LedSelected?.Invoke(this, -1);
    }

    private void LoadLedActions(int ledIndex)
    {
        LedActions.Clear();

        if (_device.Profile == null)
            return;

        // Find outputs that control this LED
        foreach (var output in _device.Profile.Outputs)
        {
            if (output.Led?.LedsGrouped?.Contains(ledIndex) == true)
            {
                LedActions.Add(new LedActionViewModel
                {
                    Id = output.Index,
                    Order = output.Led.Order,
                    Property = output.Led.Property,
                    PropertyName = !string.IsNullOrEmpty(output.Led.PropertyName)
                        ? output.Led.PropertyName
                        : output.Led.Property,
                    Color = output.Led.Color,
                    ModeIfEnabled = output.Led.ModeIfEnabled,
                    Comparative = output.Led.Comparative,
                    ValueToActivate = output.Led.ValueToActivate
                });
            }
        }
    }

    public void HandleActionResult(LedActionResult? result)
    {
        if (result == null || _device.Profile == null)
            return;

        bool isEditing = result.EditingOutputIndex.HasValue;

        // Create LED configuration from result
        var ledConfig = new LedConfiguration
        {
            Property = result.Property,
            PropertyName = result.PropertyName,
            Color = result.Color,
            Order = isEditing ? GetExistingOrder(result.EditingOutputIndex!.Value) : LedActions.Count,
            Mode = -1, // Will be controlled by SimHub
            ModeIfEnabled = result.ModeIfEnabled,
            Brightness = 100,
            BlinkSpeed = 5,
            PulseDelay = 5,
            LedsGrouped = new List<int> { result.LedIndex },
            ValueToActivate = result.ValueToActivate,
            Comparative = result.Comparative,
            VarType = DetermineVariableType(result.ValueToActivate)
        };

        ProfileOutput output;

        if (isEditing)
        {
            // Update existing output
            var existingOutput = _device.Profile.Outputs.FirstOrDefault(o => o.Index == result.EditingOutputIndex);
            if (existingOutput != null)
            {
                existingOutput.Led = ledConfig;
                existingOutput.Configuration = null; // Will be regenerated on save
                output = existingOutput;
            }
            else
            {
                // Fallback to create new if not found
                output = _device.Profile.AddOrUpdateLedOutput(result.LedIndex, ledConfig);
            }

            // Update existing action in UI list
            var existingAction = LedActions.FirstOrDefault(a => a.Id == result.EditingOutputIndex);
            if (existingAction != null)
            {
                existingAction.Property = result.Property;
                existingAction.PropertyName = result.PropertyName;
                existingAction.Color = result.Color;
                existingAction.ModeIfEnabled = result.ModeIfEnabled;
            }
        }
        else
        {
            // Add new output
            output = _device.Profile.AddOrUpdateLedOutput(result.LedIndex, ledConfig);

            // Add to UI list
            var newAction = new LedActionViewModel
            {
                Id = output.Index,
                Order = ledConfig.Order,
                Property = result.Property,
                PropertyName = result.PropertyName,
                Color = result.Color,
                ModeIfEnabled = result.ModeIfEnabled,
                TargetLedIndex = result.LedIndex
            };

            LedActions.Add(newAction);
        }

        // Mark profile as changed and needing update
        _device.Profile.Changed = true;
        _device.Profile.MarkAsNeedsUpdate();

        // Save to database
        SaveProfile();

    }

    private int GetExistingOrder(int outputIndex)
    {
        var existingOutput = _device.Profile?.Outputs.FirstOrDefault(o => o.Index == outputIndex);
        return existingOutput?.Led?.Order ?? LedActions.Count;
    }

    private static VariableType DetermineVariableType(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return VariableType.Text;

        // Check if it's a boolean
        if (value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("false", StringComparison.OrdinalIgnoreCase))
            return VariableType.Text;

        // Check if it's a number
        if (double.TryParse(value, out _))
            return VariableType.Number;

        return VariableType.Text;
    }

    private async void SaveProfile()
    {
        if (_device.Profile == null)
        {
            return;
        }

        try
        {
            SaveCurrentProfileData();
            await _profileService.SaveAsync(_device.Profile);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[JJDB01 VM] Error saving profile: {ex.Message}");
        }
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task ToggleConnection()
    {
        if (IsConnected)
        {
            await _device.DisconnectAsync();
        }
        else
        {
            await _device.ConnectAsync();
        }
    }

    [RelayCommand]
    private void Close()
    {
        _closeAction?.Invoke();
    }

    [RelayCommand]
    private void AddLedAction()
    {
        if (SelectedLedIndex < 0)
            return;

        OpenActionWindowRequested?.Invoke(this, new OpenActionWindowEventArgs
        {
            LedIndex = SelectedLedIndex,
            ExistingAction = null
        });
    }

    [RelayCommand]
    private void EditLedAction(LedActionViewModel? action)
    {
        if (action == null || SelectedLedIndex < 0)
            return;

        OpenActionWindowRequested?.Invoke(this, new OpenActionWindowEventArgs
        {
            LedIndex = SelectedLedIndex,
            ExistingAction = action
        });
    }

    [RelayCommand]
    private void DeleteLedAction(LedActionViewModel? action)
    {
        if (action == null || _device.Profile == null)
            return;

        // Remove from profile
        bool removed = _device.Profile.RemoveLedOutputByIndex(action.Id);

        if (removed)
        {

            // Remove from UI list
            LedActions.Remove(action);

            // Save to database
            SaveProfile();
        }
    }

    [RelayCommand]
    private void DuplicateLedAction(LedActionViewModel? action)
    {
        if (action == null || !IsConnected)
            return;

        // Enter duplicate target selection mode
        ActionToDuplicate = action;
        IsSelectingDuplicateTarget = true;

        // Start blinking all LEDs except current to indicate selection mode
    }

    [RelayCommand]
    private void MoveActionUp(LedActionViewModel? action)
    {
        if (action == null || _device.Profile == null)
            return;

        int index = LedActions.IndexOf(action);
        if (index <= 0)
            return; // Already at top

        // Swap with previous action
        var previousAction = LedActions[index - 1];

        // Swap Order values in profile outputs
        var currentOutput = _device.Profile.Outputs.FirstOrDefault(o => o.Index == action.Id);
        var previousOutput = _device.Profile.Outputs.FirstOrDefault(o => o.Index == previousAction.Id);

        if (currentOutput?.Led != null && previousOutput?.Led != null)
        {
            int tempOrder = currentOutput.Led.Order;
            currentOutput.Led.Order = previousOutput.Led.Order;
            previousOutput.Led.Order = tempOrder;

            // Swap in UI list
            LedActions.Move(index, index - 1);

            // Update Order in ViewModels
            action.Order = currentOutput.Led.Order;
            previousAction.Order = previousOutput.Led.Order;

            // Mark profile as changed and save
            _device.Profile.Changed = true;
            _device.Profile.MarkAsNeedsUpdate();
            SaveProfile();

        }
    }

    [RelayCommand]
    private void MoveActionDown(LedActionViewModel? action)
    {
        if (action == null || _device.Profile == null)
            return;

        int index = LedActions.IndexOf(action);
        if (index < 0 || index >= LedActions.Count - 1)
            return; // Already at bottom

        // Swap with next action
        var nextAction = LedActions[index + 1];

        // Swap Order values in profile outputs
        var currentOutput = _device.Profile.Outputs.FirstOrDefault(o => o.Index == action.Id);
        var nextOutput = _device.Profile.Outputs.FirstOrDefault(o => o.Index == nextAction.Id);

        if (currentOutput?.Led != null && nextOutput?.Led != null)
        {
            int tempOrder = currentOutput.Led.Order;
            currentOutput.Led.Order = nextOutput.Led.Order;
            nextOutput.Led.Order = tempOrder;

            // Swap in UI list
            LedActions.Move(index, index + 1);

            // Update Order in ViewModels
            action.Order = currentOutput.Led.Order;
            nextAction.Order = nextOutput.Led.Order;

            // Mark profile as changed and save
            _device.Profile.Changed = true;
            _device.Profile.MarkAsNeedsUpdate();
            SaveProfile();

        }
    }

    [RelayCommand]
    private void CancelDuplicateAction()
    {
        IsSelectingDuplicateTarget = false;
        ActionToDuplicate = null;

        // Restore normal LED state
        if (SelectedLedIndex >= 0)
        {
            _device.SelectLed(SelectedLedIndex);
        }
        else
        {
            _device.UnselectAllLeds();
        }

    }

    private void CompleteDuplicateAction(int targetLedIndex)
    {
        if (ActionToDuplicate == null || _device.Profile == null)
            return;


        // Duplicate the action in the profile
        var newOutput = _device.Profile.DuplicateLedOutput(ActionToDuplicate.Id, targetLedIndex);

        if (newOutput != null)
        {

            // Save to database
            SaveProfile();
        }

        // Exit duplicate mode
        IsSelectingDuplicateTarget = false;
        ActionToDuplicate = null;

        // Select the target LED to show the duplicated action
        SelectLed(targetLedIndex);
    }

    public void StartBlinkingLed(int ledIndex)
    {
        if (ledIndex >= 0 && ledIndex < LED_COUNT)
        {
            _device.SelectLed(ledIndex);
        }
    }

    public void StopBlinkingLed()
    {
        _device.UnselectAllLeds();
    }

    [RelayCommand]
    private void TestAllLedsOn()
    {

        if (!IsConnected)
        {
            return;
        }

        // Set all LEDs to white (FFFFFF)
        for (int i = 0; i < LED_COUNT; i++)
        {
            _device.SetLedColor(i, 255, 255, 255);
            Leds[i].SetColor(255, 255, 255);
        }

        // Request full update to force sending all LEDs
        _device.RequestFullLedUpdate();
        LedsUpdated?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void TestAllLedsOff()
    {

        if (!IsConnected)
        {
            return;
        }

        // Set all LEDs to off (000000)
        _device.ClearAllLeds();
        for (int i = 0; i < LED_COUNT; i++)
        {
            Leds[i].SetColor(0, 0, 0);
        }

        // Request full update to force sending all LEDs
        _device.RequestFullLedUpdate();
        LedsUpdated?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Property Changed Handlers

    partial void OnBrightnessChanged(int value)
    {
        if (IsConnected)
        {
            _device.SetBrightness(value);
        }

        // Save to profile
        _device.Profile?.SetDataValue("led_brightness", value);

        // Update display text
        OnPropertyChanged(nameof(BrightnessDisplayText));
    }

    partial void OnIsConnectedChanged(bool value)
    {
        OnPropertyChanged(nameof(CanEditConfiguration));
    }

    partial void OnSelectedProfileChanged(DeviceProfile? value)
    {
        // Notify that SelectedProfileId computed property changed
        OnPropertyChanged(nameof(SelectedProfileId));

        if (value == null)
            return;

        if (_device.Profile?.Id != value.Id)
        {
            SwitchProfile(value.Id);
        }

        CanDeleteProfile = Profiles.Count > 1;
        CanRenameProfile = true;
    }

    #endregion

    #region Profile Management

    private async Task LoadProfiles()
    {
        Profiles.Clear();

        if (_profileService == null)
        {
            return;
        }


        var profilesList = await _profileService.LoadAllAsync(_device.ProductId);


        foreach (var profile in profilesList)
        {
            Profiles.Add(profile);
        }
    }

    private async void SwitchProfile(Guid profileId)
    {
        if (_device.Profile != null && _device.Profile.Id != profileId)
        {
            SaveCurrentProfileData();
        }

        bool newDefaultProfile = await _profileService.SetDefaultProfileAsync(profileId, _device.ConnId);

        if (newDefaultProfile)
        {
            _device.Profile = await _profileService.GetDefaultProfileAsync(_device.ProductId, _device.ConnId);
        }
        else
        {
            Console.Error.WriteLine($"[JJDB01 VM] Failed to set default profile with Id={profileId}");
        }
    }

    private void LoadProfileData()
    {
        if (_device.Profile == null)
            return;

        // Load brightness from profile
        var brightness = _device.Profile.GetDataValue<int>("led_brightness", 50);
        Brightness = brightness;

        // Load and apply to device
        if (IsConnected)
        {
            _device.SetBrightness(Brightness);
            _device.RequestFullLedUpdate();
        }

        // Reload actions for selected LED
        if (SelectedLedIndex >= 0)
        {
            LoadLedActions(SelectedLedIndex);
        }
    }

    private void SaveCurrentProfileData()
    {
        if (_device.Profile == null)
            return;

        _device.Profile.SetDataValue("led_brightness", Brightness);
        _device.Profile.Changed = true;
    }

    [RelayCommand]
    private async Task CreateProfile()
    {
        if (string.IsNullOrWhiteSpace(NewProfileName))
            return;

        var trimmedName = NewProfileName.Trim();

        if (Profiles.Any(x => x.Name == trimmedName))
            return;

        SaveProfile();

        bool profileCreated = await _profileService.SaveAsync(new DeviceProfile { Name = trimmedName });

        if (!profileCreated)
        {
            Console.Error.WriteLine($"[JJDB01 VM] Profile with name '{trimmedName}' already exists in database, cannot create duplicate");
            return;
        }

        await LoadProfiles();

        NewProfileName = string.Empty;
    }

    [RelayCommand]
    private void DeleteProfile()
    {
        if (SelectedProfile == null || !(Profiles.Count > 1))
            return;

        var profileToDelete = SelectedProfile;
        SelectedProfile = Profiles.FirstOrDefault(p => p.Id != profileToDelete.Id);
        Profiles.Remove(profileToDelete);
    }

    [RelayCommand]
    private void StartDuplicateProfile()
    {
        if (SelectedProfile == null)
            return;

        // Generate default name suggestion
        var baseName = SelectedProfile.Name;
        var newName = $"{baseName} (Cópia)";
        var counter = 1;

        while (Profiles.Any(x => x.Name == newName))
        {
            counter++;
            newName = $"{baseName} (Cópia {counter})";
        }

        DuplicateProfileName = newName;
        IsDuplicatingProfile = true;
    }

    [RelayCommand]
    private async Task ConfirmDuplicateProfile()
    {
        if (SelectedProfile == null || string.IsNullOrWhiteSpace(DuplicateProfileName))
            return;

        var newName = DuplicateProfileName.Trim();

        // Check if name already exists
        if (Profiles.Any(x => x.Name == newName))
        {
            Console.Error.WriteLine($"[JJDB01 VM] Profile with name '{newName}' already exists");
            return;
        }

        // Save current profile first
        SaveProfile();

        // Use _device.ProductId as the source of truth (it's always set correctly from device detection)
        // SelectedProfile.ProductId might be Guid.Empty if not loaded correctly
        var productId = _device.ProductId;


        // Create duplicate with all data from current profile
        var duplicatedProfile = new DeviceProfile
        {
            Name = newName,
            ProductId = productId, // Use device's ProductId (guaranteed to be correct)
            Data = SelectedProfile.Data?.DeepClone().AsObject()
        };

        // Copy outputs
        foreach (var output in SelectedProfile.Outputs)
        {
            duplicatedProfile.Outputs.Add(new ProfileOutput
            {
                Index = output.Index,
                Mode = output.Mode,
                Configuration = output.Configuration?.DeepClone().AsObject(),
                Led = output.Led != null ? new LedConfiguration
                {
                    Property = output.Led.Property,
                    PropertyName = output.Led.PropertyName,
                    Color = output.Led.Color,
                    Order = output.Led.Order,
                    Mode = output.Led.Mode,
                    ModeIfEnabled = output.Led.ModeIfEnabled,
                    Brightness = output.Led.Brightness,
                    BlinkSpeed = output.Led.BlinkSpeed,
                    PulseDelay = output.Led.PulseDelay,
                    LedsGrouped = output.Led.LedsGrouped?.ToList(),
                    ValueToActivate = output.Led.ValueToActivate,
                    Comparative = output.Led.Comparative,
                    VarType = output.Led.VarType
                } : null
            });
        }

        // Copy inputs
        foreach (var input in SelectedProfile.Inputs)
        {
            duplicatedProfile.Inputs.Add(new ProfileInput
            {
                Index = input.Index,
                Mode = input.Mode,
                Type = input.Type,
                Configuration = input.Configuration?.DeepClone().AsObject()
            });
        }


        var profileDuplicated = await _profileService.SaveAsync(duplicatedProfile);


        if (profileDuplicated)
        {
            await LoadProfiles();

            // Select the newly created profile
            SelectedProfile = Profiles.FirstOrDefault(p => p.Name == newName);
        }
        else
        {
            Console.Error.WriteLine($"[JJDB01 VM] Failed to duplicate profile '{SelectedProfile.Name}'");
        }

        // Reset duplicate mode
        IsDuplicatingProfile = false;
        DuplicateProfileName = string.Empty;
    }

    [RelayCommand]
    private void CancelDuplicateProfile()
    {
        IsDuplicatingProfile = false;
        DuplicateProfileName = string.Empty;
    }

    #endregion

    #region Cleanup

    public void Cleanup()
    {
        _device.DataUpdated -= OnDeviceDataUpdated;
        _device.PropertyChanged -= OnDevicePropertyChanged;
    }

    #endregion
}

#region Supporting ViewModels

public partial class LedViewModel : ObservableObject
{
    public int Index { get; }

    [ObservableProperty]
    private byte _red;

    [ObservableProperty]
    private byte _green;

    [ObservableProperty]
    private byte _blue;

    [ObservableProperty]
    private bool _isSelected;

    public Color Color => Color.FromRgb(Red, Green, Blue);
    public string HexColor => $"#{Red:X2}{Green:X2}{Blue:X2}";
    public bool IsOn => Red > 0 || Green > 0 || Blue > 0;

    public LedViewModel(int index, byte r = 0, byte g = 0, byte b = 0)
    {
        Index = index;
        _red = r;
        _green = g;
        _blue = b;
    }

    public void SetColor(byte r, byte g, byte b)
    {
        Red = r;
        Green = g;
        Blue = b;
        OnPropertyChanged(nameof(Color));
        OnPropertyChanged(nameof(HexColor));
        OnPropertyChanged(nameof(IsOn));
    }
}

public class LedActionViewModel
{
    public int Id { get; set; }
    public int Order { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public string Property { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int TargetLedIndex { get; set; } = -1;
    public int ModeIfEnabled { get; set; } = 1;
    public ComparativeType Comparative { get; set; } = ComparativeType.Equals;
    public string? ValueToActivate { get; set; }
}

public class OpenActionWindowEventArgs : EventArgs
{
    public int LedIndex { get; set; }
    public LedActionViewModel? ExistingAction { get; set; }
}

#endregion
