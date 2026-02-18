using HidSharp;
using HidSharp.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using ProfileClass = JJManager.Class.App.Profile.Profile;

namespace JJManager.Class.Devices.Connections
{
    /// <summary>
    /// HID Message structure with CMD and Payload separated
    /// Supports automatic conversion from string, int, byte[] to payload bytes
    /// </summary>
    public class HIDMessage
    {
        public ushort Cmd { get; set; }
        public List<byte> Payload { get; set; }

        /// <summary>
        /// Create HID message with List of bytes payload
        /// </summary>
        public HIDMessage(ushort cmd, List<byte> payload)
        {
            Cmd = cmd;
            Payload = payload ?? new List<byte>();
        }

        /// <summary>
        /// Create HID message with byte array payload
        /// </summary>
        public HIDMessage(ushort cmd, params byte[] payload)
        {
            Cmd = cmd;
            Payload = new List<byte>(payload);
        }

        /// <summary>
        /// Create HID message with string payload (UTF-8 encoded)
        /// </summary>
        public HIDMessage(ushort cmd, string payload)
        {
            Cmd = cmd;
            Payload = payload != null ? new List<byte>(Encoding.UTF8.GetBytes(payload)) : new List<byte>();
        }

        /// <summary>
        /// Create HID message with single byte payload
        /// </summary>
        public HIDMessage(ushort cmd, byte payload)
        {
            Cmd = cmd;
            Payload = new List<byte> { payload };
        }

        /// <summary>
        /// Create HID message with int payload (converted to bytes, little-endian)
        /// </summary>
        public HIDMessage(ushort cmd, int payload)
        {
            Cmd = cmd;
            Payload = new List<byte>(BitConverter.GetBytes(payload));
        }

        /// <summary>
        /// Create HID message with uint payload (converted to bytes, little-endian)
        /// </summary>
        public HIDMessage(ushort cmd, uint payload)
        {
            Cmd = cmd;
            Payload = new List<byte>(BitConverter.GetBytes(payload));
        }

        /// <summary>
        /// Create HID message with ushort payload (converted to bytes, little-endian)
        /// </summary>
        public HIDMessage(ushort cmd, ushort payload)
        {
            Cmd = cmd;
            Payload = new List<byte>(BitConverter.GetBytes(payload));
        }

        /// <summary>
        /// Create HID message with no payload (empty)
        /// </summary>
        public HIDMessage(ushort cmd)
        {
            Cmd = cmd;
            Payload = new List<byte>();
        }
    }

    public class HID : JJDevice
    {
        protected byte _reportId = 0x00;

        private HidDevice _hidSharpDevice = null;
        private string _devicePath = null;
        private static readonly SemaphoreSlim _hidSemaphore = new SemaphoreSlim(1, 1);

        public HID(HidDevice hidSharpDevice) : base()
        {
            _hidSharpDevice = hidSharpDevice;
            _productName = _hidSharpDevice.GetProductName();
            _type = Type.HID;
            _devicePath = _hidSharpDevice.DevicePath;
            _connId = _devicePath.GetHashCode().ToString();
            GetProductID();
            GetHIDConnPort();
            _profile = new ProfileClass(this);
            GetUserProductID();
            GetAutoConnection();
        }

        /// <summary>
        /// Used to send a data to the device
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> SendHIDData(string data, bool forceConnection = false, int delay = 1500, int timeout = 2000, int delayBetweenChunks = 10, bool ignoreSemaphore = false)
        {
            await Task.Delay(delay);
            if (!ignoreSemaphore)
            {
                await _hidSemaphore.WaitAsync(); // Ensure only one request runs at a time
            }
            bool result = false;

            try
            {
                if (!_isConnected && !forceConnection)
                {
                    throw HidSharp.DeviceException.CreateIOException(_hidSharpDevice, "Device isn't connected on JJManager");
                }

                _sendInProgress = true;

                if (string.IsNullOrEmpty(data))
                {
                    throw new ArgumentNullException("data", "Necessary data to send to device");
                }

                HidStream stream = null;
                try
                {
                    if (!_hidSharpDevice.TryOpen(out stream))
                    {
                        throw HidSharp.DeviceException.CreateIOException(_hidSharpDevice, "Failed to open HID device");
                    }

                    stream.ReadTimeout = Math.Max(timeout, 5000);
                    stream.WriteTimeout = Math.Max(timeout, 5000);

                    // Wait for device to be ready
                    await Task.Delay(Math.Max(delay, 100));

                    int writeSize = _hidSharpDevice.GetMaxOutputReportLength(); // inclui o ReportID

                    int count = 0;
                    byte[] messageInBytes = Encoding.ASCII.GetBytes(data.Replace('\n', ' ').Trim() + '\n');
                    int messageSize = messageInBytes.Length;

                    while (messageSize > 0)
                    {
                        byte[] bytesToSend = new byte[writeSize];
                        int chunkSize = Math.Min(messageSize, writeSize - 1);

                        // Set the report ID as the first byte
                        bytesToSend[0] = _reportId;

                        // Copy message chunk
                        Array.Copy(messageInBytes, count, bytesToSend, 1, chunkSize);

                        // Update the counters
                        count += chunkSize;
                        messageSize -= chunkSize;

                        // Write to device
                        stream.Write(bytesToSend, 0, bytesToSend.Length);

                        if (messageSize > 0 && delayBetweenChunks > 0)
                        {
                            await Task.Delay(delayBetweenChunks);
                        }
                    }
                }
                finally
                {
                    stream?.Close();
                    stream?.Dispose();
                }

                result = true;
            }
            catch (TaskCanceledException)
            {
                // Do Nothing...
            }
            catch (TimeoutException ex)
            {
                Log.Insert("HID", "Timeout ao enviar dados para o dispositivo - dispositivo pode estar ocupado", ex);
                result = false;
                // Não dispara exception - permite retry
            }
            catch (Exception ex)
            {
                Log.Insert("HID", "Ocorreu um problema com o dispositivo ao realizar o envio de dados", ex);
                result = false;
                // Não dispara exception - permite retry
            }
            finally
            {
                _sendInProgress = false;

                if (!ignoreSemaphore)
                {
                    _hidSemaphore.Release(); // Release the semaphore
                }
            }

            return result;
        }

        /// <summary>
        /// Send multiple HID messages in sequence (Recommended - New Protocol)
        /// </summary>
        public async Task<bool> SendHIDBytes(List<HIDMessage> messages, bool forceConnection = false, int delay = 1500, int timeout = 2000, int delayBetweenChunks = 10, bool ignoreSemaphore = false)
        {
            if (messages == null || messages.Count == 0)
                return false;

            // Removed initial delay - will delay after each message
            if (!ignoreSemaphore)
            {
                await _hidSemaphore.WaitAsync();
            }

            bool allSuccess = true;

            try
            {
                foreach (var message in messages)
                {
                    // Build complete data: [CMD_H][CMD_L][Payload...]
                    List<byte> data = new List<byte>
                    {
                        (byte)(message.Cmd >> 8),   // CMD_H
                        (byte)(message.Cmd & 0xFF)  // CMD_L
                    };
                    data.AddRange(message.Payload);

                    // Send each message (with ignoreSemaphore=true to avoid deadlock)
                    bool success = await SendHIDBytes(data, forceConnection, 0, timeout, delayBetweenChunks, true);
                    if (!success)
                    {
                        allSuccess = false;
                        break;  // Stop on first failure
                    }

                    // Delay after each complete message (not just between)
                    if (delay > 0)
                    {
                        await Task.Delay(delay);
                    }
                }
            }
            finally
            {
                if (!ignoreSemaphore)
                {
                    _hidSemaphore.Release();
                }
            }

            return allSuccess;
        }

        public async Task<bool> SendHIDBytes(List<byte> data, bool forceConnection = false, int delay = 1500, int timeout = 2000, int delayBetweenChunks = 10, bool ignoreSemaphore = false)
        {
            // Removed initial delay - will delay after complete message
            if (!ignoreSemaphore)
            {
                await _hidSemaphore.WaitAsync(); // Ensure only one request runs at a time
            }
            bool result = false;

            try
            {
                if (!_isConnected && !forceConnection)
                {
                    throw HidSharp.DeviceException.CreateIOException(_hidSharpDevice, "Device isn't connected on JJManager");
                }

                _sendInProgress = true;

                HidStream stream = null;
                try
                {
                    if (!_hidSharpDevice.TryOpen(out stream))
                    {
                        throw HidSharp.DeviceException.CreateIOException(_hidSharpDevice, "Failed to open HID device");
                    }

                    stream.ReadTimeout = Math.Max(timeout, 5000);
                    stream.WriteTimeout = Math.Max(timeout, 5000);

                    int writeSize = _hidSharpDevice.GetMaxOutputReportLength(); // inclui o ReportID
                    byte[] messageInBytes = data.ToArray();
                    int totalSize = messageInBytes.Length;

                    // Extract CMD from first 2 bytes
                    ushort cmd = (ushort)((messageInBytes[0] << 8) | messageInBytes[1]);

                    // ARDUINO CHUNKING PROTOCOL:
                    // Format: [Report ID][CMD_H][CMD_L][Payload...][FLAG_H][FLAG_L]
                    // Flags (last 2 bytes of payload):
                    //   0x1001 = CONTINUE (has continuation)
                    //   0x2001 = END (final chunk)
                    // NO terminator - flags indicate the end

                    const int HEADER_SIZE = 3;  // Report ID + CMD (2 bytes)
                    const int FLAG_SIZE = 2;     // Flag (2 bytes at end)

                    // Max payload per chunk (including flags)
                    int maxChunkContent = writeSize - HEADER_SIZE;  // 64 - 3 = 61 bytes available
                    int chunkPayloadSize = maxChunkContent - FLAG_SIZE;  // 61 - 2 = 59 bytes for data

                    // Calculate number of chunks needed
                    int payloadSize = totalSize - 2;  // Total payload after CMD bytes
                    int chunkCount = (int)Math.Ceiling((double)payloadSize / chunkPayloadSize);

                    for (int chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
                    {
                        byte[] bytesToSend = new byte[writeSize];

                        // Set Report ID
                        bytesToSend[0] = _reportId;

                        // Set CMD (same for all chunks)
                        bytesToSend[1] = messageInBytes[0];  // CMD_H
                        bytesToSend[2] = messageInBytes[1];  // CMD_L

                        // Calculate payload offset and size
                        int sourceOffset = 2 + (chunkIndex * chunkPayloadSize);  // Start after CMD bytes
                        int remainingBytes = totalSize - sourceOffset;
                        int currentChunkSize = Math.Min(chunkPayloadSize, remainingBytes);

                        // Copy payload
                        Array.Copy(messageInBytes, sourceOffset, bytesToSend, 3, currentChunkSize);

                        // Set flags at the END of the payload (New protocol: FLAG_L = CMD_L)
                        bool isLastChunk = (chunkIndex == chunkCount - 1);

                        // New protocol: FLAG_H = 0x20, FLAG_L = CMD_L (for single chunk)
                        // Multi-chunk: FLAG_H = 0x10 (continue) or 0x20 (end), FLAG_L = CMD_L
                        byte flag_h = isLastChunk ? (byte)0x20 : (byte)0x10;
                        byte flag_l = (byte)(cmd & 0xFF);  // CMD_L

                        // Flags go right after the payload data
                        int flagPosition = 3 + currentChunkSize;
                        bytesToSend[flagPosition] = flag_h;      // FLAG_H (0x20 or 0x10)
                        bytesToSend[flagPosition + 1] = flag_l;  // FLAG_L (CMD_L)

                        // Write to device - SYNCHRONOUS for performance
                        stream.Write(bytesToSend, 0, bytesToSend.Length);

                        // Delay between chunks (except after last chunk)
                        if (chunkIndex < chunkCount - 1 && delayBetweenChunks > 0)
                        {
                            await Task.Delay(delayBetweenChunks);
                        }
                    }

                    // Delay AFTER sending complete message (all chunks)
                    if (delay > 0)
                    {
                        await Task.Delay(delay);
                    }
                }
                finally
                {
                    stream?.Close();
                    stream?.Dispose();
                }

                result = true;
            }
            catch (TaskCanceledException)
            {
                // Do Nothing...
            }
            catch (TimeoutException ex)
            {
                Log.Insert("HID", "Timeout ao enviar dados para o dispositivo - dispositivo pode estar ocupado", ex);
                result = false;
                // Não dispara exception - permite retry
            }
            catch (Exception ex)
            {
                Log.Insert("HID", "Ocorreu um problema com o dispositivo ao realizar o envio de dados", ex);
                result = false;
                // Não dispara exception - permite retry
            }
            finally
            {
                _sendInProgress = false;

                if (!ignoreSemaphore)
                {
                    _hidSemaphore.Release(); // Release the semaphore
                }
            }

            return result;
        }

        public async Task<bool> SendHIDBytes(List<List<byte>> data, bool forceConnection = false, int delay = 1500, int timeout = 2000, int delayBetweenChunks = 10, bool ignoreSemaphore = false)
        {
            if (data == null || data.Count == 0)
                return false;

            if (!ignoreSemaphore)
            {
                await _hidSemaphore.WaitAsync();
            }

            bool allSuccess = true;

            try
            {
                foreach (List<byte> toSend in data)
                {
                    // Call the single List<byte> overload which adds FLAGS automatically
                    bool success = await SendHIDBytes(toSend, forceConnection, delay, timeout, delayBetweenChunks, true);
                    if (!success)
                    {
                        allSuccess = false;
                        break;
                    }
                }
            }
            finally
            {
                if (!ignoreSemaphore)
                {
                    _hidSemaphore.Release();
                }
            }

            return allSuccess;
        }


        /// <summary>
        /// Used to receive a data, this will continue executing until receive anything or connection dropped
        /// </summary>
        /// <returns>A boolean saying if process execute with success and a string with the data</returns>
        public async Task<string> ReceiveHIDData(bool forceConnection = false, int timeout = 2000, bool ignoreSemaphore = false)
        {
            if (!ignoreSemaphore)
            {
                await _hidSemaphore.WaitAsync(); // Ensure only one request runs at a time
            }

            string receivedMessage = null;

            try
            {
                if (!_isConnected && !forceConnection)
                {
                    throw HidSharp.DeviceException.CreateIOException(_hidSharpDevice, "Device isn't connected on JJManager");
                }

                _receiveInProgress = true;

                using (var stream = _hidSharpDevice.Open())
                {
                    stream.ReadTimeout = timeout;
                    stream.WriteTimeout = timeout;

                    int readSize = _hidSharpDevice.GetMaxInputReportLength(); // inclui o ReportID

                    while (true)
                    {
                        try
                        {
                            byte[] bytesToRead = new byte[readSize];
                            int bytesRead = await stream.ReadAsync(bytesToRead, 0, bytesToRead.Length);

                            if (bytesRead > 0)
                            {
                                // Skip report ID (first byte) and filter out null bytes
                                receivedMessage += Encoding.ASCII.GetString(bytesToRead.Skip(1).Where(x => x != 0x00).ToArray());

                                Console.WriteLine(receivedMessage);

                                if (receivedMessage.Contains("\n"))
                                {
                                    receivedMessage = receivedMessage.Split('\n').FirstOrDefault();
                                    break;
                                }
                            }
                        }
                        catch (TimeoutException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            Log.Insert("HID", "Ocorreu um problema na leitura dos dados (ReceiveHIDData)", ex);
                            throw ex;
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Do Nothing...
            }
            catch (Exception ex)
            {
                Log.Insert("HID", "Ocorreu um problema com o dispositivo ao realizar o recebimento de dados", ex);
                throw ex;
            }
            finally
            {
                _receiveInProgress = false;

                if (!ignoreSemaphore)
                {
                    _hidSemaphore.Release();
                }
            }

            return receivedMessage;
        }

        /// <summary>
        /// Request HID bytes using HIDMessage protocol - Returns tuple with success status and response as List<byte> (raw data without CMD/FLAGS)
        /// Supports chunking (splits payload if > 60 bytes) and multiple messages
        /// </summary>
        public async Task<(bool success, List<byte> response)> RequestHIDBytes(List<HIDMessage> messages, bool forceConnection = false, int delay = 100, int timeout = 2000, int delayBetweenChunks = 10)
        {
            if (messages == null || messages.Count == 0)
                return (false, new List<byte>());

            // Protect entire operation with semaphore to prevent race conditions
            await _hidSemaphore.WaitAsync();

            try
            {
                List<byte> allResponseBytes = new List<byte>();

                // Process each message
                foreach (var message in messages)
            {
                const int MAX_CHUNK_SIZE = 60;  // Max payload bytes per chunk
                List<byte> fullPayload = message.Payload;
                int payloadLength = fullPayload.Count;
                ushort cmd = message.Cmd;

                // If payload fits in single chunk, send directly
                if (payloadLength <= MAX_CHUNK_SIZE)
                {
                    List<byte> data = new List<byte>
                    {
                        (byte)(cmd >> 8),   // CMD_H
                        (byte)(cmd & 0xFF)  // CMD_L
                    };
                    data.AddRange(fullPayload);
                    data.Add(0x20);  // FLAG_H = 0x20 (END)
                    data.Add((byte)(cmd & 0xFF));  // FLAG_L = CMD_L

                    // Call RequestHIDBytes (returns raw bytes without conversion)
                    // Use ignoreSemaphore=true to avoid deadlock since we already hold the semaphore
                    List<byte> response = await RequestHIDBytes(data, forceConnection, delay, timeout, delayBetweenChunks, true);

                    if (response != null && response.Count > 0)
                    {
                        allResponseBytes.AddRange(response);
                    }
                }
                else
                {
                    // Need to split into chunks
                    int chunkCount = (int)Math.Ceiling((double)payloadLength / MAX_CHUNK_SIZE);

                    for (int chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
                    {
                        int offset = chunkIndex * MAX_CHUNK_SIZE;
                        int chunkSize = Math.Min(MAX_CHUNK_SIZE, payloadLength - offset);
                        bool isLastChunk = (chunkIndex == chunkCount - 1);

                        List<byte> chunkData = new List<byte>
                        {
                            (byte)(cmd >> 8),   // CMD_H
                            (byte)(cmd & 0xFF)  // CMD_L
                        };

                        // Add chunk payload
                        for (int i = 0; i < chunkSize; i++)
                        {
                            chunkData.Add(fullPayload[offset + i]);
                        }

                        // Add flags
                        chunkData.Add(isLastChunk ? (byte)0x20 : (byte)0x10);  // FLAG_H (0x10=CONTINUE, 0x20=END)
                        chunkData.Add((byte)(cmd & 0xFF));  // FLAG_L = CMD_L

                        // Send chunk
                        if (isLastChunk)
                        {
                            // Wait for response on last chunk (ignoreSemaphore=true to avoid deadlock)
                            List<byte> response = await RequestHIDBytes(chunkData, forceConnection, delay, timeout, delayBetweenChunks, true);

                            if (response != null && response.Count > 0)
                            {
                                allResponseBytes.AddRange(response);
                            }
                        }
                        else
                        {
                            // No response expected for intermediate chunks (ignoreSemaphore=true)
                            await RequestHIDBytes(chunkData, forceConnection, 0, timeout, delayBetweenChunks, true);
                            await Task.Delay(delayBetweenChunks);
                        }
                    }
                }

                    // Small delay between messages
                    if (messages.IndexOf(message) < messages.Count - 1)
                    {
                        await Task.Delay(delayBetweenChunks);
                    }
                }

                return (allResponseBytes.Count > 0, allResponseBytes);
            }
            finally
            {
                _hidSemaphore.Release();
            }
        }

        public async Task<List<byte>> ReceiveHIDBytes(bool forceConnection = false, int timeout = 2000, bool ignoreSemaphore = false)
        {
            if (!ignoreSemaphore)
            {
                await _hidSemaphore.WaitAsync(); // Ensure only one request runs at a time
            }

            List<byte> bytes = new List<byte>();

            try
            {
                if (!_isConnected && !forceConnection)
                {
                    throw HidSharp.DeviceException.CreateIOException(_hidSharpDevice, "Device isn't connected on JJManager");
                }

                _receiveInProgress = true;

                using (var stream = _hidSharpDevice.Open())
                {
                    stream.ReadTimeout = timeout;
                    stream.WriteTimeout = timeout;

                    int readSize = _hidSharpDevice.GetMaxInputReportLength(); // inclui o ReportID

                    while (true)
                    {
                        try
                        {
                            byte[] bytesToRead = new byte[readSize];
                            int bytesRead = await stream.ReadAsync(bytesToRead, 0, bytesToRead.Length);

                            if (bytesRead > 0)
                            {
                                // Skip report ID (first byte)
                                bytes.AddRange(bytesToRead.Skip(1));

                                if (bytes.Contains(0x0A))
                                {
                                    bytes.RemoveRange(bytes.IndexOf(0x0A), bytes.Count - bytes.IndexOf(0x0A));
                                    break;
                                }
                            }
                        }
                        catch (TimeoutException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            Log.Insert("HID", "Ocorreu um problema na leitura dos dados (ReceiveHIDBytes)", ex);
                            throw ex;
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Do Nothing...
            }
            catch (Exception ex)
            {
                Log.Insert("HID", "Ocorreu um problema com o dispositivo ao realizar o recebimento de dados", ex);
                throw ex;
            }
            finally
            {
                _receiveInProgress = false;

                if (!ignoreSemaphore)
                {
                    _hidSemaphore.Release();
                }
            }

            return bytes;
        }


        public override bool Connect()
        {
            try
            {
                return base.Connect();
            }
            catch (Exception ex)
            {
                Log.Insert("HID", $"Não foi possível realizar a conexão com {_productName} de ID {_connId}", ex);
                return false;
            }
        }

        public override bool Disconnect()
        {
            try
            {
                return base.Disconnect();
            }
            catch (Exception ex)
            {
                Log.Insert("HID", $"Ocorreu um problema ao desconectar {_productName} de ID {_connId}", ex);
                return false;
            }
        }

        public async Task<string> RequestHIDData(string data, bool forceConnection = false, int delay = 100, int timeout = 2000, int delayBetweenChunks = 10)
        {
            await Task.Delay(delay);

            await _hidSemaphore.WaitAsync();

            string receivedMessage = null;

            try
            {
                if (!_isConnected && !forceConnection)
                {
                    throw HidSharp.DeviceException.CreateIOException(_hidSharpDevice, "Device isn't connected on JJManager");
                }

                _requestInProgress = true;

                await SendHIDData(data, forceConnection, delay, timeout, delayBetweenChunks, true);
                receivedMessage = await ReceiveHIDData(forceConnection, timeout, true);
            }
            catch (TaskCanceledException)
            {
                // Do Nothing...
            }
            catch (Exception ex)
            {
                Log.Insert("HID", "Ocorreu um problema com o dispositivo ao realizar o envio/recebimento de dados", ex);
                Disconnect();
            }
            finally
            {
                _requestInProgress = false;
                _hidSemaphore.Release();
            }

            return receivedMessage;
        }

        public async Task<List<byte>> RequestHIDBytes(List<byte> data, bool forceConnection = false, int delay = 100, int timeout = 2000, int delayBetweenChunks = 10, bool ignoreSemaphore = false)
        {
            await Task.Delay(delay);

            if (!ignoreSemaphore)
            {
                await _hidSemaphore.WaitAsync();
            }

            List<byte> assembledData = new List<byte>();

            try
            {
                if (!_isConnected && !forceConnection)
                {
                    throw HidSharp.DeviceException.CreateIOException(_hidSharpDevice, "Device isn't connected on JJManager");
                }

                _requestInProgress = true;

                using (var stream = _hidSharpDevice.Open())
                {
                    // Usa o timeout passado como parâmetro ao invés de valor fixo
                    stream.ReadTimeout = Math.Max(timeout, 1000);
                    stream.WriteTimeout = Math.Max(timeout, 1000);

                    int writeSize = _hidSharpDevice.GetMaxOutputReportLength(); // inclui o ReportID
                    int readSize = _hidSharpDevice.GetMaxInputReportLength(); // inclui o ReportID

                    // Cria o buffer completo com o descritor 0x00 no início
                    byte[] bytesToSend = new byte[writeSize];
                    byte[] bytesToRead = new byte[readSize];
                    bytesToSend[0] = 0x00; // descritor do report ID

                    // Copia o conteúdo a partir do segundo byte
                    int len = Math.Min(data.Count, writeSize - 1);
                    Array.Copy(data.ToArray(), 0, bytesToSend, 1, len);

                    try
                    {
                        Console.WriteLine("Enviando dados...");
                        await stream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                        Console.WriteLine("Enviado com sucesso!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao enviar: {ex.Message}");
                    }

                    // Read response (with potential chunking)
                    bool continueReading = true;

                    while (continueReading)
                    {
                        Array.Clear(bytesToRead, 0, bytesToRead.Length);
                        await stream.ReadAsync(bytesToRead, 0, bytesToRead.Length);
                        await stream.FlushAsync();

                        // Verify CMD
                        ushort cmd = (ushort)((bytesToRead[1] << 8) | bytesToRead[2]);
                        bool match = _cmds.Any(p => p == cmd);

                        if (!match)
                        {
                            break;
                        }

                        // Skip ReportID (1 byte) + CMD (2 bytes) = 3 bytes
                        byte[] payload = bytesToRead.Skip(3).ToArray();

                        // Find actual data length (first occurrence of 0x00 padding or end of buffer)
                        int actualDataLength = payload.Length;

                        // Find where data ends (before padding zeros, but after the flags)
                        // We need to find the flags first, so search from the end
                        for (int i = payload.Length - 1; i >= 0; i--)
                        {
                            if (payload[i] != 0x00)
                            {
                                actualDataLength = i + 1;
                                break;
                            }
                        }

                        // Check for flags in last 2 bytes of actual data
                        ushort flags = 0;
                        int dataLength = actualDataLength;

                        if (actualDataLength >= 2)
                        {
                            byte flag_h = payload[actualDataLength - 2];
                            byte flag_l = payload[actualDataLength - 1];
                            flags = (ushort)((flag_h << 8) | flag_l);
                            byte cmd_l = (byte)(cmd & 0xFF);

                            // Protocol validation: FLAG_L must equal CMD_L for single-packet responses
                            bool validSinglePacket = (flag_h == 0x20 && flag_l == cmd_l);
                            // Multi-packet flags: 0x1001=CONTINUE, 0x2001=END
                            bool validMultiPacket = (flags == 0x1001 || flags == 0x2001 ||
                                                    (flags & 0xF000) == 0x1000 || (flags & 0xF000) == 0x2000);

                            if (validSinglePacket || validMultiPacket)
                            {
                                // Remove flags from data
                                dataLength = actualDataLength - 2;
                                continueReading = (flags == 0x1001); // Continue if CONTINUE flag
                            }
                            else
                            {
                                continueReading = false;
                            }
                        }
                        else
                        {
                            continueReading = false;
                        }

                        // Add data to assembled message (skip flags)
                        for (int i = 0; i < dataLength; i++)
                        {
                            assembledData.Add(payload[i]);
                        }

                        // Check for old protocol terminator (0x0A)
                        if (assembledData.Contains((byte)0x0A))
                        {
                            continueReading = false;
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Do Nothing...
            }
            catch (Exception ex)
            {
                Log.Insert("HID", "Ocorreu um problema com o dispositivo ao realizar o envio/recebimento de dados", ex);
                Disconnect();
            }
            finally
            {
                _requestInProgress = false;

                if (!ignoreSemaphore)
                {
                    _hidSemaphore.Release();
                }
            }

            return assembledData;
        }

        public async Task GetFirmwareVersion()
        {
            // Try new binary protocol using HIDMessage (CMD: 0x00FF)
            if (_cmds != null && _cmds.Contains(0x00FF))
            {
                for (int i = 0; i < 2; i++)
                {
                    try
                    {
                        List<HIDMessage> messages = new List<HIDMessage>();
                        messages.Add(new HIDMessage(0x00FF, new byte[] { 0x00 }));  // INFO_TYPE: 0x00 = Firmware Version

                        (bool success, List<byte> responseBytes) = await RequestHIDBytes(messages, true, 0, 200, 2);

                        if (success && responseBytes != null && responseBytes.Count > 0)
                        {
                            // Response is raw bytes containing only the version string
                            // Format: [VERSION...] (e.g., "2025.12.28")

                            byte[] bytesArray = responseBytes.ToArray();
                            string versionString = System.Text.Encoding.UTF8.GetString(bytesArray).Trim();

                            _version = TranslateVersion(versionString);

                            if (_version != null)
                            {
                                break;
                            }
                        }
                    }
                    catch
                    {
                        // Continue to next attempt
                    }
                }
            }

            // Fallback to JSON protocol (for legacy devices)
            if (_version == null)
            {
                for (int i = 0; i < 2; i++)
                {
                    try
                    {
                        string jsonString = await RequestHIDData(new JsonObject { { "request", new JsonArray { { "firmware_version" } } } }.ToJsonString(), true);

                        if (!string.IsNullOrEmpty(jsonString))
                        {
                            JsonObject json = JsonObject.Parse(jsonString).AsObject();

                            if (json != null && json.ContainsKey("firmware_version"))
                            {
                                _version = TranslateVersion(json["firmware_version"].GetValue<string>());
                            }

                            break;
                        }
                    }
                    catch
                    {
                        // Continue to next attempt
                    }
                }
            }
        }

        private void GetHIDConnPort()
        {
            _connPort = new List<string>();

            try
            {
                ReportDescriptor reportDescriptor = _hidSharpDevice.GetReportDescriptor();
                HidStream hidStream = null;

                if (_hidSharpDevice.TryOpen(out hidStream))
                {
                    hidStream.WriteTimeout = 3000;
                    hidStream.ReadTimeout = 3000;

                    using (hidStream)
                    {
                        _connPort.Add(hidStream.Device.GetSerialPorts()[0].Replace("\\\\.\\", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Insert("HID", "Ocorreu um problema com o dispositivo ao realizar a busca da porta de comunicação", ex);
            }

            if (_connPort.Count == 0)
            {
                GetConnPortByVidPidAndProductName();
            }
        }
        private string ExtractFromDevicePath(string devicePath, string key, string delimiter = "&")
        {
            string result = string.Empty;
            int start = devicePath.IndexOf(key, StringComparison.OrdinalIgnoreCase);

            if (start >= 0)
            {
                start += key.Length;
                int end = devicePath.IndexOf(delimiter, start);
                if (end > start)
                {
                    result = devicePath.Substring(start, end - start);
                }
                else
                {
                    result = devicePath.Substring(start);
                }
            }

            return result.ToUpper(); // Ensure the extracted value is in uppercase
        }

        private void GetConnPortByVidPidAndProductName()
        {
            // Extract VID and PID from DevicePath
            string vid = "VID_" + ExtractFromDevicePath(_hidSharpDevice.DevicePath, "vid_");
            string pid = "PID_" + ExtractFromDevicePath(_hidSharpDevice.DevicePath, "pid_");

            // Query the WIN32_SerialPort WMI class
            using (var serialPortSearcher = new ManagementObjectSearcher("SELECT * FROM WIN32_SerialPort"))
            {
                using (var serialPortResults = serialPortSearcher.Get())
                {
                    foreach (var serialPort in serialPortResults.Cast<ManagementObject>())
                    {
                        string deviceID = serialPort["DeviceID"]?.ToString(); // e.g., "COM3"
                        string serialPNPDeviceID = serialPort["PNPDeviceID"]?.ToString(); // e.g., "USB\\VID_2341&PID_8055\\SERIALNUMBER"

                        if (serialPNPDeviceID == null) continue;

                        if (serialPNPDeviceID.Contains(vid) && serialPNPDeviceID.Contains(pid))
                        {
                            _connPort.Add(deviceID);
                        }
                    }
                }
            }
        }
    } 
}
