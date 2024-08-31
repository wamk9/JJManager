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
                    byte[] jsonData = wc.DownloadData(Properties.Settings.Default.LastVersionAppURL);
                    string charset = wc.ResponseHeaders["charset"];
                    Encoding encoding = Encoding.GetEncoding(charset ?? "utf-8");
                    string jsonString = encoding.GetString(jsonData);
                    JsonDocument Json = JsonDocument.Parse(jsonString);

                    if (Json != null)
                    {
                        for (int i = 0; i < Json.RootElement.GetArrayLength(); i++)
                        {
                            if (appName == Json.RootElement[i].GetProperty("app_name").ToString())
                            {
                                _LastVersion = new Version(Json.RootElement[i].GetProperty("last_version").ToString());
                                _DownloadURL = Json.RootElement[i].GetProperty("download_link").ToString();
                                _DownloadFileName = Json.RootElement[i].GetProperty("file_name").ToString();

                                for (int j = 0; j < Json.RootElement[i].GetProperty("change_log").GetArrayLength(); j++)
                                {
                                    _ChangeLog.Add(new string[] {
                                        Json.RootElement[i].GetProperty("change_log")[j].GetProperty("img_url").ToString(),
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
