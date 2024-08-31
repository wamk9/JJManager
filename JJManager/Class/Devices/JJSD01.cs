using HidSharp.Reports;
using HidSharp;
using JJManager.Class.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HidSharp.Reports.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text.Json.Nodes;

namespace JJManager.Class.Devices
{
    internal class JJSD01
    {
        private Device _Device = null;
        private bool _IsCommunicating = false;

        public JJSD01(Device device)
        {
            _Device = device;
        }

        public bool ReceiveData() 
        {
            HidStream hidStream;
            dynamic receivedMessage = null;
            byte[] receivedeBytes = null;
            bool resetAsyncResult = false;

            try
            {
                ReportDescriptor reportDescriptor = _Device.HidDevice.GetReportDescriptor();

                foreach (DeviceItem deviceItem in reportDescriptor.DeviceItems)
                {
                    if (_Device.HidDevice.TryOpen(out hidStream))
                    {
                        hidStream.ReadTimeout = Timeout.Infinite;

                        using (hidStream)
                        {
                            byte[] inputReportBuffer = new byte[_Device.HidDevice.GetMaxInputReportLength()];
                            HidDeviceInputReceiver inputReceiver = reportDescriptor.CreateHidDeviceInputReceiver();
                            DeviceItemInputParser inputParser = deviceItem.CreateDeviceItemInputParser();

                            IAsyncResult ar = null;

                            while (true)
                            {
                                if (ar == null || resetAsyncResult)
                                {
                                    ar = hidStream.BeginRead(inputReportBuffer, 0, inputReportBuffer.Length, null, null);
                                    resetAsyncResult = false;
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
                                                JsonObject json = JsonObject.Parse(receivedMessage);
                                                
                                                //receivedMessage = JsonConvert.DeserializeObject<dynamic>(receivedMessage);

                                                if (json != null)
                                                {
                                                    if (json.ContainsKey("data") && json["data"] is JsonObject dataObject)
                                                    {
                                                        if (dataObject.ContainsKey("key_pressed"))
                                                        {
                                                            _Device.ActiveProfile.Inputs[dataObject["key_pressed"].GetValue<int>()].Execute();
                                                        }
                                                        else if (dataObject.ContainsKey("key_released"))
                                                        {

                                                        }
                                                    }
                                                }
                                            }

                                            resetAsyncResult = true;
                                        }
                                    }
                                    else
                                    {
                                        ar.AsyncWaitHandle.WaitOne(1000);
                                    }
                                }
                            }
                            //hidStream.Close();
                            //hidStream.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Insert("JJSD01", "Ocorreu um problema ao realizar a comunicação com a JJSD-01 de ID '" + _Device.ConnId + "'", ex);
            }

            return false;
        }

        public bool SendData()
        {
            HidStream hidStream;
            String MessageToSend = "";

            try
            {
                if (_Device == null)
                    MessageBox.Show("Device Desconectado");

                ReportDescriptor reportDescriptor = _Device.HidDevice.GetReportDescriptor();

                if (_Device.HidDevice.TryOpen(out hidStream))
                {
                    hidStream.WriteTimeout = 3000;
                    hidStream.ReadTimeout = 3000;

                    using (hidStream)
                    {
                        if (_Device.ActiveProfileNeedsUpdated)
                        {
                            _Device.ActiveProfile.Restart();
                            _Device.ActiveProfileNeedsUpdated = false;
                        }

                        JObject jsonObject = new JObject
                        {
                            { "data", new JObject
                                {
                                    { "connection", "ok" },
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
                            Thread.Sleep(1000);
                            hidStream.Write(bytesToSend, 0, bytesToSend.Length);
                            return true;
                        }
                    }
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Log.Insert("JJSD01", "Ocorreu um problema ao enviar os dados para a JJSD-01", ex);
            }
            catch (IOException ex)
            {
                Log.Insert("JJSD01", "Ocorreu um problema ao enviar os dados para a JJSD-01", ex);
            }
            catch (Exception ex)
            {
                Log.Insert("JJSD01", "Ocorreu um erro ao receber a informação via HID (SendMessage): " + MessageToSend, ex);
            }

            return false;
        }
    }
}
