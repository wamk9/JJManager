namespace JJManager.Pages.ButtonBox
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
            ((System.ComponentModel.ISupportInitialize)(this.ImgJJB01)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BtnInput01JJB01)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BtnInput02JJB01)).BeginInit();
            this.SuspendLayout();
            // 
            // ImgJJB01
            // 
            this.ImgJJB01.Image = global::JJManager.Properties.Resources.JJB_01;
            this.ImgJJB01.Location = new System.Drawing.Point(6, 67);
            this.ImgJJB01.Name = "ImgJJB01";
            this.ImgJJB01.Size = new System.Drawing.Size(370, 480);
            this.ImgJJB01.TabIndex = 0;
            this.ImgJJB01.TabStop = false;
            // 
            // BtnInput01JJB01
            // 
            this.BtnInput01JJB01.Image = global::JJManager.Properties.Resources.JJB_01_input01;
            this.BtnInput01JJB01.Location = new System.Drawing.Point(202, 437);
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
            this.BtnInput02JJB01.Location = new System.Drawing.Point(276, 437);
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
            this.BtnDisconnectJJB01.Location = new System.Drawing.Point(164, 555);
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
            // JJB_01
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(385, 600);
            this.Controls.Add(this.BtnDisconnectJJB01);
            this.Controls.Add(this.BtnInput02JJB01);
            this.Controls.Add(this.BtnInput01JJB01);
            this.Controls.Add(this.ImgJJB01);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(385, 600);
            this.MinimumSize = new System.Drawing.Size(385, 600);
            this.Name = "JJB_01";
            this.Sizable = false;
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
    }
}