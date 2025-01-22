using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using AudioSwitcher.AudioApi.Observables;
using AudioSwitcher.AudioApi.Session;
using JJManager.Class.App.Input.MacroKey.Keyboard;
using JJManager.Class.App.Input.MacroKey.Mouse;
using Microsoft.SqlServer.Management.XEvent;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Concurrent;
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
        private ConcurrentBag<IDisposable> _subscriptionSession = new ConcurrentBag<IDisposable>();
        private ConcurrentBag<IDisposable> _subscriptionDevice = new ConcurrentBag<IDisposable>();
        private CancellationTokenSource _currentCtsDevice = new CancellationTokenSource();
        private CancellationTokenSource _currentCtsSession = new CancellationTokenSource();
        private List<IAudioSession> _sessionsGetted = new List<IAudioSession>();
        private List<CoreAudioDevice> _devicesToControl = new List<CoreAudioDevice>();
        private List<AudioSession> _sessionsToControl = new List<AudioSession>();
        private readonly SemaphoreSlim _volumeSemaphoreDevice = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _volumeSemaphoreApp = new SemaphoreSlim(1, 1);
        private readonly object _lock = new object();
        private bool _changingVolume = false;
        private bool _resetingCore = false;
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
        public async Task ResetCoreAudioController(CoreAudioController coreAudioController)
        {
            while (_changingVolume)
            {
                await Task.Delay(10); // do nothing, wait...
            }

            _resetingCore = true;
            
            try
            {
                _coreAudioController = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                _coreAudioController = coreAudioController;
                _coreAudioController.AudioDeviceChanged.Subscribe<DeviceChangedArgs>(x => {
                    _audioCoreNeedsRestart = true;
                    //Console.WriteLine("Device Changed...");
                });

                _devicesToControl.Clear();
                _sessionsGetted.Clear();
                _sessionsToControl.Clear();

                await _coreAudioController.GetPlaybackDevicesAsync(DeviceState.All).ContinueWith(async devices =>
                {
                    foreach (CoreAudioDevice device in devices.Result)
                    {
                        device.GetCapability<IAudioSessionController>()?.SessionCreated.Subscribe<IAudioSession>(session =>
                        {
                            _audioCoreNeedsRestart = true;
                        });

                        device.GetCapability<IAudioSessionController>()?.SessionDisconnected.Subscribe<string>(session =>
                        {
                            _audioCoreNeedsRestart = true;
                        });

                        await device.GetCapability<IAudioSessionController>()?.AllAsync().ContinueWith(sessions =>
                        {
                            foreach (IAudioSession session in sessions.Result)
                            {
                                _sessionsGetted.Add(session);
                            }
                        });

                        device.DefaultChanged.Subscribe<DeviceChangedArgs>(x =>
                        {
                            _audioCoreNeedsRestart = true;
                        });

                        device.GetCapability<IAudioSessionController>()?.SessionCreated.Subscribe<IAudioSession>(session =>
                        {
                            _audioCoreNeedsRestart = true;
                        });

                        device.GetCapability<IAudioSessionController>()?.SessionDisconnected.Subscribe<string>(session =>
                        {
                            _audioCoreNeedsRestart = true;
                        });

                        _devicesToControl.Add(device);
                    }
                });

                await _coreAudioController.GetCaptureDevicesAsync(DeviceState.All).ContinueWith( devices => {
                    foreach (CoreAudioDevice device in devices.Result)
                    {
                        device.DefaultChanged.Subscribe<DeviceChangedArgs>(x =>
                        {
                            _audioCoreNeedsRestart = true;
                        });

                        device.GetCapability<IAudioSessionController>()?.SessionCreated.Subscribe<IAudioSession>(session =>
                        {
                            _audioCoreNeedsRestart = true;
                        });

                        device.GetCapability<IAudioSessionController>()?.SessionDisconnected.Subscribe<string>(session =>
                        {
                            _audioCoreNeedsRestart = true;
                        });

                        _devicesToControl.Add(device);
                    }
                });
            }
            catch(Exception ex)
            {
                Log.Insert("AudioController", "Ocorreu um problema ao resetar o CoreAudioController", ex);
            }
            finally
            {
                _resetingCore = false;
            }
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
            try
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
            catch
            {
                return false;
            }
        }


        private async Task ChangeAppVolume(String AppExecutable, int SettedVolume)
        {
            try
            {
                //lock (_lock)
                //{
                if (_sessionsToControl != null && !_sessionsToControl.Exists( x => x.Executable == AppExecutable))
                {
                    AudioSession audioSession = new AudioSession(AppExecutable);

                    for (int i = 0; i < _sessionsGetted.Count; i++)
                    {
                        if (_sessionsGetted[i] == null)
                        {
                            continue;
                        }

                        if (CheckProcessInfo(_sessionsGetted[i].ProcessId, AppExecutable))
                        {
                            _sessionsGetted[i].StateChanged.Subscribe<SessionStateChangedArgs>(state =>
                            {
                                _audioCoreNeedsRestart = true;
                            });

                            _sessionsGetted[i].Disconnected.Subscribe<SessionDisconnectedArgs>(item =>
                            {
                                _audioCoreNeedsRestart = true;
                            });

                            audioSession.Add(_sessionsGetted[i]);
                        }
                    }

                    _sessionsToControl.Add(audioSession);
                }

                int index = _sessionsToControl.FindIndex(session => session.Executable == AppExecutable);

                if (index > -1)
                {
                    int numberSessionsToControl = _sessionsToControl[index].Sessions?.Count ?? 0;
                    
                    if (numberSessionsToControl > 0)
                    {
                        for (int i = 0; i < numberSessionsToControl; i++)
                        {
                            IAudioSession audioSession = _sessionsToControl[index]?.Sessions[i] ?? null;
                            
                            if (audioSession == null)
                            {
                                continue;
                            }

                            await SetVolumeAndMuteAsync(audioSession, SettedVolume);

                            _subscriptionSession.Add(audioSession.VolumeChanged.Subscribe<SessionVolumeChangedArgs>(async result =>
                            {
                                if (_currentCtsSession.Token.IsCancellationRequested)
                                {
                                    return;
                                }

                                if (result.Session.Volume != SettedVolume)
                                {
                                    await SetVolumeAndMuteAsync(audioSession, SettedVolume);
                                }
                            }));
                        }
                    }
                }
                //}
            }
            catch (OperationCanceledException)
            {
                // do nothing...
            }
            catch (Exception ex)
            {
                Log.Insert("AudioController", "Problema ocorrido ao buscar sessão de áudio.", ex);
            }
        }

        private async Task ChangeDeviceVolume(String deviceId, int SettedVolume)
        {
            try
            {
                if (_coreAudioController == null)
                {
                    return;
                }

                CoreAudioDevice device = await _coreAudioController.GetDeviceAsync(Guid.Parse(deviceId), DeviceState.Active);

                if (device != null && Math.Round(device.Volume) != SettedVolume)
                {
                    await SetVolumeAndMuteAsync(device, SettedVolume);

                    _subscriptionDevice.Add(device.VolumeChanged.Subscribe<DeviceVolumeChangedArgs>(async result =>
                    {
                        if (result.Device.Volume != SettedVolume)
                        {
                            await SetVolumeAndMuteAsync(device, SettedVolume);
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

        public void RemoveSubscriptions()
        {
            while (_subscriptionSession.TryTake(out var subscription))
            {
                subscription.Dispose();
            }

            while (_subscriptionDevice.TryTake(out var subscription))
            {
                subscription.Dispose();
            }
        }

        public async Task ChangeVolume()
        {
            while (_resetingCore)
            {
                await Task.Delay(10); // do nothing, wait...
            }

            _changingVolume = true;

            try
            {
                if (_audioMode == AudioMode.None || _toManage.Count == 0)
                {
                    return;
                }

                if (_coreAudioController == null)
                {
                    await ResetCoreAudioController(new CoreAudioController());
                }

                switch (_audioMode)
                {
                    case AudioMode.Application:
                        _currentCtsSession.Cancel();

                        while (_subscriptionSession.TryTake(out var subscription))
                        {
                            subscription.Dispose();
                        }

                        _currentCtsSession.Dispose();
                        _currentCtsSession = new CancellationTokenSource();

                        foreach (string app in _toManage)
                        {
                            await ChangeAppVolume(app, SettedVolume);
                        }
                        break;
                    case AudioMode.DevicePlayback:
                        while (_subscriptionDevice.TryTake(out var subscription))
                        {
                            subscription.Dispose();
                        }

                        foreach (string device in _toManage)
                        {
                            await ChangeDeviceVolume(device, SettedVolume);
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Insert("AudioController", "Ocorreu um problema ao realizar a alteração de volume", ex);
            }
            finally
            {
                _changingVolume = false;
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
