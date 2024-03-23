namespace JJManager
{
    partial class Main
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.timerSerialComUpdate = new System.Windows.Forms.Timer(this.components);
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.materialTabControl1 = new MaterialSkin.Controls.MaterialTabControl();
            this.tabConnect = new System.Windows.Forms.TabPage();
            this.btnConnChanger = new MaterialSkin.Controls.MaterialButton();
            this.btnEditDevice = new MaterialSkin.Controls.MaterialButton();
            this.btnAddDevice = new MaterialSkin.Controls.MaterialButton();
            this.materialDivider1 = new MaterialSkin.Controls.MaterialDivider();
            this.lvDevices = new MaterialSkin.Controls.MaterialListView();
            this.lvhDeviceId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvhDeviceName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvhConnType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvhConnStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabOptions = new System.Windows.Forms.TabPage();
            this.LblThemeColor = new MaterialSkin.Controls.MaterialLabel();
            this.SwtThemeColor = new MaterialSkin.Controls.MaterialSwitch();
            this.tabUpdate = new System.Windows.Forms.TabPage();
            this.BtnUpdateDevice = new MaterialSkin.Controls.MaterialButton();
            this.BtnUpdateSoftware = new MaterialSkin.Controls.MaterialButton();
            this.tabAbout = new System.Windows.Forms.TabPage();
            this.lblAboutText = new MaterialSkin.Controls.MaterialLabel();
            this.lblAboutVersion = new MaterialSkin.Controls.MaterialLabel();
            this.ImgAbout = new System.Windows.Forms.PictureBox();
            this.tbsMainMenu = new MaterialSkin.Controls.MaterialTabSelector();
            this.materialTabControl1.SuspendLayout();
            this.tabConnect.SuspendLayout();
            this.tabOptions.SuspendLayout();
            this.tabUpdate.SuspendLayout();
            this.tabAbout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImgAbout)).BeginInit();
            this.SuspendLayout();
            // 
            // timerSerialComUpdate
            // 
            this.timerSerialComUpdate.Enabled = true;
            this.timerSerialComUpdate.Interval = 2000;
            this.timerSerialComUpdate.Tick += new System.EventHandler(this.timerSerialComUpdate_Tick);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "JJManager_icones_update_16.png");
            this.imageList.Images.SetKeyName(1, "JJManager_icones_connect.png");
            this.imageList.Images.SetKeyName(2, "JJManager_icones_options_16.png");
            // 
            // materialTabControl1
            // 
            this.materialTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.materialTabControl1.Controls.Add(this.tabConnect);
            this.materialTabControl1.Controls.Add(this.tabOptions);
            this.materialTabControl1.Controls.Add(this.tabUpdate);
            this.materialTabControl1.Controls.Add(this.tabAbout);
            this.materialTabControl1.Depth = 0;
            this.materialTabControl1.ImageList = this.imageList;
            this.materialTabControl1.Location = new System.Drawing.Point(3, 118);
            this.materialTabControl1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialTabControl1.Multiline = true;
            this.materialTabControl1.Name = "materialTabControl1";
            this.materialTabControl1.SelectedIndex = 0;
            this.materialTabControl1.Size = new System.Drawing.Size(712, 400);
            this.materialTabControl1.TabIndex = 21;
            // 
            // tabConnect
            // 
            this.tabConnect.Controls.Add(this.btnConnChanger);
            this.tabConnect.Controls.Add(this.btnEditDevice);
            this.tabConnect.Controls.Add(this.btnAddDevice);
            this.tabConnect.Controls.Add(this.materialDivider1);
            this.tabConnect.Controls.Add(this.lvDevices);
            this.tabConnect.ImageKey = "JJManager_icones_connect.png";
            this.tabConnect.Location = new System.Drawing.Point(4, 23);
            this.tabConnect.Name = "tabConnect";
            this.tabConnect.Padding = new System.Windows.Forms.Padding(3);
            this.tabConnect.Size = new System.Drawing.Size(704, 373);
            this.tabConnect.TabIndex = 0;
            this.tabConnect.Text = "Gerenciar Dispositivos";
            this.tabConnect.UseVisualStyleBackColor = true;
            // 
            // btnConnChanger
            // 
            this.btnConnChanger.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConnChanger.AutoSize = false;
            this.btnConnChanger.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnConnChanger.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnConnChanger.Depth = 0;
            this.btnConnChanger.Enabled = false;
            this.btnConnChanger.HighEmphasis = true;
            this.btnConnChanger.Icon = null;
            this.btnConnChanger.Location = new System.Drawing.Point(7, 325);
            this.btnConnChanger.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnConnChanger.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnConnChanger.Name = "btnConnChanger";
            this.btnConnChanger.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnConnChanger.Size = new System.Drawing.Size(218, 36);
            this.btnConnChanger.TabIndex = 5;
            this.btnConnChanger.Text = "Conectar/Desconectar";
            this.btnConnChanger.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnConnChanger.UseAccentColor = false;
            this.btnConnChanger.UseVisualStyleBackColor = true;
            this.btnConnChanger.Click += new System.EventHandler(this.btnConnChanger_Click);
            // 
            // btnEditDevice
            // 
            this.btnEditDevice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEditDevice.AutoSize = false;
            this.btnEditDevice.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnEditDevice.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnEditDevice.Depth = 0;
            this.btnEditDevice.Enabled = false;
            this.btnEditDevice.HighEmphasis = true;
            this.btnEditDevice.Icon = null;
            this.btnEditDevice.Location = new System.Drawing.Point(243, 325);
            this.btnEditDevice.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnEditDevice.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnEditDevice.Name = "btnEditDevice";
            this.btnEditDevice.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnEditDevice.Size = new System.Drawing.Size(218, 36);
            this.btnEditDevice.TabIndex = 4;
            this.btnEditDevice.Text = "Editar Dispositivo";
            this.btnEditDevice.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnEditDevice.UseAccentColor = false;
            this.btnEditDevice.UseVisualStyleBackColor = true;
            this.btnEditDevice.Click += new System.EventHandler(this.btnEditDevice_Click);
            // 
            // btnAddDevice
            // 
            this.btnAddDevice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddDevice.AutoSize = false;
            this.btnAddDevice.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAddDevice.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnAddDevice.Depth = 0;
            this.btnAddDevice.Enabled = false;
            this.btnAddDevice.HighEmphasis = true;
            this.btnAddDevice.Icon = null;
            this.btnAddDevice.Location = new System.Drawing.Point(479, 325);
            this.btnAddDevice.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnAddDevice.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnAddDevice.Name = "btnAddDevice";
            this.btnAddDevice.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnAddDevice.Size = new System.Drawing.Size(218, 36);
            this.btnAddDevice.TabIndex = 3;
            this.btnAddDevice.Text = "Adicionar Manualmente";
            this.btnAddDevice.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnAddDevice.UseAccentColor = false;
            this.btnAddDevice.UseVisualStyleBackColor = true;
            // 
            // materialDivider1
            // 
            this.materialDivider1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialDivider1.Depth = 0;
            this.materialDivider1.Location = new System.Drawing.Point(6, 339);
            this.materialDivider1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialDivider1.Name = "materialDivider1";
            this.materialDivider1.Size = new System.Drawing.Size(692, 1);
            this.materialDivider1.TabIndex = 2;
            this.materialDivider1.Text = "materialDivider1";
            // 
            // lvDevices
            // 
            this.lvDevices.Alignment = System.Windows.Forms.ListViewAlignment.SnapToGrid;
            this.lvDevices.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvDevices.AutoArrange = false;
            this.lvDevices.AutoSizeTable = false;
            this.lvDevices.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.lvDevices.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvDevices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.lvhDeviceId,
            this.lvhDeviceName,
            this.lvhConnType,
            this.lvhConnStatus});
            this.lvDevices.Depth = 0;
            this.lvDevices.FullRowSelect = true;
            this.lvDevices.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvDevices.HideSelection = false;
            this.lvDevices.Location = new System.Drawing.Point(6, 6);
            this.lvDevices.MinimumSize = new System.Drawing.Size(200, 100);
            this.lvDevices.MouseLocation = new System.Drawing.Point(-1, -1);
            this.lvDevices.MouseState = MaterialSkin.MouseState.OUT;
            this.lvDevices.MultiSelect = false;
            this.lvDevices.Name = "lvDevices";
            this.lvDevices.OwnerDraw = true;
            this.lvDevices.Size = new System.Drawing.Size(692, 304);
            this.lvDevices.TabIndex = 1;
            this.lvDevices.UseCompatibleStateImageBehavior = false;
            this.lvDevices.View = System.Windows.Forms.View.Details;
            this.lvDevices.SelectedIndexChanged += new System.EventHandler(this.lvDevices_SelectedIndexChanged);
            // 
            // lvhDeviceId
            // 
            this.lvhDeviceId.Text = "ID";
            this.lvhDeviceId.Width = 100;
            // 
            // lvhDeviceName
            // 
            this.lvhDeviceName.Text = "Dispositivo";
            this.lvhDeviceName.Width = 350;
            // 
            // lvhConnType
            // 
            this.lvhConnType.Text = "Conexão";
            this.lvhConnType.Width = 100;
            // 
            // lvhConnStatus
            // 
            this.lvhConnStatus.Text = "Status";
            this.lvhConnStatus.Width = 150;
            // 
            // tabOptions
            // 
            this.tabOptions.Controls.Add(this.LblThemeColor);
            this.tabOptions.Controls.Add(this.SwtThemeColor);
            this.tabOptions.ImageKey = "JJManager_icones_options_16.png";
            this.tabOptions.Location = new System.Drawing.Point(4, 23);
            this.tabOptions.Name = "tabOptions";
            this.tabOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabOptions.Size = new System.Drawing.Size(704, 373);
            this.tabOptions.TabIndex = 1;
            this.tabOptions.Text = "Opções";
            this.tabOptions.UseVisualStyleBackColor = true;
            // 
            // LblThemeColor
            // 
            this.LblThemeColor.AutoSize = true;
            this.LblThemeColor.Depth = 0;
            this.LblThemeColor.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.LblThemeColor.Location = new System.Drawing.Point(7, 7);
            this.LblThemeColor.MouseState = MaterialSkin.MouseState.HOVER;
            this.LblThemeColor.Name = "LblThemeColor";
            this.LblThemeColor.Size = new System.Drawing.Size(92, 19);
            this.LblThemeColor.TabIndex = 1;
            this.LblThemeColor.Text = "Cor do Tema";
            // 
            // SwtThemeColor
            // 
            this.SwtThemeColor.AutoSize = true;
            this.SwtThemeColor.Checked = true;
            this.SwtThemeColor.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SwtThemeColor.Depth = 0;
            this.SwtThemeColor.Location = new System.Drawing.Point(3, 26);
            this.SwtThemeColor.Margin = new System.Windows.Forms.Padding(0);
            this.SwtThemeColor.MouseLocation = new System.Drawing.Point(-1, -1);
            this.SwtThemeColor.MouseState = MaterialSkin.MouseState.HOVER;
            this.SwtThemeColor.Name = "SwtThemeColor";
            this.SwtThemeColor.Ripple = true;
            this.SwtThemeColor.Size = new System.Drawing.Size(106, 37);
            this.SwtThemeColor.TabIndex = 0;
            this.SwtThemeColor.Text = "Escuro";
            this.SwtThemeColor.UseVisualStyleBackColor = true;
            this.SwtThemeColor.CheckedChanged += new System.EventHandler(this.SwtThemeColor_CheckedChanged);
            // 
            // tabUpdate
            // 
            this.tabUpdate.Controls.Add(this.BtnUpdateDevice);
            this.tabUpdate.Controls.Add(this.BtnUpdateSoftware);
            this.tabUpdate.ImageKey = "JJManager_icones_update_16.png";
            this.tabUpdate.Location = new System.Drawing.Point(4, 23);
            this.tabUpdate.Name = "tabUpdate";
            this.tabUpdate.Size = new System.Drawing.Size(704, 373);
            this.tabUpdate.TabIndex = 2;
            this.tabUpdate.Text = "Atualizações";
            this.tabUpdate.UseVisualStyleBackColor = true;
            // 
            // BtnUpdateDevice
            // 
            this.BtnUpdateDevice.AutoSize = false;
            this.BtnUpdateDevice.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnUpdateDevice.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnUpdateDevice.Depth = 0;
            this.BtnUpdateDevice.Enabled = false;
            this.BtnUpdateDevice.HighEmphasis = true;
            this.BtnUpdateDevice.Icon = null;
            this.BtnUpdateDevice.Location = new System.Drawing.Point(102, 199);
            this.BtnUpdateDevice.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnUpdateDevice.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnUpdateDevice.Name = "BtnUpdateDevice";
            this.BtnUpdateDevice.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnUpdateDevice.Size = new System.Drawing.Size(500, 50);
            this.BtnUpdateDevice.TabIndex = 3;
            this.BtnUpdateDevice.Text = "Enviar Firmware";
            this.BtnUpdateDevice.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnUpdateDevice.UseAccentColor = false;
            this.BtnUpdateDevice.UseVisualStyleBackColor = true;
            this.BtnUpdateDevice.Click += new System.EventHandler(this.BtnUpdateDevice_Click);
            // 
            // BtnUpdateSoftware
            // 
            this.BtnUpdateSoftware.AutoSize = false;
            this.BtnUpdateSoftware.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnUpdateSoftware.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnUpdateSoftware.Depth = 0;
            this.BtnUpdateSoftware.Enabled = false;
            this.BtnUpdateSoftware.HighEmphasis = true;
            this.BtnUpdateSoftware.Icon = null;
            this.BtnUpdateSoftware.Location = new System.Drawing.Point(102, 96);
            this.BtnUpdateSoftware.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnUpdateSoftware.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnUpdateSoftware.Name = "BtnUpdateSoftware";
            this.BtnUpdateSoftware.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnUpdateSoftware.Size = new System.Drawing.Size(500, 50);
            this.BtnUpdateSoftware.TabIndex = 1;
            this.BtnUpdateSoftware.Text = "Versão 1.1.13 Instalada";
            this.BtnUpdateSoftware.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnUpdateSoftware.UseAccentColor = false;
            this.BtnUpdateSoftware.UseVisualStyleBackColor = true;
            this.BtnUpdateSoftware.Click += new System.EventHandler(this.BtnUpdateSoftware_Click);
            // 
            // tabAbout
            // 
            this.tabAbout.Controls.Add(this.lblAboutText);
            this.tabAbout.Controls.Add(this.lblAboutVersion);
            this.tabAbout.Controls.Add(this.ImgAbout);
            this.tabAbout.Location = new System.Drawing.Point(4, 23);
            this.tabAbout.Name = "tabAbout";
            this.tabAbout.Size = new System.Drawing.Size(704, 373);
            this.tabAbout.TabIndex = 3;
            this.tabAbout.Text = "Sobre";
            this.tabAbout.UseVisualStyleBackColor = true;
            // 
            // lblAboutText
            // 
            this.lblAboutText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAboutText.Depth = 0;
            this.lblAboutText.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblAboutText.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lblAboutText.Location = new System.Drawing.Point(3, 150);
            this.lblAboutText.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblAboutText.Name = "lblAboutText";
            this.lblAboutText.Size = new System.Drawing.Size(698, 126);
            this.lblAboutText.TabIndex = 3;
            this.lblAboutText.Text = resources.GetString("lblAboutText.Text");
            this.lblAboutText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAboutVersion
            // 
            this.lblAboutVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAboutVersion.Depth = 0;
            this.lblAboutVersion.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblAboutVersion.Location = new System.Drawing.Point(3, 114);
            this.lblAboutVersion.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblAboutVersion.Name = "lblAboutVersion";
            this.lblAboutVersion.Size = new System.Drawing.Size(698, 26);
            this.lblAboutVersion.TabIndex = 2;
            this.lblAboutVersion.Text = "JJManager Versão 1.1.13";
            this.lblAboutVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ImgAbout
            // 
            this.ImgAbout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ImgAbout.BackgroundImage = global::JJManager.Properties.Resources.Logo_JohnJohn_JJMixer;
            this.ImgAbout.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ImgAbout.Location = new System.Drawing.Point(3, 8);
            this.ImgAbout.Name = "ImgAbout";
            this.ImgAbout.Size = new System.Drawing.Size(699, 103);
            this.ImgAbout.TabIndex = 0;
            this.ImgAbout.TabStop = false;
            // 
            // tbsMainMenu
            // 
            this.tbsMainMenu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbsMainMenu.BaseTabControl = this.materialTabControl1;
            this.tbsMainMenu.CharacterCasing = MaterialSkin.Controls.MaterialTabSelector.CustomCharacterCasing.Normal;
            this.tbsMainMenu.Depth = 0;
            this.tbsMainMenu.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.tbsMainMenu.Location = new System.Drawing.Point(0, 64);
            this.tbsMainMenu.MouseState = MaterialSkin.MouseState.HOVER;
            this.tbsMainMenu.Name = "tbsMainMenu";
            this.tbsMainMenu.Size = new System.Drawing.Size(715, 48);
            this.tbsMainMenu.TabIndex = 22;
            this.tbsMainMenu.Text = "materialTabSelector1";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(715, 521);
            this.Controls.Add(this.tbsMainMenu);
            this.Controls.Add(this.materialTabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Main";
            this.Sizable = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "JJManager";
            this.Load += new System.EventHandler(this.Main_Load);
            this.materialTabControl1.ResumeLayout(false);
            this.tabConnect.ResumeLayout(false);
            this.tabOptions.ResumeLayout(false);
            this.tabOptions.PerformLayout();
            this.tabUpdate.ResumeLayout(false);
            this.tabAbout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ImgAbout)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer timerSerialComUpdate;
        private System.Windows.Forms.ImageList imageList;
        private MaterialSkin.Controls.MaterialTabControl materialTabControl1;
        private System.Windows.Forms.TabPage tabConnect;
        private MaterialSkin.Controls.MaterialListView lvDevices;
        private System.Windows.Forms.TabPage tabOptions;
        private MaterialSkin.Controls.MaterialLabel LblThemeColor;
        private MaterialSkin.Controls.MaterialSwitch SwtThemeColor;
        private System.Windows.Forms.TabPage tabUpdate;
        private MaterialSkin.Controls.MaterialButton BtnUpdateDevice;
        private MaterialSkin.Controls.MaterialButton BtnUpdateSoftware;
        private System.Windows.Forms.TabPage tabAbout;
        private MaterialSkin.Controls.MaterialLabel lblAboutText;
        private MaterialSkin.Controls.MaterialLabel lblAboutVersion;
        private System.Windows.Forms.PictureBox ImgAbout;
        private MaterialSkin.Controls.MaterialTabSelector tbsMainMenu;
        private System.Windows.Forms.ColumnHeader lvhDeviceName;
        private System.Windows.Forms.ColumnHeader lvhConnType;
        private MaterialSkin.Controls.MaterialButton btnAddDevice;
        private MaterialSkin.Controls.MaterialDivider materialDivider1;
        private MaterialSkin.Controls.MaterialButton btnEditDevice;
        private MaterialSkin.Controls.MaterialButton btnConnChanger;
        public System.Windows.Forms.ColumnHeader lvhDeviceId;
        public System.Windows.Forms.ColumnHeader lvhConnStatus;
    }
}

