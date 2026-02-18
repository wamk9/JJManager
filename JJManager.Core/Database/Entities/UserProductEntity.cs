namespace JJManager.Core.Database.Entities;

/// <summary>
/// User's connected device entity
/// </summary>
public class UserProductEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ConnectionId { get; set; } = string.Empty;  // Unique device identifier (hash)
    public Guid? ProfileId { get; set; }
    public bool AutoConnect { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ProductEntity? Product { get; set; }
    public virtual ProfileEntity? Profile { get; set; }
}
