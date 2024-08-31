using JJManager.Pages;
using MaterialSkin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace JJManager.Class
{
    public class DatabaseConnection :IDisposable
    {
        SqlConnection _connection;
        private string _DbFolderPath = "";
        private string _DbPath = "";

        public DatabaseConnection()
        {
            _DbFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JohnJohn3D", "JJManager");
            _DbPath = Path.Combine(_DbFolderPath, "JJManagerDB.mdf");

            if (!Directory.Exists(_DbFolderPath))
                Directory.CreateDirectory(_DbFolderPath);
                
            
            if (!File.Exists(_DbPath))
            {
                File.Copy(Path.Combine(Directory.GetCurrentDirectory(), "JJManagerDB_blank.mdf"), _DbPath);
                InitDatabase();
            }
        }

        public bool RunSQLMigrateFile(String sql)
        {
            bool isQueryExecuted = false;
            string sqlSplited = "";

            using (var connection = new SqlConnection(Properties.Settings.Default.DatabaseConnectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        using (var cmd = new SqlCommand())
                        {
                            cmd.Connection = connection;
                            cmd.Transaction = transaction;

                            cmd.Parameters.Clear();

                            foreach (String line in sql.Split(new string[2] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                if (line.ToUpperInvariant().Trim() == "GO")
                                {
                                    //Log.Insert("Migration", "Problema ocorrido na migração de dados (RunSQLMigrateFile)\r\n" + sqlSplited );

                                    cmd.CommandText = sqlSplited;
                                    cmd.ExecuteNonQuery();
                                    sqlSplited = String.Empty;
                                }
                                else
                                {
                                    sqlSplited += line + "\n";
                                }
                            }
                            
                            //cmd.Dispose();
                            transaction.Commit();
                            
                            isQueryExecuted = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Log.Insert("Migration", "Problema ocorrido na migração de dados (RunSQLMigrateFile)\r\n" + sqlSplited + "\r\n", ex);
                }
                finally
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                        //connection.Dispose();
                    }
                }
            }

            return isQueryExecuted;
        }

        /// <summary>
        /// Método responsável por executar querys SQL no DBLocal com retorno.
        /// </summary>
        /// <param name="sql">Query SQL</param>
        /// <returns>Variável 'SqlDataReader' contendo o resultado caso o SQL tenha sido executado com sucesso, caso não, retornará NULL.</returns>
        public JsonDocument RunSQLWithResults(String sql)
        {
            JsonDocument Json = null;
            String JsonString = "";

            using (var connection = new SqlConnection(Properties.Settings.Default.DatabaseConnectionString))
            {
                try
                {
                    connection.Open();
                    if (connection.State == ConnectionState.Open)
                    {
                        using (var cmd = new SqlCommand())
                        {
                            cmd.Connection = connection;
                            cmd.CommandText = sql;
                            
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                JsonString = "[";

                                List<string> Row = new List<string>();
                                Dictionary<string, string> objTmp = new Dictionary<string, string>();

                                while (reader.Read())
                                {
                                    JsonString = "{";

                                    for(int i = 0; i < reader.FieldCount; i++)
                                    {
                                        objTmp[reader.GetName(i)] = reader.GetValue(i).ToString();
                                    }

                                    Row.Add(JsonConvert.SerializeObject(objTmp));
                                }

                                if (Row.Count > 0)
                                    Json = JsonDocument.Parse("[" + String.Join(",", Row.ToArray()) + "]");

                                reader.Close();
                                //reader.Dispose();
                            }

                            //cmd.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Insert("SQL", $"Falha ao executar o SQL com resultados: '{sql}'", ex);
                }
                finally
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                        //connection.Dispose();
                    }
                }
            }

            return Json;
        }

        /// <summary>
        /// Método responsável por executar querys SQL no DBLocal SEM retorno (Apenas para Inserts ou Updates)
        /// </summary>
        /// <param name="sql">Query SQL</param>
        /// <returns>TRUE caso o SQL tenha sido executado com sucesso, caso não, retornará FALSE.</returns>
        public bool RunSQL(String sql)
        {
            bool isQueryExecuted = false;

            using (var connection = new SqlConnection(Properties.Settings.Default.DatabaseConnectionString))
            {
                try
                {
                    connection.Open();
                    if (connection.State == ConnectionState.Open)
                    {
                        using (var cmd = new SqlCommand())
                        {
                            cmd.Connection = connection;
                            cmd.CommandText = sql;

                            cmd.ExecuteNonQuery();
                            isQueryExecuted = true;
                            //cmd.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Insert("SQL", $"Falha ao executar o SQL: '{sql}'", ex);
                }
                finally
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                        //connection.Dispose();
                    }
                }
            }

            return isQueryExecuted;
        }

        private void InitDatabase ()
        {
            Version actualVersion = Assembly.GetEntryAssembly().GetName().Version;

            String sql = "INSERT INTO dbo.configs (Id, theme, software_version) VALUES (1 ,'dark', '" + actualVersion.Major.ToString() + "." + actualVersion.Minor.ToString() + "." + actualVersion.Build.ToString() + "');";
            
            if (!RunSQL(sql))
            {
                // TODO: Create LOGFILE
            }
        }

        public void CreateBackup()
        {
            string sql = "";

            try
            {
                DatabaseConnection connection = new DatabaseConnection();
                sql = "SELECT software_version FROM dbo.configs;";

                using (JsonDocument Json = RunSQLWithResults(sql))
                {
                    if (!Directory.Exists(Path.Combine(_DbFolderPath, "Backup")))
                        Directory.CreateDirectory(Path.Combine(_DbFolderPath, "Backup"));

                    sql = "BACKUP DATABASE \"" + _DbPath + "\" TO DISK = '" + Path.Combine(_DbFolderPath, "Backup", "JJManagerDB_" + Json.RootElement[0].GetProperty("software_version").GetString().Replace(".", "_") + ".bkp") + "' WITH FORMAT, COPY_ONLY";

                    RunSQL(sql);
                }
            }
            catch (Exception ex)
            {
                Log.Insert("Backup", "Problema ocorrido no backup de dados (CreateBackup)\r\n" + sql + "\r\n", ex);
            }
        }

        // Future release...
        /*public void RestoreBackup(Version version)
        {
            string sql = "";

            try
            {
                DatabaseConnection connection = new DatabaseConnection();
                sql = @"
                    DECLARE @DbId INT;
                    SET @DbId = DB_ID('" + _DbPath + @"');

                    DECLARE @Sql NVARCHAR(MAX) = '';

                    -- Generate dynamic SQL to kill active sessions
                    SELECT @Sql = @Sql + 'KILL ' + CAST(session_id AS NVARCHAR(5)) + ';'
                    FROM sys.dm_exec_sessions
                    WHERE database_id = @DbId;

                    -- Execute the generated SQL to kill sessions
                    EXEC sp_executesql @Sql;
                ";
                RunSQL(sql);

                sql = "USE master;";
                RunSQL(sql);

                sql = "ALTER DATABASE \"" + _DbPath + "\" SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
                RunSQL(sql);

                sql = "RESTORE DATABASE \"" + _DbPath + "\" FROM DISK = '" + Path.Combine(_DbFolderPath, "Backup", "JJManagerDB_" + version.Major.ToString() + "_" + version.Minor.ToString() + "_" + version.Build.ToString() + ".bkp") + "' WITH REPLACE, RECOVERY, STATS = 10;";
                RunSQL(sql);

                sql = "ALTER DATABASE \"" + _DbPath + "\" SET MULTI_USER;";
                RunSQL(sql);

                sql = "USE \"" + _DbPath + "\";";
                RunSQL(sql);


            }
            catch (Exception ex)
            {
                Log.Insert("Backup", "Problema ocorrido no backup de dados (CreateBackup)\r\n" + sql + "\r\n", ex);
            }
        }*/

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
