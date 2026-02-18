using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using SharpDX.DirectInput;
using JoystickClass = JJManager.Class.Devices.Connections.Joystick;
using System.Text.Json.Nodes;
using JJManager.Class.App.Input.AudioController;
using System.Threading.Tasks;

namespace JJManager.Class.Devices
{
    public class JJB01 : JoystickClass, IDisposable
    {
        private MMDeviceEnumerator deviceEnumerator = null;
        private bool _disposed = false;

        public JJB01(Joystick joystick) : base(joystick)
        {
            RestartClass();
        }

        private void RestartClass()
        {
            // Clean up audio resources on restart
            CleanupAudioResources();

            _actionReceivingData = () => { ActionReceivingData(); };
            _actionResetParams = () => { Task.Run(() => RestartClass()); };
        }

        private void CleanupAudioResources()
        {
            try
            {
                // Dispose MMDeviceEnumerator and clear reference
                if (deviceEnumerator != null)
                {
                    deviceEnumerator.Dispose();
                    deviceEnumerator = null;
                }

                // Clean up audio controllers in each input
                if (_profile?.Inputs != null)
                {
                    foreach (var input in _profile.Inputs)
                    {
                        if (input?.AudioController != null)
                        {
                            // Clear sessions and devices
                            input.AudioController.SessionsToControl?.Clear();
                            input.AudioController.UpdateSessionsToControl = false;
                            input.AudioController.AudioCoreNeedsRestart = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Insert("JJB01", "Erro ao limpar recursos de áudio", ex);
            }
        }

        private void ActionReceivingData()
        {
            while (_isConnected)
            {
                if (!ReceiveData().Result)
                {
                    Dispose();
                    Disconnect();
                }
            }
        }

        public async Task<bool> ReceiveData()
        {
            if (_disposed)
                return false;

            var (success, currentStatus) = ReceiveJoystickData();

            if (success)
            {
                if (_profile.Inputs[0].AudioController.AudioCoreNeedsRestart || _profile.Inputs[1].AudioController.AudioCoreNeedsRestart)
                {
                    // Dispose old MMDeviceEnumerator before creating new one
                    if (deviceEnumerator != null)
                    {
                        deviceEnumerator.Dispose();
                        deviceEnumerator = null;
                    }

                    // Create new MMDeviceEnumerator using NAudio
                    var (newDeviceEnumerator, devicesGetted, sessionsGetted) = AudioController.GetNewCoreAudioController();
                    deviceEnumerator = newDeviceEnumerator;

                    // Removed forced GC.Collect() - let .NET GC handle it automatically
                    // This prevents CPU spikes and application pauses

                    for (int i = 0; i < _profile.Inputs.Count; i++)
                    {
                        if (_profile.Inputs[i].AudioController.AudioCoreNeedsRestart)
                        {
                            _profile.Inputs[i].AudioController.ResetCoreAudioController(deviceEnumerator, devicesGetted, sessionsGetted).Wait();
                            _profile.Inputs[i].AudioController.AudioCoreNeedsRestart = false;
                        }
                    }
                }

                if (currentStatus.X != -1)
                {
                    _profile.Inputs[0].Execute(new JsonObject { { "value", GetAxisPercent(currentStatus.X) } });
                }

                if (currentStatus.Y != -1)
                {
                    _profile.Inputs[1].Execute(new JsonObject { { "value", GetAxisPercent(currentStatus.Y) } });
                }
            }

            return success;
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
        ~JJB01()
        {
            Dispose(false);
        }

        #endregion
    }
}
