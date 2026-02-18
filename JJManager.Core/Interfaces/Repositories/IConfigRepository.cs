using JJManager.Core.Database.Entities;

namespace JJManager.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for configuration operations
/// </summary>
public interface IConfigRepository
{
    /// <summary>
    /// Get a configuration value by key
    /// </summary>
    Task<string?> GetValueAsync(string key);

    /// <summary>
    /// Get a configuration entity by key
    /// </summary>
    Task<ConfigEntity?> GetByKeyAsync(string key);

    /// <summary>
    /// Set a configuration value (creates if not exists)
    /// </summary>
    Task SetValueAsync(string key, string value, string? description = null);

    /// <summary>
    /// Save a configuration entity
    /// </summary>
    Task SaveAsync(ConfigEntity config);

    /// <summary>
    /// Get all configurations
    /// </summary>
    Task<List<ConfigEntity>> GetAllAsync();

    /// <summary>
    /// Delete a configuration by key
    /// </summary>
    Task DeleteAsync(string key);
}
