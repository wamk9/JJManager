using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using AudioSwitcher.AudioApi.Observables;
using AudioSwitcher.AudioApi.Session;
using HidSharp;
using HidSharp.Reports;
using JJMixer.Class;
using JJMixer_WForms.Pages;
using JJMixer_WForms.Pages.ButtonBox;
using MaterialSkin;
using MaterialSkin.Controls;
using SharpDX.DirectInput;
using System;
using System.Collections;
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
using System.Security.Cryptography;
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
        private static String ConnectionType;

        public static String MixerModel;
        public static String MixerInputId;
        public static String MixerSerialPort;
        public static String HIDInstanceGuid;

        public byte[] keyboardBuffer;  //EP1
        public HidSharp.Reports.Input.HidDeviceInputReceiver InputReceiver;
        public HidSharp.Reports.ReportDescriptor KeyboardRptDescriptor;
        public HidStream KeyboardStream;
        public HidDevice KeyboardDevice;


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

        static void ThreadJoystickConnection(Joystick joystick)
        {
            switch (joystick.Information.InstanceName)
            {
                case "ButtonBox JJB-01":
                    JJB_01 BBoxScreen = new JJB_01(joystick);
                    BBoxScreen.ShowDialog();
                    break;
            }
        }

        static void ThreadHIDConnection(HidDevice device)
        {
            String HIDResponseTreated = "";
            byte[] HIDResponseBytes = new byte[64];

            HidStream hidStream;
            var reportDescriptor = device.GetReportDescriptor();

            foreach (var deviceItem in reportDescriptor.DeviceItems)
            {
                if (device.TryOpen(out hidStream))
                {
                    switch (device.GetProductName())
                    {
                        case "Mixer de Áudio JJM-01":
                            JJM_01 MixerScreen = new JJM_01(device);
                            MixerScreen.ShowDialog();
                            break;
                    }
                }
                else
                {
                    MessageBox.Show("Dispositivo indisponível no momento, selecione outro dispositivo para continuar.");
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void BtnConnectMixer_Click(object sender, EventArgs e)
        {
            String HIDDeviceChoiced = CmbBoxHIDDevice.SelectedIndex != -1 ? CmbBoxHIDDevice.SelectedItem.ToString() : "";

            if (HIDDeviceChoiced == "")
                return;

            String HIDHash = HIDDeviceChoiced.Split().Where(x => x.StartsWith("(") && x.EndsWith(")"))
                                .Select(x => x.Replace("(", string.Empty).Replace(")", string.Empty))
                                .ToList().Last();

            BtnConnectMixer.Enabled = false;
            BtnConnectMixer.Text = "Conectando...";

            this.Visible = false;

            try
            {
                HidDevice device = JJMixerDevice.GetHIDDeviceByHash(HIDHash);
                Joystick joystick = JJMixerDevice.GetJoystickDeviceByHash(HIDHash);

                if (device != null)
                {
                    Thread thr = new Thread(() => ThreadHIDConnection(device)); ;
                    thr.Start();
                    thr.Join();
                    this.Visible = true;
                }
                else if (joystick != null)
                {
                    Thread thr = new Thread(() => ThreadJoystickConnection(joystick)); ;
                    thr.Start();
                    thr.Join();
                    this.Visible = true;
                }
                else
                {
                    MessageBox.Show("Dispositivo indisponível no momento, selecione outro dispositivo para continuar.");
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
            }

            BtnConnectMixer.Enabled = true;
            BtnConnectMixer.Text = "Conectar com JJMixer";
            BtnConnectMixer.Size = new Size(200, 40);
        }


        private void FindDevice()
        {
            var list = DeviceList.Local;
            list.Changed += (sender, e) => Console.WriteLine("Device list changed.");

            var hidDeviceList = list.GetHidDevices().ToArray();

            foreach (HidDevice dev in hidDeviceList)
            {
                if (dev.VendorID != 0x2341)
                    continue;

                MessageBox.Show(dev.GetProductName());

                var reportDescriptor = dev.GetReportDescriptor();

                foreach (var deviceItem in reportDescriptor.DeviceItems)
                {
                    foreach (var usage in deviceItem.Usages.GetAllValues())
                    {
                        Console.WriteLine(string.Format("Usage: {0:X4} {1}", usage, (Usage)usage));
                    }
                    foreach (var report in deviceItem.Reports)
                    {
                        Console.WriteLine(string.Format("{0}: ReportID={1}, Length={2}, Items={3}",
                                            report.ReportType, report.ReportID, report.Length, report.DataItems.Count));
                        foreach (var dataItem in report.DataItems)
                        {
                            Console.WriteLine(string.Format("  {0} Elements x {1} Bits, Units: {2}, Expected Usage Type: {3}, Flags: {4}, Usages: {5}",
                                dataItem.ElementCount, dataItem.ElementBits, dataItem.Unit.System, dataItem.ExpectedUsageType, dataItem.Flags,
                                string.Join(", ", dataItem.Usages.GetAllValues().Select(usage => usage.ToString("X4") + " " + ((Usage)usage).ToString()))));
                        }
                    }

                    HidStream hidStream;
                    if (dev.TryOpen(out hidStream))
                    {
                        MessageBox.Show("Opened device.");
                        hidStream.ReadTimeout = Timeout.Infinite;

                        byte[] input = Encoding.ASCII.GetBytes("IPT1|Teste");
                        byte[] inputTreated = new byte[(input.Length + 1)];
                        for (int i = 0; i < inputTreated.Length; i++)
                        {
                            if (i == 0)
                                inputTreated[i] = 0x00;
                            else
                                inputTreated[i] = input[(i-1)];
                        }

                        hidStream.Write(inputTreated);

                        using (hidStream)
                        {

                            var inputReportBuffer = new byte[dev.GetMaxInputReportLength()];
                            var inputReceiver = reportDescriptor.CreateHidDeviceInputReceiver();
                            var inputParser = deviceItem.CreateDeviceItemInputParser();

                            MessageBox.Show(dev.GetMaxOutputReportLength().ToString());

                            /* TESTE*/
                            IAsyncResult ar = null;

                            int startTime = Environment.TickCount;
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
                                            byte[] exampleByteArray = inputReportBuffer.Take(byteCount).Where(x => x != 0x00).ToArray(); // not sure this is OK with your requirements 
                                            string myString = System.Text.Encoding.ASCII.GetString(exampleByteArray).Trim();
                                            
                                            byte[] message = Encoding.ASCII.GetBytes("Teste").ToArray();
                                            
                                            //MessageBox.Show(string.Join(" ", message.Select(b => b.ToString("X2"))));
                                            //hidStream.Write(Encoding.ASCII.GetBytes("Teste"));

                                            //hidStream.Write(writeData, 0, writeData.Length);
                                            MessageBox.Show(myString);
                                        }
                                    }
                                    else
                                    {
                                        ar.AsyncWaitHandle.WaitOne(1000);
                                    }
                                }

                                uint elapsedTime = (uint)(Environment.TickCount - startTime);
                                if (elapsedTime >= 20000) { break; } // Stay open for 20 seconds.
                            }
                        }
                    }

                }

            }
        }

        private void CmbBoxHIDDevice_DropDown(object sender, EventArgs e)
        {
            CmbBoxHIDDevice.Items.Clear();

            foreach (String HIDDevice in JJMixerDevice.UpdateHIDDevices())
                CmbBoxHIDDevice.Items.Add(HIDDevice);
        }
    }
}
