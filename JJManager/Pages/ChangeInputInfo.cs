using AudioSwitcher.AudioApi.CoreAudio;
using AudioSwitcher.AudioApi.Session;
using JJManager.Class;
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

namespace JJManager.Pages
{
    public partial class ChangeInputInfo : MaterialForm
    {
        List<CoreAudioDevice> devices = new CoreAudioController().GetDevices().ToList();
        Dictionary<string, string> inputParams = new Dictionary<string, string>();

        private int _IdInput = 0;
        private Profiles _profile = null;
        private Inputs _input = null;

        #region WinForms
        private MaterialSkinManager materialSkinManager = null;
        #endregion


        public ChangeInputInfo(Profiles profile, int idInput)
        {
            DatabaseConnection database = new DatabaseConnection();

            InitializeComponent();
            components = new System.ComponentModel.Container();

            _IdInput = idInput;
            _profile = profile;
            _input = _profile.GetInputById(_IdInput);

            // MaterialDesign
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = database.GetTheme();
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);

            UpdateProgramsToCmb();
            UpdateDevicesToChkBox();

            // Events
            FormClosed += new FormClosedEventHandler(ChangeInputInfo_FormClosed);
        }

        private void ShowAppForms()
        {
            TxtMultiLineApplications.Show();
            CmbProgramsWithAudio.Show();
            BtnAddProgram.Show();
            ChkBoxInputDevices.Hide();
        }

        private void ShowDeviceForms()
        {
            TxtMultiLineApplications.Hide();
            CmbProgramsWithAudio.Hide();
            BtnAddProgram.Hide();
            ChkBoxInputDevices.Show();
        }

        private void ChangeInputInfo_FormClosed(object sender, FormClosedEventArgs e)
        {
            Enabled = false;
        }

        private void BtnCancelInputConf_Click(object sender, EventArgs e)
        {
            Close();
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

            _input.SaveInputData(TxtInputName.Text, InputType, InputInfo, ChkBoxInvertAxis.Checked);

            //_DatabaseConnection.SaveInputData(_IdProfile, Int16.Parse(_IdInput), TxtInputName.Text, InputType, InputInfo, (ChkBoxInvertAxis.Checked ? "inverted" : "normal"));
            this.Close();
        }

        private void RdBtnInput_CheckedChanged(object sender, EventArgs e)
        {
            if (RdBtnInputApp.Checked)
                ShowAppForms();
            else
                ShowDeviceForms();
        }

        private void UpdateDevicesToChkBox()
        {
            ChkBoxInputDevices.Items.Clear();
            
            foreach (CoreAudioDevice device in devices)
            {
                ChkBoxInputDevices.Items.Add(device.Name + " - " + device.InterfaceName + " (" + device.Id + ")");
            }
        }

        private void UpdateProgramsToCmb()
        {
            CmbProgramsWithAudio.Items.Clear();

            foreach (CoreAudioDevice device in devices)
            {
                if (device.IsPlaybackDevice)
                {
                    foreach (var audioSession in device.GetCapability<IAudioSessionController>().ActiveSessions())
                    {
                        if (audioSession.ExecutablePath != null)
                        {
                            CmbProgramsWithAudio.Items.Add(audioSession.DisplayName + " (" + audioSession.ExecutablePath.Split('\\').Last() + ")");
                        }
                    }
                }
            }
        }

        private void ChangeInputInfo_Load(object sender, EventArgs e)
        {
            DatabaseConnection database = new DatabaseConnection();

            this.Text = "Input " + _IdInput + " - Configurações";

            TxtInputName.Text = _input.Name;

            switch(_input.Type)
            {
                case "app":
                    RdBtnInputApp.Checked = true;
                    TxtMultiLineApplications.Text = _input.Info.Replace("|", "\n");
                    ShowAppForms();
                    break;
                case "device":
                    RdBtnInputDevices.Checked = true;
                    String[] ChkArray = _input.Info.Split('|');

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
                    ShowDeviceForms();
                    break;
                default:
                    TxtInputName.Text = "";
                    ShowAppForms();
                    TxtMultiLineApplications.Text = "";
                    break;
            }

            ChkBoxInvertAxis.Checked = _input.InvertedAxis;
        }

        private void BtnAddProgram_Click(object sender, EventArgs e)
        {
            int pFrom = CmbProgramsWithAudio.Text.IndexOf("(") + 1;
            int pTo = CmbProgramsWithAudio.Text.LastIndexOf(")");

            String text = CmbProgramsWithAudio.Text.Substring(pFrom, pTo - pFrom);


            TxtMultiLineApplications.AppendText(TxtMultiLineApplications.Text.Length > 0 ? "\n" + text : text);
            CmbProgramsWithAudio.SelectedIndex = -1;
            CmbProgramsWithAudio.Items.Clear();
        }

        private void CmbProgramsWithAudio_DropDown(object sender, EventArgs e)
        {
            UpdateProgramsToCmb();
        }
    }
}
