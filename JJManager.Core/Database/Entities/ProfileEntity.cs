namespace JJManager.Core.Database.Entities;

/// <summary>
/// Device profile entity
/// </summary>
public class ProfileEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = "Perfil Padrão";
    public string? Data { get; set; }  // JSON data for device-specific settings
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ProductEntity? Product { get; set; }
    public virtual ICollection<InputEntity> Inputs { get; set; } = new List<InputEntity>();
    public virtual ICollection<OutputEntity> Outputs { get; set; } = new List<OutputEntity>();
    public virtual ICollection<UserProductEntity> UserProducts { get; set; } = new List<UserProductEntity>();
}
