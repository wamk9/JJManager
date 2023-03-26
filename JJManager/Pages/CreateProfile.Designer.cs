namespace JJManager.Pages
{
    partial class CreateProfile
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateProfile));
            this.BtnSaveProfile = new MaterialSkin.Controls.MaterialButton();
            this.TxtProfileName = new MaterialSkin.Controls.MaterialTextBox();
            this.SuspendLayout();
            // 
            // BtnSaveProfile
            // 
            this.BtnSaveProfile.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnSaveProfile.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnSaveProfile.Depth = 0;
            this.BtnSaveProfile.HighEmphasis = true;
            this.BtnSaveProfile.Icon = null;
            this.BtnSaveProfile.Location = new System.Drawing.Point(279, 165);
            this.BtnSaveProfile.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnSaveProfile.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnSaveProfile.Name = "BtnSaveProfile";
            this.BtnSaveProfile.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnSaveProfile.Size = new System.Drawing.Size(114, 36);
            this.BtnSaveProfile.TabIndex = 0;
            this.BtnSaveProfile.Text = "Criar Perfil";
            this.BtnSaveProfile.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnSaveProfile.UseAccentColor = false;
            this.BtnSaveProfile.UseVisualStyleBackColor = true;
            this.BtnSaveProfile.Click += new System.EventHandler(this.BtnSaveProfile_Click);
            // 
            // TxtProfileName
            // 
            this.TxtProfileName.AnimateReadOnly = false;
            this.TxtProfileName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TxtProfileName.Depth = 0;
            this.TxtProfileName.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TxtProfileName.Hint = "Nome do Perfil";
            this.TxtProfileName.LeadingIcon = null;
            this.TxtProfileName.Location = new System.Drawing.Point(6, 93);
            this.TxtProfileName.MaxLength = 50;
            this.TxtProfileName.MouseState = MaterialSkin.MouseState.OUT;
            this.TxtProfileName.Multiline = false;
            this.TxtProfileName.Name = "TxtProfileName";
            this.TxtProfileName.Size = new System.Drawing.Size(387, 50);
            this.TxtProfileName.TabIndex = 1;
            this.TxtProfileName.Text = "";
            this.TxtProfileName.TrailingIcon = null;
            // 
            // CreateProfile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 210);
            this.Controls.Add(this.TxtProfileName);
            this.Controls.Add(this.BtnSaveProfile);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(400, 210);
            this.MinimumSize = new System.Drawing.Size(400, 210);
            this.Name = "CreateProfile";
            this.Sizable = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Criação de Perfil";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MaterialSkin.Controls.MaterialButton BtnSaveProfile;
        private MaterialSkin.Controls.MaterialTextBox TxtProfileName;
    }
}