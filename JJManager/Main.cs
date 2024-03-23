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
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Principal;

namespace JJManager
{
    public partial class Main : MaterialForm
    {
        private static List<String> SerialPortList = new List<String>();

        private static Class.Device _JJManagerComunication = null;
        private static DatabaseConnection _DatabaseConnection = new DatabaseConnection();
        private SoftwareUpdater _JJManagerUpdater = null;
        private List<JJManager.Class.Device> _DevicesList = new List<JJManager.Class.Device>();
        private bool _ExitApp = true;

        public static String MixerModel;
        public static String MixerInputId;
        public static String MixerSerialPort;
        public static String HIDInstanceGuid;
        public static int LvDevicesIndex = -1;

        private MaterialSkinManager materialSkinManager = null;
        public Thread threadUpdateDevicesList = null;
        public byte[] keyboardBuffer;  //EP1
        public HidSharp.Reports.Input.HidDeviceInputReceiver InputReceiver;
        public HidSharp.Reports.ReportDescriptor KeyboardRptDescriptor;
        public HidStream KeyboardStream;
        public HidDevice KeyboardDevice;

        //private BackgroundWorker backgroundWorker = new BackgroundWorker();

        private AppModulesNotifyIcon notifyIcon = null;
        private AppModulesTimer fillListTimer = null;
        private Thread ThrListDevices = null;


        public Main()
        {
            InitializeComponent();
            timerSerialComUpdate.Start();


            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = _DatabaseConnection.GetTheme();
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
            DisableAllForms();

            Version actualVersion = Assembly.GetEntryAssembly().GetName().Version;
            lblAboutVersion.Text = "JJManager Versão " + actualVersion.Major.ToString() + "." + actualVersion.Minor.ToString() + "." + actualVersion.Build.ToString();
            lblAboutText.Text = "Criado com o propósito de servir como um gerenciador para os produtos da série JJM, JJSD e JJB, o JJManager é uma solução completa que trás aos usuários diversas funções em seus produtos nos quais não são disponíveis de forma autônoma.";

            Migrate migrate = new Migrate();

            lvDevices.FullRowSelect = true;

            // Events
            FormClosing += new FormClosingEventHandler(Main_FormClosing);
            FormClosed += new FormClosedEventHandler(Main_FormClosed);

            // Start NotifyIcon
            notifyIcon = new AppModulesNotifyIcon(components, "Você continua com dispositivos conectados ao JJManager, para encerrar o programa você deve desconectar todos os dispositivos.", NotifyIcon_Click);
            fillListTimer = new AppModulesTimer(components, 2000, FillListTimer_tick);

            Shown += Main_Shown;
        }

        public bool IsUserAdministrator()
        {
            bool isAdmin;
            try
            {
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException ex)
            {
                isAdmin = false;
            }
            catch (Exception ex)
            {
                isAdmin = false;
            }
            return isAdmin;
        }

        public void GoToUpdateTab()
        {
            materialTabControl1.SelectedTab = materialTabControl1.TabPages[2];
        }

        private void updateList()
        {
            _DevicesList = JJManager.Class.Device.getDevicesList(_DevicesList);
            bool updateList = false;


            if (lvDevices.Items.Count == 0 && _DevicesList.Count > 0)
            {
                updateList = true;
            }
            else if (lvDevices.Items.Count == _DevicesList.Count)
            {
                foreach (ListViewItem actualDeviceInList in lvDevices.Items)
                {
                    if (!_DevicesList.Exists(newDevice => newDevice.ConnId == actualDeviceInList.SubItems[0].Text && newDevice.IsConnected == (actualDeviceInList.SubItems[0].Text == "Conectado" ? true : false)))
                    {
                        updateList = true;
                    }
                }
            }

            if (updateList)
            {
                int selectedIndices = lvDevices.SelectedIndices.Count > 0 ? lvDevices.SelectedIndices[0] : -1;

                lvDevices.Items.Clear();

                ListViewItem item = new ListViewItem(); // Create a new ListViewItem for each device

                _DevicesList.ForEach(device =>
                {
                    lvDevices.Items.Add(new ListViewItem(new string[]
                    {
                    device.ConnId,
                    device.ProductName,
                    device.ConnType,
                    device.IsConnected ? "Conectado" : "Desconectado"
                    }));
                });

                if (selectedIndices >= 0 && lvDevices.Items.Count > 1)
                {
                    lvDevices.Items[selectedIndices].Selected = true;
                }
            }
        }

        private void InitThrListDevices()
        {
            ThrListDevices = new Thread(() => {
                while(true)
                {
                    if (Visible)
                    {
                        if (InvokeRequired)
                        {
                            BeginInvoke((MethodInvoker)delegate
                            {
                                updateList();
                            });
                        }
                        else
                        {
                            updateList();
                        }
                    }

                    Thread.Sleep(2000);
                }
            });
            ThrListDevices.SetApartmentState(ApartmentState.STA);
            ThrListDevices.Name = "Init_List_Devices";
            //ThrListDevices.IsBackground = true;
        }

        private void DisableAllForms()
        {
            // Disable Tabs
            tabConnect.Enabled = false;
            tabOptions.Enabled = false;
            tabUpdate.Enabled = false;
            tabAbout.Enabled = false;

            // Disable Connection Tab Forms
            lvDevices.Enabled = false;
            btnAddDevice.Enabled = false;
            btnConnChanger.Enabled = false;
            btnEditDevice.Enabled = false;

            // Disable Options Tab
            SwtThemeColor.Enabled = false;

            // Disable Update Tab
            BtnUpdateSoftware.Enabled = false;
            BtnUpdateDevice.Enabled = false;
        }

        private void EnableAllForms()
        {
            // Disable Tabs
            tabConnect.Enabled = true;
            tabOptions.Enabled = true;
            tabUpdate.Enabled = true;
            tabAbout.Enabled = true;

            // Disable Connection Tab Forms
            lvDevices.Enabled = true;
            //btnAddDevice.Enabled = true; // Future release feature (or not)
            btnConnChanger.Enabled = lvDevices.SelectedIndices.Count > 0 ? true : false;
            btnEditDevice.Enabled = lvDevices.SelectedIndices.Count > 0 ? true : false; ;

            // Disable Options Tab
            SwtThemeColor.Enabled = true;

            // Disable Update Tab
            BtnUpdateSoftware.Enabled = true;
            //BtnUpdateDevice.Enabled = true; // Future release feature
        }
        private void FillListTimer_tick(object sender, EventArgs e)
        {
            if (ThrListDevices == null || ThrListDevices.ThreadState == System.Threading.ThreadState.Stopped)
            {
                InitThrListDevices();
            }

            if (ThrListDevices.ThreadState == System.Threading.ThreadState.Unstarted)
            {
                ThrListDevices.Start();
            }
        }

        private void timerSerialComUpdate_Tick(object sender, EventArgs e)
        {

            /*
            _DevicesList = JJManager.Class.Device.getDevicesList(_DevicesList);

            if (_DevicesList.Count == 0)
                return;

            int selectedIndices = lvDevices.SelectedIndices.Count > 0 ? lvDevices.SelectedIndices[0] : -1;

            lvDevices.Items.Clear();

            ListViewItem item = new ListViewItem(); // Create a new ListViewItem for each device

            _DevicesList.ForEach(device =>
            {
                item.SubItems.Add(device.ConnId);
                item.SubItems.Add(device.ProductName);
                item.SubItems.Add(device.ConnType);
                item.SubItems.Add(device.IsConnected ? "Conectado" : "Desconectado");
                lvDevices.Items.Add(item);
                item.SubItems.Clear();
            });

            if (selectedIndices >= 0 && lvDevices.Items.Count > 1)
            {
                lvDevices.Items[selectedIndices].Selected = true;
            }*/
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            notifyIcon.Hide();
            Visible = true;
            BringToFront();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && _DevicesList.Exists(device => device.IsConnected))
            {
                e.Cancel = true;
                notifyIcon.Show();
                Visible = false;
            }
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
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

        private void Main_Shown(object sender, EventArgs e)
        {
            SwtThemeColor.Checked = _DatabaseConnection.GetTheme() == MaterialSkinManager.Themes.DARK ? true : false;

            _JJManagerUpdater = new SoftwareUpdater();

            if (_JJManagerUpdater.NeedToUpdate)
            {
                BtnUpdateSoftware.Enabled = true;
                BtnUpdateSoftware.Text = "Versão " + _JJManagerUpdater.LastVersion + " Disponível";

                Visible = false;
                
                _JJManagerUpdater.ShowNotificationForm(this);
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

            updateList();

            EnableAllForms();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            
        }

        private void BtnUpdateSoftware_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Após baixado o instalador iremos encerrar o JJManager e a janela de atualização iniciará, deseja continuar?", "Iniciar Atualização do JJManager", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                BtnUpdateSoftware.Enabled = false;
                BtnUpdateSoftware.Text = "Baixando Instalador, Aguarde...";
                _JJManagerUpdater.Update();
            }
        }

        private void BtnUpdateDevice_Click(object sender, EventArgs e)
        {
            Thread thr = new Thread(() => {
                UpdateDevice updateForm = new UpdateDevice();
                updateForm.ShowDialog();
            });
            thr.Name = "Update_Device";
            thr.SetApartmentState(ApartmentState.STA);
            thr.IsBackground = true;
            thr.Start();

        }

        private bool isAllDevicesDisconnected()
        {
            foreach (JJManager.Class.Device device in _DevicesList)
            {
                if (device.IsConnected)
                {
                    return false;
                }
            }

            return true;
        }

        private void lvDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            LvDevicesIndex = lvDevices.SelectedIndices.Count > 0 ? lvDevices.SelectedIndices[0] : -1;

            if (LvDevicesIndex == -1)
            {
                btnConnChanger.Enabled = false;
                btnEditDevice.Enabled = false;
                btnConnChanger.Text = "Conectar/Desconectar";
                return;
            }

            int listIndexOfDevice = _DevicesList.FindIndex(device => device.ConnId == lvDevices.Items[LvDevicesIndex].SubItems[0].Text);

            if (listIndexOfDevice == -1)
            {
                return;
            }

            btnConnChanger.Enabled = false;

            btnConnChanger.Text = _DevicesList[listIndexOfDevice].IsConnected ? "Desconectar" : "Conectar";
            btnConnChanger.Enabled = true;
            btnEditDevice.Enabled = true;
        }

        private void btnConnChanger_Click(object sender, EventArgs e)
        {
            if (LvDevicesIndex == -1)
            {
                return;
            }

            int listIndexOfDevice = _DevicesList.FindIndex(device => device.ConnId == lvDevices.Items[LvDevicesIndex].SubItems[0].Text);

            if (listIndexOfDevice == -1)
            {
                return;
            }

            btnConnChanger.Enabled = false;
            
            while (true)
            {
                if (_DevicesList[listIndexOfDevice].IsConnected) 
                {
                    btnConnChanger.Text = "Desconectando...";

                    if (_DevicesList[listIndexOfDevice].Disconnect())
                    {
                        lvDevices.Items[LvDevicesIndex].SubItems[3].Text = "Desconectado";
                        btnConnChanger.Text = "Conectar";
                        break;
                    }
                }
                else
                {
                    btnConnChanger.Text = "Conectando...";

                    if (_DevicesList[listIndexOfDevice].Connect())
                    {
                        lvDevices.Items[LvDevicesIndex].SubItems[3].Text = "Conectado";
                        btnConnChanger.Text = "Desconectar";
                        break;
                    }
                }
            }

            btnConnChanger.Enabled = true;
        }

        private void btnEditDevice_Click(object sender, EventArgs e)
        {
            if (LvDevicesIndex == -1)
            {
                return;
            }

            int listIndexOfDevice = _DevicesList.FindIndex(device => device.ConnId == lvDevices.Items[LvDevicesIndex].SubItems[0].Text);

            if (listIndexOfDevice == -1)
            {
                return;
            }
            
            _DevicesList[listIndexOfDevice].OpenEditWindow(this);
        }
    }
}
