using JJManager.Core.Database.Entities;

namespace JJManager.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for user product (connected devices) operations
/// </summary>
public interface IUserProductRepository
{
    /// <summary>
    /// Get a user product by ID
    /// </summary>
    Task<UserProductEntity?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get a user product by connection ID and product ID
    /// </summary>
    Task<UserProductEntity?> GetByConnectionAsync(string connectionId, Guid productId);

    /// <summary>
    /// Get a user product by connection ID only
    /// </summary>
    Task<UserProductEntity?> GetByConnIdAsync(string connId);

    /// <summary>
    /// Get all user products
    /// </summary>
    Task<List<UserProductEntity>> GetAllAsync();

    /// <summary>
    /// Get all user products for a specific product
    /// </summary>
    Task<List<UserProductEntity>> GetByProductIdAsync(Guid productId);

    /// <summary>
    /// Get all user products with auto-connect enabled
    /// </summary>
    Task<List<UserProductEntity>> GetAutoConnectAsync();

    /// <summary>
    /// Save a user product (creates or updates)
    /// </summary>
    Task<Guid> SaveAsync(UserProductEntity userProduct);

    /// <summary>
    /// Update the profile association for a user product
    /// </summary>
    Task SetProfileAsync(Guid userProductId, Guid? profileId);

    /// <summary>
    /// Update the auto-connect setting for a user product
    /// </summary>
    Task SetAutoConnectAsync(Guid userProductId, bool autoConnect);

    /// <summary>
    /// Delete a user product by ID
    /// </summary>
    Task DeleteAsync(Guid id);
}
