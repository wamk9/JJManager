using JJMixer_WForms.Pages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JJMixer_WForms.Class
{
    internal class JJMixerDbConnection
    {
        string _dbPath;
        string _stringConnection;
        SqlConnection _connection;

        public JJMixerDbConnection()
        {
            _dbPath = Path.Combine(Directory.GetCurrentDirectory(), "JJMixerDB.mdf");
            _stringConnection = string.Format(@"Server=(localdb)\mssqllocaldb; Integrated Security=true; AttachDbFileName={0};", _dbPath);
        }

      

        public void SaveInputData(int input_id, String input_name, String input_type, String input_info, string invert_axis, string model)
        {
            using (_connection = new SqlConnection(_stringConnection))
            {
                _connection.Open();
                if (_connection.State == ConnectionState.Open)
                {
                    using (var cmd = new SqlCommand())
                    {
                        cmd.Connection = _connection;
                        cmd.CommandText = "MERGE inputs WITH (SERIALIZABLE) AS T USING (VALUES (" + input_id.ToString() + ",'" + input_name + "', '" + input_type + "', '" + input_info + "', '" + invert_axis + "', '" + model + "')) AS U (id, name, type, info, axis_orientation, model) " +
                            "ON U.id = T.id AND U.model = T.model WHEN MATCHED THEN " +
                                "UPDATE SET name='" + input_name + "', type='" + input_type + "', info='" + input_info + "', axis_orientation='" + invert_axis + "' " +
                            "WHEN NOT MATCHED THEN " +
                            "INSERT (id, name, type, info, axis_orientation, model) VALUES (" + input_id.ToString() + ", '" + input_name + "', '" + input_type + "', '" + input_info + "', '" + invert_axis + "' , '" + model + "');";
                        using (var reader = cmd.ExecuteReader())
                        {
                            /* while (reader.Read())
                             {
                                 MessageBox.Show(reader["input_id"] + "|" + reader["input_name"] + "|" + reader["input_type"]);
                             }*/
                        }
                    }
                }
                _connection.Close();
            }
        }

        public String GetInputAxisOrientation(int input_id, string model)
        {
            String data = "";

            using (_connection = new SqlConnection(_stringConnection))
            {
                _connection.Open();
                if (_connection.State == System.Data.ConnectionState.Open)
                {
                    using (var cmd = new SqlCommand())
                    {
                        cmd.Connection = _connection;
                        cmd.CommandText = "SELECT axis_orientation FROM inputs WHERE id=" + input_id.ToString() + " AND model = '" + model + "';";

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data = reader["axis_orientation"].ToString();
                            }
                        }
                    }
                }
                _connection.Close();
            }

            return data;
        }

        public Dictionary<string, string> GetInputData(int input_id, string model)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();

            using (_connection = new SqlConnection(_stringConnection))
            {
                _connection.Open();
                if (_connection.State == System.Data.ConnectionState.Open)
                {
                    using (var cmd = new SqlCommand())
                    {
                        cmd.Connection = _connection;
                        cmd.CommandText = "SELECT name, type, info, axis_orientation FROM inputs WHERE id=" + input_id.ToString() + " AND model = '" + model + "';";

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data["input_name"] = reader["name"].ToString();
                                data["input_type"] = reader["type"].ToString();
                                data["input_info"] = reader["info"].ToString();
                                data["axis_orientation"] = reader["axis_orientation"].ToString();
                            }
                        }
                    }
                }
                _connection.Close();
            }

            return data;
        }

        public SortedDictionary<string, string> GetAllInputName(String Model)
        {
            SortedDictionary<string, string> data = new SortedDictionary<string, string>();

            using (_connection = new SqlConnection(_stringConnection))
            {
                _connection.Open();
                if (_connection.State == System.Data.ConnectionState.Open)
                {
                    using (var cmd = new SqlCommand())
                    {
                        cmd.Connection = _connection;
                        cmd.CommandText = "SELECT id, name FROM inputs WHERE model='" + Model + "' ORDER BY id;";

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data[reader["id"].ToString()] = reader["name"].ToString();
                            }
                        }
                    }
                }
                _connection.Close();
            }

            return data;
        }

        public String[] GetInputInfo(int input_id, string model)
        {
            String[] data = null;

            using (_connection = new SqlConnection(_stringConnection))
            {
                _connection.Open();
                if (_connection.State == System.Data.ConnectionState.Open)
                {
                    using (var cmd = new SqlCommand())
                    {
                        cmd.Connection = _connection;
                        cmd.CommandText = "SELECT info FROM inputs WHERE id=" + input_id.ToString() + " AND model = '" + model + "';";

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data = reader["info"].ToString().Split('|');
                            }
                        }
                    }
                }
                _connection.Close();
            }

            return data;
        }

        public String GetInputType(int input_id, string model)
        {
            String data = null;

            using (_connection = new SqlConnection(_stringConnection))
            {
                _connection.Open();
                if (_connection.State == System.Data.ConnectionState.Open)
                {
                    using (var cmd = new SqlCommand())
                    {
                        cmd.Connection = _connection;
                        cmd.CommandText = "SELECT type FROM inputs WHERE id=" + input_id.ToString() + " AND model = '" + model + "';";

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data = reader["type"].ToString();
                            }
                        }
                    }
                }
                _connection.Close();
            }

            return data;
        }


    }
}
