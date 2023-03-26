﻿namespace JJManager.Pages.ButtonBox
{
    partial class JJB_01
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JJB_01));
            this.ImgJJB01 = new System.Windows.Forms.PictureBox();
            this.BtnInput01JJB01 = new System.Windows.Forms.PictureBox();
            this.BtnInput02JJB01 = new System.Windows.Forms.PictureBox();
            this.BtnDisconnectJJB01 = new MaterialSkin.Controls.MaterialButton();
            this.CmbBoxSelectProfile = new MaterialSkin.Controls.MaterialComboBox();
            this.BtnAddProfile = new MaterialSkin.Controls.MaterialButton();
            this.BtnRemoveProfile = new MaterialSkin.Controls.MaterialButton();
            ((System.ComponentModel.ISupportInitialize)(this.ImgJJB01)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BtnInput01JJB01)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BtnInput02JJB01)).BeginInit();
            this.SuspendLayout();
            // 
            // ImgJJB01
            // 
            this.ImgJJB01.BackColor = System.Drawing.Color.Transparent;
            this.ImgJJB01.Image = global::JJManager.Properties.Resources.JJB_01;
            this.ImgJJB01.Location = new System.Drawing.Point(69, 123);
            this.ImgJJB01.Name = "ImgJJB01";
            this.ImgJJB01.Size = new System.Drawing.Size(370, 480);
            this.ImgJJB01.TabIndex = 0;
            this.ImgJJB01.TabStop = false;
            // 
            // BtnInput01JJB01
            // 
            this.BtnInput01JJB01.Image = global::JJManager.Properties.Resources.JJB_01_input01;
            this.BtnInput01JJB01.Location = new System.Drawing.Point(265, 493);
            this.BtnInput01JJB01.Name = "BtnInput01JJB01";
            this.BtnInput01JJB01.Size = new System.Drawing.Size(52, 68);
            this.BtnInput01JJB01.TabIndex = 1;
            this.BtnInput01JJB01.TabStop = false;
            this.BtnInput01JJB01.Click += new System.EventHandler(this.BtnInput01JJB01_Click);
            this.BtnInput01JJB01.MouseEnter += new System.EventHandler(this.BtnInput01JJB01_MouseEnter);
            this.BtnInput01JJB01.MouseLeave += new System.EventHandler(this.BtnInput01JJB01_MouseLeave);
            // 
            // BtnInput02JJB01
            // 
            this.BtnInput02JJB01.Image = global::JJManager.Properties.Resources.JJB_01_input01;
            this.BtnInput02JJB01.Location = new System.Drawing.Point(339, 493);
            this.BtnInput02JJB01.Name = "BtnInput02JJB01";
            this.BtnInput02JJB01.Size = new System.Drawing.Size(52, 68);
            this.BtnInput02JJB01.TabIndex = 2;
            this.BtnInput02JJB01.TabStop = false;
            this.BtnInput02JJB01.Click += new System.EventHandler(this.BtnInput02JJB01_Click);
            this.BtnInput02JJB01.MouseEnter += new System.EventHandler(this.BtnInput02JJB01_MouseEnter);
            this.BtnInput02JJB01.MouseLeave += new System.EventHandler(this.BtnInput02JJB01_MouseLeave);
            // 
            // BtnDisconnectJJB01
            // 
            this.BtnDisconnectJJB01.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnDisconnectJJB01.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnDisconnectJJB01.Depth = 0;
            this.BtnDisconnectJJB01.HighEmphasis = true;
            this.BtnDisconnectJJB01.Icon = null;
            this.BtnDisconnectJJB01.Location = new System.Drawing.Point(279, 613);
            this.BtnDisconnectJJB01.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnDisconnectJJB01.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnDisconnectJJB01.Name = "BtnDisconnectJJB01";
            this.BtnDisconnectJJB01.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnDisconnectJJB01.Size = new System.Drawing.Size(214, 36);
            this.BtnDisconnectJJB01.TabIndex = 9;
            this.BtnDisconnectJJB01.Text = "Desconectar ButtonBox";
            this.BtnDisconnectJJB01.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnDisconnectJJB01.UseAccentColor = false;
            this.BtnDisconnectJJB01.UseVisualStyleBackColor = true;
            this.BtnDisconnectJJB01.Click += new System.EventHandler(this.BtnDisconnectJJB01_Click);
            // 
            // CmbBoxSelectProfile
            // 
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
            this.CmbBoxSelectProfile.Size = new System.Drawing.Size(371, 49);
            this.CmbBoxSelectProfile.StartIndex = 0;
            this.CmbBoxSelectProfile.TabIndex = 10;
            // 
            // BtnAddProfile
            // 
            this.BtnAddProfile.AutoSize = false;
            this.BtnAddProfile.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnAddProfile.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnAddProfile.Depth = 0;
            this.BtnAddProfile.Font = new System.Drawing.Font("Microsoft Sans Serif", 17F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnAddProfile.HighEmphasis = true;
            this.BtnAddProfile.Icon = null;
            this.BtnAddProfile.Location = new System.Drawing.Point(443, 68);
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
            this.BtnRemoveProfile.AutoSize = false;
            this.BtnRemoveProfile.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnRemoveProfile.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnRemoveProfile.Depth = 0;
            this.BtnRemoveProfile.Font = new System.Drawing.Font("Microsoft Sans Serif", 17F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnRemoveProfile.HighEmphasis = true;
            this.BtnRemoveProfile.Icon = null;
            this.BtnRemoveProfile.Location = new System.Drawing.Point(385, 68);
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
            // JJB_01
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 660);
            this.Controls.Add(this.BtnRemoveProfile);
            this.Controls.Add(this.BtnAddProfile);
            this.Controls.Add(this.CmbBoxSelectProfile);
            this.Controls.Add(this.BtnDisconnectJJB01);
            this.Controls.Add(this.BtnInput02JJB01);
            this.Controls.Add(this.BtnInput01JJB01);
            this.Controls.Add(this.ImgJJB01);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(500, 660);
            this.MinimumSize = new System.Drawing.Size(500, 660);
            this.Name = "JJB_01";
            this.Sizable = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "JJB-01";
            ((System.ComponentModel.ISupportInitialize)(this.ImgJJB01)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BtnInput01JJB01)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BtnInput02JJB01)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.PictureBox ImgJJB01;
        private System.Windows.Forms.PictureBox BtnInput01JJB01;
        private System.Windows.Forms.PictureBox BtnInput02JJB01;
        private MaterialSkin.Controls.MaterialButton BtnDisconnectJJB01;
        private MaterialSkin.Controls.MaterialComboBox CmbBoxSelectProfile;
        private MaterialSkin.Controls.MaterialButton BtnAddProfile;
        private MaterialSkin.Controls.MaterialButton BtnRemoveProfile;
    }
}