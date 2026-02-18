using HidSharp;
using JJManager.Core.Devices;
using JJManager.Core.Devices.Abstractions;
using JJManager.Core.Devices.Factories;
using JJManager.Core.Interfaces.Repositories;
using JJManager.Core.Interfaces.Services;
using JJManager.Core.Others;
using JJManager.Core.Profile;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Timers;
using Timer = System.Timers.Timer;

namespace JJManager.Infrastructure.Services;

/// <summary>
/// Service for managing JohnJohn3D devices
/// </summary>
public class DeviceService : IDeviceService, IDisposable
{
    private readonly Timer _discoveryTimer;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _isDiscovering;
    private bool _isDisposed;
    private readonly IDeviceProbe<HidSharp.HidDevice> _hidDeviceProbe;
    private readonly IProductRepository _productRepository;

    // Known product names (filter by name only)
    private static readonly string[] KnownHidDevices =
    {
        "Streamdeck JJSD-01",
        "Mixer de Áudio JJM-01",
        "ButtonBox JJB-01 V2",
        "ButtonBox JJBP-06",
        "ButtonBox JJB-999",
        "Hub ARGB JJHL-01",
        "Hub ARGB JJHL-01 Plus",
        "Hub RGB JJHL-02",
        "Hub RGB JJHL-02 Plus",
        "Dashboard JJDB-01",
        "LoadCell JJLC-01",
        "ButtonBox JJB-Slim Type A"
    };

    public ObservableCollection<JJDevice> AvailableDevices { get; } = new();
    public ObservableCollection<JJDevice> ConnectedDevices { get; } = new();

    public event EventHandler? DevicesChanged;

    private readonly IDeviceProfileService _deviceProfileService;

    public DeviceService(
        IDeviceProbe<HidSharp.HidDevice> deviceProbe,
        IProductRepository productRepository,
        IDeviceProfileService deviceProfileService)
    {
        _hidDeviceProbe = deviceProbe;
        _productRepository = productRepository;
        _deviceProfileService = deviceProfileService;

        _discoveryTimer = new Timer(2000);
        _discoveryTimer.Elapsed += OnDiscoveryTimerElapsed;
    }

    public Task StartDiscoveryAsync()
    {
        if (_isDiscovering)
            return Task.CompletedTask;

        _isDiscovering = true;
        _discoveryTimer.Start();

        // Start first refresh in background (don't await)
        _ = Task.Run(async () =>
        {
            try
            {
                await RefreshDevicesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Initial refresh error: {ex.Message}");
            }
        });

        return Task.CompletedTask;
    }

    public Task StopDiscoveryAsync()
    {
        _isDiscovering = false;
        _discoveryTimer.Stop();
        return Task.CompletedTask;
    }

    private async void OnDiscoveryTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        await RefreshDevicesAsync();
    }

    public async Task RefreshDevicesAsync()
    {
        if (!_isDiscovering)
            return;

        // Prevent concurrent refreshes (race condition fix)
        // Use SemaphoreSlim instead of Monitor for async-safe locking
        if (!await _lock.WaitAsync(0))
            return;

        try
        {
            // Get existing device paths to skip them during filtering
            var existingDevicePaths = AvailableDevices
                .OfType<Core.Devices.Connections.HidDevice>()
                .Select(d => d.DevicePath)
                .ToHashSet();

            var hidDevices = GetHidDevices(existingDevicePaths);
            var existingConnIds = AvailableDevices.Select(d => d.ConnId).ToHashSet();
            var foundConnIds = new HashSet<string>();

            bool devicesAdded = false;

            foreach (var hidDevice in hidDevices)
            {
                var connId = HashHelper.GetDeterministicHash(hidDevice.DevicePath);
                foundConnIds.Add(connId);

                if (!existingConnIds.Contains(connId))
                {
                    var device = await CreateDeviceFromHidAsync(hidDevice);
                    if (device != null)
                    {
                        AvailableDevices.Add(device);
                        devicesAdded = true;
                        Debug.WriteLine($"Device added: {device.ProductName}");
                    }
                }
            }

            var devicesToRemove = AvailableDevices
                .Where(d => !foundConnIds.Contains(d?.ConnId ?? string.Empty))
                .ToList();

            foreach (var device in devicesToRemove)
            {
                if (ConnectedDevices.Contains(device))
                    ConnectedDevices.Remove(device);

                AvailableDevices.Remove(device);
                device?.Dispose();
            }

            if (devicesAdded || devicesToRemove.Any())
                DevicesChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"RefreshDevices error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    private List<HidSharp.HidDevice> GetHidDevices(HashSet<string>? skipDevicePaths = null)
    {
        try
        {
            // Get ALL HID devices and filter by product name only
            var allDevices = DeviceList.Local.GetHidDevices().ToList();

            // Filter to known JohnJohn3D devices by name
            allDevices.RemoveAll(device =>
            {
                try
                {
                    var productName = device.GetProductName();
                    return !KnownHidDevices.Contains(productName);
                }
                catch
                {
                    return true;
                }
            });

            // Log device paths (without opening the device)
            foreach (var device in allDevices)
            {
                try
                {
                    // Skip opening devices that are already known (to avoid conflicts with DataLoop)
                    if (skipDevicePaths?.Contains(device.DevicePath) == true)
                    {
                        Debug.WriteLine($"HID Device: {device.GetProductName()} | Path: {device.DevicePath} | SKIPPED (already known)");
                        continue;
                    }

                    var reportDescriptor = device.GetReportDescriptor();
                    var reports = reportDescriptor.Reports;
                    var reportId = reports.Any() ? reports[0].ReportID : -1;
                    Debug.WriteLine($"HID Device: {device.GetProductName()} | Path: {device.DevicePath} | ReportID: {reportId:X2}");
                }
                catch (Exception)
                {

                }
            }

            // Remove duplicate HID interfaces (devices with multiple endpoints)
            allDevices.RemoveAll(device =>
            {
                try
                {
                    // Skip checking devices that are already known
                    if (skipDevicePaths?.Contains(device.DevicePath) == true)
                        return false;  // Keep known devices

                    var reportDescriptor = device.GetReportDescriptor();
                    var reports = reportDescriptor.Reports;

                    // Remove Joystick HID interface (ButtonBoxes have both HID and Joystick)
                    if (reports.Any() && reports[0].ReportID == 0xFF)
                        return true;

                    // Remove keyboard HID for JJSD-01
                    if (device.DevicePath.Contains("&mi_03#") &&
                        reports.Any() && reports[0].ReportID == 0x02)
                        return true;

                    return false;
                }
                catch
                {
                    return true;
                }
            });

            // Deduplicate by keeping only one HID interface per physical device
            // Group by VID/PID and keep only the first valid interface (lowest mi_XX number)
            var seenVidPid = new Dictionary<string, HidSharp.HidDevice>();
            allDevices.RemoveAll(device =>
            {
                try
                {
                    var vidPid = $"{device.VendorID:X4}_{device.ProductID:X4}";

                    if (!seenVidPid.ContainsKey(vidPid))
                    {
                        // First interface for this VID/PID - keep it
                        seenVidPid[vidPid] = device;
                        return false;
                    }
                    else
                    {
                        // Already have an interface for this VID/PID
                        // Compare interface numbers (mi_XX) and keep the lower one
                        var existingDevice = seenVidPid[vidPid];
                        int existingMi = ExtractInterfaceNumber(existingDevice.DevicePath);
                        int currentMi = ExtractInterfaceNumber(device.DevicePath);

                        if (currentMi < existingMi && currentMi >= 0)
                        {
                            // Current device has lower interface number - swap
                            Debug.WriteLine($"Replacing interface: {existingDevice.GetProductName()} mi_{existingMi:X2} with mi_{currentMi:X2}");
                            seenVidPid[vidPid] = device;
                            return false;  // Keep current, the old one will be removed in a separate pass
                        }
                        else
                        {
                            Debug.WriteLine($"Removing duplicate interface: {device.GetProductName()} ({vidPid}) mi_{currentMi:X2} - Path: {device.DevicePath}");
                            return true;  // Remove current, keep existing
                        }
                    }
                }
                catch
                {
                    return true;
                }
            });

            // Final pass: remove any devices that were replaced with lower interface numbers
            var validDevices = new HashSet<HidSharp.HidDevice>(seenVidPid.Values);
            allDevices.RemoveAll(device => !validDevices.Contains(device));

            return allDevices;
        }
        catch
        {
            return new List<HidSharp.HidDevice>();
        }
    }

    /// <summary>
    /// Extract interface number (mi_XX) from device path
    /// </summary>
    private static int ExtractInterfaceNumber(string devicePath)
    {
        try
        {
            // Windows HID device path format: \\?\hid#vid_XXXX&pid_XXXX&mi_XX#...
            var lowerPath = devicePath.ToLowerInvariant();
            var miIndex = lowerPath.IndexOf("&mi_");
            if (miIndex >= 0)
            {
                var miStr = lowerPath.Substring(miIndex + 4, 2);
                if (int.TryParse(miStr, System.Globalization.NumberStyles.HexNumber, null, out int mi))
                {
                    return mi;
                }
            }

            // Linux/macOS might have different patterns - return 0 as default
            return 0;
        }
        catch
        {
            return -1;
        }
    }

    private async Task<JJDevice?> CreateDeviceFromHidAsync(HidSharp.HidDevice hidDevice)
    {
        try
        {
            var productName = hidDevice.GetProductName();
            Debug.WriteLine($"Probing device: {productName}");

            var probeResult = await _hidDeviceProbe.ProbeAsync(hidDevice);

            Debug.WriteLine($"Probe result for {productName}: Success={probeResult.Success}, Kind={probeResult.DeviceKind}");

            if (!probeResult.Success)
            {
                Debug.WriteLine($"Probe failed for {productName}, creating as GenericHidDevice");
                // Even if probe fails, create a generic device so user can see it
                var genericDevice = new JJManager.Core.Devices.Generic.GenericHidDevice(hidDevice, _productRepository);
                await genericDevice.LoadProductIdAsync();
                return genericDevice;
            }

            // Use async factory to create device and load ProductId
            return await JJDeviceFactory.CreateAsync(hidDevice, probeResult, _productRepository);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating device: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> ConnectDeviceAsync(JJDevice device)
    {
        try
        {
            // Load profile from database before connecting (so DataLoop has access to outputs)
            device.Profile = await _deviceProfileService.GetDefaultProfileAsync(device.ProductId, device.ConnId);

            var result = await device.ConnectAsync();
            if (result && !ConnectedDevices.Contains(device))
            {
                ConnectedDevices.Add(device);
                DevicesChanged?.Invoke(this, EventArgs.Empty);
            }
            return result;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DisconnectDeviceAsync(JJDevice device)
    {
        try
        {
            var result = await device.DisconnectAsync();
            if (result && ConnectedDevices.Contains(device))
            {
                ConnectedDevices.Remove(device);
                DevicesChanged?.Invoke(this, EventArgs.Empty);
            }
            return result;
        }
        catch
        {
            return false;
        }
    }

    public async Task AutoConnectDevicesAsync()
    {
        var devicesToConnect = AvailableDevices
            .Where(d => d.AutoConnect && !d.IsConnected)
            .ToList();

        foreach (var device in devicesToConnect)
        {
            await ConnectDeviceAsync(device);
        }
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _discoveryTimer.Stop();
            _discoveryTimer.Dispose();
            _lock.Dispose();

            foreach (var device in AvailableDevices)
            {
                device.Dispose();
            }

            AvailableDevices.Clear();
            ConnectedDevices.Clear();

            _isDisposed = true;
        }
    }
}
