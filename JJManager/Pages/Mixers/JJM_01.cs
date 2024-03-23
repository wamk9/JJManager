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

namespace JJManager.Pages.Mixers
{
    public partial class JJM_01 : MaterialForm
    {
        private static JJManager.Class.Device _device;
        private static DatabaseConnection _DatabaseConnection = new DatabaseConnection();
        private static Profile _profile = null;
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


        public JJM_01(MaterialForm parent, Class.Device device)
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

            _device = device;
            _parent = parent;

            // Fill Forms
            foreach (String Profile in Profile.GetList(_device.Id))
                CmbBoxSelectProfile.Items.Add(Profile);

            if (CmbBoxSelectProfile.Items.Count == 0)
            {
                _profile = new Profile("Perfil Padrão", _device.JJID, 5);
                CmbBoxSelectProfile.Items.Add(_profile.Name);
                CmbBoxSelectProfile.SelectedIndex = 0;
            }
            else
            {
                CmbBoxSelectProfile.SelectedIndex = CmbBoxSelectProfile.FindStringExact(_device.ActiveProfile.Name);;
                //_profile = new Profile(CmbBoxSelectProfile.Items[0].ToString(), _device.Id, 5);
            }

            // Start NotifyIcon
            //notifyIcon = new AppModulesNotifyIcon(components, NotifyIcon_Click);
            //HidReceiver = new AppModulesTimer(components, 50, timerReceiveHIDMessage_Tick);

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
            //FormClosed += new FormClosedEventHandler(JJM_01_FormClosed);
            CmbBoxSelectProfile.DropDown += new EventHandler(CmbBoxSelectProfile_DropDown);
            CmbBoxSelectProfile.SelectedIndexChanged += new EventHandler(CmbBoxSelectProfile_SelectedIndexChanged);

            //thr = new Thread(() => ThreadSendInputNameToDeviceScreen()); ;
            //thr.Start();
        }

        private void OpenInputModal(Profile profile, int idInput)
        {
            ChangeInputInfo inputForm = new ChangeInputInfo(this, _device.ActiveProfile, idInput);
            Visible = false;
            inputForm.ShowDialog();
            _device.ActiveProfile.UpdateInputs();
            _IsInputSelected = false;
        }

        #region Events
        private void JJM_01_FormClosing(object sender, FormClosingEventArgs e)
        {
            _parent.Visible = true;
        }

        /*private void JJM_01_FormClosed(object sender, FormClosedEventArgs e)
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
                    device.SendInputNameToDeviceScreen(i, _profile.Id);
                    Thread.Sleep(200);
                }
            }
        }

        private void timerReceiveHIDMessage_Tick(object sender, EventArgs e)
        {
            DataReceived = device.ReceiveHIDMessage();

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
        }*/

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

            foreach (String Profile in Profile.GetList(_device.JJID))
                CmbBoxSelectProfile.Items.Add(Profile);

            CmbBoxSelectProfile.SelectedIndex = selectedIndex;
        }

        private void CmbBoxSelectProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CmbBoxSelectProfile.SelectedIndex == -1)
            {
                CmbBoxSelectProfile.SelectedIndex = 0;
            }
            
            _device.UpdateActiveProfile(CmbBoxSelectProfile.SelectedItem.ToString());
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
                        OpenInputModal(_device.ActiveProfile, 1);
                    });
                }
                else
                {
                    OpenInputModal(_device.ActiveProfile, 1);
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
                        OpenInputModal(_device.ActiveProfile, 2);
                    });
                }
                else
                {
                    OpenInputModal(_device.ActiveProfile, 2);
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
                        OpenInputModal(_device.ActiveProfile, 3);
                    });
                }
                else
                {
                    OpenInputModal(_device.ActiveProfile, 3);
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
                        OpenInputModal(_device.ActiveProfile, 4);
                    });
                }
                else
                {
                    OpenInputModal(_device.ActiveProfile, 4);
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
                        OpenInputModal(_device.ActiveProfile, 5);
                    });
                }
                else
                {
                    OpenInputModal(_device.ActiveProfile, 5);
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
                _DatabaseConnection.DeleteProfile(CmbBoxSelectProfile.SelectedItem.ToString(), _device.Id);

                CmbBoxSelectProfile.Items.Clear();

                foreach (String Profile in Profile.GetList(_device.Id))
                    CmbBoxSelectProfile.Items.Add(Profile);

                CmbBoxSelectProfile.SelectedIndex = 0;

                MessageBox.Show("Perfil excluído com sucesso!");
            }
        }
        #endregion
    }
}
