using JJManager.Core.Database.Entities;

namespace JJManager.Core.Interfaces.Services;

/// <summary>
/// Service interface for user product (connected devices) operations
/// </summary>
public interface IUserProductService
{
    /// <summary>
    /// Get a user product by connection ID
    /// </summary>
    Task<UserProductEntity?> GetByConnIdAsync(string connId);

    /// <summary>
    /// Check if a UserProduct exists for the given connId
    /// </summary>
    Task<bool> ExistsAsync(string connId);

    /// <summary>
    /// Save a user product (creates or updates).
    /// Either pass a UserProductEntity or connId + productId for creation.
    /// </summary>
    Task<Guid> SaveAsync(UserProductEntity? userProduct = null, string? connId = null, Guid? productId = null);

    /// <summary>
    /// Set the auto-connect setting for a user product
    /// </summary>
    Task<bool> SetAutoConnectAsync(string connId, bool autoConnect);
}
