using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JJManager.Class.App
{
    public class SoftwareUpdater : Updater
    {
        private JJManager.Pages.App.UpdateAppNotification _updateAppNotification = null;
        private Thread _windowThread = null;

        public SoftwareUpdater(string appName)
        {
            try
            {
                SetProgramInfo(appName);

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
                        // Find the app entry by matching id (lowercase app name)
                        string appId = appName.ToLowerInvariant();
                        string appType = null;

                        for (int i = 0; i < indexDoc.RootElement.GetArrayLength(); i++)
                        {
                            var entry = indexDoc.RootElement[i];
                            if (entry.TryGetProperty("id", out JsonElement idElement) &&
                                idElement.GetString() == appId)
                            {
                                if (entry.TryGetProperty("type", out JsonElement typeElement))
                                {
                                    appType = typeElement.GetString();
                                }
                                break;
                            }
                        }

                        // Step 2: Download specific app JSON if found in index
                        if (!string.IsNullOrEmpty(appType))
                        {
                            string baseUrl = Properties.Settings.Default.DevMode
                                ? Properties.Settings.Default.SoftwareUpdateUrlBase.Replace("versioncontrol", "versioncontrol_dev")
                                : Properties.Settings.Default.SoftwareUpdateUrlBase;
                            string appUrl = $"{baseUrl}{appType}/{appId}.json";
                            byte[] appData = wc.DownloadData(appUrl);
                            charset = wc.ResponseHeaders["charset"];
                            encoding = Encoding.GetEncoding(charset ?? "utf-8");
                            string appJson = encoding.GetString(appData);
                            JsonDocument appDoc = JsonDocument.Parse(appJson);

                            if (appDoc != null)
                            {
                                ParseAppDetails(appDoc.RootElement, appName);
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
        /// Parses the app details JSON and extracts version info and changelog
        /// </summary>
        private void ParseAppDetails(JsonElement root, string appName)
        {
            // New format: { "id": "...", "name": "...", "items": { "software": [...] } }
            if (root.TryGetProperty("name", out JsonElement nameElement) &&
                nameElement.GetString() == appName &&
                root.TryGetProperty("items", out JsonElement itemsElement) &&
                itemsElement.TryGetProperty("software", out JsonElement softwareArray))
            {
                // Get the first (latest) software entry
                if (softwareArray.GetArrayLength() > 0)
                {
                    var latestSoftware = softwareArray[0];

                    _LastVersion = new Version(latestSoftware.GetProperty("last_version").ToString());
                    _DownloadURL = latestSoftware.GetProperty("download_link").ToString();
                    _DownloadFileName = latestSoftware.GetProperty("file_name").ToString();

                    if (latestSoftware.TryGetProperty("change_log", out JsonElement changeLogArray))
                    {
                        for (int j = 0; j < changeLogArray.GetArrayLength(); j++)
                        {
                            var logEntry = changeLogArray[j];
                            string changeType = logEntry.TryGetProperty("type", out JsonElement typeElement)
                                ? typeElement.GetString()
                                : "";

                            _ChangeLog.Add(new string[] {
                                logEntry.GetProperty("img_url").ToString(),
                                logEntry.GetProperty("title").ToString(),
                                logEntry.GetProperty("description").ToString(),
                                changeType
                            });
                        }
                    }
                }
            }
        }

        private void SetProgramInfo(string appName)
        {
            if (appName == Assembly.GetEntryAssembly().GetName().Name)
            {
                _ConnId = Assembly.GetEntryAssembly().GetName().Name.GetHashCode().ToString();
                _Name = Assembly.GetEntryAssembly().GetName().Name;
                _Type = UpdaterType.Program;
                _ActualVersion = Assembly.GetEntryAssembly().GetName().Version;
            }
            else
            {
                // Do when we has another program
                // Tip: Do a json file with version, name, and what is important to receive here...
            }
        }


        public void ShowNotificationForm(MaterialForm parent)
        {
            _windowThread = new Thread(new ParameterizedThreadStart(CreateFormProcess));
            _windowThread.TrySetApartmentState(ApartmentState.STA);
            _windowThread.Name = "Notification_Form";
            _windowThread.IsBackground = true;
            _windowThread.Start(parent);
        }

        public void CloseNotificationForm()
        {
            if (_updateAppNotification != null)
            {
                _updateAppNotification.BeginInvoke(new System.Threading.ThreadStart(_updateAppNotification.CloseUpdateAppNotification));
                _updateAppNotification = null;
                _windowThread = null;
            }
        }

        private void CreateFormProcess()
        {
            _updateAppNotification = new JJManager.Pages.App.UpdateAppNotification(this);
            if (_updateAppNotification.InvokeRequired)
            {
                _updateAppNotification.BeginInvoke((MethodInvoker)delegate
                {
                    _updateAppNotification = new JJManager.Pages.App.UpdateAppNotification(this);
                    _updateAppNotification.Show();
                });
            }
            else
            {
                _updateAppNotification = new JJManager.Pages.App.UpdateAppNotification(this);
                _updateAppNotification.Show();
            }
        }

        private void CreateFormProcess(object parent)
        {
            MaterialForm windowParent = parent as MaterialForm;
            if (windowParent.InvokeRequired)
            {
                windowParent.BeginInvoke((MethodInvoker)delegate
                {
                    _updateAppNotification = new JJManager.Pages.App.UpdateAppNotification(windowParent, this);
                    _updateAppNotification.Show();
                });
            }
            else
            {
                _updateAppNotification = new JJManager.Pages.App.UpdateAppNotification(windowParent, this);
                _updateAppNotification.Show();
            }
        }
    }
}
