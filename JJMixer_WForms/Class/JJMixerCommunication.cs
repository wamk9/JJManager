using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Windows.Forms;

namespace JJMixer.Class
{
    public class JJMixerCommunication
    {
        private SerialPort _serialPortCom = new SerialPort();

        public void JJMixerSerialReceiverEvent(SerialDataReceivedEventHandler function)
        {
            _serialPortCom.DataReceived += new SerialDataReceivedEventHandler(function);
        }

        public bool IsConnectionOpen()
        {
            return _serialPortCom.IsOpen;
        }

        public SerialPort serialObject {
            get => _serialPortCom;
        }

        public void connectTo(string serialPortName)
        {
            try
            {
                _serialPortCom.PortName = serialPortName;//Set your board COM
                _serialPortCom.BaudRate = 9600;
                _serialPortCom.RtsEnable = true;//request to send true
                _serialPortCom.DtrEnable = true;//arduino can send messages to the c# program
                                                //_serialPortCom.Encoding = Encoding.UTF8;

                _serialPortCom.Open();
            } catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("O acesso a porta COM foi negado, tente fechar os aplicativos abertos e tente novamente.");
            }
        }

        public void Disconnect()
        {
            _serialPortCom.DiscardInBuffer();
            _serialPortCom.DiscardOutBuffer();
            _serialPortCom.Close();
        }

        public void SendToDevice(string message)
        {
            if (_serialPortCom.IsOpen)
                _serialPortCom.Write("#!#JJMIXER-START#!#" + message + "#!#JJMIXER-END#!#");
        }
    }
}