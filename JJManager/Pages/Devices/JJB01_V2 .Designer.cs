namespace JJManager.Pages.Devices
{
    partial class JJB01_V2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JJB01_V2));
            this.CmbBoxSelectProfile = new MaterialSkin.Controls.MaterialComboBox();
            this.BtnAddProfile = new MaterialSkin.Controls.MaterialButton();
            this.BtnRemoveProfile = new MaterialSkin.Controls.MaterialButton();
            this.btnSaveConfig = new MaterialSkin.Controls.MaterialButton();
            this.ImgJJB01V2Off = new System.Windows.Forms.PictureBox();
            this.ImgJJB01V2On = new System.Windows.Forms.PictureBox();
            this.materialDivider1 = new MaterialSkin.Controls.MaterialDivider();
            this.btnSaveAndCloseConfig = new MaterialSkin.Controls.MaterialButton();
            this.SldBlinkSpeed = new MaterialSkin.Controls.MaterialSlider();
            this.SldPulseSpeed = new MaterialSkin.Controls.MaterialSlider();
            this.BtnAddLedAction = new MaterialSkin.Controls.MaterialButton();
            this.FlpJJB01V2 = new System.Windows.Forms.FlowLayoutPanel();
            this.TxtGeneralConfigs = new MaterialSkin.Controls.MaterialLabel();
            this.materialDivider3 = new MaterialSkin.Controls.MaterialDivider();
            this.cmdBoxLedMode = new MaterialSkin.Controls.MaterialComboBox();
            this.SldLedBrightness = new MaterialSkin.Controls.MaterialSlider();
            this.TxtProperties = new MaterialSkin.Controls.MaterialLabel();
            this.PropertiesDivider = new MaterialSkin.Controls.MaterialDivider();
            this.dgvActions = new System.Windows.Forms.DataGridView();
            this.Id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Ordem = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvActionType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvActionEdit = new System.Windows.Forms.DataGridViewButtonColumn();
            this.dgvActionRemove = new System.Windows.Forms.DataGridViewButtonColumn();
            this.dgvActionMoveUp = new System.Windows.Forms.DataGridViewButtonColumn();
            this.dgvActionMoveDown = new System.Windows.Forms.DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)(this.ImgJJB01V2Off)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImgJJB01V2On)).BeginInit();
            this.FlpJJB01V2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvActions)).BeginInit();
            this.SuspendLayout();
            // 
            // CmbBoxSelectProfile
            // 
            this.CmbBoxSelectProfile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
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
            this.CmbBoxSelectProfile.Size = new System.Drawing.Size(871, 49);
            this.CmbBoxSelectProfile.StartIndex = 0;
            this.CmbBoxSelectProfile.TabIndex = 10;
            // 
            // BtnAddProfile
            // 
            this.BtnAddProfile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnAddProfile.AutoSize = false;
            this.BtnAddProfile.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnAddProfile.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnAddProfile.Depth = 0;
            this.BtnAddProfile.Font = new System.Drawing.Font("Microsoft Sans Serif", 17F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnAddProfile.HighEmphasis = true;
            this.BtnAddProfile.Icon = null;
            this.BtnAddProfile.Location = new System.Drawing.Point(943, 67);
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
            this.BtnRemoveProfile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnRemoveProfile.AutoSize = false;
            this.BtnRemoveProfile.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnRemoveProfile.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnRemoveProfile.Depth = 0;
            this.BtnRemoveProfile.Font = new System.Drawing.Font("Microsoft Sans Serif", 17F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnRemoveProfile.HighEmphasis = true;
            this.BtnRemoveProfile.Icon = null;
            this.BtnRemoveProfile.Location = new System.Drawing.Point(885, 67);
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
            // btnSaveConfig
            // 
            this.btnSaveConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveConfig.AutoSize = false;
            this.btnSaveConfig.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSaveConfig.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSaveConfig.Depth = 0;
            this.btnSaveConfig.HighEmphasis = true;
            this.btnSaveConfig.Icon = null;
            this.btnSaveConfig.Location = new System.Drawing.Point(688, 655);
            this.btnSaveConfig.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSaveConfig.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSaveConfig.Name = "btnSaveConfig";
            this.btnSaveConfig.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSaveConfig.Size = new System.Drawing.Size(219, 36);
            this.btnSaveConfig.TabIndex = 15;
            this.btnSaveConfig.Text = "Conectar";
            this.btnSaveConfig.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSaveConfig.UseAccentColor = false;
            this.btnSaveConfig.UseVisualStyleBackColor = true;
            this.btnSaveConfig.Click += new System.EventHandler(this.btnSaveConfig_Click);
            // 
            // ImgJJB01V2Off
            // 
            this.ImgJJB01V2Off.BackColor = System.Drawing.Color.Transparent;
            this.ImgJJB01V2Off.Image = global::JJManager.Properties.Resources.JJB_01_v2_off;
            this.ImgJJB01V2Off.Location = new System.Drawing.Point(13, 176);
            this.ImgJJB01V2Off.Margin = new System.Windows.Forms.Padding(10);
            this.ImgJJB01V2Off.Name = "ImgJJB01V2Off";
            this.ImgJJB01V2Off.Size = new System.Drawing.Size(406, 402);
            this.ImgJJB01V2Off.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ImgJJB01V2Off.TabIndex = 0;
            this.ImgJJB01V2Off.TabStop = false;
            // 
            // ImgJJB01V2On
            // 
            this.ImgJJB01V2On.BackColor = System.Drawing.Color.Transparent;
            this.ImgJJB01V2On.Image = global::JJManager.Properties.Resources.JJB_01_v2_on;
            this.ImgJJB01V2On.Location = new System.Drawing.Point(13, 176);
            this.ImgJJB01V2On.Margin = new System.Windows.Forms.Padding(10);
            this.ImgJJB01V2On.Name = "ImgJJB01V2On";
            this.ImgJJB01V2On.Size = new System.Drawing.Size(406, 402);
            this.ImgJJB01V2On.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ImgJJB01V2On.TabIndex = 16;
            this.ImgJJB01V2On.TabStop = false;
            // 
            // materialDivider1
            // 
            this.materialDivider1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialDivider1.Depth = 0;
            this.materialDivider1.Location = new System.Drawing.Point(9, 645);
            this.materialDivider1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialDivider1.Name = "materialDivider1";
            this.materialDivider1.Size = new System.Drawing.Size(984, 1);
            this.materialDivider1.TabIndex = 17;
            this.materialDivider1.Text = "materialDivider1";
            // 
            // btnSaveAndCloseConfig
            // 
            this.btnSaveAndCloseConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveAndCloseConfig.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSaveAndCloseConfig.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSaveAndCloseConfig.Depth = 0;
            this.btnSaveAndCloseConfig.HighEmphasis = true;
            this.btnSaveAndCloseConfig.Icon = null;
            this.btnSaveAndCloseConfig.Location = new System.Drawing.Point(915, 655);
            this.btnSaveAndCloseConfig.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSaveAndCloseConfig.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSaveAndCloseConfig.Name = "btnSaveAndCloseConfig";
            this.btnSaveAndCloseConfig.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSaveAndCloseConfig.Size = new System.Drawing.Size(77, 36);
            this.btnSaveAndCloseConfig.TabIndex = 18;
            this.btnSaveAndCloseConfig.Text = "Fechar";
            this.btnSaveAndCloseConfig.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSaveAndCloseConfig.UseAccentColor = false;
            this.btnSaveAndCloseConfig.UseVisualStyleBackColor = true;
            this.btnSaveAndCloseConfig.Click += new System.EventHandler(this.btnSaveAndCloseConfig_Click);
            // 
            // SldBlinkSpeed
            // 
            this.SldBlinkSpeed.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.SldBlinkSpeed.Depth = 0;
            this.SldBlinkSpeed.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.SldBlinkSpeed.Location = new System.Drawing.Point(3, 176);
            this.SldBlinkSpeed.MouseState = MaterialSkin.MouseState.HOVER;
            this.SldBlinkSpeed.Name = "SldBlinkSpeed";
            this.SldBlinkSpeed.RangeMax = 255;
            this.SldBlinkSpeed.RangeMin = 50;
            this.SldBlinkSpeed.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.SldBlinkSpeed.ShowValue = false;
            this.SldBlinkSpeed.Size = new System.Drawing.Size(553, 40);
            this.SldBlinkSpeed.TabIndex = 25;
            this.SldBlinkSpeed.Text = "Vel. Piscar";
            this.SldBlinkSpeed.Value = 100;
            // 
            // SldPulseSpeed
            // 
            this.SldPulseSpeed.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.SldPulseSpeed.Depth = 0;
            this.SldPulseSpeed.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.SldPulseSpeed.Location = new System.Drawing.Point(3, 130);
            this.SldPulseSpeed.MouseState = MaterialSkin.MouseState.HOVER;
            this.SldPulseSpeed.Name = "SldPulseSpeed";
            this.SldPulseSpeed.RangeMax = 255;
            this.SldPulseSpeed.RangeMin = 50;
            this.SldPulseSpeed.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.SldPulseSpeed.ShowValue = false;
            this.SldPulseSpeed.Size = new System.Drawing.Size(553, 40);
            this.SldPulseSpeed.TabIndex = 24;
            this.SldPulseSpeed.Text = "Vel. Pulso";
            this.SldPulseSpeed.Value = 100;
            // 
            // BtnAddLedAction
            // 
            this.BtnAddLedAction.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.BtnAddLedAction.AutoSize = false;
            this.BtnAddLedAction.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnAddLedAction.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnAddLedAction.Depth = 0;
            this.BtnAddLedAction.DrawShadows = false;
            this.BtnAddLedAction.HighEmphasis = true;
            this.BtnAddLedAction.Icon = null;
            this.BtnAddLedAction.Location = new System.Drawing.Point(4, 452);
            this.BtnAddLedAction.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnAddLedAction.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnAddLedAction.Name = "BtnAddLedAction";
            this.BtnAddLedAction.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnAddLedAction.Size = new System.Drawing.Size(551, 36);
            this.BtnAddLedAction.TabIndex = 29;
            this.BtnAddLedAction.Text = "Adicionar Ação";
            this.BtnAddLedAction.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnAddLedAction.UseAccentColor = false;
            this.BtnAddLedAction.UseVisualStyleBackColor = true;
            this.BtnAddLedAction.Visible = false;
            // 
            // FlpJJB01V2
            // 
            this.FlpJJB01V2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlpJJB01V2.AutoScroll = true;
            this.FlpJJB01V2.Controls.Add(this.TxtGeneralConfigs);
            this.FlpJJB01V2.Controls.Add(this.materialDivider3);
            this.FlpJJB01V2.Controls.Add(this.cmdBoxLedMode);
            this.FlpJJB01V2.Controls.Add(this.SldLedBrightness);
            this.FlpJJB01V2.Controls.Add(this.SldPulseSpeed);
            this.FlpJJB01V2.Controls.Add(this.SldBlinkSpeed);
            this.FlpJJB01V2.Controls.Add(this.TxtProperties);
            this.FlpJJB01V2.Controls.Add(this.PropertiesDivider);
            this.FlpJJB01V2.Controls.Add(this.dgvActions);
            this.FlpJJB01V2.Controls.Add(this.BtnAddLedAction);
            this.FlpJJB01V2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlpJJB01V2.Location = new System.Drawing.Point(432, 123);
            this.FlpJJB01V2.Name = "FlpJJB01V2";
            this.FlpJJB01V2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.FlpJJB01V2.Size = new System.Drawing.Size(561, 506);
            this.FlpJJB01V2.TabIndex = 42;
            this.FlpJJB01V2.WrapContents = false;
            // 
            // TxtGeneralConfigs
            // 
            this.TxtGeneralConfigs.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.TxtGeneralConfigs.Depth = 0;
            this.TxtGeneralConfigs.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TxtGeneralConfigs.Location = new System.Drawing.Point(4, 0);
            this.TxtGeneralConfigs.MouseState = MaterialSkin.MouseState.HOVER;
            this.TxtGeneralConfigs.Name = "TxtGeneralConfigs";
            this.TxtGeneralConfigs.Size = new System.Drawing.Size(550, 19);
            this.TxtGeneralConfigs.TabIndex = 20;
            this.TxtGeneralConfigs.Text = "Geral";
            this.TxtGeneralConfigs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // materialDivider3
            // 
            this.materialDivider3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.materialDivider3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialDivider3.Depth = 0;
            this.materialDivider3.Location = new System.Drawing.Point(3, 22);
            this.materialDivider3.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialDivider3.Name = "materialDivider3";
            this.materialDivider3.Size = new System.Drawing.Size(553, 1);
            this.materialDivider3.TabIndex = 21;
            this.materialDivider3.Text = "materialDivider3";
            // 
            // cmdBoxLedMode
            // 
            this.cmdBoxLedMode.AutoResize = false;
            this.cmdBoxLedMode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.cmdBoxLedMode.Depth = 0;
            this.cmdBoxLedMode.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cmdBoxLedMode.DropDownHeight = 174;
            this.cmdBoxLedMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmdBoxLedMode.DropDownWidth = 190;
            this.cmdBoxLedMode.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.cmdBoxLedMode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.cmdBoxLedMode.FormattingEnabled = true;
            this.cmdBoxLedMode.Hint = "Selecione um Modo";
            this.cmdBoxLedMode.IntegralHeight = false;
            this.cmdBoxLedMode.ItemHeight = 43;
            this.cmdBoxLedMode.Items.AddRange(new object[] {
            "Desligado",
            "Sempre Ligado",
            "Fade In/Out",
            "Piscar",
            "Sync com SimHub"});
            this.cmdBoxLedMode.Location = new System.Drawing.Point(3, 29);
            this.cmdBoxLedMode.MaxDropDownItems = 4;
            this.cmdBoxLedMode.MouseState = MaterialSkin.MouseState.OUT;
            this.cmdBoxLedMode.Name = "cmdBoxLedMode";
            this.cmdBoxLedMode.Size = new System.Drawing.Size(553, 49);
            this.cmdBoxLedMode.StartIndex = 0;
            this.cmdBoxLedMode.TabIndex = 22;
            // 
            // SldLedBrightness
            // 
            this.SldLedBrightness.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.SldLedBrightness.Depth = 0;
            this.SldLedBrightness.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.SldLedBrightness.Location = new System.Drawing.Point(3, 84);
            this.SldLedBrightness.MouseState = MaterialSkin.MouseState.HOVER;
            this.SldLedBrightness.Name = "SldLedBrightness";
            this.SldLedBrightness.RangeMax = 255;
            this.SldLedBrightness.ShowValue = false;
            this.SldLedBrightness.Size = new System.Drawing.Size(553, 40);
            this.SldLedBrightness.TabIndex = 23;
            this.SldLedBrightness.Text = "Brilho";
            this.SldLedBrightness.Value = 255;
            // 
            // TxtProperties
            // 
            this.TxtProperties.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.TxtProperties.Depth = 0;
            this.TxtProperties.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TxtProperties.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TxtProperties.Location = new System.Drawing.Point(3, 219);
            this.TxtProperties.MouseState = MaterialSkin.MouseState.HOVER;
            this.TxtProperties.Name = "TxtProperties";
            this.TxtProperties.Size = new System.Drawing.Size(553, 19);
            this.TxtProperties.TabIndex = 26;
            this.TxtProperties.Text = "Propriedades";
            this.TxtProperties.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.TxtProperties.Visible = false;
            // 
            // PropertiesDivider
            // 
            this.PropertiesDivider.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.PropertiesDivider.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.PropertiesDivider.Depth = 0;
            this.PropertiesDivider.Location = new System.Drawing.Point(3, 241);
            this.PropertiesDivider.MouseState = MaterialSkin.MouseState.HOVER;
            this.PropertiesDivider.Name = "PropertiesDivider";
            this.PropertiesDivider.Size = new System.Drawing.Size(553, 1);
            this.PropertiesDivider.TabIndex = 27;
            this.PropertiesDivider.Text = "materialDivider2";
            this.PropertiesDivider.Visible = false;
            // 
            // dgvActions
            // 
            this.dgvActions.AllowUserToAddRows = false;
            this.dgvActions.AllowUserToDeleteRows = false;
            this.dgvActions.AllowUserToResizeColumns = false;
            this.dgvActions.AllowUserToResizeRows = false;
            this.dgvActions.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.dgvActions.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvActions.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvActions.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvActions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvActions.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Id,
            this.Ordem,
            this.dgvActionType,
            this.dgvActionEdit,
            this.dgvActionRemove,
            this.dgvActionMoveUp,
            this.dgvActionMoveDown});
            this.dgvActions.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgvActions.EnableHeadersVisualStyles = false;
            this.dgvActions.Location = new System.Drawing.Point(3, 248);
            this.dgvActions.MultiSelect = false;
            this.dgvActions.Name = "dgvActions";
            this.dgvActions.ReadOnly = true;
            this.dgvActions.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvActions.RowHeadersVisible = false;
            this.dgvActions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvActions.Size = new System.Drawing.Size(553, 195);
            this.dgvActions.TabIndex = 28;
            this.dgvActions.Visible = false;
            // 
            // Id
            // 
            this.Id.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Id.DataPropertyName = "Id";
            this.Id.HeaderText = "ID";
            this.Id.Name = "Id";
            this.Id.ReadOnly = true;
            this.Id.Visible = false;
            // 
            // Ordem
            // 
            this.Ordem.DataPropertyName = "Order";
            this.Ordem.HeaderText = "Ordem";
            this.Ordem.Name = "Ordem";
            this.Ordem.ReadOnly = true;
            this.Ordem.Visible = false;
            // 
            // dgvActionType
            // 
            this.dgvActionType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dgvActionType.DataPropertyName = "Name";
            this.dgvActionType.FillWeight = 200F;
            this.dgvActionType.HeaderText = "Ação";
            this.dgvActionType.Name = "dgvActionType";
            this.dgvActionType.ReadOnly = true;
            this.dgvActionType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // dgvActionEdit
            // 
            this.dgvActionEdit.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dgvActionEdit.FillWeight = 35F;
            this.dgvActionEdit.HeaderText = "";
            this.dgvActionEdit.Name = "dgvActionEdit";
            this.dgvActionEdit.ReadOnly = true;
            // 
            // dgvActionRemove
            // 
            this.dgvActionRemove.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dgvActionRemove.FillWeight = 35F;
            this.dgvActionRemove.HeaderText = "";
            this.dgvActionRemove.Name = "dgvActionRemove";
            this.dgvActionRemove.ReadOnly = true;
            // 
            // dgvActionMoveUp
            // 
            this.dgvActionMoveUp.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dgvActionMoveUp.FillWeight = 35F;
            this.dgvActionMoveUp.HeaderText = "";
            this.dgvActionMoveUp.Name = "dgvActionMoveUp";
            this.dgvActionMoveUp.ReadOnly = true;
            // 
            // dgvActionMoveDown
            // 
            this.dgvActionMoveDown.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dgvActionMoveDown.FillWeight = 35F;
            this.dgvActionMoveDown.HeaderText = "";
            this.dgvActionMoveDown.Name = "dgvActionMoveDown";
            this.dgvActionMoveDown.ReadOnly = true;
            // 
            // JJB01_V2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Controls.Add(this.FlpJJB01V2);
            this.Controls.Add(this.btnSaveAndCloseConfig);
            this.Controls.Add(this.materialDivider1);
            this.Controls.Add(this.btnSaveConfig);
            this.Controls.Add(this.BtnRemoveProfile);
            this.Controls.Add(this.BtnAddProfile);
            this.Controls.Add(this.CmbBoxSelectProfile);
            this.Controls.Add(this.ImgJJB01V2Off);
            this.Controls.Add(this.ImgJJB01V2On);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1000, 700);
            this.MinimumSize = new System.Drawing.Size(1000, 700);
            this.Name = "JJB01_V2";
            this.Sizable = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ButtonBox JJB-01 V2";
            ((System.ComponentModel.ISupportInitialize)(this.ImgJJB01V2Off)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImgJJB01V2On)).EndInit();
            this.FlpJJB01V2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvActions)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private MaterialSkin.Controls.MaterialComboBox CmbBoxSelectProfile;
        private MaterialSkin.Controls.MaterialButton BtnAddProfile;
        private MaterialSkin.Controls.MaterialButton BtnRemoveProfile;
        private MaterialSkin.Controls.MaterialButton btnSaveConfig;
        private System.Windows.Forms.PictureBox ImgJJB01V2Off;
        private System.Windows.Forms.PictureBox ImgJJB01V2On;
        private MaterialSkin.Controls.MaterialDivider materialDivider1;
        private MaterialSkin.Controls.MaterialButton btnSaveAndCloseConfig;
        private System.Windows.Forms.FlowLayoutPanel FlpJJB01V2;
        private MaterialSkin.Controls.MaterialSlider SldBlinkSpeed;
        private MaterialSkin.Controls.MaterialSlider SldPulseSpeed;
        private MaterialSkin.Controls.MaterialSlider SldLedBrightness;
        private MaterialSkin.Controls.MaterialComboBox cmdBoxLedMode;
        private MaterialSkin.Controls.MaterialButton BtnAddLedAction;
        private System.Windows.Forms.DataGridView dgvActions;
        private System.Windows.Forms.DataGridViewTextBoxColumn Id;
        private System.Windows.Forms.DataGridViewTextBoxColumn Ordem;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvActionType;
        private System.Windows.Forms.DataGridViewButtonColumn dgvActionEdit;
        private System.Windows.Forms.DataGridViewButtonColumn dgvActionRemove;
        private System.Windows.Forms.DataGridViewButtonColumn dgvActionMoveUp;
        private System.Windows.Forms.DataGridViewButtonColumn dgvActionMoveDown;
        private MaterialSkin.Controls.MaterialDivider PropertiesDivider;
        private MaterialSkin.Controls.MaterialLabel TxtGeneralConfigs;
        private MaterialSkin.Controls.MaterialDivider materialDivider3;
        private MaterialSkin.Controls.MaterialLabel TxtProperties;
    }
}