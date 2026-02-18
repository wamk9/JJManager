using JJManager.Core.Database.Entities;
using JJManager.Core.Interfaces.Repositories;
using JJManager.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace JJManager.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for configuration operations
/// </summary>
public class ConfigRepository : IConfigRepository
{
    private readonly JJManagerDbContext _context;

    public ConfigRepository(JJManagerDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetValueAsync(string key)
    {
        var config = await _context.Configs.FirstOrDefaultAsync(c => c.Key == key);
        return config?.Value;
    }

    public async Task<ConfigEntity?> GetByKeyAsync(string key)
    {
        return await _context.Configs.FirstOrDefaultAsync(c => c.Key == key);
    }

    public async Task SetValueAsync(string key, string value, string? description = null)
    {
        var config = await _context.Configs.FirstOrDefaultAsync(c => c.Key == key);

        if (config != null)
        {
            config.Value = value;
            config.UpdatedAt = DateTime.UtcNow;
            if (description != null)
            {
                config.Description = description;
            }
        }
        else
        {
            _context.Configs.Add(new ConfigEntity
            {
                Id = Guid.NewGuid(),
                Key = key,
                Value = value,
                Description = description,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task SaveAsync(ConfigEntity config)
    {
        var existing = await _context.Configs.FirstOrDefaultAsync(c => c.Key == config.Key);

        if (existing != null)
        {
            existing.Value = config.Value;
            existing.Description = config.Description;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            if (config.Id == Guid.Empty)
            {
                config.Id = Guid.NewGuid();
            }
            config.UpdatedAt = DateTime.UtcNow;
            _context.Configs.Add(config);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<ConfigEntity>> GetAllAsync()
    {
        return await _context.Configs.ToListAsync();
    }

    public async Task DeleteAsync(string key)
    {
        var config = await _context.Configs.FirstOrDefaultAsync(c => c.Key == key);
        if (config != null)
        {
            _context.Configs.Remove(config);
            await _context.SaveChangesAsync();
        }
    }
}
