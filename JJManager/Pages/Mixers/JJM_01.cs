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
        private static Class.Devices _device;
        private static AudioManager _audioManager = new AudioManager();
        private static DatabaseConnection _DatabaseConnection = new DatabaseConnection();
        private static Profiles _profile = null;
        private Thread thr = null;
        private Thread thrTimers = null;
        private bool _DisconnectDevice = false;
        private bool _IsInputSelected = false;
        private bool _IsCreateProfileOpened = false;
        private String DataReceived = "";
        private String[] DataTreated = null;

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

            _device = new Devices(device);

            // Fill Forms
            foreach (String Profile in Profiles.GetList(_device.Id))
                CmbBoxSelectProfile.Items.Add(Profile);

            if (CmbBoxSelectProfile.Items.Count == 0)
            {
                _profile = new Profiles("Perfil Padrão", _device.Id, 2);
            }
            else
            {
                CmbBoxSelectProfile.SelectedIndex = 0;
                _profile = new Profiles(CmbBoxSelectProfile.Items[0].ToString(), _device.Id, 2);
            }

            // Start NotifyIcon
            notifyIcon = new AppModulesNotifyIcon(components, _device.ProductName, NotifyIcon_Click);
            HidReceiver = new AppModulesTimer(components, 50, timerReceiveHIDMessage_Tick);

            //Start Timers
            /*thrTimers = new Thread(() => {
                HidReceiver = new AppModulesTimer(components, 50, timerReceiveHIDMessage_Tick);

                while (true)
                {
                    Application.DoEvents();
                }
            });
            thrTimers.Start();*/

            // Events
            FormClosing += new FormClosingEventHandler(JJM_01_FormClosing);
            FormClosed += new FormClosedEventHandler(JJM_01_FormClosed);
            CmbBoxSelectProfile.DropDown += new EventHandler(CmbBoxSelectProfile_DropDown);
            CmbBoxSelectProfile.SelectedIndexChanged += new EventHandler(CmbBoxSelectProfile_SelectedIndexChanged);

            thr = new Thread(() => ThreadSendInputNameToDeviceScreen()); ;
            thr.Start();
        }

        private void OpenInputModal(int idInput)
        {
            Thread thr = new Thread(() => {
                if (!_IsInputSelected)
                {
                    _IsInputSelected = true;
                    ChangeInputInfo inputForm = new ChangeInputInfo(_profile, idInput);
                    inputForm.ShowDialog();
                    _profile.UpdateInputs();
                    _IsInputSelected = false;
                }

                Thread.CurrentThread.Abort();
            });
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
            //thrTimers.Abort();
            thr.Abort();
            GC.Collect();
        }

        private void ThreadSendInputNameToDeviceScreen()
        {
            while (true) 
            {
                for (int i = 1; i <= 5; i++)
                {
                    _device.SendInputNameToDeviceScreen(i, _profile.Id);
                    Thread.Sleep(200);
                }
            }
        }

        private void timerReceiveHIDMessage_Tick(object sender, EventArgs e)
        {
            DataReceived = _device.ReceiveHIDMessage();

            if (DataReceived != String.Empty)
            {
                DataTreated = DataReceived.Split('|').ToArray();

                for (int i = 0; i < DataTreated.Length; i++)
                    if (_profile.Id != "")
                        _audioManager.ChangeInputVolume(_profile.GetInputById((i + 1)), Int16.Parse(DataTreated[i]));
            }
            else
            {
                _DisconnectDevice = true;
                //Thread.CurrentThread.Abort();
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

            foreach (String Profile in Profiles.GetList(_device.Id))
                CmbBoxSelectProfile.Items.Add(Profile);

            CmbBoxSelectProfile.SelectedIndex = selectedIndex;
        }

        private void CmbBoxSelectProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CmbBoxSelectProfile.SelectedItem.ToString() != "")
            {
                _profile = new Profiles(CmbBoxSelectProfile.SelectedItem.ToString(), _device.Id, 5);
            }
            else
            {
                CmbBoxSelectProfile.SelectedIndex = 0;
                _profile = new Profiles(CmbBoxSelectProfile.SelectedItem.ToString(), _device.Id, 5);
            }

        }
        #endregion

        #region Buttons
        private void BtnInput01JJM01_Click(object sender, EventArgs e)
        {
            OpenInputModal(1);
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
            OpenInputModal(2);
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
            OpenInputModal(3);
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
            OpenInputModal(4);
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
            OpenInputModal(5);
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
                    CreateProfile createProfile = new CreateProfile(_device);
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
                _DatabaseConnection.DeleteProfile(CmbBoxSelectProfile.SelectedItem.ToString(), _device.Id);

                CmbBoxSelectProfile.Items.Clear();

                foreach (String Profile in Profiles.GetList(_device.Id))
                    CmbBoxSelectProfile.Items.Add(Profile);

                CmbBoxSelectProfile.SelectedIndex = 0;

                MessageBox.Show("Perfil excluído com sucesso!");
            }
        }
        #endregion
    }
}
