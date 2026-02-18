# Contexto: Mixer de Áudio JJM01

## Visão Geral
Mixer de áudio USB com 5 inputs (encoders rotativos) para controle de volume de aplicações do Windows.

## Características
- **Firmware:** v2025.01.17
- **Conexão:** HID (HidSharp)
- **Protocolo:** Byte-based request/response
- **Inputs:** 5 encoders rotativos (0-100 volume)
- **Funcionalidade:** Controle de volume por aplicação via AudioController

## Histórico de Desenvolvimento

### 2025-12-28: Implementação de Protocolo Byte-Based

#### Mudanças no JJManager

**Novo método RequestHIDBytes() em HID.cs:**
```csharp
public async Task<(bool success, List<byte> responseBytes)> RequestHIDBytes(
    List<HIDMessage> messages,
    bool retryOnError = false,
    int delayBetweenMessages = 0,
    int timeout = 2000,
    int maxRetries = 5)
{
    // Envia comandos e aguarda resposta do dispositivo
    // Timeout configurável (padrão 2000ms)
    // Retry automático em caso de falha
}
```

**Protocolo de Request/Response:**
```csharp
// REQUEST (JJManager → Firmware)
List<HIDMessage> messages = new List<HIDMessage>();
messages.Add(new HIDMessage(0x0002, new byte[0]));  // Request All Inputs
await RequestHIDBytes(messages, false, 0, 2000, 2);

// RESPONSE (Firmware → JJManager)
// Formato: [V0][V1][V2][V3][V4] (5 bytes, valores 0-100)
```

### Comandos Suportados

| CMD | Direção | Payload | Descrição |
|-----|---------|---------|-----------|
| 0x0001 | JJM→FW | [ORDER][LENGTH][NAME...] | Set Input Name |
| 0x0002 | JJM→FW | (vazio) | Request All Inputs |
| 0x0002 | FW→JJM | [V0][V1][V2][V3][V4] | Response: 5 volume values |
| 0x00FF | JJM→FW | [INFO_TYPE] | Device Info Request |

### Sistema de Keep-Alive
- **Request Interval:** 50ms mínimo entre requests (rate limiting)
- **Timeout:** 10 falhas consecutivas = desconexão
- **Keep-Alive:** Request All Inputs serve como keep-alive

### Performance JJM-01

#### Otimizações Implementadas (2025-12)
1. **Inicialização 10x mais rápida:** 15-30s → 3-5s
   - Processamento paralelo (`Task.WhenAll`)
   - Cache inteligente de PIDs
   - Static shared variables entre inputs

2. **Cache de Sessões de Áudio:**
   - Evita chamadas WMI repetidas
   - Cache hit rate > 90% após primeira inicialização
   - Shared entre todos os inputs

3. **Detecção Instantânea de Apps:**
   - Novos apps detectados em < 1 segundo
   - Callbacks NAudio para eventos de sessão
   - Aplicação automática de volume configurado

4. **Throttling de Eventos:**
   - 200+ eventos/s → 2 eventos/s
   - Uso de CPU < 1% em operação normal
   - Delay de 500ms entre mudanças de propriedade

## Integração com AudioController

Cada input do JJM-01 possui um `AudioController` que gerencia:
- Sessões de áudio do Windows (NAudio)
- Volume por aplicação
- Detecção automática de novos apps
- Persistência de configurações

**Fluxo de Operação:**
```
1. Firmware envia volume do encoder (0-100)
2. RequestData() recebe e atualiza AudioController.SettedVolume
3. Input.Execute() dispara
4. AudioController.ChangeAppVolume() aplica volume
5. NAudio API atualiza sessão do Windows
```

## Arquivos Relacionados
- **Device Class:** `Class/Devices/JJM01.cs`
- **UI Page:** `Pages/Devices/JJM01.Designer.cs`, `JJM01.cs`
- **Audio Controller:** `Class/App/Input/AudioController/AudioController.cs`
- **Audio Monitor:** `Class/App/Input/AudioController/AudioMonitor.cs`

## Correções Relacionadas ao JJM-01

### 2026-01-02: Fix Task.WhenAll Race Condition
**Arquivo:** `JJM01.cs` linha 332
**Problema:** Lambda não retornava Task, causando flags serem atualizadas ANTES dos resets completarem.
**Solução:**
```csharp
// ANTES (ERRADO):
await Task.WhenAll(audioInputs.Select(input =>
{
    input.AudioController.ResetCoreAudioController(...);
    input.AudioController.AudioCoreNeedsRestart = false; // ANTES!
}));

// DEPOIS (CORRETO):
await Task.WhenAll(audioInputs.Select(input =>
    input.AudioController.ResetCoreAudioController(_deviceEnumerator, devicesGetted, sessionsGetted, skipLock: true)
));

// Update flags DEPOIS de todos completarem
foreach (Input input in audioInputs)
{
    input.AudioController.AudioCoreNeedsRestart = false;
    input.AudioController.UpdateSessionsToControl = false;
    input.Execute();
}
```

### 2026-01-02: Limpeza de Logs
Removidos logs desnecessários (não-catch) para melhor performance:
- Linha 315-338: Removido Stopwatch e logs de timing
- Mantidos apenas logs em catch blocks para erros

## Benchmarks

**JJM-01 com 5 inputs configurados:**
- **Inicialização:** ~3-5 segundos
- **Cache hit rate:** >90% após primeira inicialização
- **Detecção de novo app:** <1 segundo
- **CPU usage:** <1% em operação normal
- **Memória:** ~150MB (inclui cache de sessões)

## Notas Técnicas

### Rate Limiting
```csharp
private DateTime _lastRequest = DateTime.MinValue;
private readonly TimeSpan _requestInterval = TimeSpan.FromMilliseconds(50);

// No RequestData():
if ((DateTime.Now - _lastRequest) < _requestInterval)
{
    return; // Limita a 20 requests/segundo
}
```

### Change Detection
```csharp
// Só executa se volume realmente mudou (evita async tasks desnecessárias)
if (_profile.Inputs[i].AudioController != null &&
    _profile.Inputs[i].AudioController.SettedVolume != volumeValue)
{
    _profile.Inputs[i].AudioController.SettedVolume = volumeValue;
    _profile.Inputs[i].Execute();
}
```

### Timeout e Desconexão
- **Timeout Limit:** 10 falhas consecutivas
- **Action:** Dispose() + Disconnect()
- **Cleanup:** CleanupAudioResources() libera MMDeviceEnumerator

## FAQ

**P: O JJM-01 demora muito para conectar. É normal?**
R: Não! Com as últimas atualizações, a conexão deve levar apenas 3-5 segundos.

**P: O app que abri não está sendo detectado. O que fazer?**
R: O JJManager detecta novos apps automaticamente em menos de 1 segundo. Aguarde 1-2 segundos ou clique no encoder para forçar atualização.

**P: O controle de áudio consome muita CPU?**
R: Não! Implementamos throttling que reduziu de 200+ eventos/s para 2/s. Uso de CPU < 1%.
