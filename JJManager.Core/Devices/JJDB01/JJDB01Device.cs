using JJManager.Core.Connections.SimHub;
using JJManager.Core.Devices.Connections;
using JJManager.Core.Interfaces.Repositories;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;

using HidSharpDevice = HidSharp.HidDevice;

namespace JJManager.Core.Devices.JJDB01;

/// <summary>
/// JJDB-01 Dashboard device implementation
/// Supports 16 RGB LEDs and SimHub integration for racing telemetry
/// </summary>
public class JJDB01Device : HidDevice
{
    #region Constants

    private const int LED_COUNT = 16;
    private const int CONNECTION_TIMEOUT_LIMIT = 5;
    private const int LOOP_DELAY_MS = 50;

    // Commands
    private const ushort CMD_DASHBOARD_DATA = 0x0001;
    private const ushort CMD_DASHBOARD_LAYOUT = 0x0002;
    private const ushort CMD_DASHBOARD_PAGE = 0x0003;
    private const ushort CMD_LED_DATA = 0x0010;
    private const ushort CMD_LED_BRIGHTNESS = 0x0011;
    private const ushort CMD_KEEP_ALIVE = 0x0012;
    private const ushort CMD_DEVICE_INFO = 0x00FF;

    #endregion

    #region Fields

    private int _connectionTimeoutCount = 0;
    private volatile bool _configurationNeedsUpdate = false;

    // LED state (16 LEDs x 3 bytes RGB)
    private readonly byte[] _ledColors = new byte[LED_COUNT * 3];
    private readonly byte[] _lastSentLedColors = new byte[LED_COUNT * 3];
    private bool _ledsNeedFullUpdate = true;

    // Brightness
    private int _brightness = 50;
    private int _lastSentBrightness = -1;

    // Keep-alive
    private DateTime _lastKeepAlive = DateTime.MinValue;
    private readonly TimeSpan _keepAliveInterval = TimeSpan.FromSeconds(3);

    // SimHub integration
    private SimHubWebsocket? _simHubSync;
    private bool _simHubCommunicationOk = false;

    // Cache for SimHub property values (for restoring LED colors after deselection)
    private readonly List<SimHubPropertyItem> _simHubPropertiesCache = new();

    private void SetSimHubStatus(bool connected)
    {
        if (_simHubCommunicationOk != connected)
        {
            _simHubCommunicationOk = connected;
            OnPropertyChanged(nameof(IsSimHubConnected));
        }
    }

    // LED selection blink
    private int _selectedLed = -1;
    private bool _selectedLedState = false;
    private DateTime _lastBlinkToggle = DateTime.MinValue;
    private readonly TimeSpan _blinkInterval = TimeSpan.FromMilliseconds(500);

    // LED animation state (blink/pulse effects controlled by JJManager)
    private readonly int[] _ledAnimationMode = new int[LED_COUNT]; // 0=Off, 1=On, 2=Blink, 3=Pulse
    private readonly byte[] _ledBaseColors = new byte[LED_COUNT * 3]; // Base colors before animation
    private DateTime _animationStartTime = DateTime.Now;
    private const int BLINK_INTERVAL_MS = 500; // Blink every 500ms
    private const int PULSE_CYCLE_MS = 1000; // Full pulse cycle (fade in + fade out)

    // Dashboard data tracking (only send changed values)
    private readonly Dictionary<string, string> _lastSentDashboardValues = new();
    private bool _dashboardNeedsFullUpdate = true;

    #endregion

    #region Properties

    /// <summary>
    /// Number of LEDs on this device
    /// </summary>
    public int LedCount => LED_COUNT;

    /// <summary>
    /// Current LED brightness (0-255)
    /// </summary>
    public int Brightness
    {
        get => _brightness;
        private set
        {
            if (_brightness != value)
            {
                _brightness = Math.Clamp(value, 0, 255);
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Icon name for Dashboard
    /// </summary>
    public override string IconName => "ViewDashboard";

    /// <summary>
    /// Device initials for Dashboard
    /// </summary>
    public override string DeviceInitials => "DB";

    /// <summary>
    /// Device class name for Dashboard
    /// </summary>
    public override string DeviceClassName => "JJDB01";

    /// <summary>
    /// Whether SimHub communication is working (WebSocket connected AND data exchange successful)
    /// </summary>
    public bool IsSimHubConnected => _simHubCommunicationOk;

    #endregion

    #region Events

    /// <summary>
    /// Raised when LED data is updated
    /// </summary>
    public event EventHandler<JJDB01DataEventArgs>? DataUpdated;

    #endregion

    #region Constructor

    public JJDB01Device(HidSharpDevice hidSharpDevice, IProductRepository? productRepository = null)
        : base(hidSharpDevice, productRepository)
    {
        _supportedCommands = new HashSet<ushort>
        {
            CMD_DASHBOARD_DATA,
            CMD_DASHBOARD_LAYOUT,
            CMD_DASHBOARD_PAGE,
            CMD_LED_DATA,
            CMD_LED_BRIGHTNESS,
            CMD_KEEP_ALIVE,
            CMD_DEVICE_INFO
        };

        // Initialize all LEDs to off (black)
        Array.Clear(_ledColors, 0, _ledColors.Length);
        Array.Clear(_lastSentLedColors, 0, _lastSentLedColors.Length);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Set global LED brightness
    /// </summary>
    public void SetBrightness(int brightness)
    {
        Brightness = Math.Clamp(brightness, 0, 255);
        _configurationNeedsUpdate = true;
        _profile?.MarkAsNeedsUpdate();
    }

    /// <summary>
    /// Set a single LED color
    /// </summary>
    public void SetLedColor(int ledIndex, byte r, byte g, byte b)
    {
        if (ledIndex < 0 || ledIndex >= LED_COUNT)
            return;

        int offset = ledIndex * 3;
        _ledColors[offset] = r;
        _ledColors[offset + 1] = g;
        _ledColors[offset + 2] = b;
        _configurationNeedsUpdate = true;
    }

    /// <summary>
    /// Set a single LED color from hex string
    /// </summary>
    public void SetLedColor(int ledIndex, string hexColor)
    {
        if (string.IsNullOrEmpty(hexColor))
            return;

        try
        {
            hexColor = hexColor.TrimStart('#');
            if (hexColor.Length == 6)
            {
                byte r = Convert.ToByte(hexColor.Substring(0, 2), 16);
                byte g = Convert.ToByte(hexColor.Substring(2, 2), 16);
                byte b = Convert.ToByte(hexColor.Substring(4, 2), 16);
                SetLedColor(ledIndex, r, g, b);
            }
        }
        catch
        {
            // Invalid hex color
        }
    }

    /// <summary>
    /// Set multiple LEDs to the same color
    /// </summary>
    public void SetLedGroupColor(IEnumerable<int> ledIndices, byte r, byte g, byte b)
    {
        foreach (var index in ledIndices)
        {
            SetLedColor(index, r, g, b);
        }
    }

    /// <summary>
    /// Set all LEDs to black (off)
    /// </summary>
    public void ClearAllLeds()
    {
        Array.Clear(_ledColors, 0, _ledColors.Length);
        _configurationNeedsUpdate = true;
    }

    /// <summary>
    /// Get the current color of a LED
    /// </summary>
    public (byte R, byte G, byte B) GetLedColor(int ledIndex)
    {
        if (ledIndex < 0 || ledIndex >= LED_COUNT)
            return (0, 0, 0);

        int offset = ledIndex * 3;
        return (_ledColors[offset], _ledColors[offset + 1], _ledColors[offset + 2]);
    }

    /// <summary>
    /// Request full LED update on next cycle
    /// </summary>
    public void RequestFullLedUpdate()
    {
        _ledsNeedFullUpdate = true;
        _configurationNeedsUpdate = true;
    }

    #endregion

    #region Communication Loop

    protected override async Task DataLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                try
                {
                    // 1. Keep-alive (every 3 seconds) - ESSENTIAL
                    if ((DateTime.Now - _lastKeepAlive) >= _keepAliveInterval)
                    {
                        try
                        {
                            await SendKeepAliveAsync(cancellationToken);
                            _lastKeepAlive = DateTime.Now;
                            _connectionTimeoutCount = 0;
                        }
                        catch (Exception keepAliveEx)
                        {
                            Console.WriteLine($"[JJDB01] Keep-alive error (non-fatal): {keepAliveEx.Message}");
                            _connectionTimeoutCount++;
                        }
                    }

                    // 2. LED selection blink effect - ESSENTIAL
                    if (_selectedLed >= 0 && _selectedLed < LED_COUNT)
                    {
                        if ((DateTime.Now - _lastBlinkToggle) >= _blinkInterval)
                        {
                            _selectedLedState = !_selectedLedState;
                            _lastBlinkToggle = DateTime.Now;

                            Console.WriteLine($"[JJDB01] Blink LED {_selectedLed}: state={_selectedLedState}");

                            try
                            {
                                if (_selectedLedState)
                                {
                                    // LED ON - use current color or white if no color set
                                    var (r, g, b) = GetLedColor(_selectedLed);
                                    if (r == 0 && g == 0 && b == 0)
                                    {
                                        r = g = b = 255; // White
                                    }
                                    await SendSingleLedAsync(_selectedLed, r, g, b, cancellationToken);
                                }
                                else
                                {
                                    // LED OFF
                                    await SendSingleLedAsync(_selectedLed, 0, 0, 0, cancellationToken);
                                }
                            }
                            catch (Exception blinkEx)
                            {
                                Console.WriteLine($"[JJDB01] Blink error (non-fatal): {blinkEx.Message}");
                            }
                        }
                    }

                    // 3. Manual configuration update (from UI actions like TestAllLedsOn)
                    if (_configurationNeedsUpdate)
                    {
                        try
                        {
                            await SendConfigurationAsync(cancellationToken);
                            _configurationNeedsUpdate = false;
                        }
                        catch (Exception configEx)
                        {
                            Console.WriteLine($"[JJDB01] Config update error (non-fatal): {configEx.Message}");
                        }
                    }

                    // 4. Profile update check
                    if (_profile?.NeedsUpdate == true)
                    {
                        // Force resend of all data (LEDs and Dashboard)
                        Array.Clear(_lastSentLedColors, 0, _lastSentLedColors.Length);
                        _lastSentBrightness = -1;
                        _ledsNeedFullUpdate = true;
                        _dashboardNeedsFullUpdate = true;
                        _lastSentDashboardValues.Clear();
                        _profile.ClearNeedsUpdate();
                    }

                    // 5. Brightness (only if changed)
                    int brightness = _profile?.GetDataValue<int>("led_brightness", 50) ?? 50;
                    if (_lastSentBrightness != brightness)
                    {
                        try
                        {
                            _brightness = brightness;
                            await SendBrightnessAsync(cancellationToken);
                            _lastSentBrightness = brightness;
                        }
                        catch (Exception brightnessEx)
                        {
                            Console.WriteLine($"[JJDB01] Brightness error (non-fatal): {brightnessEx.Message}");
                        }
                    }

                    // 6. SimHub connection + LED sync (OPTIONAL - don't fail if SimHub not available)
                    try
                    {
                        if (_simHubSync == null)
                        {
                            _simHubSync = new SimHubWebsocket(2920, "JJDB01_" + _connId);
                        }

                        // Try to connect if not connected
                        if (!_simHubSync.IsConnected)
                        {
                            await _simHubSync.ConnectAsync(cancellationToken);
                        }

                        // If connected, request data
                        if (_simHubSync.IsConnected)
                        {
                            var (success, data) = await _simHubSync.RequestDataAsync(cancellationToken);

                            if (success)
                            {
                                // Communication successful
                                SetSimHubStatus(true);

                                // Merge SimHub values into cache
                                MergeSimHubValues(_simHubSync.LastValues);

                                // Calculate and send LED colors based on profile outputs and cached SimHub data
                                byte[] newColors = CalculateLedColors();
                                for (int i = 0; i < LED_COUNT; i++)
                                {
                                    // Skip if this LED is currently selected (being blinked)
                                    if (i == _selectedLed)
                                        continue;

                                    int offset = i * 3;
                                    bool changed = _ledsNeedFullUpdate ||
                                                  _lastSentLedColors[offset] != newColors[offset] ||
                                                  _lastSentLedColors[offset + 1] != newColors[offset + 1] ||
                                                  _lastSentLedColors[offset + 2] != newColors[offset + 2];

                                    if (changed)
                                    {
                                        await SendSingleLedAsync(i, newColors[offset], newColors[offset + 1], newColors[offset + 2], cancellationToken);
                                        _lastSentLedColors[offset] = newColors[offset];
                                        _lastSentLedColors[offset + 1] = newColors[offset + 1];
                                        _lastSentLedColors[offset + 2] = newColors[offset + 2];
                                    }
                                }
                                _ledsNeedFullUpdate = false;

                                // 7. Dashboard Data - send telemetry values to device display
                                // Uses LastValuesUpdated to only send changed values
                                await SendDashboardDataAsync(_simHubSync.LastValuesUpdated, cancellationToken);
                            }
                            else
                            {
                                // Request failed - try to disconnect and retry next loop
                                SetSimHubStatus(false);
                                await _simHubSync.DisconnectAsync();
                            }
                        }
                        else
                        {
                            // Failed to connect
                            SetSimHubStatus(false);
                        }
                    }
                    catch (Exception simHubEx)
                    {
                        // SimHub error - mark as not working and try to disconnect
                        SetSimHubStatus(false);
                        try { await _simHubSync?.DisconnectAsync()!; } catch { }
                        Debug.WriteLine($"JJDB01 SimHub error (non-fatal): {simHubEx.Message}");
                    }

                    await Task.Delay(LOOP_DELAY_MS, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"JJDB01 DataLoop error: {ex.Message}");
                Console.WriteLine($"[JJDB01] DataLoop iteration error: {ex.Message}");
                _connectionTimeoutCount++;

                if (_connectionTimeoutCount >= CONNECTION_TIMEOUT_LIMIT)
                {
                    Debug.WriteLine("JJDB01 Connection timeout limit reached, disconnecting...");
                    Console.WriteLine("[JJDB01] Connection timeout limit reached, disconnecting...");
                    await DisconnectAsync(cancellationToken);
                    break;
                }
            }
        }
        }
        catch (Exception outerEx)
        {
            Debug.WriteLine($"JJDB01 DataLoop FATAL error: {outerEx}");
            Console.WriteLine($"[JJDB01] DataLoop FATAL error: {outerEx.Message}");
            Console.WriteLine($"[JJDB01] Stack trace: {outerEx.StackTrace}");
        }

        // Cleanup SimHub connection
        SetSimHubStatus(false); // Reset SimHub status when device disconnects
        if (_simHubSync != null)
        {
            await _simHubSync.DisconnectAsync();
            _simHubSync.Dispose();
            _simHubSync = null;
        }

        Debug.WriteLine("JJDB01 DataLoop ended");
    }

    /// <summary>
    /// Merge SimHub values into the properties cache.
    /// Updates existing properties or adds new ones.
    /// </summary>
    private void MergeSimHubValues(JsonObject? simHubValues)
    {
        if (simHubValues == null)
            return;

        foreach (var kvp in simHubValues)
        {
            string propertyKey = kvp.Key;
            object? value = null;
            
            // Extract primitive value from JsonNode
            if (kvp.Value != null)
            {
                try
                {
                    value = kvp.Value.GetValue<object>();
                }
                catch
                {
                    value = kvp.Value.ToString();
                }
            }

            // Find existing property in cache
            var existingProp = _simHubPropertiesCache.FirstOrDefault(p => p.PropertyKey == propertyKey);

            if (existingProp != null)
            {
                // Update existing property value
                existingProp.CurrentValue = value;
            }
            else
            {
                // Add new property to cache
                var newProp = new SimHubPropertyItem(
                    key: propertyKey,
                    displayName: propertyKey,
                    category: "SimHub",
                    fieldType: SimHubFieldType.Text
                )
                {
                    CurrentValue = value
                };
                _simHubPropertiesCache.Add(newProp);
            }
        }
    }

    /// <summary>
    /// Get the current value of a SimHub property from cache
    /// </summary>
    private object? GetCachedSimHubValue(string propertyKey)
    {
        var prop = _simHubPropertiesCache.FirstOrDefault(p => p.PropertyKey == propertyKey);
        return prop?.CurrentValue;
    }

    /// <summary>
    /// Calculate LED colors based on profile outputs and cached SimHub values.
    /// Applies blink/pulse effects based on ModeIfEnabled.
    /// </summary>
    private byte[] CalculateLedColors()
    {
        byte[] colors = new byte[LED_COUNT * 3];
        Array.Clear(colors, 0, colors.Length);

        // Reset animation modes
        Array.Clear(_ledAnimationMode, 0, _ledAnimationMode.Length);
        Array.Clear(_ledBaseColors, 0, _ledBaseColors.Length);

        if (_profile?.Outputs == null)
            return colors;

        // Track which LEDs have been set (last active output wins)
        bool[] ledSet = new bool[LED_COUNT];

        // Process outputs in descending order (highest Order first, so last active wins)
        var ledOutputs = _profile.Outputs
            .Where(x => x.Led != null)
            .OrderByDescending(x => x.Led!.Order);

        foreach (var output in ledOutputs)
        {
            if (string.IsNullOrEmpty(output.Led?.Property))
                continue;

            // Get SimHub value from cache
            var simHubValue = GetCachedSimHubValue(output.Led.Property);

            // Check if output is active based on SimHub value
            bool isActive = output.Led.SetActivatedValue(simHubValue ?? false);

            if (isActive && output.Led.LedsGrouped != null)
            {
                // Parse hex color (e.g., "#FF00FF")
                string hexColor = output.Led.GetActualLedColor ?? "#000000";
                if (hexColor.StartsWith("#") && hexColor.Length >= 7)
                {
                    byte r = Convert.ToByte(hexColor.Substring(1, 2), 16);
                    byte g = Convert.ToByte(hexColor.Substring(3, 2), 16);
                    byte b = Convert.ToByte(hexColor.Substring(5, 2), 16);

                    // Apply to all LEDs in this output's group
                    foreach (int ledIdx in output.Led.LedsGrouped)
                    {
                        if (ledIdx >= 0 && ledIdx < LED_COUNT && !ledSet[ledIdx])
                        {
                            int offset = ledIdx * 3;

                            // Store base color and animation mode
                            _ledBaseColors[offset] = r;
                            _ledBaseColors[offset + 1] = g;
                            _ledBaseColors[offset + 2] = b;
                            _ledAnimationMode[ledIdx] = output.Led.ModeIfEnabled;

                            // Apply animation effect
                            ApplyLedAnimation(ledIdx, r, g, b, output.Led.ModeIfEnabled, colors);

                            ledSet[ledIdx] = true;
                        }
                    }
                }
            }
        }

        return colors;
    }

    /// <summary>
    /// Apply blink/pulse animation effect to a LED color
    /// </summary>
    private void ApplyLedAnimation(int ledIdx, byte r, byte g, byte b, int mode, byte[] colors)
    {
        int offset = ledIdx * 3;
        double elapsed = (DateTime.Now - _animationStartTime).TotalMilliseconds;

        switch (mode)
        {
            case 0: // Off
                colors[offset] = 0;
                colors[offset + 1] = 0;
                colors[offset + 2] = 0;
                break;

            case 1: // On (always on)
                colors[offset] = r;
                colors[offset + 1] = g;
                colors[offset + 2] = b;
                break;

            case 2: // Blink
                // Toggle on/off based on elapsed time
                bool isOn = ((int)(elapsed / BLINK_INTERVAL_MS) % 2) == 0;
                if (isOn)
                {
                    colors[offset] = r;
                    colors[offset + 1] = g;
                    colors[offset + 2] = b;
                }
                else
                {
                    colors[offset] = 0;
                    colors[offset + 1] = 0;
                    colors[offset + 2] = 0;
                }
                break;

            case 3: // Pulse (fade in/out using sine wave)
                // Use sine wave for smooth pulsing (0 to 1 to 0)
                double phase = (elapsed % PULSE_CYCLE_MS) / PULSE_CYCLE_MS;
                double intensity = (Math.Sin(phase * 2 * Math.PI - Math.PI / 2) + 1) / 2; // 0 to 1

                colors[offset] = (byte)(r * intensity);
                colors[offset + 1] = (byte)(g * intensity);
                colors[offset + 2] = (byte)(b * intensity);
                break;

            default: // Default to On
                colors[offset] = r;
                colors[offset + 1] = g;
                colors[offset + 2] = b;
                break;
        }
    }

    #endregion

    #region Dashboard Data Methods

    /// <summary>
    /// Send dashboard data to device.
    /// Only sends values that have changed since last send.
    /// Format: CMD=0x0001, Payload=[PROP_ID_H][PROP_ID_L][Value as ASCII bytes]
    /// </summary>
    private async Task SendDashboardDataAsync(JsonObject? simHubValues, CancellationToken cancellationToken)
    {
        if (simHubValues == null || simHubValues.Count == 0)
            return;

        int sentCount = 0;

        foreach (var kvp in simHubValues)
        {
            string propertyName = kvp.Key;
            string valueString = SimHubPropertyItem.FormatValue(kvp.Value);

            // Check if property is in dictionary
            if (!SimHubPropertyItem.TryGetCommandId(propertyName, out ushort propertyId))
            {
                // Property not supported for dashboard display
                continue;
            }

            // Check if value changed (or if we need full update)
            bool shouldSend = _dashboardNeedsFullUpdate ||
                              !_lastSentDashboardValues.TryGetValue(propertyName, out var lastValue) ||
                              lastValue != valueString;

            if (!shouldSend)
                continue;

            try
            {
                // Build payload: [PROP_ID_H][PROP_ID_L][Value bytes...]
                var valueBytes = Encoding.ASCII.GetBytes(valueString);
                var payload = new byte[2 + valueBytes.Length];
                payload[0] = (byte)(propertyId >> 8);   // PROP_ID_H
                payload[1] = (byte)(propertyId & 0xFF); // PROP_ID_L
                Array.Copy(valueBytes, 0, payload, 2, valueBytes.Length);

                // Send to device (CMD_DASHBOARD_DATA = 0x0001)
                var msg = new HidMessage(CMD_DASHBOARD_DATA, payload);
                bool success = await SendMessageAsync(msg, delayAfterWriteMs:1, cancellationToken: cancellationToken);

                if (success)
                {
                    _lastSentDashboardValues[propertyName] = valueString;
                    sentCount++;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"JJDB01 Dashboard send error for {propertyName}: {ex.Message}");
            }
        }

        if (sentCount > 0)
        {
            Debug.WriteLine($"JJDB01 Sent {sentCount} dashboard value(s)");
        }

        _dashboardNeedsFullUpdate = false;
    }

    /// <summary>
    /// Request full dashboard data update on next cycle
    /// </summary>
    public void RequestFullDashboardUpdate()
    {
        _dashboardNeedsFullUpdate = true;
        _lastSentDashboardValues.Clear();
    }

    #endregion

    #region Private Methods - Sending

    private async Task SendConfigurationAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Send brightness (only if changed)
            if (_lastSentBrightness != _brightness)
            {
                await SendBrightnessAsync(cancellationToken);
                _lastSentBrightness = _brightness;
            }

            // Send LED colors
            await SendLedColorsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"JJDB01 SendConfiguration error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Unselect the currently selected LED (stops the blinking effect and restores color from cache)
    /// </summary>
    public void UnselectAllLeds()
    {
        int ledToRestore = _selectedLed;
        _selectedLed = -1;
        _selectedLedState = false;

        // Restore LED color from cached SimHub values
        if (ledToRestore >= 0 && ledToRestore < LED_COUNT)
        {
            RestoreLedColorFromCache(ledToRestore);
        }
    }

    /// <summary>
    /// Select a LED for visual feedback (blink effect handled in DataLoop)
    /// </summary>
    public void SelectLed(int index)
    {
        int lastSelectedLed = _selectedLed;
        _selectedLed = index >= 0 && index < LED_COUNT ? index : -1;

        // Restore last selected LED color from cache when changing selection
        if (lastSelectedLed >= 0 && lastSelectedLed < LED_COUNT)
        {
            RestoreLedColorFromCache(lastSelectedLed);
        }

        _selectedLedState = false;
        _lastBlinkToggle = DateTime.Now;
    }

    /// <summary>
    /// Restore a LED color from cached SimHub values
    /// </summary>
    private void RestoreLedColorFromCache(int ledIndex)
    {
        if (ledIndex < 0 || ledIndex >= LED_COUNT)
            return;

        try
        {
            // Recalculate colors based on cached SimHub values
            byte[] colors = CalculateLedColors();

            int offset = ledIndex * 3;
            byte r = colors[offset];
            byte g = colors[offset + 1];
            byte b = colors[offset + 2];

            // Send immediately
            _ = SendSingleLedAsync(ledIndex, r, g, b, new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token);

            // Update tracking
            _lastSentLedColors[offset] = r;
            _lastSentLedColors[offset + 1] = g;
            _lastSentLedColors[offset + 2] = b;

            Console.WriteLine($"[JJDB01] Restored LED {ledIndex} color from cache: RGB({r},{g},{b})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[JJDB01] Error restoring LED {ledIndex} color: {ex.Message}");
        }
    }

    private async Task SendKeepAliveAsync(CancellationToken cancellationToken)
    {
        var msg = new HidMessage(CMD_KEEP_ALIVE, (byte)0x01);
        await SendMessageAsync(msg, cancellationToken: cancellationToken);
    }

    private async Task SendBrightnessAsync(CancellationToken cancellationToken)
    {
        var msg = new HidMessage(CMD_LED_BRIGHTNESS, (byte)_brightness);
        await SendMessageAsync(msg, cancellationToken: cancellationToken);
        Debug.WriteLine($"JJDB01 Sent brightness: {_brightness}");
    }

    private async Task SendLedColorsAsync(CancellationToken cancellationToken)
    {
        int sentCount = 0;
        for (int i = 0; i < LED_COUNT; i++)
        {
            int offset = i * 3;
            byte r = _ledColors[offset];
            byte g = _ledColors[offset + 1];
            byte b = _ledColors[offset + 2];

            // Check if LED color changed (or if full update requested)
            bool changed = _ledsNeedFullUpdate ||
                          _lastSentLedColors[offset] != r ||
                          _lastSentLedColors[offset + 1] != g ||
                          _lastSentLedColors[offset + 2] != b;

            if (changed)
            {
                await SendSingleLedAsync(i, r, g, b, cancellationToken);

                // Update tracking
                _lastSentLedColors[offset] = r;
                _lastSentLedColors[offset + 1] = g;
                _lastSentLedColors[offset + 2] = b;

                sentCount++;
                await Task.Delay(2, cancellationToken); // Small delay between LEDs
            }
        }

        if (sentCount > 0)
        {
            Debug.WriteLine($"JJDB01 Sent {sentCount} LED(s)");
        }

        _ledsNeedFullUpdate = false;

        // Raise data updated event
        DataUpdated?.Invoke(this, new JJDB01DataEventArgs
        {
            Brightness = _brightness,
            LedColors = (byte[])_ledColors.Clone()
        });
    }

    private async Task SendSingleLedAsync(int ledIndex, byte r, byte g, byte b, CancellationToken cancellationToken)
    {
        var payload = new byte[] { (byte)ledIndex, r, g, b };
        var msg = new HidMessage(CMD_LED_DATA, payload);
        bool success = await SendMessageAsync(msg, cancellationToken: cancellationToken);
        Debug.WriteLine($"JJDB01 LED {ledIndex}: RGB({r},{g},{b}) - {(success ? "OK" : "FAIL")}");
    }

    #endregion

    #region Dispose

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DataUpdated = null;
        }
        base.Dispose(disposing);
    }

    #endregion
}

/// <summary>
/// Event args for JJDB01 data updates
/// </summary>
public class JJDB01DataEventArgs : EventArgs
{
    public int Brightness { get; init; }
    public byte[] LedColors { get; init; } = Array.Empty<byte>();
}
