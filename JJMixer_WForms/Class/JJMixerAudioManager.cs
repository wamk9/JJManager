using AudioSwitcher.AudioApi.CoreAudio;
using AudioSwitcher.AudioApi.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJMixer_WForms.Class
{
    internal class JJMixerAudioManager
    {
        private static List<CoreAudioDevice> _devices = new CoreAudioController().GetDevices().ToList();
        private JJMixerDbConnection _JJMixerDbConnection = new JJMixerDbConnection();
        public void ChangeInputVolume(String InputId, String Model, int SettedVolume)
        {
            String InputType = _JJMixerDbConnection.GetInputType(Int16.Parse(InputId), Model);

            if (InputType == "app")
            {
                foreach (String App in _JJMixerDbConnection.GetInputInfo(Int16.Parse(InputId), Model)) 
                {
                    this.ChangeAppVolume(App, SettedVolume);
                }
            }
            else if (InputType == "device")
            {
                foreach (String DeviceId in _JJMixerDbConnection.GetInputInfo(Int16.Parse(InputId), Model))
                {
                    this.ChangeDeviceVolume(DeviceId, SettedVolume);
                }

            }
        }

        public void ChangeAppVolume(String AppExecutable, int SettedVolume)
        {
            //bool AppVolumeChanged = false;

            foreach (CoreAudioDevice device in _devices)
            {
                if (device.IsPlaybackDevice)
                {
                    foreach (var audioSession in device.GetCapability<IAudioSessionController>())
                    {
                        if (audioSession.ExecutablePath != null && audioSession.ExecutablePath.Contains(AppExecutable))
                        {
                            audioSession.SetVolumeAsync(SettedVolume);
                            //AppVolumeChanged = true;
                        }

                        /*
                        if (AppVolumeChanged)
                        {
                            break;
                        }
                        */
                    }
                }
                /*
                if (AppVolumeChanged)
                {
                    break;
                }
                */
            }
        }

        public void ChangeDeviceVolume(String DeviceId, int SettedVolume)
        {
            foreach (CoreAudioDevice device in _devices)
            {
                if (device.Id.ToString() == DeviceId)
                {
                    device.SetVolumeAsync(SettedVolume);
                    break;
                }
            }
        }
    }
}
