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
using static OpenBLT.Lib;

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
        private CancellationTokenSource _currentCtsDevice = new CancellationTokenSource();
        private CancellationTokenSource _currentCtsSession = new CancellationTokenSource();
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

        public int SettedVolume
        {
            get => _settedVolume;
            set => _settedVolume = _invertedAxis ? Math.Abs(value - 100) : value;
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
            try
            {
                Task<IEnumerable<CoreAudioDevice>> devices = _coreAudioController.GetPlaybackDevicesAsync();
                devices.Wait();

                var token = _currentCtsSession.Token;

                foreach (CoreAudioDevice device in devices.Result)
                {
                    _subscriptionSession.Add(device.GetCapability<IAudioSessionController>().SessionCreated.Subscribe<IAudioSession>(session =>
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }

                        if (CheckProcessInfo(session.ProcessId, AppExecutable))
                        {
                            SetVolumeAndMuteAsync(session, SettedVolume).Wait();

                            _subscriptionSession.Add(session.VolumeChanged.Subscribe<SessionVolumeChangedArgs>(result =>
                            {
                                if (token.IsCancellationRequested)
                                {
                                    return;
                                }

                                if (result.Session.Volume != SettedVolume)
                                {
                                    SetVolumeAndMuteAsync(session, SettedVolume).Wait();
                                }
                            })
                            );
                        }
                    }));

                    Task<IEnumerable<IAudioSession>> sessions = device.GetCapability<IAudioSessionController>().AllAsync();
                    sessions.Wait();

                    foreach (IAudioSession session in sessions.Result)
                    {
                        if (CheckProcessInfo(session.ProcessId, AppExecutable))
                        {
                            await SetVolumeAndMuteAsync(session, SettedVolume);

                            _subscriptionSession.Add(session.VolumeChanged.Subscribe<SessionVolumeChangedArgs>(result =>
                            {
                                if (token.IsCancellationRequested)
                                {
                                    return;
                                }

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
            catch (Exception ex)
            {
                Log.Insert("AudioController", "Problema ocorrido ao buscar sessão de áudio.", ex);
            }
        }

        private async void ChangeDeviceVolume(String DeviceId, int SettedVolume)
        {
            try
            {
                Task<CoreAudioDevice> device = _coreAudioController.GetDeviceAsync(Guid.Parse(DeviceId));

                device.Wait();


                if (device.Result != null && Math.Round(device.Result.Volume) != SettedVolume)
                {
                    var token = _currentCtsDevice.Token;

                    await SetVolumeAndMuteAsync(device.Result, SettedVolume);

                    _subscriptionDevice.Add(device.Result.VolumeChanged.Subscribe<DeviceVolumeChangedArgs>(result =>
                    {
                        if (result.Device.Volume != SettedVolume)
                        {
                            if (token.IsCancellationRequested)
                            {
                                return;
                            }

                            SetVolumeAndMuteAsync(device.Result, SettedVolume).Wait();
                        }
                    })
                    );
                }
            }
            catch (Exception ex)
            {
                Log.Insert("AudioController", "Problema ocorrido ao buscar sessão de áudio.", ex);
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

        public void ChangeVolume()
        {
            try
            {
                if (_audioMode == AudioMode.None || _toManage.Count == 0)
                {
                    return;
                }

                if (_coreAudioController == null)
                {
                    ResetCoreAudioController(new CoreAudioController());
                }

                switch (_audioMode)
                {
                    case AudioMode.Application:
                        if (_subscriptionSession.Count > 0)
                        {
                            //for (int i = 0; i < _subscriptionSession.Count; i++)
                            //{
                            //    if (_subscriptionSession[i] != null)
                            //    {

                            //        //_subscriptionSession[i].Dispose();
                            //    }
                            //}

                            _currentCtsSession?.Cancel();

                            // Dispose the token source
                            _currentCtsSession?.Dispose();
                            _currentCtsSession = null;
                            _currentCtsSession = new CancellationTokenSource();

                            _subscriptionSession.Clear();
                        }

                        foreach (string app in _toManage)
                        {
                            Task.Run(() =>
                            {
                                ChangeAppVolume(app, SettedVolume);
                            }).Wait();
                        }
                        break;
                    case AudioMode.DevicePlayback:
                        if (_subscriptionDevice.Count > 0)
                        {
                            //for (int i = 0; i < _subscriptionDevice.Count; i++)
                            //{
                            //    if (_subscriptionDevice[i] != null)
                            //    {
                            //        _subscriptionDevice[i].Dispose();
                            //    }
                            //}

                            _currentCtsDevice?.Cancel();

                            // Dispose the token source
                            _currentCtsDevice?.Dispose();
                            _currentCtsDevice = null;
                            _currentCtsDevice = new CancellationTokenSource();

                            _subscriptionDevice.Clear();
                        }

                        foreach (string device in _toManage)
                        {
                            Task.Run(() =>
                            {
                                ChangeDeviceVolume(device, SettedVolume);
                            }).Wait();
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Insert("AudioController", "Ocorreu um problema ao realizar a alteração de volume", ex);
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
