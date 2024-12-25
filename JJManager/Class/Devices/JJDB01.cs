using AudioSwitcher.AudioApi.CoreAudio;
using HidSharp;
using JJManager.Class.App;
using JJManager.Class.App.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using HIDClass = JJManager.Class.Devices.Connections.HID;

namespace JJManager.Class.Devices
{
    public class JJDB01 : HIDClass, IDisposable
    {
        private SimHubWebsocket _simHubSync = null;

        public JJDB01(HidDevice hidDevice) : base(hidDevice)
        {
            _actionSendingData = () => { ActionSendingData(); };
        }

        private void ActionSendingData()
        {
            bool acceptOldVersionPlugin = false;

            while (_isConnected)
            {
                var (success, responseAboutOldVersionPlugin) = SendData(acceptOldVersionPlugin);
                acceptOldVersionPlugin = responseAboutOldVersionPlugin;

                if (!success)
                {
                    Disconnect();
                }
            }

            Dispose();
        }

        public (bool, bool) SendData(bool acceptedOldVersionPlugin)
        {
            bool result = false;
            JsonObject messageToSend = null;

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
                        "request", new JsonArray {
                            { "SimHubLastData" }
                        }
                    }
                }.ToJsonString()
            );

            if (!success)
            {
                return (false, acceptedOldVersionPlugin);
            }
            else
            {
                //Console.WriteLine(message);
                _simHubSync.TranslateToDashboardHID(message, out messageToSend);
            }

            result = SendHIDData(new JsonObject { { "dash_data", JsonNode.Parse(messageToSend["dash_data"].ToJsonString()) } }.ToJsonString(), false, 0).Result;
            result = result ? SendHIDData(new JsonObject { { "led_data", JsonNode.Parse(messageToSend["led_data"].ToJsonString()) } }.ToJsonString(), false, 0).Result : result;

            return (result, acceptedOldVersionPlugin);
        }

        public void Dispose()
        {
            if (_simHubSync != null)
            {
                _simHubSync.StopCommunication();
                _simHubSync = null;
            }
        }
    }
}
