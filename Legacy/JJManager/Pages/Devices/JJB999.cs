using JJManager.Class;
using JJManager.Class.App.Fonts;
using JJManager.Class.App.Output;
using JJManager.Class.App.Output.Leds;
using JJManager.Pages.App;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Windows.Forms;
using static JJManager.Class.App.Output.Output;
using ConfigClass = JJManager.Class.App.Config.Config;
using JJDeviceClass = JJManager.Class.Devices.JJDevice;
using ProfileClass = JJManager.Class.App.Profile.Profile;

namespace JJManager.Pages.Devices
{
    public partial class JJB999 : MaterialForm
    {
        private JJDeviceClass _device;

        private Thread thrTimers = null;
        private bool _IsInputSelected = false;
        private bool _IsCreateProfileOpened = false;
        private MaterialForm _parent = null;
        private BindingList<Output> _outputList = new BindingList<Output>();
        private Point _mousePosition = Point.Empty;
        private bool _isActionSelected = false;
        private bool _lastConnectionState = false;
        private System.Windows.Forms.Timer _connectionMonitorTimer = null;
        private bool _isLoadingData = false;

        #region WinForms
        private MaterialSkinManager materialSkinManager = null;
        private AppModulesTimer JoystickReceiver = null;
        private AppModulesNotifyIcon notifyIcon = null;
        #endregion

        public JJB999(MaterialForm parent, JJDeviceClass device)
        {
            InitializeComponent();
            components = new System.ComponentModel.Container();

            // MaterialDesign
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;

            _device = device;

            _parent = parent;

            // Fill Forms
            foreach (String Profile in ProfileClass.GetProfilesList(_device.ProductId))
                CmbBoxSelectProfile.Items.Add(Profile);

            if (CmbBoxSelectProfile.Items.Count == 0)
            {
                CmbBoxSelectProfile.Items.Add(new ProfileClass(_device, "Perfil Padrão", true));
                CmbBoxSelectProfile.SelectedIndex = 0;
            }
            else
            {
                CmbBoxSelectProfile.SelectedIndex = CmbBoxSelectProfile.FindStringExact(_device.Profile.Name); ;
            }

            // Events
            FormClosing += new FormClosingEventHandler(JJB999_FormClosing);
            //FormClosed += new FormClosedEventHandler(JJB999_FormClosed);
            CmbBoxSelectProfile.DropDown += new EventHandler(CmbBoxSelectProfile_DropDown);
            CmbBoxSelectProfile.SelectedIndexChanged += new EventHandler(CmbBoxSelectProfile_SelectedIndexChanged);
            cmdBoxLedMode.SelectedIndexChanged += new EventHandler(cmdBoxLedMode_SelectedIndexChanged);
            SldLedBrightness.onValueChanged += new MaterialSkin.Controls.MaterialSlider.ValueChanged(SldLedBrightness_onValueChanged);
            SldBlinkSpeed.onValueChanged += new MaterialSkin.Controls.MaterialSlider.ValueChanged(SldBlinkSpeed_onValueChanged);
            SldPulseSpeed.onValueChanged += new MaterialSkin.Controls.MaterialSlider.ValueChanged(SldPulseSpeed_onValueChanged);
            BtnAddLedAction.Click += new EventHandler(BtnAddLedAction_Click);

            ShowProfileConfigs();

            // Force correct control order in FlowLayoutPanel by removing and re-adding
            FlpJJB999.SuspendLayout();
            FlpJJB999.Controls.Clear();
            FlpJJB999.Controls.Add(TxtGeneralConfigs);
            FlpJJB999.Controls.Add(materialDivider3);
            FlpJJB999.Controls.Add(cmdBoxLedMode);
            FlpJJB999.Controls.Add(SldLedBrightness);
            FlpJJB999.Controls.Add(SldPulseSpeed);
            FlpJJB999.Controls.Add(SldBlinkSpeed);
            FlpJJB999.Controls.Add(TxtProperties);
            FlpJJB999.Controls.Add(PropertiesDivider);
            FlpJJB999.Controls.Add(dgvActions);
            FlpJJB999.Controls.Add(BtnAddLedAction);
            FlpJJB999.ResumeLayout(true);

            UpdateConnectionStatus();

            // Initialize connection monitor timer
            _lastConnectionState = _device.IsConnected;
            _connectionMonitorTimer = new System.Windows.Forms.Timer();
            _connectionMonitorTimer.Interval = 1000; // Check every 1 second
            _connectionMonitorTimer.Tick += ConnectionMonitorTimer_Tick;
            _connectionMonitorTimer.Start();

            typeof(DataGridView).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, dgvActions, new object[] { true });

            dgvActions.AutoGenerateColumns = false;
            dgvActions.DataSource = _outputList;
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
            dgvActions.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 5, 0, 5);
            dgvActions.AdvancedColumnHeadersBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.None;
            dgvActions.AdvancedColumnHeadersBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.None;
            dgvActions.AdvancedColumnHeadersBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;

            dgvActions.CellClick += DgvActions_CellClick;
            dgvActions.CellMouseEnter += DgvActions_CellMouseEnter;
            dgvActions.CellMouseLeave += DgvActions_CellMouseLeave;
            dgvActions.CellPainting += DgvActions_CellPainting;
            dgvActions.MouseMove += DgvActions_MouseMove;
            //SetEvents();
            LoadFormData();
        }

        private void OpenInputModal(ProfileClass profile, int idInput)
        {
            Pages.App.AudioController inputForm = new Pages.App.AudioController(this, _device.Profile, idInput);
            Visible = false;
            inputForm.ShowDialog();
            //_device.ActiveProfile.UpdateAnalogInputs(idInput);
            _IsInputSelected = false;
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
                    e.Graphics.FillPath(new SolidBrush(materialSkinManager.BackgroundColor.Darken((float).2)), path);
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
                OpenActionModal(int.Parse(dgvActions.Rows[e.RowIndex].Cells["Id"].Value.ToString()));
            }

            if (e.ColumnIndex == dgvActions.Columns["dgvActionRemove"].Index) // Index of your button column
            {
                DialogResult result = Pages.App.MessageBox.Show(this, "Confirmação de Exclusão", "Você deseja excluir está ação?", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    _device.Profile.ExcludeOutput(int.Parse(dgvActions.Rows[e.RowIndex].Cells["Id"].Value.ToString()), Output.OutputMode.Leds);
                    LoadFormData();
                }
            }

            if (e.ColumnIndex == dgvActions.Columns["dgvActionMoveUp"].Index) // Index of your button column
            {
                if (e.RowIndex == 0)
                {
                    return;
                }

                _device.Profile.MoveOutput(uint.Parse(dgvActions.Rows[e.RowIndex].Cells["Id"].Value.ToString()), (e.RowIndex - 1), dgvActions.RowCount - 1, Output.OutputMode.Leds);

                LoadFormData();
                dgvActions.Rows[(e.RowIndex - 1)].Selected = true;
            }

            if (e.ColumnIndex == dgvActions.Columns["dgvActionMoveDown"].Index) // Index of your button column
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

        private void LoadProfiles()
        {
            foreach (String Profile in ProfileClass.GetProfilesList(_device.ProductId))
                CmbBoxSelectProfile.Items.Add(Profile);

            if (CmbBoxSelectProfile.Items.Count == 0)
            {
                CmbBoxSelectProfile.Items.Add(new ProfileClass(_device, "Perfil Padrão", true));
                CmbBoxSelectProfile.SelectedIndex = 0;
            }
            else
            {
                CmbBoxSelectProfile.SelectedIndex = CmbBoxSelectProfile.FindStringExact(_device.Profile.Name); ;
            }

        }

        private void LoadFormData()
        {
            _isLoadingData = true;
            // Configure LED Mode ComboBox with DataSource
            var ledModes = new[]
            {
                new { Text = "Desligado", Value = 0 },
                new { Text = "Sempre Ligado", Value = 1 },
                new { Text = "Pulsando", Value = 2 },
                new { Text = "Piscando", Value = 3 },
                new { Text = "SimHub Sync", Value = 4 }
            };

            cmdBoxLedMode.DataSource = ledModes;
            cmdBoxLedMode.DisplayMember = "Text";
            cmdBoxLedMode.ValueMember = "Value";

            // Load LED mode from profile IMMEDIATELY after setting DataSource
            if (_device.Profile.Data.ContainsKey("led_mode"))
            {
                switch (_device.Profile.Data["led_mode"].GetValueKind())
                {
                    case JsonValueKind.String:
                        string ledModeString = _device.Profile.Data["led_mode"].GetValue<string>();
                        if (int.TryParse(ledModeString, out int ledModeValue) && (ledModeValue < cmdBoxLedMode.Items.Count))
                        {
                            cmdBoxLedMode.SelectedValue = ledModeValue;
                        }
                        break;
                    case JsonValueKind.Number:
                        cmdBoxLedMode.SelectedValue = _device.Profile.Data["led_mode"].GetValue<int>();
                        break;
                }
            }

            cmdBoxLedMode.Refresh();

            _outputList.Clear();
            //_device.Profile.OrderOutputsBy(Output.OutputMode.Leds);

            // Ensure BindingList is initialized with Outputs from Profile
            foreach (Output item in _device.Profile?.Outputs
                .Where(o => o.Mode == Output.OutputMode.Leds && o.Led.LedsGrouped.Contains(0))
                .ToList().OrderBy(x => x.Led.Order))
            {
                _outputList.Add(item);
            }

            // Load brightness from profile
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
            else
            {
                SldLedBrightness.Value = 255; // Default value
            }

            // Load blink speed from profile (inverted: max slider = min value)
            if (_device.Profile.Data.ContainsKey("blink_speed"))
            {
                switch (_device.Profile.Data["blink_speed"].GetValueKind())
                {
                    case JsonValueKind.String:
                        string blinkSpeedString = _device.Profile.Data["blink_speed"].GetValue<string>();
                        if (int.TryParse(blinkSpeedString, out int blinkSpeedValue))
                        {
                            SldBlinkSpeed.Value = SldBlinkSpeed.RangeMax + SldBlinkSpeed.RangeMin - blinkSpeedValue;
                        }
                        break;
                    case JsonValueKind.Number:
                        int blinkSpeed = _device.Profile.Data["blink_speed"].GetValue<int>();
                        SldBlinkSpeed.Value = SldBlinkSpeed.RangeMax + SldBlinkSpeed.RangeMin - blinkSpeed;
                        break;
                }
            }
            else
            {
                SldBlinkSpeed.Value = SldBlinkSpeed.RangeMax + SldBlinkSpeed.RangeMin - 100; // Default value inverted
            }

            // Load pulse delay from profile (inverted: max slider = min value)
            if (_device.Profile.Data.ContainsKey("pulse_delay"))
            {
                switch (_device.Profile.Data["pulse_delay"].GetValueKind())
                {
                    case JsonValueKind.String:
                        string pulseDelayString = _device.Profile.Data["pulse_delay"].GetValue<string>();
                        if (int.TryParse(pulseDelayString, out int pulseDelayValue))
                        {
                            SldPulseSpeed.Value = SldPulseSpeed.RangeMax + SldPulseSpeed.RangeMin - pulseDelayValue;
                        }
                        break;
                    case JsonValueKind.Number:
                        int pulseDelay = _device.Profile.Data["pulse_delay"].GetValue<int>();
                        SldPulseSpeed.Value = SldPulseSpeed.RangeMax + SldPulseSpeed.RangeMin - pulseDelay;
                        break;
                }
            }
            else
            {
                SldPulseSpeed.Value = SldPulseSpeed.RangeMax + SldPulseSpeed.RangeMin - 100; // Default value inverted
            }

            dgvActions.Refresh();

            _isLoadingData = false;
        }

        public void OpenActionModal(int idAction)
        {
            Output output = _device.Profile?.Outputs?.Where(x => x.Id == idAction).FirstOrDefault();
            int index = _device.Profile.Outputs.IndexOf(output);

            if (output.Mode == OutputMode.None)
            {
                uint idSearched = _device.Profile?.Outputs?.First(x => x.Mode == OutputMode.None).Id ?? uint.MaxValue;

                if (idSearched == uint.MaxValue)
                {
                    Log.Insert("JJB999", "Ocorreu um problema ao buscar um ID para o output especificado.");
                    throw new ArgumentNullException("Ocorreu um problema ao buscar um ID para o output especificado.");
                }

                output.SetToLeds(idSearched, Leds.LedType.RGB);
            }

            Pages.App.LedMonoAction dashboardLedsPage = new Pages.App.LedMonoAction(this, output.Led, 0);
            Visible = false;

            if (dashboardLedsPage.ShowDialog() == DialogResult.OK)
            {
                _device.Profile.Outputs[index].Name = dashboardLedsPage.LedToSave.PropertyName ?? "Nenhuma função selecionada";
                _device.Profile.Outputs[index].Led.Update(dashboardLedsPage.LedToSave.objectToJson());
                _device.Profile.Outputs[index].Changed = true;
                _device.Profile.Outputs[index].Save();
                _device.Profile.NeedsUpdate = true;
                LoadFormData();
            }

            _isActionSelected = false;
        }

        #region Events
        private void JJB999_FormClosing(object sender, FormClosingEventArgs e)
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

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            notifyIcon.Hide();
            Visible = true;
            BringToFront();
        }

        private void TimerReceiveJoystickMessage_Tick(object sender, EventArgs e)
        {
/*            int valueX = _device.GetJoystickAxisPercentage(_device.Joystick, "X");
            int valueY = _device.GetJoystickAxisPercentage(_device.Joystick, "Y");
*/


            /*_audioManager.ChangeInputVolume(_profile.GetInputById(1), valueX);
            _audioManager.ChangeInputVolume(_profile.GetInputById(2), valueY);*/
        }

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
            if (CmbBoxSelectProfile.SelectedIndex == -1)
            {
                CmbBoxSelectProfile.SelectedIndex = 0;
            }

            _device.Profile = new ProfileClass(_device, CmbBoxSelectProfile.SelectedItem.ToString(), true);
            ShowProfileConfigs();
        }
        #endregion

        private void SaveConfig()
        {
            // Don't save during initial data loading
            if (_isLoadingData)
                return;

            JsonObject jsonData = new JsonObject{
                { "led_mode", cmdBoxLedMode.SelectedIndex },
                { "brightness", SldLedBrightness.Value},
                { "blink_speed", SldBlinkSpeed.RangeMax + SldBlinkSpeed.RangeMin - SldBlinkSpeed.Value},
                { "pulse_delay", SldPulseSpeed.RangeMax + SldPulseSpeed.RangeMin - SldPulseSpeed.Value}
            };

            _device.Profile.Update(new JsonObject { { "data", jsonData } });
        }

        #region Buttons
        private void BtnAddProfile_Click(object sender, EventArgs e)
        {
            Thread thr = new Thread(() => {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        if (!_IsCreateProfileOpened)
                        {
                            _IsCreateProfileOpened = true;
                            CreateProfile createProfile = new CreateProfile(_device);
                            createProfile.ShowDialog();
                            _IsCreateProfileOpened = false;
                        }
                    });
                }
                else
                {
                    if (!_IsCreateProfileOpened)
                    {
                        _IsCreateProfileOpened = true;
                        CreateProfile createProfile = new CreateProfile(_device);
                        createProfile.ShowDialog();
                        _IsCreateProfileOpened = false;
                    }
                }
                Thread.CurrentThread.Abort();
            });
            thr.Name = "Add_Profile";
            thr.Start();
        }
        #endregion

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

            DialogResult dialogResult = Pages.App.MessageBox.Show(this, "Exclusão de Perfil", "Você está prestes a excluir o Perfil '" + CmbBoxSelectProfile.SelectedItem.ToString() + "', deseja continuar?", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                string profileNameToExclude = CmbBoxSelectProfile.SelectedItem.ToString();

                CmbBoxSelectProfile.Items.Remove(CmbBoxSelectProfile.SelectedItem);

                string profileNameToActive = CmbBoxSelectProfile.Items[(CmbBoxSelectProfile.Items[0] == CmbBoxSelectProfile.SelectedItem ? 1 : 0)].ToString();

                _device.Profile = new ProfileClass(_device, profileNameToActive, true);

                ProfileClass.Delete(profileNameToExclude, _device.ProductId);

                CmbBoxSelectProfile.SelectedIndex = 0;

                Pages.App.MessageBox.Show(this, "Perfil Excluído", "Perfil excluído com sucesso!");

            }
        }

        private void btnSaveConfig_Click(object sender, EventArgs e)
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

        private void cmdBoxLedMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedMode = (int)(cmdBoxLedMode?.SelectedValue ?? -1);

            // Save automatically when LED mode changes
            SaveConfig();

            // Suspend layout updates during multiple visibility changes
            FlpJJB999.SuspendLayout();

            try
            {
                // Update LED image visibility
                if (selectedMode == 0)
                {
                    ImgJJB999Off.Visible = true;
                    ImgJJB999On.Visible = false;
                }
                else
                {
                    ImgJJB999Off.Visible = false;
                    ImgJJB999On.Visible = true;
                }

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
            }
            finally
            {
                // Resume layout and force reorganization
                FlpJJB999.ResumeLayout(true);
            }
        }

        private void ShowProfileConfigs()
        {
            // Reload form data with profile values
            LoadFormData();

            // Get selected mode
            int selectedMode = (int)(cmdBoxLedMode?.SelectedValue ?? -1);

            // Suspend layout updates during multiple visibility changes
            FlpJJB999.SuspendLayout();

            try
            {
                // Update LED image visibility
                if (selectedMode == 0)
                {
                    ImgJJB999Off.Visible = true;
                    ImgJJB999On.Visible = false;
                }
                else
                {
                    ImgJJB999Off.Visible = false;
                    ImgJJB999On.Visible = true;
                }

                // Update sliders visibility based on loaded mode
                SldLedBrightness.Visible = (selectedMode >= 1);
                SldPulseSpeed.Visible = (selectedMode == 2 || selectedMode == 4);
                SldBlinkSpeed.Visible = (selectedMode == 3 || selectedMode == 4);

                // Update SimHub components visibility
                TxtProperties.Visible = (selectedMode == 4);
                PropertiesDivider.Visible = (selectedMode == 4);
                dgvActions.Visible = (selectedMode == 4);
                BtnAddLedAction.Visible = (selectedMode == 4);

                // Force correct order after visibility changes
                ReorganizeFlowLayoutPanel();
            }
            finally
            {
                // Resume layout and force reorganization
                FlpJJB999.ResumeLayout(true);
            }
        }

        private void ReorganizeFlowLayoutPanel()
        {
            // Force correct control order by using SetChildIndex
            // This ensures controls appear in the correct visual order
            if (FlpJJB999.Controls.Contains(TxtGeneralConfigs)) FlpJJB999.Controls.SetChildIndex(TxtGeneralConfigs, 0);
            if (FlpJJB999.Controls.Contains(materialDivider3)) FlpJJB999.Controls.SetChildIndex(materialDivider3, 1);
            if (FlpJJB999.Controls.Contains(cmdBoxLedMode)) FlpJJB999.Controls.SetChildIndex(cmdBoxLedMode, 2);
            if (FlpJJB999.Controls.Contains(SldLedBrightness)) FlpJJB999.Controls.SetChildIndex(SldLedBrightness, 3);
            if (FlpJJB999.Controls.Contains(SldPulseSpeed)) FlpJJB999.Controls.SetChildIndex(SldPulseSpeed, 4);
            if (FlpJJB999.Controls.Contains(SldBlinkSpeed)) FlpJJB999.Controls.SetChildIndex(SldBlinkSpeed, 5);
            if (FlpJJB999.Controls.Contains(TxtProperties)) FlpJJB999.Controls.SetChildIndex(TxtProperties, 6);
            if (FlpJJB999.Controls.Contains(PropertiesDivider)) FlpJJB999.Controls.SetChildIndex(PropertiesDivider, 7);
            if (FlpJJB999.Controls.Contains(dgvActions)) FlpJJB999.Controls.SetChildIndex(dgvActions, 8);
            if (FlpJJB999.Controls.Contains(BtnAddLedAction)) FlpJJB999.Controls.SetChildIndex(BtnAddLedAction, 9);
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
                btnSaveConfig.Text = "Auto Conectar Habilitado";
                btnSaveConfig.Enabled = false;
            }
            else if (_device.IsConnected)
            {
                btnSaveConfig.Text = "Desconectar";
                btnSaveConfig.Enabled = true;
            }
            else
            {
                btnSaveConfig.Text = "Conectar";
                btnSaveConfig.Enabled = true;
            }
        }

        private void SldLedBrightness_onValueChanged(object sender, int newValue)
        {
            // Save automatically when brightness changes
            SaveConfig();
        }

        private void SldBlinkSpeed_onValueChanged(object sender, int newValue)
        {
            // Save automatically when blink speed changes
            SaveConfig();
        }

        private void SldPulseSpeed_onValueChanged(object sender, int newValue)
        {
            // Save automatically when pulse speed changes
            SaveConfig();
        }

        private void btnSaveAndCloseConfig_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnAddLedAction_Click(object sender, EventArgs e)
        {
            LedMonoAction ledAction = new LedMonoAction(this, null, 0);
            DialogResult result = ledAction.ShowDialog();

            if (result == DialogResult.OK)
            {

                Output output = _device.Profile.Outputs.FirstOrDefault(x => x.Mode == Output.OutputMode.None);

                if (output == null)
                {
                    App.MessageBox.Show(this, "Problema Na Busca De Dados", "Ocorreu um problema na busca dos dados referentes a está saída, tente novamente realizar o processo.\n\nCaso o erro persista contate-nos.");
                }
                else
                {
                    // Used to get last order position
                    ledAction.LedToSave.Order = dgvActions.RowCount;

                    int index = _device.Profile.Outputs.IndexOf(output);
                    _device.Profile.Outputs[index].SetToLeds(output.Id, Leds.LedType.PWM);
                    _device.Profile.Outputs[index].Led.JsonToLeds(ledAction.LedToSave.objectToJson());
                    _device.Profile.Outputs[index].Changed = true;
                    _device.Profile.Outputs[index].Save();
                    LoadFormData();
                }
            }
        }

        private void JJB999_Load(object sender, EventArgs e)
        {

        }
    }
}
