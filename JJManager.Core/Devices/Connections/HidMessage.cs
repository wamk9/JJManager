using System.Text;

namespace JJManager.Core.Devices.Connections;

/// <summary>
/// HID Message structure for byte-based protocol
/// Supports CMD (16-bit) + Payload format used by JohnJohn3D devices
/// </summary>
public class HidMessage
{
    /// <summary>
    /// 16-bit command identifier (big-endian format)
    /// </summary>
    public ushort Command { get; set; }

    /// <summary>
    /// Message payload (variable length)
    /// </summary>
    public List<byte> Payload { get; set; }

    /// <summary>
    /// Create HID message with no payload
    /// </summary>
    public HidMessage(ushort command)
    {
        Command = command;
        Payload = new List<byte>();
    }

    /// <summary>
    /// Create HID message with byte array payload
    /// </summary>
    public HidMessage(ushort command, params byte[] payload)
    {
        Command = command;
        Payload = new List<byte>(payload);
    }

    /// <summary>
    /// Create HID message with List payload
    /// </summary>
    public HidMessage(ushort command, List<byte> payload)
    {
        Command = command;
        Payload = payload ?? new List<byte>();
    }

    /// <summary>
    /// Create HID message with string payload (UTF-8 encoded)
    /// </summary>
    public HidMessage(ushort command, string payload)
    {
        Command = command;
        Payload = string.IsNullOrEmpty(payload)
            ? new List<byte>()
            : new List<byte>(Encoding.UTF8.GetBytes(payload));
    }

    /// <summary>
    /// Create HID message with single byte payload
    /// </summary>
    public HidMessage(ushort command, byte payload)
    {
        Command = command;
        Payload = new List<byte> { payload };
    }

    /// <summary>
    /// Create HID message with int payload (little-endian)
    /// </summary>
    public HidMessage(ushort command, int payload)
    {
        Command = command;
        Payload = new List<byte>(BitConverter.GetBytes(payload));
    }

    /// <summary>
    /// Create HID message with ushort payload (little-endian)
    /// </summary>
    public HidMessage(ushort command, ushort payload)
    {
        Command = command;
        Payload = new List<byte>(BitConverter.GetBytes(payload));
    }

    /// <summary>
    /// Build the complete message bytes including CMD header
    /// </summary>
    /// <param name="includeFlags">Whether to include protocol flags at the end</param>
    public byte[] ToBytes(bool includeFlags = true)
    {
        var result = new List<byte>
        {
            (byte)(Command >> 8),    // CMD_H (high byte)
            (byte)(Command & 0xFF)   // CMD_L (low byte)
        };

        result.AddRange(Payload);

        if (includeFlags)
        {
            result.Add(0x20);                      // FLAG_H = 0x20 (END)
            result.Add((byte)(Command & 0xFF));   // FLAG_L = CMD_L (validation)
        }

        return result.ToArray();
    }

    /// <summary>
    /// Parse a response byte array to extract payload (removes CMD and FLAGS)
    /// </summary>
    public static byte[] ExtractPayload(byte[] response, bool hasFlags = true)
    {
        if (response == null || response.Length < 2)
            return Array.Empty<byte>();

        int startIndex = 2; // Skip CMD (2 bytes)
        int endIndex = hasFlags && response.Length >= 4
            ? response.Length - 2  // Remove FLAGS (2 bytes)
            : response.Length;

        int length = endIndex - startIndex;
        if (length <= 0)
            return Array.Empty<byte>();

        var payload = new byte[length];
        Array.Copy(response, startIndex, payload, 0, length);
        return payload;
    }
}
