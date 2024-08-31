namespace JJManager.Pages.App
{
    partial class MacroKeyMain
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
            this.ActionType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ActionDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnCancel = new MaterialSkin.Controls.MaterialButton();
            this.btnSaveAndClose = new MaterialSkin.Controls.MaterialButton();
            this.txtInputName = new MaterialSkin.Controls.MaterialTextBox();
            this.btnAddAction = new MaterialSkin.Controls.MaterialButton();
            this.btnSave = new MaterialSkin.Controls.MaterialButton();
            this.dgvActions = new System.Windows.Forms.DataGridView();
            this.dgvActionType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvActionDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvActionEdit = new System.Windows.Forms.DataGridViewButtonColumn();
            this.dgvActionRemove = new System.Windows.Forms.DataGridViewButtonColumn();
            this.dgvActionMoveUp = new System.Windows.Forms.DataGridViewButtonColumn();
            this.dgvActionMoveDown = new System.Windows.Forms.DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvActions)).BeginInit();
            this.SuspendLayout();
            // 
            // ActionType
            // 
            this.ActionType.Text = "Tipo de Ação";
            this.ActionType.Width = 150;
            // 
            // ActionDescription
            // 
            this.ActionDescription.Text = "Descrição";
            this.ActionDescription.Width = 637;
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
            this.btnCancel.Location = new System.Drawing.Point(437, 405);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnCancel.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnCancel.Size = new System.Drawing.Size(96, 36);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancelar";
            this.btnCancel.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnCancel.UseAccentColor = false;
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
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
            this.btnSaveAndClose.Location = new System.Drawing.Point(541, 405);
            this.btnSaveAndClose.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSaveAndClose.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSaveAndClose.Name = "btnSaveAndClose";
            this.btnSaveAndClose.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSaveAndClose.Size = new System.Drawing.Size(148, 36);
            this.btnSaveAndClose.TabIndex = 4;
            this.btnSaveAndClose.Text = "Salvar e Fechar";
            this.btnSaveAndClose.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSaveAndClose.UseAccentColor = false;
            this.btnSaveAndClose.UseVisualStyleBackColor = true;
            this.btnSaveAndClose.Click += new System.EventHandler(this.btnSaveAndClose_Click);
            // 
            // txtInputName
            // 
            this.txtInputName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInputName.AnimateReadOnly = false;
            this.txtInputName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtInputName.Depth = 0;
            this.txtInputName.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.txtInputName.Hint = "Nome do Input";
            this.txtInputName.LeadingIcon = null;
            this.txtInputName.Location = new System.Drawing.Point(7, 68);
            this.txtInputName.MaxLength = 50;
            this.txtInputName.MouseState = MaterialSkin.MouseState.OUT;
            this.txtInputName.Multiline = false;
            this.txtInputName.Name = "txtInputName";
            this.txtInputName.Size = new System.Drawing.Size(787, 50);
            this.txtInputName.TabIndex = 6;
            this.txtInputName.Text = "";
            this.txtInputName.TrailingIcon = null;
            // 
            // btnAddAction
            // 
            this.btnAddAction.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddAction.AutoSize = false;
            this.btnAddAction.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAddAction.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnAddAction.Depth = 0;
            this.btnAddAction.HighEmphasis = true;
            this.btnAddAction.Icon = null;
            this.btnAddAction.Location = new System.Drawing.Point(7, 405);
            this.btnAddAction.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnAddAction.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnAddAction.Name = "btnAddAction";
            this.btnAddAction.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnAddAction.Size = new System.Drawing.Size(188, 36);
            this.btnAddAction.TabIndex = 7;
            this.btnAddAction.Text = "Adicionar Ação";
            this.btnAddAction.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnAddAction.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnAddAction.UseAccentColor = false;
            this.btnAddAction.UseVisualStyleBackColor = true;
            this.btnAddAction.Click += new System.EventHandler(this.btnAddAction_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.AutoSize = false;
            this.btnSave.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSave.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSave.Depth = 0;
            this.btnSave.HighEmphasis = true;
            this.btnSave.Icon = null;
            this.btnSave.Location = new System.Drawing.Point(697, 405);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSave.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSave.Name = "btnSave";
            this.btnSave.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSave.Size = new System.Drawing.Size(96, 36);
            this.btnSave.TabIndex = 8;
            this.btnSave.Text = "Salvar";
            this.btnSave.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSave.UseAccentColor = false;
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // dgvActions
            // 
            this.dgvActions.AllowUserToAddRows = false;
            this.dgvActions.AllowUserToDeleteRows = false;
            this.dgvActions.AllowUserToResizeColumns = false;
            this.dgvActions.AllowUserToResizeRows = false;
            this.dgvActions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvActions.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvActions.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvActions.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvActions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvActions.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgvActionType,
            this.dgvActionDescription,
            this.dgvActionEdit,
            this.dgvActionRemove,
            this.dgvActionMoveUp,
            this.dgvActionMoveDown});
            this.dgvActions.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgvActions.EnableHeadersVisualStyles = false;
            this.dgvActions.Location = new System.Drawing.Point(6, 124);
            this.dgvActions.MultiSelect = false;
            this.dgvActions.Name = "dgvActions";
            this.dgvActions.ReadOnly = true;
            this.dgvActions.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvActions.RowHeadersVisible = false;
            this.dgvActions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvActions.Size = new System.Drawing.Size(786, 272);
            this.dgvActions.TabIndex = 12;
            // 
            // dgvActionType
            // 
            this.dgvActionType.HeaderText = "Ação";
            this.dgvActionType.Name = "dgvActionType";
            this.dgvActionType.ReadOnly = true;
            this.dgvActionType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // dgvActionDescription
            // 
            this.dgvActionDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dgvActionDescription.FillWeight = 546F;
            this.dgvActionDescription.HeaderText = "Descrição da Ação";
            this.dgvActionDescription.Name = "dgvActionDescription";
            this.dgvActionDescription.ReadOnly = true;
            this.dgvActionDescription.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // dgvActionEdit
            // 
            this.dgvActionEdit.FillWeight = 35F;
            this.dgvActionEdit.HeaderText = "";
            this.dgvActionEdit.Name = "dgvActionEdit";
            this.dgvActionEdit.ReadOnly = true;
            this.dgvActionEdit.Width = 35;
            // 
            // dgvActionRemove
            // 
            this.dgvActionRemove.FillWeight = 35F;
            this.dgvActionRemove.HeaderText = "";
            this.dgvActionRemove.Name = "dgvActionRemove";
            this.dgvActionRemove.ReadOnly = true;
            this.dgvActionRemove.Width = 35;
            // 
            // dgvActionMoveUp
            // 
            this.dgvActionMoveUp.FillWeight = 35F;
            this.dgvActionMoveUp.HeaderText = "";
            this.dgvActionMoveUp.Name = "dgvActionMoveUp";
            this.dgvActionMoveUp.ReadOnly = true;
            this.dgvActionMoveUp.Width = 35;
            // 
            // dgvActionMoveDown
            // 
            this.dgvActionMoveDown.FillWeight = 35F;
            this.dgvActionMoveDown.HeaderText = "";
            this.dgvActionMoveDown.Name = "dgvActionMoveDown";
            this.dgvActionMoveDown.ReadOnly = true;
            this.dgvActionMoveDown.Width = 35;
            // 
            // MacroKeyMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dgvActions);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnAddAction);
            this.Controls.Add(this.txtInputName);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSaveAndClose);
            this.Name = "MacroKeyMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "KeyBind";
            ((System.ComponentModel.ISupportInitialize)(this.dgvActions)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ColumnHeader ActionType;
        private System.Windows.Forms.ColumnHeader ActionDescription;
        private MaterialSkin.Controls.MaterialButton btnCancel;
        private MaterialSkin.Controls.MaterialButton btnSaveAndClose;
        private MaterialSkin.Controls.MaterialTextBox txtInputName;
        private MaterialSkin.Controls.MaterialButton btnAddAction;
        private MaterialSkin.Controls.MaterialButton btnSave;
        private System.Windows.Forms.DataGridView dgvActions;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvActionType;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvActionDescription;
        private System.Windows.Forms.DataGridViewButtonColumn dgvActionEdit;
        private System.Windows.Forms.DataGridViewButtonColumn dgvActionRemove;
        private System.Windows.Forms.DataGridViewButtonColumn dgvActionMoveUp;
        private System.Windows.Forms.DataGridViewButtonColumn dgvActionMoveDown;
    }
}