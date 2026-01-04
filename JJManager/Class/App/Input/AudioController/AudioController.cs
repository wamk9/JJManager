using NAudio.CoreAudioApi;
using JJManager.Class.App.Input.MacroKey.Keyboard;
using JJManager.Class.App.Input.MacroKey.Mouse;
using Microsoft.SqlServer.Management.XEvent;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        private MMDeviceEnumerator _deviceEnumerator = null; // NAudio: MMDeviceEnumerator replaces CoreAudioController
        private bool _deviceEnumeratorDisposed = false; // Track if enumerator was disposed
        private bool _audioCoreNeedsRestart = true;
        private CancellationTokenSource _currentCtsSession = new CancellationTokenSource();
        private readonly SemaphoreSlim _coreAudioOperationLock = new SemaphoreSlim(1, 1); // Protects reset/volume/session operations
        private bool _changingVolume = false;
        private bool _resetingCore = false;
        private bool _updateSessionsToControl = false;
        private AudioMonitor _audioMonitor = null;

        // Variáveis ESTÁTICAS compartilhadas entre todos os AudioControllers
        private static List<MMDevice> _sharedDevices = null;
        private static List<AudioSessionControl> _sharedSessions = null;
        private static List<AudioSession> _sharedSessionsGrouped = null;
        private static readonly object _sharedLock = new object();
        public bool AudioCoreNeedsRestart
        {
            get => _audioCoreNeedsRestart;
            set => _audioCoreNeedsRestart = value;
        }

        public bool UpdateSessionsToControl
        {
            get => _updateSessionsToControl;
            set => _updateSessionsToControl = value;
        }

        public MMDeviceEnumerator DeviceEnumerator
        {
            get => _deviceEnumerator;
            set => _deviceEnumerator = value;
        }

        /// <summary>
        /// Propriedade thread-safe para acessar sessões agrupadas compartilhadas
        /// </summary>
        public List<AudioSession> SessionsToControl
        {
            get
            {
                lock (_sharedLock)
                {
                    return _sharedSessionsGrouped?.ToList() ?? new List<AudioSession>();
                }
            }
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
            InitializeClassAttribs();
        }

        public AudioController(JsonObject json)
        {
            InitializeClassAttribs();

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

        private void InitializeClassAttribs()
        {
            _toManage = new ObservableCollection<string>();
            _audioMonitor = new AudioMonitor();


            _audioMonitor.SessionCreated += async (sender, e) =>
            {
                try
                {
                    string sessionName = null;

                    // Thread-safe: adicionar nova sessão às listas compartilhadas
                    lock (_sharedLock)
                    {
                        if (_sharedSessions == null)
                        {
                            _sharedSessions = new List<AudioSessionControl>();
                        }

                        if (e.Info?.Session != null && !_sharedSessions.Contains(e.Info.Session))
                        {
                            _sharedSessions.Add(e.Info.Session);
                        }

                        // Atualizar sessões agrupadas
                        _sharedSessionsGrouped = GroupSessionsByExecutableWithCache(_sharedSessions, _sharedSessionsGrouped);

                        // Buscar nome da sessão
                        sessionName = _sharedSessionsGrouped?.FirstOrDefault(x => x?.Sessions?.Any(y => y.GetProcessID == (e?.Info?.ProcessId ?? 0)) ?? false)?.Executable;
                    }

                    if (!string.IsNullOrEmpty(sessionName) && _settedVolume >= 0)
                    {
                        // Verificar se está em _toManage (app gerenciado)
                        var managedApp = _toManage.FirstOrDefault(app =>
                            !string.IsNullOrEmpty(app) &&
                            (sessionName.IndexOf(app, StringComparison.OrdinalIgnoreCase) >= 0 ||
                             app.IndexOf(sessionName, StringComparison.OrdinalIgnoreCase) >= 0));

                        if (managedApp != null)
                        {
                            // Pequeno delay para garantir que a sessão esteja completamente inicializada
                            await Task.Delay(100);

                            // Aplicar volume configurado imediatamente
                            await ChangeAppVolume(managedApp, _settedVolume);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Insert("AudioController", $"Erro ao processar nova sessão: {ex.Message}", ex);
                }
            };

            _audioMonitor.SessionDisconnected += (sender, e) =>
            {
                // Thread-safe: remover sessão das listas compartilhadas
                lock (_sharedLock)
                {
                    if (_sharedSessions != null)
                    {
                        var sessionToRemove = _sharedSessions.FirstOrDefault(s => s.GetProcessID == e.ProcessId);
                        if (sessionToRemove != null)
                        {
                            _sharedSessions.Remove(sessionToRemove);
                        }
                    }

                    // Atualizar sessões agrupadas
                    _sharedSessionsGrouped = GroupSessionsByExecutableWithCache(_sharedSessions, _sharedSessionsGrouped);
                }
            };

            _audioMonitor.SessionVolumeChanged += (sender, e) =>
            {
                // Thread-safe: buscar sessão nas listas compartilhadas
                AudioSession audioSession = null;
                lock (_sharedLock)
                {
                    audioSession = _sharedSessionsGrouped?.FirstOrDefault(s =>
                        s.Sessions?.Any(session => session.GetProcessID == e.ProcessId) == true);
                }

                if (audioSession != null)
                {
                    var session = audioSession.Sessions.FirstOrDefault(s => s.GetProcessID == e.ProcessId);
                    if (session != null)
                    {
                        SetVolumeAndMuteAsync(session, _settedVolume).Wait();
                    }
                }
            };

            _audioMonitor.SessionStateChanged += async (sender, e) =>
            {
                try
                {
                    // Quando sessão volta para Active (ex: usuário dá play após pausar), reaplicar volume
                    if (e.State == NAudio.CoreAudioApi.Interfaces.AudioSessionState.AudioSessionStateActive)
                    {
                        // Thread-safe: buscar sessão nas listas compartilhadas
                        AudioSession audioSession = null;
                        lock (_sharedLock)
                        {
                            audioSession = _sharedSessionsGrouped?.FirstOrDefault(s =>
                                s.Sessions?.Any(session => session.GetProcessID == e.ProcessId) == true);
                        }

                        if (audioSession != null && _settedVolume >= 0)
                        {
                            // Verificar se está em _toManage
                            var managedApp = _toManage.FirstOrDefault(app =>
                                !string.IsNullOrEmpty(app) &&
                                !string.IsNullOrEmpty(audioSession.Executable) &&
                                (audioSession.Executable.IndexOf(app, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                 app.IndexOf(audioSession.Executable, StringComparison.OrdinalIgnoreCase) >= 0));

                            if (managedApp != null)
                            {
                                await ChangeAppVolume(managedApp, _settedVolume);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Insert("AudioController", $"Erro ao processar mudança de estado da sessão: {ex.Message}", ex);
                }
            };

            _audioMonitor.DeviceAdded += async (sender, e) =>
            {
                try
                {
                    // Adicionar novo dispositivo à lista compartilhada
                    if (e.Device != null && e.Device.Device != null && e.Device.State == DeviceState.Active)
                    {
                        var newDevice = e.Device.Device;
                        bool wasAdded = false;

                        // Thread-safe: adicionar à lista compartilhada
                        lock (_sharedLock)
                        {
                            if (_sharedDevices == null)
                            {
                                _sharedDevices = new List<MMDevice>();
                            }

                            if (!_sharedDevices.Any(d => d.ID == newDevice.ID))
                            {
                                _sharedDevices.Add(newDevice);
                                wasAdded = true;
                            }
                        }

                        // Aplicar volume se foi adicionado e está sendo gerenciado
                        if (wasAdded && (_audioMode == AudioMode.DevicePlayback || _audioMode == AudioMode.DeviceRecord))
                        {
                            var managedDevice = _toManage.FirstOrDefault(deviceId =>
                                !string.IsNullOrEmpty(deviceId) &&
                                deviceId.Equals(e.Device.Id, StringComparison.OrdinalIgnoreCase));

                            if (managedDevice != null)
                            {
                                await ChangeDeviceVolume(managedDevice, _settedVolume);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Insert("AudioController", $"Erro ao processar novo dispositivo: {ex.Message}", ex);
                }
            };

            _audioMonitor.DeviceRemoved += (sender, e) =>
            {
                try
                {
                    // Thread-safe: remover dispositivo da lista compartilhada
                    lock (_sharedLock)
                    {
                        if (_sharedDevices != null)
                        {
                            var deviceToRemove = _sharedDevices.FirstOrDefault(d => d.ID == e.DeviceId);
                            if (deviceToRemove != null)
                            {
                                _sharedDevices.Remove(deviceToRemove);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Insert("AudioController", $"Erro ao remover dispositivo: {ex.Message}", ex);
                }
            };

            _audioMonitor.DeviceStateChanged += async (sender, e) =>
            {
                try
                {
                    if (e.Device != null && (_audioMode == AudioMode.DevicePlayback || _audioMode == AudioMode.DeviceRecord) && e.NewState == DeviceState.Active)
                    {
                        // Thread-safe: adicionar device à lista compartilhada se não estiver presente
                        if (e.Device.Device != null)
                        {
                            var newDevice = e.Device.Device;
                            lock (_sharedLock)
                            {
                                if (_sharedDevices == null)
                                {
                                    _sharedDevices = new List<MMDevice>();
                                }

                                if (!_sharedDevices.Any(d => d.ID == newDevice.ID))
                                {
                                    _sharedDevices.Add(newDevice);
                                }
                            }
                        }

                        // Verificar se device está sendo gerenciado
                        if (_settedVolume >= 0)
                        {
                            var managedDevice = _toManage.FirstOrDefault(deviceId =>
                                !string.IsNullOrEmpty(deviceId) &&
                                deviceId.Equals(e.Device.Id, StringComparison.OrdinalIgnoreCase));

                            if (managedDevice != null)
                            {
                                // Aplicar volume configurado imediatamente
                                await ChangeDeviceVolume(managedDevice, _settedVolume);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Insert("AudioController", $"Erro ao processar mudança de estado do dispositivo: {ex.Message}", ex);
                }
            };

            _audioMonitor.DefaultDeviceChanged += async (sender, e) =>
            {
                try
                {
                    // Reiniciar AudioMonitor para registrar callbacks no novo dispositivo padrão
                    _audioMonitor.StopMonitoring();
                    _audioMonitor.StartMonitoring();

                    // Atualizar sessões e reaplicar volume
                    await UpdateSessionsOnly();

                    if (_audioMode == AudioMode.Application && _settedVolume >= 0)
                    {
                        foreach (var managedApp in _toManage)
                        {
                            if (!string.IsNullOrEmpty(managedApp))
                            {
                                await ChangeAppVolume(managedApp, _settedVolume);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Insert("AudioController", $"Erro ao processar mudança de dispositivo padrão: {ex.Message}", ex);
                }
            };

            // Iniciar monitoramento de sessões e dispositivos de áudio
            if (!_audioMonitor.IsMonitoring)
            {
                _audioMonitor.StartMonitoring();
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
        public async Task ResetCoreAudioController(MMDeviceEnumerator deviceEnumerator, List<MMDevice> devices = null, List<AudioSessionControl> sessions = null, bool skipLock = false)
        {
            // Execute lock acquisition in thread pool to avoid COM deadlock on STA thread
            await Task.Run(async () =>
            {
                // Skip lock se chamado de UpdateCoreAudioController (permite paralelização)
                bool lockAcquired = skipLock;

                if (!skipLock)
                {
                    // Try to acquire lock with timeout - if can't get lock, skip this reset request
                    lockAcquired = await _coreAudioOperationLock.WaitAsync(100); // Wait max 100ms

                    if (!lockAcquired)
                    {
                        // Another operation in progress, skip this reset to avoid conflicts during disconnection
                        return;
                    }
                }

                _resetingCore = true;

                while (_changingVolume)
                {
                    // Wait for any ongoing volume changes to complete
                    await Task.Delay(50);
                }

                try
                {
                    // Dispose old enumerator if exists
                    if (_deviceEnumerator != null)
                    {
                        try
                        {
                            _deviceEnumerator.Dispose();
                        }
                        catch { }
                        _deviceEnumerator = null;
                        _deviceEnumeratorDisposed = true;
                    }

                    // Set new enumerator
                    _deviceEnumerator = deviceEnumerator;
                    _deviceEnumeratorDisposed = false; // New enumerator is not disposed

                    // NAudio: Simply store devices and sessions (no event subscriptions like AudioSwitcher)
                    // Thread-safe: atualizar listas compartilhadas de devices e sessions
                    // (já atualizadas em GetNewCoreAudioController, mas garantir aqui também)
                    if (devices != null && sessions != null)
                    {
                        lock (_sharedLock)
                        {
                            _sharedDevices = devices;
                            _sharedSessions = sessions;
                            _sharedSessionsGrouped = GroupSessionsByExecutableWithCache(_sharedSessions, _sharedSessionsGrouped);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Insert("AudioController", "Erro ao resetar CoreAudioController", ex);
                }
                finally
                {
                    _resetingCore = false;
                    // Só liberar lock se foi adquirido (não skipLock)
                    if (!skipLock && lockAcquired)
                    {
                        _coreAudioOperationLock.Release();
                    }
                }
            }).ConfigureAwait(false); // Execute entire method in thread pool
        }



        /// <summary>
        /// Updates only the audio sessions without resetting the entire MMDeviceEnumerator
        /// Used when apps change audio device but enumerator is still valid
        /// </summary>
        private async Task UpdateSessionsOnly()
        {
            try
            {
                // Check if MMDeviceEnumerator is initialized
                if (_deviceEnumerator == null)
                {
                    return;
                }

                // Small delay to allow Windows to migrate apps to new device
                await Task.Delay(500);

                // Execute COM operations in thread pool to avoid ContextSwitchDeadlock
                await Task.Run(() =>
                {
                    var newSessions = new List<AudioSessionControl>();

                    // NAudio: Get only Active devices
                    var playbackDevices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

                    // Rebuild sessions from all active devices
                    foreach (var device in playbackDevices)
                    {
                        // Validate device state
                        if (device == null || device.State != DeviceState.Active)
                        {
                            continue;
                        }

                        try
                        {
                            // NAudio: Get audio sessions from device
                            var sessionManager = device.AudioSessionManager;
                            if (sessionManager == null)
                            {
                                continue;
                            }

                            var sessionCollection = sessionManager.Sessions;
                            if (sessionCollection != null)
                            {
                                for (int i = 0; i < sessionCollection.Count; i++)
                                {
                                    var session = sessionCollection[i];
                                    if (session != null)
                                    {
                                        newSessions.Add(session);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Insert("AudioController", $"Erro ao atualizar sessões do dispositivo {device.FriendlyName}", ex);
                        }
                    }

                    // Thread-safe: atualizar listas compartilhadas
                    lock (_sharedLock)
                    {
                        _sharedSessions = newSessions;
                        _sharedSessionsGrouped = GroupSessionsByExecutableWithCache(_sharedSessions, _sharedSessionsGrouped);
                    }
                }).ConfigureAwait(false); // Execute in thread pool, don't return to STA context
            }
            catch (Exception ex)
            {
                Log.Insert("AudioController", "Erro ao atualizar sessões de áudio", ex);
            }
        }

        /// <summary>
        /// Check and optionally reset MMDeviceEnumerator
        /// </summary>
        public async Task CheckCoreAudioController(MMDeviceEnumerator deviceEnumerator, bool reset = false)
        {
            if (reset)
            {
                await ResetCoreAudioController(deviceEnumerator);
            }
            else
            {
                // Just update sessions without full reset
                await UpdateSessionsOnly();
            }
        }

        /// <summary>
        /// Creates a new MMDeviceEnumerator instance using NAudio.CoreAudioApi
        /// Returns tuple (enumerator, devices, sessions)
        /// NAudio provides direct Windows Core Audio API access without Timer_UpdatePeakValue bugs
        /// </summary>
        public static (MMDeviceEnumerator, List<MMDevice>, List<AudioSessionControl>) GetNewCoreAudioController()
        {
            MMDeviceEnumerator deviceEnumerator = null;
            List<MMDevice> devices = null;
            List<AudioSessionControl> sessions = null;

            try
            {
                deviceEnumerator = new MMDeviceEnumerator();
                devices = new List<MMDevice>();
                sessions = new List<AudioSessionControl>();

                // NAudio: Get playback (render) devices - ONLY Active devices
                try
                {
                    var playbackDevices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

                    if (playbackDevices == null)
                    {
                        return (deviceEnumerator, devices, sessions);
                    }

                    foreach (MMDevice device in playbackDevices)
                    {
                        // Validate device state
                        if (device == null || device.State != DeviceState.Active)
                        {
                            continue;
                        }

                        devices.Add(device);

                        // NAudio: Get audio sessions from device
                        try
                        {
                            var sessionManager = device.AudioSessionManager;
                            if (sessionManager != null)
                            {
                                var sessionCollection = sessionManager.Sessions;
                                if (sessionCollection != null)
                                {
                                    for (int i = 0; i < sessionCollection.Count; i++)
                                    {
                                        var session = sessionCollection[i];
                                        if (session != null)
                                        {
                                            sessions.Add(session);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Ignore if device doesn't support sessions
                            Log.Insert("AudioController", $"Erro ao obter sessões do device {device.FriendlyName} durante GetNewCoreAudioController", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Insert("AudioController", $"Erro ao enumerar playback devices: {ex.Message}");
                }

                // NAudio: Get capture (recording) devices - ONLY Active devices
                try
                {
                    var captureDevices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);

                    if (captureDevices == null)
                    {
                        return (deviceEnumerator, devices, sessions);
                    }   

                    foreach (MMDevice device in captureDevices)
                    {
                        // Validate device state
                        if (device == null || device.State != DeviceState.Active)
                        {
                            continue;
                        }

                        devices.Add(device);

                        // NAudio: Get audio sessions from capture device
                        try
                        {
                            var sessionManager = device.AudioSessionManager;
                            if (sessionManager != null)
                            {
                                var sessionCollection = sessionManager.Sessions;
                                if (sessionCollection != null)
                                {
                                    for (int i = 0; i < sessionCollection.Count; i++)
                                    {
                                        var session = sessionCollection[i];
                                        if (session != null)
                                        {
                                            sessions.Add(session);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Ignore if device doesn't support sessions
                            Log.Insert("AudioController", $"Erro ao obter sessões do device {device.FriendlyName} durante GetNewCoreAudioController", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Insert("AudioController", $"Erro ao enumerar capture devices: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Log.Insert("AudioController", "Erro ao criar novo MMDeviceEnumerator", ex);
            }

            // Atualizar variáveis estáticas compartilhadas
            lock (_sharedLock)
            {
                _sharedDevices = devices;
                _sharedSessions = sessions;

                // Agrupar sessões por executável UMA vez usando cache dos _sharedSessionsGrouped anteriores
                _sharedSessionsGrouped = GroupSessionsByExecutableWithCache(sessions, _sharedSessionsGrouped);
            }

            return (deviceEnumerator, devices, sessions);
        }

        /// <summary>
        /// Agrupa sessões por executável usando cache de sessões anteriores
        /// </summary>
        private static List<AudioSession> GroupSessionsByExecutableWithCache(List<AudioSessionControl> sessions, List<AudioSession> previousSessionsGrouped)
        {
            var result = new List<AudioSession>();

            if (sessions == null || sessions.Count == 0)
                return result;

            try
            {
                // Criar cache de PID → Executable a partir das sessões ANTERIORES
                var pidToExeCache = new Dictionary<int, string>();
                if (previousSessionsGrouped != null)
                {
                    foreach (var audioSession in previousSessionsGrouped)
                    {
                        if (audioSession?.Sessions != null)
                        {
                            foreach (var session in audioSession.Sessions)
                            {
                                try
                                {
                                    int pid = (int)session.GetProcessID;
                                    if (!pidToExeCache.ContainsKey(pid))
                                    {
                                        pidToExeCache[pid] = audioSession.Executable;
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }

                int cacheHits = 0;
                int cacheMisses = 0;

                // Agrupar sessões por executável usando CACHE
                var groupedSessions = sessions
                    .Where(s => s != null)
                    .GroupBy(s =>
                    {
                        try
                        {
                            int pid = (int)s.GetProcessID;

                            // Verificar se PID já estava em cache
                            if (pidToExeCache.TryGetValue(pid, out string cachedName))
                            {
                                cacheHits++;
                                return cachedName;
                            }

                            // Não estava no cache, buscar com WMI (lento)
                            cacheMisses++;
                            return GetProcessNameOrExecutableByIdStatic(pid);
                        }
                        catch
                        {
                            return null;
                        }
                    })
                    .Where(g => !string.IsNullOrEmpty(g.Key));

                foreach (var group in groupedSessions)
                {
                    var audioSession = new AudioSession(group.Key);
                    foreach (var session in group)
                    {
                        audioSession.Add(session);
                    }
                    result.Add(audioSession);
                }
            }
            catch (Exception ex)
            {
                Log.Insert("AudioController", $"Erro ao agrupar sessões: {ex.Message}", ex);
            }

            return result;
        }

        /// <summary>
        /// Versão estática de GetProcessNameOrExecutableById
        /// </summary>
        private static string GetProcessNameOrExecutableByIdStatic(int pid)
        {
            if (pid == 0) return string.Empty;

            try
            {
                // Fallback to Process.GetProcessById (rápido)
                try
                {
                    using (var process = System.Diagnostics.Process.GetProcessById(pid))
                    {
                        return process.ProcessName + ".exe";
                    }
                }
                catch
                {
                    // Se falhar, tentar WMI
                    using (var searcher = new System.Management.ManagementObjectSearcher(
                        $"SELECT ExecutablePath, Name FROM Win32_Process WHERE ProcessId = {pid}"))
                    {
                        foreach (System.Management.ManagementObject obj in searcher.Get())
                        {
                            try
                            {
                                string execPath = obj["ExecutablePath"]?.ToString();
                                if (!string.IsNullOrEmpty(execPath))
                                {
                                    return System.IO.Path.GetFileName(execPath);
                                }

                                string processName = obj["Name"]?.ToString();
                                if (!string.IsNullOrEmpty(processName))
                                {
                                    return processName;
                                }
                            }
                            finally
                            {
                                obj?.Dispose();
                            }
                        }
                    }
                }
            }
            catch { }

            return string.Empty;
        }

        public List<string[]> GetSessions()
        {
            List<string[]> sessions = new List<string[]>();

            MMDeviceEnumerator actualEnumerator = (_deviceEnumerator != null ? _deviceEnumerator : new MMDeviceEnumerator());

            // NAudio: Get only Active devices
            try
            {
                var devices = actualEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

                foreach (MMDevice device in devices)
                {
                    // Validate device state
                    if (device == null || device.State != DeviceState.Active)
                    {
                        continue;
                    }

                    try
                    {
                        // NAudio: Get audio sessions from device
                        var sessionManager = device.AudioSessionManager;
                        if (sessionManager == null)
                        {
                            continue;
                        }

                        var sessionCollection = sessionManager.Sessions;
                        if (sessionCollection != null)
                        {
                            for (int i = 0; i < sessionCollection.Count; i++)
                            {
                                var session = sessionCollection[i];
                                if (session != null)
                                {
                                    uint processId = session.GetProcessID;
                                    string displayName = session.DisplayName ?? "";
                                    float volume = session.SimpleAudioVolume.Volume * 100; // NAudio: Volume is 0.0-1.0

                                    sessions.Add(new string[] {
                                        processId.ToString(),
                                        displayName,
                                        GetProcessNameOrExecutableById((int)processId),
                                        ((int)volume).ToString() + "%"
                                    });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Ignore devices that fail - prevents UI from crashing
                        Log.Insert("AudioController", $"Erro ao obter sessões do device {device.FriendlyName} em GetSessions", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Insert("AudioController", $"Erro ao enumerar dispositivos em GetSessions: {ex.Message}");
            }

            return sessions;
        }


        /// <summary>
        /// Gets process name or executable by PID using WMI (universal, no Win32Exception)
        /// </summary>
        private string GetProcessNameOrExecutableById(int pid)
        {
            if (pid == 0) return string.Empty;

            try
            {
                // Try WMI first (more reliable, no Win32Exception)
                using (var searcher = new System.Management.ManagementObjectSearcher(
                    $"SELECT ExecutablePath, Name FROM Win32_Process WHERE ProcessId = {pid}"))
                {
                    foreach (System.Management.ManagementObject obj in searcher.Get())
                    {
                        try
                        {
                            // Try to get executable path first (most accurate)
                            string execPath = obj["ExecutablePath"]?.ToString();
                            if (!string.IsNullOrEmpty(execPath))
                            {
                                return System.IO.Path.GetFileName(execPath);
                            }

                            // Fallback to process name
                            string processName = obj["Name"]?.ToString();
                            if (!string.IsNullOrEmpty(processName))
                            {
                                return processName;
                            }
                        }
                        finally
                        {
                            obj?.Dispose();
                        }
                    }
                }

                // Fallback to Process.GetProcessById (faster but can throw Win32Exception)
                try
                {
                    using (var process = System.Diagnostics.Process.GetProcessById(pid))
                    {
                        return process.ProcessName + ".exe";
                    }
                }
                catch
                {
                    // Fallback failed - return empty
                    return string.Empty;
                }
            }
            catch
            {
                // Any error - fail silently
                return string.Empty;
            }
        }

        /// <summary>
        /// Checks if process info contains specified string using WMI (universal, no Win32Exception)
        /// </summary>
        private bool CheckProcessInfo(int pid, string info)
        {
            if (pid == 0 || string.IsNullOrEmpty(info)) return false;

            try
            {
                // Try WMI first (more reliable, no Win32Exception)
                using (var searcher = new System.Management.ManagementObjectSearcher(
                    $"SELECT ExecutablePath, Name FROM Win32_Process WHERE ProcessId = {pid}"))
                {
                    foreach (System.Management.ManagementObject obj in searcher.Get())
                    {
                        try
                        {
                            // Check executable path
                            string execPath = obj["ExecutablePath"]?.ToString();
                            if (!string.IsNullOrEmpty(execPath) && execPath.Contains(info))
                            {
                                return true;
                            }

                            // Check process name
                            string processName = obj["Name"]?.ToString();
                            if (!string.IsNullOrEmpty(processName) && processName.Contains(info))
                            {
                                return true;
                            }
                        }
                        finally
                        {
                            obj?.Dispose();
                        }
                    }
                }

                // Fallback to Process.GetProcessById (faster but can throw Win32Exception)
                try
                {
                    using (var process = System.Diagnostics.Process.GetProcessById(pid))
                    {
                        string name = process.ProcessName;
                        if (!string.IsNullOrEmpty(name) && name.Contains(info))
                        {
                            return true;
                        }
                    }
                }
                catch
                {
                    // Fallback failed - ignore
                }

                return false;
            }
            catch
            {
                // Any error - fail silently
                return false;
            }
        }


        private async Task ChangeAppVolume(String AppExecutable, int SettedVolume)
        {
            // Execute lock acquisition in thread pool to avoid COM deadlock on STA thread
            await Task.Run(async () =>
            {
                // SKIP immediately if reset is in progress - don't even try to acquire lock
                if (_resetingCore)
                {
                    return;
                }

                // Try to acquire lock - if another volume operation in progress, skip this request
                bool lockAcquired = await _coreAudioOperationLock.WaitAsync(50); // Wait max 50ms

                if (!lockAcquired)
                {
                    // Another operation in progress, skip to avoid conflicts
                    return;
                }

                try
                {
                    // Thread-safe: buscar TODAS as sessões com o mesmo executável (não apenas a primeira)
                    // Usar comparação flexível: permite match parcial (ex: "firefox" match "firefox.exe")
                    List<AudioSession> audioSessions = null;
                    lock (_sharedLock)
                    {
                        audioSessions = _sharedSessionsGrouped?.Where(s =>
                            !string.IsNullOrEmpty(s?.Executable) &&
                            !string.IsNullOrEmpty(AppExecutable) &&
                            (s.Executable.IndexOf(AppExecutable, StringComparison.OrdinalIgnoreCase) >= 0 ||
                             AppExecutable.IndexOf(s.Executable, StringComparison.OrdinalIgnoreCase) >= 0)
                        ).ToList();
                    }

                    if (audioSessions == null || audioSessions.Count == 0)
                    {
                        return;
                    }

                    // Aplicar volume em TODAS as sessões com o mesmo executável
                    foreach (var audioSession in audioSessions)
                    {
                        if (audioSession?.Sessions == null)
                            continue;

                        foreach (var session in audioSession.Sessions)
                        {
                            if (session == null)
                                continue;

                            try
                            {
                                // NAudio: Volume is 0.0-1.0, convert from 0-100
                                float currentVolume = session.SimpleAudioVolume.Volume * 100;

                                // Only set volume if it actually changed (prevents unnecessary operations)
                                if (Math.Round(currentVolume) != SettedVolume)
                                {
                                    await SetVolumeAndMuteAsync(session, SettedVolume);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Insert("AudioController", $"Erro ao alterar volume da sessão: {ex.Message}", ex);
                            }
                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                    // Device enumerator was disposed during operation - expected during cleanup
                }
                catch (Exception ex)
                {
                    Log.Insert("AudioController", $"Erro inesperado ao alterar volume da aplicação {AppExecutable}", ex);
                }
                finally
                {
                    _coreAudioOperationLock.Release(); // Always release lock
                }
            }).ConfigureAwait(false); // Execute entire method in thread pool
        }

        private async Task ChangeDeviceVolume(String deviceId, int SettedVolume)
        {
            // Execute lock acquisition in thread pool to avoid COM deadlock on STA thread
            await Task.Run(async () =>
            {
                // SKIP immediately if reset is in progress - don't even try to acquire lock
                if (_resetingCore)
                {
                    return;
                }

                // Try to acquire lock - if another volume operation in progress, skip this request
                bool lockAcquired = await _coreAudioOperationLock.WaitAsync(50); // Wait max 50ms

                if (!lockAcquired)
                {
                    // Another operation in progress, skip to avoid conflicts
                    return;
                }

                try
                {
                    // Capture local reference to prevent race condition
                    MMDeviceEnumerator localEnumerator = _deviceEnumerator;

                    if (localEnumerator == null || _deviceEnumeratorDisposed)
                    {
                        return; // Finally block will release the lock
                    }

                    // Execute COM operations in thread pool to avoid ContextSwitchDeadlock
                    await Task.Run(() =>
                    {
                        // Thread-safe: buscar device na lista compartilhada
                        MMDevice device = null;
                        lock (_sharedLock)
                        {
                            if (_sharedDevices != null)
                            {
                                device = _sharedDevices.FirstOrDefault(x => x.ID == deviceId);
                            }
                        }

                        if (device != null)
                        {
                            // NAudio: Volume is 0.0-1.0, convert from 0-100
                            float currentVolume = device.AudioEndpointVolume.MasterVolumeLevelScalar * 100;

                            if (Math.Round(currentVolume) != SettedVolume)
                            {
                                // NAudio: No event subscriptions - simpler architecture
                                SetVolumeAndMuteAsync(device, SettedVolume).Wait();
                            }
                        }
                    }).ConfigureAwait(false); // Execute in thread pool, don't return to STA context
                }
                catch (ObjectDisposedException)
                {
                    // MMDeviceEnumerator was disposed during execution - fail silently
                }
                catch (NullReferenceException)
                {
                    // MMDeviceEnumerator was null during execution - fail silently
                }
                catch (Exception ex)
                {
                    Log.Insert("AudioController", "Problema ocorrido ao alterar volume do dispositivo.", ex);
                }
                finally
                {
                    _coreAudioOperationLock.Release(); // Always release lock
                }
            }).ConfigureAwait(false); // Execute entire method in thread pool
        }

        private async Task SetVolumeAndMuteAsync(MMDevice device, int SettedVolume)
        {
            try
            {
                // Check if device is in a valid state before attempting volume operations
                if (device == null || device.State != DeviceState.Active)
                {
                    return;
                }

                // NAudio: Set volume (0.0-1.0) and mute
                // Clamp value to valid range [0, 100] to prevent errors
                int clampedVolume = Math.Max(0, Math.Min(100, SettedVolume));
                float volumeScalar = clampedVolume / 100f;

                // Ensure volumeScalar is exactly within [0.0, 1.0] range
                volumeScalar = Math.Max(0.0f, Math.Min(1.0f, volumeScalar));

                device.AudioEndpointVolume.MasterVolumeLevelScalar = volumeScalar;
                device.AudioEndpointVolume.Mute = (clampedVolume == 0);
            }
            catch (Exception ex)
            {
                // Log but don't throw - prevents blocking the entire audio control flow
                Log.Insert("AudioController", $"Failed to set volume for device {device?.FriendlyName}: {ex.Message}", ex);
            }
        }

        private async Task SetVolumeAndMuteAsync(AudioSessionControl session, int SettedVolume)
        {
            try
            {
                // Check if session is valid
                if (session == null)
                {
                    return;
                }

                // NAudio: Set volume (0.0-1.0) and mute via SimpleAudioVolume
                // Clamp value to valid range [0, 100] to prevent errors
                int clampedVolume = Math.Max(0, Math.Min(100, SettedVolume));
                float volumeScalar = clampedVolume / 100f;

                // Ensure volumeScalar is exactly within [0.0, 1.0] range
                volumeScalar = Math.Max(0.0f, Math.Min(1.0f, volumeScalar));

                session.SimpleAudioVolume.Volume = volumeScalar;
                session.SimpleAudioVolume.Mute = (clampedVolume == 0);
            }
            catch (Exception ex)
            {
                // Log but don't throw - prevents blocking the entire audio control flow
                Log.Insert("AudioController", $"Failed to set volume for session {session?.DisplayName}: {ex.Message}", ex);
            }
        }

        public async Task ChangeVolume()
        {
            // Execute entire method in thread pool to avoid COM deadlock on STA thread
            await Task.Run(async () =>
            {
                // SKIP if reset is in progress - don't wait, just ignore this request
                if (_resetingCore)
                {
                    return;
                }

                _changingVolume = true;

                try
                {
                    if (_audioMode == AudioMode.None || _toManage.Count == 0)
                    {
                        return;
                    }

                    if (_deviceEnumerator == null || _deviceEnumeratorDisposed)
                    {
                        var (deviceEnumerator, devices, sessions) = GetNewCoreAudioController();
                        await ResetCoreAudioController(deviceEnumerator, devices, sessions);
                    }

                    switch (_audioMode)
                    {
                        case AudioMode.Application:
                            // NAudio: No event subscriptions to cancel/dispose
                            _currentCtsSession.Cancel();
                            _currentCtsSession.Dispose();
                            _currentCtsSession = new CancellationTokenSource();

                            foreach (string app in _toManage)
                            {
                                await ChangeAppVolume(app, SettedVolume);
                            }
                            break;
                        case AudioMode.DevicePlayback:
                            // NAudio: No event subscriptions to dispose
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
            }).ConfigureAwait(false); // Execute entire method in thread pool
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
