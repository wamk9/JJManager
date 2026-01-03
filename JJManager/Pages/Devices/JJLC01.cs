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

        #region WinForms
        private MaterialSkinManager materialSkinManager = null;
        private AppModulesTimer JoystickReceiver = null;
        private AppModulesNotifyIcon notifyIcon = null;
        #endregion

        private ChartValues<double> values = new ChartValues<double> { 0, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40 };
        private bool isDragging = false;
        private int selectedPointIndex = -1;
        private static bool _updatedChart = false;
        private static bool _updatedFineOffset = false;
        private double _actualPointValue = 0.0;
        private MaterialButton btnUp, btnDown;
        private MaterialTextBox txtValue;
        private int hoveredPointIndex = -1; // No point hovered initially
        private System.Drawing.Point pointLocation = new System.Drawing.Point();
        public static SynchronizationContext UIContext { get; private set; } // Store UI context

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

            Text = $"JJLC-01 ({_device.ConnId}) - {(_device.IsConnected ? "Conectado" : "Desconectado")}";

            _parent = parent;

            TxtJJLC01Calibration.Text = string.Empty;
            TxtJJLC01Calibration.Text += "Sempre que você ligar o volante ou o cabo USB, a Load Cell JJLC-01 irá realizar uma calibração automática, buscando sempre a sua zona sem pressão do pedal.";
            TxtJJLC01Calibration.Text += Environment.NewLine;
            TxtJJLC01Calibration.Text += Environment.NewLine;
            TxtJJLC01Calibration.Text += "Em alguns casos, essa zona pode variar, por isso recomendamos realizar o ajuste fino abaixo, para tal, mantenha o pedal sem pressão e após altere o valor no slider baseando-se no 'Valor atual' apresentado e salve as alterações.";
            TxtJJLC01Calibration.Text += Environment.NewLine;
            TxtJJLC01Calibration.Text += Environment.NewLine;
            TxtJJLC01Calibration.Text += "Mantenha sempre o 'Valor atual' abaixo, porém próximo de zero. Desta forma você não terá problema com acionamentos fantasmas da célula de carga.";
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
                    Title = "KG da Célula de Carga"
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
                Title = "KG da Célula de Carga",
                MinValue = 0, // Minimum Y-axis value
                MaxValue = 100, // Maximum Y-axis value
                LabelFormatter = value => $"{value} KG"  // Format labels as needed
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
                    customTooltip.Text = $"{chartPoint.X * 10}% de potênciometro em {chartPoint.Y} kg";  // Display Y value with a unit (kg)
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
                ShowProfileConfigs();
            };
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
        private void ShowProfileConfigs()
        {
            ResetPointsValues();

            if (_device.Profile.Data.ContainsKey("jjlc01_data") &&
                _device.Profile.Data["jjlc01_data"].AsObject().ContainsKey("adc"))
            {
                for (int i = 0; i < _device.Profile.Data["jjlc01_data"]["adc"].AsArray().Count; i++)
                {
                    if (i > 10)
                    {
                        break;
                    }

                    values[i] = _device.Profile.Data["jjlc01_data"]["adc"].AsArray()[i].GetValue<double>();
                }
            }

            if (_device.Profile.Data.ContainsKey("jjlc01_data") &&
                _device.Profile.Data["jjlc01_data"].AsObject().ContainsKey("fine_offset"))
            {
                SldFineOffsetJJLC01.Value = _device.Profile.Data["jjlc01_data"]["fine_offset"].GetValue<int>();
                TxtJJLC01FineOffset.Text = $"Valor do ajuste fino: {(_device.Profile.Data["jjlc01_data"]["fine_offset"].GetValue<int>() - 150)}";
            }

            ChtLoadCell.Update(false, true);
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
            int selectedIndex = CmbBoxSelectProfile.SelectedIndex;

            CmbBoxSelectProfile.Items.Clear();

            foreach (String Profile in ProfileClass.GetProfilesList(_device.ProductId))
                CmbBoxSelectProfile.Items.Add(Profile);

            CmbBoxSelectProfile.SelectedIndex = selectedIndex;
        }

        private void JJLC01_FormClosing(object sender, FormClosingEventArgs e)
        {
            _updatedFineOffset = false;
            _updatedChart = false;
            _parent.Visible = true;
        }

        public static void UpdatePotPercent(int percent)
        {
            UIContext?.Post(_ =>
            {
                if (System.Windows.Forms.Application.OpenForms["JJLC01"] is JJLC01 form)
                {
                    form.TxtPotDataJJLC01.Text = $"Potênciometro Press.: {percent}%";
                }
            }, null);
        }

        public static void UpdateKgPressed(double kgValue)
        {
            UIContext?.Post(_ =>
            {
                if (System.Windows.Forms.Application.OpenForms["JJLC01"] is JJLC01 form)
                {
                    form.TxtLoadCellDataJJLC01.Text = $"Kilos Press.: {kgValue} KG";
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

        public static void UpdateFineOffset(int offset)
        {
            if (_updatedFineOffset)
            {
                return;
            }

            UIContext?.Post(_ =>
            {
                if (System.Windows.Forms.Application.OpenForms["JJLC01"] is JJLC01 form)
                {
                    form.SldFineOffsetJJLC01.Value = offset;
                    form.TxtJJLC01FineOffset.Text = $"Valor do ajuste fino: {(offset - 150)}";
                    _updatedFineOffset = true;
                }
            }, null);
        }

        public static void UpdateSeries(double[] series)
        {
            if (_updatedChart)
            {
                return;
            }

            UIContext?.Post(_ =>
            {
                if (System.Windows.Forms.Application.OpenForms["JJLC01"] is JJLC01 form)
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

                            _updatedChart = true;

                            form.ChtLoadCell.Update(false, true);
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
            UIContext?.Post(_ =>
            {
                if (System.Windows.Forms.Application.OpenForms["JJLC01"] is JJLC01 form)
                {
                    form.Text = $"JJLC-01 ({connId}) - Desconectado";
                }
            }, null);
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
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
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
        }

        private void Save()
        {
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
