using HidSharp;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using HIDClass = JJManager.Class.Devices.Connections.HID;
using System;
using System.Collections.Generic;

namespace JJManager.Class.Devices
{
    /// <summary>
    /// JJSD-01 StreamDeck Device Class
    /// 12-button StreamDeck with byte-based HID protocol
    /// Supports 3 button states: Pressed, Continuous, Hold
    /// </summary>
    internal class JJSD01 : HIDClass
    {
        // =====================================================================
        // Constants
        // =====================================================================

        private const int BUTTON_COUNT = 12;
        private const int INPUTS_PER_BUTTON = 3;  // Pressed, Continuous, Hold
        private const int TOTAL_INPUTS = BUTTON_COUNT * INPUTS_PER_BUTTON;  // 36

        // HID Commands
        private const ushort CMD_KEEP_ALIVE = 0x0001;
        private const ushort CMD_KEY_EVENT = 0x0002;
        private const ushort CMD_CONTINUOUS_TIME = 0x0003;
        private const ushort CMD_DEVICE_INFO = 0x00FF;

        // Key States
        private const byte KEY_STATE_HOLD = 0x00;
        private const byte KEY_STATE_PRESSED = 0x01;
        private const byte KEY_STATE_CONTINUOUS = 0x02;

        // Default continuous time (5 seconds)
        private const ushort DEFAULT_CONTINUOUS_TIME_MS = 5000;

        // =====================================================================
        // Instance Variables
        // =====================================================================

        private bool _requesting = false;

        // Continuous time per button (configurable)
        private ushort[] _continuousTime = new ushort[BUTTON_COUNT];

        // Track if continuous time was sent (change tracking)
        private ushort[] _lastSentContinuousTime = new ushort[BUTTON_COUNT];

        // =====================================================================
        // Constructor
        // =====================================================================

        public JJSD01(HidDevice hidDevice) : base(hidDevice)
        {
            // Initialize command set
            _cmds = new HashSet<ushort>
            {
                CMD_KEEP_ALIVE,
                CMD_KEY_EVENT,
                CMD_CONTINUOUS_TIME,
                CMD_DEVICE_INFO
            };

            RestartClass();
        }

        private void RestartClass()
        {
            // Initialize continuous times
            for (int i = 0; i < BUTTON_COUNT; i++)
            {
                _continuousTime[i] = DEFAULT_CONTINUOUS_TIME_MS;
                _lastSentContinuousTime[i] = 0;  // Force send on first connection
            }

            // Single thread for all communication (same pattern as JJM01/JJLC01)
            _actionReceivingData = () => { Task.Run(async () => await ActionMainLoop()); };
            _actionSendingData = null;  // Not used - everything in ActionMainLoop
            _actionResetParams = () => { Task.Run(() => RestartClass()); };
        }

        // =====================================================================
        // Main Communication Loop (Single Thread)
        // =====================================================================

        private async Task ActionMainLoop()
        {
            while (_isConnected)
            {
                await MainLoopIteration();
                await Task.Delay(2);  // Small delay to reduce CPU usage
            }
        }

        private async Task MainLoopIteration()
        {
            if (_requesting)
            {
                return;
            }

            bool forceDisconnection = false;

            try
            {
                _requesting = true;

                // Check if profile needs update
                if (_profile.NeedsUpdate)
                {
                    _profile.Restart();
                    _profile.NeedsUpdate = false;

                    // Load continuous times from profile data
                    LoadContinuousTimesFromProfile();

                    // Reset change tracking to force resend
                    for (int i = 0; i < BUTTON_COUNT; i++)
                    {
                        _lastSentContinuousTime[i] = 0;
                    }
                }

                // Send continuous time configurations (only if changed)
                await SendContinuousTimeConfigs();

                // Poll for key events (also serves as keep-alive)
                await PollKeyEvents();
            }
            catch (Exception ex)
            {
                Log.Insert("JJSD01", "Ocorreu um problema ao comunicar com a streamdeck", ex);
                forceDisconnection = true;
            }
            finally
            {
                if (forceDisconnection)
                {
                    Disconnect();
                }

                _requesting = false;
            }
        }

        // =====================================================================
        // Polling (Keep-Alive + Key Events)
        // =====================================================================

        private async Task PollKeyEvents()
        {
            // Send poll command and read response (which may contain key events)
            List<byte> pollData = new List<byte>
            {
                0x00, 0x01,  // CMD: Keep-Alive/Poll (0x0001)
                0x20, 0x01   // FLAGS
            };

            // Timeout de 2000ms para garantir comunicação estável (firmware timeout é 5000ms)
            // RequestHIDBytes retorna apenas o PAYLOAD (sem CMD e sem FLAGS)
            // - Se firmware responde com CMD_KEY_EVENT: payload = [keyId, state]
            // - Se firmware responde com CMD_KEEP_ALIVE: payload = [] (vazio)
            List<byte> response = await RequestHIDBytes(pollData, false, 0, 2000, 5);

            // Response contém apenas payload: [keyId][state] para key event, ou vazio para keep-alive
            if (response != null && response.Count >= 2)
            {
                byte keyId = response[0];
                byte state = response[1];

                ProcessKeyEvent(keyId, state);
            }
            // Se response está vazio ou tem menos de 2 bytes = keep-alive (sem eventos)
        }

        // =====================================================================
        // Continuous Time Configuration
        // =====================================================================

        private void LoadContinuousTimesFromProfile()
        {
            if (_profile.Data == null) return;

            for (int i = 0; i < BUTTON_COUNT; i++)
            {
                string key = $"continuous_time_{i}";
                if (_profile.Data.ContainsKey(key))
                {
                    try
                    {
                        _continuousTime[i] = (ushort)_profile.Data[key].GetValue<int>();
                    }
                    catch
                    {
                        _continuousTime[i] = DEFAULT_CONTINUOUS_TIME_MS;
                    }
                }
                else
                {
                    _continuousTime[i] = DEFAULT_CONTINUOUS_TIME_MS;
                }
            }
        }

        private async Task SendContinuousTimeConfigs()
        {
            for (int i = 0; i < BUTTON_COUNT; i++)
            {
                // Only send if changed
                if (_continuousTime[i] != _lastSentContinuousTime[i])
                {
                    await SendContinuousTime((byte)i, _continuousTime[i]);
                    _lastSentContinuousTime[i] = _continuousTime[i];
                }
            }
        }

        private async Task SendContinuousTime(byte buttonId, ushort timeMs)
        {
            List<byte> data = new List<byte>
            {
                0x00, 0x03,                      // CMD: Continuous Time (0x0003)
                buttonId,                         // Button ID
                (byte)(timeMs >> 8),             // Time MS High byte
                (byte)(timeMs & 0xFF),           // Time MS Low byte
                0x20, 0x03                       // FLAGS
            };

            await SendHIDBytes(data, false, 0, 2000, 5);
        }

        /// <summary>
        /// Set continuous time for a specific button
        /// </summary>
        /// <param name="buttonId">Button ID (0-11)</param>
        /// <param name="timeMs">Time in milliseconds</param>
        public void SetContinuousTime(int buttonId, ushort timeMs)
        {
            if (buttonId >= 0 && buttonId < BUTTON_COUNT)
            {
                _continuousTime[buttonId] = timeMs;

                // Save to profile data
                if (_profile.Data != null)
                {
                    _profile.Data[$"continuous_time_{buttonId}"] = timeMs;
                }
            }
        }

        /// <summary>
        /// Get continuous time for a specific button
        /// </summary>
        /// <param name="buttonId">Button ID (0-11)</param>
        /// <returns>Time in milliseconds</returns>
        public ushort GetContinuousTime(int buttonId)
        {
            if (buttonId >= 0 && buttonId < BUTTON_COUNT)
            {
                return _continuousTime[buttonId];
            }
            return DEFAULT_CONTINUOUS_TIME_MS;
        }

        // =====================================================================
        // Key Event Processing
        // =====================================================================

        private void ProcessKeyEvent(byte keyId, byte state)
        {
            if (keyId >= BUTTON_COUNT) return;

            // Calculate input index based on state
            // Inputs 0-11  = Pressed (state 0x01)
            // Inputs 12-23 = Continuous (state 0x02)
            // Inputs 24-35 = Hold (state 0x00)
            int inputIndex;

            switch (state)
            {
                case KEY_STATE_PRESSED:
                    inputIndex = keyId;  // 0-11
                    break;

                case KEY_STATE_CONTINUOUS:
                    inputIndex = keyId + BUTTON_COUNT;  // 12-23
                    break;

                case KEY_STATE_HOLD:
                    inputIndex = keyId + (BUTTON_COUNT * 2);  // 24-35
                    break;

                default:
                    return;
            }

            // Execute the input if it exists
            if (inputIndex < _profile.Inputs.Count)
            {
                _profile.Inputs[inputIndex].Execute();
            }
        }

        // =====================================================================
        // Helper Methods
        // =====================================================================

        /// <summary>
        /// Get the input index for a specific button and state
        /// </summary>
        /// <param name="buttonId">Button ID (0-11)</param>
        /// <param name="state">Key state (0=Hold, 1=Pressed, 2=Continuous)</param>
        /// <returns>Input index in profile</returns>
        public static int GetInputIndex(int buttonId, byte state)
        {
            switch (state)
            {
                case KEY_STATE_PRESSED:
                    return buttonId;
                case KEY_STATE_CONTINUOUS:
                    return buttonId + BUTTON_COUNT;
                case KEY_STATE_HOLD:
                    return buttonId + (BUTTON_COUNT * 2);
                default:
                    return buttonId;
            }
        }

        /// <summary>
        /// Get button ID and state from input index
        /// </summary>
        /// <param name="inputIndex">Input index (0-35)</param>
        /// <returns>Tuple of (buttonId, state)</returns>
        public static (int buttonId, byte state) GetButtonFromInputIndex(int inputIndex)
        {
            if (inputIndex < BUTTON_COUNT)
            {
                return (inputIndex, KEY_STATE_PRESSED);
            }
            else if (inputIndex < BUTTON_COUNT * 2)
            {
                return (inputIndex - BUTTON_COUNT, KEY_STATE_CONTINUOUS);
            }
            else
            {
                return (inputIndex - BUTTON_COUNT * 2, KEY_STATE_HOLD);
            }
        }

        /// <summary>
        /// Get state name for display
        /// </summary>
        /// <param name="state">Key state byte</param>
        /// <returns>State name string</returns>
        public static string GetStateName(byte state)
        {
            switch (state)
            {
                case KEY_STATE_PRESSED:
                    return "Pressed";
                case KEY_STATE_CONTINUOUS:
                    return "Continuous";
                case KEY_STATE_HOLD:
                    return "Hold";
                default:
                    return "Unknown";
            }
        }
    }
}
