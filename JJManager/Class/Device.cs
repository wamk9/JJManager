using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Windows.Forms;
using HidSharp;
using HidSharp.Reports;
using System.Security.Cryptography;
using JJManager.Class;
using System.IO;
using SharpDX.DirectInput;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Runtime.Remoting.Lifetime;
using SharpDX;
using System.Reflection;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Status;
using AudioSwitcher.AudioApi;
using System.Text.Json;
using MaterialSkin.Controls;

namespace JJManager.Class
{
    public class Device
    {
        private HidDevice _HidDevice = null;
        private Joystick _Joystick = null;
        private String _Id = "";
        private String _ProductName = "";
        private String _SerialNumber = "";
        private String _ConnId = "";
        private String _ConnType = "";
        private Thread _ThreadSendingData = null;
        private Thread _ThreadReceivingData = null;
        private Profile _Profile = null;
        private String _JJID = "";
        private DatabaseConnection _DatabaseConnection = new DatabaseConnection();
        //private AudioManager _AudioManager = new AudioManager();
        private bool _IsConnected = false;

        public HidDevice HidDevice
        {
            get => _HidDevice;
        }
        public Joystick Joystick
        {
            get => _Joystick;
        }
        public String Id
        {
            get => _Id;
        }
        public String ProductName
        {
            get => _ProductName;
        }
        public String SerialNumber
        {
            get => _SerialNumber;
        }
        public String ConnId
        {
            get => _ConnId;
        }

        public String ConnType
        {
            get => _ConnType;
        }

        public String JJID
        {
            get => _JJID;
        }

        public bool IsConnected
        {
            get => _IsConnected;
        }
        public Profile ActiveProfile
        {
            get => _Profile;
        }

        /// <summary>
        /// Método responsável por trazer os dados disponibilizados pelo dispositivo HID para o software.
        /// </summary>
        /// <returns>Array de bytes contendo o resultado da comunicação.</returns>
        /*private byte[] HIDReceivedData()
        {
            byte[] HIDResponseBytes = new byte[64];

            try
            {
                HidStream hidStream;

                if (_HidDevice == null)
                    MessageBox.Show("Device Desconectado");

                var reportDescriptor = _HidDevice.GetReportDescriptor();

                foreach (var deviceItem in reportDescriptor.DeviceItems)
                {
                    if (_HidDevice.TryOpen(out hidStream))
                    {
                        hidStream.ReadTimeout = Timeout.Infinite;

                        using (hidStream)
                        {
                            var inputReportBuffer = new byte[_HidDevice.GetMaxInputReportLength()];
                            var inputReceiver = reportDescriptor.CreateHidDeviceInputReceiver();
                            var inputParser = deviceItem.CreateDeviceItemInputParser();

                            IAsyncResult ar = null;

                            while (true)
                            {
                                if (ar == null)
                                {
                                    ar = hidStream.BeginRead(inputReportBuffer, 0, inputReportBuffer.Length, null, null);
                                }

                                if (ar != null)
                                {
                                    if (ar.IsCompleted)
                                    {
                                        int byteCount = hidStream.EndRead(ar);
                                        ar = null;

                                        if (byteCount > 0)
                                        {
                                            HIDResponseBytes = inputReportBuffer.Take(byteCount).Where(x => x != 0x00).ToArray();
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
                        }
                    }
                    else
                    {
                        MessageBox.Show("Dispositivo indisponível no momento, selecione outro dispositivo para continuar.");
                    }
                }
            }
            catch (IOException ex)
            {

            }

            return HIDResponseBytes;
        }*/

        /// <summary>
        /// Método responsável por enviar os dados passados via parâmetro para o dispositivo HID.
        /// </summary>
        /// <param name="Data">Dados em formato 'String', contendo no máximo 64 caracteres. </param>
        private void HIDSendData(String Data)
        {
            try
            {
                if (Data.Length > 63)
                    throw new ArgumentOutOfRangeException();

                var reportDescriptor = _HidDevice.GetReportDescriptor();
                foreach (var deviceItem in reportDescriptor.DeviceItems)
                {
                    HidStream hidStream;
                    if (_HidDevice.TryOpen(out hidStream))
                    {
                        hidStream.WriteTimeout = Timeout.Infinite;
                        
                        if (hidStream.CanWrite)
                        {
                            byte[] DataReceived = Encoding.ASCII.GetBytes(Data);
                            byte[] DataTreated = new byte[(DataReceived.Length + 1)];

                            for (int i = 0; i < DataTreated.Length; i++)
                            {
                                if (i == 0)
                                    DataTreated[i] = 0x00;
                                else
                                    DataTreated[i] = DataReceived[(i - 1)];
                            }

                            hidStream.Write(DataTreated);
                            hidStream.Close();
                            hidStream.Dispose();
                        }
                    }
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // TODO: Create LOGFILE
            }
            catch (IOException ex)
            {
                // TODO: Create LOGFILE
            }
        }

        /// <summary>
        /// Construtor para dispositivos HID em geral.
        /// </summary>
        /// <param name="HidDevice">Objeto do dispositivo HID usando o padrão da biblioteca HIDSharp.</param>
        public Device(HidDevice HidDevice)
        {
            _HidDevice = HidDevice;
            _ProductName = _HidDevice.GetProductName();
            _ConnId = _HidDevice.DevicePath.GetHashCode().ToString();
            _JJID = Device.GetJJProductId(_ProductName);
            _Profile = Profile.getActiveProfile(_ConnId);
            _Id = GetUserProductId();
            _ConnType = "USB (HID)";
        }

        /// <summary>
        /// Constutor para dispositivos como Joysticks e Gamepads.
        /// </summary>
        /// <param name="joystick">Objeto do Joystick/Gamepad usando o padrão da biblioteca SharpDX.</param>
        public Device(Joystick joystick)
        {
            _Joystick = joystick;
            _ProductName = _Joystick.Properties.ProductName;
            _ConnId = _Joystick.Properties.ClassGuid.GetHashCode().ToString();
            _JJID = Device.GetJJProductId(_ProductName);
            _Profile = Profile.getActiveProfile(_ConnId);
            _Id = GetUserProductId();
            _ConnType = "USB (Joystick)";
        }

        public void UpdateActiveProfile(string profileName)
        {
            _Profile = new Profile(profileName, _JJID, 5);
        }

        public static string getJJProductName(string connId)
        {
            // Initialize DirectInput
            DirectInput directInput = new DirectInput();

            List<DeviceInstance> directInputDevices = new List<DeviceInstance>();
            List<JJManager.Class.Device> jjDevicesList = new List<JJManager.Class.Device>();

            String[] jjJoystickNames =
            {
                "ButtonBox JJB-01",
                "ButtonBox JJB-01 V2",
                "ButtonBox JJB-02"
            };

            directInputDevices.AddRange(directInput.GetDevices(SharpDX.DirectInput.DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices));
            directInputDevices.AddRange(directInput.GetDevices(SharpDX.DirectInput.DeviceType.Joystick, DeviceEnumerationFlags.AllDevices));

            // Check if device found is a JohnJohn's device by name and connection ID.
            directInputDevices.Where(
                joystickRaw => jjJoystickNames.Any(joystickName => joystickRaw.ProductName == joystickName && joystickRaw.InstanceGuid.GetHashCode().ToString() == connId)
            );

            if (directInputDevices.Count > 0)
            {
                return directInputDevices[0].ProductName;
            }

            var list = DeviceList.Local;
            list.Changed += (sender, e) => Console.WriteLine("Device list changed.");

            var hidDeviceList = list.GetHidDevices().ToArray();

            foreach (HidDevice dev in hidDeviceList)
            {
                if (dev.VendorID != 0x2341)
                    continue;

                if (dev.DevicePath.GetHashCode().ToString() == connId)
                    return dev.GetProductName();
            }

            return "";
        }

        public static string GetJJProductId(string productName)
        {
            DatabaseConnection database = new DatabaseConnection();
            string sql = "";

            if (productName != "")
            {
                sql = "SELECT id FROM dbo.jj_products WHERE product_name = '" + productName + "'";

                using (JsonDocument Json = database.RunSQLWithResults(sql))
                {
                    if (Json != null)
                        return Json.RootElement[0].GetProperty("id").ToString();
                }
            }

            return "";
        }

        /// <summary>
        /// Envia para a tela de dispositivos da série JJM o nome dos inputs que estão cadastrados no banco de dados do software.
        /// </summary>
        /// <param name="InputId">ID do input que será enviada via HID.</param>
        public void SendInputNameToDeviceScreen (int InputId, String profileId)
        {
            try
            {
                String ReturnDictionary = "";

                SortedDictionary<String, String> InputsName = _DatabaseConnection.GetAllInputName(profileId);
                String InputNameString = (InputId).ToString() + "|";

                if (InputsName.TryGetValue((InputId).ToString(), out ReturnDictionary))
                {

                    if (ReturnDictionary == "" || ReturnDictionary == null)
                        InputNameString += "Input " + (InputId).ToString();
                    else
                        InputNameString += ReturnDictionary;
                }
                else
                {
                    InputNameString += "Input " + (InputId).ToString();
                }

                HIDSendData(InputNameString);

            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private static List<JJManager.Class.Device> CheckConnectedDevices(List<JJManager.Class.Device> newDevices = null, List < JJManager.Class.Device> actualDevices = null)
        {
            actualDevices.Where(actualDevice => actualDevice.IsConnected);
            newDevices.RemoveAll(newDevice => (actualDevices.Exists (actualDevice => newDevice.ConnId == actualDevice.ConnId)));
            newDevices.AddRange(actualDevices);

            return newDevices;
        }

        private static List<JJManager.Class.Device> getJoystickDevices()
        {
            // Initialize DirectInput
            DirectInput directInput = new DirectInput();

            List<DeviceInstance> directInputDevices = new List<DeviceInstance>();
            List<JJManager.Class.Device> jjDevicesList = new List<JJManager.Class.Device>();

            String[] jjJoystickNames =
            {
                "ButtonBox JJB-01",
                "ButtonBox JJB-01 V2",
                "ButtonBox JJB-02"
            };

            directInputDevices.AddRange(directInput.GetDevices(SharpDX.DirectInput.DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices));
            directInputDevices.AddRange(directInput.GetDevices(SharpDX.DirectInput.DeviceType.Joystick, DeviceEnumerationFlags.AllDevices));

            // Check if device found is a JohnJohn's device by name.
            directInputDevices.Where(
                joystickRaw => jjJoystickNames.Any(joystickName => joystickRaw.ProductName == joystickName)
            );

            if (directInputDevices.Count() == 0)
                return jjDevicesList;

            directInputDevices.ForEach(
                joystickRaw => jjDevicesList.Add(new JJManager.Class.Device(new Joystick(directInput, joystickRaw.InstanceGuid)))
            );

            return jjDevicesList;
        }

        private static List<JJManager.Class.Device> getUSBHidDevices(List<JJManager.Class.Device> actualDevices = null)
        {
            List<HidDevice> hidDevicesList = DeviceList.Local.GetHidDevices(0x2341).ToList();
            List<JJManager.Class.Device> jjDevicesList = new List<JJManager.Class.Device>();
            String[] jjHidNames =
            {
                "StreamDeck JJSD-01",
                "Mixer de Áudio JJM-01"
            };

            hidDevicesList.RemoveAll(deviceFound => !jjHidNames.Any(hidName => deviceFound.GetProductName() == hidName));
            
            if (actualDevices != null)
            {
                hidDevicesList.RemoveAll(deviceFound => actualDevices.Exists(actualDevice => actualDevice.ConnId == deviceFound.DevicePath.GetHashCode().ToString() && actualDevice.IsConnected));
                jjDevicesList.AddRange(actualDevices.FindAll(actualDevice => actualDevice.IsConnected));
            }

            hidDevicesList.ForEach(hidDevice =>
            {
                jjDevicesList.Add(new JJManager.Class.Device(hidDevice));
            });
            
            return jjDevicesList;
        }

        public static List<JJManager.Class.Device> getDevicesList(List<JJManager.Class.Device> actualDevicesList = null)
        {
            List<JJManager.Class.Device> newDevicesList = new List<JJManager.Class.Device>();
            //List<JJManager.Class.Device> connectedDevicesList = actualDeviceList != null ? actualDeviceList.FindAll(actual => actual.IsConnected).ToList() : new List<JJManager.Class.Device>();

            newDevicesList.AddRange(getJoystickDevices());
            newDevicesList.AddRange(getUSBHidDevices());

            newDevicesList = CheckConnectedDevices(newDevicesList, actualDevicesList);
            // Check if device found is on the actual list, if is they will be removed on this list.
            /*
            devicesList.RemoveAll(
                joystick => connectedDevicesList.Exists(connectedDevice => connectedDevice.ConnId == joystick.ConnId)
            );
            */

            return newDevicesList;
        }

        public bool Connect()
        {
            if (_IsConnected)
                return true;

            switch (_ProductName)
            {
                case "Mixer de Áudio JJM-01":
                    _ThreadReceivingData = new Thread(() =>
                    {
                        Devices.JJM01 device = new Devices.JJM01(this);
                        String[] receivedMessage = null;

                        while (true)
                        {
                            receivedMessage = device.ReceiveMessage();

                            if (receivedMessage == null)
                            {
                                continue;
                            }

                            for (int i = 0; i < receivedMessage.Length; i++)
                            {
                                device.ExecuteInputFunction((i + 1), receivedMessage[i]);
                            }

                            //Thread.Sleep(10000);
                        }
                    });
                    _ThreadReceivingData.Name = "Thread_Receiving_Data_JJM01";

                    _ThreadSendingData = new Thread(() =>
                    {
                        Devices.JJM01 device = new Devices.JJM01(this);

                        device.SendMessage();

                        while (true)
                        {
                            device.SendMessage();

                            Thread.Sleep(100);
                        }
                    });
                    _ThreadSendingData.Name = "Thread_Sending_Data_JJM01";
                    break;
                case "ButtonBox JJB-01":
                    _ThreadReceivingData = new Thread(() =>
                    {
                        Devices.JJB01 device = new Devices.JJB01(this);

                        while (true)
                        {
                            int valueX = GetJoystickAxisPercentage(_Joystick, "X");
                            int valueY = GetJoystickAxisPercentage(_Joystick, "Y");

                            if (valueX != -1)
                            {
                                device.ExecuteInputFunction(1, valueX.ToString());
                            }

                            if (valueY != -1)
                            {
                                device.ExecuteInputFunction(2, valueY.ToString());
                            }

                            //Thread.Sleep(100);
                        }
                    });
                    _ThreadReceivingData.Name = "Thread_Receiving_Data_JJB01";

                    break;

            }
            
            if (_ThreadReceivingData != null)
            {
                _ThreadReceivingData.Start();
            }

            if (_ThreadSendingData != null)
            {
                _ThreadSendingData.Start();
            }

            _IsConnected = true;

            return true;
        }

        public bool Disconnect()
        {
            if (_ThreadReceivingData != null)
            {
                _ThreadReceivingData.Abort();
                //_ThreadConnection.Interrupt();
                _ThreadReceivingData = null;
            }

            if (_ThreadSendingData != null)
            {
                _ThreadSendingData.Abort();
                //_ThreadConnection.Interrupt();
                _ThreadSendingData = null;
            }
            _IsConnected = false;

            return true;
        }

        private String GetUserProductId()
        {
            DatabaseConnection database = new DatabaseConnection();
            String sql = "SELECT id FROM dbo.user_products WHERE conn_id = '" + _ConnId + "';";
            String id = "";

            using (JsonDocument Json = database.RunSQLWithResults(sql))
            {
                if (Json == null)
                    id = SetUserProductId();
                else
                    id = Json.RootElement[0].GetProperty("id").ToString();
            }

            return id;
        }

        private String SetUserProductId()
        {
            DatabaseConnection database = new DatabaseConnection();
            string id = "";
            String sql = "INSERT INTO dbo.user_products (" +
                    " id_product," +
                    " conn_id," +
                    " id_profile" +
                ") VALUES (" +
                    " '" + _JJID + "',"+
                    " '" + _ConnId + "'," +
                    " (SELECT id from dbo.profiles WHERE id_product = '" + _JJID + "' AND name = 'Perfil Padrão')" +
                ");";

            if (database.RunSQL(sql))
            {
                sql = "SELECT id FROM dbo.user_products WHERE conn_id = '" + _ConnId + "';";

                using (JsonDocument Json = database.RunSQLWithResults(sql))
                {
                    id = Json.RootElement[0].GetProperty("id").ToString();
                    //SaveProfile("Perfil Padrão", id);
                }
            }
            return id;
        }

        public void OpenEditWindow(MaterialForm parent)
        {
            parent.Visible = false;

            switch (_ProductName)
            {
                case "Mixer de Áudio JJM-01":
                    Pages.Mixers.JJM_01 MixerScreen = new Pages.Mixers.JJM_01(parent, this);
                    MixerScreen.Show();
                    MixerScreen.Activate();

                    while (MixerScreen.Enabled)
                        Application.DoEvents();

                    break;
                case "ButtonBox JJB-01":
                    Pages.ButtonBox.JJB_01 BBScreen = new Pages.ButtonBox.JJB_01(parent, this);
                    BBScreen.Show();
                    BBScreen.Activate();

                    while (BBScreen.Enabled)
                        Application.DoEvents();

                    break;
                default:
                    MessageBox.Show("Este dispositivo ainda não é compatível com o JJManager, aguarde pelas próximas versões do software para tentar novamente.");
                    break;
            }

            parent.Visible = true;
        }
        public int GetJoystickAxisPercentage(Joystick joystick, String joystickAxis)
        {
            try
            {
                joystick.Acquire();

                joystick.Poll();
                var currentState = joystick.GetCurrentState(); //only show the last state

                if (joystickAxis == "X")
                    return (currentState.X * 100 / 65530);
                else if (joystickAxis == "Y")
                    return (currentState.Y * 100 / 65530);

                joystick.Unacquire();
            }
            catch (SharpDXException ex)
            {
                if ((ex.ResultCode == ResultCode.NotAcquired) || (ex.ResultCode == ResultCode.InputLost))
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return -1;
        }
    }
}