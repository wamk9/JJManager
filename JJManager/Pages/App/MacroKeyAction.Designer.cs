namespace JJManager.Pages.App
{
    partial class MacroKeyAction
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
            this.tbcAction = new MaterialSkin.Controls.MaterialTabControl();
            this.tabMouse = new System.Windows.Forms.TabPage();
            this.pnlMouse = new System.Windows.Forms.Panel();
            this.cmbMouseClick = new MaterialSkin.Controls.MaterialComboBox();
            this.txbMouseY = new MaterialSkin.Controls.MaterialTextBox();
            this.txbMouseX = new MaterialSkin.Controls.MaterialTextBox();
            this.tglMouse = new MaterialSkin.Controls.MaterialSwitch();
            this.tabKeyboardKey = new System.Windows.Forms.TabPage();
            this.pnlKeyboardKey = new System.Windows.Forms.Panel();
            this.cmbKeyboardKeyAction = new MaterialSkin.Controls.MaterialComboBox();
            this.txtActualKeyboadKey = new MaterialSkin.Controls.MaterialLabel();
            this.btnKeyboardKeyRec = new MaterialSkin.Controls.MaterialButton();
            this.tglKeyboardKey = new MaterialSkin.Controls.MaterialSwitch();
            this.tabKeyboardText = new System.Windows.Forms.TabPage();
            this.pnlKeyboardText = new System.Windows.Forms.Panel();
            this.txbKeyboardText = new MaterialSkin.Controls.MaterialTextBox();
            this.tglKeyboardText = new MaterialSkin.Controls.MaterialSwitch();
            this.TabWaitTime = new System.Windows.Forms.TabPage();
            this.pnlWaitTime = new System.Windows.Forms.Panel();
            this.txbWaitTime = new MaterialSkin.Controls.MaterialTextBox();
            this.tglWaitTime = new MaterialSkin.Controls.MaterialSwitch();
            this.btnSaveAndClose = new MaterialSkin.Controls.MaterialButton();
            this.btnCancel = new MaterialSkin.Controls.MaterialButton();
            this.tbsAction = new MaterialSkin.Controls.MaterialTabSelector();
            this.tbcAction.SuspendLayout();
            this.tabMouse.SuspendLayout();
            this.pnlMouse.SuspendLayout();
            this.tabKeyboardKey.SuspendLayout();
            this.pnlKeyboardKey.SuspendLayout();
            this.tabKeyboardText.SuspendLayout();
            this.pnlKeyboardText.SuspendLayout();
            this.TabWaitTime.SuspendLayout();
            this.pnlWaitTime.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbcAction
            // 
            this.tbcAction.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbcAction.Controls.Add(this.tabMouse);
            this.tbcAction.Controls.Add(this.tabKeyboardKey);
            this.tbcAction.Controls.Add(this.tabKeyboardText);
            this.tbcAction.Controls.Add(this.TabWaitTime);
            this.tbcAction.Depth = 0;
            this.tbcAction.Location = new System.Drawing.Point(7, 118);
            this.tbcAction.MouseState = MaterialSkin.MouseState.HOVER;
            this.tbcAction.Multiline = true;
            this.tbcAction.Name = "tbcAction";
            this.tbcAction.SelectedIndex = 0;
            this.tbcAction.Size = new System.Drawing.Size(687, 322);
            this.tbcAction.TabIndex = 0;
            // 
            // tabMouse
            // 
            this.tabMouse.Controls.Add(this.pnlMouse);
            this.tabMouse.Location = new System.Drawing.Point(4, 22);
            this.tabMouse.Name = "tabMouse";
            this.tabMouse.Padding = new System.Windows.Forms.Padding(3);
            this.tabMouse.Size = new System.Drawing.Size(679, 296);
            this.tabMouse.TabIndex = 0;
            this.tabMouse.Text = "Mouse";
            this.tabMouse.UseVisualStyleBackColor = true;
            // 
            // pnlMouse
            // 
            this.pnlMouse.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlMouse.Controls.Add(this.cmbMouseClick);
            this.pnlMouse.Controls.Add(this.txbMouseY);
            this.pnlMouse.Controls.Add(this.txbMouseX);
            this.pnlMouse.Controls.Add(this.tglMouse);
            this.pnlMouse.Location = new System.Drawing.Point(3, 3);
            this.pnlMouse.Name = "pnlMouse";
            this.pnlMouse.Size = new System.Drawing.Size(673, 290);
            this.pnlMouse.TabIndex = 1;
            // 
            // cmbMouseClick
            // 
            this.cmbMouseClick.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbMouseClick.AutoResize = false;
            this.cmbMouseClick.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.cmbMouseClick.Depth = 0;
            this.cmbMouseClick.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cmbMouseClick.DropDownHeight = 174;
            this.cmbMouseClick.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMouseClick.DropDownWidth = 121;
            this.cmbMouseClick.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.cmbMouseClick.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.cmbMouseClick.FormattingEnabled = true;
            this.cmbMouseClick.Hint = "Opção de Clique";
            this.cmbMouseClick.IntegralHeight = false;
            this.cmbMouseClick.ItemHeight = 43;
            this.cmbMouseClick.Items.AddRange(new object[] {
            "Não Clicar",
            "Clique Esquerdo",
            "Clique Meio",
            "Clique Direito"});
            this.cmbMouseClick.Location = new System.Drawing.Point(0, 155);
            this.cmbMouseClick.MaxDropDownItems = 4;
            this.cmbMouseClick.MouseState = MaterialSkin.MouseState.OUT;
            this.cmbMouseClick.Name = "cmbMouseClick";
            this.cmbMouseClick.Size = new System.Drawing.Size(670, 49);
            this.cmbMouseClick.StartIndex = 0;
            this.cmbMouseClick.TabIndex = 4;
            // 
            // txbMouseY
            // 
            this.txbMouseY.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbMouseY.AnimateReadOnly = false;
            this.txbMouseY.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txbMouseY.Depth = 0;
            this.txbMouseY.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.txbMouseY.Hint = "Movimentação Vertical (Em Pixels)";
            this.txbMouseY.LeadingIcon = null;
            this.txbMouseY.Location = new System.Drawing.Point(1, 98);
            this.txbMouseY.MaxLength = 50;
            this.txbMouseY.MouseState = MaterialSkin.MouseState.OUT;
            this.txbMouseY.Multiline = false;
            this.txbMouseY.Name = "txbMouseY";
            this.txbMouseY.Size = new System.Drawing.Size(670, 50);
            this.txbMouseY.TabIndex = 3;
            this.txbMouseY.Text = "";
            this.txbMouseY.TrailingIcon = null;
            // 
            // txbMouseX
            // 
            this.txbMouseX.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbMouseX.AnimateReadOnly = false;
            this.txbMouseX.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txbMouseX.Depth = 0;
            this.txbMouseX.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.txbMouseX.Hint = "Movimentação Horizontal (Em Pixels)";
            this.txbMouseX.LeadingIcon = null;
            this.txbMouseX.Location = new System.Drawing.Point(0, 41);
            this.txbMouseX.MaxLength = 50;
            this.txbMouseX.MouseState = MaterialSkin.MouseState.OUT;
            this.txbMouseX.Multiline = false;
            this.txbMouseX.Name = "txbMouseX";
            this.txbMouseX.Size = new System.Drawing.Size(670, 50);
            this.txbMouseX.TabIndex = 2;
            this.txbMouseX.Text = "";
            this.txbMouseX.TrailingIcon = null;
            // 
            // tglMouse
            // 
            this.tglMouse.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tglMouse.AutoSize = true;
            this.tglMouse.Depth = 0;
            this.tglMouse.Location = new System.Drawing.Point(0, 0);
            this.tglMouse.Margin = new System.Windows.Forms.Padding(0);
            this.tglMouse.MouseLocation = new System.Drawing.Point(-1, -1);
            this.tglMouse.MouseState = MaterialSkin.MouseState.HOVER;
            this.tglMouse.Name = "tglMouse";
            this.tglMouse.Ripple = true;
            this.tglMouse.Size = new System.Drawing.Size(144, 37);
            this.tglMouse.TabIndex = 1;
            this.tglMouse.Text = "Ativar Modo";
            this.tglMouse.UseVisualStyleBackColor = true;
            this.tglMouse.CheckedChanged += new System.EventHandler(this.tglMouse_CheckedChanged);
            // 
            // tabKeyboardKey
            // 
            this.tabKeyboardKey.Controls.Add(this.pnlKeyboardKey);
            this.tabKeyboardKey.Location = new System.Drawing.Point(4, 22);
            this.tabKeyboardKey.Name = "tabKeyboardKey";
            this.tabKeyboardKey.Size = new System.Drawing.Size(679, 296);
            this.tabKeyboardKey.TabIndex = 3;
            this.tabKeyboardKey.Text = "Teclado (Tecla)";
            this.tabKeyboardKey.UseVisualStyleBackColor = true;
            // 
            // pnlKeyboardKey
            // 
            this.pnlKeyboardKey.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlKeyboardKey.Controls.Add(this.cmbKeyboardKeyAction);
            this.pnlKeyboardKey.Controls.Add(this.txtActualKeyboadKey);
            this.pnlKeyboardKey.Controls.Add(this.btnKeyboardKeyRec);
            this.pnlKeyboardKey.Controls.Add(this.tglKeyboardKey);
            this.pnlKeyboardKey.Location = new System.Drawing.Point(3, 3);
            this.pnlKeyboardKey.Name = "pnlKeyboardKey";
            this.pnlKeyboardKey.Size = new System.Drawing.Size(673, 290);
            this.pnlKeyboardKey.TabIndex = 1;
            // 
            // cmbKeyboardKeyAction
            // 
            this.cmbKeyboardKeyAction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbKeyboardKeyAction.AutoResize = false;
            this.cmbKeyboardKeyAction.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.cmbKeyboardKeyAction.Depth = 0;
            this.cmbKeyboardKeyAction.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cmbKeyboardKeyAction.DropDownHeight = 174;
            this.cmbKeyboardKeyAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKeyboardKeyAction.DropDownWidth = 121;
            this.cmbKeyboardKeyAction.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.cmbKeyboardKeyAction.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.cmbKeyboardKeyAction.FormattingEnabled = true;
            this.cmbKeyboardKeyAction.Hint = "Ação da Tecla";
            this.cmbKeyboardKeyAction.IntegralHeight = false;
            this.cmbKeyboardKeyAction.ItemHeight = 43;
            this.cmbKeyboardKeyAction.Items.AddRange(new object[] {
            "Pressionar",
            "Soltar",
            "Pressionar/Soltar"});
            this.cmbKeyboardKeyAction.Location = new System.Drawing.Point(1, 89);
            this.cmbKeyboardKeyAction.MaxDropDownItems = 4;
            this.cmbKeyboardKeyAction.MouseState = MaterialSkin.MouseState.OUT;
            this.cmbKeyboardKeyAction.Name = "cmbKeyboardKeyAction";
            this.cmbKeyboardKeyAction.Size = new System.Drawing.Size(669, 49);
            this.cmbKeyboardKeyAction.StartIndex = 0;
            this.cmbKeyboardKeyAction.TabIndex = 3;
            // 
            // txtActualKeyboadKey
            // 
            this.txtActualKeyboadKey.AutoSize = true;
            this.txtActualKeyboadKey.Depth = 0;
            this.txtActualKeyboadKey.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.txtActualKeyboadKey.Location = new System.Drawing.Point(152, 53);
            this.txtActualKeyboadKey.MouseState = MaterialSkin.MouseState.HOVER;
            this.txtActualKeyboadKey.Name = "txtActualKeyboadKey";
            this.txtActualKeyboadKey.Size = new System.Drawing.Size(229, 19);
            this.txtActualKeyboadKey.TabIndex = 2;
            this.txtActualKeyboadKey.Text = "Nenhuma tecla gravada ainda.,..";
            // 
            // btnKeyboardKeyRec
            // 
            this.btnKeyboardKeyRec.AutoSize = false;
            this.btnKeyboardKeyRec.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnKeyboardKeyRec.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnKeyboardKeyRec.Depth = 0;
            this.btnKeyboardKeyRec.HighEmphasis = true;
            this.btnKeyboardKeyRec.Icon = null;
            this.btnKeyboardKeyRec.Location = new System.Drawing.Point(0, 43);
            this.btnKeyboardKeyRec.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnKeyboardKeyRec.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnKeyboardKeyRec.Name = "btnKeyboardKeyRec";
            this.btnKeyboardKeyRec.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnKeyboardKeyRec.Size = new System.Drawing.Size(144, 36);
            this.btnKeyboardKeyRec.TabIndex = 1;
            this.btnKeyboardKeyRec.Text = "Gravar Tecla";
            this.btnKeyboardKeyRec.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnKeyboardKeyRec.UseAccentColor = false;
            this.btnKeyboardKeyRec.UseVisualStyleBackColor = true;
            this.btnKeyboardKeyRec.Click += new System.EventHandler(this.btnKeyboardKeyRec_Click);
            // 
            // tglKeyboardKey
            // 
            this.tglKeyboardKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tglKeyboardKey.AutoSize = true;
            this.tglKeyboardKey.Depth = 0;
            this.tglKeyboardKey.Location = new System.Drawing.Point(0, 0);
            this.tglKeyboardKey.Margin = new System.Windows.Forms.Padding(0);
            this.tglKeyboardKey.MouseLocation = new System.Drawing.Point(-1, -1);
            this.tglKeyboardKey.MouseState = MaterialSkin.MouseState.HOVER;
            this.tglKeyboardKey.Name = "tglKeyboardKey";
            this.tglKeyboardKey.Ripple = true;
            this.tglKeyboardKey.Size = new System.Drawing.Size(144, 37);
            this.tglKeyboardKey.TabIndex = 0;
            this.tglKeyboardKey.Text = "Ativar Modo";
            this.tglKeyboardKey.UseVisualStyleBackColor = true;
            this.tglKeyboardKey.CheckedChanged += new System.EventHandler(this.tglKeyboardKey_CheckedChanged);
            // 
            // tabKeyboardText
            // 
            this.tabKeyboardText.Controls.Add(this.pnlKeyboardText);
            this.tabKeyboardText.Location = new System.Drawing.Point(4, 22);
            this.tabKeyboardText.Name = "tabKeyboardText";
            this.tabKeyboardText.Padding = new System.Windows.Forms.Padding(3);
            this.tabKeyboardText.Size = new System.Drawing.Size(679, 296);
            this.tabKeyboardText.TabIndex = 1;
            this.tabKeyboardText.Text = "Teclado (Texto)";
            this.tabKeyboardText.UseVisualStyleBackColor = true;
            // 
            // pnlKeyboardText
            // 
            this.pnlKeyboardText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlKeyboardText.Controls.Add(this.txbKeyboardText);
            this.pnlKeyboardText.Controls.Add(this.tglKeyboardText);
            this.pnlKeyboardText.Location = new System.Drawing.Point(3, 3);
            this.pnlKeyboardText.Name = "pnlKeyboardText";
            this.pnlKeyboardText.Size = new System.Drawing.Size(673, 246);
            this.pnlKeyboardText.TabIndex = 0;
            // 
            // txbKeyboardText
            // 
            this.txbKeyboardText.AnimateReadOnly = false;
            this.txbKeyboardText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txbKeyboardText.Depth = 0;
            this.txbKeyboardText.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.txbKeyboardText.Hint = "Texto para digitar";
            this.txbKeyboardText.LeadingIcon = null;
            this.txbKeyboardText.Location = new System.Drawing.Point(0, 40);
            this.txbKeyboardText.MaxLength = 50;
            this.txbKeyboardText.MouseState = MaterialSkin.MouseState.OUT;
            this.txbKeyboardText.Multiline = false;
            this.txbKeyboardText.Name = "txbKeyboardText";
            this.txbKeyboardText.Size = new System.Drawing.Size(670, 50);
            this.txbKeyboardText.TabIndex = 1;
            this.txbKeyboardText.Text = "";
            this.txbKeyboardText.TrailingIcon = null;
            // 
            // tglKeyboardText
            // 
            this.tglKeyboardText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tglKeyboardText.AutoSize = true;
            this.tglKeyboardText.Depth = 0;
            this.tglKeyboardText.Location = new System.Drawing.Point(0, 0);
            this.tglKeyboardText.Margin = new System.Windows.Forms.Padding(0);
            this.tglKeyboardText.MouseLocation = new System.Drawing.Point(-1, -1);
            this.tglKeyboardText.MouseState = MaterialSkin.MouseState.HOVER;
            this.tglKeyboardText.Name = "tglKeyboardText";
            this.tglKeyboardText.Ripple = true;
            this.tglKeyboardText.Size = new System.Drawing.Size(144, 37);
            this.tglKeyboardText.TabIndex = 0;
            this.tglKeyboardText.Text = "Ativar Modo";
            this.tglKeyboardText.UseVisualStyleBackColor = true;
            this.tglKeyboardText.CheckedChanged += new System.EventHandler(this.tglKeyboardText_CheckedChanged);
            // 
            // TabWaitTime
            // 
            this.TabWaitTime.Controls.Add(this.pnlWaitTime);
            this.TabWaitTime.Location = new System.Drawing.Point(4, 22);
            this.TabWaitTime.Name = "TabWaitTime";
            this.TabWaitTime.Size = new System.Drawing.Size(679, 296);
            this.TabWaitTime.TabIndex = 2;
            this.TabWaitTime.Text = "Tempo de Espera";
            this.TabWaitTime.UseVisualStyleBackColor = true;
            // 
            // pnlWaitTime
            // 
            this.pnlWaitTime.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlWaitTime.Controls.Add(this.txbWaitTime);
            this.pnlWaitTime.Controls.Add(this.tglWaitTime);
            this.pnlWaitTime.Location = new System.Drawing.Point(3, 3);
            this.pnlWaitTime.Name = "pnlWaitTime";
            this.pnlWaitTime.Size = new System.Drawing.Size(673, 246);
            this.pnlWaitTime.TabIndex = 0;
            // 
            // txbWaitTime
            // 
            this.txbWaitTime.AnimateReadOnly = false;
            this.txbWaitTime.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txbWaitTime.Depth = 0;
            this.txbWaitTime.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.txbWaitTime.Hint = "Tempo (em milissegundos)";
            this.txbWaitTime.LeadingIcon = null;
            this.txbWaitTime.Location = new System.Drawing.Point(3, 41);
            this.txbWaitTime.MaxLength = 50;
            this.txbWaitTime.MouseState = MaterialSkin.MouseState.OUT;
            this.txbWaitTime.Multiline = false;
            this.txbWaitTime.Name = "txbWaitTime";
            this.txbWaitTime.Size = new System.Drawing.Size(667, 50);
            this.txbWaitTime.TabIndex = 2;
            this.txbWaitTime.Text = "";
            this.txbWaitTime.TrailingIcon = null;
            // 
            // tglWaitTime
            // 
            this.tglWaitTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tglWaitTime.AutoSize = true;
            this.tglWaitTime.Depth = 0;
            this.tglWaitTime.Location = new System.Drawing.Point(0, 0);
            this.tglWaitTime.Margin = new System.Windows.Forms.Padding(0);
            this.tglWaitTime.MouseLocation = new System.Drawing.Point(-1, -1);
            this.tglWaitTime.MouseState = MaterialSkin.MouseState.HOVER;
            this.tglWaitTime.Name = "tglWaitTime";
            this.tglWaitTime.Ripple = true;
            this.tglWaitTime.Size = new System.Drawing.Size(144, 37);
            this.tglWaitTime.TabIndex = 1;
            this.tglWaitTime.Text = "Ativar Modo";
            this.tglWaitTime.UseVisualStyleBackColor = true;
            this.tglWaitTime.CheckedChanged += new System.EventHandler(this.tglWaitTime_CheckedChanged);
            // 
            // btnSaveAndClose
            // 
            this.btnSaveAndClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveAndClose.AutoSize = false;
            this.btnSaveAndClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSaveAndClose.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSaveAndClose.Depth = 0;
            this.btnSaveAndClose.HighEmphasis = true;
            this.btnSaveAndClose.Icon = null;
            this.btnSaveAndClose.Location = new System.Drawing.Point(441, 449);
            this.btnSaveAndClose.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSaveAndClose.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSaveAndClose.Name = "btnSaveAndClose";
            this.btnSaveAndClose.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSaveAndClose.Size = new System.Drawing.Size(252, 36);
            this.btnSaveAndClose.TabIndex = 11;
            this.btnSaveAndClose.Text = "Salvar na Lista e Fechar";
            this.btnSaveAndClose.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSaveAndClose.UseAccentColor = false;
            this.btnSaveAndClose.UseVisualStyleBackColor = true;
            this.btnSaveAndClose.Click += new System.EventHandler(this.btnSaveAndClose_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.AutoSize = false;
            this.btnCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnCancel.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnCancel.Depth = 0;
            this.btnCancel.HighEmphasis = true;
            this.btnCancel.Icon = null;
            this.btnCancel.Location = new System.Drawing.Point(337, 449);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnCancel.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnCancel.Size = new System.Drawing.Size(96, 36);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Cancelar";
            this.btnCancel.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnCancel.UseAccentColor = false;
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // tbsAction
            // 
            this.tbsAction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbsAction.BaseTabControl = this.tbcAction;
            this.tbsAction.CharacterCasing = MaterialSkin.Controls.MaterialTabSelector.CustomCharacterCasing.Normal;
            this.tbsAction.Depth = 0;
            this.tbsAction.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.tbsAction.Location = new System.Drawing.Point(0, 64);
            this.tbsAction.MouseState = MaterialSkin.MouseState.HOVER;
            this.tbsAction.Name = "tbsAction";
            this.tbsAction.Size = new System.Drawing.Size(700, 48);
            this.tbsAction.TabIndex = 23;
            this.tbsAction.Text = "tbsAction";
            // 
            // MacroKeyAction
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 494);
            this.Controls.Add(this.tbsAction);
            this.Controls.Add(this.btnSaveAndClose);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.tbcAction);
            this.KeyPreview = true;
            this.Icon = global::JJManager.Properties.Resources.JJManagerIcon_256;
            this.Name = "MacroKeyAction";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MacroKeyAction";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MacroKeyAction_KeyDown);
            this.tbcAction.ResumeLayout(false);
            this.tabMouse.ResumeLayout(false);
            this.pnlMouse.ResumeLayout(false);
            this.pnlMouse.PerformLayout();
            this.tabKeyboardKey.ResumeLayout(false);
            this.pnlKeyboardKey.ResumeLayout(false);
            this.pnlKeyboardKey.PerformLayout();
            this.tabKeyboardText.ResumeLayout(false);
            this.pnlKeyboardText.ResumeLayout(false);
            this.pnlKeyboardText.PerformLayout();
            this.TabWaitTime.ResumeLayout(false);
            this.pnlWaitTime.ResumeLayout(false);
            this.pnlWaitTime.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private MaterialSkin.Controls.MaterialTabControl tbcAction;
        private System.Windows.Forms.TabPage tabMouse;
        private System.Windows.Forms.TabPage tabKeyboardText;
        private MaterialSkin.Controls.MaterialButton btnSaveAndClose;
        private MaterialSkin.Controls.MaterialButton btnCancel;
        private MaterialSkin.Controls.MaterialTabSelector tbsAction;
        private System.Windows.Forms.TabPage TabWaitTime;
        private System.Windows.Forms.Panel pnlMouse;
        private System.Windows.Forms.Panel pnlKeyboardText;
        private MaterialSkin.Controls.MaterialSwitch tglKeyboardText;
        private System.Windows.Forms.Panel pnlWaitTime;
        private MaterialSkin.Controls.MaterialTextBox txbKeyboardText;
        private System.Windows.Forms.TabPage tabKeyboardKey;
        private System.Windows.Forms.Panel pnlKeyboardKey;
        private MaterialSkin.Controls.MaterialSwitch tglKeyboardKey;
        private MaterialSkin.Controls.MaterialSwitch tglWaitTime;
        private MaterialSkin.Controls.MaterialSwitch tglMouse;
        private MaterialSkin.Controls.MaterialComboBox cmbMouseClick;
        private MaterialSkin.Controls.MaterialTextBox txbMouseY;
        private MaterialSkin.Controls.MaterialTextBox txbMouseX;
        private MaterialSkin.Controls.MaterialLabel txtActualKeyboadKey;
        private MaterialSkin.Controls.MaterialButton btnKeyboardKeyRec;
        private MaterialSkin.Controls.MaterialComboBox cmbKeyboardKeyAction;
        private MaterialSkin.Controls.MaterialTextBox txbWaitTime;
    }
}