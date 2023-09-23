using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using AudioSwitcher.AudioApi.Observables;
using AudioSwitcher.AudioApi.Session;
using SharpDX.DirectInput;
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

        public AudioManager ()
        {
            IObservable<DeviceChangedArgs> deviceChangedTypes = _coreAudioController.AudioDeviceChanged;

            deviceChangedTypes.Subscribe(x => {
                _devices.Clear();
                _devices = new List<CoreAudioDevice>(_coreAudioController.GetDevices());
            });
        }

        public void ChangeInputVolume(Inputs input, int settedVolume)
        {
            if (input == null)
                return;

            if (input.InvertedAxis)
                settedVolume = Math.Abs(settedVolume - 100);

            foreach (String info in input.Info.Split('|'))
            {
                if (input.Type == "app" && info != String.Empty)
                    ChangeAppVolume(info, settedVolume);
                else if (input.Type == "device" && info != String.Empty)
                    ChangeDeviceVolume(info, settedVolume);
            }
        }

        private void ChangeAppVolume(String AppExecutable, int SettedVolume)
        {
            foreach (CoreAudioDevice device in _devices)
            {
                if (device.IsPlaybackDevice)
                {
                    foreach (var audioSession in device.GetCapability<IAudioSessionController>().ActiveSessions())
                    {
                        if (audioSession.ExecutablePath != null && audioSession.ExecutablePath.Contains(AppExecutable))
                        {
                            audioSession.SetVolumeAsync(SettedVolume);
                        }
                    }
                }
            }
        }

        private void ChangeDeviceVolume(String DeviceId, int SettedVolume)
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
