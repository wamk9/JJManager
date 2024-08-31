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
using System.IO;
using System.Text.Json;

namespace JJManager.Class.Devices
{
    internal class JJBP06
    {
        public static bool SendConfigs(JJManager.Class.Device device)
        {
            HidStream hidStream;
            String MessageToSend = "";

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

                        if (device.ActiveProfileNeedsUpdated)
                        {
                            device.ActiveProfile.Restart();
                            device.ActiveProfileNeedsUpdated = false;
                        }

                        MessageToSend = device.ActiveProfile.Data.ToJsonString();

                        if (MessageToSend.Length > 254)
                            throw new ArgumentOutOfRangeException();

                        byte[] messageInBytes = Encoding.ASCII.GetBytes(MessageToSend);
                        byte[] bytesToSend = new byte[(messageInBytes.Length + 1)];

                        for (int i = 0; i < bytesToSend.Length; i++)
                        {
                            bytesToSend[i] = (byte)(i == 0 ? 0x00 : messageInBytes[(i - 1)]);
                        }

                        if (hidStream.CanWrite && MessageToSend.Length > 0)
                        {
                            hidStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                            Thread.Sleep(1000);
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Insert("JJBP06", "Ocorreu um problema ao enviar os dados: " + MessageToSend, ex);
                return false;
            }

            return true;
        }
    }
}
