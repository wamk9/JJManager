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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JJManager.Class
{
    internal class SoftwareUpdater
    {
        private Version _AssemblyVersion = Assembly.GetEntryAssembly().GetName().Version;
        private Version _LastVersion = Assembly.GetEntryAssembly().GetName().Version;
        private String _LastDownloadURL = "";
        private String _LastDownloadPath = "";
        private String _LastDownloadFileName = "";


        public String LastVersion
        {
            get => _LastVersion.Major.ToString() + "." + _LastVersion.Minor.ToString() + "." + _LastVersion.Build.ToString();
        }

        public bool NeedToUpdate
        {
            get => (_LastVersion > _AssemblyVersion ? true : false);
        }

        public SoftwareUpdater()
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    JsonDocument Json = JsonDocument.Parse(wc.DownloadString(Properties.Settings.Default.LastVersionURL));

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
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

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
