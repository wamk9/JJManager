using HidSharp;
using JJManager.Class;
using MaterialSkin;
using MaterialSkin.Controls;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JJManager.Pages
{
    public partial class JJM_01 : MaterialForm
    {
        private static Class.Devices _Device;
        private static AudioManager _JJManagerAudioManager = new AudioManager();
        private static DatabaseConnection _DatabaseConnection = new DatabaseConnection();
        private static Profile _Profile = new Profile();
        private Thread thr = null;
        private Thread thrTimers = null;
        private bool _DisconnectDevice = false;
        private bool _IsInputSelected = false;
        private bool _IsCreateProfileOpened = false;

        #region WinForms
        MaterialSkinManager materialSkinManager = null;
        private AppModulesTimer HidReceiver = null;
        private AppModulesNotifyIcon notifyIcon = null;
        #endregion


        public JJM_01(HidDevice device)
        {
            InitializeComponent();
            components = new System.ComponentModel.Container();

            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = _DatabaseConnection.GetTheme();
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);

            if (device == null)
            {
                _DisconnectDevice = true;
                Close();
            }

            _Device = new Devices(device);

            // Start NotifyIcon
            notifyIcon = new AppModulesNotifyIcon(components, _Device.ProductName, NotifyIcon_Click);

            //Start Timers
            thrTimers = new Thread(() => {
                HidReceiver = new AppModulesTimer(components, 50, timerReceiveHIDMessage_Tick);

                while (true)
                {
                    Application.DoEvents();
                }
            });
            thrTimers.Start();

            // Events
            FormClosing += new FormClosingEventHandler(JJM_01_FormClosing);
            FormClosed += new FormClosedEventHandler(JJM_01_FormClosed);
            CmbBoxSelectProfile.DropDown += new EventHandler(CmbBoxSelectProfile_DropDown);
            CmbBoxSelectProfile.SelectedIndexChanged += new EventHandler(CmbBoxSelectProfile_SelectedIndexChanged);

            // Fill Forms
            foreach (String Profile in _DatabaseConnection.GetProfiles(_Device.Id))
                CmbBoxSelectProfile.Items.Add(Profile);

            CmbBoxSelectProfile.SelectedIndex = 0;

            thr = new Thread(() => ThreadSendInputNameToDeviceScreen()); ;
            thr.Start();
        }

        #region Events
        private void JJM_01_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !_DisconnectDevice)
            {
                e.Cancel = true;
                notifyIcon.Show();
                Visible = false;
            }
            else
            {
                HidReceiver.Stop();
            }
        }

        private void JJM_01_FormClosed(object sender, FormClosedEventArgs e)
        {
            Enabled = false;
            thrTimers.Abort();
            thr.Abort();
            GC.Collect();
        }

        private void ThreadSendInputNameToDeviceScreen()
        {
            while (true) 
            {
                for (int i = 1; i <= 5; i++)
                {
                    _Device.SendInputNameToDeviceScreen(i, _Profile.Id);
                    Thread.Sleep(500);
                }
            }
        }

        private void timerReceiveHIDMessage_Tick(object sender, EventArgs e)
        {
            String DataReceived = "";
            String[] DataTreated = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            DataReceived = _Device.ReceiveHIDMessage();

            if (DataReceived != String.Empty)
            {
                DataTreated = DataReceived.Split('|').ToArray();

                for (int i = 0; i < DataTreated.Length; i++)
                    _JJManagerAudioManager.ChangeInputVolume(_Profile.Id, (i + 1).ToString(), (Int16.Parse(DataTreated[i])));
            }
            else
            {
                _DisconnectDevice = true;
                Close();
            }
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            notifyIcon.Hide();
            Visible = true;
            BringToFront();
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
        private void BtnInput01JJM01_Click(object sender, EventArgs e)
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
            Thread thr = new Thread(() => {
                if (!_IsInputSelected)
                {
                    _IsInputSelected = true;
                    ChangeInputInfo inputForm = new ChangeInputInfo(_Profile.Id, "3");
                    inputForm.ShowDialog();
                    _IsInputSelected = false;
                }

                Thread.CurrentThread.Abort();
            });
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
            Thread thr = new Thread(() => {
                if (!_IsInputSelected)
                {
                    _IsInputSelected = true;
                    ChangeInputInfo inputForm = new ChangeInputInfo(_Profile.Id, "4");
                    inputForm.ShowDialog();
                    _IsInputSelected = false;
                }

                Thread.CurrentThread.Abort();
            });
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
            Thread thr = new Thread(() => {
                if (!_IsInputSelected)
                {
                    _IsInputSelected = true;
                    ChangeInputInfo inputForm = new ChangeInputInfo(_Profile.Id, "5");
                    inputForm.ShowDialog();
                    _IsInputSelected = false;
                }
               
                Thread.CurrentThread.Abort();
            });
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

        private void BtnDisconnectJJM01_Click(object sender, EventArgs e)
        {
            _DisconnectDevice = true;
            Close();
        }

        private void BtnAddProfile_Click(object sender, EventArgs e)
        {
            Thread thr = new Thread(() => {
                if (!_IsCreateProfileOpened)
                {
                    _IsCreateProfileOpened = true;
                    CreateProfile createProfile = new CreateProfile(_Device);
                    createProfile.ShowDialog();
                    _IsCreateProfileOpened = false;
                }
                Thread.CurrentThread.Abort();
            });
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
                _DatabaseConnection.DeleteProfile(CmbBoxSelectProfile.SelectedItem.ToString(), _Device.Id);

                CmbBoxSelectProfile.Items.Clear();

                foreach (String Profile in _DatabaseConnection.GetProfiles(_Device.Id))
                    CmbBoxSelectProfile.Items.Add(Profile);

                CmbBoxSelectProfile.SelectedIndex = 0;

                MessageBox.Show("Perfil excluído com sucesso!");
            }
        }
        #endregion
    }
}
