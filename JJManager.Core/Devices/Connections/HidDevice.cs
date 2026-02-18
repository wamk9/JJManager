using JJManager.Core.Database.Entities;
using JJManager.Core.Interfaces.Repositories;
using JJManager.Core.Others;
using System.Diagnostics;
using System.Text;

namespace JJManager.Core.Devices.Connections;

/// <summary>
/// Base class for HID-connected devices using HidSharp
/// Cross-platform (Windows, Linux, macOS)
/// </summary>
public abstract class HidDevice : JJDevice
{
    #region Constants

    protected const int DEFAULT_TIMEOUT_MS = 2000;
    protected const int DEFAULT_DELAY_MS = 50;

    /// <summary>
    /// Delay after HID write operation (ms)
    /// </summary>
    protected const int DELAY_AFTER_WRITE_MS = 10;

    /// <summary>
    /// Delay after HID read operation (ms)
    /// </summary>
    protected const int DELAY_AFTER_READ_MS = 10;

    #endregion

    #region Fields

    protected HidSharp.HidDevice? _hidSharpDevice;
    protected string _devicePath = string.Empty;
    protected byte _reportId = 0x00;
    protected readonly SemaphoreSlim _communicationLock = new(1, 1);

    #endregion

    #region Properties

    public HidSharp.HidDevice? HidSharpDevice => _hidSharpDevice;
    public string DevicePath => _devicePath;

    #endregion

    #region Constructor

    protected HidDevice(HidSharp.HidDevice hidSharpDevice, IProductRepository? productRepository = null)
        : base(productRepository)
    {
        _hidSharpDevice = hidSharpDevice ?? throw new ArgumentNullException(nameof(hidSharpDevice));
        _productName = _hidSharpDevice.GetProductName();
        _connectionType = DeviceConnectionType.HID;
        _devicePath = _hidSharpDevice.DevicePath;
        _connId = HashHelper.GetDeterministicHash(_devicePath);
        _productId = _productRepository?.GetByNameAndTypeAsync(_productName, "HID").GetAwaiter().GetResult()?.Id ?? Guid.Empty;
    }

    #endregion

    #region Connection Methods

    public override async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[HidDevice] ConnectAsync called. IsConnected={IsConnected}, DevicePath={_devicePath}");

        if (IsConnected)
            return true;

        try
        {
            OnConnectionStateChanged(DeviceConnectionState.Connecting);
            Console.WriteLine("[HidDevice] Setting state to Connected BEFORE starting tasks...");

            // Set connected state BEFORE starting tasks so DataLoop sees IsConnected=true
            OnConnectionStateChanged(DeviceConnectionState.Connected);

            Console.WriteLine("[HidDevice] Starting communication tasks...");

            // Start communication tasks
            await StartCommunicationTasksAsync();

            Console.WriteLine("[HidDevice] Communication tasks started successfully");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"HidDevice.ConnectAsync error: {ex.Message}");
            Console.WriteLine($"[HidDevice] ConnectAsync error: {ex.Message}");
            Console.WriteLine($"[HidDevice] Stack trace: {ex.StackTrace}");
            OnConnectionStateChanged(DeviceConnectionState.Error);
            return false;
        }
    }

    public override async Task<bool> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConnected && _connectionState != DeviceConnectionState.Connecting)
            return true;

        try
        {
            await StopCommunicationTasksAsync();
            OnConnectionStateChanged(DeviceConnectionState.Disconnected);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"HidDevice.DisconnectAsync error: {ex.Message}");
            OnConnectionStateChanged(DeviceConnectionState.Error);
            return false;
        }
    }

    #endregion

    #region HID Communication - MAIN FUNCTION

    /// <summary>
    /// Main HID communication function - Send command and receive response
    /// </summary>
    /// <param name="command">Command ID (2 bytes, e.g., 0x0004)</param>
    /// <param name="payload">Optional payload bytes</param>
    /// <param name="expectResponse">Whether to wait for a response</param>
    /// <param name="timeoutMs">Timeout in milliseconds</param>
    /// <param name="forceConnection">Allow communication even when not connected</param>
    /// <param name="delayAfterWriteMs">Delay after write operation (ms), use -1 for default</param>
    /// <param name="delayAfterReadMs">Delay after read operation (ms), use -1 for default</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple with success flag and response bytes (payload only, without CMD and flags)</returns>
    public async Task<(bool Success, byte[] Response)> SendCommandAsync(
        ushort command,
        byte[]? payload = null,
        bool expectResponse = true,
        int timeoutMs = DEFAULT_TIMEOUT_MS,
        bool forceConnection = false,
        int delayAfterWriteMs = -1,
        int delayAfterReadMs = -1,
        CancellationToken cancellationToken = default)
    {
        if (_hidSharpDevice == null)
            return (false, Array.Empty<byte>());

        if (!IsConnected && !forceConnection)
            return (false, Array.Empty<byte>());

        await _communicationLock.WaitAsync(cancellationToken);

        try
        {
            if (!_hidSharpDevice.TryOpen(out var stream))
            {
                Debug.WriteLine($"HID SendCommand: Failed to open device");
                return (false, Array.Empty<byte>());
            }

            using (stream)
            {
                stream.ReadTimeout = timeoutMs;
                stream.WriteTimeout = timeoutMs;

                // Fixed value to try don't depend of device...
                int maxOutputSize = 65; //_hidSharpDevice.GetMaxOutputReportLength();
                int maxInputSize = 65; //_hidSharpDevice.GetMaxInputReportLength();

                // === BUILD SEND BUFFER ===
                // Format: [ReportID][CMD_H][CMD_L][Payload...][FLAG_H=0x20][FLAG_L=CMD_L]
                byte cmdH = (byte)((command >> 8) & 0xFF);
                byte cmdL = (byte)(command & 0xFF);

                int payloadLength = payload?.Length ?? 0;
                int totalLength = 1 + 2 + payloadLength + 2;  // ReportID + CMD + Payload + Flags

                byte[] sendBuffer = new byte[maxOutputSize];
                sendBuffer[0] = _reportId;        // ReportID
                sendBuffer[1] = cmdH;             // CMD_H
                sendBuffer[2] = cmdL;             // CMD_L

                if (payload != null && payloadLength > 0)
                {
                    int copyLength = Math.Min(payloadLength, maxOutputSize - 5);  // Leave room for flags
                    Array.Copy(payload, 0, sendBuffer, 3, copyLength);
                }

                // Add flags at end of data
                int flagPos = 3 + payloadLength;
                if (flagPos + 1 < maxOutputSize)
                {
                    sendBuffer[flagPos] = 0x20;       // FLAG_H (END)
                    sendBuffer[flagPos + 1] = cmdL;   // FLAG_L = CMD_L
                }

                // === SEND ===
                Debug.WriteLine($"HID TX: CMD={command:X4}, Payload={payloadLength} bytes");

                // Use synchronous write like 1.3.0 for better stability
                await stream.WriteAsync(sendBuffer, 0, sendBuffer.Length);

                int writeDelay = delayAfterWriteMs >= 0 ? delayAfterWriteMs : DELAY_AFTER_WRITE_MS;
                if (writeDelay > 0)
                    await Task.Delay(writeDelay, cancellationToken);

                if (!expectResponse)
                {
                    return (true, Array.Empty<byte>());
                }

                // === RECEIVE ===
                byte[] readBuffer = new byte[maxInputSize];
                int bytesRead = stream.Read(readBuffer, 0, readBuffer.Length);

                int readDelay = delayAfterReadMs >= 0 ? delayAfterReadMs : DELAY_AFTER_READ_MS;
                if (readDelay > 0)
                    await Task.Delay(readDelay, cancellationToken);

                Debug.WriteLine($"HID RX: {bytesRead} bytes");

                if (bytesRead <= 3)
                {
                    Debug.WriteLine($"HID RX: Response too short");
                    return (false, Array.Empty<byte>());
                }

                // === PARSE RESPONSE ===
                // Format: [ReportID][CMD_H][CMD_L][Payload...][FLAG_H][FLAG_L][zeros...]
                byte respCmdH = readBuffer[1];
                byte respCmdL = readBuffer[2];
                ushort respCmd = (ushort)((respCmdH << 8) | respCmdL);

                // Verify command matches
                if (respCmd != command && !_supportedCommands.Contains(respCmd))
                {
                    Debug.WriteLine($"HID RX: CMD mismatch (sent {command:X4}, got {respCmd:X4})");
                    return (false, Array.Empty<byte>());
                }

                // Find flags by searching for [0x20][CMD_L] pattern from the end
                int dataEndPos = -1;
                for (int i = bytesRead - 2; i >= 3; i--)
                {
                    if (readBuffer[i] == 0x20 && readBuffer[i + 1] == cmdL)
                    {
                        dataEndPos = i;
                        break;
                    }
                }

                // Extract payload (skip ReportID + CMD, stop at flags)
                int payloadStart = 3;
                int payloadEnd = dataEndPos > payloadStart ? dataEndPos : bytesRead;

                // If no flags found, trim trailing zeros
                if (dataEndPos < 0)
                {
                    for (int i = bytesRead - 1; i >= payloadStart; i--)
                    {
                        if (readBuffer[i] != 0x00)
                        {
                            payloadEnd = i + 1;
                            break;
                        }
                    }
                }

                int responseLength = payloadEnd - payloadStart;
                if (responseLength <= 0)
                {
                    Debug.WriteLine($"HID RX: No payload data");
                    return (false, Array.Empty<byte>());
                }

                byte[] response = new byte[responseLength];
                Array.Copy(readBuffer, payloadStart, response, 0, responseLength);

                Debug.WriteLine($"HID RX: Parsed {responseLength} bytes payload");
                return (true, response);
            }
        }
        catch (TimeoutException)
        {
            Debug.WriteLine($"HID SendCommand: Timeout");
            return (false, Array.Empty<byte>());
        }
        catch (OperationCanceledException)
        {
            return (false, Array.Empty<byte>());
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"HID SendCommand error: {ex.Message}");
            return (false, Array.Empty<byte>());
        }
        finally
        {
            _communicationLock.Release();
        }
    }

    /// <summary>
    /// Simplified wrapper using HidMessage - expects response
    /// </summary>
    public async Task<(bool Success, byte[] Response)> RequestAsync(
        HidMessage message,
        bool forceConnection = false,
        int timeoutMs = DEFAULT_TIMEOUT_MS,
        int delayAfterWriteMs = -1,
        int delayAfterReadMs = -1,
        CancellationToken cancellationToken = default)
    {
        return await SendCommandAsync(
            message.Command,
            message.Payload.ToArray(),
            expectResponse: true,
            timeoutMs: timeoutMs,
            forceConnection: forceConnection,
            delayAfterWriteMs: delayAfterWriteMs,
            delayAfterReadMs: delayAfterReadMs,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Send message without waiting for response
    /// </summary>
    public async Task<bool> SendMessageAsync(
        HidMessage message,
        bool forceConnection = false,
        int timeoutMs = DEFAULT_TIMEOUT_MS,
        int delayAfterWriteMs = -1,
        CancellationToken cancellationToken = default)
    {
        (bool success, byte[] _) = await SendCommandAsync(
            message.Command,
            message.Payload.ToArray(),
            expectResponse: false,
            timeoutMs: timeoutMs,
            forceConnection: forceConnection,
            delayAfterWriteMs: delayAfterWriteMs,
            cancellationToken: cancellationToken);
        return success;
    }

    #endregion

    #region Firmware Version

    public override async Task<Version?> GetFirmwareVersionAsync(CancellationToken cancellationToken = default)
    {
        // Try binary protocol (CMD: 0x00FF, INFO_TYPE: 0x00 = Firmware Version)
        if (_supportedCommands.Contains(0x00FF))
        {
            for (int attempt = 0; attempt < 2; attempt++)
            {
                try
                {
                    var (success, response) = await SendCommandAsync(
                        0x00FF,
                        new byte[] { 0x00 },
                        expectResponse: true,
                        timeoutMs: 500,
                        forceConnection: true,
                        cancellationToken: cancellationToken);

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

    #endregion

    #region Dispose

    protected override void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _communicationLock.Dispose();
            }
            _hidSharpDevice = null;
        }
        base.Dispose(disposing);
    }

    #endregion
}
