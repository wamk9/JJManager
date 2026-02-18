using JJManager.Core.Profile;

namespace JJManager.Core.Interfaces.Services;

/// <summary>
/// Service for database operations
/// </summary>
public interface IAppConfigService
{
    /// <summary>
    /// Initialize the database
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Get a configuration value
    /// </summary>
    Task<string?> GetConfigAsync(string key);

    /// <summary>
    /// Set a configuration value
    /// </summary>
    Task SetConfigAsync(string key, string value);

    string GetDatabaseFolder();
}
