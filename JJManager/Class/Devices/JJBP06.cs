using HidSharp;
using JJManager.Class.App;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text.Json.Nodes;
using System;
using System.Threading.Tasks;
using HIDClass = JJManager.Class.Devices.Connections.HID;

namespace JJManager.Class.Devices
{
    public class JJBP06 : HIDClass
    {
        public JJBP06(HidDevice hidDevice) : base(hidDevice)
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

                if (_profile.NeedsUpdate)
                {
                    _profile.Restart();
                    _profile.NeedsUpdate = false;
                }

                await SendHIDData(_profile.Data.ToJsonString(), false, 100).ContinueWith((result) =>
                {
                    if (!result.Result)
                    {
                        throw new Exception($"Falha ao enviar '{_profile.Data.ToJsonString()}' via HID para a button box JJB-999 de ID '{_connId}'");
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Insert("JJBP06", "Ocorreu um problema ao enviar dados para a sua button box", ex);
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
    }
}
