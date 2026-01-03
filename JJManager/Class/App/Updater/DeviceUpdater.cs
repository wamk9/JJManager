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
using JJManager.Class.Devices.Connections;

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
                        Console.WriteLine($"[DeviceUpdater] Searching for device: '{_Name}' in {Json.RootElement.GetArrayLength()} entries");

                        for (int i = 0; i < Json.RootElement.GetArrayLength(); i++)
                        {
                            string deviceNameFromJson = Json.RootElement[i].GetProperty("device_name").ToString();
                            Console.WriteLine($"[DeviceUpdater] Comparing '{_Name}' with '{deviceNameFromJson}'");

                            if (_Name == deviceNameFromJson)
                            {
                                Console.WriteLine($"[DeviceUpdater] MATCH FOUND! Setting updater info...");
                                SetUpdaterInfo(Json.RootElement[i]);
                                break;
                            }
                        }

                        if (_LastVersion == null)
                        {
                            Console.WriteLine($"[DeviceUpdater] WARNING: No match found for device '{_Name}'");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeviceUpdater] ERROR in CheckUpdate: {ex.Message}");
                Console.WriteLine($"[DeviceUpdater] Stack trace: {ex.StackTrace}");
            }
        }

        private void SetDeviceInfo()
        {
            Task.Run(async () =>
            {
                if (_device is HID)
                {
                    await ((HID)_device).GetFirmwareVersion();
                }

                _ConnId = _device.ConnId;
                _Name = _device.ProductName;
                _comPort = _device.ConnPort;
                _Type = UpdaterType.Device;
                _ActualVersion = _device.Version;
            }).Wait(10000);
        }

        private void SetUpdaterInfo(JsonElement json)
        {
            _Name = json.GetProperty("device_name").ToString();
            _LastVersion = new Version(json.GetProperty("last_version").ToString());
            _DownloadURL = json.GetProperty("download_link").ToString();
            _DownloadFileName = json.GetProperty("file_name").ToString();

            // Process changelog if it exists
            if (json.TryGetProperty("changelog", out JsonElement changelogElement) &&
                changelogElement.ValueKind == JsonValueKind.Array)
            {
                for (int i = 0; i < changelogElement.GetArrayLength(); i++)
                {
                    _ChangeLog.Add(new string[] {
                        changelogElement[i].GetProperty("title").ToString(),
                        changelogElement[i].GetProperty("description").ToString()
                    });
                }
            }
        }
    }
}
