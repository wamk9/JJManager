using AudioSwitcher.AudioApi.CoreAudio;
using DeviceProgramming.Memory;
using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Input;
using JJManager.Class.App.Input;
using JJManager.Class.App.Profile;
using Microsoft.SqlServer.Management.Smo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpDX;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using ProfileClass = JJManager.Class.App.Profile.Profile;

namespace JJManager.Class.Devices.Connections
{
    public class HID : JJDevice
    {
        private HidDevice _hidDevice = null;

        public HID(HidDevice hidDevice) : base()
        {
            _hidDevice = hidDevice;
            _productName = hidDevice.GetProductName();
            _type = Type.HID;
            _connId = hidDevice.DevicePath.GetHashCode().ToString();
            GetProductID();
            GetFirmwareVersion();
            GetHIDConnPort();
            _profile = new ProfileClass(this);
            GetUserProductID();
        }

        /// <summary>
        /// Used to send a data to the device
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> SendHIDData(string data, bool forceConnection = false, int timeToSend = 1500)
        {
            bool result = false;

            try
            {
                if (!_isConnected && !forceConnection)
                {
                    throw DeviceException.CreateIOException(_hidDevice, "Device isn't connected on JJManager");
                }

                if (_sendInProgress)
                {
                    throw DeviceException.CreateIOException(_hidDevice, "Device is already sending data, so it is busy.");
                }

                if (string.IsNullOrEmpty(data))
                {
                    throw new ArgumentNullException("data", "Necessary data to send to device");
                }

                // Report Descriptor
                ReportDescriptor reportDescriptor = _hidDevice.GetReportDescriptor();
                HidStream hidStream = null;

                if (_hidDevice.TryOpen(out hidStream))
                {
                    hidStream.WriteTimeout = 10000;
                    hidStream.ReadTimeout = 10000;

                    using (hidStream)
                    {
                        byte[] messageInBytes = Encoding.ASCII.GetBytes(data.Replace('\n', ' ').Trim() + '\n');
                        byte[] bytesToSend = new byte[(messageInBytes.Length + 1)];

                        // Prepare the message with the first byte as report ID
                        for (int i = 0; i < bytesToSend.Length; i++)
                        {
                            bytesToSend[i] = (byte)(i == 0 ? 0x00 : messageInBytes[(i - 1)]);
                        }

                        if (hidStream.CanRead && hidStream.CanWrite)
                        {
                            int reportSize = reportDescriptor.MaxOutputReportLength; // Common HID report size
                            byte[] buffer = new byte[reportSize];

                            int bytesSent = 0;
                            while (bytesSent < messageInBytes.Length)
                            {
                                int chunkSize = Math.Min(reportSize - 1, messageInBytes.Length - bytesSent);
                                Array.Clear(buffer, 0, buffer.Length); // Clear buffer to avoid residual data
                                buffer[0] = 0x00; // Report ID or padding
                                Array.Copy(messageInBytes, bytesSent, buffer, 1, chunkSize);

                                // Wait until the stream is ready for writing
                                while (!hidStream.CanWrite)
                                {
                                    await Task.Delay(10); // Wait until the stream is ready
                                }

                                // Wait before sending data
                                await Task.Delay(timeToSend);

                                bool sended = false;
                                int waitingTime = 10;
                                int retries = 0;
                                int retriesLimit = 4000 / waitingTime;

                                while (!sended)
                                {
                                    try
                                    {
                                        hidStream.Write(buffer, 0, buffer.Length); // Write the data
                                        sended = true; // Mark as sent
                                    }
                                    catch
                                    {
                                        if (retries >= retriesLimit) throw; // Only rethrow after max retries
                                        retries++;
                                        await Task.Delay(waitingTime); // Wait before retrying
                                    }
                                }

                                bytesSent += chunkSize;
                            }

                            result = true;
                        }
                        else
                        {
                            throw DeviceException.CreateIOException(_hidDevice, "Device isn't ready to read or write data");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Insert("HID", "Ocorreu um problema com o dispositivo ao realizar o envio de dados", ex);
                result = false;
            }

            return result;
        }


        /// <summary>
        /// Used to receive a data, this will continue executing until receive anything or connection dropped
        /// </summary>
        /// <returns>A boolean saying if process execute with success and a string with the data</returns>
        public (bool, string) ReceiveHIDData(bool forceConnection = false)
        {
            bool result = false;
            string receivedMessage = null;

            try
            {
                if (!_isConnected && !forceConnection)
                {
                    throw DeviceException.CreateIOException(_hidDevice, "Device isn't connected on JJManager");
                }

                if (_receiveInProgress)
                {
                    throw DeviceException.CreateIOException(_hidDevice, "JJManager already is trying receive data.");
                }

                HidStream hidStream = null;
                ReportDescriptor reportDescriptor = _hidDevice.GetReportDescriptor();

                foreach (DeviceItem deviceItem in reportDescriptor.DeviceItems)
                {
                    if (_hidDevice.TryOpen(out hidStream))
                    {
                        hidStream.ReadTimeout = Timeout.Infinite;

                        using (hidStream)
                        {
                            byte[] inputReportBuffer = new byte[_hidDevice.GetMaxInputReportLength()];
                            HidDeviceInputReceiver inputReceiver = reportDescriptor.CreateHidDeviceInputReceiver();
                            DeviceItemInputParser inputParser = deviceItem.CreateDeviceItemInputParser();

                            IAsyncResult ar = hidStream.BeginRead(inputReportBuffer, 0, inputReportBuffer.Length, null, null);

                            while (ar != null)
                            {

                                if (ar.IsCompleted)
                                {
                                    int byteCount = hidStream.EndRead(ar);

                                    if (byteCount > 0)
                                    {
                                        byte[] receivedeBytes = inputReportBuffer.Take(byteCount).Where(x => x != 0x00).ToArray();
                                        receivedMessage = Encoding.ASCII.GetString(receivedeBytes).Trim();
                                        ar = null;
                                    }
                                }
                                else
                                {
                                    ar.AsyncWaitHandle.WaitOne(300);
                                }
                            }
                        }
                    }

                    result = true;
                }
            }
            catch (Exception ex)
            {
                Log.Insert("HID", "Ocorreu um problema com o dispositivo ao realizar o recebimento de dados", ex);
                result = false;
            }

            return (result, receivedMessage);
        }

        public (bool, string) RequestHIDData(string data, bool forceConnection = false)
        {
            bool result = false;
            string receivedMessage = null;

            try
            {
                if (!_isConnected && !forceConnection)
                {
                    throw DeviceException.CreateIOException(_hidDevice, "Device isn't connected on JJManager");
                }

                if (_receiveInProgress)
                {
                    throw DeviceException.CreateIOException(_hidDevice, "JJManager already is trying receive data.");
                }

                HidStream hidStream = null;
                ReportDescriptor reportDescriptor = _hidDevice.GetReportDescriptor();

                foreach (DeviceItem deviceItem in reportDescriptor.DeviceItems)
                {
                    if (_hidDevice.TryOpen(out hidStream))
                    {
                        hidStream.WriteTimeout = 3000;
                        hidStream.ReadTimeout = 3000;

                        using (hidStream)
                        {
                            byte[] messageInBytes = Encoding.ASCII.GetBytes(data.Replace('\n', ' ').Trim() + '\n');
                            byte[] bytesToSend = new byte[(messageInBytes.Length + 1)];

                            if (hidStream.CanRead && hidStream.CanWrite)
                            {
                                int reportSize = reportDescriptor.MaxOutputReportLength; // Common HID report size
                                byte[] buffer = new byte[reportSize];

                                int bytesSent = 0;
                                while (bytesSent < messageInBytes.Length)
                                {
                                    int chunkSize = Math.Min(reportSize - 1, messageInBytes.Length - bytesSent);
                                    Array.Clear(buffer, 0, buffer.Length); // Clear buffer to avoid residual data
                                    buffer[0] = 0x00; // Report ID or padding
                                    Array.Copy(messageInBytes, bytesSent, buffer, 1, chunkSize);



                                    while (!hidStream.CanWrite)
                                    {
                                        Thread.Sleep(10); // Wait until the stream is ready
                                    }

                                    bool sended = false;
                                    int waitingTime = 10;
                                    int retries = 0;
                                    int retriesLimit = 4000 / waitingTime;

                                    while (!sended)
                                    {
                                        try
                                        {
                                            hidStream.Write(buffer, 0, buffer.Length);
                                            break; // Exit loop if successful
                                        }
                                        catch
                                        {
                                            if (retries >= retriesLimit) throw; // Only rethrow after max retries
                                            retries++;
                                            Thread.Sleep(waitingTime); // Wait before retrying
                                        }
                                    }
                                    //hidStream.Write(buffer, 0, buffer.Length); // Write full report size
                                    bytesSent += chunkSize;
                                }

                                result = true;
                            }
                            else
                            {
                                throw DeviceException.CreateIOException(_hidDevice, "Device isn't ready to read any data");
                            }

                            byte[] inputReportBuffer = new byte[_hidDevice.GetMaxInputReportLength()];
                            HidDeviceInputReceiver inputReceiver = reportDescriptor.CreateHidDeviceInputReceiver();
                            DeviceItemInputParser inputParser = deviceItem.CreateDeviceItemInputParser();

                            IAsyncResult ar = hidStream.BeginRead(inputReportBuffer, 0, inputReportBuffer.Length, null, null);

                            while (ar != null)
                            {

                                if (ar.IsCompleted)
                                {
                                    int byteCount = hidStream.EndRead(ar);

                                    if (byteCount > 0)
                                    {
                                        byte[] receivedeBytes = inputReportBuffer.Take(byteCount).Where(x => x != 0x00).ToArray();
                                        receivedMessage = Encoding.ASCII.GetString(receivedeBytes).Trim();
                                        result = true;
                                    }

                                    ar = null;
                                }
                                else
                                {
                                    ar.AsyncWaitHandle.WaitOne(1000);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Insert("HID", "Ocorreu um problema com o dispositivo ao realizar o recebimento de dados", ex);
                result = false;
            }

            return (result, receivedMessage);
        }

        private void GetFirmwareVersion()
        {
            var (success, jsonString) = RequestHIDData(new JsonObject { { "request", new JsonArray { { "firmware_version" } } } }.ToJsonString(), true);

            if (success)
            {
                JsonObject json = !string.IsNullOrEmpty(jsonString) ? JsonObject.Parse(jsonString).AsObject() : null;

                if (json != null && json.ContainsKey("firmware_version"))
                {
                    string[] versionSplitted = json["firmware_version"].GetValue<string>().Split('.');

                    switch (versionSplitted.Length)
                    {
                        case 1:
                            _version = new Version(int.Parse(versionSplitted[0]), 0);
                            break;
                        case 2:
                            _version = new Version(int.Parse(versionSplitted[0]), int.Parse(versionSplitted[1]));
                            break;
                        case 3:
                            _version = new Version(int.Parse(versionSplitted[0]), int.Parse(versionSplitted[1]), int.Parse(versionSplitted[2]));
                            break;
                        case 4:
                            _version = new Version(int.Parse(versionSplitted[0]), int.Parse(versionSplitted[1]), int.Parse(versionSplitted[2]), int.Parse(versionSplitted[3]));
                            break;
                        default:
                            _version = null;
                            break;
                    }
                }
            }
        }

        private void GetHIDConnPort()
        {
            _connPort = new List<string>();

            try
            {
                ReportDescriptor reportDescriptor = _hidDevice.GetReportDescriptor();
                HidStream hidStream = null;

                if (_hidDevice.TryOpen(out hidStream))
                {
                    hidStream.WriteTimeout = 3000;
                    hidStream.ReadTimeout = 3000;

                    using (hidStream)
                    {
                        _connPort.Add(hidStream.Device.GetSerialPorts()[0].Replace("\\\\.\\", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Insert("HID", "Ocorreu um problema com o dispositivo ao realizar a busca da porta de comunicação", ex);
            }

            if (_connPort.Count == 0)
            {
                GetConnPortByVidPidAndProductName();
            }
        }
        private string ExtractFromDevicePath(string devicePath, string key, string delimiter = "&")
        {
            string result = string.Empty;
            int start = devicePath.IndexOf(key, StringComparison.OrdinalIgnoreCase);

            if (start >= 0)
            {
                start += key.Length;
                int end = devicePath.IndexOf(delimiter, start);
                if (end > start)
                {
                    result = devicePath.Substring(start, end - start);
                }
                else
                {
                    result = devicePath.Substring(start);
                }
            }

            return result.ToUpper(); // Ensure the extracted value is in uppercase
        }

        private void GetConnPortByVidPidAndProductName()
        {
            // Extract VID and PID from DevicePath
            string vid = "VID_" + ExtractFromDevicePath(_hidDevice.DevicePath, "vid_");
            string pid = "PID_" + ExtractFromDevicePath(_hidDevice.DevicePath, "pid_");

            // Query the WIN32_SerialPort WMI class
            using (var serialPortSearcher = new ManagementObjectSearcher("SELECT * FROM WIN32_SerialPort"))
            {
                using (var serialPortResults = serialPortSearcher.Get())
                {
                    foreach (var serialPort in serialPortResults.Cast<ManagementObject>())
                    {
                        string deviceID = serialPort["DeviceID"]?.ToString(); // e.g., "COM3"
                        string serialPNPDeviceID = serialPort["PNPDeviceID"]?.ToString(); // e.g., "USB\\VID_2341&PID_8055\\SERIALNUMBER"

                        if (serialPNPDeviceID == null) continue;

                        if (serialPNPDeviceID.Contains(vid) && serialPNPDeviceID.Contains(pid))
                        {
                            _connPort.Add(deviceID);
                        }
                    }
                }
            }
        }
    } 
}
