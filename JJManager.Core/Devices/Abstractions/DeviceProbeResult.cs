using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJManager.Core.Devices.Abstractions
{
    public sealed class DeviceProbeResult
    {
        public bool Success { get; init; }
        public string DeviceKind { get; init; } = "Generic";
        public Version? FirmwareVersion { get; init; }
    }

}
