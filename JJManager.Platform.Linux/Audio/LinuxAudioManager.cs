using JJManager.Core.Audio;

namespace JJManager.Platform.Linux.Audio;

/// <summary>
/// Linux implementation of IAudioManager using PulseAudio/PipeWire
/// PipeWire is compatible with PulseAudio API, so this implementation works with both
/// </summary>
public class LinuxAudioManager : IAudioManager
{
    private bool _isMonitoring;
    private bool _isDisposed;

    public bool IsMonitoring => _isMonitoring;

    public event EventHandler<SessionChangedEventArgs>? SessionChanged;
    public event EventHandler<DeviceChangedEventArgs>? DeviceChanged;

    public async Task<List<AudioDevice>> GetPlaybackDevicesAsync()
    {
        // TODO: Implement using PulseAudio P/Invoke (pa_context_get_sink_info_list)
        await Task.CompletedTask;
        return new List<AudioDevice>();
    }

    public async Task<List<AudioDevice>> GetRecordingDevicesAsync()
    {
        // TODO: Implement using PulseAudio P/Invoke (pa_context_get_source_info_list)
        await Task.CompletedTask;
        return new List<AudioDevice>();
    }

    public async Task<List<AudioSession>> GetActiveSessionsAsync()
    {
        // TODO: Implement using PulseAudio P/Invoke (pa_context_get_sink_input_info_list)
        await Task.CompletedTask;
        return new List<AudioSession>();
    }

    public async Task<List<AudioSession>> GetSessionsByDeviceAsync(string deviceId)
    {
        // TODO: Implement
        await Task.CompletedTask;
        return new List<AudioSession>();
    }

    public async Task SetSessionVolumeAsync(string sessionId, float volume)
    {
        // TODO: Implement using PulseAudio P/Invoke (pa_context_set_sink_input_volume)
        await Task.CompletedTask;
    }

    public async Task SetSessionMuteAsync(string sessionId, bool muted)
    {
        // TODO: Implement using PulseAudio P/Invoke (pa_context_set_sink_input_mute)
        await Task.CompletedTask;
    }

    public async Task SetDeviceVolumeAsync(string deviceId, float volume)
    {
        // TODO: Implement using PulseAudio P/Invoke (pa_context_set_sink_volume_by_name)
        await Task.CompletedTask;
    }

    public async Task SetDeviceMuteAsync(string deviceId, bool muted)
    {
        // TODO: Implement using PulseAudio P/Invoke (pa_context_set_sink_mute_by_name)
        await Task.CompletedTask;
    }

    public async Task SetDefaultPlaybackDeviceAsync(string deviceId)
    {
        // TODO: Implement using pactl or PulseAudio P/Invoke
        await Task.CompletedTask;
    }

    public async Task SetDefaultRecordingDeviceAsync(string deviceId)
    {
        // TODO: Implement using pactl or PulseAudio P/Invoke
        await Task.CompletedTask;
    }

    public async Task<AudioDevice?> GetDefaultPlaybackDeviceAsync()
    {
        // TODO: Implement using PulseAudio P/Invoke (pa_context_get_server_info)
        await Task.CompletedTask;
        return null;
    }

    public async Task<AudioDevice?> GetDefaultRecordingDeviceAsync()
    {
        // TODO: Implement using PulseAudio P/Invoke
        await Task.CompletedTask;
        return null;
    }

    public void StartMonitoring()
    {
        if (_isMonitoring)
            return;

        // TODO: Subscribe to PulseAudio events using pa_context_subscribe
        _isMonitoring = true;
    }

    public void StopMonitoring()
    {
        if (!_isMonitoring)
            return;

        // TODO: Unsubscribe from PulseAudio events
        _isMonitoring = false;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                StopMonitoring();
            }
            _isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
