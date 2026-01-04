using NAudio.CoreAudioApi;
using JJManager.Class;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ConfigClass = JJManager.Class.App.Config.Config;
using ProfileClass = JJManager.Class.App.Profile.Profile;

namespace JJManager.Pages.App
{
    public partial class AudioController : MaterialForm
    {
        List<MMDevice> devices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active).ToList();
        Dictionary<string, string> inputParams = new Dictionary<string, string>();
        JJManager.Class.App.WaitForm waitForm = new JJManager.Class.App.WaitForm();

        private int _IdInput = -1;
        private ProfileClass _profile = null;
        private MaterialForm _parent = null;

        private Thread threadAudioSession = null;

        #region WinForms
        private MaterialSkinManager materialSkinManager = null;
        #endregion


        public AudioController(MaterialForm parent, ProfileClass profile, int idInput)
        {
            DatabaseConnection database = new DatabaseConnection();

            InitializeComponent();
            components = new System.ComponentModel.Container();

            _IdInput = idInput;
            _profile = profile;
            _parent = parent;

            // MaterialDesign
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;

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
            List<string> inputValues = new List<string>();
            Class.App.Input.AudioController.AudioController.AudioMode mode = Class.App.Input.AudioController.AudioController.AudioMode.None;

            if (RdBtnInputApp.Checked)
            {
                foreach (ListViewItem item in lvwApps.Items)
                {
                    inputValues.Add(item.Text);
                }

                mode = Class.App.Input.AudioController.AudioController.AudioMode.Application;
            }
            else if (rdbPlaybackDevices.Checked)
            {
                foreach (var InputTypeChkBox in cklDevices.Items)
                {
                    if (InputTypeChkBox.Checked)
                    {
                        inputValues.Add(InputTypeChkBox.Text.Split().Where(x => x.StartsWith("(") && x.EndsWith(")"))
                                .Select(x => x.Replace("(", string.Empty).Replace(")", string.Empty))
                                .ToList().Last());
                    }
                }

                mode = Class.App.Input.AudioController.AudioController.AudioMode.DevicePlayback;
            }

            _profile.Inputs[_IdInput].AudioController.Update(mode, inputValues);
            Save();
            Close();
        }

        private void Save()
        {
            _profile.Inputs[_IdInput].SetToAudioController();
            _profile.Inputs[_IdInput].Name = TxtInputName.Text;
            _profile.Inputs[_IdInput].Data = _profile.Inputs[_IdInput].AudioController.DataToJson();

            _profile.UpdateInput(_profile.Inputs[_IdInput]);
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

            // Criar lista de dispositivos DINAMICAMENTE ao invés de usar lista estática
            using (MMDeviceEnumerator enumerator = new MMDeviceEnumerator())
            {
                var activeDevices = enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);

                foreach (MMDevice device in activeDevices)
                {
                    cklDevices.Items.Add(device.FriendlyName + " - " + device.DeviceFriendlyName + " (" + device.ID + ")");
                }
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

            this.Text = "Input " + (_IdInput + 1) + " - Configurações";

            TxtInputName.Text = _profile.Inputs[_IdInput].Name;

            if (_profile.Inputs[_IdInput].Mode != Class.App.Input.Input.InputMode.AudioController || _profile.Inputs[_IdInput].AudioController == null)
            {
                _profile.Inputs[_IdInput].SetToAudioController();
            }

            switch(_profile.Inputs[_IdInput].AudioController.Mode)
            {
                case Class.App.Input.AudioController.AudioController.AudioMode.Application:
                    RdBtnInputApp.Checked = true;
                    lvwApps.Items.Clear();
                    foreach (string info in _profile.Inputs[_IdInput].AudioController.ToManage)
                    {
                        lvwApps.Items.Add(info);

                    }
                    ShowAppForms();
                    break;
                case Class.App.Input.AudioController.AudioController.AudioMode.DevicePlayback:
                    rdbPlaybackDevices.Checked = true;
                    String[] ChkArray = _profile.Inputs[_IdInput].AudioController.ToManage.ToArray();

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
                    ShowAppForms();
                    break;
            }

            ChkBoxInvertAxis.Checked = _profile.Inputs[_IdInput].AudioController.InvertedAxis;
        }

        private void DisableAllForms()
        {
            // Disable default forms
            BtnSaveInputConf.Enabled = false;
            BtnCancelInputConf.Enabled = false;
            ChkBoxInvertAxis.Enabled = false;
            RdBtnInputApp.Enabled = false;
            rdbPlaybackDevices.Enabled = false;

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
            rdbPlaybackDevices.Enabled = true;

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

            DialogResult insertExecutables = Pages.App.MessageBox.Show(this, "Inserção de listagem de executáveis", "Ao executarmos essa opção, TODOS os executáveis da pasta selecionada (incluindo suas subpastas) serão inseridos na lista do input '" + _profile.Inputs[_IdInput].Name + "', você deseja continuar o processo? Lembrando que pode ser que o JJManager pare de responder durante esse processo.", MessageBoxButtons.YesNo);

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
            JJManager.Pages.App.AudioSession audioSession = new JJManager.Pages.App.AudioSession(this, _profile.Inputs[_IdInput]);

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

            DialogResult result = Pages.App.MessageBox.Show(this, "Exclusão de itens", "Deseja mesmo deletar " + (itemsToExclude.Count > 1 ? "os " + itemsToExclude.Count + " items" : "o item") + " selecionado?", MessageBoxButtons.YesNo);

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
