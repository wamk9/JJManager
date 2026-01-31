namespace JJManager.Pages.App
{
    partial class AudioSession
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
            this.btnCancel = new MaterialSkin.Controls.MaterialButton();
            this.btnAddExecutable = new MaterialSkin.Controls.MaterialButton();
            this.lvwAudioSessions = new MaterialSkin.Controls.MaterialListView();
            this.processId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.displayName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.executable = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.volume = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnCancel.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnCancel.Depth = 0;
            this.btnCancel.HighEmphasis = true;
            this.btnCancel.Icon = null;
            this.btnCancel.Location = new System.Drawing.Point(433, 405);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnCancel.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnCancel.Size = new System.Drawing.Size(96, 36);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancelar";
            this.btnCancel.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnCancel.UseAccentColor = false;
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnAddExecutable
            // 
            this.btnAddExecutable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddExecutable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAddExecutable.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnAddExecutable.Depth = 0;
            this.btnAddExecutable.Enabled = false;
            this.btnAddExecutable.HighEmphasis = true;
            this.btnAddExecutable.Icon = null;
            this.btnAddExecutable.Location = new System.Drawing.Point(537, 405);
            this.btnAddExecutable.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnAddExecutable.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnAddExecutable.Name = "btnAddExecutable";
            this.btnAddExecutable.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnAddExecutable.Size = new System.Drawing.Size(256, 36);
            this.btnAddExecutable.TabIndex = 4;
            this.btnAddExecutable.Text = "Adicionar Items Selecionados";
            this.btnAddExecutable.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnAddExecutable.UseAccentColor = false;
            this.btnAddExecutable.UseVisualStyleBackColor = true;
            this.btnAddExecutable.Click += new System.EventHandler(this.btnAddExecutable_Click);
            // 
            // lvwAudioSessions
            // 
            this.lvwAudioSessions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvwAudioSessions.AutoSizeTable = false;
            this.lvwAudioSessions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.lvwAudioSessions.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvwAudioSessions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.processId,
            this.displayName,
            this.executable,
            this.volume});
            this.lvwAudioSessions.Depth = 0;
            this.lvwAudioSessions.FullRowSelect = true;
            this.lvwAudioSessions.HideSelection = false;
            this.lvwAudioSessions.Location = new System.Drawing.Point(7, 68);
            this.lvwAudioSessions.MinimumSize = new System.Drawing.Size(200, 100);
            this.lvwAudioSessions.MouseLocation = new System.Drawing.Point(-1, -1);
            this.lvwAudioSessions.MouseState = MaterialSkin.MouseState.OUT;
            this.lvwAudioSessions.Name = "lvwAudioSessions";
            this.lvwAudioSessions.OwnerDraw = true;
            this.lvwAudioSessions.Size = new System.Drawing.Size(786, 328);
            this.lvwAudioSessions.TabIndex = 6;
            this.lvwAudioSessions.UseCompatibleStateImageBehavior = false;
            this.lvwAudioSessions.View = System.Windows.Forms.View.Details;
            this.lvwAudioSessions.SelectedIndexChanged += new System.EventHandler(this.lvwAudioSessions_SelectedIndexChanged);
            // 
            // processId
            // 
            this.processId.Text = "ID do Processo";
            this.processId.Width = 130;
            // 
            // displayName
            // 
            this.displayName.Text = "Nome de Exibição";
            this.displayName.Width = 356;
            // 
            // executable
            // 
            this.executable.Text = "Executável Responsável";
            this.executable.Width = 200;
            // 
            // volume
            // 
            this.volume.Text = "Volume";
            this.volume.Width = 100;
            // 
            // AudioSession
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.lvwAudioSessions);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnAddExecutable);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Icon = global::JJManager.Properties.Resources.JJManagerIcon_256;
            this.Name = "AudioSession";
            this.Sizable = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Adicionar Executável via Sessão de Áudio";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.AudioSession_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MaterialSkin.Controls.MaterialButton btnCancel;
        private MaterialSkin.Controls.MaterialButton btnAddExecutable;
        private MaterialSkin.Controls.MaterialListView lvwAudioSessions;
        private System.Windows.Forms.ColumnHeader processId;
        private System.Windows.Forms.ColumnHeader displayName;
        private System.Windows.Forms.ColumnHeader volume;
        private System.Windows.Forms.ColumnHeader executable;
    }
}