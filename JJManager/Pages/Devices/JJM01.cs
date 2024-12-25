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
        private void JJM_01_FormClosing(object sender, FormClosingEventArgs e)
        {
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

            _device.Profile = new ProfileClass(_device, CmbBoxSelectProfile.SelectedItem.ToString());
        }
        #endregion

        #region Buttons
        private void BtnInput01JJM01_Click(object sender, EventArgs e)
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
            if (_IsInputSelected)
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
            if (_IsInputSelected)
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
            if (_IsInputSelected)
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
        #endregion

        private void btnCloseConfig_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
