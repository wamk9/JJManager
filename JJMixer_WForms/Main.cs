using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using AudioSwitcher.AudioApi.Observables;
using AudioSwitcher.AudioApi.Session;
using JJMixer.Class;
using JJMixer_WForms.Pages;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JJMixer_WForms
{
    public partial class Main : MaterialForm
    {

        private static String inputNames = "";
        private static List<String> inputExec = new List<String>();

        private static CoreAudioDevice defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;
        private static CoreAudioDevice DefaultCaptureDevice = new CoreAudioController().DefaultCaptureDevice;

        private static CoreAudioDevice DefaultPlaybackCommunicationsDevice = new CoreAudioController().DefaultPlaybackCommunicationsDevice;
        private static CoreAudioDevice DefaultCaptureCommunicationsDevice = new CoreAudioController().DefaultCaptureCommunicationsDevice;

        private static List<String> SerialPortList = new List<String>();

        public static JJMixerCommunication serialPortCommunication = new JJMixerCommunication();
        public static String MixerModel;
        public static String MixerInputId;
        public static String MixerSerialPort;

        public Main()
        {
            InitializeComponent();
            timerSerialComUpdate.Start();

            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);

        }

        private void timerSerialComUpdate_Tick(object sender, EventArgs e)
        { 

                
        }


        static void ThreadSerialCom(String MixerSerialPort)
        {
            int BytesReceived = 0;
            String DataReceived = "";
            String[] DataTreated = null;

            while (serialPortCommunication.IsConnectionOpen())
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();

                BytesReceived = serialPortCommunication.IsConnectionOpen() ? serialPortCommunication.serialObject.ReadByte() : -1;

                if (BytesReceived != -1)
                {
                    DataReceived = serialPortCommunication.serialObject.ReadLine().TrimEnd('\r');
                    DataTreated = DataReceived.Split('|');

                    switch (DataTreated[0])
                    {
                        case "JJM-01":
                            serialPortCommunication.Disconnect();
                            JJM_01 MixerScreen = new JJM_01(MixerSerialPort);
                            MixerScreen.ShowDialog();
                            break;
                    }
                }
            }
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPortCommunication.IsConnectionOpen())
            {
                serialPortCommunication.Disconnect();
                timerSerialComUpdate.Stop();
            }
        }

        private void BtnConnectMixer_Click(object sender, EventArgs e)
        {
            MixerSerialPort = CmbBoxSerialCom.SelectedIndex != -1 ? CmbBoxSerialCom.SelectedItem.ToString() : "";

            if (MixerSerialPort != "")
            {   
                BtnConnectMixer.Enabled = false;
                CmbBoxSerialCom.Enabled = false;
                BtnConnectMixer.Text = "Conectando...";

                this.Visible = false;

                try
                {
                    if (serialPortCommunication.IsConnectionOpen())
                    {
                        serialPortCommunication.Disconnect();
                    }

                    serialPortCommunication.connectTo(MixerSerialPort);
                    Thread thr = new Thread(() => ThreadSerialCom(MixerSerialPort)); ;
                    thr.Start();
                    thr.Join();
                    this.Visible = true;

                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }

            BtnConnectMixer.Enabled = true;
            CmbBoxSerialCom.Enabled = true;
            BtnConnectMixer.Text = "Conectar com JJMixer";
            BtnConnectMixer.Size = new Size(200, 40);
        }

        private void CmbBoxSerialCom_DropDown(object sender, EventArgs e)
        {
            CmbBoxSerialCom.Items.Clear();

            foreach (string comPort in JJMixerDevice.UpdateCOMDevices())
            {
                CmbBoxSerialCom.Items.Add(comPort);
            }
        }
    }
}
