using AudioSwitcher.AudioApi.CoreAudio;
using Device.Net;
using Usb.Net.Windows;
using Hid.Net.Windows;
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
using Device.Net.Exceptions;

namespace JJManager.Class.Devices.Connections
{
    public class HID : JJDevice
    {
        private HidDevice _hidSharpDevice = null;
        private IDevice _hidDeviceDotNetDevice = null;
        private HidStream _hidStream = null;
        private string _devicePath = null;
        private static readonly SemaphoreSlim _semaphoreHID = new SemaphoreSlim(1, 1);

        public HID(HidDevice hidSharpDevice) : base()
        {
            _hidSharpDevice = hidSharpDevice;
            _productName = _hidSharpDevice.GetProductName();
            _type = Type.HID;
            _devicePath = _hidSharpDevice.DevicePath;
            _connId = _devicePath.GetHashCode().ToString();
            GetProductID();
            GetHIDConnPort();
            _profile = new ProfileClass(this);
            GetUserProductID();
            GetDeviceDotNetInstance();
        }

        private void GetDeviceDotNetInstance()
        {
            var hidFactory = new FilterDeviceDefinition()
                .CreateWindowsHidDeviceFactory();

            // Register the factory for creating Usb devices.
            var usbFactory = new FilterDeviceDefinition()
                .CreateWindowsUsbDeviceFactory();


            //----------------------

            // Join the factories together so that it picks up either the Hid or USB device
            var factories = hidFactory.Aggregate(usbFactory);

            // Get connected device definitions
            var deviceDefinitions = (factories.GetConnectedDeviceDefinitionsAsync().ConfigureAwait(false).GetAwaiter().GetResult()).ToList();

            if (deviceDefinitions.Count == 0)
            {
                // No devices were found
                Console.WriteLine("No devices found.");
                return;
            }

            // Get the device from its definition by matching product name

            for (int i = 0; i < deviceDefinitions.Count; i++)
            {
                try
                {
                    var trezorDevice = hidFactory.GetDeviceAsync(deviceDefinitions[i]).ConfigureAwait(false).GetAwaiter().GetResult();
                    if (trezorDevice.DeviceId == _devicePath)
                    {
                        _hidDeviceDotNetDevice = trezorDevice;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Log.Insert("HID", "Ocorreu um erro ao construir o objeto do Device.net", ex);
                }
            }
        }

        /// <summary>
        /// Used to send a data to the device
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> SendHIDData(string data, bool forceConnection = false, int delay = 1500, int timeout = 2000)
        {
            bool result = false;

            try
            {
                if (!_isConnected && !forceConnection)
                {
                    throw HidSharp.DeviceException.CreateIOException(_hidSharpDevice, "Device isn't connected on JJManager");
                }

                //if (_sendInProgress)
                //{
                //    throw DeviceException.CreateIOException(_hidSharpDevice, "Device is already sending data, so it is busy.");
                //}

                if (string.IsNullOrEmpty(data))
                {
                    throw new ArgumentNullException("data", "Necessary data to send to device");
                }

                await Task.Delay(delay);

                
                await _hidDeviceDotNetDevice.InitializeAsync().ConfigureAwait(false);
                int writeSize = _hidDeviceDotNetDevice.ConnectedDeviceDefinition.WriteBufferSize.Value;
                _hidDeviceDotNetDevice.Close();

                int count = 0;
                byte[] bytesToSend = new byte[writeSize];
//                byte[] buffer = new byte[writeSize];
                byte[] messageInBytes = Encoding.ASCII.GetBytes(data.Replace('\n', ' ').Trim() + '\n');
                int messageSize = messageInBytes.Length;


                while (messageSize > 0)
                {
                    Array.Clear(bytesToSend, 0, bytesToSend.Length);
                    int chunkSize = Math.Min(messageSize, writeSize - 1);
                    
                    // Set the report ID as the first byte (if required by your HID device)
                    bytesToSend[0] = 0x00;

                    Array.Copy(messageInBytes, count, bytesToSend, 1, chunkSize);
                    
                    // Update the counters
                    count += chunkSize;
                    messageSize -= chunkSize;
                    
                    await Task.Delay(10);
                    try
                    {
                        await _hidDeviceDotNetDevice.InitializeAsync().ConfigureAwait(false);
                        await _hidDeviceDotNetDevice.WriteAsync(bytesToSend, (new CancellationTokenSource(TimeSpan.FromMilliseconds(timeout))).Token).ContinueWith(async task =>
                        {
                        }).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException)
                    {
                        // Do Nothing...
                    }
                    catch (Exception ex)
                    {
                        Log.Insert("HID", "Ocorreu um problema na escrita dos dados (SendHIDData)", ex);
                    }
                    finally
                    {
                        _hidDeviceDotNetDevice.Close();
                    }

                    if (messageSize == 0)
                    {
                        break;
                    }
                }

                result = true;
            }
            catch (TaskCanceledException)
            {
                // Do Nothing...
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
        public async Task<string> ReceiveHIDData(bool forceConnection = false, int timeout = 2000)
        {
            string receivedMessage = null;
            bool result = false;

            try
            {
                if (!_isConnected && !forceConnection)
                {
                    throw HidSharp.DeviceException.CreateIOException(_hidSharpDevice, "Device isn't connected on JJManager");
                }

                //if (_receiveInProgress)
                //{
                //    throw DeviceException.CreateIOException(_hidSharpDevice, "JJManager already is trying receive data.");
                //}

                
                try
                {
                    await _hidDeviceDotNetDevice.InitializeAsync().ConfigureAwait(false);
                    var readBuffer = await _hidDeviceDotNetDevice.ReadAsync((new CancellationTokenSource(TimeSpan.FromMilliseconds(timeout))).Token).ConfigureAwait(false);
                    receivedMessage = Encoding.ASCII.GetString(readBuffer.Data.Where(x => x != 0x00).ToArray());
                }
                catch (TaskCanceledException)
                {
                    // Do Nothing...
                }
                catch (Exception ex)
                {
                    Log.Insert("HID", "Ocorreu um problema na leitura dos dados (RequestHIDData)", ex);
                }
                finally
                {
                    _hidDeviceDotNetDevice.Close();
                }
            }
            catch (TaskCanceledException)
            {
                // Do Nothing...
            }
            catch (Exception ex)
            {
                Log.Insert("HID", "Ocorreu um problema com o dispositivo ao realizar o recebimento de dados", ex);
            }
            finally
            {

            }

            return receivedMessage;
        }


        public override bool Connect()
        {
            try
            {
                return base.Connect();
            }
            catch (Exception ex)
            {
                Log.Insert("HID", $"Não foi possível realizar a conexão com {_productName} de ID {_connId}", ex);
                return false;
            }
        }

        public override bool Disconnect()
        {
            try
            {
                return base.Disconnect();
            }
            catch (Exception ex)
            {
                Log.Insert("HID", $"Ocorreu um problema ao desconectar {_productName} de ID {_connId}", ex);
                return false;
            }
        }

        public async Task<string> RequestHIDData(string data, bool forceConnection = false, int delay = 100, int timeout = 2000)
        {
            string receivedMessage = null;

            try
            {
                if (!_isConnected && !forceConnection)
                {
                    throw HidSharp.DeviceException.CreateIOException(_hidSharpDevice, "Device isn't connected on JJManager");
                }

                if (_receiveInProgress)
                {
                    throw   HidSharp.DeviceException.CreateIOException(_hidSharpDevice, "JJManager already is trying receive data.");
                }
                await _hidDeviceDotNetDevice.InitializeAsync().ConfigureAwait(false);
                byte[] buffer = new byte[_hidDeviceDotNetDevice.ConnectedDeviceDefinition.WriteBufferSize.Value];
                byte[] messageInBytes = Encoding.ASCII.GetBytes(data.Replace('\n', ' ').Trim() + '\n');
                byte[] bytesToSend = new byte[_hidDeviceDotNetDevice.ConnectedDeviceDefinition.WriteBufferSize.Value];
                _hidDeviceDotNetDevice.Close();

                // Prepare the message with the first byte as report ID
                for (int j = 0; j < messageInBytes.Length + 1; j++)
                {
                    bytesToSend[j] = (byte)(j == 0 ? 0x00 : messageInBytes[j - 1]);
                }

                await Task.Delay(delay);

                try
                {
                    await _hidDeviceDotNetDevice.InitializeAsync().ConfigureAwait(false);
                    await _hidDeviceDotNetDevice.WriteAsync(bytesToSend, (new CancellationTokenSource(TimeSpan.FromMilliseconds(timeout))).Token).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    // Do Nothing...
                }
                catch (Exception ex)
                {
                    Log.Insert("HID", "Ocorreu um problema na escrita dos dados (RequestHIDData)", ex);
                }
                finally
                {
                    _hidDeviceDotNetDevice.Close();
                }

                try
                {
                    await _hidDeviceDotNetDevice.InitializeAsync().ConfigureAwait(false);
                    var readBuffer = await _hidDeviceDotNetDevice.ReadAsync((new CancellationTokenSource(TimeSpan.FromMilliseconds(timeout))).Token).ConfigureAwait(false);
                    receivedMessage = Encoding.ASCII.GetString(readBuffer.Data.Where(x => x != 0x00).ToArray());
                }
                catch (TaskCanceledException)
                {
                    // Do Nothing...
                }
                catch (Exception ex)
                {
                    Log.Insert("HID", "Ocorreu um problema na leitura dos dados (RequestHIDData)", ex);
                }
                finally
                {
                    _hidDeviceDotNetDevice.Close();
                }
            }
            catch (TaskCanceledException)
            {
                // Do Nothing...
            }
            catch (ApiException ex)
            {
                Log.Insert("HID", "Ocorreu um problema com o dispositivo ao realizar o envio/recebimento de dados", ex);
                Disconnect();
            }
            catch (Exception ex)
            {
                Log.Insert("HID", "Ocorreu um problema com o dispositivo ao realizar o envio/recebimento de dados", ex);
                
            }
            finally
            {
                // Release the semaphore
            }

            return receivedMessage;
        }

        public async Task GetFirmwareVersion()
        {
            for (int i = 0; i < 5; i++)
            {
                string jsonString = await RequestHIDData(new JsonObject { { "request", new JsonArray { { "firmware_version" } } } }.ToJsonString(), true);

                if (!string.IsNullOrEmpty(jsonString))
                {
                    JsonObject json = JsonObject.Parse(jsonString).AsObject();

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

                    break;
                }
            }
        }

        private void GetHIDConnPort()
        {
            _connPort = new List<string>();

            try
            {
                ReportDescriptor reportDescriptor = _hidSharpDevice.GetReportDescriptor();
                HidStream hidStream = null;

                if (_hidSharpDevice.TryOpen(out hidStream))
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
            string vid = "VID_" + ExtractFromDevicePath(_hidSharpDevice.DevicePath, "vid_");
            string pid = "PID_" + ExtractFromDevicePath(_hidSharpDevice.DevicePath, "pid_");

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
