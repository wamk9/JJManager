using JJManager.Class;
using JJManager.Class.App.Controls.DrawImage;
using JJManager.Class.App.Fonts;
using JJManager.Class.App.Output;
using JJManager.Class.App.Output.Leds;
using JJManager.Properties;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Windows.Forms;
using ConfigClass = JJManager.Class.App.Config.Config;
using JJDeviceClass = JJManager.Class.Devices.JJDevice;
using ProfileClass = JJManager.Class.App.Profile.Profile;

namespace JJManager.Pages.Devices
{
    public partial class JJBSlim_A : MaterialForm
    {
        private JJDeviceClass _device = null;
        private bool _isCreateProfileOpened = false;
        private MaterialForm _parent = null;
        private JsonObject _data = null;
        private bool _saveData = false;
        private int _ledSelected = -1;
        private MaterialSkinManager _materialSkinManager = null;
        private bool _isLoadingData = false;
        private bool _lastConnectionState = false;
        private System.Windows.Forms.Timer _connectionMonitorTimer = null;
        private Point _mousePosition = Point.Empty;
        private BindingList<Output> _outputList = new BindingList<Output>();

        public JJBSlim_A(MaterialForm parent, JJDeviceClass device)
        {
            InitializeComponent();
            components = new System.ComponentModel.Container();

            // MaterialDesign
            _materialSkinManager = MaterialSkinManager.Instance;
            _materialSkinManager.AddFormToManage(this);
            _materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            _materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;

            _device = device;

            _parent = parent;


            FillForm();
            SetEvents();

            // Configure DataGridView styling
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, dgvActions, new object[] { true });

            dgvActions.AutoGenerateColumns = false;
            dgvActions.DataSource = _outputList;
            dgvActions.EnableHeadersVisualStyles = false;

            dgvActions.BackgroundColor = MaterialSkinManager.Instance.BackgroundColor;
            //dgvActions.ForeColor = MaterialSkinManager.Instance.TextMediumEmphasisColor;
            //dgvActions.GridColor = MaterialSkinManager.Instance.BackgroundColor;

            dgvActions.RowsDefaultCellStyle.SelectionBackColor = _materialSkinManager.Theme == MaterialSkinManager.Themes.DARK ? MaterialSkinManager.Instance.BackgroundColor.Lighten((float)0.2) : MaterialSkinManager.Instance.BackgroundColor.Darken((float)0.2);
            dgvActions.RowsDefaultCellStyle.SelectionForeColor = MaterialSkinManager.Instance.TextMediumEmphasisColor;

            dgvActions.RowsDefaultCellStyle.BackColor = MaterialSkinManager.Instance.BackgroundColor;
            dgvActions.RowsDefaultCellStyle.ForeColor = MaterialSkinManager.Instance.TextMediumEmphasisColor;

            dgvActions.ColumnHeadersDefaultCellStyle.SelectionBackColor = MaterialSkinManager.Instance.BackgroundColor;
            dgvActions.ColumnHeadersDefaultCellStyle.SelectionForeColor = MaterialSkinManager.Instance.TextMediumEmphasisColor;


            dgvActions.ColumnHeadersDefaultCellStyle.BackColor = MaterialSkinManager.Instance.BackgroundColor;
            dgvActions.ColumnHeadersDefaultCellStyle.ForeColor = MaterialSkinManager.Instance.BackgroundAlternativeColor;

            dgvActions.ColumnHeadersDefaultCellStyle.Font = new Font(dgvActions.ColumnHeadersDefaultCellStyle.Font, FontStyle.Bold);
            dgvActions.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 5, 0, 5);
            dgvActions.AdvancedColumnHeadersBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.None;
            dgvActions.AdvancedColumnHeadersBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.None;
            dgvActions.AdvancedColumnHeadersBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;
            UpdateConnectionStatus();

            // Initialize connection monitor timer
            _lastConnectionState = _device.IsConnected;
            _connectionMonitorTimer = new System.Windows.Forms.Timer();
            _connectionMonitorTimer.Interval = 1000; // Check every 1 second
            _connectionMonitorTimer.Tick += ConnectionMonitorTimer_Tick;
            _connectionMonitorTimer.Start();
        }

        public void SetEvents()
        {
            #region Profiles
            BtnAddProfile.Click += BtnAddProfile_Click;
            BtnRemoveProfile.Click += BtnRemoveProfile_Click;
            CmbBoxSelectProfile.DropDown += CmbBoxSelectProfile_DropDown;
            CmbBoxSelectProfile.SelectedIndexChanged += CmbBoxSelectProfile_SelectedIndexChanged;
            #endregion

            #region GeneralForms
            cmdBoxLedMode.SelectedIndexChanged += CmdBoxLedMode_SelectedIndexChanged;
            SldLedBrightness.onValueChanged += SldLedBrightness_onValueChanged;
            SldPulseSpeed.onValueChanged += SldPulseSpeed_onValueChanged;
            SldBlinkSpeed.onValueChanged += SldBlinkSpeed_onValueChanged;
            BtnConnect.Click += BtnConnect_Click;

            foreach (Control control in PnlJJBSlimA.Controls)
            {
                if (control is DrawImage && control.Name.StartsWith("ImgJJSlimAInput"))
                {
                    ((DrawImage)control).Click += ImgJJSlimAInput_Click;
                    ((DrawImage)control).MouseEnter += ImgJJSlimAInput_MouseEnter; ;
                    ((DrawImage)control).MouseLeave += ImgJJSlimAInput_MouseLeave; ;
                }
            }
            #endregion

            #region DataGridView
            dgvActions.CellClick += DgvActions_CellClick;
            dgvActions.CellMouseEnter += DgvActions_CellMouseEnter;
            dgvActions.CellMouseLeave += DgvActions_CellMouseLeave;
            dgvActions.CellPainting += DgvActions_CellPainting;
            dgvActions.MouseMove += DgvActions_MouseMove;
            BtnAddLedAction.Click += BtnAddLedAction_Click;
            #endregion

            #region Window
            FormClosing += JJBSlim_A_FormClosing;
            Resize += JJBSlim_A_Resize;
            BtnClose.Click += BtnClose_Click;
            //FormClosed += new FormClosedEventHandler(JJBP06_FormClosed);
            #endregion
        }

        public void FillForm()
        {
            #region Profile
            foreach (string Profile in ProfileClass.GetProfilesList(_device.ProductId))
            {
                CmbBoxSelectProfile.Items.Add(Profile);
            }

            if (CmbBoxSelectProfile.Items.Count == 0)
            {
                CmbBoxSelectProfile.Items.Add(new ProfileClass(_device, "Perfil Padrão", true));
                CmbBoxSelectProfile.SelectedIndex = 0;
            }
            else
            {
                CmbBoxSelectProfile.SelectedIndex = CmbBoxSelectProfile.FindStringExact(_device.Profile.Name); ;
            }

            UpdateForm();
            #endregion

            #region General
            // Nothing to do...
            #endregion

            #region Individual Buttons
            cmdBoxLedMode.Enabled = false;
            _ledSelected = -1;
            cmdBoxLedMode.SelectedIndex = -1;
            cmdBoxLedMode.Refresh();
            #endregion
        }

        public void UpdateForm()
        {
            _isLoadingData = true;
            _saveData = false;

            #region Individual Buttons
            if (_ledSelected > -1)
            {
                // Reload LED mode from the new profile's data
                int ledModeValue = 0; // Default to mode 0 (Desligado)

                if (_device.Profile.Data.ContainsKey("led_mode"))
                {
                    JsonArray ledModeArray = _device.Profile.Data["led_mode"].AsArray();
                    if (_ledSelected < ledModeArray.Count)
                    {
                        ledModeValue = ledModeArray[_ledSelected].GetValue<int>();
                    }
                }

                cmdBoxLedMode.SelectedIndex = ledModeValue;
                cmdBoxLedMode.Refresh();

                // Reload DataGridView with the new profile's actions for this LED
                LoadFormData();
            }
            #endregion

            #region General
            if (_device.Profile.Data.ContainsKey("brightness"))
            {
                switch (_device.Profile.Data["brightness"].GetValueKind())
                {
                    case JsonValueKind.String:
                        string brightnessString = _device.Profile.Data["brightness"].GetValue<string>();
                        if (int.TryParse(brightnessString, out int brightnessValue))
                        {
                            SldLedBrightness.Value = brightnessValue;
                        }
                        break;
                    case JsonValueKind.Number:
                        SldLedBrightness.Value = _device.Profile.Data["brightness"].GetValue<int>();
                        break;
                }
            }

            if (_device.Profile.Data.ContainsKey("pulse_speed"))
            {
                switch (_device.Profile.Data["pulse_speed"].GetValueKind())
                {
                    case JsonValueKind.String:
                        string pulseSpeedString = _device.Profile.Data["pulse_speed"].GetValue<string>();
                        if (int.TryParse(pulseSpeedString, out int pulseSpeedValue))
                        {
                            SldPulseSpeed.Value = pulseSpeedValue;
                        }
                        break;
                    case JsonValueKind.Number:
                        SldPulseSpeed.Value = _device.Profile.Data["pulse_speed"].GetValue<int>();
                        break;
                }
            }

            if (_device.Profile.Data.ContainsKey("blink_speed"))
            {
                switch (_device.Profile.Data["blink_speed"].GetValueKind())
                {
                    case JsonValueKind.String:
                        string blinkSpeedString = _device.Profile.Data["blink_speed"].GetValue<string>();
                        if (int.TryParse(blinkSpeedString, out int blinkSpeedValue))
                        {
                            SldBlinkSpeed.Value = blinkSpeedValue;
                        }
                        break;
                    case JsonValueKind.Number:
                        SldBlinkSpeed.Value = _device.Profile.Data["blink_speed"].GetValue<int>();
                        break;
                }
            }
            #endregion

            _isLoadingData = false;
            _saveData = true;
        }

        #region Event Functions (General)
        private void SldLedBrightness_onValueChanged(object sender, int newValue)
        {
            // Save automatically when brightness changes
            SaveGeneralConfig();
        }

        private void SldPulseSpeed_onValueChanged(object sender, int newValue)
        {
            // Save automatically when pulse speed changes
            SaveGeneralConfig();
        }

        private void SldBlinkSpeed_onValueChanged(object sender, int newValue)
        {
            // Save automatically when blink speed changes
            SaveGeneralConfig();
        }
        #endregion

        #region Event Functions (Individual Buttons)

        private void CmdBoxLedMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                FlpJJBSlimA.SuspendLayout();

                int selectedMode = cmdBoxLedMode.SelectedIndex;

                // Control sliders visibility based on selected mode
                // Modo 0 (Desligado): Nenhum slider
                // Modo 1 (Sempre Ligado): Apenas Brilho
                // Modo 2 (Pulsando): Brilho + Pulse Speed
                // Modo 3 (Piscando): Brilho + Blink Speed
                // Modo 4 (SimHub Sync): Todos (Brilho, Pulse, Blink, Actions)

                SldLedBrightness.Visible = (selectedMode >= 1); // Visible from mode 1 onwards
                SldPulseSpeed.Visible = (selectedMode == 2 || selectedMode == 4); // Mode 2 (Pulse) or 4 (SimHub)
                SldBlinkSpeed.Visible = (selectedMode == 3 || selectedMode == 4); // Mode 3 (Blink) or 4 (SimHub)

                // SimHub components (only visible in mode 4)
                TxtProperties.Visible = (selectedMode == 4);
                PropertiesDivider.Visible = (selectedMode == 4);
                dgvActions.Visible = (selectedMode == 4);
                BtnAddLedAction.Visible = (selectedMode == 4);

                // Force correct order after visibility changes
                ReorganizeFlowLayoutPanel();

                Save();
            }
            finally
            {
                FlpJJBSlimA.ResumeLayout(true);
            }
        }

        private void ReorganizeFlowLayoutPanel()
        {
            // Force correct control order by using SetChildIndex
            // This ensures controls appear in the correct visual order
            if (FlpJJBSlimA.Controls.Contains(LblSelectLedInstructions)) FlpJJBSlimA.Controls.SetChildIndex(LblSelectLedInstructions, 0);
            if (FlpJJBSlimA.Controls.Contains(TxtGeneralConfigs)) FlpJJBSlimA.Controls.SetChildIndex(TxtGeneralConfigs, 1);
            if (FlpJJBSlimA.Controls.Contains(materialDivider3)) FlpJJBSlimA.Controls.SetChildIndex(materialDivider3, 2);
            if (FlpJJBSlimA.Controls.Contains(cmdBoxLedMode)) FlpJJBSlimA.Controls.SetChildIndex(cmdBoxLedMode, 3);
            if (FlpJJBSlimA.Controls.Contains(SldLedBrightness)) FlpJJBSlimA.Controls.SetChildIndex(SldLedBrightness, 4);
            if (FlpJJBSlimA.Controls.Contains(SldPulseSpeed)) FlpJJBSlimA.Controls.SetChildIndex(SldPulseSpeed, 5);
            if (FlpJJBSlimA.Controls.Contains(SldBlinkSpeed)) FlpJJBSlimA.Controls.SetChildIndex(SldBlinkSpeed, 6);
            if (FlpJJBSlimA.Controls.Contains(TxtProperties)) FlpJJBSlimA.Controls.SetChildIndex(TxtProperties, 7);
            if (FlpJJBSlimA.Controls.Contains(PropertiesDivider)) FlpJJBSlimA.Controls.SetChildIndex(PropertiesDivider, 8);
            if (FlpJJBSlimA.Controls.Contains(dgvActions)) FlpJJBSlimA.Controls.SetChildIndex(dgvActions, 9);
            if (FlpJJBSlimA.Controls.Contains(BtnAddLedAction)) FlpJJBSlimA.Controls.SetChildIndex(BtnAddLedAction, 10);
        }
        #endregion

        #region Event Functions (Window)
        private void JJBSlim_A_Resize(object sender, EventArgs e)
        {
            PnlJJBSlimA.Size = new Size(this.ClientSize.Width / 2 - 20, this.ClientSize.Height - 122 - 60);
            PnlJJBSlimA.Location = new Point(10, 120);

            FlpJJBSlimA.Size = new Size(this.ClientSize.Width / 2 - 20, this.ClientSize.Height - 122 - 60);
            FlpJJBSlimA.Location = new Point(this.ClientSize.Width / 2 + 10, 120);
        }

        private void JJBSlim_A_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Stop and dispose timer
            if (_connectionMonitorTimer != null)
            {
                _connectionMonitorTimer.Stop();
                _connectionMonitorTimer.Dispose();
                _connectionMonitorTimer = null;
            }
            _parent.Visible = true;
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (_device.IsConnected)
                {
                    // Disconnect
                    if (_device.Disconnect())
                    {
                        UpdateConnectionStatus();
                    }
                }
                else
                {
                    // Connect
                    if (_device.Connect())
                    {
                        UpdateConnectionStatus();
                    }
                }
            }
            catch (Exception ex)
            {
                Pages.App.MessageBox.Show(this, "Erro de Conexão", "Erro ao conectar/desconectar: " + ex.Message);
            }
        }
        #endregion

        #region Event Functions (Profile)
        private void CmbBoxSelectProfile_DropDown(object sender, EventArgs e)
        {
            int selectedIndex = CmbBoxSelectProfile.SelectedIndex;

            CmbBoxSelectProfile.Items.Clear();

            foreach (String Profile in ProfileClass.GetProfilesList(_device.ProductId))
                CmbBoxSelectProfile.Items.Add(Profile);

            CmbBoxSelectProfile.SelectedIndex = selectedIndex;
        }

        private void CmbBoxSelectProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CmbBoxSelectProfile.SelectedIndex != -1)
            {
                _device.Profile = new ProfileClass(_device, CmbBoxSelectProfile.SelectedItem.ToString(), true);
                UpdateForm();
            }

        }
        
        private void BtnAddProfile_Click(object sender, EventArgs e)
        {
            Thread thr = new Thread(() => {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        if (!_isCreateProfileOpened)
                        {
                            _isCreateProfileOpened = true;
                            CreateProfile createProfile = new CreateProfile(_device);
                            DialogResult result = createProfile.ShowDialog();

                            if (result == DialogResult.OK)
                            {
                                FillForm();
                            }

                            _isCreateProfileOpened = false;
                        }
                    });
                }
                else
                {
                    if (!_isCreateProfileOpened)
                    {
                        _isCreateProfileOpened = true;
                        CreateProfile createProfile = new CreateProfile(_device);
                        DialogResult result = createProfile.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            FillForm();
                        }

                        _isCreateProfileOpened = false;
                    }
                }
                Thread.CurrentThread.Abort();
            });
            thr.Name = "Add_Profile";
            thr.Start();
        }

        private void BtnRemoveProfile_Click(object sender, EventArgs e)
        {
            if (CmbBoxSelectProfile.SelectedIndex == -1)
            {
                Pages.App.MessageBox.Show(this, "Selecione um Perfil", "Selecione um perfil para excluí-lo.");
                return;
            }

            if (CmbBoxSelectProfile.Items.Count == 1)
            {
                Pages.App.MessageBox.Show(this, "Não Pode Excluir", "Você possui apenas um perfil e este não pode ser excluído.");
                return;
            }

            DialogResult dialogResult = MessageBox.Show("Você está prestes a excluir o Perfil '" + CmbBoxSelectProfile.SelectedItem.ToString() + "', deseja continuar?", "Exclusão de Perfil", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                string profileNameToExclude = CmbBoxSelectProfile.SelectedItem.ToString();

                CmbBoxSelectProfile.Items.Remove(CmbBoxSelectProfile.SelectedItem);

                string profileNameToActive = CmbBoxSelectProfile.Items[(CmbBoxSelectProfile.Items[0] == CmbBoxSelectProfile.SelectedItem ? 1 : 0)].ToString();
                
                _device.Profile = new ProfileClass(_device, profileNameToActive, true);

                ProfileClass.Delete(profileNameToExclude, _device.ProductId);

                CmbBoxSelectProfile.SelectedIndex = 0;

                Pages.App.MessageBox.Show(this, "Perfil Excluído", "Perfil excluído com sucesso!");

                FillForm();
            }
        }
        #endregion

        #region DataGridView Styling

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
                    e.Graphics.FillPath(isHovered ? _materialSkinManager.ColorScheme.LightPrimaryBrush : _materialSkinManager.ColorScheme.PrimaryBrush, path);
                }
                else
                {
                    e.Graphics.FillPath(new SolidBrush(_materialSkinManager.BackgroundColor.Darken((float).2)), path);
                }

                TextRenderer.DrawText(
                    e.Graphics,
                    iconUnicode,
                    FontAwesome.UseSolid(8),
                    buttonBounds,
                    (isDisabled ? _materialSkinManager.TextDisabledOrHintColor.Darken((float).5) : e.CellStyle.ForeColor),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                );

                e.Graphics.DrawPath(_materialSkinManager.ColorScheme.DarkPrimaryPen, path);
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

        private void DgvActions_MouseMove(object sender, MouseEventArgs e)
        {
            _mousePosition = e.Location;

            var hitTest = dgvActions.HitTest(e.X, e.Y);
            if (hitTest.Type == DataGridViewHitTestType.Cell)
            {
                dgvActions.InvalidateCell(hitTest.ColumnIndex, hitTest.RowIndex);
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
            if (e.RowIndex < 0) return; // Ignore header clicks

            if (e.ColumnIndex == dgvActions.Columns["dgvActionEdit"].Index)
            {
                OpenActionModal(int.Parse(dgvActions.Rows[e.RowIndex].Cells["Id"].Value.ToString()));
            }

            if (e.ColumnIndex == dgvActions.Columns["dgvActionRemove"].Index)
            {
                DialogResult result = Pages.App.MessageBox.Show(this, "Confirmação de Exclusão", "Você deseja excluir está ação?", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    _device.Profile.ExcludeOutput(int.Parse(dgvActions.Rows[e.RowIndex].Cells["Id"].Value.ToString()), Output.OutputMode.Leds);
                    LoadFormData();
                    // Keep the current LED mode selection - don't change it after deleting
                }
            }

            if (e.ColumnIndex == dgvActions.Columns["dgvActionMoveUp"].Index)
            {
                if (e.RowIndex == 0)
                {
                    return;
                }

                _device.Profile.MoveOutput(uint.Parse(dgvActions.Rows[e.RowIndex].Cells["Id"].Value.ToString()), (e.RowIndex - 1), dgvActions.RowCount - 1, Output.OutputMode.Leds);

                LoadFormData();
                dgvActions.Rows[(e.RowIndex - 1)].Selected = true;
            }

            if (e.ColumnIndex == dgvActions.Columns["dgvActionMoveDown"].Index)
            {
                if (e.RowIndex == (dgvActions.RowCount - 1))
                {
                    return;
                }

                _device.Profile.MoveOutput(uint.Parse(dgvActions.Rows[e.RowIndex].Cells["Id"].Value.ToString()), (e.RowIndex + 1), dgvActions.RowCount - 1, Output.OutputMode.Leds);

                LoadFormData();
                dgvActions.Rows[(e.RowIndex + 1)].Selected = true;
            }
        }

        private void OpenActionModal(int outputId)
        {
            Output output = _device.Profile.Outputs.FirstOrDefault(x => x.Id == outputId);
            if (output != null)
            {
                Pages.App.LedMonoAction actionForm = new Pages.App.LedMonoAction(this, output.Led, _ledSelected);
                Visible = false;
                DialogResult result = actionForm.ShowDialog();

                if (result == DialogResult.OK)
                {
                    // Save the edited output to the database
                    output.Led.JsonToLeds(actionForm.LedToSave.objectToJson());
                    output.Changed = true;
                    output.Save();
                }

                LoadFormData();
            }
        }

        private void LoadFormData()
        {
            // Load DataGridView with LED actions (outputs in SimHub mode)
            // Only show outputs for the currently selected LED
            _outputList.Clear();

            if (_ledSelected >= 0)
            {
                foreach (Output item in _device.Profile?.Outputs
                    .Where(o => o.Mode == Output.OutputMode.Leds &&
                               o.Led != null &&
                               o.Led.LedsGrouped != null &&
                               o.Led.LedsGrouped.Contains(_ledSelected))
                    .OrderBy(o => o.Led.Order)
                    .ToList())
                {
                    _outputList.Add(item);
                }
            }

            dgvActions.Refresh();
        }

        private void BtnAddLedAction_Click(object sender, EventArgs e)
        {
            // Only allow adding action if a LED is selected
            if (_ledSelected < 0)
            {
                Pages.App.MessageBox.Show(this, "Nenhum LED Selecionado", "Por favor, selecione um LED antes de adicionar uma ação.");
                return;
            }

            Pages.App.LedMonoAction ledAction = new Pages.App.LedMonoAction(this, null, 0);
            DialogResult result = ledAction.ShowDialog();

            if (result == DialogResult.OK)
            {
                Output output = _device.Profile.Outputs.FirstOrDefault(x => x.Mode == Output.OutputMode.None);

                if (output == null)
                {
                    Pages.App.MessageBox.Show(this, "Problema Na Busca De Dados", "Ocorreu um problema na busca dos dados referentes a está saída, tente novamente realizar o processo.\n\nCaso o erro persista contate-nos.");
                }
                else
                {
                    // Used to get last order position for this LED
                    ledAction.LedToSave.Order = dgvActions.RowCount;

                    // Get JSON from LedToSave and modify to assign to selected LED
                    JsonObject ledData = ledAction.LedToSave.objectToJson();
                    ledData["leds"] = new JsonArray { _ledSelected };

                    int index = _device.Profile.Outputs.IndexOf(output);
                    _device.Profile.Outputs[index].SetToLeds(output.Id, Leds.LedType.PWM);
                    _device.Profile.Outputs[index].Led.JsonToLeds(ledData);
                    _device.Profile.Outputs[index].Changed = true;
                    _device.Profile.Outputs[index].Save();
                    LoadFormData();
                }
            }
        }

        #endregion

        #region Event Functions (Img Buttons)
        private void ImgJJSlimAInput_MouseLeave(object sender, EventArgs e)
        {
            if (int.TryParse(((DrawImage)sender).Tag?.ToString(), out int index) && _ledSelected != index)
            {
                ((DrawImage)sender).Image = null;
            }
        }

        private void ImgJJSlimAInput_MouseEnter(object sender, EventArgs e)
        {
            ((DrawImage)sender).Image = Resources.JJB_Slim_A_hover;
        }

        private void ImgJJSlimAInput_Click(object sender, EventArgs e)
        {
            _saveData = false;
            foreach (Control control in PnlJJBSlimA.Controls)
            {
                if (control is DrawImage picture && control.Name.StartsWith("ImgJJSlimAInput") && int.TryParse(((DrawImage)picture).Tag?.ToString(), out int pictureIndex) && pictureIndex == _ledSelected)
                {
                    picture.Image = null;
                }
            }

            if (int.TryParse(((DrawImage)sender).Tag?.ToString(), out int led) && _ledSelected != led && led > -1)
            {
                // LED selected
                _ledSelected = led;
                ((DrawImage)sender).Image = Resources.JJB_Slim_A_hover;

                // Hide instructions, show controls
                LblSelectLedInstructions.Visible = false;
                TxtGeneralConfigs.Visible = true;
                materialDivider3.Visible = true;
                cmdBoxLedMode.Visible = true;

                // Load LED mode from profile.Data["led_mode"]
                int ledModeValue = 0; // Default to mode 0 (Desligado)

                if (_device.Profile.Data.ContainsKey("led_mode"))
                {
                    JsonArray ledModeArray = _device.Profile.Data["led_mode"].AsArray();
                    if (led < ledModeArray.Count)
                    {
                        ledModeValue = ledModeArray[led].GetValue<int>();
                    }
                }

                cmdBoxLedMode.SelectedIndex = ledModeValue;

                cmdBoxLedMode.Enabled = true;

                // Load DataGridView with actions for this LED
                LoadFormData();

                // Re-enable saving after loading is complete
                _saveData = true;
            }
            else
            {
                // No LED selected
                cmdBoxLedMode.SelectedIndex = -1;
                cmdBoxLedMode.Enabled = false;
                _ledSelected = -1;

                // Show instructions, hide controls
                LblSelectLedInstructions.Visible = true;
                TxtGeneralConfigs.Visible = false;
                materialDivider3.Visible = false;
                cmdBoxLedMode.Visible = false;
                SldLedBrightness.Visible = false;
                SldPulseSpeed.Visible = false;
                SldBlinkSpeed.Visible = false;
                TxtProperties.Visible = false;
                PropertiesDivider.Visible = false;
                dgvActions.Visible = false;
                BtnAddLedAction.Visible = false;

                // Clear DataGridView when no LED is selected
                LoadFormData();
            }

            cmdBoxLedMode.Refresh();
            ReorganizeFlowLayoutPanel();

            _saveData = true;
        }
        #endregion

        private void SaveGeneralConfig()
        {
            // Don't save during initial data loading
            if (_isLoadingData)
                return;

            JsonObject profileData = _device.Profile.Data;
            int brightness = SldLedBrightness.Value;
            int pulseSpeed = SldPulseSpeed.Value;
            int blinkSpeed = SldBlinkSpeed.Value;

            if (!profileData.ContainsKey("brightness"))
            {
                profileData.Add("brightness", brightness);
            }
            else
            {
                profileData["brightness"] = brightness;
            }

            if (!profileData.ContainsKey("pulse_speed"))
            {
                profileData.Add("pulse_speed", pulseSpeed);
            }
            else
            {
                profileData["pulse_speed"] = pulseSpeed;
            }

            if (!profileData.ContainsKey("blink_speed"))
            {
                profileData.Add("blink_speed", blinkSpeed);
            }
            else
            {
                profileData["blink_speed"] = blinkSpeed;
            }

            _device.Profile.Update(profileData);
        }

        private void Save()
        {
            if (_ledSelected < 0 || !_saveData)
            {
                return;
            }

            int mode = cmdBoxLedMode.SelectedIndex < 0 ? 0 : cmdBoxLedMode.SelectedIndex;

            // Save LED mode in profile.Data for quick access
            JsonObject profileData = _device.Profile.Data;
            JsonArray ledMode;

            if (profileData.ContainsKey("led_mode"))
            {
                ledMode = profileData["led_mode"].AsArray();
            }
            else
            {
                // Initialize with 8 LEDs in mode 0
                ledMode = new JsonArray(0, 0, 0, 0, 0, 0, 0, 0);
                profileData.Add("led_mode", ledMode);
            }

            // Update the mode for the selected LED
            ledMode[_ledSelected] = mode;
            _device.Profile.Update(profileData);

            // Also save output for non-SimHub modes (0-3)
            // Mode 4 (SimHub) outputs are created via DataGridView
            //if (mode != 4)
            //{
            //    JsonArray ledsGrouped = new JsonArray { _ledSelected };
            //    int order = 0;

            //    _data = new JsonObject
            //    {
            //        {
            //            "leds", ledsGrouped
            //        },
            //        {
            //            "color", "#000000"
            //        },
            //        {
            //            "jj_prop", null
            //        },
            //        {
            //            "prop_name", null
            //        },
            //        {
            //            "order", order
            //        },
            //        {
            //            "led_mode", mode
            //        },
            //        {
            //            "led_type", "pwm"
            //        },
            //        {
            //            "led_mode_simhub", -1
            //        }
            //    };

            //    // Find existing output for this LED (non-SimHub output)
            //    Output existingOutput = _device.Profile.Outputs.FirstOrDefault(o =>
            //        o.Mode == Output.OutputMode.Leds &&
            //        o.Led != null &&
            //        o.Led.LedsGrouped != null &&
            //        o.Led.LedsGrouped.Contains(_ledSelected) &&
            //        o.Led.Property == null); // Only non-SimHub outputs (SimHub outputs have Property set)

            //    if (existingOutput != null)
            //    {
            //        // Update existing output
            //        existingOutput.Led.JsonToLeds(_data);
            //        existingOutput.Changed = true;
            //        existingOutput.Save();
            //    }
            //    else
            //    {
            //        // Use the output slot at _ledSelected index (outputs are pre-created by Profile)
            //        if (_ledSelected < _device.Profile.Outputs.Count)
            //        {
            //            _device.Profile.Outputs[_ledSelected].SetToLeds((uint)_ledSelected, Leds.LedType.PWM);
            //            _device.Profile.Outputs[_ledSelected].Led.JsonToLeds(_data);
            //            _device.Profile.Outputs[_ledSelected].Changed = true;
            //            _device.Profile.Outputs[_ledSelected].Save();
            //        }
            //    }
            //}
        }

        private void ConnectionMonitorTimer_Tick(object sender, EventArgs e)
        {
            bool currentState = _device.IsConnected;

            // Check if connection state changed
            if (currentState != _lastConnectionState)
            {
                // If device disconnected and it was previously connected
                if (!currentState && _lastConnectionState)
                {
                    // Disable auto connect to prevent continuous reconnection attempts
                    // This happens when SimHub or other software takes control of the device
                    _device.AutoConnect = false;
                }

                _lastConnectionState = currentState;

                // Update button text based on connection status
                UpdateConnectionStatus();
            }
        }

        private void UpdateConnectionStatus()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateConnectionStatus));
                return;
            }

            if (_device.AutoConnect)
            {
                BtnConnect.Text = "Auto Conectar Habilitado";
                BtnConnect.Enabled = false;
            }
            else if (_device.IsConnected)
            {
                BtnConnect.Text = "Desconectar";
                BtnConnect.Enabled = true;
            }
            else
            {
                BtnConnect.Text = "Conectar";
                BtnConnect.Enabled = true;
            }
        }
    }
}
