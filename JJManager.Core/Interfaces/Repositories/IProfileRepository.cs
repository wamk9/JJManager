using JJManager.Core.Profile;

namespace JJManager.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for profile operations
/// Handles profiles with their inputs and outputs
/// </summary>
public interface IProfileRepository
{
    /// <summary>
    /// Get a profile by ID (includes inputs and outputs)
    /// </summary>
    Task<DeviceProfile?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get a profile by Connection ID (includes inputs and outputs)
    /// </summary>
    Task<DeviceProfile?> GetByConnIdAsync(string connId);

    /// <summary>
    /// Get all profiles
    /// </summary>
    Task<List<DeviceProfile>> GetAllAsync();

    /// <summary>
    /// Get profiles for a specific product
    /// </summary>
    Task<List<DeviceProfile>> GetByProductIdAsync(Guid productId);

    /// <summary>
    /// Delete a profile by ID (cascades to inputs and outputs)
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Check if a profile exists
    /// </summary>
    Task<bool> ExistsAsync(Guid id);

    /// <summary>
    /// Create/Update a profile with optional connection ID and product ID (used for auto-creating default profiles)
    /// </summary>
    Task<DeviceProfile> SaveAsync(DeviceProfile? profile = null, string? connId = null, Guid? productId = null, string name = "Perfil Padrão");

    /// <summary>
    /// Set a profile to be the default for a given connection ID (associates the profile with the connection)
    /// </summary>
    Task<bool> SetDefaultProfileAsync(Guid id, string connId);
    
    /// <summary>
    /// Get a profile to be the default for a given connection ID (associates the profile with the connection)
    /// </summary>
    Task<DeviceProfile> GetDefaultProfileAsync(string connId);

    /// <summary>
    /// Load a profile with optional connection ID and product ID (used for auto-creating default profiles)
    /// </summary>
    Task<DeviceProfile> GetAsync(string? connId = null, Guid? productId = null, string name = "Perfil Padrão");
}
