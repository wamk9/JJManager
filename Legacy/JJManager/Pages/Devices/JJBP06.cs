using JJManager.Class;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Windows.Forms;
using ConfigClass = JJManager.Class.App.Config.Config;
using ProfileClass = JJManager.Class.App.Profile.Profile;
using JJDeviceClass = JJManager.Class.Devices.JJDevice;

namespace JJManager.Pages.Devices
{
    public partial class JJBP06 : MaterialForm
    {
        private JJDeviceClass _device;
        //private static AudioManager _audioManager = new AudioManager();
        private static DatabaseConnection _DatabaseConnection = new DatabaseConnection();
        private Thread thrTimers = null;
        private bool _IsInputSelected = false;
        private bool _IsCreateProfileOpened = false;
        private MaterialForm _parent = null;
        #region WinForms
        private MaterialSkinManager materialSkinManager = null;
        private AppModulesTimer JoystickReceiver = null;
        private AppModulesNotifyIcon notifyIcon = null;
        #endregion

        public JJBP06(MaterialForm parent, JJDeviceClass device)
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
            FormClosing += new FormClosingEventHandler(JJBP06_FormClosing);
            //FormClosed += new FormClosedEventHandler(JJBP06_FormClosed);
            CmbBoxSelectProfile.DropDown += new EventHandler(CmbBoxSelectProfile_DropDown);
            CmbBoxSelectProfile.SelectedIndexChanged += new EventHandler(CmbBoxSelectProfile_SelectedIndexChanged);

            ShowProfileConfigs();
        }

        private void OpenInputModal(ProfileClass profile, int idInput)
        {
            Pages.App.AudioController inputForm = new Pages.App.AudioController(this, _device.Profile, idInput);
            Visible = false;
            inputForm.ShowDialog();
            //_device.ActiveProfile.UpdateAnalogInputs(idInput);
            _IsInputSelected = false;
        }

        private void ShowProfileConfigs ()
        {
            if (_device.Profile.Data.ContainsKey("led_mode"))
            {
                switch (_device.Profile.Data["led_mode"].GetValueKind())
                {
                    case JsonValueKind.String:
                        string ledModeString = _device.Profile.Data["led_mode"].GetValue<string>();

                        if (int.TryParse(ledModeString, out int ledModeValue) && (ledModeValue < cmdBoxLedMode.Items.Count))
                        {
                            cmdBoxLedMode.SelectedIndex = ledModeValue;
                        }

                        break;
                    case JsonValueKind.Number:
                        cmdBoxLedMode.SelectedIndex = _device.Profile.Data["led_mode"].GetValue<Int16>();
                        break;
                }
            }

            cmdBoxLedMode.Refresh();

            if (_device.Profile.Data.ContainsKey("brightness"))
            {
                switch (_device.Profile.Data["brightness"].GetValueKind())
                {
                    case JsonValueKind.String:
                        string brightnessString = _device.Profile.Data["brightness"].GetValue<string>();

                        if (int.TryParse(brightnessString, out int brightnessValue) && (brightnessValue < cmdBoxLedMode.Items.Count))
                        {
                            sldLedBrightness.Value = brightnessValue;
                        }

                        break;
                    case JsonValueKind.Number:
                        sldLedBrightness.Value = _device.Profile.Data["brightness"].GetValue<Int16>();
                        break;
                }
            }

            if (cmdBoxLedMode.SelectedIndex == 0)
            {
                ImgJJBP06Off.Visible = true;
                ImgJJBP06On.Visible = false;
            }
            else
            {
                ImgJJBP06Off.Visible = false;
                ImgJJBP06On.Visible = true;
            }
        }

        #region Events
        private void JJBP06_FormClosing(object sender, FormClosingEventArgs e)
        {
            _parent.Visible = true;
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            notifyIcon.Hide();
            Visible = true;
            BringToFront();
        }

        private void TimerReceiveJoystickMessage_Tick(object sender, EventArgs e)
        {
/*            int valueX = _device.GetJoystickAxisPercentage(_device.Joystick, "X");
            int valueY = _device.GetJoystickAxisPercentage(_device.Joystick, "Y");
*/


            /*_audioManager.ChangeInputVolume(_profile.GetInputById(1), valueX);
            _audioManager.ChangeInputVolume(_profile.GetInputById(2), valueY);*/
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

            _device.Profile = new ProfileClass(_device, CmbBoxSelectProfile.SelectedItem.ToString(), true);
            ShowProfileConfigs();
        }
        #endregion

        #region Buttons
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
        #endregion

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

        private void SaveConfig(bool closeWindow = false)
        {
            string btnText = btnSaveConfig.Text;
            
            btnSaveConfig.Enabled = false;
            btnSaveAndCloseConfig.Enabled = false;
            btnSaveConfig.Text = "Salvando";

            JsonObject jsonData = new JsonObject{
                { "led_mode", cmdBoxLedMode.SelectedIndex },
                { "brightness", sldLedBrightness.Value}
            };

            _device.Profile.Update(new JsonObject { { "data", jsonData } });

            if (closeWindow)
            {
                Close();
            }
            else
            {
                btnSaveConfig.Text = btnText;
                btnSaveConfig.Enabled = true;
                btnSaveAndCloseConfig.Enabled = true;
            }
        }

        private void btnSaveConfig_Click(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void cmdBoxLedMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmdBoxLedMode.SelectedIndex == 0)
            {
                ImgJJBP06Off.Visible = true;
                ImgJJBP06On.Visible = false;
            }
            else
            {
                ImgJJBP06Off.Visible = false;
                ImgJJBP06On.Visible = true;
            }
        }

        private void sldLedBrightness_onValueChanged(object sender, int newValue)
        {

        }

        private void btnSaveAndCloseConfig_Click(object sender, EventArgs e)
        {
            SaveConfig(true);
        }
    }
}
