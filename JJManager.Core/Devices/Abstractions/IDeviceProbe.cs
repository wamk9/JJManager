using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJManager.Core.Devices.Abstractions
{
    public interface IDeviceProbe<TNativeDevice>
    {
        Task<DeviceProbeResult> ProbeAsync(
            TNativeDevice nativeDevice,
            CancellationToken cancellationToken = default);
    }

}
