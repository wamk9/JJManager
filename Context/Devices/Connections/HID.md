# Contexto: HID Communication Layer

## Visão Geral
Camada de comunicação HID (Human Interface Device) para dispositivos JJManager usando biblioteca HidSharp.

## Características
- **Biblioteca:** HidSharp 2.1.0
- **Protocolo:** Byte-based messaging com comando + payload + flags
- **Migração:** Device.Net → HidSharp (2025-12)
- **Dispositivos:** JJM01, JJB999, JJB01_V2, JJBP06, JJBSlim_A, JJSD01, JJLC01, JJDB01

## Histórico de Desenvolvimento

### 2025-12-26: Criação da Classe HIDMessage

#### Motivação
Necessidade de classe dedicada para encapsular comandos HID e facilitar construção de mensagens byte-based.

#### Implementação
```csharp
public class HIDMessage
{
    public ushort Command { get; private set; }     // 16-bit command (0x0000 - 0xFFFF)
    public byte[] Payload { get; private set; }     // Variable-length data
    public byte[] FullMessage { get; private set; } // Complete message with flags

    public HIDMessage(ushort command, byte[] payload)
    {
        Command = command;
        Payload = payload ?? new byte[0];
        BuildFullMessage();
    }

    private void BuildFullMessage()
    {
        List<byte> message = new List<byte>();

        // Add Command (big-endian)
        message.Add((byte)((Command >> 8) & 0xFF));  // CMD_H
        message.Add((byte)(Command & 0xFF));         // CMD_L

        // Add Payload
        if (Payload.Length > 0)
        {
            message.AddRange(Payload);
        }

        // Add FLAGS (0x20, CMD_L)
        message.Add(0x20);                           // FLAG_H
        message.Add((byte)(Command & 0xFF));         // FLAG_L = CMD_L

        FullMessage = message.ToArray();
    }
}
```

**Vantagens:**
- Encapsulamento de lógica de construção
- Validação automática de flags
- Redução de código duplicado
- Facilita manutenção

### 2025-12-26: Correção de FLAGS no Protocolo

#### Problema Original
Flags estavam hardcoded como `0x20 0xFF` em todos os comandos, mas a especificação do protocolo diz que **FLAG_L deve ser igual a CMD_L**.

#### Impacto
- JJB999, JJB01_V2 enviavam sempre `0x20 0xFF`
- Firmware aceitava, mas validação de flags estava incorreta
- Comandos diferentes tinham mesmas flags (perda de validação)

#### Solução
```csharp
// ANTES (ERRADO):
message.Add(0x20);  // FLAG_H
message.Add(0xFF);  // FLAG_L (sempre 0xFF)

// DEPOIS (CORRETO):
message.Add(0x20);                    // FLAG_H
message.Add((byte)(Command & 0xFF));  // FLAG_L = CMD_L
```

**Exemplos:**
| Comando | CMD | Flags Antes | Flags Depois |
|---------|-----|-------------|--------------|
| LED Mode | 0x0000 | 0x20 0xFF | 0x20 0x00 |
| Brightness | 0x0001 | 0x20 0xFF | 0x20 0x01 |
| Blink Speed | 0x0002 | 0x20 0xFF | 0x20 0x02 |
| Device Info | 0x00FF | 0x20 0xFF | 0x20 0xFF |

### 2025-12-28: Implementação de RequestHIDBytes

#### Motivação
JJM-01 precisa de comunicação request/response (envia comando, aguarda resposta do dispositivo).

#### Implementação
```csharp
public async Task<(bool success, List<byte> responseBytes)> RequestHIDBytes(
    List<HIDMessage> messages,
    bool retryOnError = false,
    int delayBetweenMessages = 0,
    int timeout = 2000,
    int maxRetries = 5)
{
    if (!_isConnected || _hidStream == null)
        return (false, null);

    try
    {
        // Send all messages
        foreach (HIDMessage message in messages)
        {
            byte[] dataToSend = new byte[_maxOutputReportLength];
            Array.Copy(message.FullMessage, dataToSend,
                Math.Min(message.FullMessage.Length, _maxOutputReportLength));

            await _hidStream.WriteAsync(dataToSend, 0, dataToSend.Length);

            if (delayBetweenMessages > 0)
                await Task.Delay(delayBetweenMessages);
        }

        // Wait for response with timeout
        byte[] buffer = new byte[_maxInputReportLength];
        using (var cts = new CancellationTokenSource(timeout))
        {
            int bytesRead = await _hidStream.ReadAsync(buffer, 0, buffer.Length, cts.Token);

            if (bytesRead > 0)
            {
                List<byte> response = new List<byte>(buffer.Take(bytesRead));
                return (true, response);
            }
        }
    }
    catch (OperationCanceledException)
    {
        // Timeout occurred
        if (retryOnError && maxRetries > 0)
        {
            // Retry logic
        }
    }

    return (false, null);
}
```

**Recursos:**
- Timeout configurável (padrão 2000ms)
- Retry automático em caso de falha
- CancellationToken para timeout assíncrono
- Suporte a múltiplas mensagens em sequência

## Protocolo HID Byte-Based

### Estrutura da Mensagem
```
[CMD_H][CMD_L][PAYLOAD...][FLAG_H][FLAG_L]
```

- **CMD_H** (1 byte): Comando High Byte
- **CMD_L** (1 byte): Comando Low Byte
- **PAYLOAD** (N bytes): Dados do comando (tamanho variável)
- **FLAG_H** (1 byte): Flag High = 0x20 (fixo)
- **FLAG_L** (1 byte): Flag Low = **CMD_L** (IMPORTANTE: deve ser igual!)

### Exemplo de Mensagem
**LED Mode = 1 (Always On):**
```
0x00 0x00 0x01 0x20 0x00
│    │    │    │    └─ FLAG_L = 0x00 (igual a CMD_L)
│    │    │    └────── FLAG_H = 0x20 (fixo)
│    │    └─────────── PAYLOAD = 1 (LED Mode)
│    └──────────────── CMD_L = 0x00
└───────────────────── CMD_H = 0x00
```

### Transmissão HID
```csharp
// HID report sempre tem tamanho fixo (geralmente 64 bytes)
byte[] dataToSend = new byte[_maxOutputReportLength]; // Ex: 64 bytes

// Copy message to report buffer
Array.Copy(message.FullMessage, dataToSend,
    Math.Min(message.FullMessage.Length, _maxOutputReportLength));

// Remaining bytes are zeros (padding)
// Send via HID stream
await _hidStream.WriteAsync(dataToSend, 0, dataToSend.Length);
```

## Dispositivos Implementados

### Dispositivos com Protocolo Byte-Based
| Dispositivo | Arquivo | Request/Response | Keep-Alive |
|-------------|---------|------------------|------------|
| JJB999 | `Class/Devices/JJB999.cs` | Não | LED Mode 3s |
| JJB01_V2 | `Class/Devices/JJB01_V2 .cs` | Não | LED Mode 3s |
| JJM01 | `Class/Devices/JJM01.cs` | **Sim** | Request All Inputs |
| JJBP06 | `Class/Devices/JJBP06.cs` | Não | LED Mode 3s |
| JJBSlim_A | `Class/Devices/JJBSlim_A.cs` | Não | LED Mode 3s |
| JJSD01 | `Class/Devices/JJSD01.cs` | Não | - |
| JJLC01 | `Class/Devices/JJLC01.cs` | Não | - |
| JJDB01 | `Class/Devices/JJDB01.cs` | Não | LED Mode 3s |

## Arquivos Relacionados
- **Classe Base:** `Class/Devices/Connections/HID.cs`
- **Message Class:** `Class/Devices/Connections/HIDMessage.cs`

## Métodos Principais

### SendHIDBytes
```csharp
public async Task SendHIDBytes(
    List<HIDMessage> messages,
    bool retryOnError = false,
    int delayBetweenMessages = 0,
    int timeout = 2000,
    int maxRetries = 5)
```
- Envia mensagens HID sem aguardar resposta
- Usado por: JJB999, JJB01_V2, JJDB01

### RequestHIDBytes
```csharp
public async Task<(bool success, List<byte> responseBytes)> RequestHIDBytes(
    List<HIDMessage> messages,
    bool retryOnError = false,
    int delayBetweenMessages = 0,
    int timeout = 2000,
    int maxRetries = 5)
```
- Envia mensagens HID e aguarda resposta
- Usado por: JJM01
- Timeout configurável
- Retry automático

### GetFirmwareVersion
```csharp
public async Task<string> GetFirmwareVersion()
{
    List<HIDMessage> messages = new List<HIDMessage>();
    messages.Add(new HIDMessage(0x00FF, new byte[] { 0x00 }));

    (bool success, List<byte> responseBytes) = await RequestHIDBytes(messages, false, 0, 2000, 5);

    if (success && responseBytes != null && responseBytes.Count >= 5)
    {
        // Parse version bytes
        // ...
    }

    return "0.0.0.0";
}
```

## Migrações

### Device.Net → HidSharp (2025-12)

**Motivo:**
- Maior estabilidade
- Melhor tratamento de timeouts
- Menos dependências
- Código mais limpo

**Dispositivos Migrados:**
- JJM01, JJB999, JJB01_V2, JJBP06, JJBSlim_A
- JJSD01, JJLC01, JJDB01

**Mudanças Principais:**
- `DeviceStream` → `HidStream`
- API assíncrona nativa (`WriteAsync`, `ReadAsync`)
- Melhor suporte a CancellationToken

## Debugging

### Captura de Mensagens HID
Use ferramentas como Wireshark com USBPcap ou monitor serial:

```
Enviando: 00 00 01 20 00  → LED Mode = 1
Enviando: 00 01 64 20 01  → Brightness = 100 (0x64)
Recebendo: 64 64 64 64 64  → JJM01: 5 inputs com volume 100
```

### Log de Erros
Apenas logs em catch blocks são mantidos:
```csharp
catch (Exception ex)
{
    Log.Insert("HID", "Erro ao enviar comando HID", ex);
}
```

## Performance

- **Latência típica:** < 50ms (envio + resposta)
- **Throughput:** Limitado por HID spec (~1000 reports/s teórico)
- **Uso prático:** 20-50 reports/s (mais que suficiente)
- **Rate limiting:** Implementado nos device classes (ex: JJM01 50ms entre requests)

## Notas Técnicas

### Big-Endian Command
Comandos são enviados em **big-endian** (byte mais significativo primeiro):
```csharp
message.Add((byte)((Command >> 8) & 0xFF));  // CMD_H
message.Add((byte)(Command & 0xFF));         // CMD_L
```

### Padding de Reports
HID reports têm tamanho fixo. Bytes restantes são preenchidos com zeros:
```csharp
byte[] dataToSend = new byte[64]; // Exemplo: report de 64 bytes
Array.Copy(message, dataToSend, message.Length);
// dataToSend[message.Length ... 63] = 0x00 (padding automático)
```

### Thread Safety
`SendHIDBytes` e `RequestHIDBytes` não são thread-safe. Dispositivos devem serializar chamadas (ex: flags `_sending`, `_requesting` no JJM01).

## Sistema de FLAGS Detalhado

### Tipos de FLAGS

| FLAG | Hexadecimal | Uso | Descrição |
|------|-------------|-----|-----------|
| **CONTINUE** | 0x1001 | Chunking | Mais chunks virão (mensagens grandes) |
| **END (JJDB-01)** | 0x2001 | JJDB-01 | Último chunk / mensagem completa |
| **END (JJB01_V2)** | 0x20FF | JJB01_V2 | Último chunk / mensagem completa |
| **END (Geral)** | 0x20[CMD_L] | Maioria | FLAG_L = CMD_L |

### Detecção de FLAGS

```csharp
// Extrai flags dos últimos 2 bytes do payload
if (payload.Length >= 2)
{
    flags = (ushort)((payload[payload.Length - 2] << 8) | payload[payload.Length - 1]);

    // Check if these are valid flags
    if (flags == 0x1001 || flags == 0x2001 || flags == 0x20FF ||
        (flags & 0xF000) == 0x1000 || (flags & 0xF000) == 0x2000)
    {
        dataLength = payload.Length - 2;  // Remove flags do payload
        continueReading = (flags == 0x1001);  // CONTINUE flag
    }
}
```

## Comandos Específicos Detalhados

### LED Mode (0x0000)

Define o modo de operação do LED.

**Estrutura:**
```
[0x00][0x00][MODE][0x20][CMD_L]
```

**Modos Suportados:**
| Valor | Nome | Descrição |
|-------|------|-----------|
| 0x00 | OFF | LED completamente apagado |
| 0x01 | ON | LED sempre aceso |
| 0x02 | BLINK | LED pisca on/off |
| 0x03 | PULSE | LED fade in/out |
| 0x04 | SimHub Sync | LED controlado por outputs do SimHub |

**Implementação Firmware (Arduino):**
```cpp
if (cmd == 0x0000) {
    if (payloadLength >= 1) {
        uint8_t ledMode = payload[0];
        jjled.changeLedMode(ledMode);
    }
}
```

**Implementação JJManager (C#):**
```csharp
List<byte> ledModeData = new List<byte>
{
    0x00, 0x00,           // CMD: 0x0000
    (byte)ledMode,        // Mode value (0-4)
    0x20, 0x00            // FLAGS (0x20, CMD_L)
};
await SendHIDBytes(ledModeData, false, 0, 2000, 5);
```

### Brightness (0x0001)

Define o brilho do LED (PWM).

**Estrutura:**
```
[0x00][0x01][BRIGHTNESS][0x20][0x01]
```

**Parâmetros:**
- **BRIGHTNESS** (1 byte): 0-255
  - 0x00 = 0% (apagado)
  - 0xFF = 100% (brilho máximo)

**Implementação Firmware:**
```cpp
else if (cmd == 0x0001) {
    uint8_t brightness = payload[0];
    jjled.changeBrightness(brightness);
}
```

**Implementação JJManager:**
```csharp
List<byte> brightnessData = new List<byte>
{
    0x00, 0x01,           // CMD: 0x0001
    (byte)brightness,     // 0-255
    0x20, 0x01            // FLAGS
};
await SendHIDBytes(brightnessData, false, 0, 2000, 5);
```

### Blink Speed (0x0002)

Define a velocidade do efeito BLINK.

**Estrutura:**
```
[0x00][0x02][SPEED][0x20][0x02]
```

**Parâmetros:**
- **SPEED** (1 byte): 0-255
  - Valores menores = mais rápido
  - Valores maiores = mais lento

**Implementação:**
```cpp
// Firmware
else if (cmd == 0x0002) {
    uint8_t blinkSpeed = payload[0];
    jjled.changeBlinkSpeed(blinkSpeed);
}
```

### Pulse Delay (0x0003)

Define o delay do efeito PULSE.

**Estrutura:**
```
[0x00][0x03][DELAY][0x20][0x03]
```

**Parâmetros:**
- **DELAY** (1 byte): 0-255
  - Valores menores = transição rápida
  - Valores maiores = transição suave

**Implementação:**
```cpp
// Firmware
else if (cmd == 0x0003) {
    uint8_t pulseDelay = payload[0];
    jjled.changePulseDelay(pulseDelay);
}
```

### Device Info Request (0x00FF)

Solicita informações do dispositivo.

**Estrutura Request:**
```
[0x00][0xFF][INFO_TYPE][0x20][0xFF]
```

**INFO_TYPE:**
- 0x00 = Firmware Version
- 0x01 = Hardware Version

**Estrutura Response:**
```
[0x00][0xFF][ASCII_DATA...][0x20][0xFF]
```

**Exemplo:**
```
Request:  0x00 0xFF 0x00 0x20 0xFF
Response: 0x00 0xFF "2024.11.29" 0x20 0xFF
```

**Implementação Firmware (envio):**
```cpp
void JJHID::sendToJJManager(uint16_t cmd, uint8_t *buffer, size_t length) {
    const size_t HEADER_SIZE = 2;
    const size_t FLAGS_SIZE = 2;

    size_t fullLength = HEADER_SIZE + length + FLAGS_SIZE;
    uint8_t *fullBuffer = (uint8_t *)malloc(fullLength);

    // Add CMD (big-endian)
    fullBuffer[0] = (cmd >> 8) & 0xFF;  // CMD_H
    fullBuffer[1] = cmd & 0xFF;         // CMD_L

    // Copy data
    memcpy(fullBuffer + HEADER_SIZE, buffer, length);

    // Add FLAGS
    fullBuffer[HEADER_SIZE + length] = 0x20;
    fullBuffer[HEADER_SIZE + length + 1] = cmd & 0xFF;  // FLAG_L = CMD_L

    // Send via RawHID (64-byte chunks)
    RawHID.write(fullBuffer, fullLength);

    free(fullBuffer);
}
```

**Implementação JJManager (recebimento):**
```csharp
public async Task<string> GetFirmwareVersion()
{
    List<HIDMessage> messages = new List<HIDMessage>();
    messages.Add(new HIDMessage(0x00FF, new byte[] { 0x00 }));

    (bool success, List<byte> responseBytes) = await RequestHIDBytes(messages, false, 0, 2000, 5);

    if (success && responseBytes != null && responseBytes.Count >= 5)
    {
        // Remove CMD (2 bytes) and FLAGS (2 bytes)
        int dataStart = 2;  // After CMD_H, CMD_L
        int dataEnd = responseBytes.Count - 2;  // Before FLAG_H, FLAG_L

        // Extract ASCII data
        List<byte> versionBytes = responseBytes.GetRange(dataStart, dataEnd - dataStart);
        string version = Encoding.ASCII.GetString(versionBytes.ToArray()).Trim('\0');

        return version;
    }

    return "0.0.0.0";
}
```

## Chunking Support (Mensagens Grandes)

O protocolo suporta chunking para mensagens que não cabem em um único pacote HID de 64 bytes.

### Transmissão em Chunks (Firmware → JJManager)

```cpp
// Envia em blocos de 64 bytes
while (bytesSent < fullLength) {
    memset(hid_buffer, 0x00, 64);
    size_t chunkSize = min(64, fullLength - bytesSent);
    memcpy(hid_buffer, fullBuffer + bytesSent, chunkSize);
    RawHID.write(hid_buffer, 64);
    bytesSent += 64;
}
```

### Recepção de Chunks (JJManager)

```csharp
List<byte> assembledData = new List<byte>();
bool continueReading = true;

while (continueReading)
{
    await stream.ReadAsync(bytesToRead, 0, bytesToRead.Length);

    // Extract payload
    byte[] payload = bytesToRead.Skip(3).ToArray();  // Skip CMD

    // Check flags in last 2 bytes
    if (payload.Length >= 2)
    {
        flags = (ushort)((payload[payload.Length - 2] << 8) | payload[payload.Length - 1]);

        // CONTINUE (0x1001) = keep reading
        // END (0x2001/0x20FF) = last chunk
        continueReading = (flags == 0x1001);

        // Remove flags from payload
        int dataLength = payload.Length - 2;
        assembledData.AddRange(payload.Take(dataLength));
    }
}

// Convert assembled data to string/object
string result = Encoding.ASCII.GetString(assembledData.ToArray());
```

## Diferenças Entre Dispositivos

### JJDB-01 vs JJB01_V2 vs JJM01

| Aspecto | JJDB-01 | JJB01_V2 | JJM01 |
|---------|---------|----------|-------|
| **FLAG de fim** | 0x2001 | 0x20FF | 0x20[CMD_L] |
| **Número de LEDs** | 140 (RGB) | 1 (PWM) | 0 |
| **Comandos LED** | RGB Set | LED Mode, Brightness, Blink, Pulse | N/A |
| **Request/Response** | Não | Não | **Sim** |
| **Device Info** | 0x00FF | 0x00FF | 0x00FF |
| **Chunking** | Sim | Sim | Sim |
| **Keep-Alive** | LED Mode 3s | LED Mode 3s | Request All Inputs |

### Comandos por Dispositivo

**JJB01_V2 / JJB999:**
- 0x0000: LED Mode
- 0x0001: Brightness
- 0x0002: Blink Speed
- 0x0003: Pulse Delay
- 0x00FF: Device Info

**JJM01:**
- 0x0001: Set Input Name
- 0x0002: Request All Inputs (Request/Response)
- 0x00FF: Device Info

**JJDB01:**
- 0x0000: Set RGB LEDs
- 0x0001: Set Brightness Global
- 0x00FF: Device Info

## Troubleshooting

### Problema: Firmware Version não é detectada

**Sintomas:**
- Dispositivo conecta mas versão aparece como "desconhecido"
- Log mostra timeout na comunicação HID

**Possíveis Causas:**
1. Firmware não está enviando resposta com flags corretas
2. JJManager não está interpretando flags corretamente
3. Timeout muito curto (< 2000ms)

**Soluções:**
```csharp
// Aumentar timeout
(bool success, List<byte> responseBytes) = await RequestHIDBytes(messages, false, 0, 5000, 5);

// Adicionar logs de debug
Log.Insert("HID", $"Response bytes: {BitConverter.ToString(responseBytes.ToArray())}");

// Verificar flags no firmware
Serial.print("Sending flags: 0x20 0x");
Serial.println(cmd & 0xFF, HEX);
```

### Problema: LED não responde a comandos

**Sintomas:**
- Comandos são enviados mas LED não muda
- Log HID mostra envio bem-sucedido

**Possíveis Causas:**
1. Pin do LED não configurado corretamente
2. `convertMessage()` não está parseando comando
3. `executeLedFunctions()` não está no loop

**Soluções:**
```cpp
// Verificar setup
void setup() {
    jjhid.init();
    jjhid.setLedPin(LED_PIN);  // Importante!
}

// Verificar loop
void loop() {
    jjhid.receiveData();
    jjhid.checkTimeout();
    jjhid.executeLedFunctions();  // Importante!
}

// Adicionar logs
void convertMessage(...) {
    Serial.print("Received CMD: 0x");
    Serial.println(cmd, HEX);
}
```

### Problema: Timeout Frequente

**Sintomas:**
- Comandos frequentemente falham com timeout
- Conexão instável

**Possíveis Causas:**
1. HID buffer muito pequeno
2. Firmware lento para processar
3. USB hub com baixa energia

**Soluções:**
```csharp
// Aumentar timeout e retries
await SendHIDBytes(messages, retryOnError: true, timeout: 5000, maxRetries: 10);

// Adicionar delay entre mensagens
await SendHIDBytes(messages, delayBetweenMessages: 100);

// Verificar energia USB
// Testar em porta USB direta (não hub)
```

### Problema: Chunking não funciona

**Sintomas:**
- Mensagens grandes são truncadas
- Apenas primeiro chunk é recebido

**Possíveis Causas:**
1. Flags CONTINUE (0x1001) não estão sendo usadas
2. Loop de recepção para no primeiro chunk
3. Firmware não está enviando múltiplos chunks

**Soluções:**
```cpp
// Firmware: Usar flags CONTINUE
if (bytesSent < fullLength - 64) {
    fullBuffer[HEADER_SIZE + 64 - 2] = 0x10;  // CONTINUE flag
    fullBuffer[HEADER_SIZE + 64 - 1] = 0x01;
}
```

```csharp
// JJManager: Loop até END flag
while (continueReading)
{
    // ... read chunk ...
    continueReading = (flags == 0x1001);  // Check CONTINUE flag
}
```

## Diagramas de Sequência

### Inicialização e Detecção de Versão
```
JJManager                    Firmware
   |                            |
   |--[0x00FF][0x00][0x20FF]--->|  Request FW Version
   |                            |
   |                            |  Parse request
   |                            |  Prepare response
   |                            |
   |<--[0x00FF]["2024.11.29"]---|  Response
   |         [0x20FF]            |
   |                            |
   |  Parse version string      |
   |  "2024.11.29"              |
   |                            |
```

### Envio de LED Mode com Change Tracking
```
JJManager                    Firmware
   |                            |
   | ledMode = 1                |
   | _lastSentLedMode = -1      |
   |                            |
   | if (ledMode != _lastSent)  |
   |--[0x00][0x00][0x01]------->|  Send LED Mode = ON
   |         [0x20][0x00]       |
   |                            |
   | _lastSentLedMode = 1       |  jjled.changeLedMode(1)
   |                            |  LED turns ON
   |                            |
   | ... no change ...          |
   | (não envia novamente)      |
   |                            |
```

### Request/Response (JJM01)
```
JJManager                    Firmware
   |                            |
   |--[0x00][0x02][]---------→|  Request All Inputs
   |         [0x20][0x02]       |
   |                            |
   |                            |  Read 5 encoders
   |                            |  [100, 80, 60, 40, 20]
   |                            |
   |<--[V0][V1][V2][V3][V4]-----|  Response (raw data)
   |    64  50  3C  28  14      |
   |                            |
   |  Parse 5 volume values     |
   |  Apply to AudioController  |
   |                            |
```

## Exemplos de Uso

### Exemplo 1: Enviar LED Mode
```csharp
// JJManager
List<HIDMessage> messages = new List<HIDMessage>();
messages.Add(new HIDMessage(0x0000, new byte[] { 0x01 }));  // LED Mode = ON
await SendHIDBytes(messages, false, 0, 2000, 5);
```

### Exemplo 2: Request Firmware Version
```csharp
// JJManager
List<HIDMessage> messages = new List<HIDMessage>();
messages.Add(new HIDMessage(0x00FF, new byte[] { 0x00 }));  // FW Version
(bool success, List<byte> response) = await RequestHIDBytes(messages, false, 0, 2000, 5);

if (success) {
    string version = ParseVersionFromResponse(response);
    Console.WriteLine($"Firmware: {version}");
}
```

### Exemplo 3: Request All Inputs (JJM01)
```csharp
// JJManager
List<HIDMessage> messages = new List<HIDMessage>();
messages.Add(new HIDMessage(0x0002, new byte[0]));  // Empty payload
(bool success, List<byte> response) = await RequestHIDBytes(messages, false, 0, 2000, 2);

if (success && response.Count >= 5) {
    for (int i = 0; i < 5; i++) {
        byte volume = response[i];
        Console.WriteLine($"Input {i}: {volume}%");
    }
}
```

### Exemplo 4: Firmware - Processar Comandos
```cpp
// Firmware
void JJHID::convertMessage(const uint8_t *buffer, size_t length) {
    if (length < 2) return;

    uint16_t cmd = (uint16_t(buffer[0]) << 8) | buffer[1];
    const uint8_t *payload = buffer + 2;
    size_t payloadLength = length - 4;  // Remove CMD (2) + FLAGS (2)

    if (cmd == 0x0000) {
        // LED Mode
        uint8_t ledMode = payload[0];
        jjled.changeLedMode(ledMode);
    }
    else if (cmd == 0x0001) {
        // Brightness
        uint8_t brightness = payload[0];
        jjled.changeBrightness(brightness);
    }
    else if (cmd == 0x00FF) {
        // Device Info
        uint8_t infoType = payload[0];
        if (infoType == 0x00) {
            sendToJJManager(0x00FF, (uint8_t*)FIRMWARE_VERSION, strlen(FIRMWARE_VERSION));
        }
    }
}
```
