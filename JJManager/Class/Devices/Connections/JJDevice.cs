using HidSharp;
using MaterialSkin.Controls;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JoystickClass = JJManager.Class.Devices.Connections.Joystick;
using ProfileClass = JJManager.Class.App.Profile.Profile;
namespace JJManager.Class.Devices
{
    public class JJDevice : INotifyPropertyChanged
    {
        public enum Type
        {
            Unsetted,
            Joystick,
            HID,
            Bluetooth
        };

        #region GeneralProductVariables
        protected DatabaseConnection _dbConnection = null;
        protected string _productId = null; // ID in jj_products table
        protected string _productName = null; // Name of product in jj_products table
        protected Type _type = Type.Unsetted;
        protected HashSet<ushort> _cmds = new HashSet<ushort>(); // Set of connection commands that this device supports
        #endregion

        #region VariablesOfUser
        protected string _deviceId = null; // ID saved in user_products table, 'id' column
        protected string _connId = null; // ID of connection, most of times is a GetHashCode()
        protected bool _autoConn = false; // Auto connection check, getted in user_products table, 'auto_conn' column
        protected bool _isConnected = false; // Auto connection check, getted in user_products table, 'auto_conn' column
        protected List<string> _connPort = null; // Connection port  (COM2, COM3...), is a list because we can get one more in keyboard devices (per example, old firmware of JJSD-01), getting same VID/PID if two or more is connected.
        protected ProfileClass _profile = null; // Actual profile of this device, getted in user_products, 'id_profile' column
        protected Version _version = null; // Actual firmware of this device, necessary to update or connect in a especific device
        protected bool _sendInProgress = false; // Used when any data are going to device
        protected bool _receiveInProgress = false; // Used when JJManager is wating to receive anything of device
        protected bool _requestInProgress = false; // Used when JJManager is requesting anything of device
        protected bool _cancelAutoConnect = false; // If has a error, cancel auto connection
        #endregion

        #region ThreadVariables
        protected Action _actionSendingData = null;
        protected Action _actionReceivingData = null;
        protected Action _actionResetParams = null;
        protected Thread _threadSendingData = null; // Responsable to send data to device
        protected Thread _threadReceivingData = null; // Responsable to receive data from device
        protected Thread _threadResetParams = null; // Responsable to reset connection params from device
        protected Task _taskReceivingData = null;
        protected Task _taskSendingData = null;
        protected Task _taskResetParams = null;
        #endregion

        #region FormsToWork
        protected Main main = null;
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        #region PublicVariables
        public string ConnId
        {
            get => _connId;
        }
        public string ProductName
        {
            get => _productName;
        }
        public string ProductId
        {
            get => _productId;
        }
        public List<string> ConnPort
        {
            get => _connPort;
        }
        public Version Version
        {
            get => _version;
        }
        public Type DeviceType
        {
            get => _type;
        }
        public ProfileClass Profile
        {
            get => _profile;
            set
            {
                if (_profile != value)
                {
                    _profile = value;
                    // Automatically trigger profile update when profile changes
                    if (_profile != null)
                    {
                        _profile.NeedsUpdate = true;
                    }
                }
            }
        }
        public bool IsConnected
        {
            get => _isConnected;
        }
        public bool AutoConnect
        {
            get => _autoConn;
            set => SetAutoConnection(value);
        }

        public bool IsAutoConnectCancelled
        {
            get => _cancelAutoConnect;
            set => _cancelAutoConnect = value;
        }
        #endregion

        public JJDevice()
        {
            _dbConnection = new DatabaseConnection();
        }

        private void SetAutoConnection(bool autoConn)
        {
            _autoConn = autoConn;

            string sql = $@"UPDATE dbo.user_products SET auto_connect = {(_autoConn ? 1 : 0)} WHERE conn_id = '{_connId}' AND id_product = '{_productId}'";

            if (_dbConnection.RunSQL(sql))
            {
                // OK
            }
        }

        protected Version TranslateVersion(string version)
        {
            if (version == null || version == string.Empty)
            {
                return null;
            }

            Version translatedVersion = null;

            string[] versionSplitted = version.Split('.');

            switch (versionSplitted.Length)
            {
                case 1:
                    translatedVersion = new Version(int.Parse(versionSplitted[0]), 0);
                    break;
                case 2:
                    translatedVersion = new Version(int.Parse(versionSplitted[0]), int.Parse(versionSplitted[1]));
                    break;
                case 3:
                    translatedVersion = new Version(int.Parse(versionSplitted[0]), int.Parse(versionSplitted[1]), int.Parse(versionSplitted[2]));
                    break;
                case 4:
                    translatedVersion = new Version(int.Parse(versionSplitted[0]), int.Parse(versionSplitted[1]), int.Parse(versionSplitted[2]), int.Parse(versionSplitted[3]));
                    break;
                default:
                    translatedVersion = null;
                    break;
            }

            return translatedVersion;
        }
        protected void GetAutoConnection()
        {
            try
            {
                if (string.IsNullOrEmpty(_connId))
                {
                    throw new ArgumentNullException("_connId", "Necessary product connection ID to check if auto connection is enabled");
                }

                if (string.IsNullOrEmpty(_productId))
                {
                    throw new ArgumentNullException("_productId", "Necessary product ID to check if auto connection is enabled");
                }

                string sql = $"SELECT auto_connect FROM dbo.user_products WHERE conn_id = '{_connId}' AND id_product = '{_productId}';";

                foreach (JsonObject obj in _dbConnection.RunSQLWithResults(sql))
                {
                    if (obj.ContainsKey("auto_connect"))
                    {
                        _autoConn = Boolean.Parse(obj["auto_connect"].GetValue<string>());
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                Log.Insert("Device", $"Ocorreu um problema ao tentarmos buscar se o produto '{_productName}' possui auto conexão habilitada", ex);
            }
        }

        public static string GetClassNameFromDB(string productName, string connType)
        {
            DatabaseConnection dbConnection = new DatabaseConnection();
            string className = null;

            try
            {
                if (string.IsNullOrEmpty(productName))
                {
                    throw new ArgumentNullException("productName", "Necessary product name to check if exists info about the class");
                }

                if (string.IsNullOrEmpty(connType))
                {
                    throw new ArgumentNullException("connType", "Necessary product connection type to check if exists info about the class");
                }

                string sql = $"SELECT class_name FROM dbo.jj_products WHERE product_name = '{productName}' AND conn_type = '{connType}';";

                foreach (JsonObject json in dbConnection.RunSQLWithResults(sql))
                {
                    if (json.ContainsKey("class_name"))
                    {
                        className = json["class_name"].GetValue<string>();
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                Log.Insert("Device", $"Ocorreu um problema ao tentarmos buscar os dados de classe do produto '{productName}'", ex);
            }

            return className;
        }

        private Type ToType(string value)
        {
            switch (value)
            {
                case "joystick":
                    return Type.Joystick;
                case "hid":
                    return Type.HID;
                case "bluetooth":
                    return Type.Bluetooth;
            }

            return Type.Unsetted;
        }

        public void OpenEditWindow(MaterialForm parent)
        {
            parent.Visible = false;

            // Class name as a string
            string className = $"JJManager.Pages.Devices.{GetClassNameFromDB(_productName, _type.ToString())}";

            // Assembly where the class is defined
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Get the Type object corresponding to the class name
            System.Type type = assembly.GetType(className);

            if (type != null)
            {
                // Create an instance of the class using Activator
                object instance = Activator.CreateInstance(type, parent, this);
                (instance as MaterialForm).ShowDialog();
            }
            else
            {
                Pages.App.MessageBox.Show(parent, "Dispositivo Incompatível", "Este dispositivo ainda não é compatível com o JJManager, aguarde pelas próximas versões do software para tentar novamente.");
                //Console.WriteLine($"Class '{className}' not found.");
            }

            parent.Visible = true;
        }

        public virtual bool Connect()
        {
            if (!CheckDeviceVersionCompatibility())
            {
                return false;
            }

            if (_isConnected)
            {
                return true;
            }

            // Start receiving task only if not already running
            if (_actionReceivingData != null && (_taskReceivingData == null || _taskReceivingData.IsCompleted))
            {
                _taskReceivingData = Task.Run(_actionReceivingData);
            }

            // Start sending task only if not already running
            if (_actionSendingData != null && (_taskSendingData == null || _taskSendingData.IsCompleted))
            {
                _taskSendingData = Task.Run(_actionSendingData);
            }

            // Start reset params task only if not already running
            if (_actionResetParams != null && (_taskResetParams == null || _taskResetParams.IsCompleted))
            {
                _taskResetParams = Task.Run(_actionResetParams);
            }

            if (_taskReceivingData != null && !_taskReceivingData.IsCompleted ||
                _taskSendingData != null && !_taskSendingData.IsCompleted ||
                _taskResetParams != null && !_taskResetParams.IsCompleted)
            {
                _isConnected = true;
                OnPropertyChanged("IsConnected");
            }

            return _isConnected;
        }

        public virtual bool Disconnect()
        {
            // Signal disconnection first
            _isConnected = false;

            // Wait for Threads to stop (legacy support)
            _threadReceivingData?.Join();
            _threadSendingData?.Join();
            _threadResetParams?.Join();

            // Wait for Tasks to complete (with 5 second timeout each)
            try
            {
                _taskReceivingData?.Wait(5000);
                _taskSendingData?.Wait(5000);
                _taskResetParams?.Wait(5000);
            }
            catch (AggregateException)
            {
                // Tasks were cancelled or timed out - this is expected
            }

            // Reset tasks to null to prevent reuse
            _taskReceivingData = null;
            _taskSendingData = null;
            _taskResetParams = null;

            // Run reset action if defined
            if (_actionResetParams != null)
            {
                _taskResetParams = Task.Run(_actionResetParams);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            OnPropertyChanged("IsConnected");

            return true;
        }

        protected void GetProductID()
        {
            try
            {
                if (string.IsNullOrEmpty(_productId))
                {
                    if (string.IsNullOrEmpty(_productName))
                    {
                        throw new ArgumentNullException("_productName", "Necessary product's name to check if exists a ID");
                    }

                    string sql = $"SELECT id FROM dbo.jj_products WHERE product_name = '{_productName}'";

                    foreach (JsonObject obj in _dbConnection.RunSQLWithResults(sql))
                    {
                        if (obj.ContainsKey("id"))
                        {
                            _productId = obj["id"].GetValue<string>();
                        }
                    }

                    if (string.IsNullOrEmpty(_productId))
                    {
                        throw new ArgumentNullException("_productId", "ID not found because this product isn't registered in database table 'jj_products'");
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                Log.Insert("Device", $"Ocorreu um problema ao tentarmos buscar o ID do produto '{_productName}'", ex);
            }
        }


        protected void GetUserProductID()
        {
            try
            {
                if (string.IsNullOrEmpty(_deviceId))
                {
                    if (string.IsNullOrEmpty(_connId))
                    {
                        throw new ArgumentNullException("_connId", "Necessary product connection ID to check if exists a product user ID");
                    }

                    string sql = $"SELECT id FROM dbo.user_products WHERE conn_id = '{_connId}';";

                    foreach (JsonObject obj in _dbConnection.RunSQLWithResults(sql))
                    {
                        if (obj.ContainsKey("id"))
                        {
                            _deviceId = obj["id"].GetValue<string>();
                        }
                    }

                    if (string.IsNullOrEmpty(_deviceId))
                    {
                        SetUserProductID();
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                Log.Insert("Device", $"Ocorreu um problema ao tentarmos buscar o ID do produto '{_productName}'", ex);
            }
        }

        protected void SetUserProductID()
        {
            try
            {
                if (string.IsNullOrEmpty(_deviceId))
                {
                    if (string.IsNullOrEmpty(_connId))
                    {
                        throw new ArgumentNullException("_connId", "Necessary product connection ID to check if exists a product user ID");
                    }

                    string sql = $@"INSERT INTO dbo.user_products 
                                (
                                    id_product,
                                    conn_id,
                                    id_profile
                                ) VALUES (
                                    '{_productId}',
                                    '{_connId}',
                                    (SELECT TOP 1 id from dbo.profiles WHERE id_product = '{_productId}' ORDER BY id DESC)
                                );";

                    if (_dbConnection.RunSQL(sql))
                    {
                        sql = $"SELECT id FROM dbo.user_products WHERE conn_id = '{_connId}';";

                        foreach (JsonObject obj in _dbConnection.RunSQLWithResults(sql))
                        {
                            if (obj.ContainsKey("id"))
                            {
                                _deviceId = obj["id"].GetValue<string>();
                            }
                        }
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                Log.Insert("Device", $"Ocorreu um problema ao tentarmos buscar o ID do produto '{_productName}'", ex);
            }
        }

        protected bool CheckDeviceVersionCompatibility()
        {
            JsonArray deviceVersions = JsonArray.Parse(JJManager.Properties.Resources.CompatibleDevices).AsArray();

            foreach (JsonArray deviceVersion in deviceVersions)
            {
                Version deviceCompatibleVersion = null;

                if (deviceVersion[0].GetValue<string>() == _productName)
                {
                    if (deviceVersion[1] == null || deviceVersion[1].GetValueKind() == JsonValueKind.Null)
                    {
                        return true;
                    }
                    else
                    {
                        string[] versionSplitted = deviceVersion[1].GetValue<string>().Split('.');

                        switch (versionSplitted.Length)
                        {
                            case 1:
                                deviceCompatibleVersion = new Version(int.Parse(versionSplitted[0]), 0);
                                break;
                            case 2:
                                deviceCompatibleVersion = new Version(int.Parse(versionSplitted[0]), int.Parse(versionSplitted[1]));
                                break;
                            case 3:
                                deviceCompatibleVersion = new Version(int.Parse(versionSplitted[0]), int.Parse(versionSplitted[1]), int.Parse(versionSplitted[2]));
                                break;
                            case 4:
                                deviceCompatibleVersion = new Version(int.Parse(versionSplitted[0]), int.Parse(versionSplitted[1]), int.Parse(versionSplitted[2]), int.Parse(versionSplitted[3]));
                                break;
                            default:
                                deviceCompatibleVersion = null;
                                break;
                        }

                        if (_version >= deviceCompatibleVersion)
                        {
                            return true;
                        }
                    }
                    Pages.App.MessageBox.Show(null, "Dispositivo Desatualizado", "O seu dispositivo '" + _productName + "' encontra-se na versão '" + (_version == null ? "Não identif." : _version.ToString()) + "' que não é mais compatível com o JJManager, para continuar utilizando todos os seus recursos realize a atualização do firmware do mesmo na aba 'Atualizações'\n\nPara conectar '" + _productName + "' você precisará da versão de firmeware '" + deviceCompatibleVersion.ToString() + "'");
                    return false;
                }
            }

            Pages.App.MessageBox.Show(null, "Dispositivo Incompatível", "Parece que o seu dispositivo '" + _productName + "' ainda não é compatível com o JJManager, entre em contato conosco para saber quando será implementado nas próximas versões.");

            return false;
        }

        public static async Task<List<string>> GetUnavailableListEntries(ObservableCollection<JJDevice> devices)
        {
            List<HidDevice> hidDevices = await GetHIDList();
            List<Joystick> joysticks = await GetJoystickList();
            List<string> listToReturn = new List<string>();

            foreach (JJDevice device in devices.Where(device => device.DeviceType == Type.HID))
            {
                if (!hidDevices.Any(hidDevice => hidDevice.DevicePath.GetHashCode().ToString() == device.ConnId))
                {
                    listToReturn.Add(device.ConnId);
                }
            }

            foreach (JJDevice device in devices.Where(device => device.DeviceType == Type.Joystick))
            {
                if (!joysticks.Any(joystick => joystick.Properties.ClassGuid.GetHashCode().ToString() == device.ConnId))
                {
                    listToReturn.Add(device.ConnId);
                }
            }

            return listToReturn;
        }

        public static async Task<List<JJDevice>> GetAvailableListEntries(ObservableCollection<JJDevice> devices)
        {
            List<JJDevice> listToReturn = new List<JJDevice>();

            try
            {
                List<HidDevice> hidDevices = await GetHIDList();
                List<Joystick> joysticks = await GetJoystickList();

                hidDevices.RemoveAll(hidDevice =>
                {
                    return (devices.Any(device =>
                    {
                        return (device.ConnId == hidDevice.DevicePath.GetHashCode().ToString());
                    }));
                });

                joysticks.RemoveAll(joystick =>
                {
                    return (devices.Any(device =>
                    {
                        return (device.ConnId == joystick.Properties.ClassGuid.GetHashCode().ToString());
                    }));
                });

                hidDevices.ForEach(hidDevice =>
                {
                    // Class name as a string
                    string className = $"JJManager.Class.Devices.{GetClassNameFromDB(hidDevice.GetProductName(), "HID")}";

                    // Assembly where the class is defined
                    Assembly assembly = Assembly.GetExecutingAssembly();

                    // Get the Type object corresponding to the class name
                    System.Type type = assembly.GetType(className);

                    if (type != null)
                    {
                        // Create an instance of the class using Activator
                        object instance = Activator.CreateInstance(type, hidDevice);
                        listToReturn.Add(instance as JJDevice);
                    }
                    else
                    {
                        //Console.WriteLine($"Class '{className}' not found.");
                    }
                });

                joysticks.ForEach(joystick =>
                {
                    listToReturn.Add(new JoystickClass(joystick));
                });

            }
            catch (Exception ex)
            {
                Log.Insert("Device", "Ocorreu um problema ao realizar a busca dos dispositivos", ex);
            }

            return listToReturn;
        }

        private static async Task<List<HidDevice>> GetHIDList()
        {
            List<HidDevice> hidDevicesList = DeviceList.Local.GetHidDevices(0x2341).ToList();
            var devicePaths = new HashSet<string>();
            var duplicateDevicePaths = new List<string>();
            bool problemWithDevice = false;
            string messageProblemWithDevice = "Um ou mais dispositivos estão com problema de conexão, verifique suas configurações de energia USB";
            String[] jjHidNames =
            {
                "Streamdeck JJSD-01",
                "Mixer de Áudio JJM-01",
                "ButtonBox JJB-01 V2", // Gerenciamento de Leds
                "ButtonBox JJBP-06", // Gerenciamento de Leds
                "ButtonBox JJB-999", // Gerenciamento de Leds
                "Hub ARGB JJHL-01",
                "Hub ARGB JJHL-01 Plus",
                "Hub RGB JJHL-02",
                "Hub RGB JJHL-02 Plus",
                "Dashboard JJDB-01",
                "LoadCell JJLC-01",
                "ButtonBox JJB-Slim Type A"
            };

            await Task.Run(() =>
            {
                hidDevicesList.RemoveAll(deviceFound =>
                {
                    try
                    {
                        string productName = deviceFound.GetProductName();
                        return !jjHidNames.Any(hidName => productName == hidName);
                    }
                    catch (IOException)
                    {
                        // Log the error, remove the device
                        problemWithDevice = true;
                        return true;
                    }
                });

                // Remove Devices how has 2 HIDs, one is Joystick (Ex: ButtonBoxes)
                hidDevicesList.RemoveAll(deviceFound =>
                    deviceFound.GetReportDescriptor().Reports[0].ReportID == (byte)0xFF
                );

                // Remove Devices how has two HIDs, one is Keyboard HIDs (Ex: StreamDeck JJSD-01 Updated)
                hidDevicesList.RemoveAll(
                    device => device.DevicePath.Contains("&mi_03#") &&
                    device.GetReportDescriptor().Reports[0].ReportID == (byte)0x02
                );

                Main main = Application.OpenForms.Cast<Main>().FirstOrDefault(f => f is Main);

                if (main != null && main.InvokeRequired)
                {
                    main.BeginInvoke((MethodInvoker)delegate
                    {
                        StatusStrip connectStatusStrip = ((StatusStrip)main.Controls["tabMain"].Controls["tabConnect"].Controls["statusStrip"]);
                        StatusStrip updateStatusStrip = ((StatusStrip)main.Controls["tabMain"].Controls["tabUpdate"].Controls["statusStrip1"]);

                        if (problemWithDevice && connectStatusStrip.Items["txtStatus"].Text == string.Empty)
                        {
                            connectStatusStrip.Items["txtStatus"].Text = messageProblemWithDevice;
                        }
                        else if (connectStatusStrip.Items["txtStatus"].Text == messageProblemWithDevice)
                        {
                            connectStatusStrip.Items["txtStatus"].Text = "";
                        }

                        if (problemWithDevice && updateStatusStrip.Items["txtStatusUpdate"].Text == string.Empty)
                        {
                            updateStatusStrip.Items["txtStatusUpdate"].Text = messageProblemWithDevice;
                        }
                        else if (updateStatusStrip.Items["txtStatusUpdate"].Text == messageProblemWithDevice)
                        {
                            updateStatusStrip.Items["txtStatusUpdate"].Text = "";
                        }
                    });
                }
                else if (main != null)
                {
                    StatusStrip connectStatusStrip = ((StatusStrip)main.Controls["tabMain"].Controls["tabConnect"].Controls["statusStrip"]);
                    StatusStrip updateStatusStrip = ((StatusStrip)main.Controls["tabMain"].Controls["tabUpdate"].Controls["statusStrip1"]);

                    if (problemWithDevice && connectStatusStrip.Items["txtStatus"].Text == string.Empty)
                    {
                        connectStatusStrip.Items["txtStatus"].Text = messageProblemWithDevice;
                    }
                    else if (connectStatusStrip.Items["txtStatus"].Text == messageProblemWithDevice)
                    {
                        connectStatusStrip.Items["txtStatus"].Text = "";
                    }

                    if (problemWithDevice && updateStatusStrip.Items["txtStatusUpdate"].Text == string.Empty)
                    {
                        updateStatusStrip.Items["txtStatusUpdate"].Text = messageProblemWithDevice;
                    }
                    else if (updateStatusStrip.Items["txtStatusUpdate"].Text == messageProblemWithDevice)
                    {
                        updateStatusStrip.Items["txtStatusUpdate"].Text = "";
                    }
                }
            });

            return hidDevicesList;
        }

        private static async Task<List<Joystick>> GetJoystickList()
        {
            DirectInput directInput = new DirectInput();

            List<DeviceInstance> deviceInstances = new List<DeviceInstance>();
            List<Joystick> joystickDevicesList = new List<Joystick>();

            String[] jjJoystickNames =
            {
                "ButtonBox JJB-01",
                "ButtonBox JJB-02"
            };

            await Task.Run(() =>
            {
                deviceInstances.AddRange(directInput.GetDevices(SharpDX.DirectInput.DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices));
                deviceInstances.AddRange(directInput.GetDevices(SharpDX.DirectInput.DeviceType.Joystick, DeviceEnumerationFlags.AllDevices));

                // Check if device found is a JohnJohn's device by name.
                deviceInstances.RemoveAll(deviceFound => !jjJoystickNames.Any(joystickName => deviceFound.ProductName == joystickName));

                foreach (DeviceInstance deviceInstance in deviceInstances)
                {
                    joystickDevicesList.Add(new Joystick(directInput, deviceInstance.InstanceGuid));
                }
            });

            return joystickDevicesList;
        }

        public static void CheckRestartAllProfileFile(ref ObservableCollection<JJDevice> devices)
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JohnJohn3D", "JJManager", "RestartProfileFile");
                if (!File.Exists(filePath))
                {
                    return;
                }

                foreach (var device in devices)
                {
                    device.Profile.NeedsUpdate = true;
                }

                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Log.Insert("JJDevice", "Falha ao tentar manipular o arquivo de reset de perfil.", ex);
            }
        }
    }
}
