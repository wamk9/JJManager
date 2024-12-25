using AudioSwitcher.AudioApi.CoreAudio;
using HidSharp;
using JJManager.Class.App.Input;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Windows.Input;
using System.Windows.Markup;
using HIDClass = JJManager.Class.Devices.Connections.HID;

namespace JJManager.Class.Devices
{
    public class JJM01 : HIDClass
    {
        private CoreAudioController coreAudioController = new CoreAudioController();

        public JJM01(HidDevice hidDevice) : base (hidDevice)
        {
            _actionReceivingData = () => { ActionReceivingData(); };
            _actionSendingData = () => { ActionSendingData(); };
        }

        private void ActionReceivingData()
        {
            while (_isConnected)
            {
                if (!ReceiveData())
                {
                    Disconnect();
                }
            }
        }

        private void ActionSendingData()
        {
            while (_isConnected)
            {
                if (!SendData())
                {
                    Disconnect();
                }
            }
        }

        public bool SendData()
        {
            bool result = false;

            if (_profile.NeedsUpdate)
            {
                _profile.Restart();
                _profile.NeedsUpdate = false;
            }

            JsonArray jsonArray = new JsonArray();
            Input[] profileInputs = _profile.Inputs.ToArray();

            foreach (Input inputToSend in profileInputs)
            {
                string messageToSend = new JsonObject
                {
                    { "data", new JsonObject
                        {
                            { "order", inputToSend.Id },
                            { "name", inputToSend.Name }
                        }
                    }
                }.ToJsonString();

                result = SendHIDData(messageToSend, false, 300).Result;
            }

            return result;
        }

        public bool ReceiveData()
        {
            var ( success, jsonString ) = ReceiveHIDData();

            if (success && !string.IsNullOrEmpty(jsonString))
            {
                try
                {
                    JsonObject json = JsonObject.Parse(jsonString).AsObject();

                    if (json.ContainsKey("order") && json.ContainsKey("value") && _profile.Inputs[json["order"].GetValue<int>()].AudioController != null)
                    {
                        if (_profile.Inputs[json["order"].GetValue<int>()].AudioController.AudioCoreNeedsRestart)
                        {
                            //if (coreAudioController != null)
                            //{
                            //    coreAudioController.Dispose();
                            //}

                            coreAudioController = new CoreAudioController();

                            for (int i = 0; i < _profile.Inputs.Count; i++)
                            {
                                if (_profile.Inputs[i].AudioController != null && _profile.Inputs[i].AudioController.AudioCoreNeedsRestart)
                                {
                                    _profile.Inputs[i].AudioController.ResetCoreAudioController(coreAudioController);
                                    _profile.Inputs[i].AudioController.AudioCoreNeedsRestart = false;
                                }
                            }

                        }

                        _profile.Inputs[json["order"].GetValue<int>()].AudioController.SettedVolume = json["value"].GetValue<int>();
                    }
                }
                catch (Exception ex)
                {
                    Log.Insert("AudioController", "Ocorreu um problema ao atualizar os dados do input", ex);
                }
            }

            for (int i = 0; i < _profile?.Inputs?.Count; i++)
            {
                if (_profile?.Inputs[i]?.AudioController?.SettedVolume == -1)
                {
                    var (requestedSuccess, requestedJsonString) = RequestHIDData(new JsonObject { { "request", new JsonArray { { "input-" + i } } } }.ToJsonString());

                    if (requestedSuccess && !string.IsNullOrEmpty(requestedJsonString))
                    {
                        JsonObject requestedJson = JsonObject.Parse(requestedJsonString).AsObject();

                        if (requestedJson.ContainsKey("input-" + i) && _profile.Inputs[i].AudioController != null)
                        {
                            _profile.Inputs[i].AudioController.SettedVolume = requestedJson["input-" + i].GetValue<int>();
                        }
                    }
                }

                _profile.Inputs[i].Execute();
            }

            return success;
        }
    }
}
