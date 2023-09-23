using JJManager.Class;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JJManager.Pages
{
    public partial class UpdateDevice : Form
    {
        public UpdateDevice()
        {
            InitializeComponent();
            OpenFirmware.ShowDialog();
        }

        private void OpenFirmware_FileOk(object sender, CancelEventArgs e)
        {
            FirmwareUpdater firmwareUpdater = new FirmwareUpdater(OpenFirmware.FileName, "COM6", "Mixer de Áudio JJM-01");
        }
    }
}
