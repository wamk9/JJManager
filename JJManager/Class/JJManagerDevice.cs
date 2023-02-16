using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using SharpDX.DirectInput;
using HidSharp.Reports;
using HidSharp;
using System.Threading;
using System.Windows.Forms;
using System.Collections;

namespace JJManager.Class
{
    public static class JJManagerDevice
    {
        /*public static List<String> UpdateCOMDevices()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM WIN32_SerialPort"))
            {
                string[] portnames = SerialPort.GetPortNames();
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
                var tList = (from n in portnames join p in ports on n equals p["DeviceID"].ToString() select n).ToList();

                List<String> ComPort = new List<String>();

                foreach (string s in tList)
                {
                    if (!ComPort.Contains(s))
                        ComPort.Add(s);
                }

                return ComPort;
            }
        }*/

        public static Joystick GetJoystickDeviceByHash(String hash)
        {
            Joystick joystick = null;

            // Initialize DirectInput
            var directInput = new DirectInput();

            // Find a Joystick Guid
            var joystickGuid = Guid.Empty;

            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
            {
                if (deviceInstance.InstanceGuid.GetHashCode().ToString() == hash)
                {
                    joystickGuid = deviceInstance.InstanceGuid;
                    break;
                }
            }

            // If Gamepad not found, look for a Joystick
            if (joystickGuid == Guid.Empty)
            {
                foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                {
                    if (deviceInstance.InstanceGuid.GetHashCode().ToString() == hash)
                    {
                        joystickGuid = deviceInstance.InstanceGuid;
                        break;
                    }
                }
            }

            if (joystickGuid != Guid.Empty)
            {
                joystick = new Joystick(directInput, joystickGuid);
                joystick.Properties.BufferSize = 128;
                joystick.Acquire();
            }

            return joystick;
        }

        public static HidDevice GetHIDDeviceByHash(String hash)
        {
            HidDevice device = null;

            var list = DeviceList.Local;
            list.Changed += (sender, e) => Console.WriteLine("Device list changed.");

            var hidDeviceList = list.GetHidDevices().ToArray();

            foreach (HidDevice dev in hidDeviceList)
            {
                if (dev.DevicePath.GetHashCode().ToString() != hash)
                {
                    continue;
                }
                else
                {
                    device = dev;
                    break;
                }
            }
            return device;
        }

        public static List<String> UpdateHIDDevices()
        {
            List<String> HIDList = new List<String>();

            // Initialize DirectInput
            var directInput = new DirectInput();

            // Find a Joystick Guid
            var joystickGuid = Guid.Empty;

            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
            {
                if (deviceInstance.ProductName.Contains("ButtonBox"))
                {
                    joystickGuid = deviceInstance.InstanceGuid;
                    HIDList.Add(deviceInstance.InstanceName + " (" + joystickGuid.GetHashCode() + ")");
                }
            }

            // If Gamepad not found, look for a Joystick
            if (joystickGuid == Guid.Empty)
            {
                foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                {
                    if (deviceInstance.ProductName.Contains("ButtonBox"))
                    {
                        joystickGuid = deviceInstance.InstanceGuid;
                        HIDList.Add(deviceInstance.InstanceName + " (" + joystickGuid.GetHashCode() + ")");
                    }
                }
            }

            var list = DeviceList.Local;
            list.Changed += (sender, e) => Console.WriteLine("Device list changed.");

            var hidDeviceList = list.GetHidDevices().ToArray();

            foreach (HidDevice dev in hidDeviceList)
            {
                if (dev.VendorID != 0x2341)
                    continue;

                if (dev.GetProductName().Contains("ButtonBox"))
                    continue;

                HIDList.Add(dev.GetProductName() + " (" + dev.DevicePath.GetHashCode() + ")");
            }

            return HIDList;
        }
    }
}
