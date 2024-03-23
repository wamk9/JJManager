using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using AudioSwitcher.AudioApi.Observables;
using AudioSwitcher.AudioApi.Session;
using HidSharp;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Cryptography;
using HidSharp.Reports.Encodings;
using System.Management;

namespace JJManager.Class.App
{
    public class AudioManager
    {
        private static CoreAudioController _coreAudioController = null;
        //private static List<CoreAudioDevice> _playbackDevices = _coreAudioController.GetPlaybackDevices().ToList();
        //private static List<CoreAudioDevice> _playbackDevices2 = _coreAudioController.GetPlaybackDevices().ToList();
        //private static List<CoreAudioDevice> _devices = _coreAudioController.GetDevices().ToList();
        private bool _audioCoreNeedsRestart = false;
        private IObservable<DeviceChangedArgs> _deviceChanged = null;

        public bool AudioCoreNeedsRestart { set {  _audioCoreNeedsRestart = value; } get { return _audioCoreNeedsRestart; } }
        public CoreAudioController CoreAudioController { get { return _coreAudioController; } set { _coreAudioController = value; } }

        public AudioManager ()
        {

        }

        public List<string[]> GetSessions(AudioSwitcher.AudioApi.Session.AudioSessionState audioSessionState = AudioSwitcher.AudioApi.Session.AudioSessionState.Active)
        {
            List<string[]> sessions = new List<string[]>();

            CoreAudioController actualCoreAudioController = (_coreAudioController != null ? _coreAudioController : new CoreAudioController());

            foreach (CoreAudioDevice device in actualCoreAudioController.GetDevices())
            {
                foreach (IAudioSession session in device.GetCapability<IAudioSessionController>().ActiveSessions())
                {
                    sessions.Add(new string[] { session.ProcessId.ToString(), session.DisplayName, GetProcessNameOrExecutableById(session.ProcessId), ((int) session.Volume).ToString() + "%" } );
                }
            }

            return sessions;
        }

        public void ResetCoreAudioController(CoreAudioController coreAudioController)
        {
            _coreAudioController = coreAudioController;
            _deviceChanged = _coreAudioController.AudioDeviceChanged;
            _deviceChanged.Subscribe(x => {
                _audioCoreNeedsRestart = true;
                //Debug.WriteLine("Device Changed...");
            });
        }

        private string GetProcessNameOrExecutableById(int pid)
        {
            var wmiQueryString = $"SELECT * FROM Win32_Process WHERE ProcessId = {pid}";

            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            using (var results = searcher.Get())
            {
                foreach (var mo in results.Cast<ManagementObject>())
                {
                    /*
                    foreach (var prop in mo.Properties)
                    {
                        Console.WriteLine($"{prop.Name}: {prop.Value}");
                        // Handle each property as needed
                    }
                    */

                    string path = (string)mo["ExecutablePath"];
                    string name = (string)mo["Name"];

                    if (!string.IsNullOrEmpty(path))
                    {
                        return System.IO.Path.GetFileName(path);
                    }
                    else if (!string.IsNullOrEmpty(name))
                    {
                        return name;
                    }
                }
            }

            return "";
        }

        private bool CheckProcessInfo(int pid, String info)
        {
            var wmiQueryString = $"SELECT * FROM Win32_Process WHERE ProcessId = {pid}";
            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            using (var results = searcher.Get())
            {
                foreach (var mo in results.Cast<ManagementObject>())
                {
                    /*
                    foreach (var prop in mo.Properties)
                    {
                        Console.WriteLine($"{prop.Name}: {prop.Value}");
                        // Handle each property as needed
                    }
                    */

                    string path = (string)mo["ExecutablePath"];
                    string name = (string)mo["Name"];
                    
                    if ((!string.IsNullOrEmpty(path) && path.Contains(info)) || (!string.IsNullOrEmpty(name) && name.Contains(info)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void ChangeVolume(AnalogInput input, int settedVolume, out bool audioCoreRestarted)
        {
            if (input == null)
            {
                audioCoreRestarted = false;
                return;
            }

            if (input.InvertedAxis)
            {
                settedVolume = Math.Abs(settedVolume - 100);
            }

            if (_coreAudioController == null)
            {
                ResetCoreAudioController(new CoreAudioController());
            }

            /*if (_audioCoreNeedsRestart && !_audioCoreRestarted)
            {
                ResetCoreAudioController();
                _audioCoreNeedsRestart = false;
                _audioCoreRestarted = true;
            }
            else 
            {
                _audioCoreNeedsRestart = false;
                _audioCoreRestarted = false;
            }*/

            if (input.Type == "app" && input.Info != String.Empty)
            {
                foreach (String info in input.Info.Split('|'))
                {
                    ChangeAppVolume(info, settedVolume);
                }
            }

            if (input.Type == "device" && input.Info != String.Empty)
            {
                foreach (String info in input.Info.Split('|'))
                {
                    ChangeDeviceVolume(info, settedVolume);
                }
            }

            audioCoreRestarted = _audioCoreNeedsRestart;
        }

        private void ChangeAppVolume(String AppExecutable, int SettedVolume)
        {
            foreach (CoreAudioDevice device in _coreAudioController.GetDevicesAsync().Result)
            {
                foreach (IAudioSession session in device.GetCapability<IAudioSessionController>().ActiveSessionsAsync().Result)
                {
                    if (CheckProcessInfo(session.ProcessId, AppExecutable)/* && Math.Round(session.Volume) != SettedVolume*/)
                    {
                        session.SetVolumeAsync(SettedVolume);
                        break;
                    }
                }
            }
        }

        private void ChangeDeviceVolume(String DeviceId, int SettedVolume)
        {
            _coreAudioController.GetDeviceAsync(Guid.Parse(DeviceId)).ContinueWith(device =>
            {
                if (device.Result != null)
                {
                    device.Result.SetVolumeAsync(SettedVolume);
                }
            });
        }
    }
}
