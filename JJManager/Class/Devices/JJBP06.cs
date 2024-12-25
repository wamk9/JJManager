using AudioSwitcher.AudioApi.CoreAudio;
using HidSharp.Reports.Input;
using HidSharp.Reports;
using HidSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.Json;
using JJManager.Class.App.Profile;
using JJManager.Class.App;
using System.Text.Json.Nodes;
using HIDClass = JJManager.Class.Devices.Connections.HID;

namespace JJManager.Class.Devices
{
    public class JJBP06 : HIDClass
    {
        public JJBP06(HidDevice hidDevice) : base(hidDevice)
        {
            _actionSendingData = () => { ActionSendingData(); };
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

            result = SendHIDData(_profile.Data.ToJsonString()).Result;

            return result;
        }
    }
}
