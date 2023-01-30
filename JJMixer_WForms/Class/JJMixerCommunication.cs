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
using JJMixer_WForms.Pages.ButtonBox;
using JJMixer_WForms.Pages;
using HidSharp.Reports;
using System.Security.Cryptography;
using JJMixer_WForms.Class;
using System.IO;
using SharpDX.DirectInput;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Runtime.Remoting.Lifetime;

namespace JJMixer.Class
{
    public class JJMixerCommunication
    {
        private HidDevice _Device;
        private Joystick _Joystick;

        public HidDevice Device
        {
            get => _Device;
            set { _Device = value; }
        }
        public Joystick Joystick
        {
            get => _Joystick;
            set { _Joystick = value; }
        }

        public JJMixerCommunication(HidDevice device)
        {
            _Device = device;
        }

        public JJMixerCommunication(Joystick joystick)
        {
            _Joystick = joystick;
        }

        public void SendHIDInputs(String Model, int Inputs)
        {
            try
            {
                var reportDescriptor = _Device.GetReportDescriptor();
                String ReturnDictionary = "";
                JJMixerDbConnection _JJMixerDbConnection = new JJMixerDbConnection();

                foreach (var deviceItem in reportDescriptor.DeviceItems)
                {
                    HidStream hidStream;
                    if (_Device.TryOpen(out hidStream))
                    {
                        hidStream.ReadTimeout = Timeout.Infinite;

                        SortedDictionary<String, String> InputNames = _JJMixerDbConnection.GetAllInputName("JJM-01");


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
        }

        public String ReceiveHIDMessage()
        {
            try
            {
                HidStream hidStream;
                String HIDResponseTreated = "";
                byte[] HIDResponseBytes = new byte[64];

                if (_Device == null)
                    MessageBox.Show("Device Desconectado");

                var reportDescriptor = _Device.GetReportDescriptor();

                foreach (var deviceItem in reportDescriptor.DeviceItems)
                {
                    if (_Device.TryOpen(out hidStream))
                    {
                        hidStream.ReadTimeout = Timeout.Infinite;

                        using (hidStream)
                        {

                            var inputReportBuffer = new byte[_Device.GetMaxInputReportLength()];
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
                                            HIDResponseTreated = Encoding.ASCII.GetString(HIDResponseBytes).Trim();

                                            return HIDResponseTreated;
                                        }
                                    }
                                    else
                                    {
                                        ar.AsyncWaitHandle.WaitOne(1000);
                                    }
                                }
                            }
                        }
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

            return String.Empty;
        }

        public String GetJoystickAxisPercentage(Joystick joystick, int input, String joystickAxis, String model)
        {
            JJMixerDbConnection _JJMixerDbConnection = new JJMixerDbConnection();
            String axisOrientation = "";
            
            axisOrientation = _JJMixerDbConnection.GetInputAxisOrientation(input, model);
            
            String Value = "";
            Dictionary<String, String> inverseDictionaryValue = new Dictionary<String, String>();

            int InverseValue = 100;

            for (int i = 0; i <= 100;i++)
            {
                inverseDictionaryValue.Add(i.ToString(), InverseValue.ToString());
                InverseValue--;
            }


            try
            {
                joystick.Poll();

                var lastState = joystick.GetBufferedData(); //only show the last state

                foreach (var state in lastState)
                {
                    if (joystickAxis == "X")
                    {
                        if (state.Offset == JoystickOffset.X)
                            Value = (state.Value * 100 / 65530).ToString();
                    }
                    else if (joystickAxis == "Y")
                    {
                        if (state.Offset == JoystickOffset.Y)
                            Value = (state.Value * 100 / 65530).ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            if (axisOrientation == "inverted" && Value != "")
                return inverseDictionaryValue[Value];
            else
                return Value;
        }
    }
}