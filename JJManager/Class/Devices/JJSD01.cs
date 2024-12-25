using HidSharp.Reports;
using HidSharp;
using JJManager.Class.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HidSharp.Reports.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text.Json.Nodes;
using AudioSwitcher.AudioApi.CoreAudio;
using JJManager.Class.App.Input;
using JJManager.Class.App.Profile;
using HIDClass = JJManager.Class.Devices.Connections.HID;

namespace JJManager.Class.Devices
{
    internal class JJSD01 : HIDClass
    {
        public JJSD01(HidDevice hidDevice) : base(hidDevice)
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

        private bool SendData()
        {
            bool result = false;

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

                result = SendHIDData(messageToSend).Result;
            }

            return result;
        }

        private bool ReceiveData()
        {
            var (success, jsonString) = ReceiveHIDData();

            if (success && !string.IsNullOrEmpty(jsonString))
            {
                JsonObject json = JsonObject.Parse(jsonString).AsObject();

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

            return success;
        }
    }
}
