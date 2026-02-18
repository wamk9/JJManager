using JJManager.Core.Database.Entities;
using JJManager.Core.Interfaces.Repositories;
using JJManager.Core.Interfaces.Services;

namespace JJManager.Infrastructure.Services;

/// <summary>
/// Service implementation for user product (connected devices) operations
/// </summary>
public class UserProductService : IUserProductService
{
    private readonly IUserProductRepository _userProductRepository;

    public UserProductService(IUserProductRepository userProductRepository)
    {
        _userProductRepository = userProductRepository;
    }

    public async Task<UserProductEntity?> GetByConnIdAsync(string connId)
    {
        return await _userProductRepository.GetByConnIdAsync(connId);
    }

    public async Task<bool> ExistsAsync(string connId)
    {
        var userProduct = await _userProductRepository.GetByConnIdAsync(connId);
        return userProduct != null;
    }

    public async Task<Guid> SaveAsync(UserProductEntity? userProduct = null, string? connId = null, Guid? productId = null)
    {
        if (userProduct != null)
        {
            return await _userProductRepository.SaveAsync(userProduct);
        }

        if (string.IsNullOrEmpty(connId) || productId == null || productId == Guid.Empty)
        {
            throw new ArgumentException("Either userProduct or both connId and productId must be provided.");
        }

        // Check if already exists
        var existing = await _userProductRepository.GetByConnIdAsync(connId);
        if (existing != null)
        {
            return existing.Id;
        }

        // Create new UserProduct
        var newUserProduct = new UserProductEntity
        {
            Id = Guid.NewGuid(),
            ConnectionId = connId,
            ProductId = productId.Value,
            ProfileId = null,
            AutoConnect = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _userProductRepository.SaveAsync(newUserProduct);
    }

    public async Task<bool> SetAutoConnectAsync(string connId, bool autoConnect)
    {
        var userProduct = await _userProductRepository.GetByConnIdAsync(connId);
        if (userProduct == null)
        {
            return false;
        }

        await _userProductRepository.SetAutoConnectAsync(userProduct.Id, autoConnect);
        return true;
    }
}
