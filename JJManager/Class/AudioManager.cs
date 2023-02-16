using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using AudioSwitcher.AudioApi.Session;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJManager.Class
{
    internal class AudioManager
    {
        private static CoreAudioController _coreAudioController = new CoreAudioController();
        private static List<CoreAudioDevice> _devices = _coreAudioController.GetDevices().ToList();
        private DatabaseConnection _DatabaseConnection = new DatabaseConnection();

        private void RefreshAudioDevices()
        {
            _devices.Clear();
            _devices = new List<CoreAudioDevice>(_coreAudioController.GetDevices());
        }

        public void ChangeInputVolume(String IdProduct, String InputId, int SettedVolume)
        {
            RefreshAudioDevices();

            String InputType = _DatabaseConnection.GetInputType(IdProduct, Int16.Parse(InputId));

            if (InputType == "app")
            {
                foreach (String App in _DatabaseConnection.GetInputInfo(IdProduct, Int16.Parse(InputId))) 
                {
                    if (App != String.Empty)
                        this.ChangeAppVolume(App, SettedVolume);
                }
            }
            else if (InputType == "device")
            {
                foreach (String DeviceId in _DatabaseConnection.GetInputInfo(IdProduct, Int16.Parse(InputId)))
                {
                    if (DeviceId != String.Empty)
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
                    
                    foreach (var audioSession in device.GetCapability<IAudioSessionController>().ActiveSessions())
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
