namespace JJManager.Pages.App
{
    partial class AudioPlayer
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
            this.TxtInputName = new MaterialSkin.Controls.MaterialTextBox();
            this.flpDragDrop = new System.Windows.Forms.FlowLayoutPanel();
            this.btnPlayActualAudio = new MaterialSkin.Controls.MaterialButton();
            this.btnRemoveActualAudio = new MaterialSkin.Controls.MaterialButton();
            this.btnPlaySelectedAudio = new MaterialSkin.Controls.MaterialButton();
            this.btnClose = new MaterialSkin.Controls.MaterialButton();
            this.btnSaveAndClose = new MaterialSkin.Controls.MaterialButton();
            this.SuspendLayout();
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
            this.TxtInputName.Location = new System.Drawing.Point(6, 67);
            this.TxtInputName.MaxLength = 15;
            this.TxtInputName.MouseState = MaterialSkin.MouseState.OUT;
            this.TxtInputName.Multiline = false;
            this.TxtInputName.Name = "TxtInputName";
            this.TxtInputName.Size = new System.Drawing.Size(618, 50);
            this.TxtInputName.TabIndex = 5;
            this.TxtInputName.Text = "";
            this.TxtInputName.TrailingIcon = null;
            // 
            // flpDragDrop
            // 
            this.flpDragDrop.AllowDrop = true;
            this.flpDragDrop.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpDragDrop.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpDragDrop.Location = new System.Drawing.Point(6, 171);
            this.flpDragDrop.Name = "flpDragDrop";
            this.flpDragDrop.Size = new System.Drawing.Size(617, 275);
            this.flpDragDrop.TabIndex = 9;
            this.flpDragDrop.WrapContents = false;
            // 
            // btnPlayActualAudio
            // 
            this.btnPlayActualAudio.AutoSize = false;
            this.btnPlayActualAudio.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnPlayActualAudio.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnPlayActualAudio.Depth = 0;
            this.btnPlayActualAudio.HighEmphasis = true;
            this.btnPlayActualAudio.Icon = null;
            this.btnPlayActualAudio.Location = new System.Drawing.Point(7, 126);
            this.btnPlayActualAudio.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnPlayActualAudio.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnPlayActualAudio.Name = "btnPlayActualAudio";
            this.btnPlayActualAudio.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnPlayActualAudio.Size = new System.Drawing.Size(200, 36);
            this.btnPlayActualAudio.TabIndex = 7;
            this.btnPlayActualAudio.Text = "Tocar Áudio Atual";
            this.btnPlayActualAudio.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnPlayActualAudio.UseAccentColor = false;
            this.btnPlayActualAudio.UseVisualStyleBackColor = true;
            this.btnPlayActualAudio.Click += new System.EventHandler(this.btnPlayActualAudio_Click);
            // 
            // btnRemoveActualAudio
            // 
            this.btnRemoveActualAudio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveActualAudio.AutoSize = false;
            this.btnRemoveActualAudio.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnRemoveActualAudio.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnRemoveActualAudio.Depth = 0;
            this.btnRemoveActualAudio.HighEmphasis = true;
            this.btnRemoveActualAudio.Icon = null;
            this.btnRemoveActualAudio.Location = new System.Drawing.Point(423, 126);
            this.btnRemoveActualAudio.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnRemoveActualAudio.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnRemoveActualAudio.Name = "btnRemoveActualAudio";
            this.btnRemoveActualAudio.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnRemoveActualAudio.Size = new System.Drawing.Size(200, 36);
            this.btnRemoveActualAudio.TabIndex = 8;
            this.btnRemoveActualAudio.Text = "Excluir Áudio Atual";
            this.btnRemoveActualAudio.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnRemoveActualAudio.UseAccentColor = false;
            this.btnRemoveActualAudio.UseVisualStyleBackColor = true;
            this.btnRemoveActualAudio.Click += new System.EventHandler(this.btnRemoveActualAudio_Click);
            // 
            // btnPlaySelectedAudio
            // 
            this.btnPlaySelectedAudio.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPlaySelectedAudio.AutoSize = false;
            this.btnPlaySelectedAudio.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnPlaySelectedAudio.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnPlaySelectedAudio.Depth = 0;
            this.btnPlaySelectedAudio.HighEmphasis = true;
            this.btnPlaySelectedAudio.Icon = null;
            this.btnPlaySelectedAudio.Location = new System.Drawing.Point(215, 126);
            this.btnPlaySelectedAudio.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnPlaySelectedAudio.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnPlaySelectedAudio.Name = "btnPlaySelectedAudio";
            this.btnPlaySelectedAudio.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnPlaySelectedAudio.Size = new System.Drawing.Size(200, 36);
            this.btnPlaySelectedAudio.TabIndex = 10;
            this.btnPlaySelectedAudio.Text = "Tocar Áudio Selecionado";
            this.btnPlaySelectedAudio.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnPlaySelectedAudio.UseAccentColor = false;
            this.btnPlaySelectedAudio.UseVisualStyleBackColor = true;
            this.btnPlaySelectedAudio.Click += new System.EventHandler(this.btnPlaySelectedAudio_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.AutoSize = false;
            this.btnClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnClose.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnClose.Depth = 0;
            this.btnClose.HighEmphasis = true;
            this.btnClose.Icon = null;
            this.btnClose.Location = new System.Drawing.Point(474, 455);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnClose.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnClose.Name = "btnClose";
            this.btnClose.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnClose.Size = new System.Drawing.Size(150, 36);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "Fechar";
            this.btnClose.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnClose.UseAccentColor = false;
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnSaveAndClose
            // 
            this.btnSaveAndClose.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveAndClose.AutoSize = false;
            this.btnSaveAndClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSaveAndClose.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSaveAndClose.Depth = 0;
            this.btnSaveAndClose.HighEmphasis = true;
            this.btnSaveAndClose.Icon = null;
            this.btnSaveAndClose.Location = new System.Drawing.Point(316, 455);
            this.btnSaveAndClose.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSaveAndClose.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSaveAndClose.Name = "btnSaveAndClose";
            this.btnSaveAndClose.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSaveAndClose.Size = new System.Drawing.Size(150, 36);
            this.btnSaveAndClose.TabIndex = 11;
            this.btnSaveAndClose.Text = "Salvar e Fechar";
            this.btnSaveAndClose.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSaveAndClose.UseAccentColor = false;
            this.btnSaveAndClose.UseVisualStyleBackColor = true;
            this.btnSaveAndClose.Click += new System.EventHandler(this.btnSaveAndClose_Click);
            // 
            // AudioPlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(630, 500);
            this.Controls.Add(this.btnSaveAndClose);
            this.Controls.Add(this.btnPlaySelectedAudio);
            this.Controls.Add(this.flpDragDrop);
            this.Controls.Add(this.btnRemoveActualAudio);
            this.Controls.Add(this.btnPlayActualAudio);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.TxtInputName);
            this.Name = "AudioPlayer";
            this.Text = "AudioPlayer";
            this.ResumeLayout(false);

        }

        #endregion

        private MaterialSkin.Controls.MaterialTextBox TxtInputName;
        private System.Windows.Forms.FlowLayoutPanel flpDragDrop;
        private MaterialSkin.Controls.MaterialButton btnPlayActualAudio;
        private MaterialSkin.Controls.MaterialButton btnRemoveActualAudio;
        private MaterialSkin.Controls.MaterialButton btnPlaySelectedAudio;
        private MaterialSkin.Controls.MaterialButton btnClose;
        private MaterialSkin.Controls.MaterialButton btnSaveAndClose;
    }
}