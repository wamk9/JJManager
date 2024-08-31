namespace JJManager.Pages.App
{
    partial class AudioController
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AudioController));
            this.BtnSaveInputConf = new MaterialSkin.Controls.MaterialButton();
            this.BtnCancelInputConf = new MaterialSkin.Controls.MaterialButton();
            this.TxtInputName = new MaterialSkin.Controls.MaterialTextBox();
            this.RdBtnInputApp = new MaterialSkin.Controls.MaterialRadioButton();
            this.rdbPlaybackDevices = new MaterialSkin.Controls.MaterialRadioButton();
            this.ChkBoxInvertAxis = new MaterialSkin.Controls.MaterialCheckbox();
            this.btnAddFolder = new MaterialSkin.Controls.MaterialButton();
            this.btnAddExecutable = new MaterialSkin.Controls.MaterialButton();
            this.btnAddSessionActive = new MaterialSkin.Controls.MaterialButton();
            this.lvwApps = new MaterialSkin.Controls.MaterialListView();
            this.appPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnDeleteItems = new MaterialSkin.Controls.MaterialButton();
            this.cklDevices = new MaterialSkin.Controls.MaterialCheckedListBox();
            this.SuspendLayout();
            // 
            // BtnSaveInputConf
            // 
            this.BtnSaveInputConf.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnSaveInputConf.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnSaveInputConf.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnSaveInputConf.Depth = 0;
            this.BtnSaveInputConf.HighEmphasis = true;
            this.BtnSaveInputConf.Icon = null;
            this.BtnSaveInputConf.Location = new System.Drawing.Point(853, 549);
            this.BtnSaveInputConf.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnSaveInputConf.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnSaveInputConf.Name = "BtnSaveInputConf";
            this.BtnSaveInputConf.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnSaveInputConf.Size = new System.Drawing.Size(76, 36);
            this.BtnSaveInputConf.TabIndex = 2;
            this.BtnSaveInputConf.Text = "Salvar";
            this.BtnSaveInputConf.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnSaveInputConf.UseAccentColor = false;
            this.BtnSaveInputConf.UseVisualStyleBackColor = true;
            this.BtnSaveInputConf.Click += new System.EventHandler(this.BtnSaveInputConf_Click);
            // 
            // BtnCancelInputConf
            // 
            this.BtnCancelInputConf.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnCancelInputConf.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnCancelInputConf.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnCancelInputConf.Depth = 0;
            this.BtnCancelInputConf.HighEmphasis = true;
            this.BtnCancelInputConf.Icon = null;
            this.BtnCancelInputConf.Location = new System.Drawing.Point(749, 549);
            this.BtnCancelInputConf.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnCancelInputConf.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnCancelInputConf.Name = "BtnCancelInputConf";
            this.BtnCancelInputConf.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnCancelInputConf.Size = new System.Drawing.Size(96, 36);
            this.BtnCancelInputConf.TabIndex = 3;
            this.BtnCancelInputConf.Text = "Cancelar";
            this.BtnCancelInputConf.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnCancelInputConf.UseAccentColor = false;
            this.BtnCancelInputConf.UseVisualStyleBackColor = true;
            this.BtnCancelInputConf.Click += new System.EventHandler(this.BtnCancelInputConf_Click);
            // 
            // TxtInputName
            // 
            this.TxtInputName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtInputName.AnimateReadOnly = false;
            this.TxtInputName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TxtInputName.Depth = 0;
            this.TxtInputName.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TxtInputName.Hint = "Nome do Input";
            this.TxtInputName.LeadingIcon = null;
            this.TxtInputName.Location = new System.Drawing.Point(13, 76);
            this.TxtInputName.MaxLength = 15;
            this.TxtInputName.MouseState = MaterialSkin.MouseState.OUT;
            this.TxtInputName.Multiline = false;
            this.TxtInputName.Name = "TxtInputName";
            this.TxtInputName.Size = new System.Drawing.Size(916, 50);
            this.TxtInputName.TabIndex = 4;
            this.TxtInputName.Text = "";
            this.TxtInputName.TrailingIcon = null;
            // 
            // RdBtnInputApp
            // 
            this.RdBtnInputApp.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.RdBtnInputApp.AutoSize = true;
            this.RdBtnInputApp.Checked = true;
            this.RdBtnInputApp.Depth = 0;
            this.RdBtnInputApp.Location = new System.Drawing.Point(283, 141);
            this.RdBtnInputApp.Margin = new System.Windows.Forms.Padding(0);
            this.RdBtnInputApp.MouseLocation = new System.Drawing.Point(-1, -1);
            this.RdBtnInputApp.MouseState = MaterialSkin.MouseState.HOVER;
            this.RdBtnInputApp.Name = "RdBtnInputApp";
            this.RdBtnInputApp.Ripple = true;
            this.RdBtnInputApp.Size = new System.Drawing.Size(112, 37);
            this.RdBtnInputApp.TabIndex = 6;
            this.RdBtnInputApp.TabStop = true;
            this.RdBtnInputApp.Text = "Aplicações";
            this.RdBtnInputApp.UseVisualStyleBackColor = true;
            this.RdBtnInputApp.CheckedChanged += new System.EventHandler(this.RdBtnInput_CheckedChanged);
            // 
            // rdbPlaybackDevices
            // 
            this.rdbPlaybackDevices.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.rdbPlaybackDevices.AutoSize = true;
            this.rdbPlaybackDevices.Depth = 0;
            this.rdbPlaybackDevices.Location = new System.Drawing.Point(412, 141);
            this.rdbPlaybackDevices.Margin = new System.Windows.Forms.Padding(0);
            this.rdbPlaybackDevices.MouseLocation = new System.Drawing.Point(-1, -1);
            this.rdbPlaybackDevices.MouseState = MaterialSkin.MouseState.HOVER;
            this.rdbPlaybackDevices.Name = "rdbPlaybackDevices";
            this.rdbPlaybackDevices.Ripple = true;
            this.rdbPlaybackDevices.Size = new System.Drawing.Size(250, 37);
            this.rdbPlaybackDevices.TabIndex = 7;
            this.rdbPlaybackDevices.TabStop = true;
            this.rdbPlaybackDevices.Text = "Canais de Áudio (Reprodução)";
            this.rdbPlaybackDevices.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.rdbPlaybackDevices.UseVisualStyleBackColor = true;
            // 
            // ChkBoxInvertAxis
            // 
            this.ChkBoxInvertAxis.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ChkBoxInvertAxis.AutoSize = true;
            this.ChkBoxInvertAxis.Depth = 0;
            this.ChkBoxInvertAxis.Location = new System.Drawing.Point(13, 547);
            this.ChkBoxInvertAxis.Margin = new System.Windows.Forms.Padding(0);
            this.ChkBoxInvertAxis.MouseLocation = new System.Drawing.Point(-1, -1);
            this.ChkBoxInvertAxis.MouseState = MaterialSkin.MouseState.HOVER;
            this.ChkBoxInvertAxis.Name = "ChkBoxInvertAxis";
            this.ChkBoxInvertAxis.ReadOnly = false;
            this.ChkBoxInvertAxis.Ripple = true;
            this.ChkBoxInvertAxis.Size = new System.Drawing.Size(121, 37);
            this.ChkBoxInvertAxis.TabIndex = 10;
            this.ChkBoxInvertAxis.Text = "Inverter Eixo";
            this.ChkBoxInvertAxis.UseVisualStyleBackColor = true;
            // 
            // btnAddFolder
            // 
            this.btnAddFolder.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnAddFolder.AutoSize = false;
            this.btnAddFolder.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAddFolder.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnAddFolder.Depth = 0;
            this.btnAddFolder.HighEmphasis = true;
            this.btnAddFolder.Icon = null;
            this.btnAddFolder.Location = new System.Drawing.Point(7, 185);
            this.btnAddFolder.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnAddFolder.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnAddFolder.Name = "btnAddFolder";
            this.btnAddFolder.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnAddFolder.Size = new System.Drawing.Size(225, 53);
            this.btnAddFolder.TabIndex = 15;
            this.btnAddFolder.Text = "Adicionar executáveis de pasta";
            this.btnAddFolder.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnAddFolder.UseAccentColor = false;
            this.btnAddFolder.UseVisualStyleBackColor = true;
            this.btnAddFolder.Click += new System.EventHandler(this.btnAddFolder_Click);
            // 
            // btnAddExecutable
            // 
            this.btnAddExecutable.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnAddExecutable.AutoSize = false;
            this.btnAddExecutable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAddExecutable.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnAddExecutable.Depth = 0;
            this.btnAddExecutable.HighEmphasis = true;
            this.btnAddExecutable.Icon = null;
            this.btnAddExecutable.Location = new System.Drawing.Point(240, 185);
            this.btnAddExecutable.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnAddExecutable.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnAddExecutable.Name = "btnAddExecutable";
            this.btnAddExecutable.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnAddExecutable.Size = new System.Drawing.Size(225, 53);
            this.btnAddExecutable.TabIndex = 15;
            this.btnAddExecutable.Text = "Adicionar Executável Individualmente";
            this.btnAddExecutable.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnAddExecutable.UseAccentColor = false;
            this.btnAddExecutable.UseVisualStyleBackColor = true;
            this.btnAddExecutable.Click += new System.EventHandler(this.btnAddExecutable_Click);
            // 
            // btnAddSessionActive
            // 
            this.btnAddSessionActive.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnAddSessionActive.AutoSize = false;
            this.btnAddSessionActive.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAddSessionActive.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnAddSessionActive.Depth = 0;
            this.btnAddSessionActive.HighEmphasis = true;
            this.btnAddSessionActive.Icon = null;
            this.btnAddSessionActive.Location = new System.Drawing.Point(473, 185);
            this.btnAddSessionActive.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnAddSessionActive.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnAddSessionActive.Name = "btnAddSessionActive";
            this.btnAddSessionActive.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnAddSessionActive.Size = new System.Drawing.Size(225, 53);
            this.btnAddSessionActive.TabIndex = 14;
            this.btnAddSessionActive.Text = "Adicionar Executável via Sessão de Áudio";
            this.btnAddSessionActive.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnAddSessionActive.UseAccentColor = false;
            this.btnAddSessionActive.UseVisualStyleBackColor = true;
            this.btnAddSessionActive.Click += new System.EventHandler(this.btnAddSessionActive_Click);
            // 
            // lvwApps
            // 
            this.lvwApps.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lvwApps.AutoSizeTable = false;
            this.lvwApps.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.lvwApps.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvwApps.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.appPath});
            this.lvwApps.Depth = 0;
            this.lvwApps.FullRowSelect = true;
            this.lvwApps.HideSelection = false;
            this.lvwApps.Location = new System.Drawing.Point(7, 247);
            this.lvwApps.MinimumSize = new System.Drawing.Size(200, 100);
            this.lvwApps.MouseLocation = new System.Drawing.Point(-1, -1);
            this.lvwApps.MouseState = MaterialSkin.MouseState.OUT;
            this.lvwApps.Name = "lvwApps";
            this.lvwApps.OwnerDraw = true;
            this.lvwApps.Size = new System.Drawing.Size(924, 297);
            this.lvwApps.TabIndex = 16;
            this.lvwApps.UseCompatibleStateImageBehavior = false;
            this.lvwApps.View = System.Windows.Forms.View.Details;
            this.lvwApps.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lvwApps_ItemSelectionChanged);
            // 
            // appPath
            // 
            this.appPath.Text = "Aplicativos controlados";
            this.appPath.Width = 907;
            // 
            // btnDeleteItems
            // 
            this.btnDeleteItems.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnDeleteItems.AutoSize = false;
            this.btnDeleteItems.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnDeleteItems.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnDeleteItems.Depth = 0;
            this.btnDeleteItems.Enabled = false;
            this.btnDeleteItems.HighEmphasis = true;
            this.btnDeleteItems.Icon = null;
            this.btnDeleteItems.Location = new System.Drawing.Point(706, 185);
            this.btnDeleteItems.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnDeleteItems.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnDeleteItems.Name = "btnDeleteItems";
            this.btnDeleteItems.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnDeleteItems.Size = new System.Drawing.Size(225, 53);
            this.btnDeleteItems.TabIndex = 17;
            this.btnDeleteItems.Text = "Excluir itens selecionados";
            this.btnDeleteItems.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnDeleteItems.UseAccentColor = false;
            this.btnDeleteItems.UseVisualStyleBackColor = true;
            this.btnDeleteItems.Click += new System.EventHandler(this.btnDeleteItems_Click);
            // 
            // cklDevices
            // 
            this.cklDevices.AutoScroll = true;
            this.cklDevices.BackColor = System.Drawing.SystemColors.Control;
            this.cklDevices.Depth = 0;
            this.cklDevices.Location = new System.Drawing.Point(7, 185);
            this.cklDevices.MouseState = MaterialSkin.MouseState.HOVER;
            this.cklDevices.Name = "cklDevices";
            this.cklDevices.Size = new System.Drawing.Size(924, 359);
            this.cklDevices.Striped = false;
            this.cklDevices.StripeDarkColor = System.Drawing.Color.Empty;
            this.cklDevices.TabIndex = 18;
            // 
            // AudioController
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(942, 600);
            this.Controls.Add(this.cklDevices);
            this.Controls.Add(this.btnDeleteItems);
            this.Controls.Add(this.lvwApps);
            this.Controls.Add(this.btnAddSessionActive);
            this.Controls.Add(this.btnAddExecutable);
            this.Controls.Add(this.ChkBoxInvertAxis);
            this.Controls.Add(this.btnAddFolder);
            this.Controls.Add(this.rdbPlaybackDevices);
            this.Controls.Add(this.RdBtnInputApp);
            this.Controls.Add(this.TxtInputName);
            this.Controls.Add(this.BtnCancelInputConf);
            this.Controls.Add(this.BtnSaveInputConf);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(700, 600);
            this.Name = "AudioController";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Input 1 - Configurações";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.ChangeInputInfo_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MaterialSkin.Controls.MaterialButton BtnSaveInputConf;
        private MaterialSkin.Controls.MaterialButton BtnCancelInputConf;
        private MaterialSkin.Controls.MaterialTextBox TxtInputName;
        private MaterialSkin.Controls.MaterialRadioButton RdBtnInputApp;
        private MaterialSkin.Controls.MaterialRadioButton rdbPlaybackDevices;
        private MaterialSkin.Controls.MaterialCheckbox ChkBoxInvertAxis;
        private MaterialSkin.Controls.MaterialButton btnAddFolder;
        private MaterialSkin.Controls.MaterialButton btnAddExecutable;
        private MaterialSkin.Controls.MaterialButton btnAddSessionActive;
        private MaterialSkin.Controls.MaterialListView lvwApps;
        public System.Windows.Forms.ColumnHeader appPath;
        private MaterialSkin.Controls.MaterialButton btnDeleteItems;
        private MaterialSkin.Controls.MaterialCheckedListBox cklDevices;
    }
}