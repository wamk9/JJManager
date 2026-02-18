using JJManager.Core.Interfaces.Repositories;
using JJManager.Core.Profile;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JJManager.Core.Devices;

/// <summary>
/// Base class for all JohnJohn3D devices
/// Cross-platform implementation using HidSharp
/// </summary>
public abstract class JJDevice : INotifyPropertyChanged, IDisposable
{

    #region Interfaces
    protected readonly IProductRepository? _productRepository;
    #endregion

    #region Fields

    protected Guid _productId = Guid.Empty;
    protected string _productName = string.Empty;
    protected string _connId = string.Empty;
    protected string _deviceId = string.Empty;
    protected DeviceConnectionType _connectionType = DeviceConnectionType.Unset;
    protected DeviceConnectionState _connectionState = DeviceConnectionState.Disconnected;
    protected Version? _firmwareVersion;
    protected List<string> _connPorts = new();
    protected bool _autoConnect;
    protected bool _isDisposed;
    protected DeviceProfile? _profile;

    // Thread management
    protected CancellationTokenSource? _cancellationTokenSource;
    protected Task? _dataTask;

    // Supported commands for byte-based protocol
    protected HashSet<ushort> _supportedCommands = new();

    #endregion

    #region Properties

    /// <summary>
    /// Unique connection identifier (usually hash of device path)
    /// </summary>
    public string ConnId => _connId;

    /// <summary>
    /// Product name as reported by the device
    /// </summary>
    public string ProductName => _productName;

    /// <summary>
    /// Icon name for the device (Material Icon kind name, e.g., "Chip", "ScaleBalance")
    /// Override in derived classes to set a specific icon
    /// </summary>
    public virtual string IconName => "Chip";

    /// <summary>
    /// Device initials for display when no icon is available (e.g., "LC" for LoadCell)
    /// Override in derived classes to set specific initials
    /// </summary>
    public virtual string DeviceInitials => "JJ";

    /// <summary>
    /// Device class name used for identifying the device type (e.g., "JJLC01", "JJDB01")
    /// Override in derived classes to set specific class name
    /// </summary>
    public virtual string DeviceClassName => "Unknown";

    /// <summary>
    /// Product ID from database
    /// </summary>
    public Guid ProductId => _productId;

    /// <summary>
    /// Set the product ID (called by factory after device creation)
    /// </summary>
    public void SetProductId(Guid productId)
    {
        _productId = productId;
    }

    /// <summary>
    /// User device ID from database
    /// </summary>
    public string DeviceId => _deviceId;

    /// <summary>
    /// Type of connection (HID, Joystick, Bluetooth)
    /// </summary>
    public DeviceConnectionType ConnectionType => _connectionType;

    /// <summary>
    /// Current connection state
    /// </summary>
    public DeviceConnectionState ConnectionState
    {
        get => _connectionState;
        protected set
        {
            if (_connectionState != value)
            {
                _connectionState = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsConnected));
            }
        }
    }

    /// <summary>
    /// Whether the device is currently connected
    /// </summary>
    public bool IsConnected => _connectionState == DeviceConnectionState.Connected;

    /// <summary>
    /// Firmware version of the device
    /// </summary>
    public Version? FirmwareVersion
    {
        get => _firmwareVersion;
        set
        {
            if (_firmwareVersion != value)
            {
                _firmwareVersion = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// COM ports associated with this device (for firmware updates)
    /// </summary>
    public IReadOnlyList<string> ConnPorts => _connPorts.AsReadOnly();

    /// <summary>
    /// Whether auto-connection is enabled for this device
    /// </summary>
    public bool AutoConnect
    {
        get => _autoConnect;
        set
        {
            if (_autoConnect != value)
            {
                _autoConnect = value;
                OnAutoConnectChanged(value);
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Device profile containing input/output configurations
    /// </summary>
    public DeviceProfile? Profile
    {
        get => _profile;
        set
        {
            if (_profile != value)
            {
                _profile = value;
                _profile?.MarkAsNeedsUpdate();
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Set of command IDs this device supports (for byte-based protocol)
    /// </summary>
    public IReadOnlySet<ushort> SupportedCommands => _supportedCommands;

    #endregion

    #region Events

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raised when device data is received
    /// </summary>
    public event EventHandler<DeviceDataEventArgs>? DataReceived;

    /// <summary>
    /// Raised when device connection state changes
    /// </summary>
    public event EventHandler<DeviceConnectionState>? ConnectionStateChanged;

    #endregion

    #region Constructor

    protected JJDevice()
    {
    }

    protected JJDevice(IProductRepository? productRepository)
    {
        _productRepository = productRepository;
    }

    #endregion

    #region Abstract Methods

    /// <summary>
    /// Connect to the device and start communication tasks
    /// </summary>
    public abstract Task<bool> ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnect from the device and stop communication tasks
    /// </summary>
    public abstract Task<bool> DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the firmware version from the device
    /// </summary>
    public abstract Task<Version?> GetFirmwareVersionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Send/Receive data to the device (main loop)
    /// </summary>
    protected abstract Task DataLoopAsync(CancellationToken cancellationToken);
    #endregion

    #region Virtual Methods

    /// <summary>
    /// Called when auto-connect setting changes
    /// </summary>
    protected virtual void OnAutoConnectChanged(bool value)
    {
        // Override in derived classes to persist the setting
    }

    /// <summary>
    /// Load the ProductId from database based on DeviceClassName
    /// </summary>
    public virtual async Task<bool> LoadProductIdAsync()
    {
        if (_productRepository == null)
        {
            Console.WriteLine($"[JJDevice] ProductRepository is null for {DeviceClassName}");
            return false;
        }

        try
        {
            var product = await _productRepository.GetByClassNameAsync(DeviceClassName);
            if (product != null)
            {
                _productId = product.Id;
                Console.WriteLine($"[JJDevice] Loaded ProductId {_productId} for {DeviceClassName}");
                return true;
            }

            Console.WriteLine($"[JJDevice] Product not found for {DeviceClassName}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[JJDevice] Error loading ProductId: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Check if the device firmware is compatible with this version of JJManager
    /// </summary>
    public virtual bool CheckVersionCompatibility(Version? minimumVersion)
    {
        if (minimumVersion == null) return true;
        if (_firmwareVersion == null) return false;
        return _firmwareVersion >= minimumVersion;
    }

    /// <summary>
    /// Translate a version string to a Version object
    /// </summary>
    protected virtual Version? TranslateVersion(string? versionString)
    {
        if (string.IsNullOrWhiteSpace(versionString))
            return null;

        var parts = versionString.Split('.');
        return parts.Length switch
        {
            1 => new Version(int.Parse(parts[0]), 0),
            2 => new Version(int.Parse(parts[0]), int.Parse(parts[1])),
            3 => new Version(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2])),
            4 => new Version(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3])),
            _ => null
        };
    }

    #endregion

    #region Protected Methods

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected void OnDataReceived(byte[] data)
    {
        DataReceived?.Invoke(this, new DeviceDataEventArgs(data));
    }

    protected void OnConnectionStateChanged(DeviceConnectionState state)
    {
        ConnectionState = state;
        ConnectionStateChanged?.Invoke(this, state);
    }

    /// <summary>
    /// Start the receiving and sending tasks
    /// </summary>
    protected virtual async Task StartCommunicationTasksAsync()
    {
        Console.WriteLine($"[JJDevice] StartCommunicationTasksAsync called for {GetType().Name}");

        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        Console.WriteLine("[JJDevice] Creating DataLoop task...");
        _dataTask = Task.Run(async () =>
        {
            try
            {
                Console.WriteLine("[JJDevice] DataLoop task starting execution...");
                await DataLoopAsync(token);
                Console.WriteLine("[JJDevice] DataLoop task completed normally");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[JJDevice] DataLoop task was cancelled");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JJDevice] DataLoop task FATAL error: {ex.Message}");
                Console.WriteLine($"[JJDevice] Stack trace: {ex.StackTrace}");
            }
        }, token);

        // Wait briefly to ensure tasks are running
        try
        {
            await Task.Delay(100, token);
            Console.WriteLine("[JJDevice] DataLoop task created and running");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("[JJDevice] Cancelled during startup");
            // Ignore if cancelled during startup
        }
    }

    /// <summary>
    /// Stop the receiving and sending tasks
    /// </summary>
    protected virtual async Task StopCommunicationTasksAsync()
    {
        var cts = _cancellationTokenSource;
        if (cts != null)
        {
            try
            {
                cts.Cancel();
            }
            catch
            {
                // Ignore cancel errors
            }

            var tasks = new List<Task>();
            if (_dataTask != null) tasks.Add(_dataTask);

            if (tasks.Count > 0)
            {
                try
                {
                    await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(5));
                }
                catch (OperationCanceledException)
                {
                    // Expected
                }
                catch (TimeoutException)
                {
                    // Tasks took too long, continue anyway
                }
                catch
                {
                    // Ignore other errors
                }
            }

            try
            {
                cts.Dispose();
            }
            catch
            {
                // Ignore dispose errors
            }

            _cancellationTokenSource = null;
        }

        _dataTask = null;
    }

    #endregion

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _ = DisconnectAsync();
                _cancellationTokenSource?.Dispose();
            }

            _isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}

/// <summary>
/// Event args for device data received
/// </summary>
public class DeviceDataEventArgs : EventArgs
{
    public byte[] Data { get; }

    public DeviceDataEventArgs(byte[] data)
    {
        Data = data;
    }
}
