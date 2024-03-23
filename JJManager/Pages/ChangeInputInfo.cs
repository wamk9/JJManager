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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using static System.Windows.Forms.DataFormats;

namespace JJManager.Pages
{
    public partial class ChangeInputInfo : MaterialForm
    {
        List<CoreAudioDevice> devices = new CoreAudioController().GetDevices().ToList();
        Dictionary<string, string> inputParams = new Dictionary<string, string>();
        JJManager.Class.App.WaitForm waitForm = new JJManager.Class.App.WaitForm();

        private int _IdInput = 0;
        private Profile _profile = null;
        private AnalogInput _input = null;
        private MaterialForm _parent = null;

        private Thread threadAudioSession = null;

        #region WinForms
        private MaterialSkinManager materialSkinManager = null;
        #endregion


        public ChangeInputInfo(MaterialForm parent, Profile profile, int idInput)
        {
            DatabaseConnection database = new DatabaseConnection();

            InitializeComponent();
            components = new System.ComponentModel.Container();

            _IdInput = idInput;
            _profile = profile;
            _input = _profile.GetAnalogInputById(_IdInput);
            _parent = parent;

            // MaterialDesign
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = database.GetTheme();
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);

            FormClosing += ChangeInputInfo_FormClosing;

            UpdateProgramsToCmb();
            UpdateDevicesToChkBox();

            // Events
            FormClosed += new FormClosedEventHandler(ChangeInputInfo_FormClosed);
        }

        private void InsertOnAppsList(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !RdBtnInputApp.Checked)
            { 
                return;
            }

            lvwApps.Items.Add(path);
        }
        private void ShowAppForms()
        {
            // Hide devices forms...
            cklDevices.Hide();

            // ...And show apps
            lvwApps.Show();
            btnAddExecutable.Show();
            btnAddSessionActive.Show();
            btnAddFolder.Show();
            btnDeleteItems.Show();
        }

        private void ShowDeviceForms()
        {
            // Hide apps forms...
            lvwApps.Hide();
            btnAddExecutable.Hide();
            btnAddSessionActive.Hide();
            btnAddFolder.Hide();
            btnDeleteItems.Hide();
            
            // ...And show devices
            cklDevices.Show();
        }

        private void ChangeInputInfo_FormClosing(object sender, FormClosingEventArgs e)
        {
            _parent.Visible = true;
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
            String inputInfo = "";
            List<String> ChkInputId = new List<String>();

            if (RdBtnInputApp.Checked)
            {
                InputType = "app";
                foreach (ListViewItem item in lvwApps.Items)
                {
                    inputInfo += ((inputInfo.Length > 0 ? "|" : "") + item.Text);
                }
            }
            else
            {
                
                InputType = "device";
                bool SetPipe = false;
                foreach (var InputTypeChkBox in cklDevices.Items)
                {
                    if (InputTypeChkBox.Checked)
                    {
                        if (SetPipe)
                        {
                            ChkInputId = InputTypeChkBox.Text.Split().Where(x => x.StartsWith("(") && x.EndsWith(")"))
                                .Select(x => x.Replace("(", string.Empty).Replace(")", string.Empty))
                                .ToList();
                            inputInfo += "|" + ChkInputId.Last();
                        }
                        else
                        {
                            ChkInputId = InputTypeChkBox.Text.Split().Where(x => x.StartsWith("(") && x.EndsWith(")"))
                                .Select(x => x.Replace("(", string.Empty).Replace(")", string.Empty))
                                .ToList();
                            inputInfo += ChkInputId.Last();
                            SetPipe = true;
                        }
                    }
                }
            }

            _input.SaveInputData(TxtInputName.Text, InputType, inputInfo, ChkBoxInvertAxis.Checked);

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
            cklDevices.Items.Clear();
            
            foreach (CoreAudioDevice device in devices)
            {
                cklDevices.Items.Add(device.Name + " - " + device.InterfaceName + " (" + device.Id + ")");
            }
        }

        private void UpdateProgramsToCmb()
        {
            /*
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
            }*/
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
                    lvwApps.Items.Clear();
                    foreach (string info in _input.Info.Split('|'))
                    {
                        lvwApps.Items.Add(info);

                    }
                    ShowAppForms();
                    break;
                case "device":
                    RdBtnInputDevices.Checked = true;
                    String[] ChkArray = _input.Info.Split('|');

                    foreach (var InputTypeChkBox in cklDevices.Items)
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
                    //TxtMultiLineApplications.Text = "";
                    break;
            }

            ChkBoxInvertAxis.Checked = _input.InvertedAxis;
        }

        private void DisableAllForms()
        {
            // Disable default forms
            BtnSaveInputConf.Enabled = false;
            BtnCancelInputConf.Enabled = false;
            ChkBoxInvertAxis.Enabled = false;
            RdBtnInputApp.Enabled = false;
            RdBtnInputDevices.Enabled = false;

            // Disable all Apps management Forms
            btnAddExecutable.Enabled = false;
            btnAddFolder.Enabled = false;
            btnAddSessionActive.Enabled = false;
            btnDeleteItems.Enabled = false;
            lvwApps.Enabled = false;
        }
        
        private void EnableAllForms()
        {
            // Disable default forms
            BtnSaveInputConf.Enabled = true;
            BtnCancelInputConf.Enabled = true;
            ChkBoxInvertAxis.Enabled = true;
            RdBtnInputApp.Enabled = true;
            RdBtnInputDevices.Enabled = true;

            // Disable all Apps management Forms
            btnAddExecutable.Enabled = true;
            btnAddFolder.Enabled = true;
            btnAddSessionActive.Enabled = true;
            btnDeleteItems.Enabled = (lvwApps.SelectedItems.Count > 0 ? true : false);
            lvwApps.Enabled = true;
        }

        private void btnAddFolder_Click(object sender, EventArgs e)
        {
            DisableAllForms();

            DialogResult insertExecutables = MessageBox.Show("Ao executarmos essa opção, TODOS os executáveis da pasta selecionada (incluindo suas subpastas) serão inseridos na lista do input '" + _input.Name + "', você deseja continuar o processo? Lembrando que pode ser que o JJManager pare de responder durante esse processo.", "Inserção de listagem de executáveis", MessageBoxButtons.YesNo);

            if (insertExecutables == DialogResult.No)
            {
                EnableAllForms();
                return;
            }

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            DialogResult result = folderBrowserDialog.ShowDialog();

            waitForm.Show(this);

            if (result == DialogResult.OK && folderBrowserDialog.SelectedPath.Length > 0)
            {
                Thread thread = new Thread(() =>
                {
                    String[] executables =
                        Directory.GetFiles(folderBrowserDialog.SelectedPath, "*.exe", SearchOption.AllDirectories)
                        .Select(fileName => Path.GetFileName(fileName))
                        .AsEnumerable()
                        .ToArray();

                    foreach (string executable in executables)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            InsertOnAppsList(executable);
                        });
                    }

                    BeginInvoke((MethodInvoker)delegate
                    {
                        EnableAllForms();
                    });
                });

                thread.Start();
            }
                
            waitForm.Close();
            
        }

        private void btnAddExecutable_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Executable Files|*.exe";
            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK && openFileDialog.FileNames.Length > 0)
            {
                foreach (string filename in openFileDialog.FileNames)
                {
                    InsertOnAppsList(System.IO.Path.GetFileName(filename));
                }
            }
        }

        private void btnAddSessionActive_Click(object sender, EventArgs e)
        {
            JJManager.Pages.App.AudioSession audioSession = new JJManager.Pages.App.AudioSession(this, _input.AudioManager);

            Visible = false;

            if (audioSession.ShowDialog() == DialogResult.OK)
            {
                

                foreach (string executable in audioSession.ExecutableOfSessions)
                {
                    InsertOnAppsList(executable);
                }

            }
        }

        private void btnDeleteItems_Click(object sender, EventArgs e)
        {
            List<ListViewItem> itemsToExclude = new List<ListViewItem>();

            foreach (ListViewItem item in lvwApps.SelectedItems)
            {
                itemsToExclude.Add(item);
            }

            if (itemsToExclude.Count == 0)
            {
                return;
            }

            DisableAllForms();
            waitForm.Show(this);

            DialogResult result = MessageBox.Show("Deseja mesmo deletar " + (itemsToExclude.Count > 1 ? "os " + itemsToExclude.Count + " items" : "o item") + " selecionado?", "Exclusão de itens", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                foreach (ListViewItem item in itemsToExclude)
                {
                    lvwApps.Items.Remove(item);
                }

                lvwApps.SelectedItems.Clear();
            }

            EnableAllForms();
            waitForm.Close();
        }

        private void lvwApps_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (lvwApps.SelectedItems.Count > 0)
            {
                btnDeleteItems.Enabled = true;
            }
            else
            {
                btnDeleteItems.Enabled = false;
            }
        }
    }
}
