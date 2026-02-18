using JJManager.Class;
using MaterialSkin.Controls;
using MaterialSkin;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using ConfigClass = JJManager.Class.App.Config.Config;
using ProfileClass = JJManager.Class.App.Profile.Profile;
using JJDeviceClass = JJManager.Class.Devices.JJDevice;

namespace JJManager.Pages.Devices
{
    public partial class JJQ01 : MaterialForm
    {
        private JJDeviceClass _device;
        private static DatabaseConnection _DatabaseConnection = new DatabaseConnection();
        private static ProfileClass _profile = null;
        private Thread thr = null;
        private Thread thrTimers = null;
        private bool _DisconnectDevice = false;
        private static bool _IsInputSelected = false;
        private bool _IsCreateProfileOpened = false;
        private String DataReceived = "";
        private String[] DataTreated = null;
        private MaterialForm _parent = null;
        private string frameToSend = "";
        private JJManager.Class.Devices.JJQ01 _jjq01Class = null;

        public string FrameToSend { get { return frameToSend; } }

        #region WinForms
        MaterialSkinManager materialSkinManager = null;
        private AppModulesTimer HidReceiver = null;
        private AppModulesNotifyIcon notifyIcon = null;
        #endregion

        public JJQ01(JJDeviceClass device)
        {
            InitializeComponent();
            components = new System.ComponentModel.Container();

            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;

            _device = device;
            //_jjq01Class = new JJManager.Class.Devices.JJQ01(_device);
            createWriteableForm();
        }

        public JJQ01(MaterialForm parent, JJDeviceClass device)
        {
            InitializeComponent();
            components = new System.ComponentModel.Container();

            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;

            _parent = parent;
            _device = device;
            //_jjq01Class = new JJManager.Class.Devices.JJQ01(_device);
            createWriteableForm();
        }

        private void createWriteableForm()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    PictureBox teste = new PictureBox();
                    Bitmap teste2 = JJManager.Properties.Resources.JJB_01;
                    teste2.SetResolution(16, 16);

                    teste.Image = teste2;
                    JJManager.Class.App.PixelGrid.Grid pixelEditor = new JJManager.Class.App.PixelGrid.Grid(teste);
                    pixelEditor.Location = new Point(160, 70);
                    pixelEditor.Name = "pnlPixelGrid";
                    Controls.Add(pixelEditor);
                }));
            }
            else
            {
                PictureBox teste = new PictureBox();
                Bitmap teste2 = JJManager.Properties.Resources.JJB_01;
                teste2.SetResolution(16, 16);

                teste.Image = teste2;
                JJManager.Class.App.PixelGrid.Grid pixelEditor = new JJManager.Class.App.PixelGrid.Grid(teste);
                pixelEditor.Location = new Point(160, 70);
                pixelEditor.Name = "pnlPixelGrid";
                Controls.Add(pixelEditor);
            }
        }

        private void materialButton1_Click(object sender, EventArgs e)
        {
            JJManager.Class.App.PixelGrid.Grid pixelEditor = null;
            foreach (Control control in Controls)
            {
                if (control.Name == "pnlPixelGrid")
                {
                    pixelEditor = (JJManager.Class.App.PixelGrid.Grid) control;
                    break;
                }
            }

            /*
            Control[] controls = Controls.Find("pnlPixelGrid", true);


            JJManager.Class.App.PixelGrid.Grid grid = (JJManager.Class.App.PixelGrid.Grid) controls[0];
            */
            frameToSend = string.Join("-", pixelEditor.GetPixelColors());

            //_jjq01Class.SaveFrame("0", "1000", frameToSend);
            Close();
        }
    }
}
