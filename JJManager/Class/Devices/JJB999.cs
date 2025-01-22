using HidSharp;
using System;
using System.Collections.Generic;
using System.Text.Json;
using JJManager.Class.App;
using HIDClass = JJManager.Class.Devices.Connections.HID;
using System.Text.Json.Nodes;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace JJManager.Class.Devices
{
    internal class JJB999 : HIDClass
    {
        private SimHubWebsocket _simHubSync = null;
        private int _sendingAttemptsLimit = 10;
        private int _actualSendingAttempts = 0;
        private bool _sending = false;

        public JJB999(HidDevice hidDevice) : base(hidDevice)
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
            if (_sending)
            {
                return;
            }

            bool forceDisconnection = false;
            bool sended = false;

            try
            {
                _sending = true;
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
                        _simHubSync = new SimHubWebsocket(2920, "JJB999_" + _connId);
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
                        _simHubSync.TranslateToButtonBoxHID(message, out messageToSend, "jjb999", brightness_limit_value);
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
                    { "jjb999_data", _profile.Data }
                };
                    messageToSend = JsonSerializer.Serialize(data);
                }

                for (int i = 0; i < _sendingAttemptsLimit; i++)
                {
                    await SendHIDData(messageToSend, false, 100).ContinueWith((result) =>
                    {
                        sended = result.Result;
                    });

                    if (sended)
                    {
                        break;
                    }
                    else
                    {
                        await Task.Delay(50);
                    }
                }

                if (!sended)
                {
                    throw new Exception($"Falha ao enviar '{messageToSend}' via HID para a button box JJB-999 de ID '{_connId}'");
                }
            }
            catch (WebSocketException)
            {
                // Called when SimHub connection fails, without log.
                forceDisconnection = true;
            }
            catch (Exception ex)
            {
                Log.Insert("JJB999", "Ocorreu um problema ao enviar dados para a sua button box", ex);
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
