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

            foreach (JsonObject json in dbConnection.RunSQLWithResults(sql))
            {
                if (json != null)
                {
                    dataJson.Clear();
                    valuesJson.Clear();

                    foreach (string individualInfo in json["info"].GetValue<string>().Split('|'))
                    {
                        valuesJson.Add(individualInfo);
                    }

                    dataJson.Add("toManage", valuesJson);
                        
                    if (json["type"].GetValue<string>() == "app")
                    {
                        dataJson.Add("audioMode", "application");
                    }
                    else if (json["type"].GetValue<string>() == "device")
                    {
                        dataJson.Add("audioMode", "deviceplayback");
                    }
                    else if (json["type"].GetValue<string>() == "nothing")
                    {
                        dataJson.Add("audioMode", "none");
                    }

                    id = (int.Parse(json["id"].GetValue<string>()) - 1).ToString();
                    name = json["name"].GetValue<string>();
                    data = dataJson.ToJsonString();
                    mode = "audiocontroller";
                    type = "analog";
                    id_profile = json["id_profile"].GetValue<string>();

                    sql = $"INSERT INTO dbo.device_inputs (id, name, data, type, mode, id_profile) VALUES ('{id}', '{name}', '{data}', '{type}', '{mode}', '{id_profile}');";

                    dbConnection.RunSQL(sql);
                }
            }
        }
    }
}
