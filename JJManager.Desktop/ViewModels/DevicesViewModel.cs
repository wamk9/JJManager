using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JJManager.Core.Devices;
using JJManager.Core.Interfaces.Services;
using JJManager.Core.Profile;
using JJManager.Desktop.Services;
using Material.Icons;
using System.Collections.ObjectModel;
using System.Timers;

namespace JJManager.Desktop.ViewModels;

public partial class DevicesViewModel : ViewModelBase, IDisposable
{
    private readonly IDeviceService _deviceService;
    private readonly System.Timers.Timer _autoRefreshTimer;
    private bool _disposed;

    [ObservableProperty]
    private ObservableCollection<DeviceItemViewModel> _devices = new();

    [ObservableProperty]
    private DeviceItemViewModel? _selectedDevice;

    [ObservableProperty]
    private bool _isRefreshing;

    public DevicesViewModel(IDeviceService deviceService)
    {
        _deviceService = deviceService;
        _deviceService.DevicesChanged += OnDevicesChanged;

        // Auto-refresh every 2 seconds (but don't start until explicitly called)
        _autoRefreshTimer = new System.Timers.Timer(2000);
        _autoRefreshTimer.Elapsed += OnAutoRefreshTimerElapsed;
        _autoRefreshTimer.AutoReset = true;
        // Note: Timer will be started by StartAutoRefresh() after window is shown
    }

    public void StartAutoRefresh()
    {
        _autoRefreshTimer.Start();
        _ = RefreshAsync();
    }

    private async void OnAutoRefreshTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (!IsRefreshing)
        {
            await RefreshAsync();
        }
    }

    private void OnDevicesChanged(object? sender, EventArgs e)
    {
        RefreshDeviceList();
    }

    private void RefreshDeviceList()
    {
        // Use Dispatcher to update UI from background thread
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            Devices.Clear();
            foreach (var device in _deviceService.AvailableDevices)
            {
                Devices.Add(new DeviceItemViewModel(device, _deviceService));
            }
        });
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsRefreshing) return;

        IsRefreshing = true;
        try
        {
            await _deviceService.RefreshDevicesAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task ConnectSelectedAsync()
    {
        if (SelectedDevice != null)
        {
            await SelectedDevice.ConnectAsync();
        }
    }

    [RelayCommand]
    private async Task DisconnectSelectedAsync()
    {
        if (SelectedDevice != null)
        {
            await SelectedDevice.DisconnectAsync();
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _autoRefreshTimer.Stop();
            _autoRefreshTimer.Dispose();
            _deviceService.DevicesChanged -= OnDevicesChanged;
            _disposed = true;
        }
    }
}

public partial class DeviceItemViewModel : ViewModelBase
{
    private readonly JJDevice _device;
    private readonly IDeviceService _deviceService;

    public string ProductName => _device.ProductName;
    public string ConnId => _device.ConnId;
    public string FirmwareVersion => _device.FirmwareVersion?.ToString() ?? "N/A";

    /// <summary>
    /// Gets the device class name (e.g., "JJDB01", "JJM01")
    /// </summary>
    public string DeviceClassName => _device.DeviceClassName;

    /// <summary>
    /// Gets the image resource path for the device
    /// </summary>
    public string ImageResourcePath => $"avares://JJManager/Assets/Devices/{DeviceClassName}/DeviceItem.png";

    /// <summary>
    /// Gets the Material Icon kind based on device type (from device class)
    /// </summary>
    public MaterialIconKind DeviceIconKind => GetDeviceIconKind();

    private MaterialIconKind GetDeviceIconKind()
    {
        // Get icon name from device class and convert to MaterialIconKind
        var iconName = _device.IconName;
        if (Enum.TryParse<MaterialIconKind>(iconName, out var iconKind))
        {
            return iconKind;
        }
        return MaterialIconKind.Chip;
    }

    /// <summary>
    /// Gets initials for devices without images (from device class)
    /// </summary>
    public string DeviceInitials => _device.DeviceInitials;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private bool _isConnecting;

    [ObservableProperty]
    private bool _autoConnect;

    public bool HasImage
    {
        get
        {
            if (string.IsNullOrWhiteSpace(DeviceClassName))
                return false;

            var uri = new Uri(
                $"avares://JJManager/Assets/Devices/{DeviceClassName}/DeviceItem.png"
            );

            return Avalonia.Platform.AssetLoader.Exists(uri);
        }
    }

    public DeviceItemViewModel(JJDevice device, IDeviceService deviceService)
    {
        _device = device;
        _deviceService = deviceService;
        _isConnected = device.IsConnected;
        _autoConnect = device.AutoConnect;

        device.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(JJDevice.IsConnected))
            {
                IsConnected = device.IsConnected;
                IsConnecting = false;
            }
        };
    }

    [RelayCommand]
    public async Task ConnectAsync()
    {
        IsConnecting = true;

        try
        {
            // Use device service to connect (loads profile from database automatically)
            await _deviceService.ConnectDeviceAsync(_device);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DeviceItemViewModel] ConnectAsync error: {ex.Message}");
        }
        finally
        {
            IsConnecting = false;
        }
    }

    [RelayCommand]
    public async Task DisconnectAsync()
    {
        await _device.DisconnectAsync();
    }

    [RelayCommand]
    public async Task OpenSettings()
    {
        // Use factory to dynamically open device-specific settings window as modal
        // Services are resolved automatically via DI (ActivatorUtilities)
        if (!await DeviceWindowFactory.OpenDeviceWindowAsync(_device))
        {
            System.Diagnostics.Debug.WriteLine($"No settings window available for device class: {_device.DeviceClassName}");
        }
    }

    /// <summary>
    /// Checks if this device has a configuration window available
    /// </summary>
    public bool HasSettingsWindow => DeviceWindowFactory.HasDeviceWindow(_device.DeviceClassName);

    partial void OnAutoConnectChanged(bool value)
    {
        _device.AutoConnect = value;
    }
}
