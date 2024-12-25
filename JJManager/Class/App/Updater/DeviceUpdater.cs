using ArduinoUploader.Hardware;
using ArduinoUploader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidSharp;
using MaterialSkin.Controls;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;
using JJManager.Class.App;
using JJManager.Pages.App.Updater;
using JJManager.Class.Devices;

namespace JJManager.Class.App
{
    public class DeviceUpdater : Updater
    {
        private List<string> _comPort = null;
        private JJDevice _device = null;

        public List<string> ComPort
        {
            get => _comPort;
        }

        public JJDevice Device
        {
            get => _device;
        }

        public DeviceUpdater(Main mainForm) 
        {
            _MainForm = mainForm;
        }

        public void CheckUpdate(JJDevice device)
        {
            try
            {
                _device = device;

                SetDeviceInfo();

                using (WebClient wc = new WebClient())
                {
                    byte[] jsonData = wc.DownloadData(Properties.Settings.Default.LastVersionDeviceURL);
                    string charset = wc.ResponseHeaders["charset"];
                    Encoding encoding = Encoding.GetEncoding(charset ?? "utf-8");
                    string jsonString = encoding.GetString(jsonData);
                    JsonDocument Json = JsonDocument.Parse(jsonString);

                    if (Json != null)
                    {
                        for (int i = 0; i < Json.RootElement.GetArrayLength(); i++)
                        {
                            if (_Name == Json.RootElement[i].GetProperty("device_name").ToString())
                            {
                                SetUpdaterInfo(Json.RootElement[i]);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void SetDeviceInfo()
        {
            _ConnId = _device.ConnId;
            _Name = _device.ProductName;
            _comPort = _device.ConnPort;
            _Type = UpdaterType.Device;
            _ActualVersion = _device.Version;
        }

        private void SetUpdaterInfo(JsonElement json)
        {
            _Name = json.GetProperty("device_name").ToString();
            _LastVersion = new Version(json.GetProperty("last_version").ToString());
            _DownloadURL = json.GetProperty("download_link").ToString();
            _DownloadFileName = json.GetProperty("file_name").ToString();

            for (int i = 0; i < json.GetArrayLength(); i++)
            {
                _ChangeLog.Add(new string[] {
                    json[i].GetProperty("title").ToString(),
                    json[i].GetProperty("description").ToString()
                });
            }
        }
    }
}
