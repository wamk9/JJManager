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
            this.CmbBoxHIDDevice = new MaterialSkin.Controls.MaterialComboBox();
            this.materialLabel1 = new MaterialSkin.Controls.MaterialLabel();
            this.BtnConnectMixer = new MaterialSkin.Controls.MaterialButton();
            this.tabOptions = new System.Windows.Forms.TabPage();
            this.LblThemeColor = new MaterialSkin.Controls.MaterialLabel();
            this.SwtThemeColor = new MaterialSkin.Controls.MaterialSwitch();
            this.tabUpdate = new System.Windows.Forms.TabPage();
            this.materialButton1 = new MaterialSkin.Controls.MaterialButton();
            this.BtnUpdateSoftware = new MaterialSkin.Controls.MaterialButton();
            this.tabAbout = new System.Windows.Forms.TabPage();
            this.ImgAbout = new System.Windows.Forms.PictureBox();
            this.lblAboutVersion = new MaterialSkin.Controls.MaterialLabel();
            this.lblAboutText = new MaterialSkin.Controls.MaterialLabel();
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
            this.timerSerialComUpdate.Interval = 500;
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
            this.materialTabControl1.Controls.Add(this.tabConnect);
            this.materialTabControl1.Controls.Add(this.tabOptions);
            this.materialTabControl1.Controls.Add(this.tabUpdate);
            this.materialTabControl1.Controls.Add(this.tabAbout);
            this.materialTabControl1.Depth = 0;
            this.materialTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.materialTabControl1.ImageList = this.imageList;
            this.materialTabControl1.Location = new System.Drawing.Point(3, 64);
            this.materialTabControl1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialTabControl1.Multiline = true;
            this.materialTabControl1.Name = "materialTabControl1";
            this.materialTabControl1.SelectedIndex = 0;
            this.materialTabControl1.Size = new System.Drawing.Size(494, 303);
            this.materialTabControl1.TabIndex = 21;
            // 
            // tabConnect
            // 
            this.tabConnect.Controls.Add(this.CmbBoxHIDDevice);
            this.tabConnect.Controls.Add(this.materialLabel1);
            this.tabConnect.Controls.Add(this.BtnConnectMixer);
            this.tabConnect.ImageKey = "JJManager_icones_connect.png";
            this.tabConnect.Location = new System.Drawing.Point(4, 23);
            this.tabConnect.Name = "tabConnect";
            this.tabConnect.Padding = new System.Windows.Forms.Padding(3);
            this.tabConnect.Size = new System.Drawing.Size(486, 276);
            this.tabConnect.TabIndex = 0;
            this.tabConnect.Text = "Conectar";
            this.tabConnect.UseVisualStyleBackColor = true;
            // 
            // CmbBoxHIDDevice
            // 
            this.CmbBoxHIDDevice.AutoResize = false;
            this.CmbBoxHIDDevice.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.CmbBoxHIDDevice.Depth = 0;
            this.CmbBoxHIDDevice.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.CmbBoxHIDDevice.DropDownHeight = 174;
            this.CmbBoxHIDDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbBoxHIDDevice.DropDownWidth = 121;
            this.CmbBoxHIDDevice.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.CmbBoxHIDDevice.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.CmbBoxHIDDevice.FormattingEnabled = true;
            this.CmbBoxHIDDevice.Hint = "Dispositivo";
            this.CmbBoxHIDDevice.IntegralHeight = false;
            this.CmbBoxHIDDevice.ItemHeight = 43;
            this.CmbBoxHIDDevice.Location = new System.Drawing.Point(43, 90);
            this.CmbBoxHIDDevice.MaxDropDownItems = 4;
            this.CmbBoxHIDDevice.MouseState = MaterialSkin.MouseState.OUT;
            this.CmbBoxHIDDevice.Name = "CmbBoxHIDDevice";
            this.CmbBoxHIDDevice.Size = new System.Drawing.Size(400, 49);
            this.CmbBoxHIDDevice.StartIndex = 0;
            this.CmbBoxHIDDevice.TabIndex = 23;
            this.CmbBoxHIDDevice.DropDown += new System.EventHandler(this.CmbBoxHIDDevice_DropDown);
            // 
            // materialLabel1
            // 
            this.materialLabel1.AutoSize = true;
            this.materialLabel1.Depth = 0;
            this.materialLabel1.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel1.Location = new System.Drawing.Point(157, 50);
            this.materialLabel1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel1.Name = "materialLabel1";
            this.materialLabel1.Size = new System.Drawing.Size(171, 19);
            this.materialLabel1.TabIndex = 22;
            this.materialLabel1.Text = "Escolha Seu Dispositivo";
            this.materialLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // BtnConnectMixer
            // 
            this.BtnConnectMixer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnConnectMixer.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnConnectMixer.Depth = 0;
            this.BtnConnectMixer.HighEmphasis = true;
            this.BtnConnectMixer.Icon = null;
            this.BtnConnectMixer.Location = new System.Drawing.Point(131, 167);
            this.BtnConnectMixer.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnConnectMixer.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnConnectMixer.Name = "BtnConnectMixer";
            this.BtnConnectMixer.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnConnectMixer.Size = new System.Drawing.Size(224, 36);
            this.BtnConnectMixer.TabIndex = 21;
            this.BtnConnectMixer.Text = "Conectar com JJManager";
            this.BtnConnectMixer.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnConnectMixer.UseAccentColor = false;
            this.BtnConnectMixer.UseVisualStyleBackColor = true;
            this.BtnConnectMixer.Click += new System.EventHandler(this.BtnConnectMixer_Click);
            // 
            // tabOptions
            // 
            this.tabOptions.Controls.Add(this.LblThemeColor);
            this.tabOptions.Controls.Add(this.SwtThemeColor);
            this.tabOptions.ImageKey = "JJManager_icones_options_16.png";
            this.tabOptions.Location = new System.Drawing.Point(4, 23);
            this.tabOptions.Name = "tabOptions";
            this.tabOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabOptions.Size = new System.Drawing.Size(486, 276);
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
            this.tabUpdate.Controls.Add(this.materialButton1);
            this.tabUpdate.Controls.Add(this.BtnUpdateSoftware);
            this.tabUpdate.ImageKey = "JJManager_icones_update_16.png";
            this.tabUpdate.Location = new System.Drawing.Point(4, 23);
            this.tabUpdate.Name = "tabUpdate";
            this.tabUpdate.Size = new System.Drawing.Size(486, 276);
            this.tabUpdate.TabIndex = 2;
            this.tabUpdate.Text = "Atualizações";
            this.tabUpdate.UseVisualStyleBackColor = true;
            // 
            // materialButton1
            // 
            this.materialButton1.AutoSize = false;
            this.materialButton1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.materialButton1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.materialButton1.Depth = 0;
            this.materialButton1.Enabled = false;
            this.materialButton1.HighEmphasis = true;
            this.materialButton1.Icon = null;
            this.materialButton1.Location = new System.Drawing.Point(103, 161);
            this.materialButton1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.materialButton1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialButton1.Name = "materialButton1";
            this.materialButton1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.materialButton1.Size = new System.Drawing.Size(280, 36);
            this.materialButton1.TabIndex = 3;
            this.materialButton1.Text = "Enviar Firmware";
            this.materialButton1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.materialButton1.UseAccentColor = false;
            this.materialButton1.UseVisualStyleBackColor = true;
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
            this.BtnUpdateSoftware.Location = new System.Drawing.Point(103, 58);
            this.BtnUpdateSoftware.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnUpdateSoftware.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnUpdateSoftware.Name = "BtnUpdateSoftware";
            this.BtnUpdateSoftware.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnUpdateSoftware.Size = new System.Drawing.Size(280, 36);
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
            this.tabAbout.Size = new System.Drawing.Size(486, 276);
            this.tabAbout.TabIndex = 3;
            this.tabAbout.Text = "Sobre";
            this.tabAbout.UseVisualStyleBackColor = true;
            // 
            // ImgAbout
            // 
            this.ImgAbout.BackgroundImage = global::JJManager.Properties.Resources.Logo_JohnJohn_JJMixer;
            this.ImgAbout.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ImgAbout.Location = new System.Drawing.Point(3, 8);
            this.ImgAbout.Name = "ImgAbout";
            this.ImgAbout.Size = new System.Drawing.Size(480, 103);
            this.ImgAbout.TabIndex = 0;
            this.ImgAbout.TabStop = false;
            // 
            // lblAboutVersion
            // 
            this.lblAboutVersion.Depth = 0;
            this.lblAboutVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.lblAboutVersion.Location = new System.Drawing.Point(3, 114);
            this.lblAboutVersion.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblAboutVersion.Name = "lblAboutVersion";
            this.lblAboutVersion.Size = new System.Drawing.Size(480, 26);
            this.lblAboutVersion.TabIndex = 2;
            this.lblAboutVersion.Text = "JJManager Versão 1.1.13";
            this.lblAboutVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAboutText
            // 
            this.lblAboutText.Depth = 0;
            this.lblAboutText.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblAboutText.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lblAboutText.Location = new System.Drawing.Point(3, 150);
            this.lblAboutText.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblAboutText.Name = "lblAboutText";
            this.lblAboutText.Size = new System.Drawing.Size(480, 126);
            this.lblAboutText.TabIndex = 3;
            this.lblAboutText.Text = resources.GetString("lblAboutText.Text");
            this.lblAboutText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 370);
            this.Controls.Add(this.materialTabControl1);
            this.DrawerTabControl = this.materialTabControl1;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Main";
            this.Sizable = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "JJManager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Main_FormClosed);
            this.Load += new System.EventHandler(this.Main_Load);
            this.materialTabControl1.ResumeLayout(false);
            this.tabConnect.ResumeLayout(false);
            this.tabConnect.PerformLayout();
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
        private System.Windows.Forms.TabPage tabOptions;
        private MaterialSkin.Controls.MaterialComboBox CmbBoxHIDDevice;
        private MaterialSkin.Controls.MaterialLabel materialLabel1;
        private MaterialSkin.Controls.MaterialButton BtnConnectMixer;
        private System.Windows.Forms.TabPage tabUpdate;
        private System.Windows.Forms.TabPage tabAbout;
        private MaterialSkin.Controls.MaterialSwitch SwtThemeColor;
        private MaterialSkin.Controls.MaterialButton BtnUpdateSoftware;
        private MaterialSkin.Controls.MaterialButton materialButton1;
        private MaterialSkin.Controls.MaterialLabel LblThemeColor;
        private System.Windows.Forms.PictureBox ImgAbout;
        private MaterialSkin.Controls.MaterialLabel lblAboutVersion;
        private MaterialSkin.Controls.MaterialLabel lblAboutText;
    }
}

