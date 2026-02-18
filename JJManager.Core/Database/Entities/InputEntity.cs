namespace JJManager.Core.Database.Entities;

/// <summary>
/// Input configuration entity
/// </summary>
public class InputEntity
{
    public Guid Id { get; set; }
    public Guid ProfileId { get; set; }
    public int Index { get; set; }  // Input index on device
    public string Mode { get; set; } = "None";  // None, MacroKey, AudioController, AudioPlayer
    public string Type { get; set; } = "Digital";  // Digital, Analog
    public string? Configuration { get; set; }  // JSON configuration
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ProfileEntity? Profile { get; set; }
}
