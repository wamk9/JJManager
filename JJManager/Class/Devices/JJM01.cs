using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Input;
using JJManager.Class.App;
using JJManager.Class.App.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OpenBLT.Lib;

namespace JJManager.Class.Devices
{
    internal class JJM01
    {
        private JJManager.Class.Device _device = null;
        private bool _ReceiveInProgress = false;
        private bool _SendInProgress = false;

        public HidDevice Hid { get { return _device.HidDevice; } }

        public Device Device {
            set { _device = value; }
            get { return _device; } 
        }

        public JJM01(JJManager.Class.Device device)
        {
            _device = device;
        }

        public dynamic ReceiveMessage()
        {
            if (_ReceiveInProgress)
                return null;

            HidStream hidStream;
            dynamic receivedMessage = null;
            byte[] receivedeBytes = null;

            _ReceiveInProgress = true;

            try
            {
                if (_device == null)
                    MessageBox.Show("Device Desconectado");

                ReportDescriptor reportDescriptor = _device.HidDevice.GetReportDescriptor();

                foreach (DeviceItem deviceItem in reportDescriptor.DeviceItems)
                {
                    if (_device.HidDevice.TryOpen(out hidStream))
                    {
                        hidStream.ReadTimeout = Timeout.Infinite;

                        using (hidStream)
                        {
                            byte[] inputReportBuffer = new byte[_device.HidDevice.GetMaxInputReportLength()];
                            HidDeviceInputReceiver inputReceiver = reportDescriptor.CreateHidDeviceInputReceiver();
                            DeviceItemInputParser inputParser = deviceItem.CreateDeviceItemInputParser();

                            IAsyncResult ar = null;

                            while (true)
                            {
                                if (ar == null)
                                {
                                    ar = hidStream.BeginRead(inputReportBuffer, 0, inputReportBuffer.Length, null, null);
                                }
                                else
                                {
                                    if (ar.IsCompleted)
                                    {
                                        int byteCount = hidStream.EndRead(ar);
                                        ar = null;

                                        if (byteCount > 0)
                                        {
                                            receivedeBytes = inputReportBuffer.Take(byteCount).Where(x => x != 0x00).ToArray();
                                            receivedMessage = Encoding.ASCII.GetString(receivedeBytes).Trim();

                                            if ((receivedMessage.StartsWith("{") && receivedMessage.EndsWith("}")) || //For object
                                                    (receivedMessage.StartsWith("[") && receivedMessage.EndsWith("]")))   //For array
                                            {
                                                receivedMessage = JsonConvert.DeserializeObject<dynamic>(receivedMessage);
                                            }
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        ar.AsyncWaitHandle.WaitOne(1000);
                                    }
                                }
                            }
                            hidStream.Close();
                            //hidStream.Dispose();

                        }
                    }
                }
            }
            catch (TimeoutException ex)
            {
                //do nothing
            }
            catch (Exception ex)
            {
                Log.Insert("Devices", "Ocorreu um erro ao receber a informação via HID (ReceiveMessage): ", ex);
            }

            _ReceiveInProgress = false;

            return receivedMessage;
        }

        public void SendMessage()
        {
            if (_SendInProgress)
                return;

            _SendInProgress = true;

            HidStream hidStream;
            String MessageToSend = "";

            try
            {
                if (_device == null)
                    MessageBox.Show("Device Desconectado");

                ReportDescriptor reportDescriptor = _device.HidDevice.GetReportDescriptor();

                if (_device.HidDevice.TryOpen(out hidStream))
                {
                    hidStream.WriteTimeout = 3000;
                    hidStream.ReadTimeout = 3000;

                    using (hidStream)
                    {
                        if (_device.ActiveProfileNeedsUpdated)
                        {
                            _device.ActiveProfile.Restart();
                            _device.ActiveProfileNeedsUpdated = false;
                        }

                        JArray jsonArray = new JArray();

                        foreach (Input inputToSend in _device.ActiveProfile.Inputs)
                        {
                            var jsonObject = new JObject
                            {
                                { "data", new JObject
                                    {
                                        { "order", inputToSend.Id },
                                        { "name", inputToSend.Name }
                                    }
                                }
                            };

                            MessageToSend = jsonObject.ToString(Formatting.None);

                            if (MessageToSend.Length > 63)
                                throw new ArgumentOutOfRangeException();

                            byte[] messageInBytes = Encoding.ASCII.GetBytes(MessageToSend);
                            byte[] bytesToSend = new byte[(messageInBytes.Length + 1)];

                            for (int i = 0; i < bytesToSend.Length; i++)
                            {
                                bytesToSend[i] = (byte)(i == 0 ? 0x00 : messageInBytes[(i - 1)]);
                            }

                            if (hidStream.CanRead && MessageToSend.Length > 1) 
                            {
                                // Need do it to don't return a System.IO.IOException
                                Thread.Sleep(1500);
                                hidStream.Write(bytesToSend, 0, bytesToSend.Length);
                            }
                        }
                    }
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Log.Insert("HidDevices", "Ocorreu um problema ao enviar os dados para o JJM-01", ex);
            }
            catch (IOException ex)
            {
                Log.Insert("HidDevices", "Ocorreu um problema ao enviar os dados para o JJM-01", ex);
            }
            catch (Exception ex)
            {
                Log.Insert("HidDevices", "Ocorreu um erro ao receber a informação via HID (SendMessage): " + MessageToSend, ex);
            }

            _SendInProgress = false;
            return;
        }

        public void ExecuteInputFunction(int order, int value)
        {
            Input inputInUse = _device.ActiveProfile.Inputs[order];
            
            if (inputInUse.Mode == Input.InputMode.AudioController)
            {
                if (inputInUse.AudioController.AudioCoreNeedsRestart)
                {
                    CoreAudioController coreAudioController = new CoreAudioController();

                    foreach (Input input in _device.ActiveProfile.Inputs)
                    {
                        input.AudioController.ResetCoreAudioController(coreAudioController);
                        input.AudioController.AudioCoreNeedsRestart = false;
                    }
                }

                inputInUse.AudioController.ChangeVolume(value);
            }
        }
    }
}
