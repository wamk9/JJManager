using HidSharp;
using JJManager.Core.Devices.Abstractions;
using JJManager.Core.Devices.Generic;
using JJManager.Core.Devices.JJDB01;
using JJManager.Core.Devices.JJLC01;
using JJManager.Core.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HidSharpDevice = HidSharp.HidDevice;

namespace JJManager.Core.Devices.Factories
{
    public static class JJDeviceFactory
    {
        public static JJDevice? Create(
            HidSharpDevice hidDevice,
            DeviceProbeResult probeResult,
            IProductRepository? productRepository = null)
        {
            try
            {
                JJDevice device = probeResult.DeviceKind switch
                {
                    "JJLC01" => new JJLC01Device(hidDevice, productRepository),
                    "JJDB01" => new JJDB01Device(hidDevice, productRepository),
                    _ => new GenericHidDevice(hidDevice, productRepository)
                };

                // Set firmware version from probe result
                if (probeResult.FirmwareVersion != null)
                {
                    device.FirmwareVersion = probeResult.FirmwareVersion;
                }

                return device;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"JJDeviceFactory.Create error for {probeResult.DeviceKind}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create device and load ProductId from database
        /// </summary>
        public static async Task<JJDevice?> CreateAsync(
            HidSharpDevice hidDevice,
            DeviceProbeResult probeResult,
            IProductRepository? productRepository = null)
        {
            var device = Create(hidDevice, probeResult, productRepository);

            if (device != null && productRepository != null)
            {
                await device.LoadProductIdAsync();
            }

            return device;
        }
    }
}
