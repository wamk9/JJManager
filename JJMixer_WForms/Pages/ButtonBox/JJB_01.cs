using HidSharp;
using JJMixer.Class;
using JJMixer_WForms.Class;
using MaterialSkin;
using MaterialSkin.Controls;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JJMixer_WForms.Pages.ButtonBox
{
    public partial class JJB_01 : MaterialForm
    {
        Joystick _Joystick = null;
        JJMixerCommunication _JJMixerCommunication = null;
        static JJMixerAudioManager _JJMixerAudioManager = null;

        static String LastAxisXValue = "";
        static String LastAxisYValue = "";
        public JJB_01(Joystick joystick)
        {
            InitializeComponent();

            if (joystick == null)
                Close();

            _Joystick = joystick;
            _JJMixerCommunication = new JJMixerCommunication(_Joystick);
            _JJMixerAudioManager = new JJMixerAudioManager();

            timerJoystickAxisUpdate.Start();

            Thread thr = new Thread(() => ThreadJoystickCommunication()); ;
            thr.Start();


            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
        }

        private void timerJoystickAxisUpdate_Tick(object sender, EventArgs e)
        {
            String ValueX = _JJMixerCommunication.GetJoystickAxisPercentage(_Joystick, 1, "X", "JJB-01");
            String ValueY = _JJMixerCommunication.GetJoystickAxisPercentage(_Joystick, 2, "Y", "JJB-01");

            //materialLabel1.Text = "X: " + Value;
            //materialLabel1.Text = "X: " + (Value != "" ? Value : LastAxisXValue);
            LastAxisXValue = (ValueX != "" ? ValueX : LastAxisXValue);
            LastAxisYValue = (ValueY != "" ? ValueY : LastAxisYValue);
        }

        static void ThreadJoystickCommunication()
        {
            String DataReceived = "";
            String[] DataTreated = null;

            while (true)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();

                _JJMixerAudioManager.ChangeInputVolume("1", "JJB-01", (LastAxisXValue != "" ? (Int16.Parse(LastAxisXValue)): 0));
                _JJMixerAudioManager.ChangeInputVolume("2", "JJB-01", (LastAxisYValue != "" ? (Int16.Parse(LastAxisYValue)) : 0));
            }
        }

        private void BtnInput01JJB01_Click(object sender, EventArgs e)
        {
            ChangeInputInfo inputForm = new ChangeInputInfo("1", "JJB-01");
            inputForm.Show();
        }

        private void BtnInput01JJB01_MouseEnter(object sender, EventArgs e)
        {
            BtnInput01JJB01.Image = JJMixer_WForms.Properties.Resources.JJB_01_input01_hover;
        }

        private void BtnInput01JJB01_MouseLeave(object sender, EventArgs e)
        {
            BtnInput01JJB01.Image = JJMixer_WForms.Properties.Resources.JJB_01_input01;
        }

        private void BtnInput02JJB01_Click(object sender, EventArgs e)
        {
            ChangeInputInfo inputForm = new ChangeInputInfo("2", "JJB-01");
            inputForm.Show();
        }
        
        private void BtnInput02JJB01_MouseEnter(object sender, EventArgs e)
        {
            BtnInput02JJB01.Image = JJMixer_WForms.Properties.Resources.JJB_01_input02_hover;
        }

        private void BtnInput02JJB01_MouseLeave(object sender, EventArgs e)
        {
            BtnInput02JJB01.Image = JJMixer_WForms.Properties.Resources.JJB_01_input02;
        }

        private void BtnDisconnectJJB01_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
