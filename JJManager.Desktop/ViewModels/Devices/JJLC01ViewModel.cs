using Avalonia.Data.Converters;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JJManager.Core.Devices;
using JJManager.Core.Devices.JJLC01;
using JJManager.Core.Profile;
using JJManager.Desktop.Services;
using System;
using System.Collections.ObjectModel;
using System.Globalization;

namespace JJManager.Desktop.ViewModels.Devices;

public partial class JJLC01ViewModel : ViewModelBase
{
    private readonly JJLC01Device _device;
    private readonly Action _closeAction;

    /// <summary>
    /// Special profile name that always reads data from device
    /// </summary>
    private const string DeviceDataProfileName = "Dados do Dispositivo";

    /// <summary>
    /// Default ADC curve (all zeros - no data from device)
    /// </summary>
    private static readonly double[] DefaultAdcCurve = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    /// <summary>
    /// Default fine offset (center value)
    /// </summary>
    private const int DefaultFineOffset = 127;

    public string IconName { get => _device.IconName; }

    #region Observable Properties - Real-time Data

    [ObservableProperty]
    private int _potPercent;

    [ObservableProperty]
    private float _kgPressed;

    [ObservableProperty]
    private short _rawValue;

    [ObservableProperty]
    private int _fineOffset = DefaultFineOffset;

    [ObservableProperty]
    private double[] _adcCurve = new double[11];

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private int _selectedPointIndex = -1;

    [ObservableProperty]
    private string _selectedPointLabel = string.Empty;

    [ObservableProperty]
    private double _selectedPointValue;

    [ObservableProperty]
    private bool _canEditPoint;

    /// <summary>
    /// Indicates if the current profile is the special "Device Data" profile
    /// </summary>
    public bool IsDeviceDataProfile => SelectedProfile == DeviceDataProfileName;

    /// <summary>
    /// Indicates if the fine offset and ADC controls can be edited
    /// (only when connected AND not using Device Data profile)
    /// </summary>
    public bool CanEditConfiguration => IsConnected && !IsDeviceDataProfile;

    #endregion

    #region Observable Properties - Profile Management

    [ObservableProperty]
    private ObservableCollection<string> _profiles = new();

    [ObservableProperty]
    private string? _selectedProfile;

    [ObservableProperty]
    private string _newProfileName = string.Empty;

    [ObservableProperty]
    private bool _canDeleteProfile;

    [ObservableProperty]
    private bool _canRenameProfile;

    #endregion

    #region Computed Properties

    private static ILocalizationService Localization => LocalizationService.Instance;

    public string ConnectionStatus => IsConnected
        ? Localization.GetString("Device_Connected")
        : Localization.GetString("Device_Disconnected");

    public string ConnectionTooltip => IsConnected
        ? Localization.GetString("Device_ConnectedTooltip")
        : Localization.GetString("Device_DisconnectedTooltip");

    public string ConnectionButtonText => IsConnected
        ? Localization.GetString("Devices_Btn_Disconnect")
        : Localization.GetString("Devices_Btn_Connect");

    public int FineOffsetDisplay => FineOffset - DefaultFineOffset;

    public string FineOffsetDisplayText => Localization.GetString("JJLC01_FineAdjustmentValue", FineOffsetDisplay);

    public string RawValueDisplayText => Localization.GetString("JJLC01_CurrentValue", RawValue);

    #endregion

    #region Events

    public event EventHandler? ChartUpdateRequested;
    public event EventHandler<int>? PointSelected;

    #endregion

    #region Converters

    public static IValueConverter BoolToColorConverter { get; } = new FuncValueConverter<bool, Color>(
        value => value ? Color.Parse("#4CAF50") : Color.Parse("#757575"));

    #endregion

    #region Constructor

    public JJLC01ViewModel(JJLC01Device device, Action closeAction)
    {
        _device = device;
        _closeAction = closeAction;
        _isConnected = device.IsConnected;

        // Initialize ADC curve from device
        Array.Copy(device.AdcCurve, _adcCurve, 11);

        // Initialize localized strings
        _selectedPointLabel = Localization.GetString("JJLC01_SelectedPoint_None");

        // Subscribe to device events
        device.DataUpdated += OnDeviceDataUpdated;
        device.PropertyChanged += OnDevicePropertyChanged;

        // Initialize profiles
        LoadProfiles();
    }

    #endregion

    #region Event Handlers

    private void OnDeviceDataUpdated(object? sender, JJLC01DataEventArgs e)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            PotPercent = e.PotPercent;
            KgPressed = e.KgPressed;
            RawValue = e.RawValue;

            // Update ADC and FineOffset from device when:
            // 1. It's initial data (first connection), OR
            // 2. The "Device Data" profile is selected (always sync with device)
            if (e.IsInitialData || IsDeviceDataProfile)
            {
                // Temporarily disable sending to device while we update from device
                _updatingFromDevice = true;
                try
                {
                    FineOffset = e.FineOffset;
                    AdcCurve = e.AdcCurve;
                    ChartUpdateRequested?.Invoke(this, EventArgs.Empty);
                }
                finally
                {
                    _updatingFromDevice = false;
                }
            }
        });
    }

    /// <summary>
    /// Flag to prevent sending data back to device when updating from device
    /// </summary>
    private bool _updatingFromDevice = false;

    /// <summary>
    /// Track the previous profile for detecting profile changes
    /// </summary>
    private string? _previousProfile = null;

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

                // When disconnected and using "Device Data" profile, reset to defaults
                if (!IsConnected && IsDeviceDataProfile)
                {
                    ResetToDefaults();
                }
            });
        }
    }

    /// <summary>
    /// Reset all values to defaults (used when "Device Data" profile is disconnected)
    /// </summary>
    private void ResetToDefaults()
    {
        _updatingFromDevice = true;
        try
        {
            PotPercent = 0;
            KgPressed = 0;
            RawValue = 0;
            FineOffset = DefaultFineOffset;
            AdcCurve = (double[])DefaultAdcCurve.Clone();
            ChartUpdateRequested?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _updatingFromDevice = false;
        }
    }

    #endregion

    #region Public Methods

    public void SelectPoint(int index)
    {
        if (index < 0 || index > 10) return;

        SelectedPointIndex = index;
        SelectedPointLabel = $"{index * 10}%";
        SelectedPointValue = AdcCurve[index];
        CanEditPoint = CanEditConfiguration;

        PointSelected?.Invoke(this, index);
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void IncreasePoint()
    {
        if (SelectedPointIndex < 0) return;

        double maxValue = SelectedPointIndex < 10 ? AdcCurve[SelectedPointIndex + 1] : 100;
        double newValue = Math.Min(SelectedPointValue + 0.5, maxValue);

        UpdatePointValue(newValue);
    }

    [RelayCommand]
    private void DecreasePoint()
    {
        if (SelectedPointIndex < 0) return;

        double minValue = SelectedPointIndex > 0 ? AdcCurve[SelectedPointIndex - 1] : 0;
        double newValue = Math.Max(SelectedPointValue - 0.5, minValue);

        UpdatePointValue(newValue);
    }

    private void UpdatePointValue(double newValue)
    {
        SelectedPointValue = Math.Round(newValue, 1);

        var newCurve = (double[])AdcCurve.Clone();
        newCurve[SelectedPointIndex] = SelectedPointValue;
        AdcCurve = newCurve;

        // Only send to device if connected and not updating from device
        if (IsConnected && !_updatingFromDevice)
        {
            _device.SetAdcCurve(AdcCurve);
        }
        ChartUpdateRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Calibrate()
    {
        _device.RequestCalibration();
    }

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

    #endregion

    #region Property Changed Handlers

    partial void OnFineOffsetChanged(int value)
    {
        // Only send to device if connected and not updating from device
        if (IsConnected && !_updatingFromDevice)
        {
            _device.SetFineOffset(value);
        }
        OnPropertyChanged(nameof(FineOffsetDisplay));
        OnPropertyChanged(nameof(FineOffsetDisplayText));
    }

    partial void OnRawValueChanged(short value)
    {
        OnPropertyChanged(nameof(RawValueDisplayText));
    }

    partial void OnIsConnectedChanged(bool value)
    {
        CanEditPoint = value && SelectedPointIndex >= 0 && !IsDeviceDataProfile;
        OnPropertyChanged(nameof(CanEditConfiguration));
    }

    //partial void OnSelectedProfileChanged(Guid? value)
    //{
    //    // Save previous profile for use in SwitchProfile
    //    var previousProfile = _previousProfile;
    //    _previousProfile = value;

    //    if (value != null)
    //    {
    //        if (value == DeviceDataProfileName)
    //        {
    //            // Device Data profile - if connected, load from device; otherwise reset
    //            if (IsConnected)
    //            {
    //                // Request fresh data from device
    //                _updatingFromDevice = true;
    //                try
    //                {
    //                    FineOffset = _device.FineOffset;
    //                    AdcCurve = (double[])_device.AdcCurve.Clone();
    //                    ChartUpdateRequested?.Invoke(this, EventArgs.Empty);
    //                }
    //                finally
    //                {
    //                    _updatingFromDevice = false;
    //                }
    //            }
    //            else
    //            {
    //                ResetToDefaults();
    //            }
    //        }
    //        else
    //        {
    //            // Switch to selected profile and load/send its data
    //            SwitchProfile(value, previousProfile);
    //        }
    //    }

    //    // Update can delete/rename based on selection
    //    // Device Data and Perfil Padrão cannot be deleted
    //    CanDeleteProfile = value != null &&
    //                      value != "Perfil Padrão" &&
    //                      value != DeviceDataProfileName &&
    //                      Profiles.Count > 2;  // At least Device Data + Perfil Padrão + 1 custom
    //    CanRenameProfile = value != null && value != DeviceDataProfileName;

    //    // Update CanEditPoint based on new profile
    //    CanEditPoint = IsConnected && SelectedPointIndex >= 0 && !IsDeviceDataProfile;

    //    OnPropertyChanged(nameof(IsDeviceDataProfile));
    //    OnPropertyChanged(nameof(CanEditConfiguration));
    //}

    #endregion

    #region Profile Management

    private void LoadProfiles()
    {
        Profiles.Clear();

        // Add "Device Data" profile first - special profile that always reads from device
        Profiles.Add(DeviceDataProfileName);

        // Add default profile
        Profiles.Add("Perfil Padrão");

        // Load additional profiles from device or storage
        // For now, just use the current profile name if different
        if (_device.Profile != null &&
            _device.Profile.Name != "Perfil Padrão" &&
            _device.Profile.Name != DeviceDataProfileName)
        {
            if (!Profiles.Contains(_device.Profile.Name))
            {
                Profiles.Add(_device.Profile.Name);
            }
        }

        // Select "Device Data" profile by default (always starts reading from device)
        SelectedProfile = DeviceDataProfileName;
    }

    private void SwitchProfile(string profileName, string? previousProfileName)
    {
        // Don't save data from Device Data profile (it's transient)
        bool wasDeviceDataProfile = previousProfileName == null || previousProfileName == DeviceDataProfileName;

        if (_device.Profile != null && _device.Profile.Name != profileName && !wasDeviceDataProfile)
        {
            // Save current profile data before switching (only if not coming from Device Data)
            SaveCurrentProfileData();
        }

        // Create or load the new profile
        _device.Profile = new DeviceProfile(profileName, _device.ProductId);

        // Load profile data into ViewModel and send to device
        LoadProfileData();
    }

    /// <summary>
    /// Load data from current profile into ViewModel and send to device
    /// </summary>
    private void LoadProfileData()
    {
        if (_device.Profile == null) return;

        _updatingFromDevice = true;
        try
        {
            // Load FineOffset from profile (or use default)
            if (_device.Profile.TryGetDataValue<int>("fine_offset", out var fineOffset))
            {
                FineOffset = fineOffset;
            }
            else
            {
                FineOffset = DefaultFineOffset;
            }

            // Load ADC curve from profile (or use linear default for profiles)
            if (_device.Profile.TryGetDataValue<double[]>("adc_curve", out var adcCurve) && adcCurve?.Length == 11)
            {
                AdcCurve = adcCurve;
            }
            else
            {
                // For profiles, use linear curve as default (not zeros)
                AdcCurve = new double[] { 0, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40 };
            }

            ChartUpdateRequested?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _updatingFromDevice = false;
        }

        // Send the loaded data to device if connected
        if (IsConnected)
        {
            _device.SetFineOffset(FineOffset);
            _device.SetAdcCurve(AdcCurve);
        }
    }

    private void SaveCurrentProfileData()
    {
        if (_device.Profile == null) return;

        // Save FineOffset and ADC curve to profile
        _device.Profile.SetDataValue("fine_offset", FineOffset);
        _device.Profile.SetDataValue("adc_curve", AdcCurve);
        _device.Profile.Changed = true;
    }

    [RelayCommand]
    private void CreateProfile()
    {
        if (string.IsNullOrWhiteSpace(NewProfileName))
            return;

        var trimmedName = NewProfileName.Trim();

        // Check if profile already exists
        if (Profiles.Contains(trimmedName))
            return;

        // Save current profile first
        SaveCurrentProfileData();

        // Add new profile
        Profiles.Add(trimmedName);

        // Switch to new profile
        SelectedProfile = trimmedName;

        // Clear the input
        NewProfileName = string.Empty;
    }

    [RelayCommand]
    private void DeleteProfile()
    {
        if (SelectedProfile == null ||
            SelectedProfile == "Perfil Padrão" ||
            SelectedProfile == DeviceDataProfileName)
            return;

        var profileToDelete = SelectedProfile;

        // Switch to Device Data profile first
        SelectedProfile = DeviceDataProfileName;

        // Remove the profile
        Profiles.Remove(profileToDelete);
    }

    [RelayCommand]
    private void DuplicateProfile()
    {
        if (SelectedProfile == null)
            return;

        // Generate new name
        var baseName = SelectedProfile;
        var newName = $"{baseName} (Cópia)";
        var counter = 1;

        while (Profiles.Contains(newName))
        {
            counter++;
            newName = $"{baseName} (Cópia {counter})";
        }

        // Save current profile data
        SaveCurrentProfileData();

        // Add duplicated profile
        Profiles.Add(newName);

        // Switch to duplicated profile
        SelectedProfile = newName;
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
