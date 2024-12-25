using MaterialSkin;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JJManager.Class.App.Config.Theme
{
    public class Theme
    {
        private DatabaseConnection _dbConnection = null;
        private MaterialSkinManager.Themes _selectedTheme = MaterialSkinManager.Themes.DARK;
        private ColorScheme _selectedColorScheme = null;

        /// <summary>
        /// Get actual theme of JJManager
        /// </summary>
        public MaterialSkinManager.Themes SelectedTheme
        {
            get => _selectedTheme;
        }

        public ColorScheme SelectedColorScheme
        {
            get => _selectedColorScheme;
        }

        #region Constructors
        public Theme() 
        {
            _dbConnection = new DatabaseConnection();
            _selectedColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
            GetThemeIntoObject();
        }

        public Theme(DatabaseConnection dbConnection) 
        {
            _dbConnection = dbConnection;
            _selectedColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
            GetThemeIntoObject();
        }
        #endregion

        #region PrivateFunctions
        private void GetThemeIntoObject()
        {
            _dbConnection = _dbConnection == null ? new DatabaseConnection() : _dbConnection;

            String sql = $"SELECT theme FROM dbo.configs;";

            foreach (JsonObject obj in _dbConnection.RunSQLWithResults(sql))
            {
                if (obj.ContainsKey("theme"))
                {
                    switch (obj["theme"].GetValue<string>())
                    {
                        case "light":
                            _selectedTheme = MaterialSkinManager.Themes.LIGHT;
                            break;
                        case "dark":
                        default:
                            _selectedTheme = MaterialSkinManager.Themes.DARK;
                            break;
                    }
                }
            }
        }
        #endregion

        #region PublicFunctions
        public void Update(string themeName)
        {
            _dbConnection = _dbConnection == null ? new DatabaseConnection() : _dbConnection;
            string sql = $"UPDATE dbo.configs SET theme='{themeName}'";

            if (!_dbConnection.RunSQL(sql))
            {
                Log.Insert("Config", "Erro ao tentar atualizar o tema do JJManager.");
            }

            _selectedTheme = themeName == "dark" ? MaterialSkinManager.Themes.DARK : MaterialSkinManager.Themes.LIGHT;
        }
        #endregion
    }
}
