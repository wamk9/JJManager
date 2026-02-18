using JJManager.Core.Devices.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJManager.Core.Devices.Abstractions
{
    public sealed class HidDeviceProbe
        : IDeviceProbe<HidSharp.HidDevice>
    {
        private const int PROBE_TIMEOUT_MS = 2000; // 1 second max per device

        public async Task<DeviceProbeResult> ProbeAsync(
            HidSharp.HidDevice hid,
            CancellationToken ct = default)
        {
            try
            {
                // Create a timeout cancellation token
                using var timeoutCts = new CancellationTokenSource(PROBE_TIMEOUT_MS);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);



                using var generic = new GenericHidDevice(hid);

                // Get firmware version with timeout
                var firmware = await generic.GetFirmwareVersionAsync(linkedCts.Token);

                // Use GenericHidDevice's DeviceClassName which has centralized mapping
                var kind = generic.DeviceClassName;
                if (kind == "Unknown") kind = "Generic";



                return new DeviceProbeResult
                {
                    Success = firmware != null,
                    DeviceKind = kind,
                    FirmwareVersion = firmware
                };
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine($"HidDeviceProbe timeout for {hid.GetProductName()}");
                return new DeviceProbeResult { Success = false };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HidDeviceProbe error for {hid.GetProductName()}: {ex.Message}");
                return new DeviceProbeResult { Success = false };
            }
        }
    }

}
