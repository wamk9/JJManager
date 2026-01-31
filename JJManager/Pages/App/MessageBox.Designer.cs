namespace JJManager.Pages.App
{
    partial class MessageBox
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
            this.materialDivider1 = new MaterialSkin.Controls.MaterialDivider();
            this.TxtMessageBox = new MaterialSkin.Controls.MaterialLabel();
            this.FlpButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // materialDivider1
            // 
            this.materialDivider1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.materialDivider1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialDivider1.Depth = 0;
            this.materialDivider1.Location = new System.Drawing.Point(13, 229);
            this.materialDivider1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialDivider1.Name = "materialDivider1";
            this.materialDivider1.Size = new System.Drawing.Size(494, 1);
            this.materialDivider1.TabIndex = 36;
            this.materialDivider1.Text = "materialDivider1";
            // 
            // TxtMessageBox
            // 
            this.TxtMessageBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtMessageBox.AutoEllipsis = true;
            this.TxtMessageBox.Depth = 0;
            this.TxtMessageBox.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TxtMessageBox.Location = new System.Drawing.Point(13, 68);
            this.TxtMessageBox.MouseState = MaterialSkin.MouseState.HOVER;
            this.TxtMessageBox.Name = "TxtMessageBox";
            this.TxtMessageBox.Size = new System.Drawing.Size(493, 158);
            this.TxtMessageBox.TabIndex = 39;
            this.TxtMessageBox.Text = "TxtMessageBox";
            // 
            // FlpButtons
            // 
            this.FlpButtons.Location = new System.Drawing.Point(16, 236);
            this.FlpButtons.Name = "FlpButtons";
            this.FlpButtons.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.FlpButtons.Size = new System.Drawing.Size(490, 51);
            this.FlpButtons.TabIndex = 40;
            // 
            // MessageBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(514, 293);
            this.Controls.Add(this.FlpButtons);
            this.Controls.Add(this.TxtMessageBox);
            this.Controls.Add(this.materialDivider1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Icon = global::JJManager.Properties.Resources.JJManagerIcon_256;
            this.Name = "MessageBox";
            this.Sizable = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MessageBox";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion
        private MaterialSkin.Controls.MaterialDivider materialDivider1;
        private MaterialSkin.Controls.MaterialLabel TxtMessageBox;
        private System.Windows.Forms.FlowLayoutPanel FlpButtons;
    }
}