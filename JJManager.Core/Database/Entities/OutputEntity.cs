namespace JJManager.Core.Database.Entities;

/// <summary>
/// Output configuration entity
/// </summary>
public class OutputEntity
{
    public Guid Id { get; set; }
    public Guid ProfileId { get; set; }
    public int Index { get; set; }  // Output index on device
    public string Mode { get; set; } = "None";  // None, Leds, DashboardLeds
    public string? Configuration { get; set; }  // JSON configuration
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ProfileEntity? Profile { get; set; }
}
