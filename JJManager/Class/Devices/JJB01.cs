using AudioSwitcher.AudioApi.CoreAudio;
using HidSharp.Reports.Input;
using HidSharp.Reports;
using HidSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JJManager.Class.Devices
{
    internal class JJB01
    {
        public static JJManager.Class.Device _device = null;
        private bool _ReceiveInProgress = false;

        public JJB01(JJManager.Class.Device device)
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
