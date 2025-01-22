using HidSharp;
using System;
using System.Collections.Generic;
using System.Text.Json;
using JJManager.Class.App;
using HIDClass = JJManager.Class.Devices.Connections.HID;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace JJManager.Class.Devices
{
    internal class JJB01_V2 : HIDClass
    {
        private SimHubWebsocket _simHubSync = null;

        public JJB01_V2(HidDevice hidDevice) : base(hidDevice)
        {
            _actionSendingData = () => { Task.Run(async () => await ActionSendingData()); };
        }

        private async Task ActionSendingData()
        {
            while (_isConnected)
            {
                await SendData();
            }
        }

        public async Task SendData()
        {
            bool forceDisconnection = false;

            try
            {
                string messageToSend = null;

                if (_profile.NeedsUpdate)
                {
                    _profile.Restart();
                    _profile.NeedsUpdate = false;
                }

                int led_mode_value = (_profile.Data.ContainsKey("led_mode") ? _profile.Data["led_mode"].GetValue<int>() : 0);
                int brightness_limit_value = (_profile.Data.ContainsKey("brightness") ? _profile.Data["brightness"].GetValue<int>() : 0);

                // 4 = it's SimHub Sync!
                if (led_mode_value == 4)
                {
                    if (_simHubSync == null)
                    {
                        _simHubSync = new SimHubWebsocket(2920, "JJB01V2_" + _connId);
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
                        100
                    );

                    if (!success)
                    {
                        throw new WebSocketException("Falha ao fazer requisição ao plugin 'JJManager Sync' do SimHub");
                    }
                    else
                    {
                        _simHubSync.TranslateToButtonBoxHID(message, out messageToSend, "jjb01v2", brightness_limit_value);
                    }
                }
                else
                {
                    if (_simHubSync != null)
                    {
                        _simHubSync.StopCommunication();
                        _simHubSync = null;
                    }

                    var data = new Dictionary<string, object>
                {
                    { "jjb01v2_data", _profile.Data }
                };
                    messageToSend = JsonSerializer.Serialize(data);
                }

                await SendHIDData(messageToSend, false, 100).ContinueWith((result) =>
                {
                    if (!result.Result)
                    {
                        throw new Exception($"Falha ao enviar '{messageToSend}' via HID para a button box JJB-01 V2 de ID '{_connId}'");
                    }
                });
            }
            catch (WebSocketException) 
            {
                // Called when SimHub connection fails, without log.
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
