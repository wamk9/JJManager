using HidSharp;
using JJManager.Class;
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
        private static Class.Devices _JJManagerCommunication;
        private static AudioManager _JJManagerAudioManager = new AudioManager();
        private static DatabaseConnection _DatabaseConnection = new DatabaseConnection();
        private Thread thr = null;
        private bool _DisconnectDevice = false;

        #region WinForms
        MaterialSkinManager materialSkinManager = null;
        private AppModulesTimer HidReceiver = null;
        private AppModulesTimer HidSender = null;
        private AppModulesTimer UpdateDevices = null;
        private AppModulesNotifyIcon notifyIcon = null;
        #endregion


        public JJM_01(HidDevice device)
        {
            InitializeComponent();
            components = new System.ComponentModel.Container();

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => {
                    // MaterialDesign
                    materialSkinManager = MaterialSkinManager.Instance;
                    materialSkinManager.AddFormToManage(this);
                    materialSkinManager.Theme = _DatabaseConnection.GetTheme();
                    materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
                }));
            }

            if (device == null)
            {
                _DisconnectDevice = true;
                Close();
            }

            _JJManagerCommunication = new Class.Devices(device);

            // Start NotifyIcon
            notifyIcon = new AppModulesNotifyIcon(components, device.GetFriendlyName(), NotifyIcon_Click);
            
            //Start Timers
            HidSender = new AppModulesTimer(components, 50, timerSendHIDMessage_Tick);
            HidReceiver = new AppModulesTimer(components, 50, timerReceiveHIDMessage_Tick);
            UpdateDevices = new AppModulesTimer(components, 500, timerUpdateDevices_Tick);

            // Events
            FormClosing += new FormClosingEventHandler(JJM_01_FormClosing);
            FormClosed += new FormClosedEventHandler(JJM_01_FormClosed);

            thr = new Thread(() => ThreadSendInputNameToDeviceScreen(_JJManagerCommunication)); ;
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
                HidSender.Stop();
                HidReceiver.Stop();
            }
        }

        private void JJM_01_FormClosed(object sender, FormClosedEventArgs e)
        {
            Enabled = false;
            thr.Abort();
            GC.Collect();
        }

        private void timerSendHIDMessage_Tick(object sender, EventArgs e)
        {

        }
        private void timerUpdateDevices_Tick(object sender, EventArgs e)
        {
            /*
            Thread thr = new Thread(() =>
            {
                _JJManagerAudioManager.RefreshAudioDevices();
                GC.Collect();
            });
            thr.Start();
            */
        }

        private void ThreadSendInputNameToDeviceScreen(Class.Devices jJManagerCommunication)
        {
            while (true) 
            {
                for (int i = 1; i <= 5; i++)
                {
                    _JJManagerCommunication.SendInputNameToDeviceScreen(i);
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

            DataReceived = _JJManagerCommunication.ReceiveHIDMessage();

            if (DataReceived != String.Empty)
            {
                DataTreated = DataReceived.Split('|').ToArray();

                for (int i = 0; i < DataTreated.Length; i++)
                    _JJManagerAudioManager.ChangeInputVolume(_JJManagerCommunication.Id, (i + 1).ToString(), (Int16.Parse(DataTreated[i])));
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
        #endregion

        #region Buttons
        private void BtnInput01JJM01_Click(object sender, EventArgs e)
        {
            Thread thr = new Thread(() => {
                ChangeInputInfo inputForm = new ChangeInputInfo(_JJManagerCommunication.Id, "1");
                inputForm.ShowDialog();
                Thread.CurrentThread.Abort();
            }); ;
            thr.Start();
        }

        private void BtnInput01JJM01_MouseEnter(object sender, EventArgs e)
        {
            BtnInput01JJM01.Image = JJManager.Properties.Resources.JJM_01_input_hover;
        }

        private void BtnInput01JJM01_MouseLeave(object sender, EventArgs e)
        {
            BtnInput01JJM01.Image = JJManager.Properties.Resources.JJM_01_input;
        }

        private void BtnInput02JJM01_Click(object sender, EventArgs e)
        {
            Thread thr = new Thread(() => {
                ChangeInputInfo inputForm = new ChangeInputInfo(_JJManagerCommunication.Id, "2");
                inputForm.ShowDialog();
                Thread.CurrentThread.Abort();
            }); ;
            thr.Start();
        }

        private void BtnInput02JJM01_MouseEnter(object sender, EventArgs e)
        {
            BtnInput02JJM01.Image = JJManager.Properties.Resources.JJM_01_input_hover;
        }

        private void BtnInput02JJM01_MouseLeave(object sender, EventArgs e)
        {
            BtnInput02JJM01.Image = JJManager.Properties.Resources.JJM_01_input;
        }

        private void BtnInput03JJM01_Click(object sender, EventArgs e)
        {
            Thread thr = new Thread(() => {
                ChangeInputInfo inputForm = new ChangeInputInfo(_JJManagerCommunication.Id, "3");
                inputForm.ShowDialog();
                Thread.CurrentThread.Abort();
            }); ;
            thr.Start();
        }

        private void BtnInput03JJM01_MouseEnter(object sender, EventArgs e)
        {
            BtnInput03JJM01.Image = JJManager.Properties.Resources.JJM_01_input_hover;
        }

        private void BtnInput03JJM01_MouseLeave(object sender, EventArgs e)
        {
            BtnInput03JJM01.Image = JJManager.Properties.Resources.JJM_01_input;
        }

        private void BtnInput04JJM01_Click(object sender, EventArgs e)
        {
            Thread thr = new Thread(() => {
                ChangeInputInfo inputForm = new ChangeInputInfo(_JJManagerCommunication.Id, "4");
                inputForm.ShowDialog();
                Thread.CurrentThread.Abort();
            }); ;
            thr.Start();
        }

        private void BtnInput04JJM01_MouseEnter(object sender, EventArgs e)
        {
            BtnInput04JJM01.Image = JJManager.Properties.Resources.JJM_01_input_hover;
        }

        private void BtnInput04JJM01_MouseLeave(object sender, EventArgs e)
        {
            BtnInput04JJM01.Image = JJManager.Properties.Resources.JJM_01_input;
        }

        private void BtnInput05JJM01_Click(object sender, EventArgs e)
        {
            Thread thr = new Thread(() => {
                ChangeInputInfo inputForm = new ChangeInputInfo(_JJManagerCommunication.Id, "5");
                inputForm.ShowDialog();
                Thread.CurrentThread.Abort();
            }); ;
            thr.Start();
        }

        private void BtnInput05JJM01_MouseEnter(object sender, EventArgs e)
        {
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
        #endregion
    }
}
