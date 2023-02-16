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
        private static Class.Devices _JJManagerCommunication;
        private static AudioManager _JJManagerAudioManager = new AudioManager();
        private static DatabaseConnection _DatabaseConnection = new DatabaseConnection();

        private bool _DisconnectDevice = false;

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

            if (joystick == null)
                Close();

            _Joystick = joystick;

            _JJManagerCommunication = new Class.Devices(_Joystick);

            // Start NotifyIcon
            notifyIcon = new AppModulesNotifyIcon(components, _Joystick.Properties.ProductName, NotifyIcon_Click);

            //Start Timers
            JoystickReceiver = new AppModulesTimer(components, 50, TimerReceiveJoystickMessage_Tick);

            // Events
            FormClosing += new FormClosingEventHandler(JJB_01_FormClosing);
            FormClosed += new FormClosedEventHandler(JJB_01_FormClosed);
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
            String ValueX = _JJManagerCommunication.GetJoystickAxisPercentage(_Joystick, 1, "X", "JJB-01");
            String ValueY = _JJManagerCommunication.GetJoystickAxisPercentage(_Joystick, 2, "Y", "JJB-01");

            LastAxisXValue = (ValueX != "" ? ValueX : LastAxisXValue);
            LastAxisYValue = (ValueY != "" ? ValueY : LastAxisYValue);

            GC.Collect();
            GC.WaitForPendingFinalizers();

            _JJManagerAudioManager.ChangeInputVolume("1", "JJB-01", (LastAxisXValue != "" ? (Int16.Parse(LastAxisXValue)) : 0));
            _JJManagerAudioManager.ChangeInputVolume("2", "JJB-01", (LastAxisYValue != "" ? (Int16.Parse(LastAxisYValue)) : 0));
        }
        #endregion

        #region Buttons
        private void BtnInput01JJB01_Click(object sender, EventArgs e)
        {
            ChangeInputInfo inputForm = new ChangeInputInfo("1", "JJB-01");
            inputForm.Show();
        }

        private void BtnInput01JJB01_MouseEnter(object sender, EventArgs e)
        {
            BtnInput01JJB01.Image = JJManager.Properties.Resources.JJB_01_input01_hover;
        }

        private void BtnInput01JJB01_MouseLeave(object sender, EventArgs e)
        {
            BtnInput01JJB01.Image = JJManager.Properties.Resources.JJB_01_input01;
        }

        private void BtnInput02JJB01_Click(object sender, EventArgs e)
        {
            ChangeInputInfo inputForm = new ChangeInputInfo("2", "JJB-01");
            inputForm.Show();
        }
        
        private void BtnInput02JJB01_MouseEnter(object sender, EventArgs e)
        {
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
        #endregion
    }
}
