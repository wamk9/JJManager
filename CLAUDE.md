# JJManager CrossPlatform - Contexto de Desenvolvimento

> Documentacao do projeto JJManager CrossPlatform para desenvolvimento com Claude Code
> Ultima atualizacao: 2026-02-18

---

## Indice

1. [Visao Geral](#visao-geral)
2. [Estrutura do Projeto](#estrutura-do-projeto)
3. [Arquitetura](#arquitetura)
4. [Configuracao e Build](#configuracao-e-build)
5. [Dependencias](#dependencias)
6. [Database](#database)
7. [Dispositivos Suportados](#dispositivos-suportados)
8. [Protocolo de Comunicacao HID](#protocolo-de-comunicacao-hid)
9. [Projeto Legacy (Windows Forms)](#projeto-legacy-windows-forms)

---

## Visao Geral

**JJManager CrossPlatform** e a nova geracao do software de gerenciamento de dispositivos JohnJohn3D, reescrito para suportar multiplas plataformas.

### Informacoes Basicas

| Item | Valor |
|------|-------|
| **Versao** | 2.0.0 |
| **Framework** | .NET 9 |
| **UI** | Avalonia UI 11.2 |
| **Plataformas** | Windows, Linux |
| **Database** | SQLite + EF Core 9 |
| **Arquitetura** | Clean Architecture + MVVM |
| **Empresa** | JohnJohn 3D |
| **Website** | https://johnjohn3d.com.br |

### Caracteristicas Principais

- Interface moderna com Avalonia UI e tema Fluent
- Suporte a Windows e Linux (x64, arm64)
- Arquitetura Clean Architecture com injecao de dependencias
- Database SQLite cross-platform
- Sistema de localizacao (pt-BR, en-US, es-ES)
- Suporte a temas (Light/Dark) com cores de acento customizaveis
- Comunicacao HID com dispositivos via HidSharp
- Integracao com SimHub para simuladores de corrida

---

## Estrutura do Projeto

```
GitHub - JJManager/
├── JJManager.Core/              # Biblioteca core (logica de negocio)
│   ├── Audio/                   # Interface de audio (IAudioManager)
│   ├── Connections/             # Conexoes externas
│   │   └── SimHub/              # WebSocket SimHub + Properties
│   ├── Database/                # Entidades do banco
│   │   └── Entities/            # ConfigEntity, ProfileEntity, etc.
│   ├── Devices/                 # Dispositivos
│   │   ├── Abstractions/        # IDeviceProbe, HidDeviceProbe
│   │   ├── Connections/         # HidDevice, HidMessage
│   │   ├── Factories/           # JJDeviceFactory
│   │   ├── Generic/             # GenericHidDevice
│   │   ├── JJDB01/              # Dashboard JJDB-01
│   │   ├── JJLC01/              # LoadCell JJLC-01
│   │   ├── JJDevice.cs          # Classe base abstrata
│   │   └── DeviceType.cs        # Enums de conexao/estado
│   ├── Interfaces/              # Contratos
│   │   ├── Repositories/        # IConfigRepository, IProductRepository, etc.
│   │   └── Services/            # IDeviceService, IAppConfigService, etc.
│   ├── Others/                  # Utilitarios (HashHelper, ComparativeItem)
│   ├── Profile/                 # DeviceProfile
│   └── Services/                # Servicos compartilhados
│
├── JJManager.Desktop/           # Aplicacao Avalonia UI
│   ├── Assets/                  # Recursos
│   │   ├── Devices/             # Imagens dos dispositivos
│   │   └── i18n/                # Arquivos de localizacao (JSON)
│   ├── Converters/              # Conversores XAML
│   ├── Services/                # Servicos de UI
│   │   ├── LocalizationService.cs
│   │   ├── ThemeService.cs
│   │   ├── DeviceWindowFactory.cs
│   │   └── SimHubPropertiesService.cs
│   ├── Styles/                  # Estilos AXAML
│   ├── ViewModels/              # ViewModels (MVVM)
│   │   ├── Devices/             # VMs especificos de dispositivos
│   │   ├── MainWindowViewModel.cs
│   │   ├── DevicesViewModel.cs
│   │   ├── SettingsViewModel.cs
│   │   └── UpdatesViewModel.cs
│   ├── Views/                   # Views AXAML
│   │   ├── Devices/             # Janelas de dispositivos
│   │   ├── MainWindow.axaml
│   │   ├── DevicesView.axaml
│   │   ├── SettingsView.axaml
│   │   └── UpdatesView.axaml
│   ├── App.axaml                # Application resources
│   ├── App.axaml.cs             # Entry point + DI configuration
│   └── Program.cs               # Main
│
├── JJManager.Infrastructure/    # Camada de infraestrutura
│   ├── Database/                # DbContext
│   │   └── JJManagerDbContext.cs
│   ├── Repositories/            # Implementacoes dos repositorios
│   ├── Services/                # Implementacoes dos servicos
│   └── DependencyInjection.cs   # Extensoes de DI
│
├── JJManager.Platform.Windows/  # Implementacoes Windows
│   └── Audio/                   # WindowsAudioManager (NAudio)
│
├── JJManager.Platform.Linux/    # Implementacoes Linux
│   └── Audio/                   # LinuxAudioManager (PulseAudio)
│
├── Legacy/                      # Projeto Windows Forms original (v1.x)
│   ├── JJManager/               # Codigo fonte WinForms
│   ├── Context/                 # Documentacao de contexto
│   └── JJManager.sln            # Solution antiga
│
├── JJManager.CrossPlatform.sln  # Solution principal
├── CLAUDE.md                    # Este arquivo
└── README.md                    # README do projeto
```

---

## Arquitetura

### Clean Architecture

O projeto segue Clean Architecture com 4 camadas:

```
┌─────────────────────────────────────────────┐
│              JJManager.Desktop              │  ← UI (Avalonia + MVVM)
├─────────────────────────────────────────────┤
│           JJManager.Infrastructure          │  ← EF Core, Repositories
├─────────────────────────────────────────────┤
│    Platform.Windows  │  Platform.Linux      │  ← Implementacoes de plataforma
├─────────────────────────────────────────────┤
│               JJManager.Core                │  ← Entidades, Interfaces, Devices
└─────────────────────────────────────────────┘
```

### Injecao de Dependencias

Configurada em `App.axaml.cs`:

```csharp
private void ConfigureServices(IServiceCollection services)
{
    // Infrastructure (EF Core + SQLite + Repositories)
    var dbPath = DependencyInjection.GetDefaultDatabasePath();
    services.AddInfrastructure(dbPath);

    // Device Probe
    services.AddSingleton<IDeviceProbe<HidSharp.HidDevice>, HidDeviceProbe>();

    // Services
    services.AddSingleton<ILocalizationService, LocalizationService>();
    services.AddSingleton<IThemeService, ThemeService>();
    services.AddSingleton<IDeviceService, DeviceService>();

    // ViewModels
    services.AddTransient<MainWindowViewModel>();
    services.AddTransient<DevicesViewModel>();
    services.AddTransient<SettingsViewModel>();
}
```

### Classe Base JJDevice

Todos os dispositivos herdam de `JJDevice`:

```csharp
public abstract class JJDevice : INotifyPropertyChanged, IDisposable
{
    // Propriedades principais
    public string ConnId { get; }
    public string ProductName { get; }
    public DeviceConnectionState ConnectionState { get; }
    public Version? FirmwareVersion { get; }
    public DeviceProfile? Profile { get; set; }

    // Metodos abstratos (implementados por cada dispositivo)
    public abstract Task<bool> ConnectAsync(CancellationToken ct);
    public abstract Task<bool> DisconnectAsync(CancellationToken ct);
    public abstract Task<Version?> GetFirmwareVersionAsync(CancellationToken ct);
    protected abstract Task DataLoopAsync(CancellationToken ct);
}
```

---

## Configuracao e Build

### Requisitos

**Windows:**
- .NET 9 SDK
- Visual Studio 2022 ou VS Code

**Linux:**
- .NET 9 SDK
- libpulse0 (para controle de audio)
- libudev1 (para HID)

### Comandos de Build

```bash
# Restaurar dependencias
dotnet restore JJManager.CrossPlatform.sln

# Build Windows (desenvolvimento)
dotnet build JJManager.Desktop -f net9.0-windows -c Debug

# Build Linux (desenvolvimento)
dotnet build JJManager.Desktop -f net9.0 -c Debug

# Publish Windows (release)
dotnet publish JJManager.Desktop -f net9.0-windows -c Release -r win-x64 --self-contained

# Publish Linux x64 (release)
dotnet publish JJManager.Desktop -f net9.0 -c Release -r linux-x64 --self-contained

# Publish Linux ARM64 (release)
dotnet publish JJManager.Desktop -f net9.0 -c Release -r linux-arm64 --self-contained
```

### Estrutura de Output

```
bin/Release/net9.0-windows/win-x64/publish/
├── JJManager.exe
├── JJManager.dll
├── JJManager.Core.dll
├── JJManager.Infrastructure.dll
├── JJManager.Platform.Windows.dll
└── [dependencias]

bin/Release/net9.0/linux-x64/publish/
├── JJManager
├── JJManager.dll
├── JJManager.Core.dll
├── JJManager.Infrastructure.dll
├── JJManager.Platform.Linux.dll
└── [dependencias]
```

---

## Dependencias

### JJManager.Core

| Pacote | Versao | Proposito |
|--------|--------|-----------|
| HidSharp | 2.1.0 | Comunicacao HID |
| Newtonsoft.Json | 13.0.3 | Serializacao JSON |
| System.IO.Ports | 9.0.0 | Comunicacao serial |
| System.Reactive | 6.0.1 | Reactive Extensions |

### JJManager.Desktop

| Pacote | Versao | Proposito |
|--------|--------|-----------|
| Avalonia | 11.2.0 | UI Framework |
| Avalonia.Desktop | 11.2.0 | Desktop support |
| Avalonia.Themes.Fluent | 11.2.0 | Tema Fluent |
| Avalonia.Fonts.Inter | 11.2.0 | Fonte Inter |
| Material.Icons.Avalonia | 2.1.10 | Icones Material |
| CommunityToolkit.Mvvm | 8.3.2 | MVVM Toolkit |
| Microsoft.Extensions.DependencyInjection | 9.0.0 | DI Container |

### JJManager.Infrastructure

| Pacote | Versao | Proposito |
|--------|--------|-----------|
| Microsoft.EntityFrameworkCore.Sqlite | 9.0.0 | SQLite + EF Core |
| Microsoft.EntityFrameworkCore.Design | 9.0.0 | EF Migrations |

### JJManager.Platform.Windows

| Pacote | Versao | Proposito |
|--------|--------|-----------|
| NAudio | 2.2.1 | Audio playback/recording |
| NAudio.Wasapi | 2.2.1 | WASAPI support |
| SharpDX | 4.2.0 | DirectX wrapper |
| SharpDX.DirectInput | 4.2.0 | Joystick support |

### JJManager.Platform.Linux

- Usa P/Invoke para libpulse (PulseAudio/PipeWire)
- Dependencia runtime: `libpulse0`

---

## Database

### Tipo e Localizacao

- **Engine:** SQLite
- **ORM:** Entity Framework Core 9

**Localizacao do arquivo:**

| Plataforma | Caminho |
|------------|---------|
| Windows | `%APPDATA%\JohnJohn3D\JJManager\jjmanager.db` |
| Linux | `~/.config/JohnJohn3D/JJManager/jjmanager.db` |
| macOS | `~/Library/Application Support/JohnJohn3D/JJManager/jjmanager.db` |

### Entidades

```
ConfigEntity      - Configuracoes chave-valor
ProductEntity     - Produtos/dispositivos cadastrados
ProfileEntity     - Perfis de configuracao
UserProductEntity - Dispositivos do usuario
InputEntity       - Configuracoes de inputs
OutputEntity      - Configuracoes de outputs
```

### Migrations

```bash
# Criar nova migration
dotnet ef migrations add NomeDaMigration -p JJManager.Infrastructure -s JJManager.Desktop

# Aplicar migrations
dotnet ef database update -p JJManager.Infrastructure -s JJManager.Desktop
```

---

## Dispositivos Suportados

### Dispositivos Implementados (v2.0)

| Dispositivo | Classe | Conexao | Status |
|-------------|--------|---------|--------|
| **JJDB-01** (Dashboard) | JJDB01Device | HID | Implementado |
| **JJLC-01** (LoadCell) | JJLC01Device | HID | Implementado |

### Dispositivos Planejados

| Dispositivo | Tipo | Conexao |
|-------------|------|---------|
| JJM-01 | Mixer de Audio | HID |
| JJB-01 V2 | ButtonBox | HID |
| JJB-999 | ButtonBox | HID |
| JJB-Slim A | ButtonBox | HID |
| JJSD-01 | StreamDeck | HID |

### Tipos de Conexao

```csharp
public enum DeviceConnectionType
{
    Unset,
    HID,       // HidSharp
    Joystick,  // DirectInput (Windows only)
    Bluetooth, // InTheHand.Bluetooth
    Serial     // System.IO.Ports
}

public enum DeviceConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Disconnecting,
    Error
}
```

---

## Protocolo de Comunicacao HID

### Formato da Mensagem

```
[CMD_H][CMD_L][PAYLOAD...][FLAG_H][FLAG_L]
```

- **CMD_H, CMD_L:** Comando 16-bit big-endian
- **PAYLOAD:** Dados do comando (0+ bytes)
- **FLAG_H:** Sempre 0x20
- **FLAG_L:** Igual a CMD_L (validacao)

### Comandos Padrao

| Comando | CMD | Payload | Descricao |
|---------|-----|---------|-----------|
| LED Mode | 0x0000 | 1 byte (0-4) | Modo do LED |
| Brightness | 0x0001 | 1 byte (0-255) | Brilho |
| Blink Speed | 0x0002 | 1 byte | Velocidade de piscada |
| Pulse Delay | 0x0003 | 1 byte | Delay do pulso |
| Device Info | 0x00FF | 1 byte (tipo) | Requisicao de info |

### Modos LED

| Valor | Modo |
|-------|------|
| 0 | Desligado |
| 1 | Sempre Ligado |
| 2 | Pulsando |
| 3 | Piscando |
| 4 | SimHub Sync |

### Keep-Alive

- **Firmware:** Timeout de 5 segundos sem comunicacao = LED OFF
- **JJManager:** Envia LED Mode a cada 3 segundos

---

## Projeto Legacy (Windows Forms)

O projeto original Windows Forms esta em `Legacy/` para referencia.

### Informacoes

| Item | Valor |
|------|-------|
| **Versao** | 1.3.0.1 |
| **Framework** | .NET Framework 4.7.2 |
| **UI** | Windows Forms + MaterialSkin |
| **Database** | SQL Server LocalDB |

### Quando Usar

- Referencia para logica de dispositivos nao migrados
- Comparacao de comportamento
- Usuarios que precisam de funcionalidades ainda nao portadas

### Build Legacy

```bash
# Usando MSBuild
msbuild Legacy/JJManager.sln -p:Configuration=Release -p:Platform="Any CPU"
```

---

## Comandos Uteis

### Desenvolvimento

```bash
# Rodar aplicacao (Windows)
dotnet run --project JJManager.Desktop -f net9.0-windows

# Rodar aplicacao (Linux)
dotnet run --project JJManager.Desktop -f net9.0

# Limpar build
dotnet clean JJManager.CrossPlatform.sln

# Verificar warnings
dotnet build JJManager.CrossPlatform.sln -warnaserror
```

### Git

```bash
# Status
git status

# Commit (usar skill /commit)
/commit

# Atualizar versao (usar skill /update-version)
/update-version 2.0.1
```

---

## Localizacao

Arquivos em `JJManager.Desktop/Assets/i18n/`:

- `pt-BR.json` - Portugues (Brasil)
- `en-US.json` - Ingles (EUA)
- `es-ES.json` - Espanhol

### Adicionar Nova String

1. Adicionar chave em todos os arquivos JSON
2. Usar no XAML: `{Binding Localize[key]}`
3. Usar no C#: `App.Localization?["key"]`

---

## Contato

- **Website:** https://johnjohn3d.com.br
- **Email:** contato@johnjohn3d.com.br
- **Instagram:** @johnjohn.3d

---

*Documento atualizado em: 2026-02-18*
