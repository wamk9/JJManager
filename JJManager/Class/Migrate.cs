using JJManager.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JJManager.Class
{

    internal class Migrate
    {
        private DatabaseConnection _database = new DatabaseConnection();
        private Version _actualVersion = Assembly.GetEntryAssembly().GetName().Version;
        private List<Version> _versions = new List<Version>();

        public Migrate() 
        {
            InitVersionList();

            String sql = "SELECT software_version FROM dbo.configs;";


            using (JsonDocument json = _database.RunSQLWithResults(sql))
            {
                if (json != null)
                {
                    Version dbVersion = new Version(json.RootElement[0].GetProperty("software_version").ToString());

                    if (_actualVersion > dbVersion)
                        ExecuteMigration(dbVersion);
                }
            }
        }

        /// <summary>
        /// Inicializa a lista preenchendo-a com todas as versões que já existiram do JJManager, incluindo a última disponível.
        /// </summary>
        private void InitVersionList()
        {
            _versions.Clear();

            _versions.Add(new Version(1, 1, 13)); // First Version
            //_versions.Add(new Version(1, 1, 1)); // Others Versions...
            _versions.Add(new Version(1, 1, 14)); // Last Version
        }

        private void ExecuteMigration (Version migrate_from)
        {
            foreach (Version version in _versions)
            {
                if (migrate_from < version)
                {
                    
                    String sql = Resources.ResourceManager.GetString("SQL_" + version.Major.ToString() + "_" + version.Minor.ToString() + "_" + version.Build.ToString(), Resources.Culture);

                    if (_database.RunSQLMigrateFile(sql))
                        MessageBox.Show("Banco de Dados atualizado para a versão " + version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString());
                    else
                        MessageBox.Show("Ocorreu um erro na atualização do banco de dados.");
                }
            }
        }
    }
}
