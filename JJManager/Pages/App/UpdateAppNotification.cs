using JJManager.Class;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using ConfigClass = JJManager.Class.App.Config.Config;

namespace JJManager.Pages.App
{
    public partial class UpdateAppNotification : MaterialForm
    {
        JJManager.Class.App.SoftwareUpdater _softwareUpdater = null;
        MaterialForm _parent = null;

        #region WinForms
        private MaterialSkinManager materialSkinManager = null;
        #endregion

        public UpdateAppNotification(JJManager.Class.App.SoftwareUpdater softwareUpdater)
        {
            DatabaseConnection database = new DatabaseConnection();

            InitializeComponent();
            components = new System.ComponentModel.Container();

            _softwareUpdater = softwareUpdater;
            Text += _softwareUpdater.LastVersion;
 
            // MaterialDesign
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;

            FillChangeLog();

            FormClosing += UpdateAppNotification_FormClosing;
        }

        private void UpdateAppNotification_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseUpdateAppNotification();
        }

        public UpdateAppNotification(MaterialForm parent, JJManager.Class.App.SoftwareUpdater softwareUpdater)
        {
            DatabaseConnection database = new DatabaseConnection();
            _parent = parent;

            InitializeComponent();
            components = new System.ComponentModel.Container();

            _softwareUpdater = softwareUpdater;
            Text += _softwareUpdater.LastVersion;

            // MaterialDesign
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;

            FillChangeLog();

            FormClosing += UpdateAppNotification_FormClosing;

            StartPosition = FormStartPosition.CenterParent;
            Focus();

            if (_parent != null)
            {
                StartPosition = FormStartPosition.Manual;
                Location = new Point(_parent.Location.X + _parent.Width / 2 - Width / 2,
                    _parent.Location.Y + _parent.Height / 2 - Height / 2);
            }
            else
            {
                StartPosition = FormStartPosition.CenterParent;
            }
        }

        private Image GetImageFromWeb(string url)
        {
            Image image = null;

            try
            {

                if (url == "" || url == "#")
                {
                    return Properties.Resources.Logo_JohnJohn_JJMixer;
                }

                WebClient webClient = new WebClient();
                byte[] imageBytes = webClient.DownloadData(url);

                if (imageBytes != null && imageBytes.Length > 0)
                {
                    using (var ms = new System.IO.MemoryStream(imageBytes))
                    {
                        image = Image.FromStream(ms);
                    }
                }

                if (image == null)
                {
                    image = Properties.Resources.Logo_JohnJohn_JJMixer;
                }
            }
            catch (Exception)
            {
                image = Properties.Resources.Logo_JohnJohn_JJMixer;
            }

            return image;
        }

        private void FillChangeLog()
        {
            picChangeLog01.Image = GetImageFromWeb(_softwareUpdater.ChangeLog.ElementAt(0)[0]);
            txtChangeLogTitle01.Text = _softwareUpdater.ChangeLog.ElementAt(0)[1];
            txtChangeLogDesc01.Text = _softwareUpdater.ChangeLog.ElementAt(0)[2];

            picChangeLog02.Image = GetImageFromWeb(_softwareUpdater.ChangeLog.ElementAt(1)[0]);
            txtChangeLogTitle02.Text = _softwareUpdater.ChangeLog.ElementAt(1)[1];
            txtChangeLogDesc02.Text = _softwareUpdater.ChangeLog.ElementAt(1)[2];

            picChangeLog03.Image = GetImageFromWeb(_softwareUpdater.ChangeLog.ElementAt(2)[0]);
            txtChangeLogTitle03.Text = _softwareUpdater.ChangeLog.ElementAt(2)[1];
            txtChangeLogDesc03.Text = _softwareUpdater.ChangeLog.ElementAt(2)[2];
        }

        private void btnGoToUpdateScreen_Click(object sender, EventArgs e)
        {
            Main parentMain = (Main)_parent;
            // Do whatever you need with the desired child form
            // For example:
            parentMain.GoToUpdateTab();

            Close();
        }

        public void CloseUpdateAppNotification()
        {
            _parent.Visible = true;
            DialogResult = DialogResult.OK;
        }
    }
}
