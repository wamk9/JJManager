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
using Avalonia.Controls;
using JJManager.Class;
using MaterialSkin.Controls;
using MaterialSkin;
using System.Threading;
using LiveCharts.Wpf;
using LiveCharts;
using Newtonsoft.Json.Linq;
using LiveCharts.Definitions.Charts;
using System.Windows.Input;

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

        private ChartValues<double> values;
        private bool isDragging = false;
        private int selectedPointIndex = -1;

        public JJLC01(MaterialForm parent, JJDeviceClass device)
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


            // Define initial points
            values = new ChartValues<double> { 0, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40};
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
            //FormClosing += new FormClosingEventHandler(JJSD01_FormClosing);
            ////FormClosed += new FormClosedEventHandler(JJB01_V2_FormClosed);

            //SelectInput();

            //CmbBoxSelectProfile.DropDown += new EventHandler(CmbBoxSelectProfile_DropDown);
            //CmbBoxSelectProfile.SelectedIndexChanged += new EventHandler(CmbBoxSelectProfile_SelectedIndexChanged);
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Get the mouse click position and find the closest point
                var mousePoint = ChtLoadCell.ConvertToChartValues(e.GetPosition(ChtLoadCell));
                double mouseX = mousePoint.X;

                // Determine the closest point
                selectedPointIndex = FindClosestPointIndex(mouseX);

                if (selectedPointIndex >= 0)
                {
                    // Convert mouse Y-coordinate to chart value
                    double newY = mousePoint.Y;

                    // Ensure the new Y value is not below the previous point
                    double previousValue = selectedPointIndex > 0 ? values[selectedPointIndex - 1] : double.MinValue;
                    double nextValue = selectedPointIndex > 0 && values.Count > (selectedPointIndex + 1) ? values[(selectedPointIndex + 1)] : 100;
                    if (newY >= 0 && newY <= 100)
                    {
                        values[selectedPointIndex] = Math.Round(newY);
                    }
                }
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
    }
}
