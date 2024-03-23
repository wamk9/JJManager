using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Input;
using JJManager.Class.App;
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

namespace JJManager.Class.Devices
{
    internal class JJM01
    {
        public static JJManager.Class.Device _device = null;
        private bool _ReceiveInProgress = false;
        private bool _SendInProgress = false;


        public JJM01(JJManager.Class.Device device)
        {
            _device = device;
        }

        public String[] ReceiveMessage()
        {
            if (_ReceiveInProgress)
                return null;

            _ReceiveInProgress = true;

            HidStream hidStream;
            String[] receivedMessage = null;
            byte[] receivedeBytes = null;

            try
            {
                if (_device == null)
                    MessageBox.Show("Device Desconectado");

                ReportDescriptor reportDescriptor = _device.HidDevice.GetReportDescriptor();

                foreach (DeviceItem deviceItem in reportDescriptor.DeviceItems)
                {
                    if (_device.HidDevice.TryOpen(out hidStream))
                    {
                        hidStream.ReadTimeout = 3000;

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
                                            byteCount -= 9;
                                            receivedeBytes = inputReportBuffer.Skip(9).Take(byteCount).Where(x => x != 0x00).ToArray();
                                            receivedMessage = Encoding.ASCII.GetString(receivedeBytes).Trim().Split('|').ToArray();

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
                            hidStream.Dispose();

                        }
                    }
                }
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
            JJManager.Class.Profile activeProfile = null;

            try
            {
                if (_device == null)
                    MessageBox.Show("Device Desconectado");

                ReportDescriptor reportDescriptor = _device.HidDevice.GetReportDescriptor();

                using (hidStream = _device.HidDevice.Open())
                {
                    hidStream.WriteTimeout = 1000;
                    hidStream.ReadTimeout = 1000;
                        
                    activeProfile = Profile.getActiveProfile(_device);

                    if (activeProfile == null)
                    {
                        return;
                    }

                    foreach (AnalogInput inputToSend in activeProfile.getAllInputs())
                    {
                        hidStream.Flush();
                            
                        MessageToSend = inputToSend.Id + "|" + inputToSend.Name;
                        Log.Insert("Devices", MessageToSend);

                        if (MessageToSend.Length > 63)
                            throw new ArgumentOutOfRangeException();

                        byte[] messageInBytes = Encoding.ASCII.GetBytes(MessageToSend);
                        byte[] bytesToSend = new byte[(messageInBytes.Length + 1)];

                        for (int i = 0; i < bytesToSend.Length; i++)
                        {
                            bytesToSend[i] = (byte)(i == 0 ? 0x00 : messageInBytes[(i - 1)]);
                        }

                        if (hidStream.CanWrite && MessageToSend.Length > 1) 
                        {
                            hidStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                            Thread.Sleep(100);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Insert("Devices", "Ocorreu um erro ao receber a informação via HID (SendMessage): " + MessageToSend, ex);
            }

            _SendInProgress = false;
            return;
        }

        public void ExecuteInputFunction(int id, String value)
        {
            AnalogInput inputInUse = _device.ActiveProfile.GetAnalogInputById(id);

            bool audioCoreRestart = false;
            //if (input.Type == "audio")
            //{
            inputInUse.ManageAudioVolume(int.Parse(value), out audioCoreRestart);

            if (inputInUse.AudioManager.AudioCoreNeedsRestart)
            {
                inputInUse.AudioManager.AudioCoreNeedsRestart = false;
                CoreAudioController coreAudioController = new CoreAudioController();

                foreach (AnalogInput input in _device.ActiveProfile.getAllInputs())
                {
                    input.AudioManager.ResetCoreAudioController(coreAudioController);
                }
            }
            //}
        }
    }
}
