using JJManager.Core.Database.Entities;

namespace JJManager.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for product operations
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Get a product by ID
    /// </summary>
    Task<ProductEntity?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get a product by name and connection type
    /// </summary>
    Task<ProductEntity?> GetByNameAndTypeAsync(string productName, string connectionType);

    /// <summary>
    /// Get a product by class name
    /// </summary>
    Task<ProductEntity?> GetByClassNameAsync(string className);

    /// <summary>
    /// Get all products
    /// </summary>
    Task<List<ProductEntity>> GetAllAsync();

    /// <summary>
    /// Save a product (creates or updates)
    /// </summary>
    Task<Guid> SaveAsync(ProductEntity product);

    /// <summary>
    /// Delete a product by ID
    /// </summary>
    Task DeleteAsync(Guid id);
}
