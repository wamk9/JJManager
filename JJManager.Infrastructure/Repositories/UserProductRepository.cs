using JJManager.Core.Database.Entities;
using JJManager.Core.Interfaces.Repositories;
using JJManager.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace JJManager.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for user product operations
/// </summary>
public class UserProductRepository : IUserProductRepository
{
    private readonly JJManagerDbContext _context;

    public UserProductRepository(JJManagerDbContext context)
    {
        _context = context;
    }

    public async Task<UserProductEntity?> GetByIdAsync(Guid id)
    {
        return await _context.UserProducts
            .Include(up => up.Product)
            .Include(up => up.Profile)
            .FirstOrDefaultAsync(up => up.Id == id);
    }

    public async Task<UserProductEntity?> GetByConnectionAsync(string connectionId, Guid productId)
    {
        return await _context.UserProducts
            .Include(up => up.Product)
            .Include(up => up.Profile)
            .FirstOrDefaultAsync(up => up.ConnectionId == connectionId && up.ProductId == productId);
    }

    public async Task<UserProductEntity?> GetByConnIdAsync(string connId)
    {
        return await _context.UserProducts
            .Include(up => up.Product)
            .Include(up => up.Profile)
            .FirstOrDefaultAsync(up => up.ConnectionId == connId);
    }

    public async Task<List<UserProductEntity>> GetAllAsync()
    {
        return await _context.UserProducts
            .Include(up => up.Product)
            .Include(up => up.Profile)
            .ToListAsync();
    }

    public async Task<List<UserProductEntity>> GetByProductIdAsync(Guid productId)
    {
        return await _context.UserProducts
            .Include(up => up.Product)
            .Include(up => up.Profile)
            .Where(up => up.ProductId == productId)
            .ToListAsync();
    }

    public async Task<List<UserProductEntity>> GetAutoConnectAsync()
    {
        return await _context.UserProducts
            .Include(up => up.Product)
            .Include(up => up.Profile)
            .Where(up => up.AutoConnect)
            .ToListAsync();
    }

    public async Task<Guid> SaveAsync(UserProductEntity userProduct)
    {
        if (userProduct.Id == Guid.Empty)
        {
            userProduct.Id = Guid.NewGuid();
            userProduct.CreatedAt = DateTime.UtcNow;
            userProduct.UpdatedAt = DateTime.UtcNow;
            _context.UserProducts.Add(userProduct);
        }
        else
        {
            var existing = await _context.UserProducts.FindAsync(userProduct.Id);
            if (existing != null)
            {
                existing.ConnectionId = userProduct.ConnectionId;
                existing.ProductId = userProduct.ProductId;
                existing.ProfileId = userProduct.ProfileId;
                existing.AutoConnect = userProduct.AutoConnect;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                userProduct.CreatedAt = DateTime.UtcNow;
                userProduct.UpdatedAt = DateTime.UtcNow;
                _context.UserProducts.Add(userProduct);
            }
        }

        await _context.SaveChangesAsync();
        return userProduct.Id;
    }

    public async Task SetProfileAsync(Guid userProductId, Guid? profileId)
    {
        var userProduct = await _context.UserProducts.FindAsync(userProductId);
        if (userProduct != null)
        {
            userProduct.ProfileId = profileId;
            userProduct.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task SetAutoConnectAsync(Guid userProductId, bool autoConnect)
    {
        var userProduct = await _context.UserProducts.FindAsync(userProductId);
        if (userProduct != null)
        {
            userProduct.AutoConnect = autoConnect;
            userProduct.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var userProduct = await _context.UserProducts.FindAsync(id);
        if (userProduct != null)
        {
            _context.UserProducts.Remove(userProduct);
            await _context.SaveChangesAsync();
        }
    }
}
