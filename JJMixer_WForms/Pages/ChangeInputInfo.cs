using AudioSwitcher.AudioApi.CoreAudio;
using JJMixer_WForms.Class;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.SqlServer.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JJMixer_WForms.Pages
{
    public partial class ChangeInputInfo : MaterialForm
    {
        List<CoreAudioDevice> devices = new CoreAudioController().GetDevices().ToList();
        Dictionary<string, string> inputParams = new Dictionary<string, string>();
        JJMixerDbConnection dbConn = new JJMixerDbConnection();
        
        private String _Id = "";
        private String _Model = "";

        public ChangeInputInfo(String Id, String Model)
        {
            InitializeComponent();

            _Id= Id;
            _Model = Model;

            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
        }

        private void BtnCancelInputConf_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnSaveInputConf_Click(object sender, EventArgs e)
        {
            String InputType = "";
            String InputInfo = "";
            List<String> ChkInputId = new List<String>();
            if (RdBtnInputApp.Checked)
            {
                InputType = "app";
                InputInfo = TxtMultiLineApplications.Text.Trim().Replace("\n", "|");
            }
            else
            {
                InputType = "device";
                bool SetPipe = false;
                foreach (var InputTypeChkBox in ChkBoxInputDevices.Items)
                {
                    if (InputTypeChkBox.Checked)
                    {
                        if (SetPipe)
                        {
                            ChkInputId = InputTypeChkBox.Text.Split().Where(x => x.StartsWith("(") && x.EndsWith(")"))
                                .Select(x => x.Replace("(", string.Empty).Replace(")", string.Empty))
                                .ToList();
                            InputInfo += "|" + ChkInputId.Last();
                        }
                        else
                        {
                            ChkInputId = InputTypeChkBox.Text.Split().Where(x => x.StartsWith("(") && x.EndsWith(")"))
                                .Select(x => x.Replace("(", string.Empty).Replace(")", string.Empty))
                                .ToList();
                            InputInfo += ChkInputId.Last();
                            SetPipe = true;
                        }
                    }
                }
            }


            dbConn.SaveInputData(Int16.Parse(_Id), TxtInputName.Text, InputType, InputInfo, _Model);
            this.Close();
        }

        private void RdBtnInput_CheckedChanged(object sender, EventArgs e)
        {
            if (RdBtnInputApp.Checked)
            {
                TxtMultiLineApplications.Show();
                ChkBoxInputDevices.Hide();

            }
            else
            {
                TxtMultiLineApplications.Hide();
                UpdateDevicesToChkBox();
                ChkBoxInputDevices.Show();
            }
        }

        private void UpdateDevicesToChkBox()
        {
            ChkBoxInputDevices.Items.Clear();

            foreach (CoreAudioDevice device in devices)
            {
                ChkBoxInputDevices.Items.Add(device.Name + " - " + device.InterfaceName + " (" + device.Id + ")");
            }
        }

        private void ChangeInputInfo_Load(object sender, EventArgs e)
        {
            this.Text = "Input " + _Id + " - Configurações";
            String returnDictionary = "";

            inputParams = dbConn.GetInputData(Int16.Parse(_Id), _Model);

            if (inputParams.TryGetValue("input_name", out returnDictionary))
            {
                TxtInputName.Text = returnDictionary;
            }
            else
            {
                TxtInputName.Text = "";
            }

            if (inputParams.TryGetValue("input_type", out returnDictionary))
            {
                if (returnDictionary == "device")
                {
                    RdBtnInputDevices.Checked = true;
                    TxtMultiLineApplications.Hide();
                    ChkBoxInputDevices.Show();
                }
                else
                {
                    RdBtnInputApp.Checked = true;
                    TxtMultiLineApplications.Show();
                    ChkBoxInputDevices.Hide();
                }
            }
            else
            {
                TxtInputName.Text = "";
                TxtMultiLineApplications.Show();
                ChkBoxInputDevices.Hide();
            }


            if (inputParams.TryGetValue("input_info", out returnDictionary))
            {
                if (RdBtnInputApp.Checked)
                {
                    TxtMultiLineApplications.Text = returnDictionary.Replace("|", "\n");
                }
                else
                {
                    String[] ChkArray = returnDictionary.Split('|');

                    foreach (var InputTypeChkBox in ChkBoxInputDevices.Items)
                    {
                        for (int i = 0; i < ChkArray.Length; i++)
                        {
                            if (InputTypeChkBox.Text.EndsWith("(" + ChkArray[i] + ")"))
                            {
                                InputTypeChkBox.Checked = true;
                            }
                        }
                    }
                }
            }
            else
            {
                TxtMultiLineApplications.Text = "";
            }
        }
    }
}
