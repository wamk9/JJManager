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

namespace JJManager.Class.App
{
    public class DeviceUpdater : Updater
    {
        private String _ComPort = "";
        private Device _device = null;

        public String ComPort
        {
            get => _ComPort;
        }

        public Device Device
        {
            get => _device;
        }

        public DeviceUpdater(Main mainForm) 
        {
            _MainForm = mainForm;
        }

        public void CheckUpdate(JJManager.Class.Device device)
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
            _ComPort = _device.ConnPort;
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

















        public static void GetDevicesCanUpdated(ref List<Device> devices)
        {
            List<HidDevice> hidDevicesList = DeviceList.Local.GetHidDevices(0x2341).ToList();
            List<JJManager.Class.Device> jjDevicesList = new List<JJManager.Class.Device>();
            List<JJManager.Class.Device> actualDevices = devices;
            JJManager.Class.Device deviceTmp = null;

            String[] jjHidNames =
            {
                "Streamdeck JJSD-01",
                "Mixer de Áudio JJM-01",
                "ButtonBox JJB-01 V2", // Gerenciamento de Leds
                "ButtonBox JJBP-06", // Gerenciamento de Leds
                "ButtonBox JJB-999" // Gerenciamento de Leds
            };

            hidDevicesList.RemoveAll(deviceFound => !jjHidNames.Any(hidName => deviceFound.GetProductName() == hidName));
            hidDevicesList.RemoveAll(deviceFound => deviceFound.GetReportDescriptor().Reports[0].ReportID == (byte)0xFF);

            bool onDevicesList = false;

            hidDevicesList.ForEach(hidDevice =>
            {
                onDevicesList = false;
                deviceTmp = null;

                actualDevices.ForEach(device =>
                {
                    if (hidDevice.DevicePath == device.HidDevice.DevicePath)
                    {
                        deviceTmp = device;
                        onDevicesList = true;
                    }
                });

                if (!onDevicesList)
                {
                    jjDevicesList.Add(new JJManager.Class.Device(hidDevice));
                }
                else
                {
                    jjDevicesList.Add(deviceTmp);

                }
            });

            devices = jjDevicesList;
        }
    }

}
