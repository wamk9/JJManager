namespace JJManager.Core.Audio;

/// <summary>
/// Represents an audio playback or recording device
/// </summary>
public class AudioDevice
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FriendlyName { get; set; } = string.Empty;
    public AudioDeviceType Type { get; set; }
    public AudioDeviceState State { get; set; }
    public bool IsDefault { get; set; }
    public float Volume { get; set; }
    public bool IsMuted { get; set; }
}

/// <summary>
/// Represents an audio session (application playing audio)
/// </summary>
public class AudioSession
{
    public string Id { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int ProcessId { get; set; }
    public string? IconPath { get; set; }
    public float Volume { get; set; }
    public bool IsMuted { get; set; }
    public AudioSessionState State { get; set; }
    public string DeviceId { get; set; } = string.Empty;
}

public enum AudioDeviceType
{
    Playback,
    Recording
}

public enum AudioDeviceState
{
    Active,
    Disabled,
    NotPresent,
    Unplugged
}

public enum AudioSessionState
{
    Active,
    Inactive,
    Expired
}

/// <summary>
/// Event args for session changes
/// </summary>
public class SessionChangedEventArgs : EventArgs
{
    public AudioSession Session { get; set; } = null!;
    public SessionChangeType ChangeType { get; set; }
}

/// <summary>
/// Event args for device changes
/// </summary>
public class DeviceChangedEventArgs : EventArgs
{
    public AudioDevice Device { get; set; } = null!;
    public DeviceChangeType ChangeType { get; set; }
}

public enum SessionChangeType
{
    Created,
    Disconnected,
    VolumeChanged,
    StateChanged
}

public enum DeviceChangeType
{
    Added,
    Removed,
    StateChanged,
    DefaultChanged,
    VolumeChanged
}

/// <summary>
/// Cross-platform audio management interface
/// </summary>
public interface IAudioManager : IDisposable
{
    /// <summary>
    /// Gets all playback devices
    /// </summary>
    Task<List<AudioDevice>> GetPlaybackDevicesAsync();

    /// <summary>
    /// Gets all recording devices
    /// </summary>
    Task<List<AudioDevice>> GetRecordingDevicesAsync();

    /// <summary>
    /// Gets all active audio sessions
    /// </summary>
    Task<List<AudioSession>> GetActiveSessionsAsync();

    /// <summary>
    /// Gets sessions filtered by device
    /// </summary>
    Task<List<AudioSession>> GetSessionsByDeviceAsync(string deviceId);

    /// <summary>
    /// Sets the volume for a specific session
    /// </summary>
    Task SetSessionVolumeAsync(string sessionId, float volume);

    /// <summary>
    /// Sets the mute state for a specific session
    /// </summary>
    Task SetSessionMuteAsync(string sessionId, bool muted);

    /// <summary>
    /// Sets the volume for a specific device
    /// </summary>
    Task SetDeviceVolumeAsync(string deviceId, float volume);

    /// <summary>
    /// Sets the mute state for a specific device
    /// </summary>
    Task SetDeviceMuteAsync(string deviceId, bool muted);

    /// <summary>
    /// Sets the default playback device
    /// </summary>
    Task SetDefaultPlaybackDeviceAsync(string deviceId);

    /// <summary>
    /// Sets the default recording device
    /// </summary>
    Task SetDefaultRecordingDeviceAsync(string deviceId);

    /// <summary>
    /// Gets the current default playback device
    /// </summary>
    Task<AudioDevice?> GetDefaultPlaybackDeviceAsync();

    /// <summary>
    /// Gets the current default recording device
    /// </summary>
    Task<AudioDevice?> GetDefaultRecordingDeviceAsync();

    /// <summary>
    /// Starts monitoring for audio changes
    /// </summary>
    void StartMonitoring();

    /// <summary>
    /// Stops monitoring for audio changes
    /// </summary>
    void StopMonitoring();

    /// <summary>
    /// Whether the audio manager is currently monitoring
    /// </summary>
    bool IsMonitoring { get; }

    /// <summary>
    /// Event raised when a session changes
    /// </summary>
    event EventHandler<SessionChangedEventArgs>? SessionChanged;

    /// <summary>
    /// Event raised when a device changes
    /// </summary>
    event EventHandler<DeviceChangedEventArgs>? DeviceChanged;
}
