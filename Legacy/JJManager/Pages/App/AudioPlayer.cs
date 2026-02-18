using JJManager.Class;
using MaterialSkin.Controls;
using MaterialSkin;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using ConfigClass = JJManager.Class.App.Config.Config;
using AudioPlayerClass = JJManager.Class.App.Input.AudioPlayer.AudioPlayer;
using ProfileClass = JJManager.Class.App.Profile.Profile;

namespace JJManager.Pages.App
{
    public partial class AudioPlayer : MaterialForm
    {

        Dictionary<string, string> inputParams = new Dictionary<string, string>();
        JJManager.Class.App.WaitForm waitForm = new JJManager.Class.App.WaitForm();
        private int _IdInput = -1;
        private string _audioPath = null;
        private string _audioFileName = null;
        private ProfileClass _profile = null;
        private MaterialForm _parent = null;
        private bool _audioChanged = false;
        private Thread threadAudioSession = null;
        private string _actualInputName = null;

        #region WinForms
        private MaterialSkinManager materialSkinManager = null;
        #endregion

        public AudioPlayer(MaterialForm parent, ProfileClass profile, int idInput)
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

            _actualInputName = _profile.Inputs[_IdInput].Name;
            TxtInputName.Text = _actualInputName;

            UpdateFormActivation();

            flpDragDrop.BorderStyle = BorderStyle.FixedSingle; // Optional to have fixed single first
            flpDragDrop.BackColor = materialSkinManager.BackdropColor; // Change color to indicate drag
            ControlPaint.DrawBorder(flpDragDrop.CreateGraphics(), flpDragDrop.ClientRectangle,
                                    Color.Gray, 2, ButtonBorderStyle.Dashed,
                                    Color.Gray, 2, ButtonBorderStyle.Dashed,
                                    Color.Gray, 2, ButtonBorderStyle.Dashed,
                                    Color.Gray, 2, ButtonBorderStyle.Dashed);


            // Events
            FormClosed += AudioPlayer_FormClosed;
            FormClosing += AudioPlayer_FormClosing;
            flpDragDrop.DragDrop += FlpDragDrop_DragDrop;
            flpDragDrop.DragEnter += FlpDragDrop_DragEnter;
            TxtInputName.TextChanged += TxtInputName_TextChanged;
        }

        private void TxtInputName_TextChanged(object sender, EventArgs e)
        {
            UpdateFormActivation();
        }

        private void FlpDragDrop_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length == 1 && Path.GetExtension(files[0]).Equals(".mp3", StringComparison.OrdinalIgnoreCase))
                {
                    e.Effect = DragDropEffects.Copy;

                    // Change the border style to a dotted border
                    flpDragDrop.BorderStyle = BorderStyle.FixedSingle; // Optional to have fixed single first
                    flpDragDrop.BackColor = materialSkinManager.BackdropColor; // Change color to indicate drag
                    ControlPaint.DrawBorder(flpDragDrop.CreateGraphics(), flpDragDrop.ClientRectangle,
                                            Color.Gray, 10, ButtonBorderStyle.Dashed,
                                            Color.Gray, 10, ButtonBorderStyle.Dashed,
                                            Color.Gray, 10, ButtonBorderStyle.Dashed,
                                            Color.Gray, 10, ButtonBorderStyle.Dashed);

                    _audioChanged = true;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                    // Provide feedback to the user
                    Pages.App.MessageBox.Show(this, "Formato Inválido", "O JJManager aceita apenas arquivos no formato '.mp3' para execução de áudio.");
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void FlpDragDrop_DragDrop(object sender, DragEventArgs e)
        {
            _audioPath = ((string[]) e.Data.GetData(DataFormats.FileDrop))[0];
            _audioFileName = _audioPath.Split('\\').Last().Split('.').First();
            UpdateFormActivation();
        }

        private void UpdateFormActivation()
        {
            btnPlayActualAudio.Enabled = (_profile.Inputs[_IdInput].AudioPlayer != null && _profile.Inputs[_IdInput].AudioPlayer.HasAudio);
            btnPlaySelectedAudio.Enabled = (_audioPath != null && File.Exists(_audioPath));
            btnRemoveActualAudio.Enabled = (_profile.Inputs[_IdInput].AudioPlayer != null && _profile.Inputs[_IdInput].AudioPlayer.HasAudio);
            btnSaveAndClose.Enabled = _audioChanged || (_actualInputName != TxtInputName.Text);

            DragDropUI();
        }

        private void DragDropUI()
        {
            flpDragDrop.Controls.Clear();
            MaterialLabel label = new MaterialLabel();
            label.Name = "txtDragDrop";

            label.TextAlign = ContentAlignment.MiddleCenter;
            label.AutoSize = false;
            label.Width = flpDragDrop.Width;
            label.Height = flpDragDrop.Height;

            if (_audioFileName == null)
            {
                label.Text = "Nenhum Áudio Selecionado\n\nArraste o arquivo correspondente ao áudio desejado para dentro do pontilhado ou clique aqui";
            }
            else
            {
                label.Text = "Áudio '" + _audioFileName + "' Selecionado\n\nPara substituir este áudio por outro, arraste o arquivo correspondente para dentro do pontilhado ou clique aqui";
            }

            label.Anchor = AnchorStyles.None;
            label.Margin = new Padding(
                (flpDragDrop.ClientSize.Width - label.Width) / 2,  // Center horizontally
                (flpDragDrop.ClientSize.Height - label.Height) / 2,  // Center vertically
                0,
                0
            );

            label.Click += Label_Click;
            flpDragDrop.Controls.Add(label);
        }

        private void Label_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Arquivo de Audio|*.mp3";
            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK && openFileDialog.FileName != string.Empty)
            {
                _audioFileName = openFileDialog.SafeFileName.Split('.')[0];
                _audioPath = openFileDialog.FileName;
                _profile.Inputs[_IdInput].AudioPlayer.SetAudio(_audioPath);
                _audioChanged = true;
                UpdateFormActivation();
            }
        }

        private void AudioPlayer_FormClosing(object sender, FormClosingEventArgs e)
        {
            _parent.Visible = true;
        }

        private void AudioPlayer_FormClosed(object sender, FormClosedEventArgs e)
        {
            Enabled = false;
        }

        private void btnPlayActualAudio_Click(object sender, EventArgs e)
        {
            if (_profile.Inputs[_IdInput].AudioPlayer != null && _profile.Inputs[_IdInput].AudioPlayer.HasAudio)
            {
                _profile.Inputs[_IdInput].AudioPlayer.PlayAudio();
            }
        }

        private void btnPlaySelectedAudio_Click(object sender, EventArgs e)
        {
            AudioPlayerClass.PlayAudioIndependent(_audioPath);
        }

        private void btnRemoveActualAudio_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = Pages.App.MessageBox.Show(this, "Deletar Áudio Atual", "Ao realizar a exclusão do áudio e salvar as informações, o mesmo não poderá ser recuperado, tem certeza que deseja continuar?", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes && _profile.Inputs[_IdInput].AudioPlayer != null)
            {
                _profile.Inputs[_IdInput].AudioPlayer.RemoveAudio();
                _audioChanged = true;
                UpdateFormActivation();
            }
        }

        private void btnSaveAndClose_Click(object sender, EventArgs e)
        {
            _profile.Inputs[_IdInput].AudioPlayer.Update();
            _profile.Inputs[_IdInput].Name = TxtInputName.Text;
            _profile.Inputs[_IdInput].Save();
            Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
