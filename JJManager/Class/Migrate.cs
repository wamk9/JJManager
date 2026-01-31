using JJManager.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;

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


            foreach (JsonObject json in _database.RunSQLWithResults(sql))
            {
                if (json.ContainsKey("software_version"))
                {
                    Version dbVersion = new Version(json["software_version"].GetValue<string>());

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
            _versions.Add(new Version(1, 2, 4));
            _versions.Add(new Version(1, 2, 5));
            _versions.Add(new Version(1, 2, 6));
            _versions.Add(new Version(1, 2, 6, 1));
            _versions.Add(new Version(1, 2, 7));
            _versions.Add(new Version(1, 2, 8));
            _versions.Add(new Version(1, 2, 9));
            _versions.Add(new Version(1, 3, 0)); // Last Version
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

                        if (_versions.Last() == actual_version)
                        {
                            Pages.App.MessageBox.Show(null, "Banco de Dados Atualizado", "Banco de Dados atualizado para a versão " + actual_version.ToString());
                        }
                    }
                    else
                    {
                        //_database.RestoreBackup(actual_version);
                        Pages.App.MessageBox.Show(null, "Erro na Atualização", "Ocorreu um erro na atualização do banco de dados para a versão " + version.ToString() + ".");
                    }
                }
            }
        }
    }
}
