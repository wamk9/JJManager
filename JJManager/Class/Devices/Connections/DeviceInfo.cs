using System;
using System.Runtime.InteropServices;
using System.Text;

namespace JJManager.Class.Devices.Connections
{
    public class DeviceInfo
    {
        private const uint DIGCF_PRESENT = 0x00000002;
        private const uint DIGCF_ALLCLASSES = 0x00000004;
        private const uint SPDRP_DEVICEDESC = 0x00000000; // Device Description
        private const uint SPDRP_FRIENDLYNAME = 0x0000000C; // Friendly Name

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public Guid ClassGuid;
            public uint DevInst;
            public IntPtr Reserved;
        }

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetupDiGetClassDevs(
            [In] ref Guid ClassGuid,
            [MarshalAs(UnmanagedType.LPStr)] string Enumerator,
            IntPtr hwndParent,
            uint Flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInfo(
            IntPtr DeviceInfoSet,
            uint MemberIndex,
            ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiGetDeviceRegistryProperty(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            uint Property,
            out uint PropertyRegDataType,
            [Out] byte[] PropertyBuffer,
            uint PropertyBufferSize,
            out uint RequiredSize);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        public static string GetProductName(string deviceInstanceId)
        {
            var classGuid = Guid.Empty;
            var deviceInfoSet = SetupDiGetClassDevs(
                ref classGuid,
                null,
                IntPtr.Zero,
                DIGCF_PRESENT | DIGCF_ALLCLASSES);

            if (deviceInfoSet == IntPtr.Zero)
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }

            var deviceInfoData = new SP_DEVINFO_DATA
            {
                cbSize = (uint)Marshal.SizeOf(typeof(SP_DEVINFO_DATA))
            };

            for (uint i = 0; SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfoData); i++)
            {
                uint dataType;
                byte[] buffer = new byte[1024];
                uint requiredSize;

                // Try to get the device description first
                if (SetupDiGetDeviceRegistryProperty(
                    deviceInfoSet,
                    ref deviceInfoData,
                    SPDRP_DEVICEDESC,
                    out dataType,
                    buffer,
                    (uint)buffer.Length,
                    out requiredSize))
                {
                    string deviceDescription = Encoding.Unicode.GetString(buffer).TrimEnd('\0');
                    if (!string.IsNullOrEmpty(deviceDescription))
                    {
                        SetupDiDestroyDeviceInfoList(deviceInfoSet);
                        return deviceDescription;
                    }
                }

                // Try to get the friendly name if the description is not satisfactory
                if (SetupDiGetDeviceRegistryProperty(
                    deviceInfoSet,
                    ref deviceInfoData,
                    SPDRP_FRIENDLYNAME,
                    out dataType,
                    buffer,
                    (uint)buffer.Length,
                    out requiredSize))
                {
                    string friendlyName = Encoding.Unicode.GetString(buffer).TrimEnd('\0');
                    if (!string.IsNullOrEmpty(friendlyName))
                    {
                        SetupDiDestroyDeviceInfoList(deviceInfoSet);
                        return friendlyName;
                    }
                }
            }

            SetupDiDestroyDeviceInfoList(deviceInfoSet);
            return null;
        }
    }
}