using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using AudioSwitcher.AudioApi.Observables;
using AudioSwitcher.AudioApi.Session;
using JJManager.Class.App.Input.MacroKey.Keyboard;
using JJManager.Class.App.Input.MacroKey.Mouse;
using Microsoft.SqlServer.Management.XEvent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace JJManager.Class.App.Input.AudioController
{
    public class AudioController
    {
        public enum AudioMode
        {
            None,
            Application,
            DevicePlayback,
            DeviceRecord
        }

        private int _settedVolume = -1;
        private AudioMode _audioMode = AudioMode.None;
        private ObservableCollection<string> _toManage = null;
        private bool _invertedAxis = false;
        private CoreAudioController _coreAudioController = null;
        private bool _audioCoreNeedsRestart = true;
        private List<IDisposable> _subscriptionSession = new List<IDisposable>();
        private List<IDisposable> _subscriptionDevice = new List<IDisposable>();

        public bool AudioCoreNeedsRestart 
        {
            get => _audioCoreNeedsRestart; 
            set => _audioCoreNeedsRestart = value; 
        }

        public CoreAudioController CoreAudioController 
        { 
            get => _coreAudioController;
            set => _coreAudioController = value; 
        }


        public ObservableCollection<string> ToManage
        {
            get => _toManage;
            set => _toManage = value;
        }

        public AudioMode Mode
        {
            get => _audioMode;
            set => _audioMode = value;
        }

        public bool InvertedAxis
        {
            get => _invertedAxis;
        }

        public AudioController()
        {
            _toManage = new ObservableCollection<string>();
        }

        public AudioController(JsonObject json)
        {
            _toManage = new ObservableCollection<string>();
            if (json.ContainsKey("toManage"))
            {
                foreach (var toManage in (JsonArray) json["toManage"])
                {
                    _toManage.Add(toManage.GetValue<string>());
                }
            }

            if (json.ContainsKey("audioMode"))
            {
                _audioMode = ToAudioMode(json["audioMode"].GetValue<string>());
            }
        }

        public JsonObject DataToJson()
        {
            JsonArray json = new JsonArray();

            foreach (string toManage in _toManage)
            {
                json.Add(toManage);
            }

            return new JsonObject()
            {
                { "toManage", json },
                { "audioMode", _audioMode.ToString().ToLower() }
            };
        }

        private AudioMode ToAudioMode(string value)
        {
            switch (value)
            {
                case "application":
                    return AudioMode.Application;
                case "deviceplayback":
                    return AudioMode.DevicePlayback;
                case "devicerecord":
                    return AudioMode.DeviceRecord;
            }

            return AudioMode.None;
        }
        public void ResetCoreAudioController(CoreAudioController coreAudioController)
        {
            _coreAudioController = coreAudioController;
            _coreAudioController.AudioDeviceChanged.Subscribe<DeviceChangedArgs>(x => {
                _audioCoreNeedsRestart = true;
                //Debug.WriteLine("Device Changed...");
            });
        }


        public List<string[]> GetSessions(AudioSwitcher.AudioApi.Session.AudioSessionState audioSessionState = AudioSwitcher.AudioApi.Session.AudioSessionState.Active)
        {
            List<string[]> sessions = new List<string[]>();

            CoreAudioController actualCoreAudioController = (_coreAudioController != null ? _coreAudioController : new CoreAudioController());

            foreach (CoreAudioDevice device in actualCoreAudioController.GetDevices())
            {
                foreach (IAudioSession session in device.GetCapability<IAudioSessionController>().ActiveSessions())
                {
                    sessions.Add(new string[] { session.ProcessId.ToString(), session.DisplayName, GetProcessNameOrExecutableById(session.ProcessId), ((int)session.Volume).ToString() + "%" });
                }
            }

            return sessions;
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


        private async void ChangeAppVolume(String AppExecutable, int SettedVolume)
        {
            Task<IEnumerable<CoreAudioDevice>> devices = _coreAudioController.GetPlaybackDevicesAsync();
            devices.Wait();

            foreach (CoreAudioDevice device in devices.Result)
            {
                Task<IEnumerable<IAudioSession>> sessions = device.GetCapability<IAudioSessionController>().ActiveSessionsAsync();
                sessions.Wait();

                foreach (IAudioSession session in sessions.Result)
                {
                    if (CheckProcessInfo(session.ProcessId, AppExecutable))
                    {
                        await SetVolumeAndMuteAsync(session, SettedVolume);

                        _subscriptionSession.Add(session.VolumeChanged.Subscribe<SessionVolumeChangedArgs>(result =>
                        {
                            if (result.Session.Volume != SettedVolume)
                            {
                                SetVolumeAndMuteAsync(session, SettedVolume).Wait();
                            }
                        })
                        );
                    }
                }
            }
        }

        private async void ChangeDeviceVolume(String DeviceId, int SettedVolume)
        {
            Task<CoreAudioDevice> device = _coreAudioController.GetDeviceAsync(Guid.Parse(DeviceId));

            device.Wait();

            if (device.Result != null && Math.Round(device.Result.Volume) != SettedVolume)
            {
                await SetVolumeAndMuteAsync(device.Result, SettedVolume);

                _subscriptionDevice.Add(device.Result.VolumeChanged.Subscribe<DeviceVolumeChangedArgs>(result =>
                {
                    if (result.Device.Volume != SettedVolume)
                    {
                        SetVolumeAndMuteAsync(device.Result, SettedVolume).Wait();
                    }
                })
                );
            }
        }

        private async Task SetVolumeAndMuteAsync(CoreAudioDevice device, int SettedVolume)
        {
            await device.SetVolumeAsync(SettedVolume);
            await device.SetMuteAsync(SettedVolume > 0 ? false : true);
        }

        private async Task SetVolumeAndMuteAsync(IAudioSession session, int SettedVolume)
        {
            await session.SetVolumeAsync(SettedVolume);
            await session.SetMuteAsync(SettedVolume > 0 ? false : true);
        }

        public void ChangeVolume(int settedVolume)
        {
            if (_audioMode == AudioMode.None || _toManage.Count == 0)
            {
                return;
            }

            if (_invertedAxis)
            {
                settedVolume = Math.Abs(settedVolume - 100);
            }

            if (_coreAudioController == null)
            {
                ResetCoreAudioController(new CoreAudioController());
            }

            _settedVolume = settedVolume;

            switch (_audioMode)
            {
                case AudioMode.Application:
                    if (_subscriptionSession.Count > 0)
                    {
                        _subscriptionSession.ForEach(subs =>
                        {
                            if (subs != null)
                            {
                                subs.Dispose();
                            }
                        });

                        _subscriptionSession.Clear();
                    }

                    foreach (string app in _toManage)
                    {
                        Task.Run(() =>
                        {
                            ChangeAppVolume(app, settedVolume);
                        });
                    }
                    break;
                case AudioMode.DevicePlayback:
                    if (_subscriptionDevice.Count > 0)
                    {
                        _subscriptionDevice.ForEach(subs =>
                        {
                            if (subs != null)
                            {
                                subs.Dispose();
                            }
                        });

                        _subscriptionDevice.Clear();
                    }

                    foreach (string device in _toManage)
                    {
                        Task.Run(() =>
                        {
                            ChangeDeviceVolume(device, settedVolume);
                        });
                    }

                    break;
            }
        }

        public void Update(AudioMode audioMode, List<string> values)
        {
            _audioMode = audioMode;

            // Clear the original collection and repopulate (Need clean ListView)
            _toManage.Clear();

            foreach (string value in values)
            {
                _toManage.Add(value);
            }
        }
    }
}
