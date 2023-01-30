using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace JJMixer.Class
{
    public static class JJMixerDevice
    {
        public static List<String> UpdateCOMDevices()
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
        }
    }
}
