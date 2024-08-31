namespace JJManager.Pages.ButtonBox
{
    partial class JJBP06
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JJBP06));
            this.CmbBoxSelectProfile = new MaterialSkin.Controls.MaterialComboBox();
            this.BtnAddProfile = new MaterialSkin.Controls.MaterialButton();
            this.BtnRemoveProfile = new MaterialSkin.Controls.MaterialButton();
            this.cmdBoxLedMode = new MaterialSkin.Controls.MaterialComboBox();
            this.sldLedBrightness = new MaterialSkin.Controls.MaterialSlider();
            this.btnSaveAndCloseConfig = new MaterialSkin.Controls.MaterialButton();
            this.ImgJJBP06On = new System.Windows.Forms.PictureBox();
            this.materialDivider1 = new MaterialSkin.Controls.MaterialDivider();
            this.ImgJJBP06Off = new System.Windows.Forms.PictureBox();
            this.btnSaveConfig = new MaterialSkin.Controls.MaterialButton();
            ((System.ComponentModel.ISupportInitialize)(this.ImgJJBP06On)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImgJJBP06Off)).BeginInit();
            this.SuspendLayout();
            // 
            // CmbBoxSelectProfile
            // 
            this.CmbBoxSelectProfile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CmbBoxSelectProfile.AutoResize = false;
            this.CmbBoxSelectProfile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.CmbBoxSelectProfile.Depth = 0;
            this.CmbBoxSelectProfile.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.CmbBoxSelectProfile.DropDownHeight = 174;
            this.CmbBoxSelectProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbBoxSelectProfile.DropDownWidth = 121;
            this.CmbBoxSelectProfile.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.CmbBoxSelectProfile.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.CmbBoxSelectProfile.FormattingEnabled = true;
            this.CmbBoxSelectProfile.Hint = "Selecione o Perfil";
            this.CmbBoxSelectProfile.IntegralHeight = false;
            this.CmbBoxSelectProfile.ItemHeight = 43;
            this.CmbBoxSelectProfile.Location = new System.Drawing.Point(7, 68);
            this.CmbBoxSelectProfile.MaxDropDownItems = 4;
            this.CmbBoxSelectProfile.MouseState = MaterialSkin.MouseState.OUT;
            this.CmbBoxSelectProfile.Name = "CmbBoxSelectProfile";
            this.CmbBoxSelectProfile.Size = new System.Drawing.Size(671, 49);
            this.CmbBoxSelectProfile.StartIndex = 0;
            this.CmbBoxSelectProfile.TabIndex = 10;
            // 
            // BtnAddProfile
            // 
            this.BtnAddProfile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnAddProfile.AutoSize = false;
            this.BtnAddProfile.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnAddProfile.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnAddProfile.Depth = 0;
            this.BtnAddProfile.Font = new System.Drawing.Font("Microsoft Sans Serif", 17F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnAddProfile.HighEmphasis = true;
            this.BtnAddProfile.Icon = null;
            this.BtnAddProfile.Location = new System.Drawing.Point(743, 68);
            this.BtnAddProfile.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnAddProfile.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnAddProfile.Name = "BtnAddProfile";
            this.BtnAddProfile.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnAddProfile.Size = new System.Drawing.Size(50, 50);
            this.BtnAddProfile.TabIndex = 11;
            this.BtnAddProfile.Text = "+";
            this.BtnAddProfile.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnAddProfile.UseAccentColor = false;
            this.BtnAddProfile.UseVisualStyleBackColor = true;
            this.BtnAddProfile.Click += new System.EventHandler(this.BtnAddProfile_Click);
            // 
            // BtnRemoveProfile
            // 
            this.BtnRemoveProfile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnRemoveProfile.AutoSize = false;
            this.BtnRemoveProfile.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnRemoveProfile.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnRemoveProfile.Depth = 0;
            this.BtnRemoveProfile.Font = new System.Drawing.Font("Microsoft Sans Serif", 17F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnRemoveProfile.HighEmphasis = true;
            this.BtnRemoveProfile.Icon = null;
            this.BtnRemoveProfile.Location = new System.Drawing.Point(685, 68);
            this.BtnRemoveProfile.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnRemoveProfile.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnRemoveProfile.Name = "BtnRemoveProfile";
            this.BtnRemoveProfile.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnRemoveProfile.Size = new System.Drawing.Size(50, 50);
            this.BtnRemoveProfile.TabIndex = 12;
            this.BtnRemoveProfile.Text = "-";
            this.BtnRemoveProfile.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnRemoveProfile.UseAccentColor = false;
            this.BtnRemoveProfile.UseVisualStyleBackColor = true;
            this.BtnRemoveProfile.Click += new System.EventHandler(this.BtnRemoveProfile_Click);
            // 
            // cmdBoxLedMode
            // 
            this.cmdBoxLedMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdBoxLedMode.AutoResize = false;
            this.cmdBoxLedMode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.cmdBoxLedMode.Depth = 0;
            this.cmdBoxLedMode.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cmdBoxLedMode.DropDownHeight = 174;
            this.cmdBoxLedMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmdBoxLedMode.DropDownWidth = 121;
            this.cmdBoxLedMode.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.cmdBoxLedMode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.cmdBoxLedMode.FormattingEnabled = true;
            this.cmdBoxLedMode.Hint = "Selecione um Modo";
            this.cmdBoxLedMode.IntegralHeight = false;
            this.cmdBoxLedMode.ItemHeight = 43;
            this.cmdBoxLedMode.Items.AddRange(new object[] {
            "Desligado",
            "Sempre Ligado",
            "Fade In/Out"});
            this.cmdBoxLedMode.Location = new System.Drawing.Point(433, 271);
            this.cmdBoxLedMode.MaxDropDownItems = 4;
            this.cmdBoxLedMode.MouseState = MaterialSkin.MouseState.OUT;
            this.cmdBoxLedMode.Name = "cmdBoxLedMode";
            this.cmdBoxLedMode.Size = new System.Drawing.Size(360, 49);
            this.cmdBoxLedMode.StartIndex = 0;
            this.cmdBoxLedMode.TabIndex = 13;
            this.cmdBoxLedMode.SelectedIndexChanged += new System.EventHandler(this.cmdBoxLedMode_SelectedIndexChanged);
            // 
            // sldLedBrightness
            // 
            this.sldLedBrightness.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sldLedBrightness.Depth = 0;
            this.sldLedBrightness.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.sldLedBrightness.Location = new System.Drawing.Point(433, 345);
            this.sldLedBrightness.MouseState = MaterialSkin.MouseState.HOVER;
            this.sldLedBrightness.Name = "sldLedBrightness";
            this.sldLedBrightness.Size = new System.Drawing.Size(360, 40);
            this.sldLedBrightness.TabIndex = 14;
            this.sldLedBrightness.Text = "Brilho";
            this.sldLedBrightness.Value = 100;
            this.sldLedBrightness.onValueChanged += new MaterialSkin.Controls.MaterialSlider.ValueChanged(this.sldLedBrightness_onValueChanged);
            // 
            // btnSaveAndCloseConfig
            // 
            this.btnSaveAndCloseConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveAndCloseConfig.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSaveAndCloseConfig.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSaveAndCloseConfig.Depth = 0;
            this.btnSaveAndCloseConfig.HighEmphasis = true;
            this.btnSaveAndCloseConfig.Icon = null;
            this.btnSaveAndCloseConfig.Location = new System.Drawing.Point(645, 555);
            this.btnSaveAndCloseConfig.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSaveAndCloseConfig.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSaveAndCloseConfig.Name = "btnSaveAndCloseConfig";
            this.btnSaveAndCloseConfig.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSaveAndCloseConfig.Size = new System.Drawing.Size(148, 36);
            this.btnSaveAndCloseConfig.TabIndex = 15;
            this.btnSaveAndCloseConfig.Text = "Salvar e Fechar";
            this.btnSaveAndCloseConfig.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSaveAndCloseConfig.UseAccentColor = false;
            this.btnSaveAndCloseConfig.UseVisualStyleBackColor = true;
            this.btnSaveAndCloseConfig.Click += new System.EventHandler(this.btnSaveAndCloseConfig_Click);
            // 
            // ImgJJBP06On
            // 
            this.ImgJJBP06On.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ImgJJBP06On.BackColor = System.Drawing.Color.Transparent;
            this.ImgJJBP06On.Image = global::JJManager.Properties.Resources.JJBP_06_on;
            this.ImgJJBP06On.Location = new System.Drawing.Point(13, 130);
            this.ImgJJBP06On.Margin = new System.Windows.Forms.Padding(10);
            this.ImgJJBP06On.Name = "ImgJJBP06On";
            this.ImgJJBP06On.Size = new System.Drawing.Size(407, 402);
            this.ImgJJBP06On.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ImgJJBP06On.TabIndex = 0;
            this.ImgJJBP06On.TabStop = false;
            // 
            // materialDivider1
            // 
            this.materialDivider1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialDivider1.Depth = 0;
            this.materialDivider1.Location = new System.Drawing.Point(13, 545);
            this.materialDivider1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialDivider1.Name = "materialDivider1";
            this.materialDivider1.Size = new System.Drawing.Size(780, 1);
            this.materialDivider1.TabIndex = 16;
            this.materialDivider1.Text = "materialDivider1";
            // 
            // ImgJJBP06Off
            // 
            this.ImgJJBP06Off.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ImgJJBP06Off.BackColor = System.Drawing.Color.Transparent;
            this.ImgJJBP06Off.Image = global::JJManager.Properties.Resources.JJBP_06_off;
            this.ImgJJBP06Off.Location = new System.Drawing.Point(13, 130);
            this.ImgJJBP06Off.Margin = new System.Windows.Forms.Padding(10);
            this.ImgJJBP06Off.Name = "ImgJJBP06Off";
            this.ImgJJBP06Off.Size = new System.Drawing.Size(407, 402);
            this.ImgJJBP06Off.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ImgJJBP06Off.TabIndex = 17;
            this.ImgJJBP06Off.TabStop = false;
            // 
            // btnSaveConfig
            // 
            this.btnSaveConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveConfig.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSaveConfig.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSaveConfig.Depth = 0;
            this.btnSaveConfig.HighEmphasis = true;
            this.btnSaveConfig.Icon = null;
            this.btnSaveConfig.Location = new System.Drawing.Point(561, 555);
            this.btnSaveConfig.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSaveConfig.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSaveConfig.Name = "btnSaveConfig";
            this.btnSaveConfig.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSaveConfig.Size = new System.Drawing.Size(76, 36);
            this.btnSaveConfig.TabIndex = 18;
            this.btnSaveConfig.Text = "Salvar";
            this.btnSaveConfig.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSaveConfig.UseAccentColor = false;
            this.btnSaveConfig.UseVisualStyleBackColor = true;
            this.btnSaveConfig.Click += new System.EventHandler(this.btnSaveConfig_Click);
            // 
            // JJBP06
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.btnSaveConfig);
            this.Controls.Add(this.materialDivider1);
            this.Controls.Add(this.btnSaveAndCloseConfig);
            this.Controls.Add(this.sldLedBrightness);
            this.Controls.Add(this.cmdBoxLedMode);
            this.Controls.Add(this.BtnRemoveProfile);
            this.Controls.Add(this.BtnAddProfile);
            this.Controls.Add(this.CmbBoxSelectProfile);
            this.Controls.Add(this.ImgJJBP06On);
            this.Controls.Add(this.ImgJJBP06Off);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1000, 600);
            this.MinimumSize = new System.Drawing.Size(500, 500);
            this.Name = "JJBP06";
            this.Sizable = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "JJBP-06";
            ((System.ComponentModel.ISupportInitialize)(this.ImgJJBP06On)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImgJJBP06Off)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private MaterialSkin.Controls.MaterialComboBox CmbBoxSelectProfile;
        private MaterialSkin.Controls.MaterialButton BtnAddProfile;
        private MaterialSkin.Controls.MaterialButton BtnRemoveProfile;
        private MaterialSkin.Controls.MaterialComboBox cmdBoxLedMode;
        private MaterialSkin.Controls.MaterialSlider sldLedBrightness;
        private MaterialSkin.Controls.MaterialButton btnSaveAndCloseConfig;
        private System.Windows.Forms.PictureBox ImgJJBP06On;
        private MaterialSkin.Controls.MaterialDivider materialDivider1;
        private System.Windows.Forms.PictureBox ImgJJBP06Off;
        private MaterialSkin.Controls.MaterialButton btnSaveConfig;
    }
}