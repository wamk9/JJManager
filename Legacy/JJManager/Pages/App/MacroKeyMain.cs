using JJManager.Class;
using JJManager.Class.App.Fonts;
using JJManager.Class.App.Input;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Threading;
using System.Windows.Forms;
using ConfigClass = JJManager.Class.App.Config.Config;
using ProfileClass = JJManager.Class.App.Profile.Profile;

namespace JJManager.Pages.App
{
    public partial class MacroKeyMain : MaterialForm
    {
        private static ProfileClass _activeProfile;
        //private static AudioManager _audioManager = new AudioManager();
        private static DatabaseConnection _DatabaseConnection = new DatabaseConnection();
        private Thread thrTimers = null;
        private bool _IsInputSelected = false;
        private bool _IsCreateProfileOpened = false;
        private MaterialForm _parent = null;
        private bool _isActionSelected = false;
        private uint _idInput = 0;

        private Point _mousePosition = Point.Empty;

        #region WinForms
        private MaterialSkinManager materialSkinManager = null;
        private AppModulesTimer JoystickReceiver = null;
        private AppModulesNotifyIcon notifyIcon = null;
        #endregion

        public MacroKeyMain(MaterialForm parent, ProfileClass profile, uint idInput)
        {
            InitializeComponent();
            components = new System.ComponentModel.Container();
            _activeProfile = profile;
            _idInput = idInput;
            _parent = parent;
            _activeProfile.Inputs[(int)_idInput].RestartInput();

            Text = "Input " + (idInput + 1);
            txtInputName.Text = _activeProfile.Inputs[(int)idInput].Name;

            //_macroKey = new MacroKey(_activeProfile.Inputs[(int)_idInput].Data);

            // MaterialDesign
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;

            // Double buffer, to not flick buttons on hover
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, dgvActions, new object[] { true });

            dgvActions.EnableHeadersVisualStyles = false;

            dgvActions.BackgroundColor = MaterialSkinManager.Instance.BackgroundColor;
            //dgvActions.ForeColor = MaterialSkinManager.Instance.TextMediumEmphasisColor;
            //dgvActions.GridColor = MaterialSkinManager.Instance.BackgroundColor;
            
            dgvActions.RowsDefaultCellStyle.SelectionBackColor = materialSkinManager.Theme == MaterialSkinManager.Themes.DARK ? MaterialSkinManager.Instance.BackgroundColor.Lighten((float)0.2) : MaterialSkinManager.Instance.BackgroundColor.Darken((float)0.2);
            dgvActions.RowsDefaultCellStyle.SelectionForeColor = MaterialSkinManager.Instance.TextMediumEmphasisColor;

            dgvActions.RowsDefaultCellStyle.BackColor = MaterialSkinManager.Instance.BackgroundColor;
            dgvActions.RowsDefaultCellStyle.ForeColor = MaterialSkinManager.Instance.TextMediumEmphasisColor;

            dgvActions.ColumnHeadersDefaultCellStyle.SelectionBackColor = MaterialSkinManager.Instance.BackgroundColor;
            dgvActions.ColumnHeadersDefaultCellStyle.SelectionForeColor = MaterialSkinManager.Instance.TextMediumEmphasisColor;


            dgvActions.ColumnHeadersDefaultCellStyle.BackColor = MaterialSkinManager.Instance.BackgroundColor;
            dgvActions.ColumnHeadersDefaultCellStyle.ForeColor = MaterialSkinManager.Instance.BackgroundAlternativeColor;

            dgvActions.ColumnHeadersDefaultCellStyle.Font = new Font(dgvActions.ColumnHeadersDefaultCellStyle.Font, FontStyle.Bold);
            dgvActions.ColumnHeadersDefaultCellStyle.Padding = new Padding(0,5,0,5);
            dgvActions.AdvancedColumnHeadersBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.None;
            dgvActions.AdvancedColumnHeadersBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.None;
            dgvActions.AdvancedColumnHeadersBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;
            
            dgvActions.CellClick += DgvActions_CellClick;
            dgvActions.CellMouseEnter += DgvActions_CellMouseEnter;
            dgvActions.CellMouseLeave += DgvActions_CellMouseLeave;
            dgvActions.CellPainting += DgvActions_CellPainting;
            dgvActions.MouseMove += DgvActions_MouseMove;
            LoadFormData();

            if (_activeProfile.Inputs[(int)_idInput].Mode == Input.InputMode.MacroKey)
            {
                _activeProfile.Inputs[(int)_idInput].MacroKey.Actions.CollectionChanged += Actions_CollectionChanged;
            }

            // Events
            FormClosing += new FormClosingEventHandler(MacroKey_FormClosing);
            //FormClosed += new FormClosedEventHandler(JJB01_V2_FormClosed);
        }

        private void DgvActions_MouseMove(object sender, MouseEventArgs e)
        {
            _mousePosition = e.Location;

            var hitTest = dgvActions.HitTest(e.X, e.Y);
            if (hitTest.Type == DataGridViewHitTestType.Cell)
            {
                dgvActions.Invalidate(dgvActions.GetCellDisplayRectangle(hitTest.ColumnIndex, hitTest.RowIndex, true));
            }
        }

        private void CreateStyledButtonOnDataViewGrid(DataGridViewCellPaintingEventArgs e, string iconUnicode, int disableOnIndex = -1)
        {
            e.Paint(e.CellBounds, DataGridViewPaintParts.All);

            // Draw the icon as text in the center of the button
            //e.Graphics.FillRectangle(materialSkinManager.ColorScheme.PrimaryBrush, e.CellBounds); // Change 'Green' to any desired color

            int margin = 1;
            int borderRadius = 4; // Adjust the border radius as needed
            Rectangle cellBounds = e.CellBounds;
            Rectangle buttonBounds = new Rectangle(
                cellBounds.X + margin,
                cellBounds.Y + margin,
                cellBounds.Width - 2 * margin,
                cellBounds.Height - 2 * margin
            );

            // Create a GraphicsPath for rounded rectangle
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(buttonBounds.X, buttonBounds.Y, borderRadius, borderRadius, 180, 90);
                path.AddArc(buttonBounds.X + buttonBounds.Width - borderRadius, buttonBounds.Y, borderRadius, borderRadius, 270, 90);
                path.AddArc(buttonBounds.X + buttonBounds.Width - borderRadius, buttonBounds.Y + buttonBounds.Height - borderRadius, borderRadius, borderRadius, 0, 90);
                path.AddArc(buttonBounds.X, buttonBounds.Y + buttonBounds.Height - borderRadius, borderRadius, borderRadius, 90, 90);
                path.CloseFigure();

                bool isHovered = buttonBounds.Contains(_mousePosition);
                bool isDisabled = disableOnIndex == e.RowIndex;
                // Fill the button background with color
                if (!isDisabled)
                {
                    e.Graphics.FillPath(isHovered ? materialSkinManager.ColorScheme.LightPrimaryBrush : materialSkinManager.ColorScheme.PrimaryBrush, path);
                }
                else
                {
                    e.Graphics.FillPath(new SolidBrush(materialSkinManager.BackgroundColor.Darken((float).2)), path) ;
                }

                TextRenderer.DrawText(
                    e.Graphics,
                    iconUnicode,
                    FontAwesome.UseSolid(8),
                    buttonBounds,
                    (isDisabled ? materialSkinManager.TextDisabledOrHintColor.Darken((float).5) : e.CellStyle.ForeColor),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                );

                e.Graphics.DrawPath(materialSkinManager.ColorScheme.DarkPrimaryPen, path);
            }

            e.Handled = true; // Indicate that the painting is handled
        }


        private void DgvActions_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == dgvActions.Columns["dgvActionEdit"].Index && e.RowIndex >= 0)
            {
                CreateStyledButtonOnDataViewGrid(e, "\uf303");
            }

            if (e.ColumnIndex == dgvActions.Columns["dgvActionRemove"].Index && e.RowIndex >= 0)
            {
                CreateStyledButtonOnDataViewGrid(e, "\uf00d");
            }

            if (e.ColumnIndex == dgvActions.Columns["dgvActionMoveUp"].Index && e.RowIndex >= 0)
            {
                CreateStyledButtonOnDataViewGrid(e, "\uf077", 0);
            }

            if (e.ColumnIndex == dgvActions.Columns["dgvActionMoveDown"].Index && e.RowIndex >= 0)
            {
                CreateStyledButtonOnDataViewGrid(e, "\uf078", (dgvActions.RowCount - 1));
            }
        }

        private void DgvActions_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dgvActions.Rows[e.RowIndex].Selected = false;
            }
        }

        private void DgvActions_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dgvActions.ClearSelection();
                dgvActions.Rows[e.RowIndex].Selected = true;
            }
        }

        private void DgvActions_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dgvActions.Columns["dgvActionEdit"].Index) // Index of your button column
            {
                OpenActionModal(e.RowIndex);
            }

            if (e.ColumnIndex == dgvActions.Columns["dgvActionRemove"].Index) // Index of your button column
            {
                DialogResult result = Pages.App.MessageBox.Show(this, "Confirmação de Exclusão", "Você deseja excluir está ação?", MessageBoxButtons.YesNo);
                
                if (result == DialogResult.Yes)
                {
                    int idToRemove = e.RowIndex;

                    _activeProfile.Inputs[(int)_idInput].MacroKey.Actions.RemoveAt(idToRemove);

                    for (int i = 0; i < _activeProfile.Inputs[(int)_idInput].MacroKey.Actions.Count; i++)
                    {
                        if (idToRemove >= _activeProfile.Inputs[(int)_idInput].MacroKey.Actions[i].Order)
                        {
                            _activeProfile.Inputs[(int)_idInput].MacroKey.Actions[i].Order--;
                        }
                    }

                    dgvActions.Rows.Clear();
                    _activeProfile.Inputs[(int)_idInput].MacroKey.OrderActions();
                }
            }

            if (e.ColumnIndex == dgvActions.Columns["dgvActionMoveUp"].Index) // Index of your button column
            {
                if (e.RowIndex == 0)
                {
                    return;
                }

                int idToMove = e.RowIndex;

                for (int i = 0; i < _activeProfile.Inputs[(int)_idInput].MacroKey.Actions.Count; i++)
                {
                    if (idToMove == _activeProfile.Inputs[(int)_idInput].MacroKey.Actions[i].Order)
                    {
                        _activeProfile.Inputs[(int)_idInput].MacroKey.Actions[i].Order--;
                    }
                    else if ((idToMove - 1) == _activeProfile.Inputs[(int)_idInput].MacroKey.Actions[i].Order)
                    {
                        _activeProfile.Inputs[(int)_idInput].MacroKey.Actions[i].Order++;
                    }
                }

                dgvActions.Rows.Clear();
                _activeProfile.Inputs[(int)_idInput].MacroKey.OrderActions();
                dgvActions.ClearSelection();
                dgvActions.Rows[(idToMove - 1)].Selected = true;
            }

            if (e.ColumnIndex == dgvActions.Columns["dgvActionMoveDown"].Index) // Index of your button column
            {
                if (e.RowIndex == (dgvActions.RowCount - 1))
                {
                    return;
                }

                int idToMove = e.RowIndex;

                for (int i = 0; i < _activeProfile.Inputs[(int)_idInput].MacroKey.Actions.Count; i++)
                {
                    if (idToMove == _activeProfile.Inputs[(int)_idInput].MacroKey.Actions[i].Order)
                    {
                        _activeProfile.Inputs[(int)_idInput].MacroKey.Actions[i].Order++;
                    }
                    else if ((idToMove + 1) == _activeProfile.Inputs[(int)_idInput].MacroKey.Actions[i].Order)
                    {
                        _activeProfile.Inputs[(int)_idInput].MacroKey.Actions[i].Order--;
                    }
                }

                dgvActions.Rows.Clear();
                _activeProfile.Inputs[(int)_idInput].MacroKey.OrderActions();
                dgvActions.Rows[(idToMove + 1)].Selected = true;
            }
        }

        private void Actions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var newItem in e.NewItems)
                {
                    JJManager.Class.App.Input.MacroKey.MacroKeyAction action = newItem as JJManager.Class.App.Input.MacroKey.MacroKeyAction;

                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            dgvActions.Rows.Add(action.TypeList, action.Description, "Editar ação", "Excluir ação", "Mover para cima", "Mover para baixo");
                        });
                    }
                    else
                    {
                        dgvActions.Rows.Add(action.TypeList, action.Description, "Editar ação", "Excluir ação", "Mover para cima", "Mover para baixo");
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var oldItem in e.OldItems)
                {
                    JJManager.Class.App.Input.MacroKey.MacroKeyAction action = oldItem as JJManager.Class.App.Input.MacroKey.MacroKeyAction;

                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            //dgvActions.Rows.RemoveAt((int)action.Order);
                            dgvActions.Rows.Clear();
                            _activeProfile.Inputs[(int)_idInput].MacroKey.OrderActions();
                        });
                    }
                    else
                    {
                        //dgvActions.Rows.RemoveAt((int)action.Order);
                        dgvActions.Rows.Clear();
                        _activeProfile.Inputs[(int)_idInput].MacroKey.OrderActions();

                    }
                }
            }
        }

        private void LoadFormData()
        {
            if (_activeProfile.Inputs[(int)_idInput].Mode == Input.InputMode.MacroKey)
            {
                dgvActions.Rows.Clear();

                foreach (JJManager.Class.App.Input.MacroKey.MacroKeyAction action in _activeProfile.Inputs[(int)_idInput].MacroKey.Actions)
                {
                    dgvActions.Rows.Add(action.TypeList, action.Description, "Editar ação", "Excluir ação", "Mover para cima", "Mover para baixo");
                }
            }
        }

        private void MacroKey_FormClosing(object sender, FormClosingEventArgs e)
        {
            _parent.Visible = true;
        }

        private void btnAddAction_Click(object sender, EventArgs e)
        {
            if (_isActionSelected)
                return;

            _isActionSelected = true;

            Thread thr = new Thread(() => {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        OpenActionModal((dgvActions.SelectedRows.Count > 0 ? dgvActions.SelectedRows[0].Index : -1));
                    });
                }
                else
                {
                    OpenActionModal((dgvActions.SelectedRows.Count > 0 ? dgvActions.SelectedRows[0].Index : -1));
                }
            });
            thr.Name = "JJSD01_Action_" + (dgvActions.SelectedRows.Count > 0 ? dgvActions.SelectedRows[0].Index : -1);
            thr.Start();
        }

        public void OpenActionModal(int idAction)
        {
            if (_activeProfile.Inputs[(int)_idInput].Mode != Input.InputMode.MacroKey)
            {
                _activeProfile.Inputs[(int)_idInput].SetToMacroKey();
                _activeProfile.Inputs[(int)_idInput].MacroKey.Actions.CollectionChanged += Actions_CollectionChanged;
            }

            Pages.App.MacroKeyAction macroKeyActionPage = new Pages.App.MacroKeyAction(this, (idAction > -1 ? _activeProfile.Inputs[(int)_idInput].MacroKey.Actions[idAction] : new Class.App.Input.MacroKey.MacroKeyAction(new JsonObject())));
            Visible = false;

            if (macroKeyActionPage.ShowDialog() == DialogResult.OK)
            {
                _activeProfile.Inputs[(int)_idInput].MacroKey.Update(idAction, macroKeyActionPage.Action);
            }

            _isActionSelected = false;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Save()
        {
            dgvActions.Rows.Clear();

            _activeProfile.Inputs[(int)_idInput].Name = txtInputName.Text;
            _activeProfile.Inputs[(int)_idInput].Data = _activeProfile.Inputs[(int)_idInput].MacroKey.ActionsToJson();

            _activeProfile.UpdateInput(_activeProfile.Inputs[(int)_idInput]);
        }

        private void btnSaveAndClose_Click(object sender, EventArgs e)
        {
            Save();
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Save();
        }
    }
}
