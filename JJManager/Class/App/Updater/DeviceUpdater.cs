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
                    // Step 1: Download available.json index
                    string indexBaseUrl = Properties.Settings.Default.DevMode
                        ? Properties.Settings.Default.DeviceUpdateUrlBase.Replace("versioncontrol", "versioncontrol_dev")
                        : Properties.Settings.Default.DeviceUpdateUrlBase;
                    byte[] indexData = wc.DownloadData(indexBaseUrl + Properties.Settings.Default.ListUpdateFileName);
                    string charset = wc.ResponseHeaders["charset"];
                    Encoding encoding = Encoding.GetEncoding(charset ?? "utf-8");
                    string indexJson = encoding.GetString(indexData);
                    JsonDocument indexDoc = JsonDocument.Parse(indexJson);

                    if (indexDoc != null && indexDoc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        // Find the device entry by matching id (using class name as lowercase)
                        string deviceId = _device.GetType().Name.ToLowerInvariant();
                        string deviceType = null;

                        Console.WriteLine($"[DeviceUpdater] Searching for device ID: '{deviceId}' in {indexDoc.RootElement.GetArrayLength()} entries");

                        for (int i = 0; i < indexDoc.RootElement.GetArrayLength(); i++)
                        {
                            var entry = indexDoc.RootElement[i];
                            if (entry.TryGetProperty("id", out JsonElement idElement) &&
                                idElement.GetString() == deviceId)
                            {
                                if (entry.TryGetProperty("type", out JsonElement typeElement))
                                {
                                    deviceType = typeElement.GetString();
                                }
                                Console.WriteLine($"[DeviceUpdater] MATCH FOUND! Type: {deviceType}");
                                break;
                            }
                        }

                        // Step 2: Download specific device JSON if found in index
                        if (!string.IsNullOrEmpty(deviceType))
                        {
                            string baseUrl = Properties.Settings.Default.DevMode
                                ? Properties.Settings.Default.DeviceUpdateUrlBase.Replace("versioncontrol", "versioncontrol_dev")
                                : Properties.Settings.Default.DeviceUpdateUrlBase;
                            string deviceUrl = $"{baseUrl}{deviceType}/{deviceId}.json";
                            Console.WriteLine($"[DeviceUpdater] Downloading device details from: {deviceUrl}");

                            byte[] deviceData = wc.DownloadData(deviceUrl);
                            charset = wc.ResponseHeaders["charset"];
                            encoding = Encoding.GetEncoding(charset ?? "utf-8");
                            string deviceJson = encoding.GetString(deviceData);
                            JsonDocument deviceDoc = JsonDocument.Parse(deviceJson);

                            if (deviceDoc != null)
                            {
                                ParseDeviceDetails(deviceDoc.RootElement);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[DeviceUpdater] WARNING: No match found for device '{deviceId}'");
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

        /// <summary>
        /// Parses the device details JSON and extracts version info and changelog
        /// </summary>
        private void ParseDeviceDetails(JsonElement root)
        {
            // New format: { "id": "...", "name": "...", "items": { "firmware": [...] } }
            if (root.TryGetProperty("items", out JsonElement itemsElement) &&
                itemsElement.TryGetProperty("firmware", out JsonElement firmwareArray))
            {
                // Get the first (latest) firmware entry
                if (firmwareArray.GetArrayLength() > 0)
                {
                    var latestFirmware = firmwareArray[0];

                    _LastVersion = new Version(latestFirmware.GetProperty("last_version").ToString());
                    _DownloadURL = latestFirmware.GetProperty("download_link").ToString();
                    _DownloadFileName = latestFirmware.GetProperty("file_name").ToString();

                    // Process changelog if it exists
                    if (latestFirmware.TryGetProperty("change_log", out JsonElement changeLogArray))
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

                    Console.WriteLine($"[DeviceUpdater] Parsed device: LastVersion={_LastVersion}, DownloadURL={_DownloadURL}");
                }
            }
        }
    }
}
