using HidSharp;
using System;
using System.Collections.Generic;
using System.Text.Json;
using JJManager.Class.App;
using HIDClass = JJManager.Class.Devices.Connections.HID;
using HIDMessage = JJManager.Class.Devices.Connections.HIDMessage;
using OutputClass = JJManager.Class.App.Output.Output;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Net.WebSockets;
using JJManager.Class.App.Output.Leds;
using JJManager.Class.App.Output;
using System.Linq;

namespace JJManager.Class.Devices
{
    internal class JJBSlim_A : HIDClass
    {
        private SimHubWebsocket _simHubSync = null;
        private bool _sending = false;

        // Keep-alive tracking
        private DateTime _lastKeepAlive = DateTime.MinValue;
        private readonly TimeSpan _keepAliveInterval = TimeSpan.FromSeconds(3);

        // Change tracking variables (to avoid sending unchanged data)
        // Array for 8 LEDs (indices 0-7)
        private int[] _lastSentLedModes = new int[8] { -1, -1, -1, -1, -1, -1, -1, -1 };  // -1 = never sent
        private int _lastSentBrightness = -1;
        private int _lastSentBlinkSpeed = -1;
        private int _lastSentPulseDelay = -1;

        public JJBSlim_A(HidDevice hidDevice) : base(hidDevice)
        {
            _cmds = new HashSet<ushort>
            {
                0x0001, // LED Mode (changed from 0x0000)
                0x0002, // Brightness (changed from 0x0001)
                0x0003, // Blink Speed (changed from 0x0002)
                0x0004, // Pulse Delay (changed from 0x0003)
                0x00FF  // Device Info
            };

            RestartClass();
        }

        private void RestartClass()
        {
            // Reset tracking variables to force resend of all parameters
            for (int i = 0; i < 8; i++)
            {
                _lastSentLedModes[i] = -1;
            }
            _lastSentBrightness = -1;
            _lastSentBlinkSpeed = -1;
            _lastSentPulseDelay = -1;
            _lastKeepAlive = DateTime.MinValue;

            _actionSendingData = () => { Task.Run(async () => await ActionSendingData()); };
            _actionResetParams = () => { Task.Run(() => RestartClass()); };
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
            if (_sending)
            {
                return;
            }

            bool forceDisconnection = false;

            try
            {
                _sending = true;

                if (_profile.NeedsUpdate)
                {
                    _profile.Restart();
                    _profile.NeedsUpdate = false;
                }

                int brightness = (_profile.Data.ContainsKey("brightness") ? _profile.Data["brightness"].GetValue<int>() : 100);
                int blinkSpeed = (_profile.Data.ContainsKey("blink_speed") ? _profile.Data["blink_speed"].GetValue<int>() : 50);
                int pulseDelay = (_profile.Data.ContainsKey("pulse_delay") ? _profile.Data["pulse_delay"].GetValue<int>() : 50);
                
                // Send if changed
                bool shouldSendBrightness = _lastSentBrightness != brightness;
                bool shouldSendPulseDelay = _lastSentPulseDelay != pulseDelay;
                bool shouldSendBlinkSpeed = _lastSentBlinkSpeed != blinkSpeed;

                // Prepare list of messages to send
                List<HIDMessage> messages = new List<HIDMessage>();

                // JJBSlim_A has 8 LEDs (indices 0-7)
                // Determine active mode for each LED based on outputs
                int[] activeLedModes = new int[8];
                int[] ledModeArray = _profile.Data.ContainsKey("led_mode") ? _profile.Data["led_mode"].AsArray().GetValues<int>().ToArray() : new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };

                if (ledModeArray.Contains(4))
                {
                    // At least one LED is in "Property-Based" mode
                    // Ensure SimHub connection is established
                    if (_simHubSync == null)
                    {
                        _simHubSync = new SimHubWebsocket(2920, "JJBSlim_A_" + _connId);
                    }
                    if (!_simHubSync.IsConnected)
                    {
                        _simHubSync.StartCommunication();
                    }
                }
                else
                {
                    if (_simHubSync != null)
                    {
                        _simHubSync.StopCommunication();
                        _simHubSync = null;
                    }
                }

                for (int ledIndex = 0; ledIndex < 8; ledIndex++)
                {
                    if (ledModeArray[ledIndex] == 4)
                    {
                        var (success, _) = _simHubSync.RequestMessage(
                            new JsonObject {
                                {
                                    "request", new JsonArray {
                                        { "SimHubLastData" }
                                    }
                                }
                            }.ToJsonString(),
                            100
                        );

                        if (!success)
                        {
                            throw new WebSocketException("Falha ao fazer requisição ao plugin 'JJManager Sync' do SimHub");
                        }

                        // Use cached LastValues instead of parsing response every time
                        JsonObject lastData = _simHubSync.LastValues ?? new JsonObject();

                        // Get only LED outputs (filter first to optimize)
                        var ledOutputs = _profile.Outputs
                            .Where(o => o.Mode == OutputClass.OutputMode.Leds && o.Led != null && !string.IsNullOrEmpty(o.Led.Property) && o.Led.LedsGrouped.Contains(ledIndex))
                            .ToList();

                        activeLedModes[ledIndex] = 0;

                        // Iterate LED outputs from last to first (reverse order)
                        // This ensures that the LAST active output in the list has priority
                        for (int i = ledOutputs.Count - 1; i >= 0; i--)
                        {
                            var output = ledOutputs[i];
                            string property = output.Led.Property ?? string.Empty;

                            if (lastData.ContainsKey(property))
                            {
                                dynamic value = lastData[property]?.GetValue<dynamic>() ?? false;
                                bool isActive = output.Led.SetActivatedValue(value);

                                if (isActive && output.Led.LedsGrouped != null &&
                                    output.Led.LedsGrouped.Contains(ledIndex))
                                {
                                    activeLedModes[ledIndex] = output.Led.ModeIfEnabled;
                                    break;  // Found the last active output for this LED
                                }
                            }
                        }
                    }
                    else
                    {
                        activeLedModes[ledIndex] = ledModeArray[ledIndex];
                    }

                    // Send LED Mode for each LED that changed
                    if (_lastSentLedModes[ledIndex] != activeLedModes[ledIndex] ||
                                            (DateTime.Now - _lastKeepAlive) >= _keepAliveInterval)
                    {
                        // Create message: CMD=0x0001, Payload=[LED_INDEX][MODE]
                        messages.Add(new HIDMessage(0x0001, new byte[] { (byte)ledIndex, (byte)activeLedModes[ledIndex] }));
                    }
                }

                if (shouldSendBrightness)
                {
                    messages.Add(new HIDMessage(0x0002, (byte)brightness));
                }

                if (shouldSendBlinkSpeed)
                {
                    messages.Add(new HIDMessage(0x0003, (byte)blinkSpeed));
                }

                if (shouldSendPulseDelay)
                {
                    messages.Add(new HIDMessage(0x0004, (byte)pulseDelay));
                }

                // Send all messages at once
                if (messages.Count > 0)
                {
                    await SendHIDBytes(messages, false, 0, 2000, 5);

                    // Update tracking variables
                    for (int ledIndex = 0; ledIndex < 8; ledIndex++)
                    {
                        if (_lastSentLedModes[ledIndex] != activeLedModes[ledIndex])
                        {
                            _lastSentLedModes[ledIndex] = activeLedModes[ledIndex];
                            _lastKeepAlive = DateTime.Now;
                        }
                    }

                    if (shouldSendBrightness)
                    {
                        _lastSentBrightness = brightness;
                        _lastKeepAlive = DateTime.Now;
                    }

                    if (shouldSendPulseDelay)
                    {
                        _lastSentPulseDelay = pulseDelay;
                        _lastKeepAlive = DateTime.Now;
                    }

                    if (shouldSendBlinkSpeed)
                    {
                        _lastSentBlinkSpeed = blinkSpeed;
                        _lastKeepAlive = DateTime.Now;
                    }
                }
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
                Log.Insert("JJBSlim_A", "Ocorreu um problema ao enviar dados para a sua button box", ex);
                forceDisconnection = true;
            }
            finally
            {
                if (forceDisconnection)
                {
                    Disconnect();
                }

                _sending = false;
            }
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
