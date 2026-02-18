namespace JJManager.Core.Devices;

/// <summary>
/// Type of device connection
/// </summary>
public enum DeviceConnectionType
{
    Unset,
    HID,
    Joystick,
    Bluetooth
}

/// <summary>
/// State of device connection
/// </summary>
public enum DeviceConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Error
}
