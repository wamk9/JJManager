using JJManager.Core.Interfaces.Repositories;
using JJManager.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace JJManager.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation for managing database transactions
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly JJManagerDbContext _context;
    private IDbContextTransaction? _transaction;

    private IConfigRepository? _configs;
    private IProductRepository? _products;
    private IProfileRepository? _profiles;
    private IUserProductRepository? _userProducts;

    public UnitOfWork(JJManagerDbContext context)
    {
        _context = context;
    }

    public IConfigRepository Configs => _configs ??= new ConfigRepository(_context);
    public IProductRepository Products => _products ??= new ProductRepository(_context);
    public IProfileRepository Profiles => _profiles ??= new ProfileRepository(_context);
    public IUserProductRepository UserProducts => _userProducts ??= new UserProductRepository(_context);

    public async Task InitializeAsync()
    {
        // Ensure database directory exists
       var directory = GetDatabaseFolder();

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Apply migrations
        await _context.Database.MigrateAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public string GetDatabaseFolder()
    {
        var connectionString = _context.Database.GetConnectionString();

        if (!string.IsNullOrEmpty(connectionString) && connectionString.Contains("Data Source="))
        {
            var dataSourceStart = connectionString.IndexOf("Data Source=") + 12;
            var dataSourceEnd = connectionString.IndexOf(';', dataSourceStart);
            var dbPath = dataSourceEnd > 0
                ? connectionString.Substring(dataSourceStart, dataSourceEnd - dataSourceStart)
                : connectionString.Substring(dataSourceStart);

            return Path.GetDirectoryName(dbPath) ?? string.Empty;
        }

        return string.Empty;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
