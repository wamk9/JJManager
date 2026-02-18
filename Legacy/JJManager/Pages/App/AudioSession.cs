using JJManager.Class;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using ConfigClass = JJManager.Class.App.Config.Config;

namespace JJManager.Pages.App
{
    public partial class AudioSession : MaterialForm
    {
        private static DatabaseConnection _DatabaseConnection = new DatabaseConnection();
        private MaterialSkinManager materialSkinManager = null;
        private string[] _executableOfSessions = null;
        Thread thread = null;
        JJManager.Class.AppModulesTimer _appModulesTimer = null;
        JJManager.Class.App.Input.Input _input = null;
        MaterialForm _parent = null;

        public string[] ExecutableOfSessions
        {
            get => _executableOfSessions;
        }


        public AudioSession(MaterialForm parent, JJManager.Class.App.Input.Input input)
        {
            InitializeComponent();

            _input = input;
            _parent = parent;

            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;

            FormClosing += AudioSession_FormClosing;
            Shown += AudioSession_Shown;
        }

        private void UpdateAudioSessionsList_tick(object sender, EventArgs e)
        {
            updateAudioSessionsList();
        }
        private void AudioSession_FormClosing(object sender, FormClosingEventArgs e)
        {
            _parent.Visible = true;
        }

        private void updateAudioSessionsList()
        {
            ListViewItem newItem = null;
            List<string> pidsToExclude = new List<string>();
            int actualIndex = -1;
            bool isNewItem = false;

            foreach (ListViewItem item in lvwAudioSessions.Items) 
            {
                pidsToExclude.Add(item.SubItems[0].Text);
            }

            foreach (string[] session in _input.AudioController.GetSessions())
            {
                newItem = new ListViewItem(session);
                isNewItem = true;

                foreach (ListViewItem oldItem in lvwAudioSessions.Items)
                {
                    if (oldItem.SubItems[0].Text == newItem.SubItems[0].Text)
                    {
                        actualIndex = lvwAudioSessions.Items.IndexOf(oldItem);
                        lvwAudioSessions.Items[actualIndex].SubItems[3].Text = session[3];
                        pidsToExclude.Remove(lvwAudioSessions.Items[actualIndex].SubItems[0].Text);
                        actualIndex = -1;
                        isNewItem = false;
                        break;
                    }
                }

                if (isNewItem)
                {
                    lvwAudioSessions.Items.Add(newItem);
                    isNewItem = false;
                }
            }

            foreach (ListViewItem itemToRemove in lvwAudioSessions.Items)
            {
                foreach (string pid in pidsToExclude)
                {
                    if (itemToRemove.SubItems[0].Text == pid)
                    {
                        actualIndex = lvwAudioSessions.Items.IndexOf(itemToRemove);
                        lvwAudioSessions.Items.RemoveAt(actualIndex);
                        pidsToExclude.Remove(pid);
                        actualIndex = -1;
                        break;
                    }
                }
            }
        }

        private void AudioSession_Shown(object sender, EventArgs e)
        {
            updateAudioSessionsList();
        }

        private void btnAddExecutable_Click(object sender, EventArgs e)
        {
            thread = null;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            thread = null;
            _executableOfSessions = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void lvwAudioSessions_SelectedIndexChanged(object sender, EventArgs e)
        {
            List<string> executables = new List<string>();

            foreach (ListViewItem item in lvwAudioSessions.Items)
            {
                if (item.Selected)
                {
                    executables.Add(item.SubItems[2].Text);
                }
            }

            btnAddExecutable.Enabled = (executables.Count > 0 ? true : false);
            _executableOfSessions = executables.ToArray();
        }

        private void AudioSession_Load(object sender, EventArgs e)
        {

        }
    }
}
