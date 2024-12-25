//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//using HidSharp;
//using HidSharp.Reports;
//using System.IO;
//using SharpDX.DirectInput;
//using SharpDX;
//using System.Text.Json;
//using MaterialSkin.Controls;
//using InTheHand.Net.Sockets;
//using System.Text.Json.Nodes;
//using Newtonsoft.Json;
//using HidSharp.Reports.Input;
//using AudioSwitcher.AudioApi.CoreAudio;
//using System.ComponentModel;
//using System.Collections.ObjectModel;
//using ProfileClass = JJManager.Class.App.Profile.Profile;
//using JJManager.Class.App.Input;
//using System.Management;
//using System.IO.Ports;
//using LibUsbDotNet.Info;
//using LibUsbDotNet.Main;
//using LibUsbDotNet;

//namespace JJManager.Class
//{
//    public class Device : INotifyPropertyChanged
//    {
//        public enum DeviceType
//        {
//            Unsetted,
//            HID,
//            Joystick,
//            Bluetooth
//        }

//        private HidDevice _HidDevice = null;
//        private Joystick _Joystick = null;
//        private BluetoothDeviceInfo _BtDevice = null;

//        private String _Id = "";
//        private String _ProductName = "";
//        private String _SerialNumber = "";
//        private String _ConnId = "";
//        private String _ConnType = "";
//        private String _ConnPort = "";
//        private Thread _ThreadSendingData = null;
//        private Thread _ThreadReceivingData = null;
//        private ProfileClass _Profile = null;
//        private String _JJID = "";
//        private DatabaseConnection _DatabaseConnection = new DatabaseConnection();
//        private readonly object _lock = new object();
//        private Version _Version = null;
//        private bool _IsConnected = false;
//        private bool _ActiveProfileNeedsUpdated = false;
//        private DeviceType _Type = Device.DeviceType.Unsetted;
//        private Main _MainForm = null;

//        public event PropertyChangedEventHandler PropertyChanged;

//        // This method is called by the Set accessor of each property.
//        // The CallerMemberName attribute that is applied to the optional propertyName
//        // parameter causes the property name of the caller to be substituted as an argument.
//        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

//        public DeviceType Type
//        {
//            get => _Type;
//        }
//        public HidDevice HidDevice
//        {
//            get => _HidDevice;
//        }
        
//        public Joystick Joystick
//        {
//            get => _Joystick;
//        }
        
//        public BluetoothDeviceInfo BtDevice
//        {
//            get => _BtDevice;
//        }
        
//        public String Id
//        {
//            get => _Id;
//        }
        
//        public String ProductName
//        {
//            get => _ProductName;
//        }

//        public String ConnId
//        {
//            get => _ConnId;
//        }

//        public String ConnType
//        {
//            get => _ConnType;
//        }

//        public String ConnPort
//        {
//            get => _ConnPort;
//        }

//        public String JJID
//        {
//            get => _JJID;
//        }

//        public Version Version
//        {
//            get => _Version;
//        }

//        public bool IsConnected
//        {
//            get => _IsConnected;
//        }

//        public bool ActiveProfileNeedsUpdated
//        {
//            set => _ActiveProfileNeedsUpdated = value;
//            get => _ActiveProfileNeedsUpdated;
//        }

//        public ProfileClass ActiveProfile
//        {
//            get => _Profile;
//        }


//        /// <summary>
//        /// Construtor para dispositivos HID em geral.
//        /// </summary>
//        /// <param name="HidDevice">Objeto do dispositivo HID usando o padrão da biblioteca HIDSharp.</param>
//        public Device(HidDevice HidDevice)
//        {
//            _HidDevice = HidDevice;
//            _ProductName = _HidDevice.GetProductName();
//            _ConnId = _HidDevice.DevicePath.GetHashCode().ToString();
//            _JJID = Device.GetJJProductId(_ProductName);
//            _Profile = new ProfileClass(this);
//            _Id = GetUserProductId();
//            _ConnType = "USB (HID)";
//            _Type = DeviceType.HID;

//            HidStream hidStream;
//            String receivedMessage = null;
//            byte[] receivedeBytes = null;

//            try
//            {
//                ReportDescriptor reportDescriptor = _HidDevice.GetReportDescriptor();

//                foreach (DeviceItem deviceItem in reportDescriptor.DeviceItems)
//                {
//                    if (_HidDevice.TryOpen(out hidStream))
//                    {
//                        hidStream.ReadTimeout = 3000;

//                        using (hidStream)
//                        {
//                            _ConnPort = hidStream.Device.GetSerialPorts()[0].Replace("\\\\.\\", "");
//                            string MessageToSend = "{\"request\": [\"firmware_version\"]}";

//                            if (MessageToSend.Length > 63)
//                                throw new ArgumentOutOfRangeException();

//                            byte[] messageInBytes = Encoding.ASCII.GetBytes(MessageToSend);
//                            byte[] bytesToSend = new byte[(messageInBytes.Length + 1)];

//                            for (int i = 0; i < bytesToSend.Length; i++)
//                            {
//                                bytesToSend[i] = (byte)(i == 0 ? 0x00 : messageInBytes[(i - 1)]);
//                            }

//                            if (hidStream.CanWrite && MessageToSend.Length > 1)
//                            {
                                
//                                byte[] inputReportBuffer = new byte[HidDevice.GetMaxInputReportLength()];
//                                HidDeviceInputReceiver inputReceiver = reportDescriptor.CreateHidDeviceInputReceiver();
//                                DeviceItemInputParser inputParser = deviceItem.CreateDeviceItemInputParser();

//                                IAsyncResult ar = null;

//                                while (true)
//                                {
//                                    if (ar == null)
//                                    {
//                                        ar = hidStream.BeginRead(inputReportBuffer, 0, inputReportBuffer.Length, null, null);

//                                        Exception senderEx = null;
//                                        const uint attemptsLimit = 3;
//                                        bool hidInfoSended = false;

//                                        for (uint i = 0; i < attemptsLimit; i++)
//                                        {
//                                            try
//                                            {
//                                                Thread.Sleep(100);
//                                                hidStream.Write(bytesToSend, 0, bytesToSend.Length);
//                                                hidInfoSended = true;
//                                            }
//                                            catch (Exception ex)
//                                            {
//                                                Thread.Sleep(300);
//                                                senderEx = ex;
//                                            }

//                                            if (hidInfoSended)
//                                            {
//                                                break;
//                                            }
//                                        }

//                                        if (!hidInfoSended)
//                                        {
//                                            Log.Insert("Devices", "Ocorreu um problema ao enviar os dados: " + MessageToSend, senderEx);
//                                        }
//                                    }
//                                    else
//                                    {
//                                        if (ar.IsCompleted)
//                                        {
//                                            int byteCount = hidStream.EndRead(ar);
//                                            ar = null;

//                                            if (byteCount > 0)
//                                            {
//                                                receivedeBytes = inputReportBuffer.Take(byteCount).Where(x => x != 0x00).ToArray();
//                                                receivedMessage = Encoding.ASCII.GetString(receivedeBytes);

//                                                if ((receivedMessage.StartsWith("{") && receivedMessage.EndsWith("}")) || //For object
//                                                    (receivedMessage.StartsWith("[") && receivedMessage.EndsWith("]")))   //For array
//                                                {
//                                                    var data = JsonConvert.DeserializeObject<dynamic>(receivedMessage);

//                                                    string firmwareVersion = data.firmware_version;

//                                                    if (firmwareVersion != null)
//                                                    {
//                                                        string[] versionSplitted = firmwareVersion.Split('.');

//                                                        switch (versionSplitted.Length)
//                                                        {
//                                                            case 1:
//                                                                _Version = new Version(int.Parse(versionSplitted[0]),0);
//                                                                break;
//                                                            case 2:
//                                                                _Version = new Version(int.Parse(versionSplitted[0]), int.Parse(versionSplitted[1]));
//                                                                break;
//                                                            case 3:
//                                                                _Version = new Version(int.Parse(versionSplitted[0]), int.Parse(versionSplitted[1]), int.Parse(versionSplitted[2]));
//                                                                break;
//                                                            case 4:
//                                                                _Version = new Version(int.Parse(versionSplitted[0]), int.Parse(versionSplitted[1]), int.Parse(versionSplitted[2]), int.Parse(versionSplitted[3]));
//                                                                break;
//                                                            default:
//                                                                _Version = null;
//                                                                break;
//                                                        }
//                                                    }
//                                                }
//                                                break;
//                                            }
//                                        }
//                                    }
//                                }
//                            }   
//                        }
//                     }
//                }
//            }
//            catch (Exception ex)
//            {
//                Log.Insert("Devices", "Ocorreu um erro ao receber a informação via HID:", ex);
//            }
//        }

//        public IReadOnlyList<string> GetComPortByVidPidAndProductName()
//        {
//            List<string> ports = new List<string>();

//            // Extract VID, PID, and GUID from DevicePath
//            string vid = "VID_" + ExtractFromDevicePath(HidDevice.DevicePath, "vid_");
//            string pid = "PID_" + ExtractFromDevicePath(HidDevice.DevicePath, "pid_");

//            // Query the WIN32_SerialPort WMI class
//            using (var serialPortSearcher = new ManagementObjectSearcher("SELECT * FROM WIN32_SerialPort"))
//            {
//                using (var serialPortResults = serialPortSearcher.Get())
//                {
//                    foreach (var serialPort in serialPortResults.Cast<ManagementObject>())
//                    {
//                        string deviceID = serialPort["DeviceID"]?.ToString(); // e.g., "COM3"
//                        string serialPNPDeviceID = serialPort["PNPDeviceID"]?.ToString(); // e.g., "USB\\VID_2341&PID_8055\\SERIALNUMBER"

//                        if (serialPNPDeviceID == null) continue;

//                        if (serialPNPDeviceID.Contains(vid) && serialPNPDeviceID.Contains(pid))
//                        {
//                        ports.Add(deviceID);
//                        }
//                    }
//                }
//            }

//            return ports;
//        }

//        private string GetProductNameForDevice(string pnpDeviceID)
//        {
//            string productName = null;

//            try
//            {
//                // Escape single quotes in pnpDeviceID
//                string escapedDeviceID = pnpDeviceID.Replace("'", "''");

//                // Query the Win32_PnPSignedDriver class to get product name
//                string query = $"SELECT * FROM Win32_PnPEntity WHERE DeviceID = '{escapedDeviceID}'";
//                using (var driverSearcher = new ManagementObjectSearcher(query))
//                {
//                    using (var driverResults = driverSearcher.Get())
//                    {
//                        foreach (var driver in driverResults.Cast<ManagementObject>())
//                        {
//                            productName = driver["Caption"]?.ToString(); // Product name is often in the Caption property
//                            break; // Assuming one result is sufficient
//                        }
//                    }
//                }
//            }
//            catch (ManagementException ex)
//            {
//                // Log or display the exception details
//                Console.WriteLine($"ManagementException: {ex.Message}");
//                Console.WriteLine($"ErrorCode: {ex.ErrorCode}");
//            }
//            catch (Exception ex)
//            {
//                // Log or display other exceptions
//                Console.WriteLine($"Exception: {ex.Message}");
//            }


//            return productName;
//        }


//        static string ExtractFromDevicePath(string devicePath, string key, string delimiter = "&")
//        {
//            string result = string.Empty;
//            int start = devicePath.IndexOf(key, StringComparison.OrdinalIgnoreCase);

//            if (start >= 0)
//            {
//                start += key.Length;
//                int end = devicePath.IndexOf(delimiter, start);
//                if (end > start)
//                {
//                    result = devicePath.Substring(start, end - start);
//                }
//                else
//                {
//                    result = devicePath.Substring(start);
//                }
//            }

//            return result.ToUpper(); // Ensure the extracted value is in uppercase
//        }

//        /// <summary>
//        /// Constutor para dispositivos como Joysticks e Gamepads.
//        /// </summary>
//        /// <param name="joystick">Objeto do Joystick/Gamepad usando o padrão da biblioteca SharpDX.</param>
//        public Device(Joystick joystick)
//        {
//            _Joystick = joystick;
//            _ProductName = _Joystick.Properties.ProductName;
//            _ConnId = _Joystick.Properties.ClassGuid.GetHashCode().ToString();
//            _JJID = Device.GetJJProductId(_ProductName);
//            _Profile = new ProfileClass(this);
//            _Id = GetUserProductId();
//            _ConnType = "USB (Joystick)";
//            _Type = DeviceType.Joystick;
//        }

//        public Device(BluetoothDeviceInfo BtDevice)
//        {
//            _BtDevice = BtDevice;
//            _ProductName = _BtDevice.DeviceName;
//            _ConnId = _BtDevice.DeviceAddress.GetHashCode().ToString();
//            _JJID = Device.GetJJProductId(_ProductName);
//            _Profile = new ProfileClass(this);
//            _Id = GetUserProductId();
//            _ConnType = "Bluetooth";
//            _Type = DeviceType.Bluetooth;
//        }

//        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
//        {
//            add
//            {
//                throw new NotImplementedException();
//            }

//            remove
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public void UpdateActiveProfile(string profileName)
//        {
//            _Profile = new ProfileClass(this, profileName, true);
//            ActiveProfileNeedsUpdated = true;
//        }

//        public static string getJJProductName(string connId)
//        {
//            // Initialize DirectInput
//            DirectInput directInput = new DirectInput();

//            List<DeviceInstance> directInputDevices = new List<DeviceInstance>();
//            List<JJManager.Class.Device> jjDevicesList = new List<JJManager.Class.Device>();

//            // JOYSTICK DEVICES

//            String[] jjJoystickNames =
//            {
//                "ButtonBox JJB-01",
//                "ButtonBox JJB-02"
//            };

//            directInputDevices.AddRange(directInput.GetDevices(SharpDX.DirectInput.DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices));
//            directInputDevices.AddRange(directInput.GetDevices(SharpDX.DirectInput.DeviceType.Joystick, DeviceEnumerationFlags.AllDevices));

//            // Check if device found is a JohnJohn's device by name and connection ID.
//            /*foreach (DeviceInstance joystickRaw in directInputDevices)
//            {
//                if (jjJoystickNames.Any(joystickName => joystickRaw.ProductName == joystickName && joystickRaw.InstanceGuid.GetHashCode().ToString() == connId))
//                {
//                    return joystickRaw.ProductName;
//                }
//            }*/
//            foreach (DeviceInstance joystick in directInputDevices)
//            {
//                if (new Joystick(directInput, joystick.InstanceGuid).Properties.ClassGuid.GetHashCode().ToString() == connId)
//                {
//                    return joystick.ProductName;
//                }
//            }

//            // HID DEVICES
//            var list = DeviceList.Local;
//            list.Changed += (sender, e) => Console.WriteLine("Device list changed.");

//            var hidDeviceList = list.GetHidDevices().ToArray();

//            foreach (HidDevice dev in hidDeviceList)
//            {
//                if (dev.DevicePath.GetHashCode().ToString() == connId)
//                    return dev.GetProductName();
//            }

//            // BLUETOOTH DEVICES

//            BluetoothClient client = new BluetoothClient();
//            IReadOnlyCollection<BluetoothDeviceInfo> btDevices = client.DiscoverDevices();
//            client.Close();

//            foreach (BluetoothDeviceInfo btDevice in btDevices)
//            {
//                if (btDevice.DeviceAddress.GetHashCode().ToString() == connId)
//                {
//                    return btDevice.DeviceName;
//                }
//            }

//            return "";
//        }

//        public static string GetJJProductId(string productName)
//        {
//            DatabaseConnection database = new DatabaseConnection();
//            string sql = "";

//            if (productName != "")
//            {
//                sql = "SELECT id FROM dbo.jj_products WHERE product_name = '" + productName + "'";

//                using (JsonDocument Json = database.RunSQLWithResults(sql))
//                {
//                    if (Json != null)
//                        return Json.RootElement[0].GetProperty("id").ToString();
//                }
//            }

//            return "";
//        }

//        private static List<JJManager.Class.Device> CheckConnectedDevices(List<JJManager.Class.Device> newDevices = null, List < JJManager.Class.Device> actualDevices = null)
//        {
//            actualDevices.Where(actualDevice => actualDevice.IsConnected);
//            newDevices.RemoveAll(newDevice => (actualDevices.Exists (actualDevice => newDevice.ConnId == actualDevice.ConnId)));
//            newDevices.AddRange(actualDevices);

//            return newDevices;
//        }


//        public static List<JJManager.Class.Device> GetBluetoothDevices(List<JJManager.Class.Device> actualDevices = null)
//        {
//            List<JJManager.Class.Device> jjDevicesList = new List<JJManager.Class.Device>();

//            BluetoothClient client = new BluetoothClient();
//            IReadOnlyCollection<BluetoothDeviceInfo> devices = client.DiscoverDevices();
            
//            string[] jjBtNames =
//            {
//                "Painel de Leds JJQ-01"
//            };

//            foreach (BluetoothDeviceInfo device in devices)
//            {
//                if (!jjBtNames.Any(name => name == device.DeviceName))
//                {
//                    continue; 
//                }

//                jjDevicesList.Add(new JJManager.Class.Device(device));
//            }

//            client.Close();

//            return jjDevicesList;
//        }

//        public static List<JJManager.Class.Device> getBtDevicesList(List<JJManager.Class.Device> actualDevicesList = null)
//        {
//            List<JJManager.Class.Device> newDevicesList = new List<JJManager.Class.Device>();

//            newDevicesList.AddRange(GetBluetoothDevices());

//            newDevicesList = CheckConnectedDevices(newDevicesList, actualDevicesList);

//            return newDevicesList;
//        }

//        public bool Connect()
//        {
//            if (!CheckDeviceVersionCompatibility())
//            {
//                return false;
//            }

//            if (_IsConnected)
//            {
//                return true;
//            }

//            lock (_lock)
//            {
//                switch (_ProductName)
//                {
//                    //case "Mixer de Áudio JJM-01":
//                    //    _ThreadReceivingData = new Thread(() =>
//                    //    {
//                    //        Devices.JJM01 jjm01 = new Devices.JJM01(this);
//                    //        dynamic receivedMessage = null;

//                    //        CoreAudioController coreAudioController = new CoreAudioController();

//                    //        foreach (Input input in ActiveProfile.Inputs)
//                    //        {
//                    //            if (input.AudioController != null)
//                    //            {
//                    //                input.AudioController.ResetCoreAudioController(coreAudioController);
//                    //                input.AudioController.AudioCoreNeedsRestart = false;
//                    //            }
//                    //        }

//                    //        while (_IsConnected)
//                    //        {
//                    //            receivedMessage = jjm01.ReceiveMessage();

//                    //            if (receivedMessage != null && receivedMessage.order != null && receivedMessage.value != null)
//                    //            {
//                    //                jjm01.ExecuteInputFunction((int)receivedMessage.order, (int)receivedMessage.value);
//                    //            }
//                    //        }
//                    //    });
//                    //    _ThreadReceivingData.Name = "Thread_Receiving_Data_JJM01_" + _ConnId;

//                    //    _ThreadSendingData = new Thread(() =>
//                    //    {
//                    //        Devices.JJM01 jjm01 = new Devices.JJM01(this);

//                    //        while (_IsConnected)
//                    //        {
//                    //            jjm01.SendMessage();
//                    //        }
//                    //    });
//                    //    _ThreadSendingData.Name = "Thread_Sending_Data_JJM01_" + _ConnId;
//                    //    break;
//                    //case "ButtonBox JJB-01":
//                    //    _ThreadReceivingData = new Thread(() =>
//                    //    {
//                    //        Devices.JJB01 jjb01 = new Devices.JJB01(this);

//                    //        while (_IsConnected)
//                    //        {
//                    //            int valueX = GetJoystickAxisPercentage(_Joystick, "X");
//                    //            int valueY = GetJoystickAxisPercentage(_Joystick, "Y");

//                    //            if (valueX != -1)
//                    //            {
//                    //                jjb01.ExecuteInputFunction(1, valueX.ToString());
//                    //            }

//                    //            if (valueY != -1)
//                    //            {
//                    //                jjb01.ExecuteInputFunction(2, valueY.ToString());
//                    //            }

//                    //            if (valueX == -1 && valueY == -1)
//                    //            {
//                    //                break;
//                    //            }
//                    //        }

//                    //        _IsConnected = false;
//                    //    });
//                    //    _ThreadReceivingData.Name = "Thread_Receiving_Data_JJB01_" + _ConnId;

//                    //    break;
//                    //case "Painel de Leds JJQ-01":
//                    //    _ThreadSendingData = new Thread(() =>
//                    //    {
//                    //        JJManager.Class.Devices.JJQ01 jjq01 = new JJManager.Class.Devices.JJQ01(this);


//                    //        //JJManager.Pages.Devices.JJQ01 jjq01 = new JJManager.Pages.Devices.JJQ01(this);
//                    //        while (_IsConnected)
//                    //        {
//                    //            int cont = 0;
//                    //            List<string> strings = jjq01.LoadFrames();
//                    //            foreach (string frame in strings)
//                    //            {
//                    //                jjq01.SendMessage(frame);
//                    //            }


//                    //            JsonObject showFrames = new JsonObject
//                    //            {
//                    //            { "mode", "SHOW" }
//                    //            };

//                    //            jjq01.SendMessage(System.Text.Json.JsonSerializer.Serialize(showFrames));

//                    //            Thread.Sleep(10000);

//                    //        }
//                    //    });
//                    //    _ThreadSendingData.Name = "Thread_Sending_Data_JJQ01_" + _ConnId; 

//                    //    break;
//                    //case "ButtonBox JJBP-06":
//                    //    _ThreadSendingData = new Thread(SendDataJJBP06);
//                    //    _ThreadSendingData.Name = "Thread_Sending_Data_JJBP06_" + _ConnId;
//                    //    break;
//                    //case "ButtonBox JJB-999":
//                    //    _ThreadSendingData = new Thread(SendDataJJB999);
//                    //    _ThreadSendingData.Name = "Thread_Sending_Data_JJB999_" + _ConnId;
//                    //    break;
//                    //case "ButtonBox JJB-01 V2":
//                    //    _ThreadSendingData = new Thread(SendDataJJB01V2);
//                    //    _ThreadSendingData.Name = "Thread_Sending_Data_JJB01V2_" + _ConnId;
//                    //    break;
//                    //case "Streamdeck JJSD-01":
//                    //    _ThreadReceivingData = new Thread(ReceiveDataJJSD01);
//                    //    _ThreadReceivingData.Name = "Thread_Receiving_Data_JJSD01_" + _ConnId;
//                    //    _ThreadSendingData = new Thread(SendDataJJSD01);
//                    //    _ThreadSendingData.Name = "Thread_Sending_Data_JJSD01_" + _ConnId;
//                    //    break;
//                }
            
//                _IsConnected = true;
            
//                if (_ThreadReceivingData != null)
//                {
//                    _ThreadReceivingData.Start();
//                }

//                if (_ThreadSendingData != null)
//                {
//                    _ThreadSendingData.Start();
//                }
//            }

//            OnPropertyChanged("IsConnected");

//            return true;
//        }

//        //private void SendDataJJBP06()
//        //{
//        //    while (_IsConnected)
//        //    {
//        //        if (!JJManager.Class.Devices.JJBP06.SendConfigs(this))
//        //        {
//        //            Disconnect();
//        //        }
//        //    }
//        //}

//        //private void SendDataJJB999()
//        //{
//        //    JJManager.Class.Devices.JJB999 jjb999 = new JJManager.Class.Devices.JJB999(this);

//        //    bool notifiedFirmwareOldVersion = false;
//        //    bool notifiedPluginOldVersion = false;
            
//        //    while (_IsConnected)
//        //    {
//        //        var (success, firmwareOldVersion, pluginOldVersion) = jjb999.SendConfigs(this, notifiedFirmwareOldVersion, notifiedPluginOldVersion);

//        //        if (!success)
//        //        {
//        //            Disconnect();
//        //        }
//        //    }

//        //    jjb999.Dispose();
//        //}
//        //private void SendDataJJB01V2()
//        //{
//        //    JJManager.Class.Devices.JJB01_V2 jjb01_v2 = new JJManager.Class.Devices.JJB01_V2(this);

//        //    bool notifiedFirmwareOldVersion = false;
//        //    bool notifiedPluginOldVersion = false;
//        //    while (_IsConnected)
//        //    {
//        //        var (success, firmwareOldVersion, pluginOldVersion) = jjb01_v2.SendConfigs(this, notifiedFirmwareOldVersion, notifiedPluginOldVersion);

//        //        if (!success)
//        //        {
//        //            Disconnect();
//        //        }
//        //    }

//        //    jjb01_v2.Dispose();
//        //}

//        //private void SendDataJJSD01()
//        //{
//        //    Task.Run(() =>
//        //    {
//        //        JJManager.Class.Devices.JJSD01 jjsd01 = new JJManager.Class.Devices.JJSD01(this);

//        //        while (_IsConnected)
//        //        {
                
//        //            if (!jjsd01.SendData())
//        //            {
//        //                Disconnect();
//        //            }
                
//        //        }
//        //    });
//        //}

//        //private void ReceiveDataJJSD01()
//        //{
//        //    Task.Run(() =>
//        //    {
//        //        JJManager.Class.Devices.JJSD01 jjsd01 = new JJManager.Class.Devices.JJSD01(this);

//        //        while (_IsConnected)
//        //        {
                
//        //            if (!jjsd01.ReceiveData())
//        //            {
//        //                Disconnect();
//        //            }
//        //        }
//        //    });
//        //}

//        public bool Disconnect()
//        {
//            _IsConnected = false;

//            if (_ThreadReceivingData != null)
//            {
//                //_ThreadReceivingData.Abort();
//                //_ThreadConnection.Interrupt();
//                _ThreadReceivingData = null;
//            }

//            if (_ThreadSendingData != null)
//            {
//                //_ThreadSendingData.Abort();
//                //_ThreadConnection.Interrupt();
//                _ThreadSendingData = null;
//            }

//            OnPropertyChanged("IsConnected");

//            return true;
//        }

//        private String GetUserProductId()
//        {
//            DatabaseConnection database = new DatabaseConnection();
//            String sql = "SELECT id FROM dbo.user_products WHERE conn_id = '" + _ConnId + "';";
//            String id = "";

//            using (JsonDocument Json = database.RunSQLWithResults(sql))
//            {
//                if (Json == null)
//                    id = SetUserProductId();
//                else
//                    id = Json.RootElement[0].GetProperty("id").ToString();
//            }

//            return id;
//        }

//        private String SetUserProductId()
//        {
//            DatabaseConnection database = new DatabaseConnection();
//            string id = "";
//            String sql = "INSERT INTO dbo.user_products (" +
//                    " id_product," +
//                    " conn_id," +
//                    " id_profile" +
//                ") VALUES (" +
//                    " '" + _JJID + "',"+
//                    " '" + _ConnId + "'," +
//                    " (SELECT TOP 1 id from dbo.profiles WHERE id_product = '" + _JJID + "' ORDER BY id DESC)" +
//                ");";

//            if (database.RunSQL(sql))
//            {
//                sql = "SELECT id FROM dbo.user_products WHERE conn_id = '" + _ConnId + "';";

//                using (JsonDocument Json = database.RunSQLWithResults(sql))
//                {
//                    id = Json.RootElement[0].GetProperty("id").ToString();
//                    //SaveProfile("Perfil Padrão", id);
//                }
//            }
//            return id;
//        }

//        public void OpenEditWindow(MaterialForm parent)
//        {
//            parent.Visible = false;

//            switch (_ProductName)
//            {
//                case "Mixer de Áudio JJM-01":
//                    Pages.Mixers.JJM_01 MixerScreen = new Pages.Mixers.JJM_01(parent, this);
//                    MixerScreen.ShowDialog();
//                    break;
//                case "ButtonBox JJB-01":
//                    Pages.ButtonBox.JJB_01 BBScreen = new Pages.ButtonBox.JJB_01(parent, this);
//                    BBScreen.ShowDialog();
//                    break;
//                case "Painel de Leds JJQ-01":
//                    Pages.OtherDevices.JJQ01 panelScreen = new Pages.OtherDevices.JJQ01(parent, this);
//                    panelScreen.ShowDialog();
//                    break;
//                case "ButtonBox JJBP-06":
//                    Pages.ButtonBox.JJBP06 jjbp06 = new Pages.ButtonBox.JJBP06(parent, this);
//                    jjbp06.ShowDialog();
//                    break;
//                case "ButtonBox JJB-999":
//                    Pages.ButtonBox.JJB999 jjb999 = new Pages.ButtonBox.JJB999(parent, this);
//                    jjb999.ShowDialog();
//                    break;
//                case "ButtonBox JJB-01 V2":
//                    Pages.ButtonBox.JJB01_V2 jjb01v2 = new Pages.ButtonBox.JJB01_V2(parent, this);
//                    jjb01v2.ShowDialog();
//                    break;
//                case "Streamdeck JJSD-01":
//                    Pages.OtherDevices.JJSD01 jjsd01 = new Pages.OtherDevices.JJSD01(parent, this);
//                    jjsd01.ShowDialog();
//                    break;
//                default:
//                    MessageBox.Show("Este dispositivo ainda não é compatível com o JJManager, aguarde pelas próximas versões do software para tentar novamente.");
//                    break;
//            }

//            parent.Visible = true;
//        }
//        public int GetJoystickAxisPercentage(Joystick joystick, String joystickAxis)
//        {
//            try
//            {
//                joystick.Acquire();

//                joystick.Poll();
//                var currentState = joystick.GetCurrentState(); //only show the last state

//                if (joystickAxis == "X")
//                    return (currentState.X * 100 / 65530);
//                else if (joystickAxis == "Y")
//                    return (currentState.Y * 100 / 65530);

//                joystick.Unacquire();
//            }
//            catch (SharpDXException ex)
//            {
//                if ((ex.ResultCode == ResultCode.NotAcquired) || (ex.ResultCode == ResultCode.InputLost))
//                {
//                    Log.Insert("Device", "Ocorreu um problema relacionado ao SharpDX", ex);
//                    return -1;
//                }
//            }
//            catch (Exception ex)
//            {
//                Log.Insert("Device", "Ocorreu um problema relacionado a busca da porcentagem de inputs de joystick", ex);
//            }

//            return -1;
//        }

//        public static void CheckRestartAllProfileFile(ref ObservableCollection<Device> devices)
//        {
//            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JohnJohn3D", "JJManager", "RestartProfileFile");
//            if (!File.Exists(filePath))
//            {
//                return;
//            }

//            foreach (var device in devices)
//            {
//                device.ActiveProfileNeedsUpdated = true;
//            }

//            File.Delete(filePath);
//        }

//        public static async Task<List<string>> GetUnavailableListEntries(ObservableCollection<Device> devices)
//        {
//            List<HidDevice> hidDevices = await GetHIDList();
//            List<Joystick> joysticks = await GetJoystickList();
//            List<string> listToReturn = new List<string>();

//            foreach (Device device in devices.Where(device => device.Type == DeviceType.HID))
//            {
//                if (!hidDevices.Any(hidDevice => hidDevice.DevicePath.GetHashCode().ToString() == device.ConnId))
//                {
//                    listToReturn.Add(device.ConnId);
//                }
//            }

//            foreach (Device device in devices.Where(device => device.Type == DeviceType.Joystick))
//            {
//                if (!joysticks.Any(joystick => joystick.Properties.ClassGuid.GetHashCode().ToString() == device.ConnId))
//                {
//                    listToReturn.Add(device.ConnId);
//                }
//            }

//            return listToReturn;
//        }

//        public static async Task<List<Device>> GetAvailableListEntries(ObservableCollection<Device> devices)
//        {
//            List<Device> listToReturn = new List<Device>();

//            try
//            {
//                List<HidDevice> hidDevices = await GetHIDList();
//                List<Joystick> joysticks = await GetJoystickList();

//                hidDevices.RemoveAll(hidDevice =>
//                {
//                    return (devices.Any(device =>
//                    {
//                        return (device.ConnId == hidDevice.DevicePath.GetHashCode().ToString());
//                    }));
//                });

//                joysticks.RemoveAll(joystick =>
//                {
//                    return (devices.Any(device =>
//                    {
//                        return (device.ConnId == joystick.Properties.ClassGuid.GetHashCode().ToString());
//                    }));
//                });

//                hidDevices.ForEach(hidDevice =>
//                {
//                    listToReturn.Add(new Device(hidDevice));
//                });

//                joysticks.ForEach(joystick =>
//                {
//                    listToReturn.Add(new Device(joystick));
//                });

//            }
//            catch (Exception ex)
//            {
//                Log.Insert("Device", "Ocorreu um problema ao realizar a busca dos dispositivos", ex);
//            }

//            return listToReturn;
//        }

//        private static async Task<List<HidDevice>> GetHIDList()
//        {
//            List<HidDevice> hidDevicesList = DeviceList.Local.GetHidDevices(0x2341).ToList();
//            var devicePaths = new HashSet<string>();
//            var duplicateDevicePaths = new List<string>();
//            bool problemWithDevice = false;
//            string messageProblemWithDevice = "Um ou mais dispositivos estão com problema de conexão, verifique suas configurações de energia USB";
//            String[] jjHidNames =
//            {
//                "Streamdeck JJSD-01",
//                "Mixer de Áudio JJM-01",
//                "ButtonBox JJB-01 V2", // Gerenciamento de Leds
//                "ButtonBox JJBP-06", // Gerenciamento de Leds
//                "ButtonBox JJB-999" // Gerenciamento de Leds
//            };

//            await Task.Run(() =>
//            {
//                hidDevicesList.RemoveAll(deviceFound =>
//                {
//                    try
//                    {
//                        string productName = deviceFound.GetProductName();
//                        return !jjHidNames.Any(hidName => productName == hidName);
//                    }
//                    catch (IOException)
//                    {
//                        // Log the error, remove the device
//                        problemWithDevice = true;
//                        return true;
//                    }
//                });

//                // Remove Devices how has 2 HIDs, one is Joystick (Ex: ButtonBoxes)
//                hidDevicesList.RemoveAll(deviceFound =>
//                    deviceFound.GetReportDescriptor().Reports[0].ReportID == (byte)0xFF
//                );

//                // Remove Devices how has two HIDs, one is Keyboard HIDs (Ex: StreamDeck JJSD-01 Updated)
//                hidDevicesList.RemoveAll(
//                    device => device.DevicePath.Contains("&mi_03#") &&
//                    device.GetReportDescriptor().Reports[0].ReportID == (byte)0x02
//                );

//                Main main = (Main)Main.ActiveForm;

//                if (main != null && main.InvokeRequired)
//                {
//                    main.BeginInvoke((MethodInvoker)delegate
//                    {
//                        StatusStrip connectStatusStrip = ((StatusStrip)main.Controls["tabMain"].Controls["tabConnect"].Controls["statusStrip"]);
//                        StatusStrip updateStatusStrip = ((StatusStrip)main.Controls["tabMain"].Controls["tabUpdate"].Controls["statusStrip1"]);

//                        if (problemWithDevice && connectStatusStrip.Items["txtStatus"].Text == string.Empty)
//                        {
//                            connectStatusStrip.Items["txtStatus"].Text = messageProblemWithDevice;
//                        }
//                        else if (connectStatusStrip.Items["txtStatus"].Text == messageProblemWithDevice)
//                        {
//                            connectStatusStrip.Items["txtStatus"].Text = "";
//                        }

//                        if (problemWithDevice && updateStatusStrip.Items["txtStatusUpdate"].Text == string.Empty)
//                        {
//                            updateStatusStrip.Items["txtStatusUpdate"].Text = messageProblemWithDevice;
//                        }
//                        else if (updateStatusStrip.Items["txtStatusUpdate"].Text == messageProblemWithDevice)
//                        {
//                            updateStatusStrip.Items["txtStatusUpdate"].Text = "";
//                        }
//                    });
//                }
//                else if (main != null)
//                {
//                    StatusStrip connectStatusStrip = ((StatusStrip)main.Controls["tabMain"].Controls["tabConnect"].Controls["statusStrip"]);
//                    StatusStrip updateStatusStrip = ((StatusStrip)main.Controls["tabMain"].Controls["tabUpdate"].Controls["statusStrip1"]);

//                    if (problemWithDevice && connectStatusStrip.Items["txtStatus"].Text == string.Empty)
//                    {
//                        connectStatusStrip.Items["txtStatus"].Text = messageProblemWithDevice;
//                    }
//                    else if (connectStatusStrip.Items["txtStatus"].Text == messageProblemWithDevice)
//                    {
//                        connectStatusStrip.Items["txtStatus"].Text = "";
//                    }

//                    if (problemWithDevice && updateStatusStrip.Items["txtStatusUpdate"].Text == string.Empty)
//                    {
//                        updateStatusStrip.Items["txtStatusUpdate"].Text = messageProblemWithDevice;
//                    }
//                    else if (updateStatusStrip.Items["txtStatusUpdate"].Text == messageProblemWithDevice)
//                    {
//                        updateStatusStrip.Items["txtStatusUpdate"].Text = "";
//                    }
//                }
//            });

//            return hidDevicesList;
//        }

//        private static async Task<List<Joystick>> GetJoystickList()
//        {
//            DirectInput directInput = new DirectInput();

//            List<DeviceInstance> deviceInstances = new List<DeviceInstance>();
//            List<Joystick> joystickDevicesList = new List<Joystick>();

//            String[] jjJoystickNames =
//            {
//                "ButtonBox JJB-01",
//                "ButtonBox JJB-02"
//            };

//            await Task.Run(() =>
//            {
//                deviceInstances.AddRange(directInput.GetDevices(SharpDX.DirectInput.DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices));
//                deviceInstances.AddRange(directInput.GetDevices(SharpDX.DirectInput.DeviceType.Joystick, DeviceEnumerationFlags.AllDevices));

//                // Check if device found is a JohnJohn's device by name.
//                deviceInstances.RemoveAll(deviceFound => !jjJoystickNames.Any(joystickName => deviceFound.ProductName == joystickName));

//                foreach (DeviceInstance deviceInstance in deviceInstances)
//                {
//                    joystickDevicesList.Add(new Joystick(directInput, deviceInstance.InstanceGuid));
//                }
//            });

//            return joystickDevicesList;
//        }

//        private bool CheckDeviceVersionCompatibility()
//        {
//            JsonDocument deviceVersions = JsonDocument.Parse(JJManager.Properties.Resources.CompatibleDevices);

//            for (int i = 0; i < deviceVersions.RootElement.GetArrayLength(); i++)
//            {
//                if (deviceVersions.RootElement[i][0].GetString() == _ProductName)
//                {
//                    if (deviceVersions.RootElement[i][1].ValueKind == JsonValueKind.Null)
//                    {
//                        return true;
//                    }
//                    else
//                    {
//                        string[] versionSplitted = deviceVersions.RootElement[i][1].GetString().Split('.');

//                        Version deviceCompatibleVersion = null;

//                        switch (versionSplitted.Length)
//                        {
//                            case 1:
//                                deviceCompatibleVersion = new Version(int.Parse(versionSplitted[0]), 0);
//                                break;
//                            case 2:
//                                deviceCompatibleVersion = new Version(int.Parse(versionSplitted[0]), int.Parse(versionSplitted[1]));
//                                break;
//                            case 3:
//                                deviceCompatibleVersion = new Version(int.Parse(versionSplitted[0]), int.Parse(versionSplitted[1]), int.Parse(versionSplitted[2]));
//                                break;
//                            case 4:
//                                deviceCompatibleVersion = new Version(int.Parse(versionSplitted[0]), int.Parse(versionSplitted[1]), int.Parse(versionSplitted[2]), int.Parse(versionSplitted[3]));
//                                break;
//                            default:
//                                deviceCompatibleVersion = null;
//                                break;
//                        }

//                        if (_Version >= deviceCompatibleVersion)
//                        {
//                            return true;
//                        }
//                    }

//                    MessageBox.Show("O seu dispositivo '" + _ProductName + "' encontra-se na versão '" + (_Version == null ? "Não identif." : _Version.ToString()) + "' que não é mais compatível com o JJManager, para continuar utilizando todos os seus recursos realize a atualização do firmware do mesmo na aba 'Atualizações'", "Dispositivo desatualizado");
//                    return false;
//                }
//            }

//            MessageBox.Show("Parece que o seu dispositivo '" + _ProductName + "' ainda não é compatível com o JJManager, entre em contato conosco para saber quando será implementado nas próximas versões.", "Dispositivo incompatível");

//            return false;
//        }
//    }
//}