using JJManager.Pages;
using MaterialSkin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace JJManager.Class
{
    internal class DatabaseConnection
    {
        SqlConnection _connection;

        public DatabaseConnection()
        {
            string _dbFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JohnJohn3D", "JJManager");
            string _dbPath = Path.Combine(_dbFolderPath, "JJManagerDB.mdf");

            if (!Directory.Exists(_dbFolderPath))
                Directory.CreateDirectory(_dbFolderPath);
                
            
            if (!File.Exists(_dbPath))
            {
                File.Copy(Path.Combine(Directory.GetCurrentDirectory(), "JJManagerDB_blank.mdf"), _dbPath);
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
                                    cmd.CommandText = sqlSplited;
                                    cmd.ExecuteNonQuery();
                                    sqlSplited = String.Empty;
                                }
                                else
                                {
                                    sqlSplited += line + "\n";
                                }
                            }

                            isQueryExecuted = true;
                            cmd.Dispose();
                            transaction.Commit();
                        }
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                        connection.Dispose();
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

                                List<String> Row = new List<String>();

                                while (reader.Read())
                                {
                                    JsonString = "{";

                                    for(int i = 0; i < reader.FieldCount; i++)
                                    {
                                        if (i == 0)
                                            JsonString += "\"" + reader.GetName(i) + "\":\"" + reader.GetValue(i) + "\"";
                                        else
                                            JsonString += ", \"" + reader.GetName(i) + "\":\"" + reader.GetValue(i) + "\"";
                                    }

                                    JsonString += "}";

                                    Row.Add(JsonString);
                                }

                                if (Row.Count > 0)
                                    Json = JsonDocument.Parse("[" + String.Join(",", Row.ToArray()) + "]");

                                reader.Close();
                                reader.Dispose();
                            }

                            cmd.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("RunSQLWithResults: " + ex);
                }
                finally
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                        connection.Dispose();
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
                            cmd.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("RunSQL: " + ex);
                }
                finally
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                        connection.Dispose();
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









        public String GetProductId(String productName)
        {
            String sql = "SELECT id FROM dbo.products WHERE name = '" + productName + "';";
            String id = "";

            using (JsonDocument Json = RunSQLWithResults(sql))
            {
                if (Json == null)
                    id = SetProductId(productName);
                else
                    id = Json.RootElement[0].GetProperty("id").ToString();
            }

            return id;
        }

        private String SetProductId(String productName)
        {
            String sql = "INSERT INTO dbo.products (name) VALUES ('" + productName + "');";
            String id = "";

            if (RunSQL(sql))
            {
                sql = "SELECT id FROM dbo.products WHERE name = '" + productName + "';";

                using (JsonDocument Json = RunSQLWithResults(sql))
                {
                    id = Json.RootElement[0].GetProperty("id").ToString();
                    SaveProfile("Perfil Padrão", id);
                }
            }

            return id;
        }

        public void SaveInputData (String id_profile, int input_id, String input_name, String input_type, String input_info, string invert_axis)
        {
            String sql = "MERGE analog_inputs WITH (SERIALIZABLE) AS T USING (VALUES (" + input_id.ToString() + ",'" + input_name + "', '" + input_type + "', '" + input_info + "', '" + invert_axis + "', '" + id_profile + "')) AS U (id, name, type, info, axis_orientation, id_profile) " +
                            "ON U.id = T.id AND U.id_profile = T.id_profile WHEN MATCHED THEN " +
                                "UPDATE SET name='" + input_name + "', type='" + input_type + "', info='" + input_info + "', axis_orientation='" + invert_axis + "' " +
                            "WHEN NOT MATCHED THEN " +
                            "INSERT (id, name, type, info, axis_orientation, id_profile) VALUES (" + input_id.ToString() + ", '" + input_name + "', '" + input_type + "', '" + input_info + "', '" + invert_axis + "' , '" + id_profile + "');";

            if (!RunSQL(sql))
            {
                // TODO: Create LOGFILE
            }
        }

        public Dictionary<string, string> GetInputData(String id_profile, int input_id)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();

            String sql = "SELECT name, type, info, axis_orientation FROM analog_inputs WHERE id=" + input_id.ToString() + " AND id_profile = '" + id_profile + "';";

            using (JsonDocument Json = RunSQLWithResults(sql))
            {
                if (Json != null)
                {
                    data["input_name"] = Json.RootElement[0].GetProperty("name").ToString();
                    data["input_type"] = Json.RootElement[0].GetProperty("type").ToString();
                    data["input_info"] = Json.RootElement[0].GetProperty("info").ToString();
                    data["axis_orientation"] = Json.RootElement[0].GetProperty("axis_orientation").ToString();

                }
            }

            return data;
        }

        public SortedDictionary<string, string> GetAllInputName(String id_profile)
        {
            SortedDictionary<string, string> data = new SortedDictionary<string, string>();

            String sql = "SELECT id, name FROM analog_inputs WHERE id_profile='" + id_profile + "' ORDER BY id;";

            using (JsonDocument Json = RunSQLWithResults(sql))
            {
                if (Json != null)
                {
                    for (int i = 0; i < Json.RootElement.GetArrayLength(); i++)
                        data[Json.RootElement[i].GetProperty("id").ToString()] = Json.RootElement[i].GetProperty("name").ToString();
                }
            }

            return data;
        }

        public String GetInputType(String id_profile, int input_id)
        {
            String data = "";

            String sql = "SELECT type FROM analog_inputs WHERE id=" + input_id.ToString() + " AND id_profile = '" + id_profile + "';";

            using (JsonDocument Json = RunSQLWithResults(sql))
            {
                if (Json != null)
                {
                    data = Json.RootElement[0].GetProperty("type").ToString();

                }
            }

            return data != null ? data : "";
        }

        public String[] GetInputInfo(String id_profile, int input_id)
        {
            String[] data = new String[0];

            String sql = "SELECT info FROM analog_inputs WHERE id=" + input_id.ToString() + " AND id_profile = '" + id_profile + "';";

            using (JsonDocument Json = RunSQLWithResults(sql))
            {
                if (Json != null)
                {
                    data = Json.RootElement[0].GetProperty("info").ToString().Split('|');
                }
            }

            return data;
        }

        public void SaveTheme(String theme)
        {
            Version lastVersion = Assembly.GetEntryAssembly().GetName().Version;

            String sql = "MERGE configs WITH (SERIALIZABLE) AS T USING (VALUES (1, '" + theme + "')) AS U (id, theme) " +
                            "ON U.id = T.id WHEN MATCHED THEN " +
                                "UPDATE SET theme='" + theme + "' " +
                            "WHEN NOT MATCHED THEN " +
                            "INSERT (id, theme, software_version) VALUES (1, '" + theme + "', '" + lastVersion.Major.ToString() + "." + lastVersion.Minor.ToString() + "." + lastVersion.Build.ToString() + "');";

            if (!RunSQL(sql))
            {
                // TODO: Create LOGFILE
            }
        }

        public MaterialSkinManager.Themes GetTheme (String id = "1")
        {
            String sql = "SELECT theme FROM configs WHERE id=" + id + ";";

            using (JsonDocument Json = RunSQLWithResults(sql))
            {
                if (Json != null)
                {
                    return (Json.RootElement[0].GetProperty("theme").ToString() == "dark" ? MaterialSkinManager.Themes.DARK : MaterialSkinManager.Themes.LIGHT);
                }
                else
                {
                    return MaterialSkinManager.Themes.DARK;
                }
            }
        }

        public void SaveProfile(String profileName, String productId)
        {
            String sql = "MERGE dbo.profiles WITH (SERIALIZABLE) AS T USING (VALUES ('" + profileName + "', " + productId + ")) AS U (name, id_product) " +
                            "ON U.name = T.name AND U.id_product = T.id_product WHEN MATCHED THEN " +
                                "UPDATE SET name='" + profileName + "' " +
                            "WHEN NOT MATCHED THEN " +
                            "INSERT (name, id_product) VALUES ('" + profileName + "', " + productId + ");";

            if (!RunSQL(sql))
            {
                // TODO: Create LOGFILE
            }
        }

        public void DeleteProfile(String profileName, String productId)
        {
            String sql = "DELETE FROM dbo.profiles WHERE name = '" + profileName + "' AND id_product = " + productId + ";";

            if (!RunSQL(sql))
            {
                // TODO: Create LOGFILE
            }
        }

        public String GetProfileId(String profileName, String productId)
        {
            String sql = "SELECT id FROM profiles WHERE name = '" + profileName + "' AND id_product = " + productId + ";";

            using (JsonDocument Json = RunSQLWithResults(sql))
            {
                if (Json != null)
                {
                    return Json.RootElement[0].GetProperty("id").ToString();
                }
                else
                {
                    return String.Empty;
                }
            }

        }

        public List<String> GetProfiles(String productId)
        {
            List<String> list = new List<String>();
            String sql = "SELECT name FROM profiles WHERE id_product = " + productId + " ORDER BY id ASC;";

            using (JsonDocument Json = RunSQLWithResults(sql))
            {
                if (Json != null)
                {
                    for (int i = 0; i < Json.RootElement.GetArrayLength(); i++)
                        list.Add(Json.RootElement[i].GetProperty("name").ToString()); 
                }

                return list;
            }
        }









        public String GetInputAxisOrientation(String id_profile, String id_input)
        {
            String data = "";

            using (_connection = new SqlConnection(Properties.Settings.Default.DatabaseConnectionString))
            {
                try
                {
                    _connection.Open();
                    if (_connection.State == System.Data.ConnectionState.Open)
                    {
                        using (var cmd = new SqlCommand())
                        {
                            cmd.Connection = _connection;
                            cmd.CommandText = "SELECT axis_orientation FROM analog_inputs WHERE id=" + id_input + " AND id_profile = '" + id_profile + "';";

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    data = reader["axis_orientation"].ToString();
                                }

                                reader.Close();
                                reader.Dispose();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("GetInputAxisOrientation: " + ex);
                }
                finally
                {
                }
            }

            return data;
        }
    }
}
