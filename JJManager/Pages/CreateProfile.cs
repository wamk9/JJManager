using JJManager.Class;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Windows.Forms;
using ConfigClass = JJManager.Class.App.Config.Config;
using ProfileClass = JJManager.Class.App.Profile.Profile;

namespace JJManager.Pages
{
    public partial class CreateProfile : MaterialForm
    {
        private static Class.Device _JJManagerCommunication;
        private static DatabaseConnection _DatabaseConnection = new DatabaseConnection();
        private static Device _Device = null;
        
        #region WinForms
        private MaterialSkinManager materialSkinManager = null;
        #endregion

        public CreateProfile(Device device)
        {
            InitializeComponent();
            components = new System.ComponentModel.Container();

            _Device = device;

            // MaterialDesign
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;
        }

        private void BtnSaveProfile_Click(object sender, EventArgs e)
        {
            if (TxtProfileName.Text.Length == 0)
            {
                MessageBox.Show("Insira um nome de perfil para continuar.");
                TxtProfileName.SetErrorState(true);
                return;
            }

            ProfileClass newProfile = new ProfileClass(_Device, TxtProfileName.Text);

            Close();
        }
    }
}
