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

namespace JJManager.Class
{
    public class SoftwareUpdater
    {
        private Version _AssemblyVersion = Assembly.GetEntryAssembly().GetName().Version;
        private Version _LastVersion = Assembly.GetEntryAssembly().GetName().Version;
        private String _LastDownloadURL = "";
        private String _LastDownloadPath = "";
        private String _LastDownloadFileName = "";
        private List<string[]> _changeLog = new List<string[]>();
        private JJManager.Pages.App.UpdateAppNotification _updateAppNotification = null;
        private Thread _windowThread = null;

        public String LastVersion
        {
            get => _LastVersion.Major.ToString() + "." + _LastVersion.Minor.ToString() + "." + _LastVersion.Build.ToString();
        }

        public bool NeedToUpdate
        {
            get => (_LastVersion > _AssemblyVersion ? true : false);
        }

        public List<string[]> ChangeLog
        {
            get => _changeLog;
        }

        public SoftwareUpdater()
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    byte[] jsonData = wc.DownloadData(Properties.Settings.Default.LastVersionURL);
                    string charset = wc.ResponseHeaders["charset"];
                    Encoding encoding = Encoding.GetEncoding(charset ?? "utf-8");
                    string jsonString = encoding.GetString(jsonData);
                    JsonDocument Json = JsonDocument.Parse(jsonString);

                    if (Json != null)
                    {
                        for (int i = 0; i < Json.RootElement.GetArrayLength(); i++)
                        {
                            if (Assembly.GetEntryAssembly().GetName().Name == Json.RootElement[i].GetProperty("app_name").ToString())
                            {
                                _LastVersion = new Version(Json.RootElement[i].GetProperty("last_version").ToString());
                                _LastDownloadURL = Json.RootElement[i].GetProperty("download_link").ToString();
                                _LastDownloadPath = Path.Combine(Directory.GetCurrentDirectory(), "downloads");
                                _LastDownloadFileName = Json.RootElement[i].GetProperty("file_name").ToString();
                                
                                for (int j = 0; j < Json.RootElement[i].GetProperty("change_log").GetArrayLength(); j++)
                                {
                                    _changeLog.Add(new string[] {
                                        Json.RootElement[i].GetProperty("change_log")[j].GetProperty("img_url").ToString(),
                                        Json.RootElement[i].GetProperty("change_log")[j].GetProperty("title").ToString(),
                                        Json.RootElement[i].GetProperty("change_log")[j].GetProperty("description").ToString()
                                    }) ;
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

        public void Update()
        {
            using (WebClient wc = new WebClient())
            {
                if (!Directory.Exists(_LastDownloadPath))
                    Directory.CreateDirectory(_LastDownloadPath);

                wc.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted);
                wc.DownloadFileAsync(new Uri(_LastDownloadURL), Path.Combine(_LastDownloadPath, _LastDownloadFileName));
            }
        }

        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.ToString());
            }
            else if (e.Cancelled)
            {
                MessageBox.Show("Download da nova versão cancelada.");
            }
            else
            {
                System.Diagnostics.Process.Start(Path.Combine(_LastDownloadPath, _LastDownloadFileName));
                Environment.Exit(0);
            }
        }
    }
}
