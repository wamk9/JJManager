namespace JJManager.Pages.App.Updater
{
    partial class MultipleComPort
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MultipleComPort));
            this.txtPortInfo = new MaterialSkin.Controls.MaterialLabel();
            this.cmbSelectedPort = new MaterialSkin.Controls.MaterialComboBox();
            this.btnSelectPort = new MaterialSkin.Controls.MaterialButton();
            this.SuspendLayout();
            // 
            // txtPortInfo
            // 
            this.txtPortInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPortInfo.Depth = 0;
            this.txtPortInfo.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.txtPortInfo.Location = new System.Drawing.Point(7, 68);
            this.txtPortInfo.MouseState = MaterialSkin.MouseState.HOVER;
            this.txtPortInfo.Name = "txtPortInfo";
            this.txtPortInfo.Size = new System.Drawing.Size(377, 143);
            this.txtPortInfo.TabIndex = 0;
            this.txtPortInfo.Text = resources.GetString("txtPortInfo.Text");
            this.txtPortInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cmbSelectedPort
            // 
            this.cmbSelectedPort.AutoResize = false;
            this.cmbSelectedPort.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.cmbSelectedPort.Depth = 0;
            this.cmbSelectedPort.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cmbSelectedPort.DropDownHeight = 174;
            this.cmbSelectedPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSelectedPort.DropDownWidth = 121;
            this.cmbSelectedPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.cmbSelectedPort.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.cmbSelectedPort.FormattingEnabled = true;
            this.cmbSelectedPort.IntegralHeight = false;
            this.cmbSelectedPort.ItemHeight = 43;
            this.cmbSelectedPort.Location = new System.Drawing.Point(10, 247);
            this.cmbSelectedPort.MaxDropDownItems = 4;
            this.cmbSelectedPort.MouseState = MaterialSkin.MouseState.OUT;
            this.cmbSelectedPort.Name = "cmbSelectedPort";
            this.cmbSelectedPort.Size = new System.Drawing.Size(374, 49);
            this.cmbSelectedPort.StartIndex = 0;
            this.cmbSelectedPort.TabIndex = 1;
            // 
            // btnSelectPort
            // 
            this.btnSelectPort.AutoSize = false;
            this.btnSelectPort.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSelectPort.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSelectPort.Depth = 0;
            this.btnSelectPort.HighEmphasis = true;
            this.btnSelectPort.Icon = null;
            this.btnSelectPort.Location = new System.Drawing.Point(10, 349);
            this.btnSelectPort.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSelectPort.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSelectPort.Name = "btnSelectPort";
            this.btnSelectPort.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSelectPort.Size = new System.Drawing.Size(374, 36);
            this.btnSelectPort.TabIndex = 2;
            this.btnSelectPort.Text = "Utilizar Porta";
            this.btnSelectPort.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSelectPort.UseAccentColor = false;
            this.btnSelectPort.UseVisualStyleBackColor = true;
            this.btnSelectPort.Click += new System.EventHandler(this.btnSelectPort_Click);
            // 
            // MultipleComPort
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 391);
            this.Controls.Add(this.btnSelectPort);
            this.Controls.Add(this.cmbSelectedPort);
            this.Controls.Add(this.txtPortInfo);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Icon = global::JJManager.Properties.Resources.JJManagerIcon_256;
            this.Name = "MultipleComPort";
            this.ShowInTaskbar = false;
            this.Sizable = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Atualização - Porta de Comunicação";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private MaterialSkin.Controls.MaterialLabel txtPortInfo;
        private MaterialSkin.Controls.MaterialComboBox cmbSelectedPort;
        private MaterialSkin.Controls.MaterialButton btnSelectPort;
    }
}