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

        // Map plugin display names to their JSON ids
        private static readonly Dictionary<string, string> PluginIdMap = new Dictionary<string, string>
        {
            { "JJManager Sync (Integração SimHub)", "jjmanagersync_simhub" }
        };

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
                    // Step 1: Download available.json index
                    string indexBaseUrl = Properties.Settings.Default.DevMode
                        ? Properties.Settings.Default.SoftwareUpdateUrlBase.Replace("versioncontrol", "versioncontrol_dev")
                        : Properties.Settings.Default.SoftwareUpdateUrlBase;
                    byte[] indexData = wc.DownloadData(indexBaseUrl + Properties.Settings.Default.ListUpdateFileName);
                    string charset = wc.ResponseHeaders["charset"];
                    Encoding encoding = Encoding.GetEncoding(charset ?? "utf-8");
                    string indexJson = encoding.GetString(indexData);
                    JsonDocument indexDoc = JsonDocument.Parse(indexJson);

                    if (indexDoc != null && indexDoc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        // Get the plugin id from the map
                        string pluginId = PluginIdMap.ContainsKey(pluginName) ? PluginIdMap[pluginName] : pluginName.ToLowerInvariant().Replace(" ", "_");
                        string pluginType = null;

                        for (int i = 0; i < indexDoc.RootElement.GetArrayLength(); i++)
                        {
                            var entry = indexDoc.RootElement[i];
                            if (entry.TryGetProperty("id", out JsonElement idElement) &&
                                idElement.GetString() == pluginId)
                            {
                                if (entry.TryGetProperty("type", out JsonElement typeElement))
                                {
                                    pluginType = typeElement.GetString();
                                }
                                break;
                            }
                        }

                        // Step 2: Download specific plugin JSON if found in index
                        if (!string.IsNullOrEmpty(pluginType))
                        {
                            string baseUrl = Properties.Settings.Default.DevMode
                                ? Properties.Settings.Default.SoftwareUpdateUrlBase.Replace("versioncontrol", "versioncontrol_dev")
                                : Properties.Settings.Default.SoftwareUpdateUrlBase;
                            string pluginUrl = $"{baseUrl}{pluginType}/{pluginId}.json";
                            byte[] pluginData = wc.DownloadData(pluginUrl);
                            charset = wc.ResponseHeaders["charset"];
                            encoding = Encoding.GetEncoding(charset ?? "utf-8");
                            string pluginJson = encoding.GetString(pluginData);
                            JsonDocument pluginDoc = JsonDocument.Parse(pluginJson);

                            if (pluginDoc != null)
                            {
                                ParsePluginDetails(pluginDoc.RootElement, pluginName);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Parses the plugin details JSON and extracts version info and changelog
        /// </summary>
        private void ParsePluginDetails(JsonElement root, string pluginName)
        {
            // New format: { "id": "...", "name": "...", "items": { "software": [...] } }
            if (root.TryGetProperty("items", out JsonElement itemsElement) &&
                itemsElement.TryGetProperty("software", out JsonElement softwareArray))
            {
                // Get the first (latest) software entry
                if (softwareArray.GetArrayLength() > 0)
                {
                    var latestSoftware = softwareArray[0];

                    _LastVersion = new Version(latestSoftware.GetProperty("last_version").ToString());
                    _DownloadURL = latestSoftware.GetProperty("download_link").ToString();
                    _DownloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JohnJohn3D", "JJManager", "downloads");
                    _DownloadFileName = latestSoftware.GetProperty("file_name").ToString();

                    if (latestSoftware.TryGetProperty("change_log", out JsonElement changeLogArray))
                    {
                        for (int j = 0; j < changeLogArray.GetArrayLength(); j++)
                        {
                            var logEntry = changeLogArray[j];
                            // Support both formats: with img_url and without
                            string imgUrl = logEntry.TryGetProperty("img_url", out JsonElement imgElement) ? imgElement.GetString() : "#";
                            string changeType = logEntry.TryGetProperty("type", out JsonElement typeElement) ? typeElement.GetString() : "";

                            _ChangeLog.Add(new string[] {
                                imgUrl,
                                logEntry.GetProperty("title").ToString(),
                                logEntry.GetProperty("description").ToString(),
                                changeType
                            });
                        }
                    }
                }
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
