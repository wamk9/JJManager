namespace JJManager.Core.Interfaces.Repositories;

/// <summary>
/// Unit of Work interface for managing database transactions
/// Provides access to all repositories
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Configuration repository
    /// </summary>
    IConfigRepository Configs { get; }

    /// <summary>
    /// Product repository
    /// </summary>
    IProductRepository Products { get; }

    /// <summary>
    /// Profile repository
    /// </summary>
    IProfileRepository Profiles { get; }

    /// <summary>
    /// User product repository
    /// </summary>
    IUserProductRepository UserProducts { get; }

    /// <summary>
    /// Initialize the database (run migrations)
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Save all pending changes
    /// </summary>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// Begin a new transaction
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commit the current transaction
    /// </summary>
    Task CommitAsync();

    /// <summary>
    /// Rollback the current transaction
    /// </summary>
    Task RollbackAsync();

    string GetDatabaseFolder();
}
