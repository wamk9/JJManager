namespace JJMixer_WForms
{
    partial class Main
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.timerSerialComUpdate = new System.Windows.Forms.Timer(this.components);
            this.BtnConnectMixer = new MaterialSkin.Controls.MaterialButton();
            this.materialLabel1 = new MaterialSkin.Controls.MaterialLabel();
            this.CmbBoxHIDDevice = new MaterialSkin.Controls.MaterialComboBox();
            this.SuspendLayout();
            // 
            // timerSerialComUpdate
            // 
            this.timerSerialComUpdate.Enabled = true;
            this.timerSerialComUpdate.Interval = 500;
            this.timerSerialComUpdate.Tick += new System.EventHandler(this.timerSerialComUpdate_Tick);
            // 
            // BtnConnectMixer
            // 
            this.BtnConnectMixer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnConnectMixer.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnConnectMixer.Depth = 0;
            this.BtnConnectMixer.HighEmphasis = true;
            this.BtnConnectMixer.Icon = null;
            this.BtnConnectMixer.Location = new System.Drawing.Point(87, 194);
            this.BtnConnectMixer.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnConnectMixer.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnConnectMixer.Name = "BtnConnectMixer";
            this.BtnConnectMixer.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnConnectMixer.Size = new System.Drawing.Size(198, 36);
            this.BtnConnectMixer.TabIndex = 16;
            this.BtnConnectMixer.Text = "Conectar com JJMixer";
            this.BtnConnectMixer.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnConnectMixer.UseAccentColor = false;
            this.BtnConnectMixer.UseVisualStyleBackColor = true;
            this.BtnConnectMixer.Click += new System.EventHandler(this.BtnConnectMixer_Click);
            // 
            // materialLabel1
            // 
            this.materialLabel1.AutoSize = true;
            this.materialLabel1.Depth = 0;
            this.materialLabel1.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel1.Location = new System.Drawing.Point(97, 80);
            this.materialLabel1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel1.Name = "materialLabel1";
            this.materialLabel1.Size = new System.Drawing.Size(171, 19);
            this.materialLabel1.TabIndex = 19;
            this.materialLabel1.Text = "Escolha Seu Dispositivo";
            this.materialLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CmbBoxHIDDevice
            // 
            this.CmbBoxHIDDevice.AutoResize = false;
            this.CmbBoxHIDDevice.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.CmbBoxHIDDevice.Depth = 0;
            this.CmbBoxHIDDevice.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.CmbBoxHIDDevice.DropDownHeight = 174;
            this.CmbBoxHIDDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbBoxHIDDevice.DropDownWidth = 121;
            this.CmbBoxHIDDevice.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.CmbBoxHIDDevice.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.CmbBoxHIDDevice.FormattingEnabled = true;
            this.CmbBoxHIDDevice.Hint = "Dispositivo";
            this.CmbBoxHIDDevice.IntegralHeight = false;
            this.CmbBoxHIDDevice.ItemHeight = 43;
            this.CmbBoxHIDDevice.Location = new System.Drawing.Point(40, 119);
            this.CmbBoxHIDDevice.MaxDropDownItems = 4;
            this.CmbBoxHIDDevice.MouseState = MaterialSkin.MouseState.OUT;
            this.CmbBoxHIDDevice.Name = "CmbBoxHIDDevice";
            this.CmbBoxHIDDevice.Size = new System.Drawing.Size(300, 49);
            this.CmbBoxHIDDevice.StartIndex = 0;
            this.CmbBoxHIDDevice.TabIndex = 20;
            this.CmbBoxHIDDevice.DropDown += new System.EventHandler(this.CmbBoxHIDDevice_DropDown);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 270);
            this.Controls.Add(this.CmbBoxHIDDevice);
            this.Controls.Add(this.materialLabel1);
            this.Controls.Add(this.BtnConnectMixer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Main";
            this.Sizable = false;
            this.Text = "JJMixer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Timer timerSerialComUpdate;
        private MaterialSkin.Controls.MaterialButton BtnConnectMixer;
        private MaterialSkin.Controls.MaterialLabel materialLabel1;
        private MaterialSkin.Controls.MaterialComboBox CmbBoxHIDDevice;
    }
}

