using HidSharp.Reports;
using HidSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Text.Json;
using JJManager.Class.App;

namespace JJManager.Class.Devices
{
    internal class JJB999
    {
        private SimHubWebsocket _SimHubSync = null;
        private Device _Device = null;

        public JJB999(Device device)
        {
            _Device = device;
        }

        public void Dispose()
        {
            if (_SimHubSync != null)
            {
                _SimHubSync.StopCommunication();
                _SimHubSync = null;
            }
        }

        public (bool, bool, bool) SendConfigs(JJManager.Class.Device device, bool acceptedOldVersionFirmware, bool acceptedOldVersionPlugin)
        {
            HidStream hidStream;
            String MessageToSend = "";
            const uint attemptsLimit = 5;
            bool hidInfoSended = false;

            try
            {
                if (device == null)
                    MessageBox.Show("Device Desconectado");

                ReportDescriptor reportDescriptor = device.HidDevice.GetReportDescriptor();

                if (device.HidDevice.TryOpen(out hidStream))
                {
                    hidStream.WriteTimeout = 3000;
                    hidStream.ReadTimeout = 3000;

                    using (hidStream)
                    {
                        int led_mode_value = (_Device.ActiveProfile.Data.ContainsKey("led_mode") ? _Device.ActiveProfile.Data["led_mode"].GetValue<int>() : 0);
                        int brightness_limit_value = (_Device.ActiveProfile.Data.ContainsKey("brightness") ? _Device.ActiveProfile.Data["brightness"].GetValue<int>() : 0);

                        // 4 = it's SimHub Sync!
                        if (led_mode_value == 4)
                        {
                            if (_SimHubSync == null)
                            {
                                _SimHubSync = new SimHubWebsocket(2920, "JJB999_" + _Device.ConnId);
                            }

                            if (!_SimHubSync.IsConnected)
                            {
                                _SimHubSync.StartCommunication();
                            }


                            var (success, message) = _SimHubSync.RequestMessage("{ \"request\": [\"SimHubLastData\"]}");

                            if (!success)
                            {
                                return (false, acceptedOldVersionFirmware, acceptedOldVersionPlugin);
                            }
                            else
                            {
                                _SimHubSync.TranslateToButtonBoxHID(message, out MessageToSend, ref acceptedOldVersionPlugin, brightness_limit_value);
                            }
                        }
                        else
                        {
                            if (_SimHubSync != null)
                            {
                                _SimHubSync.StopCommunication();
                                _SimHubSync = null;
                            }

                            var data = new Dictionary<string, object>
                            {
                                { "data", _Device.ActiveProfile.Data }
                            };

                            MessageToSend = JsonSerializer.Serialize(data);
                        }

                        if (MessageToSend.Length > 63)
                            throw new ArgumentOutOfRangeException();

                        byte[] messageInBytes = Encoding.ASCII.GetBytes(MessageToSend);
                        byte[] bytesToSend = new byte[(messageInBytes.Length + 1)];

                        for (int i = 0; i < bytesToSend.Length; i++)
                        {
                            bytesToSend[i] = (byte)(i == 0 ? 0x00 : messageInBytes[(i - 1)]);
                        }

                        if (hidStream.CanWrite && MessageToSend.Length > 0)
                        {
                            Exception senderEx = null;

                            for (uint i = 0; i < attemptsLimit; i++)
                            {
                                try
                                {
                                    Thread.Sleep(100);

                                    hidStream.Write(bytesToSend, 0, bytesToSend.Length);
                                    hidInfoSended = true;
                                }
                                catch (Exception ex)
                                {
                                    Thread.Sleep(300);
                                    senderEx = ex;
                                }

                                if (hidInfoSended)
                                {
                                    break;
                                }
                            }

                            if (!hidInfoSended)
                            {
                                Log.Insert("JJB999", "Ocorreu um problema ao enviar os dados: " + MessageToSend, senderEx);
                                return (false, acceptedOldVersionFirmware, acceptedOldVersionPlugin);
                            }
                        }
                    }
                }
                else
                {
                    return (false, acceptedOldVersionFirmware, acceptedOldVersionPlugin);
                }
            }
            catch (Exception ex)
            {
                Log.Insert("JJB999", "Ocorreu um problema ao enviar os dados: " + MessageToSend, ex);
                return (false, acceptedOldVersionFirmware, acceptedOldVersionPlugin);
            }

            return (true, acceptedOldVersionFirmware, acceptedOldVersionPlugin);
        }
        
    }
}
