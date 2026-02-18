using JJManager.Class;
using MaterialSkin;
using MaterialSkin.Controls;
using System.Drawing;
using System.Windows.Forms;
using ConfigClass = JJManager.Class.App.Config.Config;

namespace JJManager.Pages.App
{
    public partial class WaitForm : MaterialForm
    {
        MaterialSkinManager materialSkinManager = null;
        private static DatabaseConnection _DatabaseConnection = new DatabaseConnection();

        public WaitForm()
        {
            InitializeComponent();
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;

            this.StartPosition = FormStartPosition.CenterParent;

            this.Focus();
        }

        public WaitForm(MaterialForm parent)
        {
            InitializeComponent();
            
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;

            this.StartPosition = FormStartPosition.CenterParent;

            this.Focus();

            if (parent != null)
            {
                this.StartPosition = FormStartPosition.Manual;
                this.Location = new Point(parent.Location.X + parent.Width / 2 - this.Width / 2,
                    parent.Location.Y + parent.Height / 2 - this.Height / 2);
            }
            else
                this.StartPosition = FormStartPosition.CenterParent;
        }

        public void CloseWaitForm()
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
            /*if (label1.Image != null)
            {
                label1.Image.Dispose();
            }*/
        }
    }
}
