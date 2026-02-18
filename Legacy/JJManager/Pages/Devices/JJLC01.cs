using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ConfigClass = JJManager.Class.App.Config.Config;
using ProfileClass = JJManager.Class.App.Profile.Profile;
using JJDeviceClass = JJManager.Class.Devices.JJDevice;
using JJManager.Class;
using MaterialSkin.Controls;
using MaterialSkin;
using System.Threading;
using LiveCharts.Wpf;
using LiveCharts;
using Newtonsoft.Json.Linq;
using LiveCharts.Definitions.Charts;
using System.Windows.Input;
using System.Text.Json.Nodes;
using LiveCharts.Helpers;
using LiveCharts.Configurations;
using System.Windows;
using System.Text.Json;
using System.Globalization;
using JJManager.Class.Devices.Connections;

namespace JJManager.Pages.Devices
{
    public partial class JJLC01 : MaterialForm
    {
        private JJDeviceClass _device;
        //private static AudioManager _audioManager = new AudioManager();
        private static DatabaseConnection _DatabaseConnection = new DatabaseConnection();
        private Thread thrTimers = null;
        private bool _isInputSelected = false;
        private bool _IsCreateProfileOpened = false;
        private int _lastInputSelected = -1;
        private MaterialForm _parent = null;
        private bool _isLoadingData = false;
        private bool _lastConnectionState = false;
        private System.Windows.Forms.Timer _connectionMonitorTimer = null;
        private bool _loadingProfiles = false; // Flag to prevent SelectedIndexChanged during dropdown load
        private const string ACTIVE_PROFILE_NAME = "Perfil Ativo Ao Conectar"; // Temporary profile name (not in database)

        #region WinForms
        private MaterialSkinManager materialSkinManager = null;
        private AppModulesTimer JoystickReceiver = null;
        private AppModulesNotifyIcon notifyIcon = null;
        #endregion

        private ChartValues<double> values = new ChartValues<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private bool isDragging = false;
        private int selectedPointIndex = -1;
        private double _actualPointValue = 0.0;
        private MaterialButton btnUp, btnDown;
        private MaterialTextBox txtValue;
        private int hoveredPointIndex = -1; // No point hovered initially
        private System.Drawing.Point pointLocation = new System.Drawing.Point();
        public static SynchronizationContext UIContext { get; private set; } // Store UI context

        // Original firmware data captured on connection (read-only snapshot)
        private double[] _originalFirmwareAdc = null;
        private int _originalFirmwareFineOffset = 150;

        public JJLC01(MaterialForm parent, JJDeviceClass device)
        {
            InitializeComponent();
            components = new System.ComponentModel.Container();

            // MaterialDesign
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;
            ResetPointsValues();

            _device = device;

            Text = "JJLC-01";

            _parent = parent;

            // Clean up any existing profile with the reserved name from database (legacy fix)
            CleanupReservedProfileFromDatabase();

            TxtJJLC01Calibration.Text = string.Empty;
            TxtJJLC01Calibration.Text += "Sempre que você ligar o volante ou o cabo USB, a Load Cell JJLC-01 irá realizar uma calibração automática, buscando sempre a sua zona sem pressão do pedal.";
            TxtJJLC01Calibration.Text += Environment.NewLine;
            TxtJJLC01Calibration.Text += Environment.NewLine;
            TxtJJLC01Calibration.Text += "Em alguns casos, essa zona pode variar, por isso recomendamos realizar o ajuste fino abaixo, para tal, mantenha o pedal sem pressão e após altere o valor no slider baseando-se no 'Valor atual' apresentado e salve as alterações.";
            TxtJJLC01Calibration.Text += Environment.NewLine;
            TxtJJLC01Calibration.Text += Environment.NewLine;
            TxtJJLC01Calibration.Text += "Mantenha sempre o 'Valor atual' abaixo, porém próximo de zero. Desta forma você não terá problema com acionamentos fantasmas da célula de carga.";

            // Fill Forms with profiles
            var allProfiles = ProfileClass.GetProfilesList(_device.ProductId);

            // Always add active profile first (shows firmware raw data)
            CmbBoxSelectProfile.Items.Add(ACTIVE_PROFILE_NAME);

            // Add all profiles from database (presets for quick loading)
            // Filter out any profile with the reserved name to prevent duplicates
            foreach (String profile in allProfiles)
            {
                if (profile != ACTIVE_PROFILE_NAME)
                {
                    CmbBoxSelectProfile.Items.Add(profile);
                }
            }

            // Always start with active profile (firmware data)
            CmbBoxSelectProfile.SelectedIndex = 0;

            // Events
            FormClosing += JJLC01_FormClosing; ;
            CmbBoxSelectProfile.DropDown += CmbBoxSelectProfile_DropDown;
            CmbBoxSelectProfile.SelectedIndexChanged += CmbBoxSelectProfile_SelectedIndexChanged;

            // Define initial points
            var labels = new[] { "0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%" };

            // Add line series
            ChtLoadCell.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Values = values,
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 15,
                    Title = "Quilos da Célula de Carga"
                }
            };

            ChtLoadCell.AxisX.Add(new Axis
            {
                Title = "Percentual Do Potenciômetro",
                Labels = labels, // Set custom labels
                Separator = new LiveCharts.Wpf.Separator { Step = 1 } // Ensure labels are spaced correctly
            });

            ChtLoadCell.AxisY.Add(new Axis
            {
                Title = "Quilos da Célula de Carga",
                MinValue = 0, // Minimum Y-axis value
                MaxValue = 100, // Maximum Y-axis value
                LabelFormatter = value => $"{value} kg"  // Format labels as needed
            });

            ChtLoadCell.DataTooltip = null;

            Label customTooltip = new Label
            {
                BackColor = Color.White,
                ForeColor = Color.Black,
                Padding = new Padding(5),
                Visible = false, // Initially hidden
                AutoSize = true
            };

            this.Controls.Add(customTooltip);
            customTooltip.BringToFront();

            ChtLoadCell.Hoverable = true; // Keep hover active

            ChtLoadCell.DataHover += (sender, args) =>
            {
                // Get the chart point from the hover event
                var chartPoint = args.AsPoint();
                hoveredPointIndex = args.Key; // Update hovered index

                if (chartPoint != null)
                {
                    // Set the tooltip content based on the hovered point (e.g., the Y value)
                    customTooltip.Text = $"{chartPoint.X * 10}% de potenciômetro em {chartPoint.Y} kg";  // Display Y value with a unit (kg)
                    // Calculate the position where you want the tooltip
                    var closestPoint = ChtLoadCell.Series[0].ClosestPointTo(hoveredPointIndex, AxisOrientation.X);
                    var tooltipX = closestPoint.ChartLocation.X + ChtLoadCellJJLC01.Location.X + 49 - 5; // X offset to the right
                    var tooltipY = closestPoint.ChartLocation.Y + ChtLoadCellJJLC01.Location.Y; // Y offset above the point

                    pointLocation = new System.Drawing.Point((int)tooltipX, (int)(tooltipY));

                    // Set the tooltip's position
                    customTooltip.Location = new System.Drawing.Point((int)tooltipX, (int)tooltipY - 30);
                    customTooltip.Visible = true;
                }
            };

            ChtLoadCell.MouseMove += (sender, args) =>
            {
                var pointX = args.GetPosition(ChtLoadCell).X + ChtLoadCellJJLC01.Location.X; // X offset to the right
                var pointY = args.GetPosition(ChtLoadCell).Y + ChtLoadCellJJLC01.Location.Y; // Y offset above the point

                if (pointX >= (pointLocation.X - 2) && pointX <= (pointLocation.X + 12)  &&
                    pointY >= (pointLocation.Y - 2) && pointY <= (pointLocation.Y + 12))
                {
                    customTooltip.Visible = true;
                }
                else
                {
                    customTooltip.Visible = false;
                }
            };

            // Add mouse event for dragging points
            ChtLoadCell.MouseMove += OnMouseMove;
            ChtLoadCell.MouseUp += (s, e) =>
            {
                if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                {
                    // Save automatically when user finishes dragging a point
                    SaveConfig();
                }
            };
            //foreach (String Profile in ProfileClass.GetProfilesList(_device.ProductId))
            //    CmbBoxSelectProfile.Items.Add(Profile);

            //if (CmbBoxSelectProfile.Items.Count == 0)
            //{
            //    CmbBoxSelectProfile.Items.Add(new ProfileClass(_device, "Perfil Padrão", true));
            //    CmbBoxSelectProfile.SelectedIndex = 0;
            //}
            //else
            //{
            //    CmbBoxSelectProfile.SelectedIndex = CmbBoxSelectProfile.FindStringExact(_device.Profile.Name); ;
            //}

            //// Events
            //foreach (Control control in Controls)
            //{
            //    if (control is PictureBox && control.Name.StartsWith("ImgJJSD01Input"))
            //    {
            //        ((PictureBox)control).Click += ImgJJSD01Input_Click;
            //        ((PictureBox)control).MouseEnter += ImgJJSD01Input_MouseEnter;
            //        ((PictureBox)control).MouseLeave += ImgJJSD01Input_MouseLeave;
            //    }
            //}

            //BtnAddProfile.Click += BtnAddProfile_Click;
            //BtnRemoveProfile.Click += BtnRemoveProfile_Click;
            //FormClosed += new FormClosedEventHandler(JJB01_V2_FormClosed);

            //SelectInput();

            //CmbBoxSelectProfile.DropDown += new EventHandler(CmbBoxSelectProfile_DropDown);
            //CmbBoxSelectProfile.SelectedIndexChanged += new EventHandler(CmbBoxSelectProfile_SelectedIndexChanged);

            UIContext = SynchronizationContext.Current; // Capture UI thread context
                                                        // Create Buttons and TextBox


            // Define Mapper for Dynamic Point Color Change
            var mapper = Mappers.Xy<double>()
                .X((value, index) => index)
                .Y(value => value)
                .Fill((value, index) => index == selectedPointIndex ? System.Windows.Media.Brushes.Cyan : System.Windows.Media.Brushes.Blue);

            Charting.For<double>(mapper);

            // Initial Value
            //txtValue.Text = values[selectedPointIndex].ToString();

            ChtLoadCell.DataClick += (s, e) =>
            {
                if (e.SeriesView is LineSeries lineSeries)
                {
                    selectedPointIndex = int.Parse(e.AsPoint().X.ToString());
                    TxtLCWeight.Enabled = true;

                    UpdateTextBox(); // Reformat with "kg"
                                        
                    ChtLoadCell.Update(false, true); // Refresh Chart
                }
            };

            Load += (s, e) =>
            {
                // On form load, show active profile data
                ProfileClass initialProfile = null;

                if (!_device.IsConnected)
                {
                    // Device disconnected - create empty temporary profile
                    initialProfile = new ProfileClass(_device, ACTIVE_PROFILE_NAME, true, true);
                    _device.Profile = initialProfile;
                }
                else
                {
                    // Device connected - use device's temporary profile (with firmware data)
                    initialProfile = _device.Profile;
                }

                ShowProfileConfigs(initialProfile);
            };

            // Initialize connection monitor timer
            _lastConnectionState = _device.IsConnected;
            _connectionMonitorTimer = new System.Windows.Forms.Timer();
            _connectionMonitorTimer.Interval = 1000; // Check every 1 second
            _connectionMonitorTimer.Tick += ConnectionMonitorTimer_Tick;
            _connectionMonitorTimer.Start();

            UpdateConnectionStatus();
        }


        private void CmbBoxSelectProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Don't process if we're just loading the profile list
            if (_loadingProfiles)
            {
                return;
            }

            if (CmbBoxSelectProfile.SelectedIndex == -1)
            {
                if (CmbBoxSelectProfile.Items.Count > 0)
                {
                    CmbBoxSelectProfile.SelectedIndex = 0;
                }
                return;
            }

            string selectedProfileName = CmbBoxSelectProfile.SelectedItem.ToString();

            // If selecting the active profile (firmware data)
            if (selectedProfileName == ACTIVE_PROFILE_NAME)
            {
                if (!_device.IsConnected)
                {
                    // Device disconnected - create empty temporary profile for UI
                    ProfileClass emptyProfile = new ProfileClass(_device, ACTIVE_PROFILE_NAME, true, true);
                    _device.Profile = emptyProfile;
                    ShowProfileConfigs(emptyProfile);
                }
                else
                {
                    // Device connected - restore original firmware data captured at connection time
                    RestoreOriginalFirmwareData();
                }
            }
            else
            {
                // Load preset from database
                ProfileClass presetProfile = new ProfileClass(_device, selectedProfileName, false);

                // Load preset values into UI and send to firmware
                LoadPresetIntoUI(presetProfile);
            }
        }
        private void LoadPresetIntoUI(ProfileClass preset)
        {
            _isLoadingData = true;

            // Load preset values into UI (not into device profile)
            if (preset.Data.ContainsKey("jjlc01_data") &&
                preset.Data["jjlc01_data"].AsObject().ContainsKey("adc"))
            {
                for (int i = 0; i < preset.Data["jjlc01_data"]["adc"].AsArray().Count; i++)
                {
                    if (i > 10) break;
                    values[i] = preset.Data["jjlc01_data"]["adc"].AsArray()[i].GetValue<double>();
                }
            }
            else
            {
                // No data in preset, use defaults
                ResetPointsValues();
            }

            if (preset.Data.ContainsKey("jjlc01_data") &&
                preset.Data["jjlc01_data"].AsObject().ContainsKey("fine_offset"))
            {
                SldFineOffsetJJLC01.Value = preset.Data["jjlc01_data"]["fine_offset"].GetValue<int>();
                TxtJJLC01FineOffset.Text = $"Valor do ajuste fino: {(preset.Data["jjlc01_data"]["fine_offset"].GetValue<int>() - 150)}";
            }
            else
            {
                SldFineOffsetJJLC01.Value = 150;
                TxtJJLC01FineOffset.Text = "Valor do ajuste fino: 0";
            }

            // Enable controls (preset data can be edited)
            ChtLoadCell.IsEnabled = true;
            SldFineOffsetJJLC01.Enabled = true;
            TxtLCWeight.Enabled = true;
            BtnSaveLoadCellPoint.Enabled = false;

            ChtLoadCell.Update(false, true);
            _isLoadingData = false;

            // Automatically save to firmware (through device's active profile)
            SaveConfig();
        }

        private void ShowProfileConfigs(ProfileClass profile)
        {
            _isLoadingData = true;

            bool isConnected = _device.IsConnected;

            // If disconnected: zero values and lock controls
            if (!isConnected)
            {
                // Set ALL values to ZERO
                for (int i = 0; i < values.Count; i++)
                {
                    values[i] = 0;
                }

                SldFineOffsetJJLC01.Value = 150;
                TxtJJLC01FineOffset.Text = "Valor do ajuste fino: 0";

                // Lock all controls (read-only when disconnected)
                ChtLoadCell.IsEnabled = false;
                SldFineOffsetJJLC01.Enabled = false;
                TxtLCWeight.Enabled = false;
                BtnSaveLoadCellPoint.Enabled = false;
            }
            else
            {
                // Device connected - show data from profile parameter (firmware data)
                ChtLoadCell.IsEnabled = true;
                SldFineOffsetJJLC01.Enabled = true;
                TxtLCWeight.Enabled = true;
                BtnSaveLoadCellPoint.Enabled = false;

                // Reset to defaults first
                ResetPointsValues();

                // Load data from profile
                if (profile != null && profile.Data != null && profile.Data.ContainsKey("jjlc01_data"))
                {
                    var jjlc01Data = profile.Data["jjlc01_data"].AsObject();

                    // Load ADC curve points
                    if (jjlc01Data.ContainsKey("adc"))
                    {
                        var adcArray = jjlc01Data["adc"].AsArray();
                        for (int i = 0; i < Math.Min(adcArray.Count, values.Count); i++)
                        {
                            values[i] = adcArray[i].GetValue<double>();
                        }
                    }

                    // Load fine offset
                    if (jjlc01Data.ContainsKey("fine_offset"))
                    {
                        int fineOffset = jjlc01Data["fine_offset"].GetValue<int>();
                        SldFineOffsetJJLC01.Value = fineOffset;
                        TxtJJLC01FineOffset.Text = $"Valor do ajuste fino: {(fineOffset - 150)}";
                    }
                    else
                    {
                        SldFineOffsetJJLC01.Value = 150;
                        TxtJJLC01FineOffset.Text = "Valor do ajuste fino: 0";
                    }
                }
                else
                {
                    // No data in profile - use defaults
                    SldFineOffsetJJLC01.Value = 150;
                    TxtJJLC01FineOffset.Text = "Valor do ajuste fino: 0";
                }
            }

            ChtLoadCell.Update(false, true);
            _isLoadingData = false;
        }

        /// <summary>
        /// Restores the original firmware data captured when the device first connected.
        /// This ensures "Perfil Ativo Ao Conectar" always shows the data from connection time,
        /// not the last preset that was loaded.
        /// </summary>
        private void RestoreOriginalFirmwareData()
        {
            _isLoadingData = true;

            // Restore ADC curve from original firmware data
            if (_originalFirmwareAdc != null && _originalFirmwareAdc.Length > 0)
            {
                for (int i = 0; i < Math.Min(_originalFirmwareAdc.Length, values.Count); i++)
                {
                    values[i] = _originalFirmwareAdc[i];
                }
            }
            else
            {
                // No original data captured yet - use defaults
                ResetPointsValues();
            }

            // Restore fine offset from original firmware data
            SldFineOffsetJJLC01.Value = _originalFirmwareFineOffset;
            TxtJJLC01FineOffset.Text = $"Valor do ajuste fino: {(_originalFirmwareFineOffset - 150)}";

            // Enable controls (device is connected)
            ChtLoadCell.IsEnabled = true;
            SldFineOffsetJJLC01.Enabled = true;
            TxtLCWeight.Enabled = true;
            BtnSaveLoadCellPoint.Enabled = false;

            ChtLoadCell.Update(false, true);
            _isLoadingData = false;

            // Update device profile with original firmware data
            // This ensures the firmware receives the original data, not preset data
            JsonArray adcArray = new JsonArray();
            if (_originalFirmwareAdc != null)
            {
                foreach (double val in _originalFirmwareAdc)
                {
                    adcArray.Add(val);
                }
            }
            else
            {
                // Use current values if no original data
                foreach (double val in values)
                {
                    adcArray.Add(val);
                }
            }

            JsonObject jsonToUpdate = new JsonObject
            {
                {
                    "data", new JsonObject
                    {
                        {
                            "jjlc01_data", new JsonObject
                            {
                                {"adc", adcArray },
                                {"fine_offset", _originalFirmwareFineOffset }
                            }
                        }
                    }
                }
            };

            _device.Profile.Update(jsonToUpdate);
        }

        private void ResetPointsValues()
        {
            for (int i = 0; i < values.Count; i++ )
            {
                values[i] = i * 4;
            };
        }

        private void CmbBoxSelectProfile_DropDown(object sender, EventArgs e)
        {
            // Set flag to prevent SelectedIndexChanged from triggering during load
            _loadingProfiles = true;

            string selectedProfileName = CmbBoxSelectProfile.SelectedIndex >= 0 ?
                CmbBoxSelectProfile.SelectedItem.ToString() : null;

            CmbBoxSelectProfile.Items.Clear();

            // Always add temporary profile first (will be zeroed and locked if disconnected)
            CmbBoxSelectProfile.Items.Add(ACTIVE_PROFILE_NAME);

            // Add all profiles from database (filter out reserved name to prevent duplicates)
            foreach (String profile in ProfileClass.GetProfilesList(_device.ProductId))
            {
                if (profile != ACTIVE_PROFILE_NAME)
                {
                    CmbBoxSelectProfile.Items.Add(profile);
                }
            }

            // Restore selection by name if possible, otherwise select first
            if (!string.IsNullOrEmpty(selectedProfileName))
            {
                int profileIndex = CmbBoxSelectProfile.FindStringExact(selectedProfileName);
                if (profileIndex != -1)
                {
                    CmbBoxSelectProfile.SelectedIndex = profileIndex;
                }
                else if (CmbBoxSelectProfile.Items.Count > 0)
                {
                    CmbBoxSelectProfile.SelectedIndex = 0;
                }
            }
            else if (CmbBoxSelectProfile.Items.Count > 0)
            {
                CmbBoxSelectProfile.SelectedIndex = 0;
            }

            // Reset flag after loading is complete
            _loadingProfiles = false;
        }

        private void JJLC01_FormClosing(object sender, FormClosingEventArgs e)
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

        public static void UpdatePotPercent(int percent)
        {
            UIContext?.Post(_ =>
            {
                if (System.Windows.Forms.Application.OpenForms["JJLC01"] is JJLC01 form)
                {
                    form.TxtPotDataJJLC01.Text = $"Potenciômetro Press.: {percent}%";
                }
            }, null);
        }

        public static void UpdateKgPressed(double kgValue)
        {
            UIContext?.Post(_ =>
            {
                if (System.Windows.Forms.Application.OpenForms["JJLC01"] is JJLC01 form)
                {
                    form.TxtLoadCellDataJJLC01.Text = $"Quilos Press.: {kgValue:F1} kg";
                }
            }, null);
        }

        public static void UpdateRawData(int raw)
        {
            UIContext?.Post(_ =>
            {
                if (System.Windows.Forms.Application.OpenForms["JJLC01"] is JJLC01 form)
                {
                    form.TxtJJLC01RawData.Text = $"Valor atual: {raw}";
                }
            }, null);
        }

        public static void UpdateFineOffset(int offset, bool isOriginalFirmwareData = false)
        {
            UIContext?.Post(_ =>
            {
                if (System.Windows.Forms.Application.OpenForms["JJLC01"] is JJLC01 form)
                {
                    // Capture original firmware data on first connection
                    if (isOriginalFirmwareData)
                    {
                        form._originalFirmwareFineOffset = offset;
                    }

                    // Only update if not currently loading data
                    if (!form._isLoadingData)
                    {
                        form.SldFineOffsetJJLC01.Value = offset;
                        form.TxtJJLC01FineOffset.Text = $"Valor do ajuste fino: {(offset - 150)}";
                    }
                }
            }, null);
        }

        public static void UpdateSeries(double[] series, bool isOriginalFirmwareData = false)
        {
            UIContext?.Post(_ =>
            {
                if (System.Windows.Forms.Application.OpenForms["JJLC01"] is JJLC01 form)
                {
                    // Capture original firmware data on first connection
                    if (isOriginalFirmwareData || form._originalFirmwareAdc == null)
                    {
                        form._originalFirmwareAdc = (double[])series.Clone();
                    }

                    // Only update if not currently loading data
                    if (!form._isLoadingData)
                    {
                        for (int i = 0; i < Math.Min(series.Length, form.ChtLoadCell.Series.Count); i++)
                        {
                            if (form.ChtLoadCell.Series[i].Values is ChartValues<double> values)
                            {
                                values.Clear(); // Clear existing points

                                // Add ALL new points, not just one
                                foreach (var point in series)
                                {
                                    values.Add(point);
                                }

                                form.ChtLoadCell.Update(false, true);
                            }
                        }
                    }
                }
            }, null);
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Get the mouse position in chart values
                var mousePoint = ChtLoadCell.ConvertToChartValues(e.GetPosition(ChtLoadCell));
                double mouseX = mousePoint.X;

                // Find the closest point index
                selectedPointIndex = FindClosestPointIndex(mouseX);

                if (selectedPointIndex >= 0)
                {
                    // Convert mouse Y-coordinate to chart value
                    double newY = mousePoint.Y;

                    // Get the previous and next values
                    double previousValue = selectedPointIndex > 0 ? values[selectedPointIndex - 1] : 0;
                    double nextValue = (selectedPointIndex + 1 < values.Count) ? values[selectedPointIndex + 1] : 100;

                    // Ensure newY is within bounds
                    if (newY >= previousValue && newY <= nextValue)
                    {
                        values[selectedPointIndex] = Math.Round(newY,1);
                    }
                }

                UpdateTextBox();
            }
        }


        private int FindClosestPointIndex(double x)
        {
            // Find the closest point by X-coordinate
            for (int i = 0; i < values.Count; i++)
            {
                if (Math.Abs(i - x) < 0.5) // Adjust tolerance as needed
                {
                    return i;
                }
            }
            return -1; // No close point found
        }
        private void UpdateTextBox(bool preserveSeparator = false)
        {
            int caret = TxtLCWeight.SelectionStart;

            string value = values[selectedPointIndex] % 1 == 0 ? $"{values[selectedPointIndex]:0}" : $"{values[selectedPointIndex]:0.0}";
            
            TxtLCWeight.Text = $"{value}{(preserveSeparator && value.Count(c => c == ',') == 0 ? "," : "")} kg"; // Always show "kg"
            TxtLCWeight.SelectionStart = Math.Min(caret, TxtLCWeight.Text.Length); // Restore caret position
        }

        private void BtnMoreLCWeight_Click(object sender, EventArgs e)
        {
            if (selectedPointIndex >= 0)
            {
                string text = TxtLCWeight.Text.Replace("kg", "").Trim(); // Remove "kg" and extra spaces
                double actualValue = double.TryParse(text, out double value) ? value : values[selectedPointIndex];
                actualValue++;

                // Get the previous and next values
                double previousValue = selectedPointIndex > 0 ? values[selectedPointIndex - 1] : 0;
                double nextValue = (selectedPointIndex + 1 < values.Count) ? values[selectedPointIndex + 1] : 100;

                // Ensure newY is within bounds
                if (actualValue >= previousValue && actualValue <= nextValue)
                {
                    values[selectedPointIndex] = Math.Round(actualValue, 1);
                }

                UpdateTextBox();
                ChtLoadCell.Update(false, true);
            }
        }

        private void BtnLessLCWeight_Click(object sender, EventArgs e)
        {
            if (selectedPointIndex >= 0)
            {
                string text = TxtLCWeight.Text.Replace("kg", "").Trim(); // Remove "kg" and extra spaces
                double actualValue = double.TryParse(text, out double value) ? value : values[selectedPointIndex];

                // Get the previous and next values
                double previousValue = selectedPointIndex > 0 ? values[selectedPointIndex - 1] : 0;
                double nextValue = (selectedPointIndex + 1 < values.Count) ? values[selectedPointIndex + 1] : 100;
                actualValue--;

                // Ensure newY is within bounds
                if (actualValue >= previousValue && actualValue <= nextValue)
                {
                    values[selectedPointIndex] = Math.Round(actualValue, 1);
                }

                UpdateTextBox();
                ChtLoadCell.Update(false, true);
            }
        }

        private void TxtLCWeight_TextChanged(object sender, EventArgs e)
        {
            if (selectedPointIndex < 0 || selectedPointIndex > values.Count)
            {
                return;
            }

            string text = TxtLCWeight.Text.Replace("kg", "").Trim(); // Remove "kg" and extra spaces

            if (text.EndsWith(","))
            {
                return;
            }

            double actualValue = double.TryParse(text, out double value) ? value : 0;

            // Get the previous and next values
            double previousValue = selectedPointIndex > 0 ? values[selectedPointIndex - 1] : 0;
            double nextValue = (selectedPointIndex + 1 < values.Count) ? values[selectedPointIndex + 1] : 100;

            // Clamp value within bounds
            double clampedValue = Math.Max(previousValue, Math.Min(actualValue, nextValue));
            clampedValue = Math.Round(clampedValue, 1);

            if (clampedValue != values[selectedPointIndex] && (actualValue >= previousValue && actualValue <= nextValue))
            {
                BtnSaveLoadCellPoint.Enabled = true;
                _actualPointValue = clampedValue;
            }
            else
            {
                BtnSaveLoadCellPoint.Enabled = false;
                _actualPointValue = values[selectedPointIndex];
            }
        }

        private void CommitLoadCellValueWithText()
        {
            
        }

        private void TxtLCWeight_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                
            }
        }

        private void BtnRemoveProfile_Click(object sender, EventArgs e)
        {
            if (CmbBoxSelectProfile.SelectedIndex == -1)
            {

                Pages.App.MessageBox.Show(this, "Selecione um Perfil", "Selecione um perfil para excluí-lo.");
                return;
            }

            string selectedProfileName = CmbBoxSelectProfile.SelectedItem.ToString();

            // Check if trying to delete "Perfil Ativo Ao Conectar"
            if (selectedProfileName == ACTIVE_PROFILE_NAME)
            {
                Pages.App.MessageBox.Show(this, "Perfil Não Pode Ser Excluído",
                    "O 'Perfil Ativo Ao Conectar' é um perfil especial do sistema que captura automaticamente os dados do dispositivo ao conectar.\n\n" +
                    "Este perfil não pode ser excluído pois é essencial para o funcionamento do JJLC-01.");
                return;
            }

            if (CmbBoxSelectProfile.Items.Count == 1)
            {
                Pages.App.MessageBox.Show(this, "Não Pode Excluir", "Você possui apenas um perfil e este não pode ser excluído.");
                return;
            }

            DialogResult dialogResult = Pages.App.MessageBox.Show(this, "Exclusão de Perfil", "Você está prestes a excluir o Perfil '" + selectedProfileName + "', deseja continuar?", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                string profileNameToExclude = selectedProfileName;

                // Delete from database
                ProfileClass.Delete(profileNameToExclude, _device.ProductId);

                // Refresh dropdown completely to reflect database state
                _loadingProfiles = true;
                CmbBoxSelectProfile.Items.Clear();

                // Add active profile first
                CmbBoxSelectProfile.Items.Add(ACTIVE_PROFILE_NAME);

                // Add all profiles from database (after deletion, filter out reserved name)
                foreach (String profile in ProfileClass.GetProfilesList(_device.ProductId))
                {
                    if (profile != ACTIVE_PROFILE_NAME)
                    {
                        CmbBoxSelectProfile.Items.Add(profile);
                    }
                }

                _loadingProfiles = false;

                // Select active profile (first item in list)
                CmbBoxSelectProfile.SelectedIndex = 0;

                Pages.App.MessageBox.Show(this, "Perfil Excluído", "Perfil excluído com sucesso!");
            }
        }

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

        public static void NotifyDisconnectedDevice(string connId)
        {
            // Title no longer shows connection status
            // Connection status is now shown in the button
        }

        /// <summary>
        /// Removes any profile with the reserved name from database (legacy cleanup)
        /// </summary>
        private void CleanupReservedProfileFromDatabase()
        {
            try
            {
                // Check if there's a profile with the reserved name in the database
                var profiles = ProfileClass.GetProfilesList(_device.ProductId);
                if (profiles.Contains(ACTIVE_PROFILE_NAME))
                {
                    // Delete it from database
                    ProfileClass.Delete(ACTIVE_PROFILE_NAME, _device.ProductId);
                    Log.Insert("JJLC01", $"Removed legacy profile '{ACTIVE_PROFILE_NAME}' from database");
                }
            }
            catch (Exception ex)
            {
                Log.Insert("JJLC01", "Error cleaning up reserved profile from database", ex);
            }
        }

        private void TbcJJLC01Calibration_Click(object sender, EventArgs e)
        {

        }

        private void TxtJJLC01Calibration_Click(object sender, EventArgs e)
        {

        }

        private void SldFineOffsetJJLC01_onValueChanged(object sender, int newValue)
        {
            TxtJJLC01FineOffset.Text = $"Valor do ajuste fino: {(newValue - 150)}";
            // Save automatically when fine offset changes
            SaveConfig();
        }

        private void BtnSave_Click(object sender, EventArgs e)
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

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
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
                BtnSave.Text = "Auto Conectar Habilitado";
                BtnSave.Enabled = false;
            }
            else if (_device.IsConnected)
            {
                BtnSave.Text = "Desconectar";
                BtnSave.Enabled = true;
            }
            else
            {
                BtnSave.Text = "Conectar";
                BtnSave.Enabled = true;
            }
        }

        /// <summary>
        /// Enables chart and slider controls when device connects while viewing active profile.
        /// The firmware data will be populated through UpdateSeries/UpdateFineOffset callbacks.
        /// </summary>
        private void EnableControlsForConnectedDevice()
        {
            // Enable all controls (device just connected)
            ChtLoadCell.IsEnabled = true;
            SldFineOffsetJJLC01.Enabled = true;
            TxtLCWeight.Enabled = true;
            BtnSaveLoadCellPoint.Enabled = false;

            // Note: The actual data will come through UpdateSeries() and UpdateFineOffset()
            // which are called by the device class when it receives data from firmware
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

                    // If currently viewing active profile, refresh UI to show zeroed/locked state
                    string selectedProfileName = CmbBoxSelectProfile.SelectedItem?.ToString();
                    if (selectedProfileName == ACTIVE_PROFILE_NAME)
                    {
                        // Create empty temporary profile for UI when disconnected
                        ProfileClass emptyProfile = new ProfileClass(_device, ACTIVE_PROFILE_NAME, true, true);
                        _device.Profile = emptyProfile;
                        ShowProfileConfigs(emptyProfile);
                    }
                }

                // If device just connected (was disconnected, now connected)
                if (currentState && !_lastConnectionState)
                {
                    // Check current profile selection
                    string selectedProfileName = CmbBoxSelectProfile.SelectedItem?.ToString();
                    int activeProfileIndex = CmbBoxSelectProfile.FindStringExact(ACTIVE_PROFILE_NAME);

                    if (selectedProfileName == ACTIVE_PROFILE_NAME)
                    {
                        // Already on active profile - SelectedIndexChanged won't fire
                        // Manually enable controls and let firmware data come through callbacks
                        EnableControlsForConnectedDevice();
                    }
                    else if (activeProfileIndex != -1)
                    {
                        // Switch to active profile (this will trigger SelectedIndexChanged)
                        CmbBoxSelectProfile.SelectedIndex = activeProfileIndex;
                    }
                }

                _lastConnectionState = currentState;

                // Update button text based on connection status
                UpdateConnectionStatus();
            }
        }


        private void TxtLCWeight_Leave(object sender, EventArgs e)
        {
            CommitLoadCellValueWithText();
        }

        private void TxtLCWeight_Enter(object sender, EventArgs e)
        {
            
        }

        private void BtnSaveLoadCellPoint_Click(object sender, EventArgs e)
        {
            if (selectedPointIndex < 0 || selectedPointIndex > values.Count)
            {
                return;
            }

            values[selectedPointIndex] = _actualPointValue;
            ChtLoadCell.Update(false, true);
            BtnSaveLoadCellPoint.Enabled = false;

            // Save automatically when point is updated
            SaveConfig();
        }

        private void SaveConfig()
        {
            // Don't save during initial data loading
            if (_isLoadingData)
                return;

            // Don't save if temporary profile and disconnected (controls are locked)
            bool isTemporaryProfile = _device.Profile.Id.StartsWith("temp_");
            if (isTemporaryProfile && !_device.IsConnected)
                return;

            JsonArray adcValues = new JsonArray();

            foreach (LineSeries lineSeries in ChtLoadCell.Series)
            {
                foreach (var value in lineSeries.Values)
                {
                    adcValues.Add((Math.Round((double)value, 1)));
                }
            }


            var roundedValues = adcValues.Select(v => Math.Round(v.GetValue<double>(), 1)).ToArray();
            var jsonArray = new JsonArray { roundedValues.Select(v => JsonValue.Create(v.ToString("0.0", CultureInfo.InvariantCulture))) };

            // Convert the JsonArray to a JSON string
            string jsonString = jsonArray.ToString();

            // Parse the JSON string into a JsonObject
            var jsonObjectFromString = JsonArray.Parse(jsonString.Replace('"', ' '))[0];



            _device.Profile.Update(new JsonObject
            {
                {
                    "data",  new JsonObject
                    {
                        {
                            "jjlc01_data", new JsonObject {
                                {"adc", jsonObjectFromString.DeepClone() },
                                {"fine_offset", SldFineOffsetJJLC01.Value }
                            }
                        }
                    }
                }
            });
        }
    }
}
