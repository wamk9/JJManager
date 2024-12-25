using HidSharp.Reports;
using HidSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Text.Json;
using JJManager.Class.App;
using HIDClass = JJManager.Class.Devices.Connections.HID;
using AudioSwitcher.AudioApi.CoreAudio;
using JJManager.Class.App.Input;
using System.Text.Json.Nodes;

namespace JJManager.Class.Devices
{
    internal class JJB01_V2 : HIDClass, IDisposable
    {
        private SimHubWebsocket _simHubSync = null;

        public JJB01_V2(HidDevice hidDevice) : base(hidDevice)
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
                    }.ToJsonString()
                );

                if (!success)
                {
                    return (false, acceptedOldVersionPlugin);
                }
                else
                {
                    _simHubSync.TranslateToButtonBoxHID(message, out messageToSend, "jjb01v2", ref acceptedOldVersionPlugin, brightness_limit_value);
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

            result = SendHIDData(messageToSend, false, 200).Result;

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
