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
using System.Text.Json.Nodes;

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
        public JsonArray RunSQLWithResults(String sql)
        {
            JsonArray jsonRows = new JsonArray();

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
                                while (reader.Read())
                                {
                                    JsonObject jsonRow = new JsonObject();

                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        string columnName = reader.GetName(i);
                                        object value = !reader.IsDBNull(i) ? reader.GetValue(i) : null;

                                        // Check for empty or whitespace strings
                                        if (value is null || (value is string strValue && string.IsNullOrWhiteSpace(strValue)))
                                        {
                                            value = ""; // Optionally replace with null or a specific marker
                                        }

                                        jsonRow.Add(columnName, value.ToString());
                                    }

                                    jsonRows.Add(jsonRow);
                                }

                                reader.Close();
                            }
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
                    }
                }
            }

            return jsonRows;
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

        public void CreateBackup()
        {
            string sql = "";

            try
            {
                DatabaseConnection connection = new DatabaseConnection();
                sql = "SELECT software_version FROM dbo.configs;";

                foreach (JsonObject json in RunSQLWithResults(sql))
                {
                    if (!Directory.Exists(Path.Combine(_DbFolderPath, "Backup")))
                        Directory.CreateDirectory(Path.Combine(_DbFolderPath, "Backup"));

                    if (json.ContainsKey("software_version"))
                    {
                        sql = "BACKUP DATABASE \"" + _DbPath + "\" TO DISK = '" + Path.Combine(_DbFolderPath, "Backup", "JJManagerDB_" + json["software_version"].GetValue<string>().Replace(".", "_") + ".bkp") + "' WITH FORMAT, COPY_ONLY";
                        RunSQL(sql);
                    }
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
