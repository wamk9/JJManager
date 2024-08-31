using JJManager.Class;
using JJManager.Class.App.Input;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace JJManager.MigrateCommands
{
    public class _1_2_4
    {
        public static void ExecuteMigration()
        {
            DatabaseConnection dbConnection = new DatabaseConnection();
            string id = "";
            string name = "";
            string data = "";
            string mode = "";
            string type = "";
            string id_profile = "";
            JsonArray valuesJson = new JsonArray();
            JsonObject dataJson = new JsonObject();

            string sql = "SELECT id, name, type, info, inverted_axis, id_profile FROM dbo.analog_inputs;";

            using (JsonDocument json = dbConnection.RunSQLWithResults(sql))
            {
                if (json != null)
                {
                    foreach (var item in json.RootElement.EnumerateArray())
                    {
                        dataJson.Clear();
                        valuesJson.Clear();

                        foreach (string individualInfo in item.GetProperty("info").GetString().Split('|'))
                        {
                            valuesJson.Add(individualInfo);
                        }

                        dataJson.Add("toManage", valuesJson);
                        
                        if (item.GetProperty("type").GetString() == "app")
                        {
                            dataJson.Add("audioMode", "application");
                        }
                        else if (item.GetProperty("type").GetString() == "device")
                        {
                            dataJson.Add("audioMode", "deviceplayback");
                        }
                        else if (item.GetProperty("type").GetString() == "nothing")
                        {
                            dataJson.Add("audioMode", "none");
                        }

                        id = (int.Parse(item.GetProperty("id").GetString()) - 1).ToString();
                        name = item.GetProperty("name").GetString();
                        data = dataJson.ToJsonString();
                        mode = "audiocontroller";
                        type = "analog";
                        id_profile = item.GetProperty("id_profile").GetString();

                        sql = $"INSERT INTO dbo.device_inputs (id, name, data, type, mode, id_profile) VALUES ('{id}', '{name}', '{data}', '{type}', '{mode}', '{id_profile}');";

                        dbConnection.RunSQL(sql);
                    }
                }
            }
        }
    }
}
