namespace JJManager.Core.Database.Entities;

/// <summary>
/// JohnJohn3D product definition entity
/// </summary>
public class ProductEntity
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ConnectionType { get; set; } = string.Empty;  // HID, Joystick, Bluetooth
    public string? ClassName { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<UserProductEntity> UserProducts { get; set; } = new List<UserProductEntity>();
    public virtual ICollection<ProfileEntity> Profiles { get; set; } = new List<ProfileEntity>();
}
