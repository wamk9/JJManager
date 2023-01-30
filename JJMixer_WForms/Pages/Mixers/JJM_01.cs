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
        private static JJMixerCommunication _JJMixerCommunication = new JJMixerCommunication();
        private static JJMixerAudioManager _JJMixerAudioManager = new JJMixerAudioManager();
        private static JJMixerDbConnection _JJMixerDbConnection = new JJMixerDbConnection();
        public JJM_01(String MixerSerialPort)
        {
            InitializeComponent();

            if (MixerSerialPort.Contains("COM"))
            {
                _JJMixerCommunication.connectTo(MixerSerialPort);
                Thread thr = new Thread(() => ThreadSerialCom()); ;
                thr.Start();
            }
            else
            {
                Close();
            }

         
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
        }

        static void ThreadSerialCom()
        {
            int BytesReceived = 0;
            String DataReceived = "";
            String ReturnDictionary = "";
            String[] DataTreated = null;
            SortedDictionary<String, String> InputNames = new SortedDictionary<String, String>();

            while (_JJMixerCommunication.IsConnectionOpen())
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();

                try
                {
                    BytesReceived = _JJMixerCommunication.IsConnectionOpen() ? _JJMixerCommunication.serialObject.ReadByte() : -1;

                    if (BytesReceived != -1)
                    {
                        DataReceived = _JJMixerCommunication.serialObject.ReadLine().TrimEnd('\r');
                        DataTreated = DataReceived.Split('|').Skip(1).ToArray();
                        InputNames = _JJMixerDbConnection.GetAllInputName("JJM-01");

                        for (int i = 0; i < DataTreated.Length; i++)
                        {
                            if (!InputNames.TryGetValue((i+1).ToString(), out ReturnDictionary))
                            {
                                InputNames[(i + 1).ToString()] = "Input " + (i + 1).ToString();
                            }
                             _JJMixerAudioManager.ChangeInputVolume((i+1).ToString(), "JJM-01", (Int16.Parse(DataTreated[i].ToString())) * 100 / 1023);
                        }

                        _JJMixerCommunication.SendToDevice(String.Join("|", InputNames.Values));
                    }
                }
                catch (IOException ex)
                { 
                
                }
            }


            /*while (serialPortCommunication.IsConnectionOpen())
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();

                try
                {
                    int b = serialPortCommunication.IsConnectionOpen() ? serialPortCommunication.serialObject.ReadByte() : -1;

                    if (b != -1)
                    {
                        String teste = serialPortCommunication.serialObject.ReadLine().TrimEnd('\r');
                        String[] dataReceived = teste.Split('|');

                        int[] inputValuesInt = new int[dataReceived.Count()];

                        for (int i = 0; i < dataReceived.Count(); i++)
                        {
                            inputValuesInt[i] = (Int32.Parse(dataReceived[i]) * 100) / 1023;
                        }

                            serialPortCommunication.SendToDevice(inputNames);
                            //MessageBox.Show(inputNames);


                            for (int i = 0; i < dataReceived.Count(); i++)
                            {
                                var executableNameByInput = inputExec.ElementAt(i).Split('|');
                                int executableNameByInputCount = executableNameByInput.Count();

                                if (executableNameByInputCount == 0)
                                {
                                    continue;
                                }
                                else
                                {
                                    foreach (var executableNameIndividual in executableNameByInput)
                                    {
                                        if (executableNameIndividual == "{MASTER}")
                                        {
                                            defaultPlaybackDevice.SetVolumeAsync(inputValuesInt[i]);
                                        }
                                        else if (executableNameIndividual == "{MASTER-COMMUNICATION}")
                                        {
                                            DefaultPlaybackCommunicationsDevice.SetVolumeAsync(inputValuesInt[i]);
                                        }
                                        else if (executableNameIndividual == "{MIC}")
                                        {
                                            DefaultCaptureDevice.SetVolumeAsync(inputValuesInt[i]);
                                        }
                                        else if (executableNameIndividual == "{MIC-COMMUNICATION}")
                                        {
                                            DefaultCaptureCommunicationsDevice.SetVolumeAsync(inputValuesInt[i]);
                                        }
                                        else if (executableNameIndividual != "")
                                        {
                                            foreach (var audioSession in defaultPlaybackDevice.GetCapability<IAudioSessionController>())
                                            {
                                                if (audioSession.ExecutablePath != null && audioSession.ExecutablePath.Contains(executableNameIndividual))
                                                {
                                                    audioSession.SetVolumeAsync(inputValuesInt[i]);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (IOException ex)
                    {
                        //FIX-ME: Verificar pq quando fecha conexão forçadamente cai aqui.
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }

                }*/
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
            if (_JJMixerCommunication.IsConnectionOpen())
            {
                _JJMixerCommunication.Disconnect();
            }
        }
    }
}
