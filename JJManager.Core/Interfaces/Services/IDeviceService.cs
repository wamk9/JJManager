using JJManager.Core.Devices;
using System.Collections.ObjectModel;

namespace JJManager.Core.Interfaces.Services;

/// <summary>
/// Service for managing JohnJohn3D devices
/// </summary>
public interface IDeviceService
{
    /// <summary>
    /// All available (detected) devices
    /// </summary>
    ObservableCollection<JJDevice> AvailableDevices { get; }

    /// <summary>
    /// Currently connected devices
    /// </summary>
    ObservableCollection<JJDevice> ConnectedDevices { get; }

    /// <summary>
    /// Event raised when the device list changes
    /// </summary>
    event EventHandler? DevicesChanged;

    /// <summary>
    /// Start device discovery
    /// </summary>
    Task StartDiscoveryAsync();

    /// <summary>
    /// Stop device discovery
    /// </summary>
    Task StopDiscoveryAsync();

    /// <summary>
    /// Refresh the device list
    /// </summary>
    Task RefreshDevicesAsync();

    /// <summary>
    /// Connect to a device
    /// </summary>
    Task<bool> ConnectDeviceAsync(JJDevice device);

    /// <summary>
    /// Disconnect from a device
    /// </summary>
    Task<bool> DisconnectDeviceAsync(JJDevice device);

    /// <summary>
    /// Auto-connect devices marked for auto-connection
    /// </summary>
    Task AutoConnectDevicesAsync();
}
