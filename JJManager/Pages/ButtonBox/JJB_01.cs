using AudioSwitcher.AudioApi;
using HidSharp;
using JJManager.Class;
using MaterialSkin;
using MaterialSkin.Controls;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JJManager.Pages.ButtonBox
{
    public partial class JJB_01 : MaterialForm
    {
        Joystick _Joystick = null;
        private static Class.Devices _Device;
        private static Profile _Profile = new Profile();
        private static AudioManager _JJManagerAudioManager = new AudioManager();
        private static DatabaseConnection _DatabaseConnection = new DatabaseConnection();
        private Thread thrTimers = null;
        private bool _DisconnectDevice = false;
        private bool _IsInputSelected = false;
        private bool _IsCreateProfileOpened = false;

        #region WinForms
        private MaterialSkinManager materialSkinManager = null;
        private AppModulesTimer JoystickReceiver = null;
        private AppModulesNotifyIcon notifyIcon = null;
        #endregion

        private static String LastAxisXValue = "";
        private static String LastAxisYValue = "";

        public JJB_01(Joystick joystick)
        {
            InitializeComponent();
            components = new System.ComponentModel.Container();

            // MaterialDesign
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = _DatabaseConnection.GetTheme();
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);

            if (joystick == null)
                Close();

            _Joystick = joystick;

            _Device = new Class.Devices(_Joystick);

            // Start NotifyIcon
            notifyIcon = new AppModulesNotifyIcon(components, _Joystick.Properties.ProductName, NotifyIcon_Click);

            //Start Timers
            thrTimers = new Thread(() => {
                JoystickReceiver = new AppModulesTimer(components, 50, TimerReceiveJoystickMessage_Tick);

                while (true)
                {
                    Application.DoEvents();
                }
            });
            thrTimers.Start();            

            // Events
            FormClosing += new FormClosingEventHandler(JJB_01_FormClosing);
            FormClosed += new FormClosedEventHandler(JJB_01_FormClosed);
            CmbBoxSelectProfile.DropDown += new EventHandler(CmbBoxSelectProfile_DropDown);
            CmbBoxSelectProfile.SelectedIndexChanged += new EventHandler(CmbBoxSelectProfile_SelectedIndexChanged);

            // Fill Forms
            foreach (String Profile in _DatabaseConnection.GetProfiles(_Device.Id))
                CmbBoxSelectProfile.Items.Add(Profile);

            CmbBoxSelectProfile.SelectedIndex = 0;

        }

        #region Events
        private void JJB_01_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !_DisconnectDevice)
            {
                e.Cancel = true;
                notifyIcon.Show();
                Visible = false;
            }
            else
            {
                JoystickReceiver.Stop();
            }
        }

        private void JJB_01_FormClosed(object sender, FormClosedEventArgs e)
        {
            Enabled = false;
            thrTimers.Abort();
            GC.Collect();
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            notifyIcon.Hide();
            Visible = true;
            BringToFront();
        }

        private void TimerReceiveJoystickMessage_Tick(object sender, EventArgs e)
        {
            String ValueX = _Device.GetJoystickAxisPercentage(_Joystick, "1", "X", _Profile.Id);
            String ValueY = _Device.GetJoystickAxisPercentage(_Joystick, "2", "Y", _Profile.Id);

            //String ValueX = "50";
            //String ValueY = "50";

            LastAxisXValue = (ValueX != "" ? ValueX : LastAxisXValue);
            LastAxisYValue = (ValueY != "" ? ValueY : LastAxisYValue);

            GC.Collect();
            GC.WaitForPendingFinalizers();

            _JJManagerAudioManager.ChangeInputVolume(_Profile.Id, "1", (LastAxisXValue != "" ? (Int16.Parse(LastAxisXValue)) : 0));
            _JJManagerAudioManager.ChangeInputVolume(_Profile.Id, "2", (LastAxisYValue != "" ? (Int16.Parse(LastAxisYValue)) : 0));
        }

        private void CmbBoxSelectProfile_DropDown(object sender, EventArgs e)
        {
            int selectedIndex = CmbBoxSelectProfile.SelectedIndex;

            CmbBoxSelectProfile.Items.Clear();

            foreach (String Profile in _DatabaseConnection.GetProfiles(_Device.Id))
                CmbBoxSelectProfile.Items.Add(Profile);

            CmbBoxSelectProfile.SelectedIndex = selectedIndex;
        }

        private void CmbBoxSelectProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Profile.Id = _DatabaseConnection.GetProfileId(CmbBoxSelectProfile.SelectedItem.ToString(), _Device.Id);
        }
        #endregion

        #region Buttons
        private void BtnInput01JJB01_Click(object sender, EventArgs e)
        {
            Thread thr = new Thread(() => {
                if (!_IsInputSelected)
                {
                    _IsInputSelected = true;
                    ChangeInputInfo inputForm = new ChangeInputInfo(_Profile.Id, "1");
                    inputForm.ShowDialog();
                    _IsInputSelected = false;
                }

                Thread.CurrentThread.Abort();
            });
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
            Thread thr = new Thread(() => {
                if (!_IsInputSelected)
                {
                    _IsInputSelected = true;
                    ChangeInputInfo inputForm = new ChangeInputInfo(_Profile.Id, "2");
                    inputForm.ShowDialog();
                    _IsInputSelected = false;
                }

                Thread.CurrentThread.Abort();
            });
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

        private void BtnDisconnectJJB01_Click(object sender, EventArgs e)
        {
            _DisconnectDevice = true;
            Close();
        }

        private void BtnAddProfile_Click(object sender, EventArgs e)
        {
            if (!_IsCreateProfileOpened)
            {
                Thread thr = new Thread(() => {
                    _IsCreateProfileOpened = true;
                    CreateProfile createProfile = new CreateProfile(_Device);
                    createProfile.ShowDialog();
                    _IsCreateProfileOpened = false;
                    Thread.CurrentThread.Abort();
                });
                thr.Start();
            }
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
                _DatabaseConnection.DeleteProfile(CmbBoxSelectProfile.SelectedItem.ToString(), _Device.Id);

                CmbBoxSelectProfile.Items.Clear();

                foreach (String Profile in _DatabaseConnection.GetProfiles(_Device.Id))
                    CmbBoxSelectProfile.Items.Add(Profile);

                CmbBoxSelectProfile.SelectedIndex = 0;

                MessageBox.Show("Perfil excluído com sucesso!");
            }
        }
    }
}
