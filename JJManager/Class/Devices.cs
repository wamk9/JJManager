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
using JJManager.Pages.ButtonBox;
using JJManager.Pages;
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

namespace JJManager.Class
{
    public class Devices
    {
        private HidDevice _HidDevice = null;
        private Joystick _Joystick = null;
        private String _Id = "";
        private String _ProductName = "";
        private String _SerialNumber = "";
        

        private DatabaseConnection _DatabaseConnection = new DatabaseConnection();

        public HidDevice HidDevice
        {
            get => _HidDevice;
            set { _HidDevice = value; }
        }
        public Joystick Joystick
        {
            get => _Joystick;
            set { _Joystick = value; }
        }
        public String Id
        {
            get => _Id;
            set { _Id = value; }
        }
        public String ProductName
        {
            get => _ProductName;
            set { _ProductName = value; }
        }
        public String SerialNumber
        {
            get => _SerialNumber;
            set { _SerialNumber = value; }
        }

        /// <summary>
        /// Método responsável por trazer os dados disponibilizados pelo dispositivo HID para o software.
        /// </summary>
        /// <returns>Array de bytes contendo o resultado da comunicação.</returns>
        private byte[] HIDReceivedData()
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
        }

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
        public Devices(HidDevice HidDevice)
        {
            _HidDevice = HidDevice;
            _ProductName = _HidDevice.GetProductName();
            _Id = _DatabaseConnection.GetProductId(_ProductName);
        }

        /// <summary>
        /// Constutor para dispositivos como Joysticks e Gamepads.
        /// </summary>
        /// <param name="joystick">Objeto do Joystick/Gamepad usando o padrão da biblioteca SharpDX.</param>
        public Devices(Joystick joystick)
        {
            _Joystick = joystick;
            _ProductName = _Joystick.Properties.ProductName;
            _Id = _DatabaseConnection.GetProductId(_ProductName);
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











        public void SendHIDInputs(String Model, int Inputs)
        {
            try
            {
                var reportDescriptor = _HidDevice.GetReportDescriptor();
                String ReturnDictionary = "";
                DatabaseConnection _DatabaseConnection = new DatabaseConnection();

                foreach (var deviceItem in reportDescriptor.DeviceItems)
                {
                    HidStream hidStream;
                    if (_HidDevice.TryOpen(out hidStream))
                    {
                        hidStream.ReadTimeout = Timeout.Infinite;

                        SortedDictionary<String, String> InputNames = _DatabaseConnection.GetAllInputName(_Id);


                        String InputsString = "";

                        for (int i = 0; i < Inputs; i++)
                        {
                            if (InputNames.TryGetValue((i + 1).ToString(), out ReturnDictionary))
                            {
                                if (i == 0)
                                {
                                    if (ReturnDictionary == "" || ReturnDictionary == null)
                                        InputsString = "Input " + (i + 1).ToString();
                                    else
                                        InputsString = ReturnDictionary;
                                }
                                else
                                {
                                    if (ReturnDictionary == "")
                                        InputsString += "|" + "Input " + (i + 1).ToString();
                                    else
                                        InputsString += "|" + ReturnDictionary;
                                }
                            }
                            else
                            {
                                if (i == 0)
                                {
                                    InputsString = "Input " + (i + 1).ToString();
                                }
                                else
                                {
                                    InputsString += "|" + "Input " + (i + 1).ToString();
                                }

                            }
                        }

                        //byte[] input = Encoding.ASCII.GetBytes("IPT1|Teste");
                        byte[] input = Encoding.ASCII.GetBytes(InputsString);
                        byte[] inputTreated = new byte[(input.Length + 1)];
                        for (int i = 0; i < inputTreated.Length; i++)
                        {
                            if (i == 0)
                                inputTreated[i] = 0x00;
                            else
                                inputTreated[i] = input[(i - 1)];
                        }

                        hidStream.Write(inputTreated);
                    }
                }
            }
            catch (IOException ex)
            {

            }
            catch (ThreadAbortException ex)
            {
                MessageBox.Show("Bugou");
            }
        }

        public String ReceiveHIDMessage()
        {
            try
            {
                HidStream hidStream;
                String HIDResponseTreated = "";
                byte[] HIDResponseBytes = new byte[64];

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
                                            byteCount -= 9;
                                            HIDResponseBytes = inputReportBuffer.Skip(9).Take(byteCount).Where(x => x != 0x00).ToArray();
                                            HIDResponseTreated = Encoding.ASCII.GetString(HIDResponseBytes).Trim();

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
                        return HIDResponseTreated;
                    }
                    else
                    {
                        MessageBox.Show("Dispositivo indisponível no momento, selecione outro dispositivo para continuar.");
                        return String.Empty;
                    }
                }
            }
            catch (IOException ex)
            {

            }
            catch(ThreadAbortException ex)
            {

            }

            return String.Empty;
        }


        public int GetJoystickAxisPercentage(Joystick joystick, String joystickAxis)
        {
            try
            {
                joystick.Poll();
                var currentState = joystick.GetCurrentState(); //only show the last state

                if (joystickAxis == "X")
                    return (currentState.X * 100 / 65530);
                else if (joystickAxis == "Y")
                    return (currentState.Y * 100 / 65530);
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