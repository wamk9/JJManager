using HidSharp;
using JJManager.Class.App;
using JJManager.Class.App.Output;
using JJManager.Class.App.Output.Leds;
using JJManager.Class.Devices.Connections;
using JJManager.Properties;
using Newtonsoft.Json.Linq;
using STBootLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Markup;
using static JJManager.Class.App.Output.Leds.Leds;
using HIDClass = JJManager.Class.Devices.Connections.HID;

namespace JJManager.Class.Devices
{
    public class JJDB01 : HIDClass
    {
        private SimHubWebsocket _simHubSync = null;
        private int HIDInfoLimit = 64 * 8 - 1;
        private JsonObject _requestedData = null;
        private readonly int _connectionTimeoutLimit = 10;
        private int _actualConnectionTimeout = 0;
        private string _dashboardId = "default";
        private int _dashboardActualPage = -1;
        private bool _sendAllDashData = false;
        private uint _qtdPassedAllData = 0;
        private uint _limitPassedAllData = 1;
        private bool _lastGameRunningState = false;
        private JsonArray _DashboardPagesVariables = null;
        private Channel<(JsonObject, JsonObject)> _incomingQueue;
        private CancellationTokenSource _cts;
        private Task _pollTask;
        private Task _processTask;
        private Task _sendTask;
        private List<string> jsonsToSend = new List<string>();

        // Keep-alive tracking
        private DateTime _lastKeepAlive = DateTime.MinValue;
        private readonly TimeSpan _keepAliveInterval = TimeSpan.FromSeconds(3);

        // Change tracking - LEDs (16 LEDs x 3 bytes RGB)
        private byte[] _lastSentLedColors = new byte[16 * 3];
        private bool _ledsNeedUpdate = true;

        // Change tracking - Global settings
        private int _lastSentBrightness = -1;


        public JJDB01(HidDevice hidDevice) : base(hidDevice)
        {
            _reportId = 0x00;

            _cmds = new HashSet<ushort>
            {
                0x0001, // Request/Send Dashboard Data
                0x0002, // Receive Actual Dashboard Layout
                0x0003, // Receive Actual Dashboard Page
                0x0010, // Send LED Data (1 LED: [LED_IDX][R][G][B])
                0x0011, // Set LED Brightness (0-255)
                0x0012, // Keep-Alive
                0x00FF  // Request/Send Device Info
            };

            RestartClass();
        }

        private void RestartClass()
        {
            _DashboardPagesVariables = JsonNode.Parse(Properties.Resources.JJDB01_dashboard_vars).AsArray();
            _actionSendingData = () => { Task.Run(async () => await ActionSendingData()); };
            _actionResetParams = () => { Task.Run(() => RestartClass()); };

            // Reset change tracking variables
            _lastKeepAlive = DateTime.MinValue;
            _lastSentBrightness = -1;
            Array.Clear(_lastSentLedColors, 0, _lastSentLedColors.Length);
            _ledsNeedUpdate = true;

            if (_cts != null)
            {
                _cts.Cancel();
            }

            _cts = new CancellationTokenSource();
    //        _incomingQueue = Channel.CreateBounded<(JsonObject, JsonObject)>(
    //new BoundedChannelOptions(1)
    //{
    //    SingleWriter = true,
    //    SingleReader = true,
    //    FullMode = BoundedChannelFullMode.DropOldest // keep only latest
    //});
    //        // bounded=1 → keeps only the latest data, avoids backlog

    //        _pollTask = Task.Run(async () => { while (_isConnected) { await PollSimHubPlugin(_cts.Token); } });
    //        _processTask = Task.Run(async () => { while (_isConnected) { await ProcessJJDB01Json(_cts.Token); } });
    //        _sendTask = Task.Run(async () => { while (_isConnected) { await ProcessSendQueue(_cts.Token); } });

    //        Task.WhenAll(new List<Task> { _pollTask, _processTask, _sendTask });
        }


        private List<string> GetFIFOJson()
        {
            return jsonsToSend;
        }

        private async Task ActionSendingData()
        {
            while (_isConnected)
            {
                await SendData();
                await Task.Delay(2); // Wait 2ms between iterations to reduce CPU usage
            }
        }

        public async Task SendData()
        {
            bool forceDisconnection = false;

            try
            {
                // 1. Profile update check
                if (_profile.NeedsUpdate)
                {
                    _profile.Restart();
                    _profile.NeedsUpdate = false;
                    // Force resend of all data
                    Array.Clear(_lastSentLedColors, 0, _lastSentLedColors.Length);
                    _lastSentBrightness = -1;
                    _ledsNeedUpdate = true;
                }

                // 2. SimHub connection + request data
                if (_simHubSync == null)
                {
                    _simHubSync = new SimHubWebsocket(2920, "JJDB01_" + _connId);
                }

                if (!_simHubSync.IsConnected)
                {
                    _simHubSync.StartCommunication();
                }

                var (success, message) = _simHubSync.RequestMessage(
                    new JsonObject {
                    {
                        "request", new JsonArray {
                            { "SimHubLastData" }
                        }
                    }
                    }.ToJsonString(),
                    0
                );

                if (!success)
                {
                    throw new WebSocketException("Falha ao fazer requisição ao plugin 'JJManager Sync' do SimHub");
                }

                // 3. Keep-alive (every 3 seconds)
                if ((DateTime.Now - _lastKeepAlive) >= _keepAliveInterval)
                {
                    // FLAGS are added automatically by SendHIDBytes
                    await SendHIDBytes(new List<byte> { 0x00, 0x12, 0x01 }, false, 0, 2000, 5);
                    _lastKeepAlive = DateTime.Now;
                }

                // 4. Brightness (only if changed)
                int brightness = (_profile.Data.ContainsKey("brightness") ? _profile.Data["brightness"].GetValue<int>() : 100);
                if (_lastSentBrightness != brightness)
                {
                    // FLAGS are added automatically by SendHIDBytes
                    await SendHIDBytes(new List<byte> { 0x00, 0x11, (byte)brightness }, false, 0, 2000, 5);
                    _lastSentBrightness = brightness;
                }

                // 5. LED Data (envia apenas LEDs alterados, 1 por vez)
                byte[] newColors = CalculateLedColors(_simHubSync?.LastValues);
                for (int i = 0; i < 16; i++)
                {
                    int offset = i * 3;
                    // Force send all LEDs on first connection or profile change
                    if (_ledsNeedUpdate || LedChanged(i, newColors))
                    {
                        await SendLed(i, newColors[offset], newColors[offset + 1], newColors[offset + 2]);
                        UpdateLedTracking(i, newColors);
                    }
                }
                _ledsNeedUpdate = false;










                // 6. Check if GameRunning changed from 0 to 1 (game started/resumed)
                bool currentGameRunningState = false;
                if (_simHubSync?.LastValues != null && _simHubSync.LastValues.ContainsKey("GameRunning"))
                {
                    string gameRunningValue = _simHubSync.LastValues["GameRunning"]?.GetValue<string>() ?? "0";
                    currentGameRunningState = gameRunningValue != "0";
                }

                if (!_lastGameRunningState && currentGameRunningState)
                {
                    // Game just started/resumed - force resend all data
                    _sendAllDashData = true;
                    _qtdPassedAllData = 0;
                }
                _lastGameRunningState = currentGameRunningState;

                _sendAllDashData = _sendAllDashData && _limitPassedAllData > _qtdPassedAllData;
                _qtdPassedAllData = _sendAllDashData ? _qtdPassedAllData + 1 : 0;

                // 7. Dashboard Data (existing logic - already works)
                TranslateToDeviceScreen(
                    _simHubSync?.LastValuesUpdated ?? new JsonObject(),
                    null,
                    out List<HIDMessage> translatedDashMessage,
                    HIDInfoLimit
                );

                await SendHIDBytes(translatedDashMessage, false, 5, 2000, 5);
            }
            catch (WebSocketException)
            {
                // Called when SimHub connection fails, without log.
                if (SimHubWebsocket.CancelAutoConnIfEnabled)
                {
                    AutoConnect = false;
                }

                forceDisconnection = true;
            }
            catch (Exception ex)
            {
                Log.Insert("JJDB01", "Ocorreu um problema ao enviar dados para o seu dashboard", ex);
                forceDisconnection = true;
            }
            finally
            {
                if (forceDisconnection)
                {
                    Disconnect();
                }
            }
        }

        #region LED Helper Methods

        /// <summary>
        /// Sends a single LED color to the device
        /// Format: [CMD_H=0x00][CMD_L=0x10][LED_IDX][R][G][B]
        /// Note: FLAGS are added automatically by SendHIDBytes
        /// </summary>
        private async Task SendLed(int ledIndex, byte r, byte g, byte b)
        {
            List<byte> payload = new List<byte>
            {
                (byte)ledIndex,     // LED index (0-15)
                r,                  // Red
                g,                  // Green
                b                   // Blue
            };
            await SendHIDBytes(new List<HIDMessage> { new HIDMessage(0x0010, payload.ToArray()) }, false, 5, 2000, 5);
        }

        /// <summary>
        /// Checks if a LED color has changed from the last sent value
        /// </summary>
        private bool LedChanged(int ledIndex, byte[] newColors)
        {
            int offset = ledIndex * 3;
            return _lastSentLedColors[offset] != newColors[offset] ||
                   _lastSentLedColors[offset + 1] != newColors[offset + 1] ||
                   _lastSentLedColors[offset + 2] != newColors[offset + 2];
        }

        /// <summary>
        /// Updates the tracking array for a specific LED
        /// </summary>
        private void UpdateLedTracking(int ledIndex, byte[] newColors)
        {
            int offset = ledIndex * 3;
            _lastSentLedColors[offset] = newColors[offset];
            _lastSentLedColors[offset + 1] = newColors[offset + 1];
            _lastSentLedColors[offset + 2] = newColors[offset + 2];
        }

        /// <summary>
        /// Calculates the color for all 16 LEDs based on profile outputs and SimHub data
        /// </summary>
        private byte[] CalculateLedColors(JsonObject simHubValues)
        {
            byte[] colors = new byte[16 * 3]; // 16 LEDs x 3 bytes (RGB)

            // Initialize all to black (off)
            Array.Clear(colors, 0, colors.Length);

            // Track which LEDs have been set (last active output wins)
            bool[] ledSet = new bool[16];

            // Process outputs in descending order (highest Order first, so last active wins)
            foreach (Output output in _profile.Outputs
                .Where(x => x.Led != null)
                .OrderByDescending(x => x.Led.Order))
            {
                if (string.IsNullOrEmpty(output?.Led?.Property))
                {
                    continue;
                }

                // Check if output is active based on SimHub value
                bool isActive = output?.Led?.SetActivatedValue(
                    simHubValues?[output.Led.Property]?.GetValue<dynamic>() ?? false
                );

                if (isActive)
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
                            if (ledIdx >= 0 && ledIdx < 16 && !ledSet[ledIdx])
                            {
                                int offset = ledIdx * 3;
                                colors[offset] = r;
                                colors[offset + 1] = g;
                                colors[offset + 2] = b;
                                ledSet[ledIdx] = true;
                            }
                        }
                    }
                }
            }

            return colors;
        }

        #endregion

        public bool TranslateToDeviceScreen(JsonObject messageReceived, JsonArray varsToUse, out List<HIDMessage> translatedMessage, int limitChars = 64 * 7)
        {
            translatedMessage = new List<HIDMessage>();

            if (messageReceived.Count == 0)
            {
                return true;
            }

            foreach (var kvp in messageReceived)
            {
                string propertyName = kvp.Key;
                JsonNode value = kvp.Value;

                if ((varsToUse != null && varsToUse.Any(x => x?.GetValue<string>() == propertyName)) || varsToUse == null)
                {
                    var dict = JsonNode.Parse(Resources.JJPropertyDictionary).AsObject();
                    var match = dict.FirstOrDefault(x => string.Equals(x.Value?.ToString(), propertyName, StringComparison.OrdinalIgnoreCase));

                    // SKIP if property not found in dictionary
                    if (match.Key == null)
                    {
                        Console.WriteLine($"[JJDB01] Property NOT in dictionary: '{propertyName}' = {value}");
                        continue;  // Ignore properties not in JJPropertyDictionary
                    }

                    // parse hex like "0x0001"
                    ushort cmdJJProperty = Convert.ToUInt16(match.Key, 16);
                    ushort cmdDashboardRequest = 0x0001;
                    byte[] message = System.Text.Encoding.ASCII.GetBytes(value?.DeepClone().ToString().Trim());

                    // Build payload: [PROP_ID_H][PROP_ID_L][Data...]
                    List<byte> payload = new List<byte>
                    {
                        (byte)(cmdJJProperty >> 8),    // Property ID High byte
                        (byte)(cmdJJProperty & 0xFF)   // Property ID Low byte
                    };
                    payload.AddRange(message);

                    translatedMessage.Add(new HIDMessage(cmdDashboardRequest, payload));
                }
            }

            return true;
        }

        public override bool Disconnect()
        {
            if (_simHubSync != null)
            {
                _simHubSync.StopCommunication();
                _simHubSync = null;
            }

            return base.Disconnect();
        }
    }
}
