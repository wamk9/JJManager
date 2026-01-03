using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace JJManager.Class.App
{
    public class PluginUpdater : Updater
    {
        private bool _InExecution = false;

        public bool InExecution
        {
            get => _InExecution;
        }

        public PluginUpdater(string pluginName)
        {
            try
            {
                if (!SetPluginInfo(pluginName))
                {
                    return;
                }

                using (WebClient wc = new WebClient())
                {
                    
                    byte[] jsonData = wc.DownloadData(Properties.Settings.Default.LastVersionPluginURL);
                    string charset = wc.ResponseHeaders["charset"];
                    Encoding encoding = Encoding.GetEncoding(charset ?? "utf-8");
                    string jsonString = encoding.GetString(jsonData);
                    JsonDocument Json = JsonDocument.Parse(jsonString);

                    if (Json != null)
                    {
                        for (int i = 0; i < Json.RootElement.GetArrayLength(); i++)
                        {
                            if (pluginName == Json.RootElement[i].GetProperty("plugin_name").ToString())
                            {
                                _LastVersion = new Version(Json.RootElement[i].GetProperty("last_version").ToString());
                                _DownloadURL = Json.RootElement[i].GetProperty("download_link").ToString();
                                _DownloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JohnJohn3D", "JJManager", "downloads");
                                _DownloadFileName = Json.RootElement[i].GetProperty("file_name").ToString();

                                for (int j = 0; j < Json.RootElement[i].GetProperty("change_log").GetArrayLength(); j++)
                                {
                                    _ChangeLog.Add(new string[] {
                                        Json.RootElement[i].GetProperty("change_log")[j].GetProperty("title").ToString(),
                                        Json.RootElement[i].GetProperty("change_log")[j].GetProperty("description").ToString()
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private bool SetPluginInfo(string pluginName)
        {
            _ConnId = pluginName.GetHashCode().ToString();
            _Name = pluginName;
            _Type = UpdaterType.Plugin;

            SimHubWebsocket simHubWebsocket = new SimHubWebsocket(2920, "JJMANAGER_VERSION_CHECK");

            if (simHubWebsocket != null && !simHubWebsocket.IsConnected)
            {
                simHubWebsocket.StartCommunication(false);
            }

            int attemptsToGetVersion = 0;

            while(simHubWebsocket.IsConnected)
            {
                var (success, result) = simHubWebsocket.RequestMessage("{\"request\": [\"PluginVersion\"]}");

                JsonObject jsonResult = result;

                if (!success && attemptsToGetVersion > 3)
                {
                    simHubWebsocket.StopCommunication();
                }

                _InExecution = (success && jsonResult != null && jsonResult.ContainsKey("status") && jsonResult["status"].ToString() == "online");
                
                if (success && jsonResult != null &&
                    (jsonResult.ContainsKey("status") && jsonResult["status"].ToString() == "online") &&
                    (jsonResult.ContainsKey("data") && jsonResult["data"] is JsonObject dataObject) &&
                    dataObject.ContainsKey("PluginVersion"))
                {
                    _ActualVersion = new Version(dataObject["PluginVersion"].ToString());
                    simHubWebsocket.StopCommunication();
                }
            }

            return _InExecution;
        }
    }
}
