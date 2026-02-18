using HidSharp;
using JJManager.Core.Devices.Connections;
using JJManager.Core.Interfaces.Repositories;
using JJManager.Core.Others;
using System.Text;

using JJHidDevice = JJManager.Core.Devices.Connections.HidDevice;
using HidSharpDevice = HidSharp.HidDevice;
using JJManager.Core.Devices.Abstractions;

namespace JJManager.Core.Devices.Generic;

/// <summary>
/// Generic HID device wrapper for JohnJohn3D devices
/// Used for device discovery before specific device class is instantiated
/// </summary>
public class GenericHidDevice : JJHidDevice
{
    protected HidStream? _stream;

    public GenericHidDevice(HidSharpDevice hidDevice, IProductRepository? productRepository = null)
        : base(hidDevice, productRepository)
    {
        _hidSharpDevice = hidDevice;
        _productName = _hidSharpDevice.GetProductName();
        _connId = HashHelper.GetDeterministicHash(_hidSharpDevice.DevicePath);
        _connectionType = DeviceConnectionType.HID;
        _supportedCommands = new HashSet<ushort>
        {
            0x00FF
        };
    }

    public override async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (IsConnected)
                return true;

            _stream = _hidSharpDevice?.Open();

            if (_stream != null)
            {
                OnConnectionStateChanged(DeviceConnectionState.Connected);

                // Try to get firmware version
                _firmwareVersion = await GetFirmwareVersionAsync(cancellationToken);

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error connecting to {ProductName}: {ex.Message}");
            OnConnectionStateChanged(DeviceConnectionState.Disconnected);
            return false;
        }
    }

    public override Task<bool> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _stream?.Close();
            _stream?.Dispose();
            _stream = null;
            OnConnectionStateChanged(DeviceConnectionState.Disconnected);
            return Task.FromResult(true);
        }
        catch
        {
            OnConnectionStateChanged(DeviceConnectionState.Disconnected);
            return Task.FromResult(false);
        }
    }

    public override async Task<Version?> GetFirmwareVersionAsync(CancellationToken cancellationToken = default)
    {
        // For generic devices, return a placeholder version
        // Each specific device class will implement its own protocol

        if (_supportedCommands.Contains(0x00FF))
        {
            for (int attempt = 0; attempt < 2; attempt++)
            {
                try
                {
                    var message = new HidMessage(0x00FF, (byte)0x00);  // INFO_TYPE: 0x00 = Firmware Version
                    var (success, response) = await RequestAsync(message, forceConnection: true, timeoutMs: 1000, cancellationToken: cancellationToken);

                    if (success && response.Length > 0)
                    {
                        string versionString = Encoding.UTF8.GetString(response).Trim('\0').Trim();
                        var version = TranslateVersion(versionString);
                        if (version != null)
                            return version;
                    }
                }
                catch
                {
                    // Continue to next attempt
                }
            }
        }

        return null;
    }

    protected override Task DataLoopAsync(CancellationToken cancellationToken)
    {
        // Generic device doesn't have a send/receive loop
        // Specific device classes implement this
        return Task.CompletedTask;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _stream?.Close();
            _stream?.Dispose();
            _stream = null;
        }
        base.Dispose(disposing);
    }

    /// <summary>
    /// Icon name for generic devices
    /// </summary>
    public override string IconName => "Chip";

    /// <summary>
    /// Device initials for generic devices
    /// </summary>
    public override string DeviceInitials => "JJ";

    /// <summary>
    /// Device class name based on product name
    /// </summary>
    public override string DeviceClassName => GetDeviceClassName();

    /// <summary>
    /// Get the device class name based on product name
    /// </summary>
    public string GetDeviceClassName()
    {
        return ProductName switch
        {
            "Streamdeck JJSD-01" => "JJSD01",
            "Mixer de Áudio JJM-01" => "JJM01",
            "ButtonBox JJB-01" => "JJB01",
            "ButtonBox JJB-01 V2" => "JJB01_V2",
            "ButtonBox JJBP-06" => "JJBP06",
            "ButtonBox JJB-999" => "JJB999",
            "Hub ARGB JJHL-01" => "JJHL01",
            "Hub ARGB JJHL-01 Plus" => "JJHL01",
            "Hub RGB JJHL-02" => "JJHL02",
            "Hub RGB JJHL-02 Plus" => "JJHL02",
            "Dashboard JJDB-01" => "JJDB01",
            "LoadCell JJLC-01" => "JJLC01",
            "ButtonBox JJB-Slim Type A" => "JJBSlim_A",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Get the image resource name for this device
    /// </summary>
    public string GetImageResourceName()
    {
        var className = GetDeviceClassName();
        return $"avares://JJManager/Assets/Devices/{className}.png";
    }
}
