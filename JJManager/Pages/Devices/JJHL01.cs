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
using JJHL01Class = JJManager.Class.Devices.JJHL01;

namespace JJManager.Pages.Devices
{
    public partial class JJHL01 : MaterialForm
    {
        private JJHL01Class _device;
        private bool _isInputSelected = false;
        private int _lastInputSelected = -1;
        private MaterialForm _parent = null;

        #region WinForms
        private MaterialSkinManager materialSkinManager = null;
        #endregion

        public JJHL01(MaterialForm parent, JJDeviceClass device)
        {
            InitializeComponent();
            components = new System.ComponentModel.Container();

            // MaterialDesign
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;

            Cyotek.Windows.Forms.ColorWheel colorWheel = new Cyotek.Windows.Forms.ColorWheel();
            Controls.Add(colorWheel);

            _device = device as JJHL01Class;

            _parent = parent;
        }
        private static String ToHex(System.Drawing.Color c)
    => $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        private void clrWheelSolid_ColorChanged(object sender, EventArgs e)
        {
            _device.Color = ToHex(clrWheelSolid.Color).Replace("#", "");
        }
    }
}
