namespace JJManager.Pages
{
    partial class ChangeInputInfo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChangeInputInfo));
            this.BtnSaveInputConf = new MaterialSkin.Controls.MaterialButton();
            this.BtnCancelInputConf = new MaterialSkin.Controls.MaterialButton();
            this.TxtInputName = new MaterialSkin.Controls.MaterialTextBox();
            this.RdBtnInputApp = new MaterialSkin.Controls.MaterialRadioButton();
            this.RdBtnInputDevices = new MaterialSkin.Controls.MaterialRadioButton();
            this.TxtMultiLineApplications = new MaterialSkin.Controls.MaterialMultiLineTextBox();
            this.ChkBoxInputDevices = new MaterialSkin.Controls.MaterialCheckedListBox();
            this.ChkBoxInvertAxis = new MaterialSkin.Controls.MaterialCheckbox();
            this.BtnAddProgram = new MaterialSkin.Controls.MaterialButton();
            this.CmbProgramsWithAudio = new MaterialSkin.Controls.MaterialComboBox();
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
            this.BtnSaveInputConf.Location = new System.Drawing.Point(372, 530);
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
            this.BtnCancelInputConf.Location = new System.Drawing.Point(268, 530);
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
            this.TxtInputName.Size = new System.Drawing.Size(435, 50);
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
            this.RdBtnInputApp.Location = new System.Drawing.Point(42, 141);
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
            // RdBtnInputDevices
            // 
            this.RdBtnInputDevices.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.RdBtnInputDevices.AutoSize = true;
            this.RdBtnInputDevices.Depth = 0;
            this.RdBtnInputDevices.Location = new System.Drawing.Point(171, 141);
            this.RdBtnInputDevices.Margin = new System.Windows.Forms.Padding(0);
            this.RdBtnInputDevices.MouseLocation = new System.Drawing.Point(-1, -1);
            this.RdBtnInputDevices.MouseState = MaterialSkin.MouseState.HOVER;
            this.RdBtnInputDevices.Name = "RdBtnInputDevices";
            this.RdBtnInputDevices.Ripple = true;
            this.RdBtnInputDevices.Size = new System.Drawing.Size(246, 37);
            this.RdBtnInputDevices.TabIndex = 7;
            this.RdBtnInputDevices.TabStop = true;
            this.RdBtnInputDevices.Text = "Microfones e Canais de Áudio";
            this.RdBtnInputDevices.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RdBtnInputDevices.UseVisualStyleBackColor = true;
            // 
            // TxtMultiLineApplications
            // 
            this.TxtMultiLineApplications.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtMultiLineApplications.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.TxtMultiLineApplications.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TxtMultiLineApplications.Depth = 0;
            this.TxtMultiLineApplications.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TxtMultiLineApplications.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.TxtMultiLineApplications.Location = new System.Drawing.Point(13, 241);
            this.TxtMultiLineApplications.MouseState = MaterialSkin.MouseState.HOVER;
            this.TxtMultiLineApplications.Name = "TxtMultiLineApplications";
            this.TxtMultiLineApplications.Size = new System.Drawing.Size(435, 280);
            this.TxtMultiLineApplications.TabIndex = 8;
            this.TxtMultiLineApplications.Text = "";
            // 
            // ChkBoxInputDevices
            // 
            this.ChkBoxInputDevices.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ChkBoxInputDevices.AutoScroll = true;
            this.ChkBoxInputDevices.BackColor = System.Drawing.SystemColors.Control;
            this.ChkBoxInputDevices.Depth = 0;
            this.ChkBoxInputDevices.Location = new System.Drawing.Point(13, 187);
            this.ChkBoxInputDevices.MouseState = MaterialSkin.MouseState.HOVER;
            this.ChkBoxInputDevices.Name = "ChkBoxInputDevices";
            this.ChkBoxInputDevices.Size = new System.Drawing.Size(435, 334);
            this.ChkBoxInputDevices.Striped = false;
            this.ChkBoxInputDevices.StripeDarkColor = System.Drawing.Color.Empty;
            this.ChkBoxInputDevices.TabIndex = 9;
            // 
            // ChkBoxInvertAxis
            // 
            this.ChkBoxInvertAxis.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ChkBoxInvertAxis.AutoSize = true;
            this.ChkBoxInvertAxis.Depth = 0;
            this.ChkBoxInvertAxis.Location = new System.Drawing.Point(13, 528);
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
            // BtnAddProgram
            // 
            this.BtnAddProgram.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnAddProgram.AutoSize = false;
            this.BtnAddProgram.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnAddProgram.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnAddProgram.Depth = 0;
            this.BtnAddProgram.HighEmphasis = true;
            this.BtnAddProgram.Icon = null;
            this.BtnAddProgram.Location = new System.Drawing.Point(321, 184);
            this.BtnAddProgram.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnAddProgram.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnAddProgram.Name = "BtnAddProgram";
            this.BtnAddProgram.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnAddProgram.Size = new System.Drawing.Size(127, 53);
            this.BtnAddProgram.TabIndex = 14;
            this.BtnAddProgram.Text = "Acrescentar";
            this.BtnAddProgram.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnAddProgram.UseAccentColor = false;
            this.BtnAddProgram.UseVisualStyleBackColor = true;
            this.BtnAddProgram.Click += new System.EventHandler(this.BtnAddProgram_Click);
            // 
            // CmbProgramsWithAudio
            // 
            this.CmbProgramsWithAudio.AutoResize = false;
            this.CmbProgramsWithAudio.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.CmbProgramsWithAudio.Depth = 0;
            this.CmbProgramsWithAudio.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.CmbProgramsWithAudio.DropDownHeight = 174;
            this.CmbProgramsWithAudio.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbProgramsWithAudio.DropDownWidth = 121;
            this.CmbProgramsWithAudio.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.CmbProgramsWithAudio.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.CmbProgramsWithAudio.FormattingEnabled = true;
            this.CmbProgramsWithAudio.IntegralHeight = false;
            this.CmbProgramsWithAudio.ItemHeight = 43;
            this.CmbProgramsWithAudio.Location = new System.Drawing.Point(13, 186);
            this.CmbProgramsWithAudio.MaxDropDownItems = 4;
            this.CmbProgramsWithAudio.MouseState = MaterialSkin.MouseState.OUT;
            this.CmbProgramsWithAudio.Name = "CmbProgramsWithAudio";
            this.CmbProgramsWithAudio.Size = new System.Drawing.Size(294, 49);
            this.CmbProgramsWithAudio.StartIndex = 0;
            this.CmbProgramsWithAudio.TabIndex = 13;
            this.CmbProgramsWithAudio.DropDown += new System.EventHandler(this.CmbProgramsWithAudio_DropDown);
            // 
            // ChangeInputInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(461, 581);
            this.Controls.Add(this.BtnAddProgram);
            this.Controls.Add(this.CmbProgramsWithAudio);
            this.Controls.Add(this.ChkBoxInvertAxis);
            this.Controls.Add(this.TxtMultiLineApplications);
            this.Controls.Add(this.RdBtnInputDevices);
            this.Controls.Add(this.RdBtnInputApp);
            this.Controls.Add(this.TxtInputName);
            this.Controls.Add(this.BtnCancelInputConf);
            this.Controls.Add(this.BtnSaveInputConf);
            this.Controls.Add(this.ChkBoxInputDevices);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 480);
            this.Name = "ChangeInputInfo";
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
        private MaterialSkin.Controls.MaterialRadioButton RdBtnInputDevices;
        private MaterialSkin.Controls.MaterialMultiLineTextBox TxtMultiLineApplications;
        private MaterialSkin.Controls.MaterialCheckedListBox ChkBoxInputDevices;
        private MaterialSkin.Controls.MaterialCheckbox ChkBoxInvertAxis;
        private MaterialSkin.Controls.MaterialButton BtnAddProgram;
        private MaterialSkin.Controls.MaterialComboBox CmbProgramsWithAudio;
    }
}