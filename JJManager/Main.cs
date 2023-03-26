using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using AudioSwitcher.AudioApi.Observables;
using AudioSwitcher.AudioApi.Session;
using HidSharp;
using HidSharp.Reports;
using JJManager.Class;
using JJManager.Pages;
using JJManager.Pages.ButtonBox;
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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JJManager
{
    public partial class Main : MaterialForm
    {
        private static List<String> SerialPortList = new List<String>();

        private static Class.Devices _JJManagerComunication = null;
        private static DatabaseConnection _DatabaseConnection = new DatabaseConnection();
        private SoftwareUpdater _JJManagerUpdater = null;
        private bool _ExitApp = true;

        public static String MixerModel;
        public static String MixerInputId;
        public static String MixerSerialPort;
        public static String HIDInstanceGuid;

        private MaterialSkinManager materialSkinManager = null;

        public byte[] keyboardBuffer;  //EP1
        public HidSharp.Reports.Input.HidDeviceInputReceiver InputReceiver;
        public HidSharp.Reports.ReportDescriptor KeyboardRptDescriptor;
        public HidStream KeyboardStream;
        public HidDevice KeyboardDevice;

        public Main()
        {
            InitializeComponent();
            timerSerialComUpdate.Start();


            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = _DatabaseConnection.GetTheme();
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);

            Version actualVersion = Assembly.GetEntryAssembly().GetName().Version;
            lblAboutVersion.Text = "JJManager Versão " + actualVersion.Major.ToString() + "." + actualVersion.Minor.ToString() + "." + actualVersion.Build.ToString();
            lblAboutText.Text = "Criado com o propósito de servir como um gerenciador para os produtos da série JJM, JJSD e JJB, o JJManager é uma solução completa que trás aos usuários diversas funções em seus produtos nos quais não são disponíveis de forma autônoma.";
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
                    BBoxScreen.Show();
                    BBoxScreen.Activate();

                    while (BBoxScreen.Enabled)
                        Application.DoEvents();

                    break;
                default:
                    MessageBox.Show("Este dispositivo ainda não é compatível com o JJManager, aguarde pelas próximas versões do software para tentar novamente.");
                    break;

            }

            Main main = new Main();
            main.Show();
            main.Activate();

            while (main.Enabled)
                Application.DoEvents();
        }

        static void ThreadHIDConnection(HidDevice device)
        {
            byte[] HIDResponseBytes = new byte[64];

            _JJManagerComunication = new Class.Devices(device);

            switch (_JJManagerComunication.ProductName)
            {
                case "Mixer de Áudio JJM-01":
                    JJM_01 MixerScreen = new JJM_01(device);
                    MixerScreen.Show();
                    MixerScreen.Activate();

                    while (MixerScreen.Enabled)
                        Application.DoEvents();
                    break;
                default:
                    MessageBox.Show("Este dispositivo ainda não é compatível com o JJManager, aguarde pelas próximas versões do software para tentar novamente.");
                    break;
            }

            Main main = new Main();
            main.Show();
            main.Activate();
            while (main.Enabled)
                Application.DoEvents();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && _ExitApp)
                Environment.Exit(0);
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
            _ExitApp = false;

            try
            {
                HidDevice device = JJManagerDevice.GetHIDDeviceByHash(HIDHash);
                Joystick joystick = JJManagerDevice.GetJoystickDeviceByHash(HIDHash);

                if (device != null)
                {
                    Thread thr = new Thread(() => ThreadHIDConnection(device));
                    thr.Start();
                }
                else if (joystick != null)
                {
                    Thread thr = new Thread(() => ThreadJoystickConnection(joystick)); ;
                    thr.Start();
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

            Close();
        }

        private void CmbBoxHIDDevice_DropDown(object sender, EventArgs e)
        {
            CmbBoxHIDDevice.Items.Clear();

            foreach (String HIDDevice in JJManagerDevice.UpdateHIDDevices())
                CmbBoxHIDDevice.Items.Add(HIDDevice);
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            GC.Collect();
            Application.ExitThread();
        }

        private void SwtThemeColor_CheckedChanged(object sender, EventArgs e)
        {
            if (SwtThemeColor.Checked)
            {
                SwtThemeColor.Text = "Escuro";
                materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            }
            else
            {
                SwtThemeColor.Text = "Claro";
                materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            }

            _DatabaseConnection.SaveTheme((SwtThemeColor.Checked ? "dark" : "light"));
        }

        private void Main_Load(object sender, EventArgs e)
        {
            SwtThemeColor.Checked = _DatabaseConnection.GetTheme() == MaterialSkinManager.Themes.DARK ? true : false;


            _JJManagerUpdater = new SoftwareUpdater();

            if (_JJManagerUpdater.NeedToUpdate)
            {
                BtnUpdateSoftware.Enabled = true;
                BtnUpdateSoftware.Text = "Versão " + _JJManagerUpdater.LastVersion + " Disponível";
            }
            else
            {
                BtnUpdateSoftware.Enabled = false;
                BtnUpdateSoftware.Text = "Versão " + _JJManagerUpdater.LastVersion + " Instalada";
            }

            if (Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "downloads")))
            {
                DirectoryInfo dir = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "downloads"));
                dir.Delete(true);
            }
        }

        private void BtnUpdateSoftware_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Após baixado o instalador iremos encerrar o JJManager e a janela de atualização iniciará, deseja continuar?", "Iniciar Atualização do JJManager", MessageBoxButtons.OKCancel);

            if (dialogResult == DialogResult.OK)
            {
                BtnUpdateSoftware.Enabled = false;
                BtnUpdateSoftware.Text = "Baixando Instalador, Aguarde...";
                _JJManagerUpdater.Update();
            }
        }
    }
}
