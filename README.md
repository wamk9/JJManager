# JJManager CrossPlatform

Versao cross-platform do JJManager desenvolvida com .NET 9 e Avalonia UI.

## Visao Geral

O **JJManager CrossPlatform** e a nova geracao do software de gerenciamento de dispositivos JohnJohn3D, reescrito para suportar multiplas plataformas (Windows e Linux).

| Caracteristica | Valor |
|----------------|-------|
| **Versao** | 2.0.0 |
| **Framework** | .NET 9.0 |
| **UI** | Avalonia 11.2 |
| **Plataformas** | Windows x64, Linux x64, Linux ARM64 |
| **Database** | SQLite (EF Core) |
| **Idiomas** | Portugues (BR), Ingles (US), Espanhol (ES) |

## Arquitetura

```
JJManager.CrossPlatform.sln
├── JJManager.Core              # Dominio, dispositivos, interfaces
├── JJManager.Infrastructure    # EF Core, SQLite, Repositories
├── JJManager.Desktop           # UI Avalonia (MVVM)
├── JJManager.Platform.Windows  # Audio (NAudio), DirectInput
└── JJManager.Platform.Linux    # Audio (PulseAudio/PipeWire)
```

## Requisitos

### Runtime Windows
- Windows 10/11 x64
- .NET 9.0 Runtime

### Runtime Linux
- Ubuntu 22.04+ / Debian 12+ / Fedora 38+
- .NET 9.0 Runtime
- `libpulse0` e `libhidapi-hidraw0`

## Build

```bash
# Restaurar dependencias
dotnet restore JJManager.CrossPlatform.sln

# Build Windows
dotnet build -f net9.0-windows -c Release

# Build Linux
dotnet build -f net9.0 -c Release
```

## Publicacao

```bash
# Windows x64
dotnet publish JJManager.Desktop -c Release -f net9.0-windows -r win-x64 --self-contained

# Linux x64
dotnet publish JJManager.Desktop -c Release -f net9.0 -r linux-x64 --self-contained

# Linux ARM64
dotnet publish JJManager.Desktop -c Release -f net9.0 -r linux-arm64 --self-contained
```

## Dispositivos Suportados

| Dispositivo | Tipo | Status |
|-------------|------|--------|
| JJDB-01 | Dashboard | Em desenvolvimento |
| JJLC-01 | Load Cell | Em desenvolvimento |
| JJM-01 | Audio Mixer | Planejado |
| JJSD-01 | StreamDeck | Planejado |
| JJB-01 V2 | ButtonBox | Planejado |
| JJB-999 | ButtonBox | Planejado |
| JJB-Slim A | ButtonBox | Planejado |

## Versao Legacy

A versao anterior do JJManager (Windows Forms, .NET Framework 4.7.2) esta disponivel na pasta `Legacy/`.

## Contato

- **Website**: https://johnjohn3d.com.br
- **Email**: contato@johnjohn3d.com.br
- **Instagram**: @johnjohn.3d

---
Copyright (c) JohnJohn 3D 2024-2026
