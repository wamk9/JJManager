using HidSharp;
using JJMixer.Class;
using JJMixer_WForms.Class;
using MaterialSkin;
using MaterialSkin.Controls;
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

namespace JJMixer_WForms.Pages
{
    public partial class JJM_01 : MaterialForm
    {
        private static JJMixerCommunication _JJMixerCommunication;
        private static JJMixerAudioManager _JJMixerAudioManager = new JJMixerAudioManager();
        private static JJMixerDbConnection _JJMixerDbConnection = new JJMixerDbConnection();
        public JJM_01(HidDevice device)
        {
            InitializeComponent();

            if (device == null)
                this.Close();

            _JJMixerCommunication = new JJMixerCommunication(device);

            timerSendHIDMessage.Start();

            Thread thr = new Thread(() => ThreadHIDCommunication()); ;
            thr.Start();


            
            /*if (MixerSerialPort.Contains("COM"))
            {
                _JJMixerCommunication.connectTo(MixerSerialPort);
                Thread thr = new Thread(() => ThreadSerialCom()); ;
                thr.Start();
            }
            else
            {
                Close();
            }*/


            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
        }

        static void ThreadHIDCommunication()
        {
            String DataReceived = "";
            String[] DataTreated = null;

            while (true)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();

                DataReceived = _JJMixerCommunication.ReceiveHIDMessage();

                if (DataReceived != String.Empty)
                {
                    DataTreated = DataReceived.Split('|').ToArray();
                    //InputNames = _JJMixerDbConnection.GetAllInputName("JJM-01");

                    for (int i = 0; i < DataTreated.Length; i++)
                        _JJMixerAudioManager.ChangeInputVolume((i + 1).ToString(), "JJM-01", (Int16.Parse(DataTreated[i])));
                }
                //  break;

                
                //_JJMixerCommunication.SendToDevice(String.Join("|", InputNames.Values));
            }
        }

        #region Botoes
        private void BtnInput01JJM01_Click(object sender, EventArgs e)
        {
            ChangeInputInfo inputForm = new ChangeInputInfo("1", "JJM-01");
            inputForm.Show();
        }

        private void BtnInput01JJM01_MouseEnter(object sender, EventArgs e)
        {
            BtnInput01JJM01.Image = JJMixer_WForms.Properties.Resources.JJM_01_input_hover;
        }

        private void BtnInput01JJM01_MouseLeave(object sender, EventArgs e)
        {
            BtnInput01JJM01.Image = JJMixer_WForms.Properties.Resources.JJM_01_input;
        }

        private void BtnInput02JJM01_Click(object sender, EventArgs e)
        {
            ChangeInputInfo inputForm = new ChangeInputInfo("2", "JJM-01");
            inputForm.Show();
        }

        private void BtnInput02JJM01_MouseEnter(object sender, EventArgs e)
        {
            BtnInput02JJM01.Image = JJMixer_WForms.Properties.Resources.JJM_01_input_hover;
        }

        private void BtnInput02JJM01_MouseLeave(object sender, EventArgs e)
        {
            BtnInput02JJM01.Image = JJMixer_WForms.Properties.Resources.JJM_01_input;
        }

        private void BtnInput03JJM01_Click(object sender, EventArgs e)
        {
            ChangeInputInfo inputForm = new ChangeInputInfo("3", "JJM-01");
            inputForm.Show();
        }

        private void BtnInput03JJM01_MouseEnter(object sender, EventArgs e)
        {
            BtnInput03JJM01.Image = JJMixer_WForms.Properties.Resources.JJM_01_input_hover;
        }

        private void BtnInput03JJM01_MouseLeave(object sender, EventArgs e)
        {
            BtnInput03JJM01.Image = JJMixer_WForms.Properties.Resources.JJM_01_input;
        }

        private void BtnInput04JJM01_Click(object sender, EventArgs e)
        {
            ChangeInputInfo inputForm = new ChangeInputInfo("4", "JJM-01");
            inputForm.Show();
        }

        private void BtnInput04JJM01_MouseEnter(object sender, EventArgs e)
        {
            BtnInput04JJM01.Image = JJMixer_WForms.Properties.Resources.JJM_01_input_hover;
        }

        private void BtnInput04JJM01_MouseLeave(object sender, EventArgs e)
        {
            BtnInput04JJM01.Image = JJMixer_WForms.Properties.Resources.JJM_01_input;
        }

        private void BtnInput05JJM01_Click(object sender, EventArgs e)
        {
            ChangeInputInfo inputForm = new ChangeInputInfo("5", "JJM-01");
            inputForm.Show();
        }

        private void BtnInput05JJM01_MouseEnter(object sender, EventArgs e)
        {
            BtnInput05JJM01.Image = JJMixer_WForms.Properties.Resources.JJM_01_input_hover;
        }

        private void BtnInput05JJM01_MouseLeave(object sender, EventArgs e)
        {
            BtnInput05JJM01.Image = JJMixer_WForms.Properties.Resources.JJM_01_input;
        }

        private void BtnDisconnectJJM01_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

        private void JJM_01_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void timerSendHIDMessage_Tick(object sender, EventArgs e)
        {
            _JJMixerCommunication.SendHIDInputs("JJM-01", 5);
        }
    }
}
