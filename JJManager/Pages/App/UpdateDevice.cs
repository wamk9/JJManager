using DeviceProgramming.FileFormat;
using HidSharp;
using JJManager.Class;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using MaterialSkin.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SharpDX.Utilities;

namespace JJManager.Pages
{
    public partial class UpdateDevice : MaterialForm
    {
        public UpdateDevice()
        {
            InitializeComponent();
            // Get all serial ports
            string[] serialPorts = SerialPort.GetPortNames();

            Console.WriteLine("List of Serial Ports:");
            foreach (string port in serialPorts)
            {
                Console.WriteLine(port);
            }

            // Registry path to the Enum key
            string registryPath = @"SYSTEM\CurrentControlSet\Enum";

            // Open the registry key
            using (RegistryKey enumKey = Registry.LocalMachine.OpenSubKey(registryPath))
            {
                if (enumKey != null)
                {
                    foreach (string subKeyName in enumKey.GetSubKeyNames())
                    {
                        using (RegistryKey subKey = enumKey.OpenSubKey(subKeyName))
                        {
                            if (subKey != null)
                            {
                                foreach (string deviceKeyName in subKey.GetSubKeyNames())
                                {
                                    using (RegistryKey deviceKey = subKey.OpenSubKey(deviceKeyName))
                                    {
                                        if (deviceKey != null)
                                        {
                                            foreach (string instanceKeyName in deviceKey.GetSubKeyNames())
                                            {
                                                using (RegistryKey instanceKey = deviceKey.OpenSubKey(instanceKeyName))
                                                {
                                                    if (instanceKey != null)
                                                    {
                                                        using (RegistryKey deviceParametersKey = instanceKey.OpenSubKey("Device Parameters"))
                                                        {
                                                            if (deviceParametersKey != null)
                                                            {
                                                                string portName = deviceParametersKey.GetValue("PortName") as string;
                                                                string busReportedDeviceDesc = deviceParametersKey.GetValue("BusReportedDeviceDesc") as string;

                                                                if (!string.IsNullOrEmpty(portName) && !string.IsNullOrEmpty(busReportedDeviceDesc))
                                                                {
                                                                    Console.WriteLine($"Port: {portName}, Device Description: {busReportedDeviceDesc}");
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        
        //OpenFirmware.ShowDialog();
    }

        private void OpenFirmware_FileOk(object sender, CancelEventArgs e)
        {
            UsbRegDeviceList devicelist = new UsbRegDeviceList();

            List<LibUsbDfu.Device> teste =  LibUsbDfu.Device.OpenAll(devicelist);

            MessageBox.Show (teste[0].DfuDescriptor.DfuVersion.Build.ToString());





            Console.WriteLine("LibOpenBLT version number: {0}", OpenBLT.Lib.Version.GetNumber());

            // ------------ Initialization ---------------------------------------------- 
            OpenBLT.Lib.Session.SessionSettingsXcpV10 sessionSettings;
            sessionSettings.timeoutT1 = 1000;
            sessionSettings.timeoutT3 = 2000;
            sessionSettings.timeoutT4 = 10000;
            sessionSettings.timeoutT5 = 1000;
            sessionSettings.timeoutT6 = 50;
            sessionSettings.timeoutT7 = 2000;
            sessionSettings.seedKeyFile = "";
            sessionSettings.connectMode = 0;

            OpenBLT.Lib.Session.TransportSettingsXcpV10Rs232 transportSettingsRs232;
            transportSettingsRs232.portName = "COM15";
            transportSettingsRs232.baudrate = 9600;

            OpenBLT.Lib.Session.Init(sessionSettings, transportSettingsRs232);
            OpenBLT.Lib.Firmware.Init(OpenBLT.Lib.Firmware.FIRMWARE_PARSER_SRECORD);


            OpenBLT.Lib.Firmware.Init(OpenBLT.Lib.Firmware.FIRMWARE_PARSER_SRECORD);
            if (OpenBLT.Lib.Firmware.LoadFromFile(OpenFirmware.FileName, 0) != OpenBLT.Lib.RESULT_OK)
            {
                Console.WriteLine("Could not load firmware data from the S-record.");
            }

            // ------------ Connect to target ------------------------------------------- 
            if (OpenBLT.Lib.Session.Start() != OpenBLT.Lib.RESULT_OK)
            {
                Console.WriteLine("Could not connect to the target.");
            }

            // ------------ Erase flash ------------------------------------------------- 
            for (UInt32 segmentIdx = 0; segmentIdx < OpenBLT.Lib.Firmware.GetSegmentCount(); segmentIdx++)
            {
                var segment = OpenBLT.Lib.Firmware.GetSegment(segmentIdx);
                if (OpenBLT.Lib.Session.ClearMemory(segment.address, segment.data.Length) != OpenBLT.Lib.RESULT_OK)
                {
                    Console.WriteLine("Could not erase memory.");
                }
            }

            // ------------ Program flash -----------------------------------------------
            for (UInt32 segmentIdx = 0; segmentIdx < OpenBLT.Lib.Firmware.GetSegmentCount(); segmentIdx++)
            {
                var segment = OpenBLT.Lib.Firmware.GetSegment(segmentIdx);
                if (OpenBLT.Lib.Session.WriteData(segment.address, segment.data) != OpenBLT.Lib.RESULT_OK)
                {
                    Console.WriteLine("Could not program memory.");
                }
            }

            // ------------ Disconnect from target --------------------------------------
            OpenBLT.Lib.Session.Stop();

            // ------------ Termination -------------------------------------------------
            OpenBLT.Lib.Firmware.Terminate();
            OpenBLT.Lib.Session.Terminate();

            /*foreach (UsbRegistry device in LibUsbDfu.Device.OpenAll(allDevices))
            {
                MessageBox.Show(device.FullName);
            }

            STBootLib.STBoot sTBoot = new STBootLib.STBoot();
            */

            /*
            sTBoot.Version
            foreach (var teste in sTBoot.Commands)
            {
                MessageBox.Show(teste.ToString());
            }
            */
            //sTBoot.Open("COM15", 115200);
            //sTBoot.WriteMemory(0x08000000,)
            //UsbRegistry usbRegistry;

            //UsbDevice usbDevice;

            //usbRegistry.


            MessageBox.Show("fim");

            //LibUsbDfu.Device.TryOpen()
            //FirmwareUpdater firmwareUpdater = new FirmwareUpdater(OpenFirmware.FileName, "COM6", "Mixer de Áudio JJM-01");
        }
    }
}
