using HidSharp;
using JJManager.Class.App;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using HIDClass = JJManager.Class.Devices.Connections.HID;

namespace JJManager.Class.Devices
{
    public class JJDB01 : HIDClass
    {
        private SimHubWebsocket _simHubSync = null;
        private int HIDInfoLimit = 64 * 10;

        public JJDB01(HidDevice hidDevice) : base(hidDevice)
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
                List<JsonObject> messagesToSend = new List<JsonObject>();
                bool result = false;

                if (_profile.NeedsUpdate)
                {
                    _profile.Restart();
                    _profile.NeedsUpdate = false;
                }

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
                        "request", new JsonArray
                            {
                                { "SimHubLastData" }
                            }
                        }
                    }.ToJsonString(), 200
                );

                if (!success)
                {
                    throw new WebSocketException("Falha ao fazer requisição ao plugin 'JJManager Sync' do SimHub");
                }
                else
                {
                    _simHubSync.TranslateToDashboardHID(message, out messagesToSend, HIDInfoLimit);
                }

                foreach (JsonObject json in messagesToSend)
                {
                    result = await SendHIDData(json.ToJsonString(), false, 0);

                    if (!result)
                    {
                        throw new Exception($"Falha ao enviar '{json.ToJsonString()}' via HID para o dashboard JJDB-01 de ID '{_connId}'");
                    }
                }
            }
            catch (WebSocketException)
            {
                // Called when SimHub connection fails, without log.
                forceDisconnection = true;
            }
            catch (Exception ex)
            {
                Log.Insert("JJDB01", "Ocorreu um problema ao enviar dados para a seu dashboard", ex);
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
