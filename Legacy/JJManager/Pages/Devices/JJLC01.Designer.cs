namespace JJManager.Pages.Devices
{
    partial class JJLC01
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
            this.ChtLoadCellJJLC01 = new System.Windows.Forms.Integration.ElementHost();
            this.ChtLoadCell = new LiveCharts.Wpf.CartesianChart();
            this.BtnClose = new MaterialSkin.Controls.MaterialButton();
            this.materialDivider2 = new MaterialSkin.Controls.MaterialDivider();
            this.BtnSave = new MaterialSkin.Controls.MaterialButton();
            this.BtnRemoveProfile = new MaterialSkin.Controls.MaterialButton();
            this.BtnAddProfile = new MaterialSkin.Controls.MaterialButton();
            this.CmbBoxSelectProfile = new MaterialSkin.Controls.MaterialComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.TxtLoadCellDataJJLC01 = new MaterialSkin.Controls.MaterialLabel();
            this.TxtPotDataJJLC01 = new MaterialSkin.Controls.MaterialLabel();
            this.materialDivider1 = new MaterialSkin.Controls.MaterialDivider();
            this.TxtTitleDataJJLC01 = new MaterialSkin.Controls.MaterialLabel();
            this.BtnSaveLoadCellPoint = new MaterialSkin.Controls.MaterialButton();
            this.TxtLCWeight = new MaterialSkin.Controls.MaterialTextBox2();
            this.materialTabSelector1 = new MaterialSkin.Controls.MaterialTabSelector();
            this.TbcJJLC01 = new MaterialSkin.Controls.MaterialTabControl();
            this.TbcJJLC01Settings = new System.Windows.Forms.TabPage();
            this.TbcJJLC01Calibration = new System.Windows.Forms.TabPage();
            this.TxtJJLC01FineOffset = new MaterialSkin.Controls.MaterialLabel();
            this.TxtJJLC01RawData = new MaterialSkin.Controls.MaterialLabel();
            this.SldFineOffsetJJLC01 = new MaterialSkin.Controls.MaterialSlider();
            this.TxtJJLC01Calibration = new MaterialSkin.Controls.MaterialLabel();
            this.panel1.SuspendLayout();
            this.TbcJJLC01.SuspendLayout();
            this.TbcJJLC01Settings.SuspendLayout();
            this.TbcJJLC01Calibration.SuspendLayout();
            this.SuspendLayout();
            // 
            // ChtLoadCellJJLC01
            // 
            this.ChtLoadCellJJLC01.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ChtLoadCellJJLC01.Location = new System.Drawing.Point(5, 55);
            this.ChtLoadCellJJLC01.Name = "ChtLoadCellJJLC01";
            this.ChtLoadCellJJLC01.Size = new System.Drawing.Size(571, 299);
            this.ChtLoadCellJJLC01.TabIndex = 0;
            this.ChtLoadCellJJLC01.Text = "elementHost1";
            this.ChtLoadCellJJLC01.Child = this.ChtLoadCell;
            // 
            // BtnClose
            // 
            this.BtnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnClose.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnClose.Depth = 0;
            this.BtnClose.HighEmphasis = true;
            this.BtnClose.Icon = null;
            this.BtnClose.Location = new System.Drawing.Point(728, 588);
            this.BtnClose.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnClose.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnClose.Name = "BtnClose";
            this.BtnClose.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnClose.Size = new System.Drawing.Size(77, 36);
            this.BtnClose.TabIndex = 24;
            this.BtnClose.Text = "Fechar";
            this.BtnClose.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnClose.UseAccentColor = false;
            this.BtnClose.UseVisualStyleBackColor = true;
            this.BtnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // materialDivider2
            // 
            this.materialDivider2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.materialDivider2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialDivider2.Depth = 0;
            this.materialDivider2.Location = new System.Drawing.Point(13, 578);
            this.materialDivider2.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialDivider2.Name = "materialDivider2";
            this.materialDivider2.Size = new System.Drawing.Size(792, 1);
            this.materialDivider2.TabIndex = 23;
            this.materialDivider2.Text = "materialDivider2";
            // 
            // BtnSave
            // 
            this.BtnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnSave.AutoSize = false;
            this.BtnSave.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnSave.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnSave.Depth = 0;
            this.BtnSave.HighEmphasis = true;
            this.BtnSave.Icon = null;
            this.BtnSave.Location = new System.Drawing.Point(501, 588);
            this.BtnSave.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnSave.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnSave.Name = "BtnSave";
            this.BtnSave.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnSave.Size = new System.Drawing.Size(219, 36);
            this.BtnSave.TabIndex = 22;
            this.BtnSave.Text = "Conectar";
            this.BtnSave.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnSave.UseAccentColor = false;
            this.BtnSave.UseVisualStyleBackColor = true;
            this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
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
            this.BtnRemoveProfile.Location = new System.Drawing.Point(684, 0);
            this.BtnRemoveProfile.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnRemoveProfile.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnRemoveProfile.Name = "BtnRemoveProfile";
            this.BtnRemoveProfile.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnRemoveProfile.Size = new System.Drawing.Size(50, 50);
            this.BtnRemoveProfile.TabIndex = 21;
            this.BtnRemoveProfile.Text = "-";
            this.BtnRemoveProfile.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnRemoveProfile.UseAccentColor = false;
            this.BtnRemoveProfile.UseVisualStyleBackColor = true;
            this.BtnRemoveProfile.Click += new System.EventHandler(this.BtnRemoveProfile_Click);
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
            this.BtnAddProfile.Location = new System.Drawing.Point(742, 0);
            this.BtnAddProfile.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnAddProfile.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnAddProfile.Name = "BtnAddProfile";
            this.BtnAddProfile.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnAddProfile.Size = new System.Drawing.Size(50, 50);
            this.BtnAddProfile.TabIndex = 20;
            this.BtnAddProfile.Text = "+";
            this.BtnAddProfile.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnAddProfile.UseAccentColor = false;
            this.BtnAddProfile.UseVisualStyleBackColor = true;
            this.BtnAddProfile.Click += new System.EventHandler(this.BtnAddProfile_Click);
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
            this.CmbBoxSelectProfile.Location = new System.Drawing.Point(6, 0);
            this.CmbBoxSelectProfile.MaxDropDownItems = 4;
            this.CmbBoxSelectProfile.MouseState = MaterialSkin.MouseState.OUT;
            this.CmbBoxSelectProfile.Name = "CmbBoxSelectProfile";
            this.CmbBoxSelectProfile.Size = new System.Drawing.Size(671, 49);
            this.CmbBoxSelectProfile.StartIndex = 0;
            this.CmbBoxSelectProfile.TabIndex = 19;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.TxtLoadCellDataJJLC01);
            this.panel1.Controls.Add(this.TxtPotDataJJLC01);
            this.panel1.Controls.Add(this.materialDivider1);
            this.panel1.Controls.Add(this.TxtTitleDataJJLC01);
            this.panel1.Location = new System.Drawing.Point(583, 56);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(209, 349);
            this.panel1.TabIndex = 25;
            // 
            // TxtLoadCellDataJJLC01
            // 
            this.TxtLoadCellDataJJLC01.AutoSize = true;
            this.TxtLoadCellDataJJLC01.Depth = 0;
            this.TxtLoadCellDataJJLC01.Dock = System.Windows.Forms.DockStyle.Top;
            this.TxtLoadCellDataJJLC01.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TxtLoadCellDataJJLC01.Location = new System.Drawing.Point(0, 39);
            this.TxtLoadCellDataJJLC01.MouseState = MaterialSkin.MouseState.HOVER;
            this.TxtLoadCellDataJJLC01.Name = "TxtLoadCellDataJJLC01";
            this.TxtLoadCellDataJJLC01.Size = new System.Drawing.Size(170, 19);
            this.TxtLoadCellDataJJLC01.TabIndex = 9;
            this.TxtLoadCellDataJJLC01.Text = "Kilos Pressionado: 0 KG";
            // 
            // TxtPotDataJJLC01
            // 
            this.TxtPotDataJJLC01.AutoSize = true;
            this.TxtPotDataJJLC01.Depth = 0;
            this.TxtPotDataJJLC01.Dock = System.Windows.Forms.DockStyle.Top;
            this.TxtPotDataJJLC01.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TxtPotDataJJLC01.Location = new System.Drawing.Point(0, 20);
            this.TxtPotDataJJLC01.MouseState = MaterialSkin.MouseState.HOVER;
            this.TxtPotDataJJLC01.Name = "TxtPotDataJJLC01";
            this.TxtPotDataJJLC01.Size = new System.Drawing.Size(150, 19);
            this.TxtPotDataJJLC01.TabIndex = 6;
            this.TxtPotDataJJLC01.Text = "Pot. Pressionado: 0%";
            // 
            // materialDivider1
            // 
            this.materialDivider1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialDivider1.Depth = 0;
            this.materialDivider1.Dock = System.Windows.Forms.DockStyle.Top;
            this.materialDivider1.Location = new System.Drawing.Point(0, 19);
            this.materialDivider1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialDivider1.Name = "materialDivider1";
            this.materialDivider1.Size = new System.Drawing.Size(209, 1);
            this.materialDivider1.TabIndex = 1;
            this.materialDivider1.Text = "materialDivider1";
            // 
            // TxtTitleDataJJLC01
            // 
            this.TxtTitleDataJJLC01.AutoSize = true;
            this.TxtTitleDataJJLC01.Depth = 0;
            this.TxtTitleDataJJLC01.Dock = System.Windows.Forms.DockStyle.Top;
            this.TxtTitleDataJJLC01.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TxtTitleDataJJLC01.Location = new System.Drawing.Point(0, 0);
            this.TxtTitleDataJJLC01.MouseState = MaterialSkin.MouseState.HOVER;
            this.TxtTitleDataJJLC01.Name = "TxtTitleDataJJLC01";
            this.TxtTitleDataJJLC01.Size = new System.Drawing.Size(95, 19);
            this.TxtTitleDataJJLC01.TabIndex = 5;
            this.TxtTitleDataJJLC01.Text = "Dados atuais";
            // 
            // BtnSaveLoadCellPoint
            // 
            this.BtnSaveLoadCellPoint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnSaveLoadCellPoint.AutoSize = false;
            this.BtnSaveLoadCellPoint.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnSaveLoadCellPoint.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnSaveLoadCellPoint.Depth = 0;
            this.BtnSaveLoadCellPoint.Enabled = false;
            this.BtnSaveLoadCellPoint.HighEmphasis = true;
            this.BtnSaveLoadCellPoint.Icon = null;
            this.BtnSaveLoadCellPoint.Location = new System.Drawing.Point(480, 363);
            this.BtnSaveLoadCellPoint.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnSaveLoadCellPoint.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnSaveLoadCellPoint.Name = "BtnSaveLoadCellPoint";
            this.BtnSaveLoadCellPoint.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnSaveLoadCellPoint.Size = new System.Drawing.Size(96, 48);
            this.BtnSaveLoadCellPoint.TabIndex = 27;
            this.BtnSaveLoadCellPoint.Text = "Atualizar";
            this.BtnSaveLoadCellPoint.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnSaveLoadCellPoint.UseAccentColor = false;
            this.BtnSaveLoadCellPoint.UseVisualStyleBackColor = true;
            this.BtnSaveLoadCellPoint.Click += new System.EventHandler(this.BtnSaveLoadCellPoint_Click);
            // 
            // TxtLCWeight
            // 
            this.TxtLCWeight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtLCWeight.AnimateReadOnly = false;
            this.TxtLCWeight.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.TxtLCWeight.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.TxtLCWeight.Depth = 0;
            this.TxtLCWeight.Enabled = false;
            this.TxtLCWeight.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TxtLCWeight.HideSelection = true;
            this.TxtLCWeight.LeadingIcon = null;
            this.TxtLCWeight.Location = new System.Drawing.Point(6, 363);
            this.TxtLCWeight.MaxLength = 32767;
            this.TxtLCWeight.MouseState = MaterialSkin.MouseState.OUT;
            this.TxtLCWeight.Name = "TxtLCWeight";
            this.TxtLCWeight.PasswordChar = '\0';
            this.TxtLCWeight.PrefixSuffixText = null;
            this.TxtLCWeight.ReadOnly = false;
            this.TxtLCWeight.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.TxtLCWeight.SelectedText = "";
            this.TxtLCWeight.SelectionLength = 0;
            this.TxtLCWeight.SelectionStart = 0;
            this.TxtLCWeight.ShortcutsEnabled = true;
            this.TxtLCWeight.Size = new System.Drawing.Size(467, 48);
            this.TxtLCWeight.TabIndex = 28;
            this.TxtLCWeight.TabStop = false;
            this.TxtLCWeight.Text = "Clique em um ponto para edita-lo";
            this.TxtLCWeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TxtLCWeight.TrailingIcon = null;
            this.TxtLCWeight.UseSystemPasswordChar = false;
            this.TxtLCWeight.Enter += new System.EventHandler(this.TxtLCWeight_Enter);
            this.TxtLCWeight.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtLCWeight_KeyPress);
            this.TxtLCWeight.Leave += new System.EventHandler(this.TxtLCWeight_Leave);
            this.TxtLCWeight.TextChanged += new System.EventHandler(this.TxtLCWeight_TextChanged);
            // 
            // materialTabSelector1
            // 
            this.materialTabSelector1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.materialTabSelector1.BaseTabControl = this.TbcJJLC01;
            this.materialTabSelector1.CharacterCasing = MaterialSkin.Controls.MaterialTabSelector.CustomCharacterCasing.Normal;
            this.materialTabSelector1.Depth = 0;
            this.materialTabSelector1.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.materialTabSelector1.Location = new System.Drawing.Point(0, 64);
            this.materialTabSelector1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialTabSelector1.Name = "materialTabSelector1";
            this.materialTabSelector1.Size = new System.Drawing.Size(815, 48);
            this.materialTabSelector1.TabIndex = 29;
            this.materialTabSelector1.Text = "materialTabSelector1";
            // 
            // TbcJJLC01
            // 
            this.TbcJJLC01.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TbcJJLC01.Controls.Add(this.TbcJJLC01Settings);
            this.TbcJJLC01.Controls.Add(this.TbcJJLC01Calibration);
            this.TbcJJLC01.Depth = 0;
            this.TbcJJLC01.Location = new System.Drawing.Point(7, 119);
            this.TbcJJLC01.MouseState = MaterialSkin.MouseState.HOVER;
            this.TbcJJLC01.Multiline = true;
            this.TbcJJLC01.Name = "TbcJJLC01";
            this.TbcJJLC01.SelectedIndex = 0;
            this.TbcJJLC01.Size = new System.Drawing.Size(802, 455);
            this.TbcJJLC01.TabIndex = 30;
            // 
            // TbcJJLC01Settings
            // 
            this.TbcJJLC01Settings.Controls.Add(this.CmbBoxSelectProfile);
            this.TbcJJLC01Settings.Controls.Add(this.ChtLoadCellJJLC01);
            this.TbcJJLC01Settings.Controls.Add(this.TxtLCWeight);
            this.TbcJJLC01Settings.Controls.Add(this.BtnAddProfile);
            this.TbcJJLC01Settings.Controls.Add(this.BtnSaveLoadCellPoint);
            this.TbcJJLC01Settings.Controls.Add(this.BtnRemoveProfile);
            this.TbcJJLC01Settings.Controls.Add(this.panel1);
            this.TbcJJLC01Settings.Location = new System.Drawing.Point(4, 22);
            this.TbcJJLC01Settings.Name = "TbcJJLC01Settings";
            this.TbcJJLC01Settings.Padding = new System.Windows.Forms.Padding(3);
            this.TbcJJLC01Settings.Size = new System.Drawing.Size(794, 429);
            this.TbcJJLC01Settings.TabIndex = 0;
            this.TbcJJLC01Settings.Text = "Ajustes";
            this.TbcJJLC01Settings.UseVisualStyleBackColor = true;
            // 
            // TbcJJLC01Calibration
            // 
            this.TbcJJLC01Calibration.Controls.Add(this.TxtJJLC01FineOffset);
            this.TbcJJLC01Calibration.Controls.Add(this.TxtJJLC01RawData);
            this.TbcJJLC01Calibration.Controls.Add(this.SldFineOffsetJJLC01);
            this.TbcJJLC01Calibration.Controls.Add(this.TxtJJLC01Calibration);
            this.TbcJJLC01Calibration.Location = new System.Drawing.Point(4, 22);
            this.TbcJJLC01Calibration.Name = "TbcJJLC01Calibration";
            this.TbcJJLC01Calibration.Padding = new System.Windows.Forms.Padding(3);
            this.TbcJJLC01Calibration.Size = new System.Drawing.Size(794, 429);
            this.TbcJJLC01Calibration.TabIndex = 1;
            this.TbcJJLC01Calibration.Text = "Calibração";
            this.TbcJJLC01Calibration.UseVisualStyleBackColor = true;
            this.TbcJJLC01Calibration.Click += new System.EventHandler(this.TbcJJLC01Calibration_Click);
            // 
            // TxtJJLC01FineOffset
            // 
            this.TxtJJLC01FineOffset.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtJJLC01FineOffset.Depth = 0;
            this.TxtJJLC01FineOffset.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TxtJJLC01FineOffset.Location = new System.Drawing.Point(52, 344);
            this.TxtJJLC01FineOffset.MouseState = MaterialSkin.MouseState.HOVER;
            this.TxtJJLC01FineOffset.Name = "TxtJJLC01FineOffset";
            this.TxtJJLC01FineOffset.Size = new System.Drawing.Size(693, 23);
            this.TxtJJLC01FineOffset.TabIndex = 34;
            this.TxtJJLC01FineOffset.Text = "Valor do ajuste fino: N/A";
            this.TxtJJLC01FineOffset.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TxtJJLC01RawData
            // 
            this.TxtJJLC01RawData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtJJLC01RawData.Depth = 0;
            this.TxtJJLC01RawData.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TxtJJLC01RawData.Location = new System.Drawing.Point(52, 367);
            this.TxtJJLC01RawData.MouseState = MaterialSkin.MouseState.HOVER;
            this.TxtJJLC01RawData.Name = "TxtJJLC01RawData";
            this.TxtJJLC01RawData.Size = new System.Drawing.Size(693, 23);
            this.TxtJJLC01RawData.TabIndex = 33;
            this.TxtJJLC01RawData.Text = "Valor atual: 0";
            this.TxtJJLC01RawData.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SldFineOffsetJJLC01
            // 
            this.SldFineOffsetJJLC01.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SldFineOffsetJJLC01.Depth = 0;
            this.SldFineOffsetJJLC01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.SldFineOffsetJJLC01.Location = new System.Drawing.Point(51, 301);
            this.SldFineOffsetJJLC01.MouseState = MaterialSkin.MouseState.HOVER;
            this.SldFineOffsetJJLC01.Name = "SldFineOffsetJJLC01";
            this.SldFineOffsetJJLC01.RangeMax = 300;
            this.SldFineOffsetJJLC01.ShowText = false;
            this.SldFineOffsetJJLC01.ShowValue = false;
            this.SldFineOffsetJJLC01.Size = new System.Drawing.Size(693, 40);
            this.SldFineOffsetJJLC01.TabIndex = 32;
            this.SldFineOffsetJJLC01.Text = "materialSlider1";
            this.SldFineOffsetJJLC01.onValueChanged += new MaterialSkin.Controls.MaterialSlider.ValueChanged(this.SldFineOffsetJJLC01_onValueChanged);
            // 
            // TxtJJLC01Calibration
            // 
            this.TxtJJLC01Calibration.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtJJLC01Calibration.Depth = 0;
            this.TxtJJLC01Calibration.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TxtJJLC01Calibration.Location = new System.Drawing.Point(49, 71);
            this.TxtJJLC01Calibration.MouseState = MaterialSkin.MouseState.HOVER;
            this.TxtJJLC01Calibration.Name = "TxtJJLC01Calibration";
            this.TxtJJLC01Calibration.Size = new System.Drawing.Size(696, 215);
            this.TxtJJLC01Calibration.TabIndex = 0;
            this.TxtJJLC01Calibration.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.TxtJJLC01Calibration.Click += new System.EventHandler(this.TxtJJLC01Calibration_Click);
            // 
            // JJLC01
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(812, 633);
            this.Controls.Add(this.TbcJJLC01);
            this.Controls.Add(this.materialTabSelector1);
            this.Controls.Add(this.BtnClose);
            this.Controls.Add(this.materialDivider2);
            this.Controls.Add(this.BtnSave);
            this.Icon = global::JJManager.Properties.Resources.JJManagerIcon_256;
            this.Name = "JJLC01";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Load Cell JJLC-01";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.TbcJJLC01.ResumeLayout(false);
            this.TbcJJLC01Settings.ResumeLayout(false);
            this.TbcJJLC01Calibration.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost ChtLoadCellJJLC01;
        private LiveCharts.Wpf.CartesianChart ChtLoadCell;
        private MaterialSkin.Controls.MaterialButton BtnClose;
        private MaterialSkin.Controls.MaterialDivider materialDivider2;
        private MaterialSkin.Controls.MaterialButton BtnSave;
        private MaterialSkin.Controls.MaterialButton BtnRemoveProfile;
        private MaterialSkin.Controls.MaterialButton BtnAddProfile;
        private MaterialSkin.Controls.MaterialComboBox CmbBoxSelectProfile;
        private System.Windows.Forms.Panel panel1;
        private MaterialSkin.Controls.MaterialDivider materialDivider1;
        private MaterialSkin.Controls.MaterialLabel TxtLoadCellDataJJLC01;
        private MaterialSkin.Controls.MaterialLabel TxtPotDataJJLC01;
        private MaterialSkin.Controls.MaterialLabel TxtTitleDataJJLC01;
        private MaterialSkin.Controls.MaterialButton BtnSaveLoadCellPoint;
        private MaterialSkin.Controls.MaterialTextBox2 TxtLCWeight;
        private MaterialSkin.Controls.MaterialTabSelector materialTabSelector1;
        private MaterialSkin.Controls.MaterialTabControl TbcJJLC01;
        private System.Windows.Forms.TabPage TbcJJLC01Settings;
        private System.Windows.Forms.TabPage TbcJJLC01Calibration;
        private MaterialSkin.Controls.MaterialLabel TxtJJLC01Calibration;
        private MaterialSkin.Controls.MaterialSlider SldFineOffsetJJLC01;
        private MaterialSkin.Controls.MaterialLabel TxtJJLC01RawData;
        private MaterialSkin.Controls.MaterialLabel TxtJJLC01FineOffset;
    }
}