using JJManager.Core.Devices.Abstractions;
using JJManager.Core.Devices.Connections;
using JJManager.Core.Interfaces.Repositories;
using System.Diagnostics;

using HidSharpDevice = HidSharp.HidDevice;

namespace JJManager.Core.Devices.JJLC01
{
    /// <summary>
    /// JJLC-01 LoadCell device implementation
    /// Supports calibration curve and fine offset configuration
    /// </summary>
    public class JJLC01Device : HidDevice
    {
        #region Constants

        private const int CONNECTION_TIMEOUT_LIMIT = 2;
        private const int LOOP_DELAY_MS = 50;  // 20 iterations/second

        // Commands
        private const ushort CMD_SET_FINE_OFFSET = 0x0001;
        private const ushort CMD_SET_ADC_POINTS = 0x0002;
        private const ushort CMD_CALIBRATE = 0x0003;
        private const ushort CMD_REQUEST_DATA = 0x0004;
        private const ushort CMD_DEVICE_INFO = 0x00FF;

        #endregion

        #region Fields

        private int _connectionTimeoutCount = 0;
        private volatile bool _calibrationRequested = false;
        private volatile bool _configurationNeedsUpdate = false;

        // Tracking for change detection (avoid redundant sends)
        private int _lastSentFineOffset = -1;
        private double[]? _lastSentAdcPoints = null;

        #endregion

        #region Properties

        /// <summary>
        /// Current potentiometer percentage (0-100)
        /// </summary>
        public int PotPercent { get; private set; }

        /// <summary>
        /// Current kilograms pressed on the load cell
        /// </summary>
        public float KgPressed { get; private set; }

        /// <summary>
        /// Raw ADC value from the load cell
        /// </summary>
        public short RawValue { get; private set; }

        /// <summary>
        /// Fine offset value (0-255, with 127 being the center/zero point)
        /// </summary>
        public int FineOffset { get; private set; } = 127;

        /// <summary>
        /// Icon name for LoadCell (Controller)
        /// </summary>
        public override string IconName => "Controller";

        /// <summary>
        /// Device initials for LoadCell
        /// </summary>
        public override string DeviceInitials => "LC";

        /// <summary>
        /// Device class name for LoadCell
        /// </summary>
        public override string DeviceClassName => "JJLC01";

        /// <summary>
        /// ADC calibration curve (11 points from 0% to 100%)
        /// </summary>
        public double[] AdcCurve { get; private set; } = new double[11];

        #endregion

        #region Events

        /// <summary>
        /// Raised when real-time data is updated from the device
        /// </summary>
        public event EventHandler<JJLC01DataEventArgs>? DataUpdated;

        #endregion

        #region Constructor

        public JJLC01Device(HidSharpDevice hidSharpDevice, IProductRepository? productRepository = null)
            : base(hidSharpDevice, productRepository)
        {
            _supportedCommands = new HashSet<ushort>
            {
                CMD_SET_FINE_OFFSET,
                CMD_SET_ADC_POINTS,
                CMD_CALIBRATE,
                CMD_REQUEST_DATA,
                CMD_DEVICE_INFO
            };

            // Initialize default ADC curve (linear: 0, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40)
            for (int i = 0; i < 11; i++)
            {
                AdcCurve[i] = i * 4;
            }

            
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Request calibration of the load cell (sets zero point)
        /// </summary>
        public void RequestCalibration()
        {
            _calibrationRequested = true;
        }

        /// <summary>
        /// Update fine offset and mark for sending to device
        /// </summary>
        public void SetFineOffset(int offset)
        {
            // Clamp value to valid range
            offset = Math.Clamp(offset, 0, 255);

            if (offset == _lastSentFineOffset)
            {
                return;
            }

            FineOffset = offset;
            _configurationNeedsUpdate = true;
            _profile?.MarkAsNeedsUpdate();
        }

        /// <summary>
        /// Update ADC curve points and mark for sending to device
        /// </summary>
        public void SetAdcCurve(double[] points)
        {
            if (points.Length != 11)
                throw new ArgumentException("ADC curve must have exactly 11 points", nameof(points));

            Array.Copy(points, AdcCurve, 11);
            _configurationNeedsUpdate = true;
            _profile?.MarkAsNeedsUpdate();
        }

        #endregion

        #region Communication Loops
        protected override async Task DataLoopAsync(CancellationToken cancellationToken)
        {
            // Request initial data to populate ADC curve and fine offset
            try
            {
                bool initialSuccess = await RequestDataAsync(true, cancellationToken);
                if (initialSuccess)
                {
                    _connectionTimeoutCount = 0;
                    Debug.WriteLine("JJLC01 Initial data request successful");
                }
                else
                {
                    Debug.WriteLine("JJLC01 Initial data request returned invalid data");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"JJLC01 Initial data request failed: {ex.Message}");
                // Continue anyway - we'll retry in the loop
            }

            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                try
                {
                    // Request real-time data from device
                    bool success = await RequestDataAsync(false, cancellationToken);

                    if (success)
                    {
                        _connectionTimeoutCount = 0;  // Reset timeout count on success

                        try
                        {
                            // Handle calibration request
                            if (_calibrationRequested)
                            {
                                await SendCalibrateCommandAsync(cancellationToken);
                                _calibrationRequested = false;
                            }

                            // Check if configuration needs update (user changed settings in UI)
                            // This works independently of profile to ensure real-time updates
                            if (_configurationNeedsUpdate || _profile?.NeedsUpdate == true)
                            {
                                await SendConfigurationAsync(cancellationToken);
                                _configurationNeedsUpdate = false;
                                _profile?.ClearNeedsUpdate();
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"JJLC01 SendDataLoop error: {ex.Message}");
                        }
                    }
                    else
                    {
                        _connectionTimeoutCount++;
                        Debug.WriteLine($"JJLC01 Connection timeout count: {_connectionTimeoutCount}/{CONNECTION_TIMEOUT_LIMIT}");

                        if (_connectionTimeoutCount >= CONNECTION_TIMEOUT_LIMIT)
                        {
                            Debug.WriteLine("JJLC01 Connection timeout limit reached, disconnecting...");
                            await DisconnectAsync(cancellationToken);
                            break;
                        }
                    }

                    await Task.Delay(LOOP_DELAY_MS, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"JJLC01 ReceiveDataLoop error: {ex.Message}");
                    _connectionTimeoutCount++;

                    if (_connectionTimeoutCount >= CONNECTION_TIMEOUT_LIMIT)
                    {
                        Debug.WriteLine("JJLC01 Connection timeout limit reached after error, disconnecting...");
                        await DisconnectAsync(cancellationToken);
                        break;
                    }

                    // Small delay before retry to avoid hammering the device
                    await Task.Delay(100, cancellationToken);
                }
            }
        }

        #endregion

        #region Private Methods - Sending

        private async Task SendConfigurationAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Send Fine Offset (only if changed)
                if (_lastSentFineOffset != FineOffset)
                {
                    var fineOffsetMsg = new HidMessage(CMD_SET_FINE_OFFSET, (byte)FineOffset);
                    await SendMessageAsync(fineOffsetMsg, cancellationToken: cancellationToken);
                    _lastSentFineOffset = FineOffset;
                    await Task.Delay(20, cancellationToken);
                }

                // Send ADC Points (only if changed)
                bool adcChanged = _lastSentAdcPoints == null || !AdcCurve.SequenceEqual(_lastSentAdcPoints);
                if (adcChanged)
                {
                    // Build ADC payload: 11 floats = 44 bytes
                    var adcPayload = new byte[44];
                    for (int i = 0; i < 11; i++)
                    {
                        var floatBytes = BitConverter.GetBytes((float)AdcCurve[i]);
                        Array.Copy(floatBytes, 0, adcPayload, i * 4, 4);
                    }

                    var adcMsg = new HidMessage(CMD_SET_ADC_POINTS, adcPayload);
                    await SendMessageAsync(adcMsg, cancellationToken: cancellationToken);
                    _lastSentAdcPoints = (double[])AdcCurve.Clone();
                    await Task.Delay(20, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"JJLC01 SendConfiguration error: {ex.Message}");
            }
        }

        private async Task SendCalibrateCommandAsync(CancellationToken cancellationToken)
        {
            try
            {
                var calibrateMsg = new HidMessage(CMD_CALIBRATE);
                await SendMessageAsync(calibrateMsg, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"JJLC01 Calibrate error: {ex.Message}");
            }
        }

        #endregion

        #region Private Methods - Receiving

        private async Task<bool> RequestDataAsync(bool isInitialRequest, CancellationToken cancellationToken)
        {
            try
            {
                if (isInitialRequest)
                {
                    // Initial Delay to request informations...
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }

                // Request all data (request type 0x06)
                var requestMsg = new HidMessage(CMD_REQUEST_DATA, (byte)0x06);
                var (success, response) = await RequestAsync(requestMsg, timeoutMs: 2000, cancellationToken: cancellationToken);

                // Response payload: pot_percent(1) + kg_pressed(4) + raw(2) + fine_offset(1) + adc[11](44) = 52 bytes
                if (success && response.Length >= 52)
                {
                    ParseDataResponse(response, isInitialRequest);
                    return true;  // Success
                }
                else
                {
                    Debug.WriteLine($"JJLC01 RequestData: Invalid response (success={success}, length={response?.Length ?? 0})");
                    return false;  // Failed but no exception
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"JJLC01 RequestData error: {ex.Message}");
                throw;  // Re-throw so ReceiveDataLoopAsync can handle the error
            }
        }

        private void ParseDataResponse(byte[] response, bool isInitialRequest)
        {
            int offset = 0;

            // pot_percent (1 byte)
            PotPercent = response[offset++];

            // kg_pressed (4 bytes - float)
            KgPressed = BitConverter.ToSingle(response, offset);
            offset += 4;

            // raw (2 bytes - int16)
            RawValue = BitConverter.ToInt16(response, offset);
            offset += 2;

            // fine_offset (1 byte)
            int receivedFineOffset = response[offset++];

            // adc array (44 bytes - 11 floats)
            var receivedAdcCurve = new double[11];
            for (int i = 0; i < 11; i++)
            {
                receivedAdcCurve[i] = BitConverter.ToSingle(response, offset);
                offset += 4;
            }

            // On initial request, update our local values from firmware
            if (isInitialRequest)
            {
                FineOffset = receivedFineOffset;
                _lastSentFineOffset = receivedFineOffset;
                Array.Copy(receivedAdcCurve, AdcCurve, 11);
                _lastSentAdcPoints = (double[])receivedAdcCurve.Clone();
                Debug.WriteLine($"JJLC01 Initial data: Pot={PotPercent}%, Kg={KgPressed:F1}, Raw={RawValue}, FineOffset={receivedFineOffset}");
            }

            // Raise event with updated data
            DataUpdated?.Invoke(this, new JJLC01DataEventArgs
            {
                PotPercent = PotPercent,
                KgPressed = KgPressed,
                RawValue = RawValue,
                FineOffset = receivedFineOffset,
                AdcCurve = receivedAdcCurve,
                IsInitialData = isInitialRequest
            });
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

        #region Private Methods - Handshake / Probe
        private async Task SendInitialHandshakeAsync(
            CancellationToken ct)
        {
            var msg = new HidMessage(CMD_REQUEST_DATA, 0x00);
            await RequestAsync(msg, cancellationToken: ct);
            await Task.Delay(20, ct);
        }
        #endregion
    }

    /// <summary>
    /// Event args for JJLC01 data updates
    /// </summary>
    public class JJLC01DataEventArgs : EventArgs
    {
        public int PotPercent { get; init; }
        public float KgPressed { get; init; }
        public short RawValue { get; init; }
        public int FineOffset { get; init; }
        public double[] AdcCurve { get; init; } = Array.Empty<double>();
        public bool IsInitialData { get; init; }
    }
}