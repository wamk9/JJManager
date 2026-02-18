using JJManager.Core.Interfaces.Repositories;
using JJManager.Core.Interfaces.Services;
using JJManager.Infrastructure.Database;
using JJManager.Infrastructure.Repositories;
using JJManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JJManager.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure services
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Add Infrastructure services to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="sqlitePath">Full path to the SQLite database file</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string sqlitePath)
    {
        // Ensure directory exists
        var directory = Path.GetDirectoryName(sqlitePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Register DbContext
        services.AddDbContext<JJManagerDbContext>(options =>
            options.UseSqlite($"Data Source={sqlitePath}"));

        // Register repositories
        services.AddScoped<IConfigRepository, ConfigRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IUserProductRepository, UserProductRepository>();

        // Register services
        services.AddScoped<IAppConfigService, AppConfigService>();
        services.AddScoped<IDeviceProfileService, DeviceProfileService>();
        services.AddScoped<IUserProductService, UserProductService>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    /// <summary>
    /// Get the default database path for the current platform
    /// </summary>
    /// <returns>Full path to the database file</returns>
    public static string GetDefaultDatabasePath()
    {
        string baseFolder;

        if (OperatingSystem.IsWindows())
        {
            baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }
        else if (OperatingSystem.IsLinux())
        {
            var xdgConfig = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            baseFolder = !string.IsNullOrEmpty(xdgConfig)
                ? xdgConfig
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
        }
        else if (OperatingSystem.IsMacOS())
        {
            baseFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support");
        }
        else
        {
            baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        return Path.Combine(baseFolder, "JohnJohn3D", "JJManager", "jjmanager.db");
    }
}
