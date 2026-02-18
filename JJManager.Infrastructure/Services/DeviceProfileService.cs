using JJManager.Core.Interfaces.Repositories;
using JJManager.Core.Interfaces.Services;
using JJManager.Core.Profile;

namespace JJManager.Infrastructure.Services
{
    public class DeviceProfileService : IDeviceProfileService
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IUserProductService _userProductService;

        public DeviceProfileService(IProfileRepository profileRepository, IUserProductService userProductService)
        {
            _profileRepository = profileRepository;
            _userProductService = userProductService;
        }

        public async Task<DeviceProfile?> LoadSpecificAsync(string? connId = null, Guid? productId = null, string? name = null)
        {

            if (productId != null && !string.IsNullOrEmpty(name))
            {
                return await _profileRepository.GetByProductIdAsync(productId ?? Guid.Empty).ContinueWith(x => x.Result.FirstOrDefault(y => y.Name == name)) ?? null;
            }
            else if (connId != null)
            {
                return await _profileRepository.GetByConnIdAsync(connId) ?? null;
            }
            else
            {
                throw new ArgumentException("Either connId or both productId and name must be provided to load a device profile.");
            }
        }

        public async Task<List<DeviceProfile>> LoadAllAsync(Guid? productId)
        {

            if (productId != Guid.Empty)
            {
                return await _profileRepository.GetByProductIdAsync(productId ?? Guid.Empty) ?? new List<DeviceProfile>();
            }
            else
            {
                throw new ArgumentException("ProductId must be provided to load a list of device profile.");
            }
        }

        public async Task<bool> SetDefaultProfileAsync(Guid profileId, string connId)
        {
            // Check if profile exists
            var profileExists = await _profileRepository.ExistsAsync(profileId);
            if (!profileExists)
            {
                return false;
            }

            // Try to set via repository
            var success = await _profileRepository.SetDefaultProfileAsync(profileId, connId);

            if (!success)
            {
                // UserProduct doesn't exist, create it first
                var profile = await _profileRepository.GetByIdAsync(profileId);
                if (profile == null)
                {
                    return false;
                }

                await _userProductService.SaveAsync(connId: connId, productId: profile.ProductId);

                // Now set the profile
                await _profileRepository.SetDefaultProfileAsync(profileId, connId);
            }

            return true;
        }

        public async Task<DeviceProfile> GetDefaultProfileAsync(Guid productId, string connId)
        {
            // Ensure UserProduct exists
            if (!await _userProductService.ExistsAsync(connId))
            {
                await _userProductService.SaveAsync(connId: connId, productId: productId);
            }

            var profile = await _profileRepository.GetByConnIdAsync(connId);

            if (profile != null)
                return profile;

            var profiles = await _profileRepository.GetByProductIdAsync(productId);
            profile = profiles.FirstOrDefault();

            if (profile != null)
            {
                await _profileRepository.SetDefaultProfileAsync(profile.Id, connId);
                return profile;
            }

            profile = await _profileRepository.SaveAsync(productId: productId, connId: connId);

            return profile;
        }

        public async Task<bool> SaveAsync(DeviceProfile profile)
        {
            var profileUpdated = await _profileRepository.SaveAsync(profile);

            return profileUpdated?.Id != Guid.Empty;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            await _profileRepository.DeleteAsync(id);

            return !(await _profileRepository.ExistsAsync(id));
        }
    }
}
