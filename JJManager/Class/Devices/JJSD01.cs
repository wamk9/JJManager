using HidSharp;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using HIDClass = JJManager.Class.Devices.Connections.HID;
using System.Net.WebSockets;
using System;

namespace JJManager.Class.Devices
{
    internal class JJSD01 : HIDClass
    {
        private bool _requesting = false;

        public JJSD01(HidDevice hidDevice) : base(hidDevice)
        {
            RestartClass();
        }
        private void RestartClass()
        {
            _actionReceivingData = () => { Task.Run(async () => await ActionReceivingData()); };
            _actionResetParams = () => { Task.Run(() => RestartClass()); };
        }

        private async Task ActionReceivingData()
        {
            while (_isConnected)
            {
                await RequestData();
            }
        }

        private async Task RequestData()
        {
            if (_requesting)
            {
                return;
            }

            bool forceDisconnection = false;

            try
            {
                _requesting = true;

                if (_profile.NeedsUpdate)
                {
                    _profile.Restart();
                    _profile.NeedsUpdate = false;
                }

                JsonArray jsonArray = new JsonArray();

                for (int i = 0; i < _profile.Inputs.Count; i++)
                {
                    string messageToSend = new JsonObject
                        {
                            { "data", new JsonObject
                                {
                                    { "connection", "ok" },
                                }
                            }
                        }.ToJsonString();

                    await RequestHIDData(messageToSend, false, 100).ContinueWith((result) =>
                    {
                        if (!string.IsNullOrEmpty(result.Result))
                        {
                            JsonObject json = JsonObject.Parse(result.Result).AsObject();

                            if (json.ContainsKey("data") && json["data"] is JsonObject dataObject)
                            {
                                if (dataObject.ContainsKey("key_pressed"))
                                {
                                    _profile.Inputs[dataObject["key_pressed"].GetValue<int>()].Execute();
                                }
                                else if (dataObject.ContainsKey("key_released"))
                                {

                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Insert("JJSD01", "Ocorreu um problema ao enviar dados para a sua streamdeck", ex);
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
    }
}
