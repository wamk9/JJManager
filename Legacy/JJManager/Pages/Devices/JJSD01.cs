using JJManager.Class;
using JJManager.Class.App.Input;
using JJManager.Pages.App;
using JJManager.Properties;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using ConfigClass = JJManager.Class.App.Config.Config;
using ProfileClass = JJManager.Class.App.Profile.Profile;
using JJDeviceClass = JJManager.Class.Devices.JJDevice;

namespace JJManager.Pages.Devices
{
    public partial class JJSD01 : MaterialForm
    {
        private JJDeviceClass _device;
        //private static AudioManager _audioManager = new AudioManager();
        private static DatabaseConnection _DatabaseConnection = new DatabaseConnection();
        private Thread thrTimers = null;
        private bool _isInputSelected = false;
        private bool _IsCreateProfileOpened = false;
        private int _lastInputSelected = -1;
        private MaterialForm _parent = null;
        private bool _lastConnectionState = false;
        private System.Windows.Forms.Timer _connectionMonitorTimer = null;
        private readonly object cmbFunctionItems = new[]
        {
            new { Text = "Sem função", Value = Input.InputMode.None },
            new { Text = "MacroKey", Value = Input.InputMode.MacroKey },
            new { Text = "AudioPlayer", Value = Input.InputMode.AudioPlayer }
        };
        private int inputValueToAdd = 0; // Necessário para identificar qual valor adicionar ao input ao clicar nos botões de ação.


        #region WinForms
        private MaterialSkinManager materialSkinManager = null;
        private AppModulesTimer JoystickReceiver = null;
        private AppModulesNotifyIcon notifyIcon = null;
        #endregion

        
        public JJSD01(MaterialForm parent, JJDeviceClass device)
        {
            InitializeComponent();
            components = new System.ComponentModel.Container();

            // MaterialDesign
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;

            _device = device;

            _parent = parent;

            // Fill Forms
            foreach (String Profile in ProfileClass.GetProfilesList(_device.ProductId))
                CmbBoxSelectProfile.Items.Add(Profile);

            if (CmbBoxSelectProfile.Items.Count == 0)
            {
                CmbBoxSelectProfile.Items.Add(new ProfileClass(_device, "Perfil Padrão", true));
                CmbBoxSelectProfile.SelectedIndex = 0;
            }
            else
            {
                CmbBoxSelectProfile.SelectedIndex = CmbBoxSelectProfile.FindStringExact(_device.Profile.Name); ;
            }

            // Events
            foreach (Control control in Controls)
            {
                if (control is PictureBox && control.Name.StartsWith("ImgJJSD01Input"))
                {
                    ((PictureBox)control).Click += ImgJJSD01Input_Click;
                    ((PictureBox)control).MouseEnter += ImgJJSD01Input_MouseEnter;
                    ((PictureBox)control).MouseLeave += ImgJJSD01Input_MouseLeave;
                }
            }

            BtnAddProfile.Click += BtnAddProfile_Click;
            BtnRemoveProfile.Click += BtnRemoveProfile_Click;
            FormClosing += new FormClosingEventHandler(JJSD01_FormClosing);
            //FormClosed += new FormClosedEventHandler(JJB01_V2_FormClosed);

            CmbFunction.DataSource = cmbFunctionItems;
            CmbFunction.DisplayMember = "Text";
            CmbFunction.ValueMember = "Value";
            CmbFunction.SelectedValue = Input.InputType.None;
            CmbFunction.SelectedIndexChanged += (s, e) =>
            {
                if (_lastInputSelected < 0)
                    return;

                int inputSelected = _lastInputSelected + inputValueToAdd;
                Input.InputMode selectedMode = (Input.InputMode)CmbFunction.SelectedValue;

                if (_device.Profile.Inputs[inputSelected].Mode != selectedMode)
                {
                    if (selectedMode == Input.InputMode.None)
                    {
                        BtnEditFunction.Enabled = false;
                    }
                    else
                    {
                        BtnEditFunction.Enabled = true;
                    }

                    switch (selectedMode)
                    {
                        case Input.InputMode.None:
                            _device.Profile.Inputs[inputSelected].RemoveFunction();
                            break;
                        case Input.InputMode.MacroKey:
                            _device.Profile.Inputs[inputSelected].SetToMacroKey();
                            break;
                        case Input.InputMode.AudioPlayer:
                            _device.Profile.Inputs[inputSelected].SetToAudioPlayer();
                            break;
                    }
                    _device.Profile.Inputs[inputSelected].Save();
                }
            };
            CmbFunction.Refresh();
            
            if ((CmbFunction?.SelectedValue ?? Input.InputMode.None).Equals(Input.InputMode.None))
            {
                BtnEditFunction.Enabled = false;
            }
            else
            {
                BtnEditFunction.Enabled = true;
            }

            BtnEditFunction.Click += (s, e) =>
            {
                if (_lastInputSelected < 0)
                    return;

                var selectedMode = (Input.InputMode)CmbFunction.SelectedValue;
                if (selectedMode == Input.InputMode.MacroKey)
                {
                    MacroKeyMain macroKey = new MacroKeyMain(this, _device.Profile, (uint)(_lastInputSelected + inputValueToAdd));
                    Visible = false;
                    macroKey.ShowDialog();
                }
                else if (selectedMode == Input.InputMode.AudioPlayer)
                {
                    AudioPlayer audioPlayer = new AudioPlayer(this, _device.Profile, (_lastInputSelected + inputValueToAdd));
                    Visible = false;
                    audioPlayer.ShowDialog();
                }
            };

            BtnPress.Tag = (int)Class.Devices.JJSD01.InputState.Press;
            BtnContinuous.Tag = (int)Class.Devices.JJSD01.InputState.Continuous;
            BtnHold.Tag = (int)Class.Devices.JJSD01.InputState.Hold;

            BtnPress.Click += BtnFunctions_Click;
            BtnContinuous.Click += BtnFunctions_Click;
            BtnHold.Click += BtnFunctions_Click;
            
            SelectInput();

            CmbBoxSelectProfile.DropDown += new EventHandler(CmbBoxSelectProfile_DropDown);
            CmbBoxSelectProfile.SelectedIndexChanged += new EventHandler(CmbBoxSelectProfile_SelectedIndexChanged);

            // Initialize connection monitor timer
            _lastConnectionState = _device.IsConnected;
            _connectionMonitorTimer = new System.Windows.Forms.Timer();
            _connectionMonitorTimer.Interval = 500;
            _connectionMonitorTimer.Tick += ConnectionMonitorTimer_Tick;
            _connectionMonitorTimer.Start();

            // Update initial connection status
            UpdateConnectionStatus();
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

            DialogResult dialogResult = Pages.App.MessageBox.Show(this, "Exclusão de Perfil", "Você está prestes a excluir o Perfil '" + CmbBoxSelectProfile.SelectedItem.ToString() + "', deseja continuar?", MessageBoxButtons.YesNo);

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

        private void ChangeInputVisibility(bool visibility = false)
        {
            BtnPress.Enabled = visibility;
            BtnContinuous.Enabled = visibility;
            BtnHold.Enabled = visibility;

            FlpFunction.Visible = visibility;
        }

        private void SelectInput()
        {
            if (_lastInputSelected < 0)
            {
                ChangeInputVisibility();

                foreach (Control control in FlpPressMode.Controls)
                {
                    if (control is MaterialButton btn && btn.Type == MaterialButton.MaterialButtonType.Contained)
                    {
                        btn.Type = MaterialButton.MaterialButtonType.Outlined;
                    }
                }
            }
            else
            {
                ChangeInputVisibility(true);

                bool hasAnyPressModeSelected = false;

                foreach (Control control in FlpPressMode.Controls)
                {
                    if (control is MaterialButton btn && btn.Type == MaterialButton.MaterialButtonType.Contained)
                    {
                        hasAnyPressModeSelected = true;
                        break;
                    }
                }

                if (!hasAnyPressModeSelected)
                {
                    BtnPress.PerformClick();
                }
                CmbFunction.SelectedValue = _device.Profile.Inputs[_lastInputSelected + inputValueToAdd]?.Mode ?? Input.InputMode.None;
                CmbFunction.Refresh();

                if ((CmbFunction?.SelectedValue ?? Input.InputMode.None).Equals(Input.InputMode.None))
                {
                    BtnEditFunction.Enabled = false;
                }
                else
                {
                    BtnEditFunction.Enabled = true;
                }

            }
        }

        private void Mode_CheckedChanged(object sender, EventArgs e)
        {
            MaterialRadioButton radioButton = (MaterialRadioButton) sender;
            string radioButtonMode = radioButton.Name.Replace("rdb", "").Replace("Mode", "").ToLower();

            flpInput.Controls["btnEditInput"].Enabled = (radioButtonMode != "none");

            if (radioButton.Checked && radioButtonMode == "none" && _device.Profile.Inputs[_lastInputSelected].Mode != Input.InputMode.None)
            {
                _device.Profile.Inputs[_lastInputSelected].RemoveFunction();
                _device.Profile.Inputs[_lastInputSelected].Save();
            }
            else if (radioButton.Checked && radioButtonMode == "macrokey" && _device.Profile.Inputs[_lastInputSelected].Mode != Input.InputMode.MacroKey)
            {
                _device.Profile.Inputs[_lastInputSelected].SetToMacroKey();
                _device.Profile.Inputs[_lastInputSelected].Save();
            }
            else if (radioButton.Checked && radioButtonMode == "audioplayer" && _device.Profile.Inputs[_lastInputSelected].Mode != Input.InputMode.AudioPlayer)
            {
                _device.Profile.Inputs[_lastInputSelected].SetToAudioPlayer();
                _device.Profile.Inputs[_lastInputSelected].Save();
            }
        }

        private void EditInput_Click(object sender, EventArgs e)
        {
            if (((MaterialRadioButton) flpInput.Controls["rdbMacroKeyMode"]).Checked)
            {
                MacroKeyMain macroKey = new MacroKeyMain(this, _device.Profile, (uint) _lastInputSelected);
                Visible = false;
                macroKey.ShowDialog();
            }
            else if (((MaterialRadioButton)flpInput.Controls["rdbAudioPlayerMode"]).Checked)
            {
                AudioPlayer audioPlayer = new AudioPlayer(this, _device.Profile, _lastInputSelected);
                Visible = false;
                audioPlayer.ShowDialog();
            }
        }

        private void JJSD01_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Stop and dispose the connection monitor timer
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
            int selectedIndex = CmbBoxSelectProfile.SelectedIndex;

            CmbBoxSelectProfile.Items.Clear();

            foreach (String Profile in ProfileClass.GetProfilesList(_device.ProductId))
                CmbBoxSelectProfile.Items.Add(Profile);

            CmbBoxSelectProfile.SelectedIndex = selectedIndex;
        }

        private void CmbBoxSelectProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CmbBoxSelectProfile.SelectedIndex == -1)
            {
                CmbBoxSelectProfile.SelectedIndex = 0;
            }

            _device.Profile = new ProfileClass (_device, CmbBoxSelectProfile.SelectedItem.ToString(), true);
            //ShowProfileConfigs();
        }

        private void ImgJJSD01Input_Click(object sender, EventArgs e)
        {
            if (_isInputSelected)
                return;

            if (_lastInputSelected > -1)
            {
                foreach (Control control in Controls)
                {
                    if (control is PictureBox picture && control.Name.StartsWith("ImgJJSD01Input") && int.Parse(picture.Tag.ToString()) == _lastInputSelected)
                    {
                        picture.Image = Resources.JJSD01_input;
                    }
                }
            }

            int newInputSelected = int.Parse((sender as PictureBox).Tag.ToString());

            if (_lastInputSelected != newInputSelected)
            {
                _lastInputSelected = newInputSelected;
            }
            else
            {
                _lastInputSelected = -1;
            }

            SelectInput();



            _isInputSelected = true;

            /*Thread thr = new Thread(() => {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        OpenInputModal(id);
                    });
                }
                else
                {
                    OpenInputModal(id);
                }
            });
            thr.Name = "JJSD01_Input_" + id;
            thr.Start();*/
            _isInputSelected = false;

        }

        private void ImgJJSD01Input_MouseEnter(object sender, EventArgs e)
        {
            ((PictureBox)sender).Image = Resources.JJSD01_input_hover;
        }

        private void ImgJJSD01Input_MouseLeave(object sender, EventArgs e)
        {
            if (_lastInputSelected != int.Parse((sender as PictureBox).Tag.ToString()))
            ((PictureBox)sender).Image = Resources.JJSD01_input;
        }

        private void OpenInputModal(uint idInput)
        {
            MacroKeyMain macroKey = new MacroKeyMain(this, _device.Profile, idInput);
            Visible = false;
            macroKey.ShowDialog();
            
            //_device.ActiveProfile.UpdateAnalogInputs(idInput);
            
            _isInputSelected = false;
        }

        private void btnCloseConfig_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void flpInput_SizeChanged(object sender, EventArgs e)
        {
            foreach (Control ctrl in flpInput.Controls)
            {
                ctrl.Width = flpInput.ClientSize.Width;
            }
        }

        private void BtnFunctions_Click(object sender, EventArgs e)
        {
            foreach (Control control in FlpPressMode.Controls)
            {
                if (control is MaterialButton)
                {
                    ((MaterialButton)control).Type = MaterialButton.MaterialButtonType.Outlined;
                    ((MaterialButton)control).Refresh();
                }
            }

            if (_lastInputSelected < 0)
            {
                inputValueToAdd = 0;
                return;
            }

            ((MaterialButton)sender).Type = MaterialButton.MaterialButtonType.Contained;
            ((MaterialButton)sender).Refresh();
            inputValueToAdd = Class.Devices.JJSD01.ButtonCount * (int) ((MaterialButton) sender).Tag;
            CmbFunction.SelectedValue = _device.Profile.Inputs[_lastInputSelected + inputValueToAdd]?.Mode ?? Input.InputMode.None;
            CmbFunction.Refresh();

            if ((CmbFunction?.SelectedValue ?? Input.InputMode.None).Equals(Input.InputMode.None))
            {
                BtnEditFunction.Enabled = false;
            }
            else
            {
                BtnEditFunction.Enabled = true;
            }
        }

        private void BtnConnect_Click(object sender, EventArgs e)
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

        private void UpdateConnectionStatus()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateConnectionStatus));
                return;
            }

            if (_device.AutoConnect)
            {
                BtnConnect.Text = "Auto Conectar Habilitado";
                BtnConnect.Enabled = false;
            }
            else if (_device.IsConnected)
            {
                BtnConnect.Text = "Desconectar";
                BtnConnect.Enabled = true;
            }
            else
            {
                BtnConnect.Text = "Conectar";
                BtnConnect.Enabled = true;
            }
        }

        private void ConnectionMonitorTimer_Tick(object sender, EventArgs e)
        {
            bool currentState = _device.IsConnected;

            // Check if connection state changed
            if (currentState != _lastConnectionState)
            {
                _lastConnectionState = currentState;
                UpdateConnectionStatus();
            }
        }
    }
}
