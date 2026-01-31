namespace JJManager.Pages.Devices
{
    partial class JJHL01
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
            this.BtnRemoveProfile = new MaterialSkin.Controls.MaterialButton();
            this.BtnAddProfile = new MaterialSkin.Controls.MaterialButton();
            this.CmbBoxSelectProfile = new MaterialSkin.Controls.MaterialComboBox();
            this.btnSaveAndCloseConfig = new MaterialSkin.Controls.MaterialButton();
            this.materialDivider1 = new MaterialSkin.Controls.MaterialDivider();
            this.btnSaveConfig = new MaterialSkin.Controls.MaterialButton();
            this.clrWheelSolid = new Cyotek.Windows.Forms.ColorWheel();
            this.SuspendLayout();
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
            this.BtnRemoveProfile.Location = new System.Drawing.Point(685, 70);
            this.BtnRemoveProfile.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnRemoveProfile.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnRemoveProfile.Name = "BtnRemoveProfile";
            this.BtnRemoveProfile.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnRemoveProfile.Size = new System.Drawing.Size(50, 50);
            this.BtnRemoveProfile.TabIndex = 15;
            this.BtnRemoveProfile.Text = "-";
            this.BtnRemoveProfile.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnRemoveProfile.UseAccentColor = false;
            this.BtnRemoveProfile.UseVisualStyleBackColor = true;
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
            this.BtnAddProfile.Location = new System.Drawing.Point(743, 70);
            this.BtnAddProfile.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnAddProfile.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnAddProfile.Name = "BtnAddProfile";
            this.BtnAddProfile.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnAddProfile.Size = new System.Drawing.Size(50, 50);
            this.BtnAddProfile.TabIndex = 14;
            this.BtnAddProfile.Text = "+";
            this.BtnAddProfile.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnAddProfile.UseAccentColor = false;
            this.BtnAddProfile.UseVisualStyleBackColor = true;
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
            this.CmbBoxSelectProfile.Location = new System.Drawing.Point(7, 70);
            this.CmbBoxSelectProfile.MaxDropDownItems = 4;
            this.CmbBoxSelectProfile.MouseState = MaterialSkin.MouseState.OUT;
            this.CmbBoxSelectProfile.Name = "CmbBoxSelectProfile";
            this.CmbBoxSelectProfile.Size = new System.Drawing.Size(671, 49);
            this.CmbBoxSelectProfile.StartIndex = 0;
            this.CmbBoxSelectProfile.TabIndex = 13;
            // 
            // btnSaveAndCloseConfig
            // 
            this.btnSaveAndCloseConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveAndCloseConfig.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSaveAndCloseConfig.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSaveAndCloseConfig.Depth = 0;
            this.btnSaveAndCloseConfig.HighEmphasis = true;
            this.btnSaveAndCloseConfig.Icon = null;
            this.btnSaveAndCloseConfig.Location = new System.Drawing.Point(645, 405);
            this.btnSaveAndCloseConfig.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSaveAndCloseConfig.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSaveAndCloseConfig.Name = "btnSaveAndCloseConfig";
            this.btnSaveAndCloseConfig.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSaveAndCloseConfig.Size = new System.Drawing.Size(148, 36);
            this.btnSaveAndCloseConfig.TabIndex = 21;
            this.btnSaveAndCloseConfig.Text = "Salvar e Fechar";
            this.btnSaveAndCloseConfig.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSaveAndCloseConfig.UseAccentColor = false;
            this.btnSaveAndCloseConfig.UseVisualStyleBackColor = true;
            // 
            // materialDivider1
            // 
            this.materialDivider1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.materialDivider1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialDivider1.Depth = 0;
            this.materialDivider1.Location = new System.Drawing.Point(13, 395);
            this.materialDivider1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialDivider1.Name = "materialDivider1";
            this.materialDivider1.Size = new System.Drawing.Size(780, 1);
            this.materialDivider1.TabIndex = 20;
            this.materialDivider1.Text = "materialDivider1";
            // 
            // btnSaveConfig
            // 
            this.btnSaveConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveConfig.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSaveConfig.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSaveConfig.Depth = 0;
            this.btnSaveConfig.HighEmphasis = true;
            this.btnSaveConfig.Icon = null;
            this.btnSaveConfig.Location = new System.Drawing.Point(561, 405);
            this.btnSaveConfig.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSaveConfig.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSaveConfig.Name = "btnSaveConfig";
            this.btnSaveConfig.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSaveConfig.Size = new System.Drawing.Size(76, 36);
            this.btnSaveConfig.TabIndex = 19;
            this.btnSaveConfig.Text = "Salvar";
            this.btnSaveConfig.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSaveConfig.UseAccentColor = false;
            this.btnSaveConfig.UseVisualStyleBackColor = true;
            // 
            // clrWheelSolid
            // 
            this.clrWheelSolid.Color = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.clrWheelSolid.Location = new System.Drawing.Point(272, 125);
            this.clrWheelSolid.Name = "clrWheelSolid";
            this.clrWheelSolid.Size = new System.Drawing.Size(250, 250);
            this.clrWheelSolid.TabIndex = 22;
            this.clrWheelSolid.ColorChanged += new System.EventHandler(this.clrWheelSolid_ColorChanged);
            // 
            // JJHL01
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.clrWheelSolid);
            this.Controls.Add(this.btnSaveAndCloseConfig);
            this.Controls.Add(this.materialDivider1);
            this.Controls.Add(this.btnSaveConfig);
            this.Controls.Add(this.BtnRemoveProfile);
            this.Controls.Add(this.BtnAddProfile);
            this.Controls.Add(this.CmbBoxSelectProfile);
            this.Icon = global::JJManager.Properties.Resources.JJManagerIcon_256;
            this.Name = "JJHL01";
            this.Text = "JJHL01";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MaterialSkin.Controls.MaterialButton BtnRemoveProfile;
        private MaterialSkin.Controls.MaterialButton BtnAddProfile;
        private MaterialSkin.Controls.MaterialComboBox CmbBoxSelectProfile;
        private MaterialSkin.Controls.MaterialButton btnSaveAndCloseConfig;
        private MaterialSkin.Controls.MaterialDivider materialDivider1;
        private MaterialSkin.Controls.MaterialButton btnSaveConfig;
        private Cyotek.Windows.Forms.ColorWheel clrWheelSolid;
    }
}