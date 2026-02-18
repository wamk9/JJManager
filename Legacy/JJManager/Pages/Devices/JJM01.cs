using JJManager.Class;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Threading;
using System.Windows.Forms;
using ConfigClass = JJManager.Class.App.Config.Config;
using ProfileClass = JJManager.Class.App.Profile.Profile;
using JJDeviceClass = JJManager.Class.Devices.JJDevice;

namespace JJManager.Pages.Devices
{
    public partial class JJM01 : MaterialForm
    {
        private JJDeviceClass _device;
        //private static DatabaseConnection _DatabaseConnection = new DatabaseConnection();
        //private static ProfileClass _profile = null;
        private Thread thr = null;
        private Thread thrTimers = null;
        private bool _DisconnectDevice = false;
        private static bool _IsInputSelected = false;
        private bool _IsCreateProfileOpened = false;
        private String DataReceived = "";
        private String[] DataTreated = null;
        private MaterialForm _parent = null;

        // Profile switching UI blocking
        private System.Windows.Forms.Timer _profileSwitchMonitor = null;
        private bool _isProfileSwitching = false;
        private bool _loadingProfiles = false; // Flag to prevent SelectedIndexChanged during dropdown load

        // Connection monitoring
        private bool _lastConnectionState = false;
        private System.Windows.Forms.Timer _connectionMonitorTimer = null;

        #region WinForms
        MaterialSkinManager materialSkinManager = null;
        private AppModulesTimer HidReceiver = null;
        private AppModulesNotifyIcon notifyIcon = null;
        #endregion


        public JJM01(MaterialForm parent, JJDeviceClass device)
        {
            InitializeComponent();
            components = new System.ComponentModel.Container();

            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;

            if (device == null)
            {
                _DisconnectDevice = true;
                Close();
            }

            _device = device;
            _parent = parent;

            // Fill Forms
            foreach (string profileName in ProfileClass.GetProfilesList(_device.ProductId))
            {
                CmbBoxSelectProfile.Items.Add(profileName);
            }

            if (CmbBoxSelectProfile.Items.Count == 0)
            {
                _device.Profile.CreateNewProfileIntoObject(_device, "Perfil Padrão", true);
                CmbBoxSelectProfile.Items.Add(_device.Profile.Name);
                CmbBoxSelectProfile.SelectedIndex = 0;
            }
            else
            {
                CmbBoxSelectProfile.SelectedIndex = CmbBoxSelectProfile.FindStringExact(_device.Profile.Name);
            }

            // Events
            FormClosing += new FormClosingEventHandler(JJM_01_FormClosing);

            CmbBoxSelectProfile.DropDown += new EventHandler(CmbBoxSelectProfile_DropDown);
            CmbBoxSelectProfile.SelectedIndexChanged += new EventHandler(CmbBoxSelectProfile_SelectedIndexChanged);

            // Initialize profile switch monitor timer
            _profileSwitchMonitor = new System.Windows.Forms.Timer();
            _profileSwitchMonitor.Interval = 100; // Check every 100ms
            _profileSwitchMonitor.Tick += ProfileSwitchMonitor_Tick;

            // Initialize connection monitor timer
            _lastConnectionState = _device.IsConnected;
            _connectionMonitorTimer = new System.Windows.Forms.Timer();
            _connectionMonitorTimer.Interval = 1000; // Check every 1 second
            _connectionMonitorTimer.Tick += ConnectionMonitorTimer_Tick;
            _connectionMonitorTimer.Start();

            UpdateConnectionStatus();
        }

        private void OpenInputModal(ProfileClass profile, int idInput)
        {
            Pages.App.AudioController inputForm = new Pages.App.AudioController(this, _device.Profile, idInput);
            Visible = false;
            inputForm.ShowDialog();
            //_device.ActiveProfile.UpdateAnalogInputs(idInput);
            _IsInputSelected = false;
        }

        #region Profile Switch UI Blocking

        /// <summary>
        /// Blocks all UI controls during profile switch
        /// </summary>
        private void BlockUIControls()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(BlockUIControls));
                return;
            }

            _isProfileSwitching = true;

            // Disable profile selector
            CmbBoxSelectProfile.Enabled = false;

            // Disable input buttons
            BtnInput01JJM01.Enabled = false;
            BtnInput02JJM01.Enabled = false;
            BtnInput03JJM01.Enabled = false;
            BtnInput04JJM01.Enabled = false;
            BtnInput05JJM01.Enabled = false;

            // Disable profile management buttons
            BtnAddProfile.Enabled = false;
            BtnRemoveProfile.Enabled = false;
        }

        /// <summary>
        /// Unblocks all UI controls after profile switch completes
        /// </summary>
        private void UnblockUIControls()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(UnblockUIControls));
                return;
            }

            _isProfileSwitching = false;

            // Enable profile selector
            CmbBoxSelectProfile.Enabled = true;

            // Enable input buttons
            BtnInput01JJM01.Enabled = true;
            BtnInput02JJM01.Enabled = true;
            BtnInput03JJM01.Enabled = true;
            BtnInput04JJM01.Enabled = true;
            BtnInput05JJM01.Enabled = true;

            // Enable profile management buttons
            BtnAddProfile.Enabled = true;
            BtnRemoveProfile.Enabled = true;
        }

        /// <summary>
        /// Timer event that monitors profile switch completion
        /// </summary>
        private void ProfileSwitchMonitor_Tick(object sender, EventArgs e)
        {
            try
            {
                // Check if profile switch is complete AND communication is healthy
                // Profile switch is complete when:
                // 1. NeedsUpdate becomes false (profile processing done)
                // 2. IsCommunicationHealthy is true (RequestData() responding successfully)
                if (_device != null && _device.Profile != null && !_device.Profile.NeedsUpdate)
                {
                    // Cast to JJM01 to access IsCommunicationHealthy property
                    var jjm01Device = _device as Class.Devices.JJM01;
                    if (jjm01Device != null && jjm01Device.IsCommunicationHealthy)
                    {
                        // Stop monitoring
                        _profileSwitchMonitor.Stop();

                        // Unblock UI
                        UnblockUIControls();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Insert("JJM01_UI", "Erro ao monitorar troca de perfil", ex);
                // In case of error, stop monitoring and unblock UI
                _profileSwitchMonitor.Stop();
                UnblockUIControls();
            }
        }

        #endregion

        #region Events
        private void JJM_01_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Stop and dispose timers
            if (_profileSwitchMonitor != null)
            {
                _profileSwitchMonitor.Stop();
                _profileSwitchMonitor.Dispose();
                _profileSwitchMonitor = null;
            }

            if (_connectionMonitorTimer != null)
            {
                _connectionMonitorTimer.Stop();
                _connectionMonitorTimer.Dispose();
                _connectionMonitorTimer = null;
            }

            _parent.Visible = true;
        }

        private void CmbBoxSelectProfile_DropDown(object sender, EventArgs e)
        {
            // Set flag to prevent SelectedIndexChanged from triggering during load
            _loadingProfiles = true;

            int selectedIndex = CmbBoxSelectProfile.SelectedIndex;

            CmbBoxSelectProfile.Items.Clear();

            foreach (String Profile in ProfileClass.GetProfilesList(_device.ProductId))
                CmbBoxSelectProfile.Items.Add(Profile);

            CmbBoxSelectProfile.SelectedIndex = selectedIndex;

            // Reset flag after loading is complete
            _loadingProfiles = false;
        }

        private void CmbBoxSelectProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Don't process if we're just loading the profile list
            if (_loadingProfiles)
            {
                return;
            }

            if (CmbBoxSelectProfile.SelectedIndex == -1)
            {
                CmbBoxSelectProfile.SelectedIndex = 0;
                return;
            }

            // Block UI during profile switch
            BlockUIControls();

            // Request profile change in a thread-safe manner
            // The device loop will handle the actual profile change
            var jjm01Device = _device as Class.Devices.JJM01;
            if (jjm01Device != null)
            {
                jjm01Device.RequestProfileChange(CmbBoxSelectProfile.SelectedItem.ToString());
            }
            else
            {
                // Fallback for other device types (direct assignment)
                _device.Profile = new ProfileClass(_device, CmbBoxSelectProfile.SelectedItem.ToString(), true);
            }

            // Start monitoring for profile switch completion
            _profileSwitchMonitor.Start();
        }
        #endregion

        #region Connection Monitoring

        private void ConnectionMonitorTimer_Tick(object sender, EventArgs e)
        {
            bool currentState = _device.IsConnected;

            // Check if connection state changed
            if (currentState != _lastConnectionState)
            {
                // If device disconnected and it was previously connected
                if (!currentState && _lastConnectionState)
                {
                    // Disable auto connect to prevent continuous reconnection attempts
                    // This happens when SimHub or other software takes control of the device
                    _device.AutoConnect = false;
                }

                _lastConnectionState = currentState;

                // Update button text based on connection status
                UpdateConnectionStatus();
            }
        }

        private void UpdateConnectionStatus()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateConnectionStatus));
                return;
            }

            if (_device.AutoConnect)
            {
                btnConnectDevice.Text = "Auto Conectar Habilitado";
                btnConnectDevice.Enabled = false;
            }
            else if (_device.IsConnected)
            {
                btnConnectDevice.Text = "Desconectar";
                btnConnectDevice.Enabled = true;
            }
            else
            {
                btnConnectDevice.Text = "Conectar";
                btnConnectDevice.Enabled = true;
            }
        }

        private void btnConnectDevice_Click(object sender, EventArgs e)
        {
            try
            {
                if (_device.IsConnected)
                {
                    // Disconnect
                    if (_device.Disconnect())
                    {
                        UpdateConnectionStatus();
                    }
                }
                else
                {
                    // Connect
                    if (_device.Connect())
                    {
                        UpdateConnectionStatus();
                    }
                }
            }
            catch (Exception ex)
            {
                Pages.App.MessageBox.Show(this, "Erro de Conexão", "Erro ao conectar/desconectar: " + ex.Message);
            }
        }

        #endregion

        #region Buttons
        private void BtnInput01JJM01_Click(object sender, EventArgs e)
        {
            if (_IsInputSelected || _isProfileSwitching)
                return;

            _IsInputSelected = true;

            Thread thr = new Thread(() => {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        OpenInputModal(_device.Profile, 0);
                    });
                }
                else
                {
                    OpenInputModal(_device.Profile, 0);
                }
            });
            thr.Name = "JJM01_Input_01";
            thr.Start();
        }

        private void BtnInput01JJM01_MouseEnter(object sender, EventArgs e)
        {
            if (!_IsInputSelected)
                BtnInput01JJM01.Image = JJManager.Properties.Resources.JJM_01_input_hover;
        }

        private void BtnInput01JJM01_MouseLeave(object sender, EventArgs e)
        {
            BtnInput01JJM01.Image = JJManager.Properties.Resources.JJM_01_input;
        }

        private void BtnInput02JJM01_Click(object sender, EventArgs e)
        {
            if (_IsInputSelected || _isProfileSwitching)
                return;

            _IsInputSelected = true;

            Thread thr = new Thread(() => {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        OpenInputModal(_device.Profile, 1);
                    });
                }
                else
                {
                    OpenInputModal(_device.Profile, 1);
                }
            });
            thr.Name = "JJM01_Input_02";
            thr.Start();
        }

        private void BtnInput02JJM01_MouseEnter(object sender, EventArgs e)
        {
            if (!_IsInputSelected)
                BtnInput02JJM01.Image = JJManager.Properties.Resources.JJM_01_input_hover;
        }

        private void BtnInput02JJM01_MouseLeave(object sender, EventArgs e)
        {
            BtnInput02JJM01.Image = JJManager.Properties.Resources.JJM_01_input;
        }

        private void BtnInput03JJM01_Click(object sender, EventArgs e)
        {
            if (_IsInputSelected || _isProfileSwitching)
                return;

            _IsInputSelected = true;

            Thread thr = new Thread(() => {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        OpenInputModal(_device.Profile, 2);
                    });
                }
                else
                {
                    OpenInputModal(_device.Profile, 2);
                }
            });
            thr.Name = "JJM01_Input_03";
            thr.Start();
        }

        private void BtnInput03JJM01_MouseEnter(object sender, EventArgs e)
        {
            if (!_IsInputSelected)
                BtnInput03JJM01.Image = JJManager.Properties.Resources.JJM_01_input_hover;
        }

        private void BtnInput03JJM01_MouseLeave(object sender, EventArgs e)
        {
            BtnInput03JJM01.Image = JJManager.Properties.Resources.JJM_01_input;
        }

        private void BtnInput04JJM01_Click(object sender, EventArgs e)
        {
            if (_IsInputSelected || _isProfileSwitching)
                return;

            _IsInputSelected = true;

            Thread thr = new Thread(() => {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        OpenInputModal(_device.Profile, 3);
                    });
                }
                else
                {
                    OpenInputModal(_device.Profile, 3);
                }
            });
            thr.Name = "JJM01_Input_04";
            thr.Start();
        }

        private void BtnInput04JJM01_MouseEnter(object sender, EventArgs e)
        {
            if (!_IsInputSelected)
                BtnInput04JJM01.Image = JJManager.Properties.Resources.JJM_01_input_hover;
        }

        private void BtnInput04JJM01_MouseLeave(object sender, EventArgs e)
        {
            BtnInput04JJM01.Image = JJManager.Properties.Resources.JJM_01_input;
        }

        private void BtnInput05JJM01_Click(object sender, EventArgs e)
        {
            if (_IsInputSelected || _isProfileSwitching)
                return;

            _IsInputSelected = true;

            Thread thr = new Thread(() => {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        OpenInputModal(_device.Profile, 4);
                    });
                }
                else
                {
                    OpenInputModal(_device.Profile, 4);
                }
            });
            thr.Name = "JJM01_Input_05";
            thr.Start();
        }

        private void BtnInput05JJM01_MouseEnter(object sender, EventArgs e)
        {
            if (!_IsInputSelected)
                BtnInput05JJM01.Image = JJManager.Properties.Resources.JJM_01_input_hover;
        }

        private void BtnInput05JJM01_MouseLeave(object sender, EventArgs e)
        {
            BtnInput05JJM01.Image = JJManager.Properties.Resources.JJM_01_input;
        }

        private void BtnAddProfile_Click(object sender, EventArgs e)
        {
            Thread thr = new Thread(() => {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        if (!_IsCreateProfileOpened)
                        {
                            _IsCreateProfileOpened = true;
                            CreateProfile createProfile = new CreateProfile(_device);
                            createProfile.ShowDialog();
                            _IsCreateProfileOpened = false;
                        }
                   });
                }
                else
                {
                    if (!_IsCreateProfileOpened)
                    {
                        _IsCreateProfileOpened = true;
                        CreateProfile createProfile = new CreateProfile(_device);
                        createProfile.ShowDialog();
                        _IsCreateProfileOpened = false;
                    }
                }
                Thread.CurrentThread.Abort();
            });
            thr.Name = "Add_Profile";
            thr.Start();
        }

        private void BtnRemoveProfile_Click(object sender, EventArgs e)
        {
            if (CmbBoxSelectProfile.SelectedIndex == -1)
            {
                Pages.App.MessageBox.Show(this, "Selecione um Perfil", "Selecione um perfil para excluí-lo.");
                return;
            }

            if (CmbBoxSelectProfile.Items.Count == 1)
            {
                Pages.App.MessageBox.Show(this, "Não Pode Excluir", "Você possui apenas um perfil e este não pode ser excluído.");
                return;
            }

            DialogResult dialogResult = MessageBox.Show("Você está prestes a excluir o Perfil '" + CmbBoxSelectProfile.SelectedItem.ToString() + "', deseja continuar?", "Exclusão de Perfil", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                string profileNameToExclude = CmbBoxSelectProfile.SelectedItem.ToString();

                CmbBoxSelectProfile.Items.Remove(CmbBoxSelectProfile.SelectedItem);

                string profileNameToActive = CmbBoxSelectProfile.Items[(CmbBoxSelectProfile.Items[0] == CmbBoxSelectProfile.SelectedItem ? 1 : 0)].ToString();

                _device.Profile = new ProfileClass(_device, profileNameToActive, true);

                ProfileClass.Delete(profileNameToExclude, _device.ProductId);

                CmbBoxSelectProfile.SelectedIndex = 0;

                Pages.App.MessageBox.Show(this, "Perfil Excluído", "Perfil excluído com sucesso!");

            }
        }
        #endregion

        private void btnCloseConfig_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
