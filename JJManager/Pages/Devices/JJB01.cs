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
    public partial class JJB01 : MaterialForm
    {
        private JJDeviceClass _device;
        //private static AudioManager _audioManager = new AudioManager();
        private Thread thrTimers = null;
        private bool _IsInputSelected = false;
        private bool _IsCreateProfileOpened = false;
        private MaterialForm _parent = null;
        #region WinForms
        private MaterialSkinManager materialSkinManager = null;
        private AppModulesTimer JoystickReceiver = null;
        private AppModulesNotifyIcon notifyIcon = null;
        #endregion

        public JJB01(MaterialForm parent, JJDeviceClass device)
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
            FormClosing += new FormClosingEventHandler(JJB_01_FormClosing);
            //FormClosed += new FormClosedEventHandler(JJB_01_FormClosed);
            CmbBoxSelectProfile.DropDown += new EventHandler(CmbBoxSelectProfile_DropDown);
            CmbBoxSelectProfile.SelectedIndexChanged += new EventHandler(CmbBoxSelectProfile_SelectedIndexChanged);
        }

        private void OpenInputModal(ProfileClass profile, int idInput)
        {
            Pages.App.AudioController inputForm = new Pages.App.AudioController(this, _device.Profile, idInput);
            Visible = false;
            inputForm.ShowDialog();
            //_device.ActiveProfile.UpdateAnalogInputs(idInput);
            _IsInputSelected = false;
        }



        #region Events
        private void JJB_01_FormClosing(object sender, FormClosingEventArgs e)
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

        }
        #endregion

        #region Buttons
        private void BtnInput01JJB01_Click(object sender, EventArgs e)
        {
            if (_IsInputSelected)
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
            thr.Name = "JJB01_Input_01";
            thr.Start();
        }

        private void BtnInput01JJB01_MouseEnter(object sender, EventArgs e)
        {
            if (!_IsInputSelected)
                BtnInput01JJB01.Image = JJManager.Properties.Resources.JJB_01_input01_hover;
        }

        private void BtnInput01JJB01_MouseLeave(object sender, EventArgs e)
        {
            BtnInput01JJB01.Image = JJManager.Properties.Resources.JJB_01_input01;
        }

        private void BtnInput02JJB01_Click(object sender, EventArgs e)
        {
            if (_IsInputSelected)
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
            thr.Name = "JJB01_Input_02";
            thr.Start();
        }

        private void BtnInput02JJB01_MouseEnter(object sender, EventArgs e)
        {
            if (!_IsInputSelected)
                BtnInput02JJB01.Image = JJManager.Properties.Resources.JJB_01_input02_hover;
        }

        private void BtnInput02JJB01_MouseLeave(object sender, EventArgs e)
        {
            BtnInput02JJB01.Image = JJManager.Properties.Resources.JJB_01_input02;
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
        #endregion

        private void BtnRemoveProfile_Click(object sender, EventArgs e)
        {
            if (CmbBoxSelectProfile.SelectedIndex == -1)
            {
                MessageBox.Show("Selecione um perfil para exclui-lo.");
                return;
            }

            if (CmbBoxSelectProfile.Items.Count == 1)
            {
                MessageBox.Show("Você possui apenas um perfil e este não pode ser excluído.");
                return;
            }

            DialogResult dialogResult = MessageBox.Show("Você está prestes a excluir o Perfil '" + CmbBoxSelectProfile.SelectedItem.ToString() + "', deseja continuar?", "Exclusão de Perfil", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                string profileNameToExclude = CmbBoxSelectProfile.SelectedItem.ToString();

                CmbBoxSelectProfile.Items.Remove(CmbBoxSelectProfile.SelectedItem);

                string profileNameToActive = CmbBoxSelectProfile.Items[0].ToString();
                CmbBoxSelectProfile.SelectedIndex = 0;
                
                _device.Profile.Delete(_device, profileNameToActive);

                MessageBox.Show("Perfil excluído com sucesso!");
            }
        }
    }
}
