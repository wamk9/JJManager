using JJManager.Core.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJManager.Core.Interfaces.Services
{
    public interface IDeviceProfileService
    {
        Task<DeviceProfile?> LoadSpecificAsync(string? connId = null, Guid? productId = null, string? name = null);
        Task<List<DeviceProfile>> LoadAllAsync(Guid? productId);
        Task<bool> SetDefaultProfileAsync(Guid profileId, string connId);
        Task<DeviceProfile> GetDefaultProfileAsync(Guid productId, string connId);
        Task<bool> SaveAsync(DeviceProfile profile);
        Task<bool> DeleteAsync(Guid id);
    }
}
