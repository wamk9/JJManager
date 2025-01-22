using AudioSwitcher.AudioApi.CoreAudio;
using System;
using SharpDX.DirectInput;
using JoystickClass = JJManager.Class.Devices.Connections.Joystick;
using System.Text.Json.Nodes;

namespace JJManager.Class.Devices
{
    public class JJB01 : JoystickClass
    {
        private CoreAudioController coreAudioController = new CoreAudioController();

        public JJB01(Joystick joystick) : base(joystick)
        {
            _actionReceivingData = () => { ActionReceivingData(); };
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

        public bool ReceiveData()
        {
            var (success, currentStatus) = ReceiveJoystickData();

            if (success)
            {
                if (_profile.Inputs[0].AudioController.AudioCoreNeedsRestart || _profile.Inputs[1].AudioController.AudioCoreNeedsRestart)
                {
                    coreAudioController = new CoreAudioController();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    for (int i = 0; i < _profile.Inputs.Count; i++)
                    {
                        if (_profile.Inputs[i].AudioController.AudioCoreNeedsRestart)
                        {
                            _profile.Inputs[i].AudioController.ResetCoreAudioController(coreAudioController).Wait();
                            _profile.Inputs[i].AudioController.AudioCoreNeedsRestart = false;
                        }
                    }
                }

                if (currentStatus.X != -1)
                {
                    _profile.Inputs[0].Execute(new JsonObject { { "value", GetAxisPercent(currentStatus.X) } });                    
                }

                if (currentStatus.Y != -1)
                {
                    _profile.Inputs[1].Execute(new JsonObject { { "value", GetAxisPercent(currentStatus.Y) } });
                }
            }

            return success;
        }
    }
}
