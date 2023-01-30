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
            this.CmbBoxSerialCom = new MaterialSkin.Controls.MaterialComboBox();
            this.BtnConnectMixer = new MaterialSkin.Controls.MaterialButton();
            this.SuspendLayout();
            // 
            // timerSerialComUpdate
            // 
            this.timerSerialComUpdate.Enabled = true;
            this.timerSerialComUpdate.Interval = 500;
            this.timerSerialComUpdate.Tick += new System.EventHandler(this.timerSerialComUpdate_Tick);
            // 
            // CmbBoxSerialCom
            // 
            this.CmbBoxSerialCom.AutoResize = false;
            this.CmbBoxSerialCom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.CmbBoxSerialCom.Depth = 0;
            this.CmbBoxSerialCom.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.CmbBoxSerialCom.DropDownHeight = 174;
            this.CmbBoxSerialCom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbBoxSerialCom.DropDownWidth = 121;
            this.CmbBoxSerialCom.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.CmbBoxSerialCom.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.CmbBoxSerialCom.FormattingEnabled = true;
            this.CmbBoxSerialCom.Hint = "Porta de Comunicação";
            this.CmbBoxSerialCom.IntegralHeight = false;
            this.CmbBoxSerialCom.ItemHeight = 43;
            this.CmbBoxSerialCom.Location = new System.Drawing.Point(26, 92);
            this.CmbBoxSerialCom.MaxDropDownItems = 4;
            this.CmbBoxSerialCom.MouseState = MaterialSkin.MouseState.OUT;
            this.CmbBoxSerialCom.Name = "CmbBoxSerialCom";
            this.CmbBoxSerialCom.Size = new System.Drawing.Size(300, 49);
            this.CmbBoxSerialCom.StartIndex = 0;
            this.CmbBoxSerialCom.TabIndex = 15;
            this.CmbBoxSerialCom.DropDown += new System.EventHandler(this.CmbBoxSerialCom_DropDown);
            // 
            // BtnConnectMixer
            // 
            this.BtnConnectMixer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnConnectMixer.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnConnectMixer.Depth = 0;
            this.BtnConnectMixer.HighEmphasis = true;
            this.BtnConnectMixer.Icon = null;
            this.BtnConnectMixer.Location = new System.Drawing.Point(73, 167);
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
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(352, 248);
            this.Controls.Add(this.BtnConnectMixer);
            this.Controls.Add(this.CmbBoxSerialCom);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Main";
            this.Text = "JJMixer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Timer timerSerialComUpdate;
        private MaterialSkin.Controls.MaterialComboBox CmbBoxSerialCom;
        private MaterialSkin.Controls.MaterialButton BtnConnectMixer;
    }
}

