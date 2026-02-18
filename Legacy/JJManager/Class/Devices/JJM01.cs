using NAudio.CoreAudioApi;
using HidSharp;
using JJManager.Class.App.Input;
using JJManager.Class.App.Input.AudioController;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProfileClass = JJManager.Class.App.Profile.Profile;
using HIDClass = JJManager.Class.Devices.Connections.HID;
using HIDMessage = JJManager.Class.Devices.Connections.HIDMessage;

namespace JJManager.Class.Devices
{
    public class JJM01 : HIDClass, IDisposable
    {
        private volatile bool _requesting = false;
        private volatile bool _sending = false;
        private volatile bool _updatingAudioController = false; // Prevents concurrent audio controller updates
        private readonly int _connectionTimeoutLimit = 10;
        private int _actualConnectionTimeout = 0;
        private bool _disposed = false;

        // Profile change request (safer than changing profile directly from UI thread)
        private volatile string _pendingProfileName = null;

        // Public property to check if communication is healthy
        public bool IsCommunicationHealthy => _actualConnectionTimeout == 0;

        // Keep-alive tracking
        private DateTime _lastKeepAlive = DateTime.MinValue;
        private readonly TimeSpan _keepAliveInterval = TimeSpan.FromSeconds(3);

        // Request rate limiting (to avoid HID overload while keeping inputs responsive)
        private DateTime _lastRequest = DateTime.MinValue;
        private readonly TimeSpan _requestInterval = TimeSpan.FromMilliseconds(50); // 20 requests/second max

        // Audio session dirty checking (only update when sessions change)
        // Track PIDs per input for granular updates
        private Dictionary<int, HashSet<int>> _lastActivePIDsByInput = new Dictionary<int, HashSet<int>>();
        private DateTime _lastAudioSessionCheck = DateTime.MinValue;
        private readonly TimeSpan _audioSessionCheckInterval = TimeSpan.FromSeconds(5);

        public JJM01(HidDevice hidDevice) : base (hidDevice)
        {
            _cmds = new HashSet<ushort>
            {
                0x0001, // Set Input Name
                0x0002, // Request All Inputs
                0x00FF  // Device Info
            };

            RestartClass();
        }

        /// <summary>
        /// Requests a profile change in a thread-safe manner.
        /// The actual profile change will happen in the next loop iteration.
        /// </summary>
        /// <param name="profileName">Name of the profile to switch to</param>
        public void RequestProfileChange(string profileName)
        {
            _pendingProfileName = profileName;
        }

        private void RestartClass()
        {
            // Clean up audio resources
            CleanupAudioResources();

            // Reset tracking variables
            _lastKeepAlive = DateTime.MinValue;
            _lastRequest = DateTime.MinValue;
            _lastAudioSessionCheck = DateTime.MinValue;
            _lastActivePIDsByInput.Clear();
            _updatingAudioController = false;

            _actionReceivingData = () => { Task.Run(async () => await ActionMainLoop()); };
            _actionSendingData = null; // Não usado mais - tudo em ActionMainLoop
            _actionResetParams = () => { Task.Run(() => RestartClass()); };
        }

        private void CleanupAudioResources()
        {
            try
            {
                // Clean up audio controllers in each input
                if (_profile?.Inputs != null)
                {
                    foreach (var input in _profile.Inputs)
                    {
                        if (input?.AudioController != null)
                        {
                            // SessionsToControl agora é propriedade calculada (não armazenada), não precisa limpar
                            input.AudioController.UpdateSessionsToControl = false;
                            input.AudioController.AudioCoreNeedsRestart = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Insert("JJM01", "Erro ao limpar recursos de áudio", ex);
            }
        }

        private async Task ActionMainLoop()
        {
            // Inicialização: aguarda dispositivo estar pronto
            if (_isConnected)
            {
                // Run in background to not block keep-alive loop
                await UpdateCoreAudioController(forceRecreateCore: true);
                _ = Task.Run(async () => await SendInputs());
                await Task.Delay(1000);  // Aguarda processamento dos inputs
            }

            DateTime lastAudioCheckTime = DateTime.Now;

            while (_isConnected)
            {
                try
                {
                    // PRIORIDADE MÁXIMA: Verifica requisição de mudança de perfil NO INÍCIO do loop
                    // Isso garante que nenhuma operação está em execução quando o perfil muda
                    if (_pendingProfileName != null && !_updatingAudioController)
                    {
                        _updatingAudioController = true;
                        try
                        {
                            string profileNameToLoad = _pendingProfileName;
                            _pendingProfileName = null; // Clear request

                            // Create new profile in a thread-safe manner (within device loop)
                            _profile = new ProfileClass(this, profileNameToLoad, true);

                            // Sinaliza que AudioControllers precisam ser atualizados com as sessões estáticas
                            foreach (var input in _profile.Inputs.Where(x => x.AudioController != null))
                            {
                                input.AudioController.UpdateSessionsToControl = true;
                            }

                            await SendInputs();
                            _profile.NeedsUpdate = false;
                            lastAudioCheckTime = DateTime.Now;
                        }
                        finally
                        {
                            _updatingAudioController = false;
                        }

                        // IMPORTANTE: Pula para próxima iteração - não executa mais nada durante troca de perfil
                        await Task.Delay(10);
                        continue;
                    }

                    // Verifica se precisa atualizar perfil (mudança interna, não da UI)
                    if (_profile.NeedsUpdate && !_updatingAudioController)
                    {
                        _updatingAudioController = true;
                        try
                        {
                            //_profile.Restart();
                            // IMPORTANTE: Usar await para garantir que termina antes de processar outro perfil
                            //await UpdateCoreAudioController(forceRecreateCore: true);
                            await SendInputs();
                            _profile.NeedsUpdate = false;
                            lastAudioCheckTime = DateTime.Now;
                        }
                        finally
                        {
                            _updatingAudioController = false;
                        }

                        // IMPORTANTE: Pula para próxima iteração - não executa mais nada durante troca de perfil
                        await Task.Delay(10);
                        continue;
                    }

                    // Verifica se algum AudioController solicitou restart ou update
                    // Só processa se não estiver atualizando perfil
                    //if (!_updatingAudioController)
                    //{
                    //    bool needsRestartCore = _profile.Inputs?.Any(x => x.AudioController?.AudioCoreNeedsRestart ?? false) ?? false;
                    //    bool needsUpdateSessions = _profile.Inputs?.Any(x => x.AudioController?.UpdateSessionsToControl ?? false) ?? false;

                    //    // Run in background to not block RequestData() and keep device communication alive
                    //    if (needsRestartCore || needsUpdateSessions)
                    //    {
                    //        _updatingAudioController = true;
                    //        try
                    //        {
                    //            await UpdateCoreAudioController(forceRecreateCore: needsRestartCore, forceUpdateSessions: needsUpdateSessions);
                    //        }
                    //        finally
                    //        {
                    //            _updatingAudioController = false;
                    //        }
                    //    }
                    //}

                    // Requisita dados do mixer (valores dos potenciômetros)
                    // Continua executando mesmo durante atualização do CoreAudioController
                    await RequestData();
                    
                }
                catch (Exception ex)
                {
                    // Erro crítico no loop principal - logar mas não desconectar imediatamente
                    Log.Insert("JJM01", "Erro no loop principal do JJM-01", ex);
                }

                // Delay de 10ms entre iterações
                await Task.Delay(10);
            }
        }

        public Task SendInputs()
        {
            if (_disposed || _sending || _requesting)
            {
                return Task.CompletedTask;
            }

            bool forceDisconnection = false;

            try
            {
                _sending = true;

                Input[] profileInputs = _profile.Inputs.ToArray();
                List<HIDMessage> messages = new List<HIDMessage>();

                foreach (Input inputToSend in profileInputs)
                {
                    // Encode input name as UTF-8 bytes
                    byte[] nameBytes = Encoding.UTF8.GetBytes(inputToSend.Name ?? "");
                    byte nameLength = (byte)Math.Min(nameBytes.Length, 255);

                    // Create message: CMD=0x0001, Payload=[ORDER][LENGTH][NAME...]
                    List<byte> payload = new List<byte>();
                    payload.Add((byte)inputToSend.Id);  // ORDER (0-4)
                    payload.Add(nameLength);             // LENGTH
                    payload.AddRange(nameBytes.Take(nameLength));  // NAME

                    messages.Add(new HIDMessage(0x0001, payload.ToArray()));
                }

                // Send all input name messages
                if (messages.Count > 0)
                {
                    _ = Task.Run( async () => await SendHIDBytes(messages, false, 750, 10000, 2));
                    _lastKeepAlive = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                Log.Insert("JJM01", "Erro ao enviar nomes dos inputs para o mixer", ex);
                forceDisconnection = true;
            }
            finally
            {
                if (forceDisconnection)
                {
                    Dispose();
                    Disconnect();
                }

                _sending = false;
            }

            return Task.CompletedTask;
        }

        public async Task UpdateCoreAudioController(bool forceRecreateCore = false, bool forceUpdateSessions = false)
        {
            bool needsRestart = _profile.Inputs.Any(x => x?.AudioController?.AudioCoreNeedsRestart ?? false);

            if (forceRecreateCore || needsRestart)
            {
                AudioController.GetNewCoreAudioController();
            }
            
            var audioInputs = _profile.Inputs.Where(x => x.AudioController != null && x.Mode == Input.InputMode.AudioController).ToList();

            // Atualizar flags e executar após todos os resets
            foreach (Input input in audioInputs)
            {
                input.Execute();
            }
        }

        public async Task RequestData()
        {
            // Wait to acquire the semaphore
            if (_disposed || _sending || _requesting)
            {
                return;
            }

            // Rate limiting: Don't request more often than every 50ms (20 requests/second)
            // This prevents HID overload while keeping inputs responsive
            if ((DateTime.Now - _lastRequest) < _requestInterval)
            {
                return;
            }

            bool forceDisconnection = false;

            try
            {
                _requesting = true;

                // Send Request All Inputs command (0x0002)
                // This also serves as keep-alive (firmware timeout is 10 seconds)
                List<HIDMessage> messages = new List<HIDMessage>();
                messages.Add(new HIDMessage(0x0002, new byte[0]));  // No payload

                // Timeout aumentado de 100ms para 2000ms para dar tempo ao firmware
                (bool success, List<byte> responseBytes) = await RequestHIDBytes(messages, false, 0, 2000, 2);

                if (success && responseBytes != null && responseBytes.Count >= 5)  // 5 volume values (0-100)
                {
                    // Response format: [V0][V1][V2][V3][V4] (no CMD, no FLAGS - raw data only)
                    for (int i = 0; i < Math.Min(5, _profile.Inputs.Count) && i < responseBytes.Count; i++)
                    {
                        byte volumeValue = responseBytes[i];

                        // CRITICAL: Only Execute() if volume actually changed
                        // Otherwise creates hundreds of async tasks (fire and forget)
                        if (_profile.Inputs[i].AudioController != null &&
                            _profile.Inputs[i].AudioController.SettedVolume != volumeValue)
                        {
                            _profile.Inputs[i].AudioController.SettedVolume = volumeValue;
                            _profile.Inputs[i].Execute();
                        }
                    }
                    _actualConnectionTimeout = 0;
                    _lastKeepAlive = DateTime.Now;
                    _lastRequest = DateTime.Now;
                }
                else
                {
                    _actualConnectionTimeout++;

                    if (_actualConnectionTimeout >= _connectionTimeoutLimit)
                    {
                        forceDisconnection = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Insert("JJM01", "Erro ao requisitar dados do mixer", ex);
                _actualConnectionTimeout++;

                if (_actualConnectionTimeout >= _connectionTimeoutLimit)
                {
                    forceDisconnection = true;
                }
            }
            finally
            {
                if (forceDisconnection)
                {
                    Dispose();
                    Disconnect();
                }

                _requesting = false;
            }
        }

        #region IDisposable Implementation

        /// <summary>
        /// Dispose pattern implementation
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Dispose managed resources
                CleanupAudioResources();

                // Clear tracking dictionaries
                _lastActivePIDsByInput?.Clear();

                // Reset flags
                _requesting = false;
                _sending = false;
                _updatingAudioController = false;
                _actualConnectionTimeout = 0;
            }

            // Mark as disposed
            _disposed = true;
        }

        /// <summary>
        /// Public Dispose method - Call this when disconnecting device
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer - ensures cleanup even if Dispose is not called
        /// </summary>
        ~JJM01()
        {
            Dispose(false);
        }

        #endregion
    }
}
