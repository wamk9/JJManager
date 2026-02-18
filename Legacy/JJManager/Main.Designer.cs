namespace JJManager
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
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.tabMain = new MaterialSkin.Controls.MaterialTabControl();
            this.tabConnect = new System.Windows.Forms.TabPage();
            this.DgvDevices = new System.Windows.Forms.DataGridView();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.txtStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnConnChanger = new MaterialSkin.Controls.MaterialButton();
            this.btnEditDevice = new MaterialSkin.Controls.MaterialButton();
            this.btnSearchBluetooth = new MaterialSkin.Controls.MaterialButton();
            this.tabOptions = new System.Windows.Forms.TabPage();
            this.flpOptions = new System.Windows.Forms.FlowLayoutPanel();
            this.LblThemeColor = new MaterialSkin.Controls.MaterialLabel();
            this.SwtThemeColor = new MaterialSkin.Controls.MaterialSwitch();
            this.txtStartOnBoot = new MaterialSkin.Controls.MaterialLabel();
            this.swtStartOnBoot = new MaterialSkin.Controls.MaterialSwitch();
            this.txtLog = new MaterialSkin.Controls.MaterialLabel();
            this.dgvLog = new System.Windows.Forms.DataGridView();
            this.colModule = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvLogSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvLogOpen = new System.Windows.Forms.DataGridViewButtonColumn();
            this.dgvLogRemove = new System.Windows.Forms.DataGridViewButtonColumn();
            this.btnRemoveAllLogs = new MaterialSkin.Controls.MaterialButton();
            this.tabUpdate = new System.Windows.Forms.TabPage();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.txtStatusUpdate = new System.Windows.Forms.ToolStripStatusLabel();
            this.progressBarDownload = new System.Windows.Forms.ToolStripProgressBar();
            this.lvDevicesToUpdate = new MaterialSkin.Controls.MaterialListView();
            this.DeviceId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DeviceName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DeviceActualVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DeviceLastVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.BtnUpdateDevice = new MaterialSkin.Controls.MaterialButton();
            this.tabAbout = new System.Windows.Forms.TabPage();
            this.lblAboutText = new MaterialSkin.Controls.MaterialLabel();
            this.lblAboutVersion = new MaterialSkin.Controls.MaterialLabel();
            this.ImgAbout = new System.Windows.Forms.PictureBox();
            this.tbsMainMenu = new MaterialSkin.Controls.MaterialTabSelector();
            this.tabMain.SuspendLayout();
            this.tabConnect.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DgvDevices)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.tabOptions.SuspendLayout();
            this.flpOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLog)).BeginInit();
            this.tabUpdate.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tabAbout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImgAbout)).BeginInit();
            this.SuspendLayout();
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "JJManager_icones_update_16.png");
            this.imageList.Images.SetKeyName(1, "JJManager_icones_connect.png");
            this.imageList.Images.SetKeyName(2, "JJManager_icones_options_16.png");
            // 
            // tabMain
            // 
            this.tabMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabMain.Controls.Add(this.tabConnect);
            this.tabMain.Controls.Add(this.tabOptions);
            this.tabMain.Controls.Add(this.tabUpdate);
            this.tabMain.Controls.Add(this.tabAbout);
            this.tabMain.Depth = 0;
            this.tabMain.ImageList = this.imageList;
            this.tabMain.Location = new System.Drawing.Point(3, 118);
            this.tabMain.MouseState = MaterialSkin.MouseState.HOVER;
            this.tabMain.Multiline = true;
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(712, 400);
            this.tabMain.TabIndex = 21;
            // 
            // tabConnect
            // 
            this.tabConnect.Controls.Add(this.DgvDevices);
            this.tabConnect.Controls.Add(this.statusStrip);
            this.tabConnect.Controls.Add(this.btnConnChanger);
            this.tabConnect.Controls.Add(this.btnEditDevice);
            this.tabConnect.Controls.Add(this.btnSearchBluetooth);
            this.tabConnect.ImageKey = "JJManager_icones_connect.png";
            this.tabConnect.Location = new System.Drawing.Point(4, 23);
            this.tabConnect.Name = "tabConnect";
            this.tabConnect.Padding = new System.Windows.Forms.Padding(3);
            this.tabConnect.Size = new System.Drawing.Size(704, 373);
            this.tabConnect.TabIndex = 0;
            this.tabConnect.Text = "Gerenciar Dispositivos";
            this.tabConnect.UseVisualStyleBackColor = true;
            // 
            // DgvDevices
            // 
            this.DgvDevices.AllowUserToAddRows = false;
            this.DgvDevices.AllowUserToDeleteRows = false;
            this.DgvDevices.AllowUserToResizeColumns = false;
            this.DgvDevices.AllowUserToResizeRows = false;
            this.DgvDevices.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DgvDevices.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.DgvDevices.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DgvDevices.Location = new System.Drawing.Point(9, 7);
            this.DgvDevices.MultiSelect = false;
            this.DgvDevices.Name = "DgvDevices";
            this.DgvDevices.ReadOnly = true;
            this.DgvDevices.RowHeadersVisible = false;
            this.DgvDevices.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.DgvDevices.Size = new System.Drawing.Size(689, 293);
            this.DgvDevices.TabIndex = 7;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.txtStatus});
            this.statusStrip.Location = new System.Drawing.Point(3, 348);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(698, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 6;
            this.statusStrip.Text = "statusStrip";
            // 
            // txtStatus
            // 
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.Size = new System.Drawing.Size(0, 17);
            // 
            // btnConnChanger
            // 
            this.btnConnChanger.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConnChanger.AutoSize = false;
            this.btnConnChanger.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnConnChanger.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnConnChanger.Depth = 0;
            this.btnConnChanger.Enabled = false;
            this.btnConnChanger.HighEmphasis = true;
            this.btnConnChanger.Icon = null;
            this.btnConnChanger.Location = new System.Drawing.Point(9, 309);
            this.btnConnChanger.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnConnChanger.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnConnChanger.Name = "btnConnChanger";
            this.btnConnChanger.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnConnChanger.Size = new System.Drawing.Size(218, 36);
            this.btnConnChanger.TabIndex = 5;
            this.btnConnChanger.Text = "Conectar/Desconectar";
            this.btnConnChanger.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnConnChanger.UseAccentColor = false;
            this.btnConnChanger.UseVisualStyleBackColor = true;
            this.btnConnChanger.Click += new System.EventHandler(this.btnConnChanger_Click);
            // 
            // btnEditDevice
            // 
            this.btnEditDevice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEditDevice.AutoSize = false;
            this.btnEditDevice.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnEditDevice.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnEditDevice.Depth = 0;
            this.btnEditDevice.Enabled = false;
            this.btnEditDevice.HighEmphasis = true;
            this.btnEditDevice.Icon = null;
            this.btnEditDevice.Location = new System.Drawing.Point(245, 309);
            this.btnEditDevice.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnEditDevice.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnEditDevice.Name = "btnEditDevice";
            this.btnEditDevice.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnEditDevice.Size = new System.Drawing.Size(218, 36);
            this.btnEditDevice.TabIndex = 4;
            this.btnEditDevice.Text = "Editar Dispositivo";
            this.btnEditDevice.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnEditDevice.UseAccentColor = false;
            this.btnEditDevice.UseVisualStyleBackColor = true;
            this.btnEditDevice.Click += new System.EventHandler(this.btnEditDevice_Click);
            // 
            // btnSearchBluetooth
            // 
            this.btnSearchBluetooth.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearchBluetooth.AutoSize = false;
            this.btnSearchBluetooth.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSearchBluetooth.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSearchBluetooth.Depth = 0;
            this.btnSearchBluetooth.Enabled = false;
            this.btnSearchBluetooth.HighEmphasis = true;
            this.btnSearchBluetooth.Icon = null;
            this.btnSearchBluetooth.Location = new System.Drawing.Point(481, 309);
            this.btnSearchBluetooth.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSearchBluetooth.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSearchBluetooth.Name = "btnSearchBluetooth";
            this.btnSearchBluetooth.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSearchBluetooth.Size = new System.Drawing.Size(218, 36);
            this.btnSearchBluetooth.TabIndex = 3;
            this.btnSearchBluetooth.Text = "Buscar Disp. Bluetooth";
            this.btnSearchBluetooth.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSearchBluetooth.UseAccentColor = false;
            this.btnSearchBluetooth.UseVisualStyleBackColor = true;
            this.btnSearchBluetooth.Click += new System.EventHandler(this.btnSearchBluetooth_Click);
            // 
            // tabOptions
            // 
            this.tabOptions.Controls.Add(this.flpOptions);
            this.tabOptions.ImageKey = "JJManager_icones_options_16.png";
            this.tabOptions.Location = new System.Drawing.Point(4, 23);
            this.tabOptions.Name = "tabOptions";
            this.tabOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabOptions.Size = new System.Drawing.Size(704, 373);
            this.tabOptions.TabIndex = 1;
            this.tabOptions.Text = "Opções";
            this.tabOptions.UseVisualStyleBackColor = true;
            // 
            // flpOptions
            // 
            this.flpOptions.Controls.Add(this.LblThemeColor);
            this.flpOptions.Controls.Add(this.SwtThemeColor);
            this.flpOptions.Controls.Add(this.txtStartOnBoot);
            this.flpOptions.Controls.Add(this.swtStartOnBoot);
            this.flpOptions.Controls.Add(this.txtLog);
            this.flpOptions.Controls.Add(this.dgvLog);
            this.flpOptions.Controls.Add(this.btnRemoveAllLogs);
            this.flpOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpOptions.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpOptions.Location = new System.Drawing.Point(3, 3);
            this.flpOptions.Name = "flpOptions";
            this.flpOptions.Size = new System.Drawing.Size(698, 367);
            this.flpOptions.TabIndex = 2;
            this.flpOptions.WrapContents = false;
            // 
            // LblThemeColor
            // 
            this.LblThemeColor.AutoSize = true;
            this.LblThemeColor.Depth = 0;
            this.LblThemeColor.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.LblThemeColor.Location = new System.Drawing.Point(3, 0);
            this.LblThemeColor.MouseState = MaterialSkin.MouseState.HOVER;
            this.LblThemeColor.Name = "LblThemeColor";
            this.LblThemeColor.Size = new System.Drawing.Size(92, 19);
            this.LblThemeColor.TabIndex = 1;
            this.LblThemeColor.Text = "Cor do Tema";
            // 
            // SwtThemeColor
            // 
            this.SwtThemeColor.AutoSize = true;
            this.SwtThemeColor.Checked = true;
            this.SwtThemeColor.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SwtThemeColor.Depth = 0;
            this.SwtThemeColor.Location = new System.Drawing.Point(0, 19);
            this.SwtThemeColor.Margin = new System.Windows.Forms.Padding(0);
            this.SwtThemeColor.MouseLocation = new System.Drawing.Point(-1, -1);
            this.SwtThemeColor.MouseState = MaterialSkin.MouseState.HOVER;
            this.SwtThemeColor.Name = "SwtThemeColor";
            this.SwtThemeColor.Ripple = true;
            this.SwtThemeColor.Size = new System.Drawing.Size(106, 37);
            this.SwtThemeColor.TabIndex = 0;
            this.SwtThemeColor.Text = "Escuro";
            this.SwtThemeColor.UseVisualStyleBackColor = true;
            this.SwtThemeColor.CheckedChanged += new System.EventHandler(this.SwtThemeColor_CheckedChanged);
            // 
            // txtStartOnBoot
            // 
            this.txtStartOnBoot.AutoSize = true;
            this.txtStartOnBoot.Depth = 0;
            this.txtStartOnBoot.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.txtStartOnBoot.Location = new System.Drawing.Point(3, 56);
            this.txtStartOnBoot.MouseState = MaterialSkin.MouseState.HOVER;
            this.txtStartOnBoot.Name = "txtStartOnBoot";
            this.txtStartOnBoot.Size = new System.Drawing.Size(161, 19);
            this.txtStartOnBoot.TabIndex = 3;
            this.txtStartOnBoot.Text = "Iniciar com o Windows";
            // 
            // swtStartOnBoot
            // 
            this.swtStartOnBoot.AutoSize = true;
            this.swtStartOnBoot.Depth = 0;
            this.swtStartOnBoot.Location = new System.Drawing.Point(0, 75);
            this.swtStartOnBoot.Margin = new System.Windows.Forms.Padding(0);
            this.swtStartOnBoot.MouseLocation = new System.Drawing.Point(-1, -1);
            this.swtStartOnBoot.MouseState = MaterialSkin.MouseState.HOVER;
            this.swtStartOnBoot.Name = "swtStartOnBoot";
            this.swtStartOnBoot.Ripple = true;
            this.swtStartOnBoot.Size = new System.Drawing.Size(87, 37);
            this.swtStartOnBoot.TabIndex = 2;
            this.swtStartOnBoot.Text = "Não";
            this.swtStartOnBoot.UseVisualStyleBackColor = true;
            this.swtStartOnBoot.CheckedChanged += new System.EventHandler(this.swtStartOnBoot_CheckedChanged);
            // 
            // txtLog
            // 
            this.txtLog.AutoSize = true;
            this.txtLog.Depth = 0;
            this.txtLog.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.txtLog.Location = new System.Drawing.Point(3, 112);
            this.txtLog.MouseState = MaterialSkin.MouseState.HOVER;
            this.txtLog.Name = "txtLog";
            this.txtLog.Size = new System.Drawing.Size(204, 19);
            this.txtLog.TabIndex = 5;
            this.txtLog.Text = "Logs gerados no JJManager";
            // 
            // dgvLog
            // 
            this.dgvLog.AllowUserToAddRows = false;
            this.dgvLog.AllowUserToDeleteRows = false;
            this.dgvLog.AllowUserToResizeColumns = false;
            this.dgvLog.AllowUserToResizeRows = false;
            this.dgvLog.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLog.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colModule,
            this.dgvLogSize,
            this.dgvLogOpen,
            this.dgvLogRemove});
            this.dgvLog.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgvLog.EnableHeadersVisualStyles = false;
            this.dgvLog.Location = new System.Drawing.Point(3, 134);
            this.dgvLog.MultiSelect = false;
            this.dgvLog.Name = "dgvLog";
            this.dgvLog.ReadOnly = true;
            this.dgvLog.RowHeadersVisible = false;
            this.dgvLog.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvLog.Size = new System.Drawing.Size(692, 183);
            this.dgvLog.TabIndex = 4;
            // 
            // colModule
            // 
            this.colModule.FillWeight = 525F;
            this.colModule.HeaderText = "Módulo";
            this.colModule.Name = "colModule";
            this.colModule.ReadOnly = true;
            this.colModule.Width = 525;
            // 
            // dgvLogSize
            // 
            this.dgvLogSize.HeaderText = "Tamanho";
            this.dgvLogSize.Name = "dgvLogSize";
            this.dgvLogSize.ReadOnly = true;
            // 
            // dgvLogOpen
            // 
            this.dgvLogOpen.FillWeight = 32F;
            this.dgvLogOpen.HeaderText = "";
            this.dgvLogOpen.Name = "dgvLogOpen";
            this.dgvLogOpen.ReadOnly = true;
            this.dgvLogOpen.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvLogOpen.Width = 32;
            // 
            // dgvLogRemove
            // 
            this.dgvLogRemove.FillWeight = 32F;
            this.dgvLogRemove.HeaderText = "";
            this.dgvLogRemove.Name = "dgvLogRemove";
            this.dgvLogRemove.ReadOnly = true;
            this.dgvLogRemove.Width = 32;
            // 
            // btnRemoveAllLogs
            // 
            this.btnRemoveAllLogs.AutoSize = false;
            this.btnRemoveAllLogs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnRemoveAllLogs.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnRemoveAllLogs.Depth = 0;
            this.btnRemoveAllLogs.HighEmphasis = true;
            this.btnRemoveAllLogs.Icon = null;
            this.btnRemoveAllLogs.Location = new System.Drawing.Point(4, 326);
            this.btnRemoveAllLogs.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnRemoveAllLogs.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnRemoveAllLogs.Name = "btnRemoveAllLogs";
            this.btnRemoveAllLogs.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnRemoveAllLogs.Size = new System.Drawing.Size(691, 36);
            this.btnRemoveAllLogs.TabIndex = 6;
            this.btnRemoveAllLogs.Text = "Limpar  Logs";
            this.btnRemoveAllLogs.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnRemoveAllLogs.UseAccentColor = false;
            this.btnRemoveAllLogs.UseVisualStyleBackColor = true;
            this.btnRemoveAllLogs.Click += new System.EventHandler(this.btnRemoveAllLogs_Click);
            // 
            // tabUpdate
            // 
            this.tabUpdate.Controls.Add(this.statusStrip1);
            this.tabUpdate.Controls.Add(this.lvDevicesToUpdate);
            this.tabUpdate.Controls.Add(this.BtnUpdateDevice);
            this.tabUpdate.ImageKey = "JJManager_icones_update_16.png";
            this.tabUpdate.Location = new System.Drawing.Point(4, 23);
            this.tabUpdate.Name = "tabUpdate";
            this.tabUpdate.Size = new System.Drawing.Size(704, 373);
            this.tabUpdate.TabIndex = 2;
            this.tabUpdate.Text = "Atualizações";
            this.tabUpdate.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.txtStatusUpdate,
            this.progressBarDownload});
            this.statusStrip1.Location = new System.Drawing.Point(0, 351);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(704, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // txtStatusUpdate
            // 
            this.txtStatusUpdate.AutoSize = false;
            this.txtStatusUpdate.Name = "txtStatusUpdate";
            this.txtStatusUpdate.Size = new System.Drawing.Size(689, 17);
            this.txtStatusUpdate.Spring = true;
            this.txtStatusUpdate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressBarDownload
            // 
            this.progressBarDownload.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.progressBarDownload.AutoSize = false;
            this.progressBarDownload.Name = "progressBarDownload";
            this.progressBarDownload.Size = new System.Drawing.Size(100, 16);
            this.progressBarDownload.Visible = false;
            // 
            // lvDevicesToUpdate
            // 
            this.lvDevicesToUpdate.Alignment = System.Windows.Forms.ListViewAlignment.SnapToGrid;
            this.lvDevicesToUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvDevicesToUpdate.AutoArrange = false;
            this.lvDevicesToUpdate.AutoSizeTable = false;
            this.lvDevicesToUpdate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.lvDevicesToUpdate.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvDevicesToUpdate.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.DeviceId,
            this.DeviceName,
            this.DeviceActualVersion,
            this.DeviceLastVersion});
            this.lvDevicesToUpdate.Depth = 0;
            this.lvDevicesToUpdate.FullRowSelect = true;
            this.lvDevicesToUpdate.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvDevicesToUpdate.HideSelection = false;
            this.lvDevicesToUpdate.Location = new System.Drawing.Point(6, 6);
            this.lvDevicesToUpdate.MinimumSize = new System.Drawing.Size(200, 100);
            this.lvDevicesToUpdate.MouseLocation = new System.Drawing.Point(-1, -1);
            this.lvDevicesToUpdate.MouseState = MaterialSkin.MouseState.OUT;
            this.lvDevicesToUpdate.MultiSelect = false;
            this.lvDevicesToUpdate.Name = "lvDevicesToUpdate";
            this.lvDevicesToUpdate.OwnerDraw = true;
            this.lvDevicesToUpdate.Size = new System.Drawing.Size(692, 289);
            this.lvDevicesToUpdate.TabIndex = 4;
            this.lvDevicesToUpdate.UseCompatibleStateImageBehavior = false;
            this.lvDevicesToUpdate.View = System.Windows.Forms.View.Details;
            this.lvDevicesToUpdate.SelectedIndexChanged += new System.EventHandler(this.lvDevicesToUpdate_SelectedIndexChanged);
            // 
            // DeviceId
            // 
            this.DeviceId.Text = "ID";
            this.DeviceId.Width = 100;
            // 
            // DeviceName
            // 
            this.DeviceName.Text = "Dispositivo";
            this.DeviceName.Width = 330;
            // 
            // DeviceActualVersion
            // 
            this.DeviceActualVersion.Text = "Versão Atual";
            this.DeviceActualVersion.Width = 120;
            // 
            // DeviceLastVersion
            // 
            this.DeviceLastVersion.Text = "Última Versão";
            this.DeviceLastVersion.Width = 150;
            // 
            // BtnUpdateDevice
            // 
            this.BtnUpdateDevice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnUpdateDevice.AutoSize = false;
            this.BtnUpdateDevice.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnUpdateDevice.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnUpdateDevice.Depth = 0;
            this.BtnUpdateDevice.Enabled = false;
            this.BtnUpdateDevice.HighEmphasis = true;
            this.BtnUpdateDevice.Icon = null;
            this.BtnUpdateDevice.Location = new System.Drawing.Point(9, 309);
            this.BtnUpdateDevice.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnUpdateDevice.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnUpdateDevice.Name = "BtnUpdateDevice";
            this.BtnUpdateDevice.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnUpdateDevice.Size = new System.Drawing.Size(689, 36);
            this.BtnUpdateDevice.TabIndex = 3;
            this.BtnUpdateDevice.Text = "Atualizar";
            this.BtnUpdateDevice.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnUpdateDevice.UseAccentColor = false;
            this.BtnUpdateDevice.UseVisualStyleBackColor = true;
            this.BtnUpdateDevice.Click += new System.EventHandler(this.BtnUpdateDevice_Click);
            // 
            // tabAbout
            // 
            this.tabAbout.Controls.Add(this.lblAboutText);
            this.tabAbout.Controls.Add(this.lblAboutVersion);
            this.tabAbout.Controls.Add(this.ImgAbout);
            this.tabAbout.Location = new System.Drawing.Point(4, 23);
            this.tabAbout.Name = "tabAbout";
            this.tabAbout.Size = new System.Drawing.Size(704, 373);
            this.tabAbout.TabIndex = 3;
            this.tabAbout.Text = "Sobre";
            this.tabAbout.UseVisualStyleBackColor = true;
            // 
            // lblAboutText
            // 
            this.lblAboutText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAboutText.Depth = 0;
            this.lblAboutText.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblAboutText.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lblAboutText.Location = new System.Drawing.Point(3, 150);
            this.lblAboutText.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblAboutText.Name = "lblAboutText";
            this.lblAboutText.Size = new System.Drawing.Size(698, 126);
            this.lblAboutText.TabIndex = 3;
            this.lblAboutText.Text = resources.GetString("lblAboutText.Text");
            this.lblAboutText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAboutVersion
            // 
            this.lblAboutVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAboutVersion.Depth = 0;
            this.lblAboutVersion.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblAboutVersion.Location = new System.Drawing.Point(3, 114);
            this.lblAboutVersion.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblAboutVersion.Name = "lblAboutVersion";
            this.lblAboutVersion.Size = new System.Drawing.Size(698, 26);
            this.lblAboutVersion.TabIndex = 2;
            this.lblAboutVersion.Text = "JJManager Versão 1.1.13";
            this.lblAboutVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ImgAbout
            // 
            this.ImgAbout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ImgAbout.BackgroundImage = global::JJManager.Properties.Resources.Logo_JohnJohn_JJMixer;
            this.ImgAbout.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ImgAbout.Location = new System.Drawing.Point(3, 8);
            this.ImgAbout.Name = "ImgAbout";
            this.ImgAbout.Size = new System.Drawing.Size(699, 103);
            this.ImgAbout.TabIndex = 0;
            this.ImgAbout.TabStop = false;
            // 
            // tbsMainMenu
            // 
            this.tbsMainMenu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbsMainMenu.BaseTabControl = this.tabMain;
            this.tbsMainMenu.CharacterCasing = MaterialSkin.Controls.MaterialTabSelector.CustomCharacterCasing.Normal;
            this.tbsMainMenu.Depth = 0;
            this.tbsMainMenu.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.tbsMainMenu.Location = new System.Drawing.Point(0, 64);
            this.tbsMainMenu.MouseState = MaterialSkin.MouseState.HOVER;
            this.tbsMainMenu.Name = "tbsMainMenu";
            this.tbsMainMenu.Size = new System.Drawing.Size(715, 48);
            this.tbsMainMenu.TabIndex = 22;
            this.tbsMainMenu.Text = "materialTabSelector1";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(715, 521);
            this.Controls.Add(this.tbsMainMenu);
            this.Controls.Add(this.tabMain);
            this.MaximizeBox = false;
            this.Icon = global::JJManager.Properties.Resources.JJManagerIcon_256;
            this.Name = "Main";
            this.Sizable = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "JJManager";
            this.tabMain.ResumeLayout(false);
            this.tabConnect.ResumeLayout(false);
            this.tabConnect.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DgvDevices)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.tabOptions.ResumeLayout(false);
            this.flpOptions.ResumeLayout(false);
            this.flpOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLog)).EndInit();
            this.tabUpdate.ResumeLayout(false);
            this.tabUpdate.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabAbout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ImgAbout)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ImageList imageList;
        private MaterialSkin.Controls.MaterialTabControl tabMain;
        private System.Windows.Forms.TabPage tabConnect;
        private System.Windows.Forms.TabPage tabOptions;
        private MaterialSkin.Controls.MaterialLabel LblThemeColor;
        private MaterialSkin.Controls.MaterialSwitch SwtThemeColor;
        private System.Windows.Forms.TabPage tabUpdate;
        private MaterialSkin.Controls.MaterialButton BtnUpdateDevice;
        private System.Windows.Forms.TabPage tabAbout;
        private MaterialSkin.Controls.MaterialLabel lblAboutText;
        private MaterialSkin.Controls.MaterialLabel lblAboutVersion;
        private System.Windows.Forms.PictureBox ImgAbout;
        private MaterialSkin.Controls.MaterialTabSelector tbsMainMenu;
        private MaterialSkin.Controls.MaterialButton btnSearchBluetooth;
        private MaterialSkin.Controls.MaterialButton btnEditDevice;
        private MaterialSkin.Controls.MaterialButton btnConnChanger;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel txtStatus;
        private MaterialSkin.Controls.MaterialListView lvDevicesToUpdate;
        public System.Windows.Forms.ColumnHeader DeviceId;
        private System.Windows.Forms.ColumnHeader DeviceName;
        public System.Windows.Forms.ColumnHeader DeviceActualVersion;
        private System.Windows.Forms.ColumnHeader DeviceLastVersion;
        private System.Windows.Forms.StatusStrip statusStrip1;
        public System.Windows.Forms.ToolStripStatusLabel txtStatusUpdate;
        public System.Windows.Forms.ToolStripProgressBar progressBarDownload;
        private System.Windows.Forms.FlowLayoutPanel flpOptions;
        private MaterialSkin.Controls.MaterialLabel txtStartOnBoot;
        private MaterialSkin.Controls.MaterialSwitch swtStartOnBoot;
        private System.Windows.Forms.DataGridView dgvLog;
        private System.Windows.Forms.DataGridViewTextBoxColumn colModule;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvLogSize;
        private System.Windows.Forms.DataGridViewButtonColumn dgvLogOpen;
        private System.Windows.Forms.DataGridViewButtonColumn dgvLogRemove;
        private MaterialSkin.Controls.MaterialLabel txtLog;
        private MaterialSkin.Controls.MaterialButton btnRemoveAllLogs;
        private System.Windows.Forms.DataGridView DgvDevices;
    }
}

