using HidSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using JJManager.Class.App;
using HIDClass = JJManager.Class.Devices.Connections.HID;
using HIDMessage = JJManager.Class.Devices.Connections.HIDMessage;
using OutputClass = JJManager.Class.App.Output.Output;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Text;

namespace JJManager.Class.Devices
{
    internal class JJB01_V2 : HIDClass
    {
        private SimHubWebsocket _simHubSync = null;
        private int _lastSentLedMode = -1;      // Track last sent LED mode (-1 = never sent)
        private int _lastSentBrightness = -1;   // Track last sent brightness
        private int _lastSentBlinkSpeed = -1;   // Track last sent blink speed
        private int _lastSentPulseDelay = -1;   // Track last sent pulse delay
        private DateTime _lastLedModeKeepAlive = DateTime.MinValue;  // Keep-alive timer for LED mode
        private readonly TimeSpan _keepAliveInterval = TimeSpan.FromSeconds(3);  // Send every 3 seconds

        public JJB01_V2(HidDevice hidDevice) : base(hidDevice)
        {
            _reportId = 0x00;

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
            _lastSentLedMode = -1;
            _lastSentBrightness = -1;
            _lastSentBlinkSpeed = -1;
            _lastSentPulseDelay = -1;
            _lastLedModeKeepAlive = DateTime.MinValue;

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
            bool forceDisconnection = false;

            try
            {
                if (_profile.NeedsUpdate)
                {
                    _profile.Restart();
                    _profile.NeedsUpdate = false;
                }

                int ledMode = (_profile.Data.ContainsKey("led_mode") ? _profile.Data["led_mode"].GetValue<int>() : 0);
                int brightness = (_profile.Data.ContainsKey("brightness") ? _profile.Data["brightness"].GetValue<int>() : 100);
                int blinkSpeed = (_profile.Data.ContainsKey("blink_speed") ? _profile.Data["blink_speed"].GetValue<int>() : 50);
                int pulseDelay = (_profile.Data.ContainsKey("pulse_delay") ? _profile.Data["pulse_delay"].GetValue<int>() : 50);

                // 4 = it's SimHub Sync!
                if (ledMode == 4)
                {
                    if (_simHubSync == null)
                    {
                        _simHubSync = new SimHubWebsocket(2920, "JJB01V2_" + _connId);
                    }

                    if (!_simHubSync.IsConnected)
                    {
                        _simHubSync.StartCommunication();
                    }

                    // Request to update data
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

                    // Use LastValues (already updated by the request)
                    JsonObject lastData = _simHubSync.LastValues ?? new JsonObject();

                    // For JJB01_V2: only 1 LED (LED 0)
                    // Find the last active output (reverse iteration) that controls LED 0
                    int targetLedIndex = 0;  // JJB01_V2 has only 1 LED (index 0)
                    OutputClass activeOutputForLed = null;

                    // Get only LED outputs (filter first to optimize)
                    var ledOutputs = _profile.Outputs
                        .Where(o => o.Mode == OutputClass.OutputMode.Leds && o.Led != null)
                        .OrderBy(o => o.Led.Order)
                        .ToList();

                    // Iterate LED outputs from last to first (reverse order)
                    for (int i = ledOutputs.Count - 1; i >= 0; i--)
                    {
                        var output = ledOutputs[i];
                        string property = output.Led.Property;

                        if (lastData.ContainsKey(property))
                        {
                            dynamic value = lastData[property]?.GetValue<dynamic>() ?? false;
                            bool isActive = output.Led.SetActivatedValue(value);

                            if (isActive && output.Led.LedsGrouped != null && output.Led.LedsGrouped.Contains(targetLedIndex))
                            {
                                activeOutputForLed = output;
                                break;  // Found the last active output for this LED
                            }
                        }
                    }

                    // Use active output configuration or turn off if no output is active
                    int activeLedMode = activeOutputForLed?.Led.ModeIfEnabled ?? 0;

                    // Prepare list of messages to send
                    List<HIDMessage> messages = new List<HIDMessage>();

                    // Keep-alive: Send LED Mode if changed OR every 3 seconds
                    bool shouldSendLedMode = _lastSentLedMode != activeLedMode ||
                                           (DateTime.Now - _lastLedModeKeepAlive) >= _keepAliveInterval;

                    if (shouldSendLedMode)
                    {
                        Console.WriteLine(activeLedMode);
                        messages.Add(new HIDMessage(0x0001, (byte)activeLedMode));
                    }

                    // Send Brightness only if changed
                    if (_lastSentBrightness != brightness)
                    {
                        messages.Add(new HIDMessage(0x0002, (byte)brightness));
                    }

                    // Send Blink Speed only if changed
                    if (_lastSentBlinkSpeed != blinkSpeed)
                    {
                        messages.Add(new HIDMessage(0x0003, (byte)blinkSpeed));
                    }

                    // Send Pulse Delay only if changed
                    if (_lastSentPulseDelay != pulseDelay)
                    {
                        messages.Add(new HIDMessage(0x0004, (byte)pulseDelay));
                    }

                    // Send all messages at once
                    if (messages.Count > 0)
                    {
                        await SendHIDBytes(messages, false, 0, 2000, 5);

                        // Update tracking variables
                        if (shouldSendLedMode)
                        {
                            _lastSentLedMode = activeLedMode;
                            _lastLedModeKeepAlive = DateTime.Now;
                        }
                        if (_lastSentBrightness != brightness) _lastSentBrightness = brightness;
                        if (_lastSentBlinkSpeed != blinkSpeed) _lastSentBlinkSpeed = blinkSpeed;
                        if (_lastSentPulseDelay != pulseDelay) _lastSentPulseDelay = pulseDelay;
                    }
                }
                else
                {
                    if (_simHubSync != null)
                    {
                        _simHubSync.StopCommunication();
                        _simHubSync = null;
                    }

                    // Prepare list of messages to send
                    List<HIDMessage> messages = new List<HIDMessage>();

                    // Keep-alive: Send LED Mode if changed OR every 3 seconds
                    bool shouldSendLedMode = _lastSentLedMode != ledMode ||
                                           (DateTime.Now - _lastLedModeKeepAlive) >= _keepAliveInterval;

                    if (shouldSendLedMode)
                    {
                        messages.Add(new HIDMessage(0x0001, (byte)ledMode));
                    }

                    // Send Brightness only if changed
                    if (_lastSentBrightness != brightness)
                    {
                        messages.Add(new HIDMessage(0x0002, (byte)brightness));
                    }

                    // Send Blink Speed only if changed
                    if (_lastSentBlinkSpeed != blinkSpeed)
                    {
                        messages.Add(new HIDMessage(0x0003, (byte)blinkSpeed));
                    }

                    // Send Pulse Delay only if changed
                    if (_lastSentPulseDelay != pulseDelay)
                    {
                        messages.Add(new HIDMessage(0x0004, (byte)pulseDelay));
                    }

                    // Send all messages at once
                    if (messages.Count > 0)
                    {
                        await SendHIDBytes(messages, false, 0, 2000, 5);

                        // Update tracking variables
                        if (shouldSendLedMode)
                        {
                            _lastSentLedMode = ledMode;
                            _lastLedModeKeepAlive = DateTime.Now;
                        }
                        if (_lastSentBrightness != brightness) _lastSentBrightness = brightness;
                        if (_lastSentBlinkSpeed != blinkSpeed) _lastSentBlinkSpeed = blinkSpeed;
                        if (_lastSentPulseDelay != pulseDelay) _lastSentPulseDelay = pulseDelay;
                    }
                }
            }
            catch (WebSocketException ex)
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
                Log.Insert("JJB01V2", "Ocorreu um problema ao enviar dados para a sua button box", ex);
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
