# Contexto: ButtonBox JJB01_V2

## Visão Geral
ButtonBox com protocolo de comunicação byte-based via HID (migrado de JSON em 2025-12-25).

## Características
- **Firmware:** v2024.11.29
- **Conexão:** HID (HidSharp)
- **Protocolo:** Byte-based (similar ao JJDB-01)
- **LED:** 1 LED RGB controlável

## Histórico de Desenvolvimento

### 2025-12-25: Migração para Protocolo Byte-Based
**Objetivo:** Substituir protocolo JSON por byte-based para melhor performance e confiabilidade.

#### Mudanças no Firmware (Arduino)
- Removidas dependências JSON
- Parser byte-based em `JJHID.cpp`
- Keep-alive timeout de 5 segundos
- Comandos suportados: LED Mode, Brightness, Blink Speed, Pulse Delay, Device Info

#### Mudanças no JJManager (C#)
- Change tracking para otimizar envios HID
- Keep-alive a cada 3 segundos (LED Mode)
- SimHub Mode com prioridade de outputs (iteração reversa)
- Detecção automática de versão de firmware

### 2025-12-27: Correções de UI e Performance

#### FlowLayoutPanel Auto-Ordering
**Problema:** Outputs eram adicionados ao FlowLayoutPanel na ordem errada devido ao DefaultLayout.
**Solução:**
```csharp
// Remover DefaultLayout antes de adicionar
flowLayoutPanelOutputLeds.Controls.Remove(DefaultLayout);

// Adicionar layouts na ordem correta
foreach (var output in orderedOutputs)
{
    var layout = CreateOutputLayout(output);
    flowLayoutPanelOutputLeds.Controls.Add(layout);
}
```

#### Auto-Save em Sliders
**Problema:** Usuário tinha que clicar em Salvar após ajustar sliders.
**Solução:**
```csharp
sldLedBrightness.ValueChanged += (s, e) =>
{
    SaveConfig(closeWindow: false);
};
```

#### Slider Invertido
**Problema:** Brightness estava invertido (0=brilhante, 255=escuro).
**Solução:** `slider.Inverted = true`

#### Busy Loop Fix
**Problema:** CPU 6% devido a loop sem delay em `ActionMainLoop()`.
**Solução:**
```csharp
while (_isConnected)
{
    await SendData();
    await Task.Delay(2); // CPU: 6% → 1%
}
```

## Protocolo de Comunicação

### Estrutura Geral
```
[CMD_H][CMD_L][PAYLOAD...][FLAG_H][FLAG_L]
```

### Comandos
| CMD | Payload | Descrição |
|-----|---------|-----------|
| 0x0000 | 1 byte (0-4) | LED Mode: 0=Off, 1=On, 2=Pulse, 3=Blink, 4=SimHub |
| 0x0001 | 1 byte (0-255) | Brightness |
| 0x0002 | 1 byte (0-255) | Blink Speed |
| 0x0003 | 1 byte (0-255) | Pulse Delay |
| 0x00FF | 1 byte (tipo) | Device Info Request |

### Sistema Keep-Alive
- **Firmware:** Timeout de 5s → força LED Mode = 0
- **JJManager:** Envia LED Mode a cada 3s

### SimHub Mode (LED Mode = 4)
Quando ativado, itera outputs do perfil de **trás para frente**:
- **Último Output ativo prevalece** (maior prioridade)
- Verifica `output.Led.SetActivatedValue(value)`
- Primeiro ativo encontrado (iteração reversa) controla o LED

## Arquivos Relacionados
- **Device Class:** `Class/Devices/JJB01_V2 .cs`
- **UI Page:** `Pages/Devices/JJB01_V2 .Designer.cs`, `JJB01_V2 .cs`
- **Firmware:** `D:\OneDrive\JohnJohn3D\02 - Producao\01 - Sim Racing\18 - ButtonBox JJB-01 V2\02 - Source\02 - Programacao\Code_JJB01_V2_2025-12-25\`

## Documentação Adicional
- [Protocolo HID Completo](Connections/HID.md) - Especificação detalhada com comandos, FLAGS, chunking e troubleshooting

## Bugs Corrigidos

### Bug 1: Valores de perfil não carregavam na UI
**Causa:** `LoadFormData()` chamava valores antes de configurar `DataSource`.
**Solução:** Inverter ordem - `DataSource` primeiro, depois carregar valores.

### Bug 2: Exclusão de output não funcionava
**Causa:** Flag `Changed` não era marcada.
**Solução:** `Profile.cs` linha 85 - Marcar `Changed = true` explicitamente.

### Bug 3: Race condition em conexão/desconexão rápida
**Causa:** Tasks antigas não eram aguardadas antes de criar novas.
**Solução:**
```csharp
// JJDevice.cs - Connect()
if (_taskSendingData != null && !_taskSendingData.IsCompleted)
{
    await Task.WhenAny(_taskSendingData, Task.Delay(5000));
}

// Disconnect()
if (_taskReceivingData != null)
{
    await Task.WhenAny(_taskReceivingData, Task.Delay(5000));
}
```

## Notas de Performance
- **Latência de comunicação:** < 100ms (protocolo byte-based)
- **CPU em idle:** < 1% (com Task.Delay(2))
- **Keep-alive overhead:** Mínimo (1 comando a cada 3s)
