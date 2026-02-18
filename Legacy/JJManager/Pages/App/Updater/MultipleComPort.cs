using MaterialSkin;
using MaterialSkin.Controls;
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

namespace JJManager.Pages.App.Updater
{
    public partial class MultipleComPort : MaterialForm
    {
        private string _port = null;

        public string Port
        {
            get => _port;
        }

        #region WinForms
        private MaterialSkinManager materialSkinManager = null;
        #endregion

        public MultipleComPort(IReadOnlyList<string> ports)
        {
            InitializeComponent();

            components = new System.ComponentModel.Container();

            foreach (string port in ports)
            {
                cmbSelectedPort.Items.Add(port);
            }

            if (cmbSelectedPort.Items.Count > 0)
            {
                cmbSelectedPort.SelectedIndex = 0;
            }

            // MaterialDesign
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;

        }

        private void btnSelectPort_Click(object sender, EventArgs e)
        {
            if (cmbSelectedPort.SelectedIndex > -1)
            {
                _port = cmbSelectedPort.SelectedItem.ToString();
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
