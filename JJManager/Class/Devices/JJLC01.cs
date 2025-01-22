using HidSharp;
using System.Threading.Tasks;
using HIDClass = JJManager.Class.Devices.Connections.HID;

namespace JJManager.Class.Devices
{
    public class JJLC01 : HIDClass
    {
        public JJLC01(HidDevice hidDevice) : base(hidDevice)
        {
            _actionReceivingData = () => { Task.Run(async () => await ActionReceivingData()); };
            _actionSendingData = () => { Task.Run(async () => await ActionSendingData()); };
        }

        private async Task ActionReceivingData()
        {
            if (_isConnected)
            {
                //UpdateCoreAudioController(true);
                //await SendInputs();
            }
            while (_isConnected)
            {
                //await UpdateCoreAudioController();
                if (!_profile.NeedsUpdate)
                {
                    //await RequestData();
                }
            }
        }

        private async Task ActionSendingData()
        {
            while (_isConnected)
            {
                if (_profile.NeedsUpdate)
                {
                    //_profile.Restart();
                    //UpdateCoreAudioController(true);
                    //await SendInputs();
                    //_profile.NeedsUpdate = false;
                }

                //UpdateCoreAudioController();
                //await Task.Delay(1000);
            }
        }
    }
}
