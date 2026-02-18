namespace JJManager.Pages.App
{
    partial class LedMonoAction
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
            this.BtnSave = new MaterialSkin.Controls.MaterialButton();
            this.materialDivider1 = new MaterialSkin.Controls.MaterialDivider();
            this.BtnCancel = new MaterialSkin.Controls.MaterialButton();
            this.CmbSimHubProps = new MaterialSkin.Controls.MaterialComboBox();
            this.TxtAlertMessage = new MaterialSkin.Controls.MaterialLabel();
            this.PnlAlertMessage = new System.Windows.Forms.Panel();
            this.TxtAlertMessageIcon = new System.Windows.Forms.Label();
            this.CmbLedMode = new MaterialSkin.Controls.MaterialComboBox();
            this.PnlAlertDynamicBrightness = new System.Windows.Forms.Panel();
            this.TxtAlertDynamicBrightnessIcon = new System.Windows.Forms.Label();
            this.materialLabel1 = new MaterialSkin.Controls.MaterialLabel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.PnlAlertMessage.SuspendLayout();
            this.PnlAlertDynamicBrightness.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BtnSave
            // 
            this.BtnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnSave.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnSave.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnSave.Depth = 0;
            this.BtnSave.HighEmphasis = true;
            this.BtnSave.Icon = null;
            this.BtnSave.Location = new System.Drawing.Point(645, 313);
            this.BtnSave.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnSave.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnSave.Name = "BtnSave";
            this.BtnSave.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnSave.Size = new System.Drawing.Size(148, 36);
            this.BtnSave.TabIndex = 26;
            this.BtnSave.Text = "Salvar e Fechar";
            this.BtnSave.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnSave.UseAccentColor = false;
            this.BtnSave.UseVisualStyleBackColor = true;
            // 
            // materialDivider1
            // 
            this.materialDivider1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.materialDivider1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialDivider1.Depth = 0;
            this.materialDivider1.Location = new System.Drawing.Point(12, 303);
            this.materialDivider1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialDivider1.Name = "materialDivider1";
            this.materialDivider1.Size = new System.Drawing.Size(780, 1);
            this.materialDivider1.TabIndex = 25;
            this.materialDivider1.Text = "materialDivider1";
            // 
            // BtnCancel
            // 
            this.BtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnCancel.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnCancel.Depth = 0;
            this.BtnCancel.HighEmphasis = true;
            this.BtnCancel.Icon = null;
            this.BtnCancel.Location = new System.Drawing.Point(541, 313);
            this.BtnCancel.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnCancel.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnCancel.Size = new System.Drawing.Size(96, 36);
            this.BtnCancel.TabIndex = 27;
            this.BtnCancel.Text = "Cancelar";
            this.BtnCancel.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnCancel.UseAccentColor = false;
            this.BtnCancel.UseVisualStyleBackColor = true;
            // 
            // CmbSimHubProps
            // 
            this.CmbSimHubProps.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CmbSimHubProps.AutoResize = false;
            this.CmbSimHubProps.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.CmbSimHubProps.Depth = 0;
            this.CmbSimHubProps.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.CmbSimHubProps.DropDownHeight = 174;
            this.CmbSimHubProps.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbSimHubProps.DropDownWidth = 121;
            this.CmbSimHubProps.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.CmbSimHubProps.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.CmbSimHubProps.FormattingEnabled = true;
            this.CmbSimHubProps.Hint = "Selecione Uma Propriedade";
            this.CmbSimHubProps.IntegralHeight = false;
            this.CmbSimHubProps.ItemHeight = 43;
            this.CmbSimHubProps.Location = new System.Drawing.Point(3, 109);
            this.CmbSimHubProps.MaxDropDownItems = 4;
            this.CmbSimHubProps.MouseState = MaterialSkin.MouseState.OUT;
            this.CmbSimHubProps.Name = "CmbSimHubProps";
            this.CmbSimHubProps.Size = new System.Drawing.Size(786, 49);
            this.CmbSimHubProps.StartIndex = 0;
            this.CmbSimHubProps.TabIndex = 28;
            // 
            // TxtAlertMessage
            // 
            this.TxtAlertMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtAlertMessage.Depth = 0;
            this.TxtAlertMessage.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TxtAlertMessage.Location = new System.Drawing.Point(58, 0);
            this.TxtAlertMessage.MouseState = MaterialSkin.MouseState.HOVER;
            this.TxtAlertMessage.Name = "TxtAlertMessage";
            this.TxtAlertMessage.Size = new System.Drawing.Size(724, 47);
            this.TxtAlertMessage.TabIndex = 29;
            this.TxtAlertMessage.Text = "As configurações aqui dispostas apenas funcionarão caso você esteja com o SimHub " +
    "em execução e o JJManager Sync instalado no mesmo.";
            this.TxtAlertMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PnlAlertMessage
            // 
            this.PnlAlertMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PnlAlertMessage.Controls.Add(this.TxtAlertMessageIcon);
            this.PnlAlertMessage.Controls.Add(this.TxtAlertMessage);
            this.PnlAlertMessage.Location = new System.Drawing.Point(3, 3);
            this.PnlAlertMessage.Name = "PnlAlertMessage";
            this.PnlAlertMessage.Size = new System.Drawing.Size(785, 47);
            this.PnlAlertMessage.TabIndex = 30;
            // 
            // TxtAlertMessageIcon
            // 
            this.TxtAlertMessageIcon.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.TxtAlertMessageIcon.BackColor = System.Drawing.Color.Transparent;
            this.TxtAlertMessageIcon.Location = new System.Drawing.Point(3, 0);
            this.TxtAlertMessageIcon.Name = "TxtAlertMessageIcon";
            this.TxtAlertMessageIcon.Size = new System.Drawing.Size(47, 47);
            this.TxtAlertMessageIcon.TabIndex = 31;
            this.TxtAlertMessageIcon.Text = "ICON";
            this.TxtAlertMessageIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CmbLedMode
            // 
            this.CmbLedMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CmbLedMode.AutoResize = false;
            this.CmbLedMode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.CmbLedMode.Depth = 0;
            this.CmbLedMode.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.CmbLedMode.DropDownHeight = 174;
            this.CmbLedMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbLedMode.DropDownWidth = 121;
            this.CmbLedMode.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.CmbLedMode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.CmbLedMode.FormattingEnabled = true;
            this.CmbLedMode.Hint = "Selecione um modo";
            this.CmbLedMode.IntegralHeight = false;
            this.CmbLedMode.ItemHeight = 43;
            this.CmbLedMode.Location = new System.Drawing.Point(3, 164);
            this.CmbLedMode.MaxDropDownItems = 4;
            this.CmbLedMode.MouseState = MaterialSkin.MouseState.OUT;
            this.CmbLedMode.Name = "CmbLedMode";
            this.CmbLedMode.Size = new System.Drawing.Size(786, 49);
            this.CmbLedMode.StartIndex = 0;
            this.CmbLedMode.TabIndex = 31;
            // 
            // PnlAlertDynamicBrightness
            // 
            this.PnlAlertDynamicBrightness.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PnlAlertDynamicBrightness.Controls.Add(this.TxtAlertDynamicBrightnessIcon);
            this.PnlAlertDynamicBrightness.Controls.Add(this.materialLabel1);
            this.PnlAlertDynamicBrightness.Location = new System.Drawing.Point(3, 56);
            this.PnlAlertDynamicBrightness.Name = "PnlAlertDynamicBrightness";
            this.PnlAlertDynamicBrightness.Size = new System.Drawing.Size(785, 47);
            this.PnlAlertDynamicBrightness.TabIndex = 32;
            this.PnlAlertDynamicBrightness.Visible = false;
            // 
            // TxtAlertDynamicBrightnessIcon
            // 
            this.TxtAlertDynamicBrightnessIcon.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.TxtAlertDynamicBrightnessIcon.BackColor = System.Drawing.Color.Transparent;
            this.TxtAlertDynamicBrightnessIcon.Location = new System.Drawing.Point(3, 0);
            this.TxtAlertDynamicBrightnessIcon.Name = "TxtAlertDynamicBrightnessIcon";
            this.TxtAlertDynamicBrightnessIcon.Size = new System.Drawing.Size(47, 47);
            this.TxtAlertDynamicBrightnessIcon.TabIndex = 31;
            this.TxtAlertDynamicBrightnessIcon.Text = "ICON";
            this.TxtAlertDynamicBrightnessIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // materialLabel1
            // 
            this.materialLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.materialLabel1.Depth = 0;
            this.materialLabel1.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel1.Location = new System.Drawing.Point(58, 0);
            this.materialLabel1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel1.Name = "materialLabel1";
            this.materialLabel1.Size = new System.Drawing.Size(724, 47);
            this.materialLabel1.TabIndex = 29;
            this.materialLabel1.Text = "Recomendamos o uso da opção \"Brilho dinâmico\" apenas para propriedades que usem p" +
    "ercentual, outros dados podem não funcionar corretamente.";
            this.materialLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.PnlAlertMessage);
            this.flowLayoutPanel1.Controls.Add(this.PnlAlertDynamicBrightness);
            this.flowLayoutPanel1.Controls.Add(this.CmbSimHubProps);
            this.flowLayoutPanel1.Controls.Add(this.CmbLedMode);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(6, 67);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(788, 230);
            this.flowLayoutPanel1.TabIndex = 33;
            // 
            // LedMonoAction
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 358);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnSave);
            this.Controls.Add(this.materialDivider1);
            this.Icon = global::JJManager.Properties.Resources.JJManagerIcon_256;
            this.Name = "LedMonoAction";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Criando/Editando Ação Do Led";
            this.PnlAlertMessage.ResumeLayout(false);
            this.PnlAlertDynamicBrightness.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MaterialSkin.Controls.MaterialButton BtnSave;
        private MaterialSkin.Controls.MaterialDivider materialDivider1;
        private MaterialSkin.Controls.MaterialButton BtnCancel;
        private MaterialSkin.Controls.MaterialComboBox CmbSimHubProps;
        private MaterialSkin.Controls.MaterialLabel TxtAlertMessage;
        private System.Windows.Forms.Panel PnlAlertMessage;
        private System.Windows.Forms.Label TxtAlertMessageIcon;
        private MaterialSkin.Controls.MaterialComboBox CmbLedMode;
        private System.Windows.Forms.Panel PnlAlertDynamicBrightness;
        private System.Windows.Forms.Label TxtAlertDynamicBrightnessIcon;
        private MaterialSkin.Controls.MaterialLabel materialLabel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}