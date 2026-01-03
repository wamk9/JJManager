using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace JJManager.Class.App.Input.AudioController
{
    /// <summary>
    /// Monitora sessões de áudio e dispositivos do sistema e notifica quando há mudanças
    /// </summary>
    public class AudioMonitor : IDisposable
    {
        private MMDeviceEnumerator _enumerator;
        private MMDevice _device;
        private SessionNotificationHandler _sessionNotification;
        private DeviceNotificationHandler _deviceNotification;
        private SessionCollection _sessions;
        private Dictionary<uint, AudioSessionControl> _monitoredSessions;
        private Dictionary<string, DeviceState> _lastDeviceStates;
        private Dictionary<string, DateTime> _lastPropertyChangeTime;
        private readonly object _deviceStateLock = new object();
        private readonly object _propertyThrottleLock = new object();
        private readonly TimeSpan _propertyChangeThrottle = TimeSpan.FromMilliseconds(500); // Throttle: max 1 evento a cada 500ms por device
        private bool _isMonitoring;
        private DataFlow _currentDataFlow;
        private Role _currentRole;

        /// <summary>
        /// Indica se o monitoramento está ativo
        /// </summary>
        public bool IsMonitoring => _isMonitoring;

        #region Session Events

        /// <summary>
        /// Disparado quando uma nova sessão de áudio é criada
        /// </summary>
        public event EventHandler<SessionCreatedEventArgs> SessionCreated;

        /// <summary>
        /// Disparado quando uma sessão de áudio é desconectada/fechada
        /// </summary>
        public event EventHandler<SessionDisconnectedEventArgs> SessionDisconnected;

        /// <summary>
        /// Disparado quando o volume de uma sessão é alterado
        /// </summary>
        public event EventHandler<SessionVolumeChangedEventArgs> SessionVolumeChanged;

        /// <summary>
        /// Disparado quando o estado de uma sessão muda (ativo, inativo, expirado)
        /// </summary>
        public event EventHandler<SessionStateChangedEventArgs> SessionStateChanged;

        /// <summary>
        /// Disparado quando o nome de exibição de uma sessão muda
        /// </summary>
        public event EventHandler<SessionDisplayNameChangedEventArgs> SessionDisplayNameChanged;

        #endregion

        #region Device Events

        /// <summary>
        /// Disparado quando um novo dispositivo de áudio é adicionado ao sistema
        /// </summary>
        public event EventHandler<DeviceAddedEventArgs> DeviceAdded;

        /// <summary>
        /// Disparado quando um dispositivo de áudio é removido do sistema
        /// </summary>
        public event EventHandler<DeviceRemovedEventArgs> DeviceRemoved;

        /// <summary>
        /// Disparado quando o dispositivo padrão muda
        /// </summary>
        public event EventHandler<DefaultDeviceChangedEventArgs> DefaultDeviceChanged;

        /// <summary>
        /// Disparado quando o estado de um dispositivo muda (ativo, desabilitado, não presente, etc)
        /// </summary>
        public event EventHandler<DeviceStateChangedEventArgs> DeviceStateChanged;

        /// <summary>
        /// Disparado quando uma propriedade de um dispositivo muda
        /// </summary>
        public event EventHandler<DevicePropertyChangedEventArgs> DevicePropertyChanged;

        #endregion

        public AudioMonitor()
        {
            _monitoredSessions = new Dictionary<uint, AudioSessionControl>();
            _lastDeviceStates = new Dictionary<string, DeviceState>();
            _lastPropertyChangeTime = new Dictionary<string, DateTime>();
        }

        /// <summary>
        /// Inicia o monitoramento de sessões de áudio e dispositivos
        /// </summary>
        /// <param name="dataFlow">Tipo de dispositivo (Render = saída, Capture = entrada)</param>
        /// <param name="role">Papel do dispositivo</param>
        public void StartMonitoring(DataFlow dataFlow = DataFlow.Render, Role role = Role.Multimedia)
        {
            if (_isMonitoring)
            {
                return;
            }

            try
            {
                _currentDataFlow = dataFlow;
                _currentRole = role;

                // Criar enumerador de dispositivos
                _enumerator = new MMDeviceEnumerator();

                // Registrar notificações de dispositivos
                _deviceNotification = new DeviceNotificationHandler(this);
                _enumerator.RegisterEndpointNotificationCallback(_deviceNotification);

                // Obter o dispositivo de áudio padrão
                _device = _enumerator.GetDefaultAudioEndpoint(dataFlow, role);

                // Obter o gerenciador de sessões
                var sessionManager = _device.AudioSessionManager;
                _sessions = sessionManager.Sessions;

                // Registrar callback para novas sessões
                _sessionNotification = new SessionNotificationHandler(this);
                sessionManager.OnSessionCreated += SessionManager_OnSessionCreated;

                // Monitorar sessões existentes
                MonitorExistingSessions();

                _isMonitoring = true;
            }
            catch (Exception ex)
            {
                Log.Insert("AudioMonitor", $"Erro ao iniciar monitoramento: {ex.Message}");
                throw;
            }
        }

        private void SessionManager_OnSessionCreated(object sender, IAudioSessionControl newSession)
        {
            try
            {
                OnNewSessionCreated(new AudioSessionControl(newSession));
            }
            catch (Exception ex)
            {
                Log.Insert("AudioMonitor", $"Erro ao processar nova sessão criada: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Para o monitoramento de sessões e dispositivos
        /// </summary>
        public void StopMonitoring()
        {
            if (!_isMonitoring)
                return;

            try
            {
                // Desregistrar notificações de dispositivos
                if (_enumerator != null && _deviceNotification != null)
                {
                    _enumerator.UnregisterEndpointNotificationCallback(_deviceNotification);
                }

                // Desregistrar callback de novas sessões
                if (_device != null)
                {
                    var sessionManager = _device.AudioSessionManager;
                    sessionManager.OnSessionCreated -= SessionManager_OnSessionCreated;
                }

                // Desregistrar eventos de todas as sessões monitoradas
                foreach (var session in _monitoredSessions.Values)
                {
                    try
                    {
                        session.UnRegisterEventClient(_sessionNotification);
                    }
                    catch { }
                }

                _monitoredSessions.Clear();

                // Limpar estados de dispositivos de forma thread-safe
                lock (_deviceStateLock)
                {
                    _lastDeviceStates.Clear();
                }

                // Limpar timestamps de throttling
                lock (_propertyThrottleLock)
                {
                    _lastPropertyChangeTime.Clear();
                }

                _isMonitoring = false;
            }
            catch (Exception ex)
            {
                Log.Insert("AudioMonitor", $"Erro ao parar monitoramento: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém todas as sessões de áudio ativas no momento
        /// </summary>
        public List<AudioSessionInfo> GetActiveSessions()
        {
            var result = new List<AudioSessionInfo>();

            if (_sessions == null)
                return result;

            try
            {
                for (int i = 0; i < _sessions.Count; i++)
                {
                    var session = _sessions[i];
                    var info = GetSessionInfo(session);
                    if (info != null)
                        result.Add(info);
                }
            }
            catch (Exception ex)
            {
                Log.Insert("AudioMonitor", $"Erro ao obter sessões ativas: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Obtém todos os dispositivos de áudio do sistema
        /// </summary>
        /// <param name="dataFlow">Tipo de dispositivo (All, Render, Capture)</param>
        /// <param name="state">Estado do dispositivo (All, Active, Disabled, etc)</param>
        public List<AudioDeviceInfo> GetDevices(DataFlow dataFlow = DataFlow.All, DeviceState state = DeviceState.Active)
        {
            var result = new List<AudioDeviceInfo>();

            if (_enumerator == null)
                return result;

            try
            {
                var devices = _enumerator.EnumerateAudioEndPoints(dataFlow, state);

                foreach (var device in devices)
                {
                    var info = GetDeviceInfo(device);
                    if (info != null)
                        result.Add(info);
                }
            }
            catch (Exception ex)
            {
                Log.Insert("AudioMonitor", $"Erro ao obter dispositivos: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Obtém o dispositivo de áudio padrão atual
        /// </summary>
        public AudioDeviceInfo GetDefaultDevice(DataFlow dataFlow = DataFlow.Render, Role role = Role.Multimedia)
        {
            try
            {
                var device = _enumerator.GetDefaultAudioEndpoint(dataFlow, role);
                return GetDeviceInfo(device);
            }
            catch (Exception ex)
            {
                Log.Insert("AudioMonitor", $"Erro ao obter dispositivo padrão: {ex.Message}");
                return null;
            }
        }

        private void MonitorExistingSessions()
        {
            for (int i = 0; i < _sessions.Count; i++)
            {
                try
                {
                    var session = _sessions[i];
                    RegisterSessionEvents(session);
                }
                catch (Exception ex)
                {
                    Log.Insert("AudioMonitor", $"Erro ao monitorar sessão existente: {ex.Message}");
                }
            }
        }

        private void OnNewSessionCreated(AudioSessionControl session)
        {
            try
            {
                RegisterSessionEvents(session);

                var info = GetSessionInfo(session);
                SessionCreated?.Invoke(this, new SessionCreatedEventArgs(session, info));
            }
            catch (Exception ex)
            {
                Log.Insert("AudioMonitor", $"Erro ao processar nova sessão: {ex.Message}", ex);
            }
        }

        private void RegisterSessionEvents(AudioSessionControl session)
        {
            try
            {
                uint pid = session.GetProcessID;

                if (!_monitoredSessions.ContainsKey(pid))
                {
                    _monitoredSessions[pid] = session;
                    session.RegisterEventClient(_sessionNotification);
                }
            }
            catch (Exception ex)
            {
                Log.Insert("AudioMonitor", $"Erro ao registrar eventos da sessão: {ex.Message}");
            }
        }

        private AudioSessionInfo GetSessionInfo(AudioSessionControl session)
        {
            try
            {
                uint pid = session.GetProcessID;
                string displayName = session.DisplayName;
                string iconPath = session.IconPath;
                float volume = session.SimpleAudioVolume.Volume;
                bool isMuted = session.SimpleAudioVolume.Mute;
                AudioSessionState state = session.State;

                string processName = "";
                try
                {
                    var process = Process.GetProcessById((int)pid);
                    processName = process.ProcessName;
                }
                catch { }

                return new AudioSessionInfo
                {
                    ProcessId = pid,
                    ProcessName = processName,
                    DisplayName = displayName,
                    IconPath = iconPath,
                    Volume = volume,
                    IsMuted = isMuted,
                    State = state,
                    Session = session
                };
            }
            catch
            {
                return null;
            }
        }

        private AudioDeviceInfo GetDeviceInfo(MMDevice device)
        {
            try
            {
                return new AudioDeviceInfo
                {
                    Id = device.ID,
                    FriendlyName = device.FriendlyName,
                    DeviceFriendlyName = device.DeviceFriendlyName,
                    IconPath = device.IconPath,
                    DataFlow = device.DataFlow,
                    State = device.State,
                    Device = device
                };
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            StopMonitoring();

            _device?.Dispose();
            _enumerator?.Dispose();
            _monitoredSessions?.Clear();
        }

        #region Nested Classes

        /// <summary>
        /// Handler interno para notificações de sessão
        /// </summary>
        private class SessionNotificationHandler : IAudioSessionNotification, IAudioSessionEventsHandler
        {
            private readonly AudioMonitor _monitor;

            public SessionNotificationHandler(AudioMonitor monitor)
            {
                _monitor = monitor;
            }

            // IAudioSessionNotification
            public int OnSessionCreated(IAudioSessionControl newSession)
            {
                _monitor.OnNewSessionCreated(new AudioSessionControl(newSession));
                return 0; // S_OK (HRESULT success)
            }

            // IAudioSessionEventsHandler
            public void OnVolumeChanged(float volume, bool isMuted)
            {
                _monitor.SessionVolumeChanged?.Invoke(_monitor,
                    new SessionVolumeChangedEventArgs(0, volume, isMuted));
            }

            public void OnDisplayNameChanged(string displayName)
            {
                _monitor.SessionDisplayNameChanged?.Invoke(_monitor,
                    new SessionDisplayNameChangedEventArgs(0, displayName));
            }

            public void OnStateChanged(AudioSessionState state)
            {
                _monitor.SessionStateChanged?.Invoke(_monitor,
                    new SessionStateChangedEventArgs(0, state));
            }

            public void OnSessionDisconnected(AudioSessionDisconnectReason disconnectReason)
            {
                _monitor.SessionDisconnected?.Invoke(_monitor,
                    new SessionDisconnectedEventArgs(0, disconnectReason));
            }

            public void OnIconPathChanged(string iconPath) { }
            public void OnChannelVolumeChanged(uint channelCount, IntPtr newVolumes, uint channelIndex) { }
            public void OnGroupingParamChanged(ref Guid groupingId) { }
        }

        /// <summary>
        /// Handler interno para notificações de dispositivos
        /// </summary>
        private class DeviceNotificationHandler : IMMNotificationClient
        {
            private readonly AudioMonitor _monitor;

            public DeviceNotificationHandler(AudioMonitor monitor)
            {
                _monitor = monitor;
            }

            public void OnDeviceAdded(string deviceId)
            {
                try
                {
                    // Removidos logs excessivos que causavam overhead de CPU
                    var device = _monitor._enumerator.GetDevice(deviceId);
                    var info = _monitor.GetDeviceInfo(device);

                    _monitor.DeviceAdded?.Invoke(_monitor, new DeviceAddedEventArgs(info));
                }
                catch (Exception ex)
                {
                    Log.Insert("AudioMonitor", $"Erro ao processar dispositivo adicionado: {ex.Message}", ex);
                }
            }

            public void OnDeviceRemoved(string deviceId)
            {
                _monitor.DeviceRemoved?.Invoke(_monitor, new DeviceRemovedEventArgs(deviceId));
            }

            public void OnDeviceStateChanged(string deviceId, DeviceState newState)
            {
                try
                {
                    bool shouldNotify = false;

                    // Usar lock para evitar race conditions entre threads
                    lock (_monitor._deviceStateLock)
                    {
                        // Verificar se o estado realmente mudou
                        if (_monitor._lastDeviceStates.TryGetValue(deviceId, out DeviceState previousState))
                        {
                            // Estado já registrado, verificar se mudou
                            if (previousState == newState)
                            {
                                // Mesmo estado, ignorar (Windows dispara callbacks duplicados)
                                return;
                            }
                        }

                        // Atualizar o estado rastreado
                        _monitor._lastDeviceStates[deviceId] = newState;
                        shouldNotify = true;
                    }

                    // Disparar evento apenas se o estado mudou (fora do lock para evitar deadlocks)
                    if (shouldNotify)
                    {
                        var device = _monitor._enumerator.GetDevice(deviceId);
                        var info = _monitor.GetDeviceInfo(device);

                        _monitor.DeviceStateChanged?.Invoke(_monitor, new DeviceStateChangedEventArgs(info, newState));
                    }
                }
                catch (Exception ex)
                {
                    Log.Insert("AudioMonitor", $"Erro ao processar mudança de estado: {ex.Message}", ex);
                }
            }

            public void OnDefaultDeviceChanged(DataFlow dataFlow, Role role, string defaultDeviceId)
            {
                try
                {
                    var device = _monitor._enumerator.GetDevice(defaultDeviceId);
                    var info = _monitor.GetDeviceInfo(device);

                    _monitor.DefaultDeviceChanged?.Invoke(_monitor,
                        new DefaultDeviceChangedEventArgs(info, dataFlow, role));
                }
                catch (Exception ex)
                {
                    Log.Insert("AudioMonitor", $"Erro ao processar mudança de dispositivo padrão: {ex.Message}");
                }
            }

            public void OnPropertyValueChanged(string deviceId, PropertyKey propertyKey)
            {
                // OTIMIZAÇÃO CRÍTICA: Este callback é chamado centenas de vezes por segundo
                // Verificar primeiro se há listeners (early exit se ninguém está ouvindo)
                if (_monitor.DevicePropertyChanged == null)
                {
                    return; // Ninguém está ouvindo, não fazer nada
                }

                try
                {
                    bool shouldNotify = false;

                    // Throttling: Limitar a 1 evento a cada 500ms por deviceId
                    lock (_monitor._propertyThrottleLock)
                    {
                        DateTime now = DateTime.UtcNow;

                        if (_monitor._lastPropertyChangeTime.TryGetValue(deviceId, out DateTime lastTime))
                        {
                            // Verificar se passou tempo suficiente desde a última notificação
                            if ((now - lastTime) < _monitor._propertyChangeThrottle)
                            {
                                return; // Throttled - ignorar este evento
                            }
                        }

                        // Atualizar timestamp
                        _monitor._lastPropertyChangeTime[deviceId] = now;
                        shouldNotify = true;
                    }

                    // Processar evento (fora do lock)
                    if (shouldNotify)
                    {
                        var device = _monitor._enumerator.GetDevice(deviceId);
                        var info = _monitor.GetDeviceInfo(device);

                        _monitor.DevicePropertyChanged?.Invoke(_monitor,
                            new DevicePropertyChangedEventArgs(info, propertyKey));
                    }
                }
                catch (Exception ex)
                {
                    Log.Insert("AudioMonitor", $"Erro ao processar mudança de propriedade: {ex.Message}");
                }
            }
        }

        #endregion
    }

    #region Session Event Args Classes

    public class SessionCreatedEventArgs : EventArgs
    {
        public AudioSessionControl Session { get; }
        public AudioSessionInfo Info { get; }

        public SessionCreatedEventArgs(AudioSessionControl session, AudioSessionInfo info)
        {
            Session = session;
            Info = info;
        }
    }

    public class SessionDisconnectedEventArgs : EventArgs
    {
        public uint ProcessId { get; }
        public AudioSessionDisconnectReason Reason { get; }

        public SessionDisconnectedEventArgs(uint processId, AudioSessionDisconnectReason reason)
        {
            ProcessId = processId;
            Reason = reason;
        }
    }

    public class SessionVolumeChangedEventArgs : EventArgs
    {
        public uint ProcessId { get; }
        public float Volume { get; }
        public bool IsMuted { get; }

        public SessionVolumeChangedEventArgs(uint processId, float volume, bool isMuted)
        {
            ProcessId = processId;
            Volume = volume;
            IsMuted = isMuted;
        }
    }

    public class SessionStateChangedEventArgs : EventArgs
    {
        public uint ProcessId { get; }
        public AudioSessionState State { get; }

        public SessionStateChangedEventArgs(uint processId, AudioSessionState state)
        {
            ProcessId = processId;
            State = state;
        }
    }

    public class SessionDisplayNameChangedEventArgs : EventArgs
    {
        public uint ProcessId { get; }
        public string DisplayName { get; }

        public SessionDisplayNameChangedEventArgs(uint processId, string displayName)
        {
            ProcessId = processId;
            DisplayName = displayName;
        }
    }

    #endregion

    #region Device Event Args Classes

    public class DeviceAddedEventArgs : EventArgs
    {
        public AudioDeviceInfo Device { get; }

        public DeviceAddedEventArgs(AudioDeviceInfo device)
        {
            Device = device;
        }
    }

    public class DeviceRemovedEventArgs : EventArgs
    {
        public string DeviceId { get; }

        public DeviceRemovedEventArgs(string deviceId)
        {
            DeviceId = deviceId;
        }
    }

    public class DeviceStateChangedEventArgs : EventArgs
    {
        public AudioDeviceInfo Device { get; }
        public DeviceState NewState { get; }

        public DeviceStateChangedEventArgs(AudioDeviceInfo device, DeviceState newState)
        {
            Device = device;
            NewState = newState;
        }
    }

    public class DefaultDeviceChangedEventArgs : EventArgs
    {
        public AudioDeviceInfo Device { get; }
        public DataFlow DataFlow { get; }
        public Role Role { get; }

        public DefaultDeviceChangedEventArgs(AudioDeviceInfo device, DataFlow dataFlow, Role role)
        {
            Device = device;
            DataFlow = dataFlow;
            Role = role;
        }
    }

    public class DevicePropertyChangedEventArgs : EventArgs
    {
        public AudioDeviceInfo Device { get; }
        public PropertyKey PropertyKey { get; }

        public DevicePropertyChangedEventArgs(AudioDeviceInfo device, PropertyKey propertyKey)
        {
            Device = device;
            PropertyKey = propertyKey;
        }
    }

    #endregion

    #region Info Classes

    /// <summary>
    /// Informações sobre uma sessão de áudio
    /// </summary>
    public class AudioSessionInfo
    {
        public uint ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string DisplayName { get; set; }
        public string IconPath { get; set; }
        public float Volume { get; set; }
        public bool IsMuted { get; set; }
        public AudioSessionState State { get; set; }
        public AudioSessionControl Session { get; set; }
    }

    /// <summary>
    /// Informações sobre um dispositivo de áudio
    /// </summary>
    public class AudioDeviceInfo
    {
        public string Id { get; set; }
        public string FriendlyName { get; set; }
        public string DeviceFriendlyName { get; set; }
        public string IconPath { get; set; }
        public DataFlow DataFlow { get; set; }
        public DeviceState State { get; set; }
        public MMDevice Device { get; set; }
    }

    #endregion
}
