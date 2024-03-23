using JJManager.Class;
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

namespace JJManager.Pages
{
    public partial class CreateProfile : MaterialForm
    {
        private static Class.Device _JJManagerCommunication;
        //private static AudioManager _JJManagerAudioManager = new AudioManager();
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
            materialSkinManager.Theme = _DatabaseConnection.GetTheme();
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
        }

        private void BtnSaveProfile_Click(object sender, EventArgs e)
        {
            if (TxtProfileName.Text.Length == 0)
            {
                MessageBox.Show("Insira um nome de perfil para continuar.");
                TxtProfileName.SetErrorState(true);
                return;
            }

            _DatabaseConnection.SaveProfile(TxtProfileName.Text, _Device.Id);
            Close();
        }
    }
}
