using JJManager.Properties;
using Microsoft.SqlServer.Management.Smo;
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
                    {
                        ExecuteMigration(dbVersion);
                    }
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
            _versions.Add(new Version(1, 1, 14));
            _versions.Add(new Version(1, 1, 15));
            _versions.Add(new Version(1, 2, 0));
            _versions.Add(new Version(1, 2, 1));
            _versions.Add(new Version(1, 2, 2));
            _versions.Add(new Version(1, 2, 3));
            _versions.Add(new Version(1, 2, 3, 1));
            _versions.Add(new Version(1, 2, 4)); // Last Version
        }

        private void ExecuteMigration (Version actual_version)
        {
            foreach (Version version in _versions)
            {
                if (version > actual_version)
                {
                    _database.CreateBackup();

                    String sql = Resources.ResourceManager.GetString("SQL_" + version.Major.ToString() + "_" + version.Minor.ToString() + "_" + version.Build.ToString() + (version.Revision  <= 0  ? "" : "_" + version.Revision.ToString()), Resources.Culture);

                    if (_database.RunSQLMigrateFile(sql))
                    {
                        actual_version = version;

                        // Fully qualified name including namespace
                        string className = "JJManager.MigrateCommands._" + version.Major.ToString() + "_" + version.Minor.ToString() + "_" + version.Build.ToString() + (version.Revision <= 0 ? "" : "_" + version.Revision.ToString());

                        // Get the type of the class
                        Type type = Type.GetType(className);

                        if (type != null)
                        {
                            type.GetMethod("ExecuteMigration").Invoke(null, null);
                        }
                        else
                        {
                            Console.WriteLine("Class not found.");
                        }

                        MessageBox.Show("Banco de Dados atualizado para a versão " + actual_version.ToString());
                    }
                    else
                    {
                        //_database.RestoreBackup(actual_version);
                        MessageBox.Show("Ocorreu um erro na atualização do banco de dados.");
                    }
                }
            }
        }
    }
}
