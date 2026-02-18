using JJManager.Core.Database.Entities;
using JJManager.Core.Interfaces.Repositories;
using JJManager.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace JJManager.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for product operations
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly JJManagerDbContext _context;

    public ProductRepository(JJManagerDbContext context)
    {
        _context = context;
    }

    public async Task<ProductEntity?> GetByIdAsync(Guid id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<ProductEntity?> GetByNameAndTypeAsync(string productName, string connectionType)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.ProductName == productName && p.ConnectionType == connectionType);
    }

    public async Task<ProductEntity?> GetByClassNameAsync(string className)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.ClassName == className);
    }

    public async Task<List<ProductEntity>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<Guid> SaveAsync(ProductEntity product)
    {
        if (product.Id == Guid.Empty)
        {
            product.Id = Guid.NewGuid();
            product.CreatedAt = DateTime.UtcNow;
            _context.Products.Add(product);
        }
        else
        {
            var existing = await _context.Products.FindAsync(product.Id);
            if (existing != null)
            {
                existing.ProductName = product.ProductName;
                existing.ConnectionType = product.ConnectionType;
                existing.ClassName = product.ClassName;
                existing.Description = product.Description;
            }
            else
            {
                product.CreatedAt = DateTime.UtcNow;
                _context.Products.Add(product);
            }
        }

        await _context.SaveChangesAsync();
        return product.Id;
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}
