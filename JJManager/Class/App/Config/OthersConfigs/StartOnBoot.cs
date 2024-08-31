using Microsoft.Win32;
using System;
using System.Reflection;

namespace JJManager.Class.App.Config.OthersConfigs
{
    public class StartOnBoot
    {
        private string _appName = Assembly.GetExecutingAssembly().GetName().Name;
        private string _appPath = Assembly.GetExecutingAssembly().Location;
        
        public bool OnBoot
        {
            get => CheckIfIsOnBoot();
        }

        public void Update(bool startOnBoot)
        {
            if (startOnBoot)
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                registryKey.SetValue(_appName, _appPath);
            }
            else
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                registryKey.DeleteValue(_appName, false);
            }
        }

        private bool CheckIfIsOnBoot()
        {
            using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false))
            {
                return !string.IsNullOrEmpty((string)(registryKey?.GetValue(_appName, null)));
            }
        }
    }
}
