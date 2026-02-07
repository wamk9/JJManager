# JJManager - Contexto Completo de Desenvolvimento

> Documentação consolidada do projeto JJManager para desenvolvimento com Claude Code
> Última atualização: 2026-01-09

---

## Índice

1. [Visão Geral do Projeto](#visão-geral-do-projeto)
2. [Estrutura do Projeto](#estrutura-do-projeto)
3. [Dispositivos Suportados](#dispositivos-suportados)
4. [Arquitetura e Componentes](#arquitetura-e-componentes)
5. [Configuração e Build](#configuração-e-build)
6. [Dependências](#dependências)
7. [Database](#database)
8. [Comunicação com Dispositivos](#comunicação-com-dispositivos)
9. [Integrações Externas](#integrações-externas)
10. [Entry Point e Fluxo de Inicialização](#entry-point-e-fluxo-de-inicialização)
11. [Melhorias Recentes](#melhorias-recentes)
12. [Protocolo JJB01_V2](#protocolo-jjb01_v2)
13. [Changelog JJB01_V2](#changelog-jjb01_v2)
14. [Plano de Testes JJB01_V2](#plano-de-testes-jjb01_v2)
15. [Protocolo de Comunicação HID Byte-Based (ATUALIZADO 2025-12-26)](#15-protocolo-de-comunicação-hid-byte-based-atualizado-2025-12-26)
16. [Sistema de Filtragem de Sessões de Áudio por Input (2026-01-09)](#16-sistema-de-filtragem-de-sessões-de-áudio-por-input-2026-01-09)

---

## Visão Geral do Projeto

**JJManager** é uma aplicação Windows Forms Desktop desenvolvida em C# para gerenciar dispositivos de hardware da JohnJohn3D.

### Informações Básicas

- **Versão Atual:** 1.3.0.1
- **Framework:** .NET Framework 4.7.2
- **Plataforma:** x64 (Any CPU na solution)
- **Tipo:** Windows Forms Application (WinExe)
- **Empresa:** JohnJohn 3D
- **Website:** https://johnjohn3d.com.br
- **Propósito:** Gerenciar inputs/outputs de dispositivos JJM (Audio Mixer) e JJB (ButtonBox) series

### Características Principais

- Interface Material Design (MaterialSkin 2.3.1)
- Suporte a 13 tipos de dispositivos diferentes
- Sistema de perfis para configurações personalizadas
- Integração com SimHub para simuladores de corrida
- Controle de áudio do sistema (AudioController)
- Sistema de macros (teclado/mouse)
- Atualização automática de firmware
- Database local SQL Server LocalDB
- Personalização de LEDs com animações

### Funcionalidades Principais

#### Gerenciamento de Perfis
- Crie perfis diferentes para cada aplicação ou jogo
- Troque entre perfis com um clique
- Configurações específicas para cada uso

#### Sistema de Macros
- Grave sequências de teclas e cliques do mouse
- Execute atalhos complexos com um único botão
- Suporte a delays e repetições

#### Controle de Áudio
- Controle o volume de aplicações individuais
- Mude dispositivos de áudio (fones, caixas de som)
- Mute/unmute rápido de aplicações específicas

#### Integração SimHub
- Conecte ao SimHub para dados de telemetria em tempo real
- Configure LEDs que reagem a RPM, velocidade, marcha, etc.
- Suporte a múltiplos simuladores (iRacing, ACC, F1, etc.)

---

## Estrutura do Projeto

```
GitHub - JJManager/
├── JJManager/                                    # Main application folder
│   ├── Class/                                    # Core business logic
│   │   ├── App/                                  # Application features
│   │   │   ├── Config/                           # Configuration management
│   │   │   │   ├── Config.cs                    # Main config handler
│   │   │   │   ├── OthersConfigs/               # Theme and startup configs
│   │   │   │   └── Theme/
│   │   │   ├── Connections/                      # Network/communication
│   │   │   │   ├── DatabaseConnection.cs        # SQL database handler
│   │   │   │   └── SimHubWebsocket.cs           # WebSocket for SimHub integration
│   │   │   ├── Controls/                         # Custom UI controls
│   │   │   │   ├── CustomMaterialSkin/           # Material design components
│   │   │   │   └── DrawImage/                    # Image rendering control
│   │   │   ├── Fonts/
│   │   │   │   └── FontAwesome.cs               # Font Awesome icon support
│   │   │   ├── Input/                            # Input handling modules
│   │   │   │   ├── Input.cs                     # Input base class
│   │   │   │   ├── AudioController/              # Audio device control
│   │   │   │   ├── AudioPlayer/                  # Audio playback
│   │   │   │   └── MacroKey/                     # Keyboard/Mouse macros
│   │   │   │       ├── MacroKey.cs              # Macro definition
│   │   │   │       ├── Keyboard/                # Keyboard control
│   │   │   │       └── Mouse/                    # Mouse control
│   │   │   ├── Output/                           # Output handling
│   │   │   │   ├── Output.cs                    # Base output class
│   │   │   │   ├── Leds/                         # LED control
│   │   │   │   └── DashboardLeds/                # Dashboard LED management
│   │   │   ├── PixelGrid/                        # Visual pixel grid for LEDs
│   │   │   ├── Profile/
│   │   │   │   └── Profile.cs                   # Profile management
│   │   │   ├── Updater/                          # Update mechanisms
│   │   │   │   ├── Updater.cs                   # Base updater
│   │   │   │   ├── DeviceUpdater.cs             # Device firmware updates
│   │   │   │   ├── SoftwareUpdater.cs           # Application updates
│   │   │   │   └── PluginUpdater.cs             # Plugin updates
│   │   │   ├── OpenBLT.cs                       # Bootloader communication
│   │   │   ├── CallForm.cs                      # Form launcher
│   │   │   └── WaitForm.cs                      # Loading/waiting form
│   │   ├── Devices/                              # Device implementations
│   │   │   ├── JJDevice.cs                      # Base device class (abstract)
│   │   │   ├── Connections/                      # Device connection types
│   │   │   │   ├── JJDevice.cs                  # Device abstraction
│   │   │   │   ├── HID.cs                       # HID protocol
│   │   │   │   ├── Joystick.cs                  # Joystick/DirectInput
│   │   │   │   └── Bluetooth.cs                 # Bluetooth connectivity
│   │   │   └── [Device implementations]
│   │   │       ├── JJB01.cs                     # ButtonBox JJB-01
│   │   │       ├── JJB01_V2.cs                  # ButtonBox JJB-01 V2
│   │   │       ├── JJB999.cs                    # ButtonBox JJB-999
│   │   │       ├── JJBP06.cs                    # ButtonBox JJBP-06
│   │   │       ├── JJBSlim_A.cs                 # ButtonBox JJB-Slim Type A
│   │   │       ├── JJDB01.cs                    # Dashboard JJDB-01 (newest)
│   │   │       ├── JJHL01.cs                    # Hub ARGB JJHL-01
│   │   │       ├── JJLC01.cs                    # LoadCell JJLC-01
│   │   │       ├── JJM01.cs                     # Audio Mixer JJM-01
│   │   │       ├── JJSD01.cs                    # Streamdeck JJSD-01
│   │   │       └── JJQ01.cs                     # JJQ-01 (unknown)
│   │   ├── AppModules.cs                         # Utility classes (NotifyIcon, Timer, etc.)
│   │   ├── Device.cs                             # Legacy Device class (commented out)
│   │   ├── Log.cs                                # Logging system
│   │   └── Migrate.cs                            # Database migration handler
│   ├── Pages/                                    # UI Forms (Windows Forms)
│   │   ├── App/                                  # Application feature pages
│   │   │   ├── AudioController.cs               # Audio control UI
│   │   │   ├── AudioPlayer.cs                   # Audio player UI
│   │   │   ├── AudioSession.cs                  # Audio session management
│   │   │   ├── LedMonoAction.cs                 # LED mono action configuration
│   │   │   ├── LedRgbAction.cs                  # LED RGB action configuration
│   │   │   ├── MacroKeyMain.cs                  # Macro key configuration
│   │   │   ├── MacroKeyAction.cs                # Individual macro action
│   │   │   ├── MessageBox.cs                    # Custom message box
│   │   │   ├── UpdateAppNotification.cs         # Update notification UI
│   │   │   ├── WaitForm.cs                      # Loading/waiting UI
│   │   │   └── Updater/
│   │   │       └── MultipleComPort.cs           # Multiple COM port selector
│   │   ├── Devices/                              # Device configuration pages
│   │   │   ├── [Device UI pages matching each device class]
│   │   │   ├── JJDB01.cs                        # Dashboard JJDB-01 UI
│   │   │   ├── JJSD01.cs                        # Streamdeck JJSD-01 UI
│   │   │   └── [Others...]
│   │   └── CreateProfile.cs                      # Profile creation UI
│   ├── Config/                                   # Configuration files
│   │   └── JJPropertyDictionary.json            # SimHub property mapping
│   ├── Database/                                 # Database files
│   │   └── JJManagerDB_blank.mdf                # Blank database template
│   ├── MigrateQuerys/                           # SQL migration scripts
│   │   └── [Version-specific SQL files]
│   │       ├── 1_1_14.sql
│   │       ├── 1_2_0.sql through 1_3_0.sql
│   ├── MigrateCommands/
│   │   └── 1_2_4.cs                             # Migration code
│   ├── Properties/                               # Assembly metadata
│   │   ├── AssemblyInfo.cs                      # Version: 1.2.9.0
│   │   ├── Resources.resx                       # Resource file
│   │   └── Settings.settings                    # Application settings
│   ├── Resources/                                # UI resources
│   │   ├── [Device UI images]
│   │   ├── [Font files] (fa-*.ttf)
│   │   ├── compatible_devices.json              # Supported devices list
│   │   └── [Various PNG icons/images]
│   ├── Main.cs / Main.Designer.cs               # Main application form
│   ├── Main.resx                                # Main form resources
│   ├── Program.cs                               # Application entry point
│   ├── Settings.cs                              # Settings management
│   ├── JJManager.csproj                         # Project configuration
│   ├── App.config                               # Application configuration
│   └── packages.config                          # NuGet dependencies
└── packages/                                     # NuGet packages directory
```

### Estatísticas do Projeto

| Component | File Count | Purpose |
|-----------|-----------|---------|
| Device Classes | 11 | One per supported device type |
| Device UI Pages | 11 | Configuration UI per device |
| Input Classes | 4 | MacroKey, AudioController, AudioPlayer, etc. |
| Output Classes | 3 | Leds, DashboardLeds, plus base |
| Pages/Forms | 15+ | All UI windows and dialogs |
| Class files | 50+ | Business logic and services |
| **Total C# Files** | **106** | Main application code |

---

## Dispositivos Suportados

### Lista Completa de Dispositivos (compatible_devices.json)

1. **Streamdeck JJSD-01** (v2024.09.01)
2. **Mixer de Áudio JJM-01** (v2025.01.17)
3. **ButtonBox JJB-01** (legacy)
4. **ButtonBox JJB-01 V2** (v2024.11.29)
5. **ButtonBox JJBP-06** (legacy)
6. **ButtonBox JJB-999** (v2024.11.29)
7. **Hub ARGB JJHL-01** (legacy)
8. **Hub ARGB JJHL-01 Plus** (legacy)
9. **Hub RGB JJHL-02** (legacy)
10. **Hub RGB JJHL-02 Plus** (legacy)
11. **Dashboard JJDB-01** (newest, actively maintained)
12. **LoadCell JJLC-01** (v2025.04.15)
13. **ButtonBox JJB-Slim Type A** (v2025.04.19)

### Tipos de Conexão

**JJDevice.Type enum:**
- **Joystick** - SharpDX.DirectInput based (para JJB01, JJM01, etc.)
- **HID** - HID protocol via HidSharp (para alguns ButtonBox variants)
- **Bluetooth** - InTheHand.Net.Bluetooth
- **Custom** - Device-specific protocols

---

## Arquitetura e Componentes

### Padrões de Arquitetura Utilizados

1. **Inheritance Hierarchy:** JJDevice base class com implementações específicas por dispositivo
2. **Observer Pattern:** ObservableCollection para atualizações da lista de dispositivos
3. **Thread-Based Async:** Threads separadas para envio/recebimento de dados dos dispositivos
4. **Factory Pattern:** Criação de dispositivos baseada no tipo de conexão
5. **Singleton Pattern:** MaterialSkinManager para temas da UI
6. **Repository Pattern:** DatabaseConnection para acesso a dados
7. **Configuration Pattern:** ConfigClass para configurações da aplicação

### Fluxo de Dados dos Dispositivos

```
Device Hardware
    ↓
Connection (Joystick/HID/Bluetooth)
    ↓
JJDevice (Base abstraction)
    ↓
Specific Device Class (JJB01, JJDB01, etc.)
    ↓
Thread-based Receiving/Sending Tasks
    ↓
Profile (Input/Output mappings)
    ↓
Input Handlers (MacroKey, AudioController, AudioPlayer)
    ↓
Output Handlers (Leds, DashboardLeds)
```

### Módulos Principais

#### A. Input Handling (Class/App/Input/)
- **MacroKey:** Automação de teclado e mouse
- **AudioController:** Controle de dispositivos de áudio e volume do sistema
- **AudioPlayer:** Integração de reprodução de arquivos de áudio
- Modes: None, MacroKey, AudioController, AudioPlayer
- Types: Digital (on/off), Analog (value-based)

#### B. Output Handling (Class/App/Output/)
- **Leds:** Controle e animação de LEDs padrão
- **DashboardLeds:** Tratamento especial para LEDs do dashboard (JJDB-01)
- Visualização de pixel grid para layout de LEDs

#### C. Profile System (Class/App/Profile/)
- Perfis de configuração por dispositivo
- Armazena mapeamentos de input/output
- Persistência de perfis no database
- UI de seleção de perfil em Pages/CreateProfile.cs

#### D. Update System
- **SoftwareUpdater:** Atualizações de versão da aplicação via manifesto JSON online
- **DeviceUpdater:** Atualizações de firmware via COM port/bootloader
- **PluginUpdater:** Gerenciamento de plugins externos
- Usa protocolo OpenBLT bootloader para flash de firmware

#### E. Logging System
- Logs armazenados em: `%APPDATA%\JohnJohn3D\JJManager\Log\`
- Um log por módulo: `Log_<ModuleName>.txt`
- Exibe informações de tamanho de arquivo (bytes, KB, MB)
- Recuperação de informação de módulos via `Log.GetModulesInfo()`

#### F. Configuration Management
- Seleção de tema (light/dark, múltiplos esquemas de cores)
- Configuração de inicialização com o Windows
- Preferências de auto-conexão de dispositivos
- Armazenado em database e configurações locais

#### G. SimHub Integration
- **SimHubWebsocket.cs:** Conexão WebSocket para SimHub
- **JJPropertyDictionary.json:** Mapeia dados de telemetria do SimHub para propriedades do dispositivo
- Suporta dados de simulação de corrida: marcha, velocidade, RPM, pressão dos pneus, combustível, flags, etc.

---

## Configuração e Build

### Requisitos do Sistema

- Windows OS (x64 architecture required)
- .NET Framework 4.7.2
- SQL Server LocalDB (Express)
- Acesso administrativo (para acesso a dispositivos USB/HID)
- Acesso a porta COM (para atualizações de firmware)

### Processo de Build

**Usando MSBuild (Recomendado para .NET Framework):**

```bash
# Localizar MSBuild
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"

# Restaurar pacotes
msbuild JJManager.sln -t:Restore -p:Configuration=Release -p:Platform="Any CPU"

# Compilar Release
msbuild JJManager.sln -p:Configuration=Release -p:Platform="Any CPU" -m -v:minimal
```

**Configurações de Build:**
- **Configuration:** Debug | Release
- **Platform:** Any CPU (internamente configurado para x64)
- **Output Type:** WinExe
- **Target Framework:** .NET Framework 4.7.2
- **Output:** `JJManager\bin\Release\JJManager.exe`

**Post-Build Event:**
- Move DLLs, XMLs, e PDBs para subdiretório `lib/`

### Arquivo de Configuração Principal (App.config)

#### Database Connection String
```xml
<connectionStrings>
  <add name="JJManagerConnection"
       connectionString="Data Source=(LocalDB)\MSSQLLocalDB;
                         AttachDbFilename=|DataDirectory|\JohnJohn3D\JJManager\JJManagerDB.mdf;
                         Integrated Security=True"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

**Localização do Database:** `%APPDATA%\JohnJohn3D\JJManager\JJManagerDB.mdf`

#### URLs de Configuração
```xml
<applicationSettings>
  <JJManager.Properties.Settings>
    <setting name="url_lastVersions_app">
      <value>https://johnjohn3d.com.br/jjmanager/versioncontrol/lastversions_app.json</value>
    </setting>
    <setting name="url_lastVersions_device">
      <value>https://johnjohn3d.com.br/jjmanager/versioncontrol/lastversions_device.json</value>
    </setting>
    <setting name="url_lastVersions_plugin">
      <value>https://johnjohn3d.com.br/jjmanager/versioncontrol/lastversions_plugin.json</value>
    </setting>
  </JJManager.Properties.Settings>
</applicationSettings>
```

---

## Dependências

### NuGet Packages (103 total)

#### UI Framework
| Package | Version | Purpose |
|---------|---------|---------|
| MaterialSkin.2 | 2.3.1 | Material Design UI components |
| Avalonia | 11.1.3 | Cross-platform UI (mostly unused) |
| LiveCharts, LiveCharts.Wpf | 0.9.7 | Data visualization charts |
| Cyotek.Windows.Forms.ColorPicker | 1.7.2 | Color picker control |

#### Audio
| Package | Version | Purpose |
|---------|---------|---------|
| NAudio (Core, Asio, Midi, Wasapi, WinMM, WinForms) | 2.2.1 | Audio playback and recording |
| AudioSwitcher.AudioApi | 4.0.0-alpha5 | Audio device management |

#### Device Communication
| Package | Version | Purpose |
|---------|---------|---------|
| HidSharp | 2.1.0 | HID device communication |
| Device.Net, Hid.Net, Usb.Net | 4.2.1 | USB/HID device abstraction |
| SharpDX, SharpDX.DirectInput | 4.2.0 | DirectInput for joysticks |
| InTheHand.Net.Bluetooth | 4.1.44 | Bluetooth connectivity |

#### Firmware Updates
| Package | Version | Purpose |
|---------|---------|---------|
| ArduinoUploader | 3.2.0 | Arduino firmware upload |
| LibUsbDfu, LibUsbDotNet | 1.1.0, 2.2.29 | USB DFU protocol |
| DeviceProgramming | 1.0.3 | Device programming utilities |
| STBootLib | 1.0.0 | STM32 bootloader |
| IntelHexFormatReader | 2.2.3 | Intel HEX file parsing |

#### Database
| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.Data.SqlClient | 5.2.2 | SQL Server client |
| Microsoft.SqlServer.SqlManagementObjects | 172.52.0 | SQL Server management |
| Microsoft.SqlServer.Assessment | 1.1.17 | SQL assessment tools |

#### JSON/Serialization
| Package | Version | Purpose |
|---------|---------|---------|
| Newtonsoft.Json | 13.0.3 | JSON serialization/deserialization |
| System.Text.Json | 9.0.0 | Modern JSON handling |

#### Serial Communication
| Package | Version | Purpose |
|---------|---------|---------|
| SerialPortStream (RJCP) | 2.4.2 | Advanced serial port handling |
| System.IO.Ports | 9.0.0 | .NET serial port |

#### Security/Identity
| Package | Version | Purpose |
|---------|---------|---------|
| Azure.Identity, Azure.Core | 1.13.1, 1.44.1 | Azure authentication |
| Microsoft.Identity.Client | 4.67.0 | MSAL authentication |
| Microsoft.IdentityModel.* | 8.3.0 | Identity model components |

---

## Database

### Arquitetura do Database

**Tipo:** SQL Server LocalDB (.mdf file)
**Localização:** `%APPDATA%\JohnJohn3D\JJManager\JJManagerDB.mdf`
**Template:** `JJManagerDB_blank.mdf` (copiado no primeiro run)

### Sistema de Migração

- **Migration System:** Version-aware migrations (atualmente em v1.2.9)
- **Migration Scripts:** Localizados em `MigrateQuerys/` folder
- **Auto-Backup:** Cria backup antes de cada migração
- **Configuração:** Armazenada na tabela `dbo.configs` com coluna `software_version`

**Versões de Migração Suportadas:**
- 1.1.13 (primeira) até 1.2.9 (atual)
- Cada versão tem arquivo SQL correspondente
- Código C# de migração customizado em `MigrateCommands/` para operações complexas

### Inicialização do Database

**Sequência de Inicialização:**
1. Verifica existência do database em `%APPDATA%`
2. Se não existir, copia `JJManagerDB_blank.mdf`
3. Executa migrations necessárias baseado em `software_version`
4. Cria backup antes de cada migration
5. Atualiza `software_version` após migration bem-sucedida

---

## Comunicação com Dispositivos

### Camadas de Comunicação

```
Application Layer (Pages/Devices/*.cs)
    ↓
Device Class Layer (Class/Devices/*.cs)
    ↓
Connection Layer (Class/Devices/Connections/*.cs)
    ↓
Protocol Layer (HID, Joystick, Bluetooth)
    ↓
Hardware Device
```

### Protocolo de Comunicação

#### Sending Thread
- Thread dedicada por dispositivo
- Envia comandos da fila de saída
- Implementa retry logic para falhas
- Throttling para não sobrecarregar dispositivo

#### Receiving Thread
- Thread dedicada por dispositivo
- Poll contínuo ou interrupt-based
- Parser de protocolo específico por dispositivo
- Atualiza estado interno do device class

#### Data Format
- Varia por dispositivo (definido em cada Device class)
- Geralmente: Header (tipo de mensagem) + Payload (dados)
- Checksum/CRC para validação (em alguns dispositivos)

---

## Integrações Externas

### 1. SimHub Racing Simulator
- **Método:** WebSocket connection
- **Classe:** `SimHubWebsocket.cs`
- **Config:** `JJPropertyDictionary.json`
- **Dados Suportados:**
  - Telemetria do carro (velocidade, RPM, marcha, combustível)
  - Pressão e temperatura dos pneus
  - Flags de corrida (bandeiras)
  - Posição e tempo de volta

### 2. JohnJohn3D Web Service
- **Update Checks:** Online version/update checks
- **URLs:**
  - App Version: `https://johnjohn3d.com.br/jjmanager/versioncontrol/lastversions_app.json`
  - Device Version: `https://johnjohn3d.com.br/jjmanager/versioncontrol/lastversions_device.json`
  - Plugin Version: `https://johnjohn3d.com.br/jjmanager/versioncontrol/lastversions_plugin.json`

### 3. Windows Audio API
- **Biblioteca:** NAudio, AudioSwitcher
- **Funcionalidades:**
  - Enumeração de dispositivos de áudio
  - Controle de volume por aplicação
  - Mudança de dispositivo de áudio padrão
  - Monitoramento de sessões de áudio

### 4. DirectInput (Windows)
- **Biblioteca:** SharpDX.DirectInput
- **Funcionalidades:**
  - Enumeração de joysticks/devices de jogo
  - Leitura de botões e eixos
  - Force feedback (em dispositivos suportados)

### 5. SQL Server LocalDB
- **Biblioteca:** Microsoft.Data.SqlClient
- **Funcionalidades:**
  - Storage local de perfis
  - Configurações da aplicação
  - Histórico de uso
  - Migration tracking

---

## Entry Point e Fluxo de Inicialização

### Program.cs - Main Entry Point

```csharp
[STAThread]
static void Main()
{
    // Single Instance Enforcement
    if (mutex.WaitOne(TimeSpan.Zero, true))
    {
        // Setup DataDirectory for LocalDB
        AppDomain.CurrentDomain.SetData("DataDirectory",
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

        // Initialize Windows Forms
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Start main form
        Application.Run(new Main());
    }
    else
    {
        // Bring existing instance to foreground
        BringToForeground();
    }
}
```

**Mutex ID:** `{71fcb366-7349-40b3-835a-943c012e1283}`

### Main.cs - Inicialização da Aplicação

**Sequência de Inicialização:**
```
Main() Constructor
├─ Load theme configuration
├─ Initialize Material Design
├─ Disable all forms initially
├─ Run database migration
├─ Setup NotifyIcon (tray)
├─ Setup timers (FillLists, AutoConnect)
├─ Initialize device grid
└─ Load log data
```

**Threads Principais:**
- `threadUpdateDevicesList` - Monitora disponibilidade de dispositivos
- `fillListsTimer` - Atualiza lista de dispositivos (intervalo de 2 segundos)
- `AutoConnectTimer` - Tenta auto-conexão (intervalo de 2 segundos)

---

## Melhorias Recentes

### 1. Sistema de Perfis Compartilhados (Profile Sharing)

**Implementação:** `Class/App/Profile/Profile.cs`

Dispositivos do mesmo modelo agora compartilham um único perfil padrão ao invés de criar duplicatas.

**Funcionamento:**
1. Ao conectar um dispositivo, verifica se já existe um "Perfil Padrão" para aquele `ProductId`
2. Se existir, reutiliza o perfil existente e associa ao novo dispositivo
3. Se não existir, cria um novo perfil padrão
4. Reduz redundância de dados e facilita configuração de múltiplos dispositivos

### 2. Limpeza Automática de Downloads

**Implementação:** `Main.cs` - método `CleanDownloadFiles()`

Na inicialização do JJManager, a pasta de downloads temporários é automaticamente limpa.

**Localização dos Downloads:** `%AppData%\JohnJohn3D\JJManager\downloads`

### 3. Barra de Progresso para Downloads e Updates

**Implementação:**
- `Main.Designer.cs` - `ToolStripProgressBar progressBarDownload`
- `Class/App/Updater/Updater.cs` - eventos de progresso

**Funcionalidades:**
- Barra de progresso integrada no StatusBar da aba "Atualizações"
- Usa `WebClient.DownloadProgressChanged` para atualização em tempo real
- Mostra percentagem de download no texto do status
- Progress reporter usando `Progress<double>` para upload de firmware

### 4. MessageBox Customizado Material Design

**Implementação:** Conversão completa de todos os `MessageBox.Show()` do sistema

**Classe CustomMessageBox:** `Pages/App/MessageBox.cs`
- Interface consistente com Material Design
- Títulos contextuais em todos os diálogos
- Suporta tema dark/light
- Botões estilizados

### 5. Migração HID de Device.Net para HidSharp

**Implementação:** `Class/Devices/Connections/HID.cs`

Migração completa da biblioteca de comunicação HID de Device.Net para HidSharp puro.

**Dispositivos Migrados:**
- JJM01 (Mixer)
- JJB999, JJB01, JJB01_V2, JJBP06, JJBSlim_A (ButtonBoxes)
- JJSD01 (StreamDeck)
- JJLC01 (LoadCell)
- JJDB01 (Dashboard)

**Melhorias:**
- Maior estabilidade na comunicação
- Melhor tratamento de timeouts
- Menos dependências externas
- Código mais limpo e manutenível

---

## Protocolo JJB01_V2

### Visão Geral

O JJB01_V2 utiliza protocolo de comunicação baseado em bytes via HID, similar ao JJDB-01. Este protocolo substitui o anterior baseado em JSON para melhor performance e confiabilidade.

### Estrutura Geral dos Pacotes

**Formato Padrão:**
```
[CMD_H][CMD_L][PAYLOAD...][FLAG_H][FLAG_L]
```

- **CMD_H, CMD_L**: Comando de 16 bits em big-endian
- **PAYLOAD**: Dados específicos do comando (0 ou mais bytes)
- **FLAG_H, FLAG_L**: Flags de controle (16 bits)

### Sistema de FLAGS

| FLAG   | Hexadecimal | Significado |
|--------|-------------|-------------|
| CONTINUE | 0x1001    | Mais chunks virão (para mensagens grandes) |
| END (JJDB-01) | 0x2001 | Último chunk / mensagem completa |
| END (JJB01_V2) | 0x20FF | Último chunk / mensagem completa |

**Nota**: O JJB01_V2 usa **0x20FF** como flag de fim padrão.

### Comandos Suportados (JJManager → Firmware)

#### 1. LED Mode (0x0000)
Define o modo de operação do LED.

**Estrutura:**
```
[0x00][0x00][MODE][0x20][0xFF]
```

**Parâmetros:**
- `MODE` (1 byte): Modo do LED
  - `0x00` = OFF
  - `0x01` = ON (contínuo)
  - `0x02` = BLINK (piscando)
  - `0x03` = PULSE (pulsando)
  - `0x04` = SimHub Sync Mode

#### 2. Brightness (0x0001)
Define o brilho do LED (PWM).

**Estrutura:**
```
[0x00][0x01][BRIGHTNESS][0x20][0xFF]
```

**Parâmetros:**
- `BRIGHTNESS` (1 byte): Valor de brilho (0-255)

#### 3. Blink Speed (0x0002)
Define a velocidade do efeito BLINK.

**Estrutura:**
```
[0x00][0x02][SPEED][0x20][0xFF]
```

#### 4. Pulse Delay (0x0003)
Define o delay do efeito PULSE.

**Estrutura:**
```
[0x00][0x03][DELAY][0x20][0xFF]
```

#### 5. Device Info Request (0x00FF)
Solicita informações do dispositivo.

**Estrutura:**
```
[0x00][0xFF][INFO_TYPE][0x20][0xFF]
```

**INFO_TYPE:**
- `0x00` = Firmware Version
- `0x01` = Hardware Version

### Otimização: Change Tracking

Para reduzir tráfego HID desnecessário, o JJManager implementa **change tracking** para todos os parâmetros.

**Variáveis de tracking:**
```csharp
private int _lastSentLedMode = -1;      // -1 = never sent
private int _lastSentBrightness = -1;
private int _lastSentBlinkSpeed = -1;
private int _lastSentPulseDelay = -1;
```

### SimHub Sync Mode (LED Mode = 4)

Quando `LED Mode = 4`, o JJB01_V2 entra em modo de sincronização com SimHub para controle dinâmico do LED baseado em telemetria.

**Lógica de Prioridade de Outputs:**

O JJB01_V2 possui apenas 1 LED (índice 0). Quando múltiplos Outputs podem controlar este LED:

1. **Iteração reversa** dos Outputs do perfil (`Count - 1` até `0`)
2. **Verificação de ativação** via `output.Led.SetActivatedValue(value)`
3. **Primeiro Output ativo encontrado** (de trás para frente) é usado
4. **Último Output ativo prevalece** (prioridade aos últimos da lista)

### Sistema de Keep-Alive

**No JJManager (C#):**
- Envia LED Mode a cada **3 segundos** (independente de mudanças)
- Timer: `_lastLedModeKeepAlive` + `_keepAliveInterval`

**No Firmware (Arduino):**
- Rastreia último comando recebido: `lastCommandTime`
- Timeout: **5 segundos** (`KEEP_ALIVE_TIMEOUT_MS`)
- Se timeout: força LED Mode = 0 (OFF)

---

## Changelog JJB01_V2

### Resumo Executivo

**Data:** 2025-12-25
**Objetivo:** Migrar JJB01_V2 de protocolo JSON para byte-based (similar ao JJDB-01)

### Objetivos Alcançados

✅ Protocolo byte-based completo implementado
✅ Keep-alive com timeout de 5 segundos
✅ Detecção de versão de firmware funcional
✅ SimHub mode com prioridade de outputs
✅ Change tracking para otimização de envios
✅ Chunking support para mensagens grandes
✅ Correções de race conditions em conexão/desconexão
✅ Correções de UI (carregamento de valores)

### Alterações no Firmware (Arduino)

#### Arquivos Modificados

**1. Configs.hpp**
- Adicionado: `#define KEEP_ALIVE_TIMEOUT_MS 5000`

**2. JJHID.hpp**
- Removido: Dependências JSON
- Mudança de tipo: `char` → `uint8_t` para buffers
- Adicionado: Variável de keep-alive (`lastCommandTime`)

**3. JJHID.cpp**
- Reescrito completamente: Parser JSON → Parser byte-based
- Método `convertMessage()`: Processa comandos byte-based
- Método `receiveData()`: Atualiza keep-alive timer
- Método `checkTimeout()`: Verifica timeout e desliga LED
- Método `sendToJJManager()`: Envia respostas com protocolo byte-based

**4. Code_JJB01_V2_2025-12-25.ino**
- Adicionado ao loop: `jjhid.checkTimeout()`

### Alterações no JJManager (C#)

#### Arquivos Modificados

**1. JJB01_V2.cs**
- Adicionado: Variáveis de keep-alive
- Modificado: Método `SendData()` - SimHub Mode com prioridade de outputs
- Implementado: Keep-alive a cada 3 segundos

**2. HID.cs**
- Método `GetFirmwareVersion()`: Suporte ao novo protocolo byte-based
- Método `RequestHIDBytes()`: Suporte a chunking e detecção de flags

**3. JJDevice.cs**
- Método `Connect()`: Verificação de Tasks antes de criar novas
- Método `Disconnect()`: Espera Tasks com timeout de 5s

**4. Profile.cs**
- Método `ExcludeOutput()`: Marca Changed = true ao excluir

**5. JJB01_V2.cs (UI)**
- Método `LoadFormData()`: Carrega valores do perfil após configurar DataSource
- Método `ShowProfileConfigs()`: Simplificado

**6. LedMonoAction.cs**
- Correção: Cast de `int` (não usar operador `as`)
- Correção: Ordem de inicialização (DataSource antes de UpdateForm)
- Correção: UpdateForm() carrega ModeIfEnabled

### Correções de Bugs

#### Bug 1: Flags 0x20 0xFF apareciam no retorno
**Solução:** Encontrar fim real dos dados antes de verificar flags

#### Bug 2: Output filtrado não estava carregando
**Solução:** Filtrar outputs com LINQ antes de iterar

#### Bug 3: JsonNode causava erro de conversão
**Solução:** Usar `GetValue<dynamic>()`

#### Bug 4: Exclusão de output não funcionava
**Solução:** Marcar `Changed = true` explicitamente

#### Bug 5: Valores de perfil não eram carregados na UI
**Solução:** Inverter ordem de chamadas (DataSource → valores)

#### Bug 6: LED Mode não carregava no LedMonoAction
**Solução:** Corrigir cast, ordem de inicialização e carregamento de valores

#### Bug 7: Race condition em conexão/desconexão rápida
**Solução:** Esperar Tasks com timeout de 5s e verificar antes de criar novas

---

## Plano de Testes JJB01_V2

### Pré-requisitos

#### Hardware Necessário
- ✅ JJB01_V2 (ButtonBox versão 2)
- ✅ Cabo USB (para conexão HID)
- ✅ Cabo Serial/COM (para atualização de firmware via bootloader)
- ✅ Computador Windows x64

#### Software Necessário
- ✅ Arduino IDE 1.8.x ou 2.x
- ✅ Visual Studio 2019/2022 (ou MSBuild)
- ✅ SQL Server LocalDB
- ✅ SimHub (opcional, para testes de integração)

### Testes do Firmware

#### Teste 1: Boot e Inicialização
**Objetivo:** Verificar que o firmware inicializa corretamente

**Critério de Sucesso:**
- [ ] LED responde
- [ ] Dispositivo reconhecido pelo Windows
- [ ] Porta HID acessível

#### Teste 2: Timeout Keep-Alive
**Objetivo:** Verificar que LED desliga após 5 segundos sem comunicação

**Critério de Sucesso:**
- [ ] LED desliga em ≤ 5 segundos após desconectar
- [ ] Firmware não trava
- [ ] Reconectar funciona normalmente

#### Teste 3: Recebimento de Comandos
**Objetivo:** Verificar que firmware recebe e processa todos os comandos

**Critério de Sucesso:**
- [ ] Todos os comandos são processados
- [ ] LED responde conforme esperado
- [ ] Sem travamentos

#### Teste 4: Device Info Request
**Objetivo:** Verificar que firmware responde com informações corretas

**Critério de Sucesso:**
- [ ] Firmware responde em < 1 segundo
- [ ] Versão está correta
- [ ] FLAGS estão corretos (0x20FF)

### Testes do JJManager

#### Teste 5: Detecção de Dispositivo
**Critério de Sucesso:**
- [ ] Dispositivo aparece em ≤ 5 segundos
- [ ] Nome correto exibido
- [ ] Status "Desconectado" correto

#### Teste 6: Conexão e GetFirmwareVersion()
**Critério de Sucesso:**
- [ ] Conexão em ≤ 3 segundos
- [ ] Versão detectada corretamente
- [ ] Sem erros no log

#### Teste 7: Controle de LED Mode
**Critério de Sucesso:**
- [ ] Todos os 5 modos funcionam
- [ ] Resposta em ≤ 1 segundo
- [ ] Configuração salva no perfil

#### Teste 8: Keep-Alive (JJManager → Firmware)
**Critério de Sucesso:**
- [ ] Keep-alive enviado a cada 3±0.5 segundos
- [ ] LED não desliga enquanto conectado
- [ ] Sem overhead excessivo de CPU

#### Teste 9: Change Tracking
**Critério de Sucesso:**
- [ ] Brightness enviado apenas quando muda
- [ ] Blink Speed enviado apenas quando muda
- [ ] Pulse Delay enviado apenas quando muda
- [ ] LED Mode enviado a cada 3s (keep-alive) OU quando muda

#### Teste 10: Persistência de Perfil
**Critério de Sucesso:**
- [ ] Todos os valores persistem no database
- [ ] UI carrega valores corretamente
- [ ] LED aplica configurações imediatamente

### Testes de Integração

#### Teste 11: SimHub Mode
**Critério de Sucesso:**
- [ ] SimHub conecta via WebSocket
- [ ] Properties recebidas corretamente
- [ ] LED responde a mudanças de property
- [ ] Sem lag perceptível

#### Teste 12: SimHub Output Priority
**Critério de Sucesso:**
- [ ] Último output ativo tem prioridade
- [ ] Transições suaves
- [ ] Sem flickering
- [ ] Ordem dos outputs respeitada

#### Teste 13: Exclusão de Output
**Critério de Sucesso:**
- [ ] Output removido da UI
- [ ] Database atualizado (Changed = true)
- [ ] Persistência confirmada

### Testes de Robustez

#### Teste 14: Desconexão/Reconexão Rápida
**Critério de Sucesso:**
- [ ] Sem exceções no log
- [ ] Conexão final estável
- [ ] LED controlável normalmente

#### Teste 15: Timeout Durante Comunicação
**Critério de Sucesso:**
- [ ] Detecção de desconexão em ≤ 5 segundos
- [ ] Sem exceções não tratadas
- [ ] Reconexão funciona normalmente

#### Teste 16: Múltiplos JJB01_V2
**Critério de Sucesso:**
- [ ] Detecção de todos os dispositivos
- [ ] Controle independente
- [ ] Sem conflitos de comunicação

### Critério de Aprovação Geral

- ≥ 95% dos testes PASS
- Nenhum erro crítico (crash, perda de dados)
- Performance aceitável (< 1s resposta, < 5% CPU)

---

## Notas de Desenvolvimento

### Avisos Comuns de Compilação

A build gera aproximadamente 93 warnings (CS0168, CS0169, CS0414, CS0067, CS1998):
- Variáveis declaradas mas não utilizadas
- Campos atribuídos mas nunca lidos
- Eventos declarados mas nunca disparados
- Métodos async sem await

**Nota:** Estes warnings são comuns em projetos legados e não afetam a funcionalidade.

### Versão Histórico (Git)

**Commits Recentes:**
- `1f0e599` - Files of v1.2.6.1
- `8dd15c8` - Updated to v1.2.5 with Dashboard JJDB-01 work
- `e76cd8c` - Files of v1.2.4
- `b2be21e` - Removed import of libraries from root folder
- `bb340af` - Changed update URL to production

---

## Build Information

**Última Build Bem-Sucedida:**
- **Data:** 2025-11-30
- **Configuração:** Release | Any CPU
- **Compiler:** MSBuild 17.14.23
- **Framework:** .NET Framework 4.7.2
- **Erros:** 0
- **Avisos:** 93
- **Tempo:** ~10 segundos

**Localização do Executável:**
`JJManager\bin\Release\JJManager.exe`

---

## Informações de Contato e Suporte

**Website:** https://johnjohn3d.com.br
**Email:** contato@johnjohn3d.com.br
**Instagram:** @johnjohn.3d
**Versão do Software:** 1.2.9.0
**Empresa:** JohnJohn 3D

---

## 15. Protocolo de Comunicação HID Byte-Based (ATUALIZADO 2025-12-26)

### Visão Geral

Protocolo unificado de comunicação HID para dispositivos JJB (ButtonBox) usando comandos byte-based ao invés de JSON. Implementado nos dispositivos **JJB01_V2** e **JJB999**.

### Especificação do Protocolo

#### Formato da Mensagem

```
[CMD_H][CMD_L][PAYLOAD...][FLAG_H][FLAG_L]
```

**Componentes:**
- **CMD_H** (1 byte): Comando High Byte (sempre 0x00 para comandos básicos)
- **CMD_L** (1 byte): Comando Low Byte (identifica o tipo de comando)
- **PAYLOAD** (N bytes): Dados do comando (tamanho variável)
- **FLAG_H** (1 byte): Flag High = 0x20 (fixo)
- **FLAG_L** (1 byte): Flag Low = **CMD_L** (repetição do CMD_L para validação)

**IMPORTANTE:** O FLAG_L DEVE ser igual ao CMD_L. Isso funciona como validação/checksum do comando.

#### Tabela de Comandos

| Comando | CMD | CMD_H | CMD_L | Payload | Exemplo Completo | Descrição |
|---------|-----|-------|-------|---------|------------------|-----------|
| **LED Mode** | 0x0000 | 0x00 | 0x00 | 1 byte (0-4) | `0x00 0x00 0x01 0x20 0x00` | Modo do LED: 0=Off, 1=On, 2=Pulse, 3=Blink, 4=SimHub |
| **Brightness** | 0x0001 | 0x00 | 0x01 | 1 byte (0-255) | `0x00 0x01 0x64 0x20 0x01` | Brilho do LED (100 = 0x64) |
| **Blink Speed** | 0x0002 | 0x00 | 0x02 | 1 byte (0-255) | `0x00 0x02 0x05 0x20 0x02` | Velocidade de piscada |
| **Pulse Delay** | 0x0003 | 0x00 | 0x03 | 1 byte (0-255) | `0x00 0x03 0x05 0x20 0x03` | Delay entre pulsos |
| **Device Info** | 0x00FF | 0x00 | 0xFF | 1 byte (tipo) | `0x00 0xFF 0x00 0x20 0xFF` | Requisição de info (0=FW ver, 1=HW ver) |

#### Modos LED

| Valor | Nome | Descrição |
|-------|------|-----------|
| 0 | Desligado | LED completamente apagado |
| 1 | Sempre Ligado | LED sempre aceso com brilho configurado |
| 2 | Pulsando | LED com efeito fade in/out (usa pulse_delay) |
| 3 | Piscando | LED pisca on/off (usa blink_speed) |
| 4 | SimHub Sync | LED controlado por outputs do SimHub |

### Implementação no Firmware (Arduino)

#### Arquivos Modificados

**JJHID.hpp:**
```cpp
struct JJHID {
    private:
    uint8_t rawhidData[MAX_MESSAGE_SIZE];
    uint8_t buffer[MAX_MESSAGE_SIZE];
    JJLED jjled;
    unsigned long lastCommandTime = 0;  // Keep-alive tracking

    void convertMessage(const uint8_t *buffer, size_t length);

    public:
    JJHID();
    void init();
    void setLedPin(uint8_t pin);
    void receiveData();
    void sendToJJManager(uint16_t cmd, uint8_t *buffer, size_t length);
    void executeLedFunctions();
    void checkTimeout();  // Keep-alive timeout check
};
```

**JJHID.cpp - Recebimento:**
```cpp
void JJHID::convertMessage(const uint8_t *buffer, size_t length) {
  if (length < 2) return;

  // Combine first two bytes into a 16-bit command (big-endian)
  uint16_t cmd = (uint16_t(buffer[0]) << 8) | buffer[1];

  // Payload starts at byte 2
  const uint8_t *payload = buffer + 2;
  size_t payloadLength = length - 2;

  // Remove flags from payload (last 2 bytes: 0x20, CMD_L)
  if (payloadLength >= 2) {
    payloadLength -= 2;
  }

  // Handle commands
  if (cmd == 0x0000) {
    // LED Mode
    if (payloadLength >= 1) {
      uint8_t ledMode = payload[0];
      jjled.changeLedMode(ledMode);
    }
  }
  else if (cmd == 0x0001) { /* Brightness */ }
  else if (cmd == 0x0002) { /* Blink Speed */ }
  else if (cmd == 0x0003) { /* Pulse Delay */ }
  else if (cmd == 0x00FF) { /* Device Info */ }
}
```

**JJHID.cpp - Envio:**
```cpp
void JJHID::sendToJJManager(uint16_t cmd, uint8_t *buffer, size_t length) {
  const size_t HEADER_SIZE = 2;  // CMD (2 bytes)
  const size_t FLAGS_SIZE = 2;   // FLAGS (2 bytes: 0x20, CMD_L)

  size_t fullLength = HEADER_SIZE + length + FLAGS_SIZE;
  uint8_t *fullBuffer = (uint8_t *)malloc(fullLength);

  if (!fullBuffer) return;

  // Add CMD (big-endian)
  fullBuffer[0] = (cmd >> 8) & 0xFF;  // CMD_H
  fullBuffer[1] = cmd & 0xFF;         // CMD_L

  // Copy data
  memcpy(fullBuffer + HEADER_SIZE, buffer, length);

  // Add FLAGS (0x20, CMD_L)
  fullBuffer[HEADER_SIZE + length] = 0x20;
  fullBuffer[HEADER_SIZE + length + 1] = cmd & 0xFF;  // FLAG_L = CMD_L

  // Send via RawHID (64-byte chunks)
  RawHID.write(fullBuffer, fullLength);

  free(fullBuffer);
}
```

**Configs.hpp:**
```cpp
#define KEEP_ALIVE_TIMEOUT_MS 5000  // 5 seconds without communication = LED OFF
```

**Main .ino - Loop:**
```cpp
void loop() {
  checkRotaryEncoder();
  checkPotenciometers();
  jjhid.receiveData();
  jjhid.checkTimeout();  // Keep-alive: Desliga LED após 5s sem comunicação
  jjhid.executeLedFunctions();
  if (shift_register.update()) {
    checkPushButtons();
  }
}
```

### Implementação no JJManager (C#)

#### Device Class - Envio de Comandos

**JJB999.cs / JJB01_V2.cs:**
```csharp
// Keep-alive tracking
private DateTime _lastLedModeKeepAlive = DateTime.MinValue;
private readonly TimeSpan _keepAliveInterval = TimeSpan.FromSeconds(3);

// Change tracking (avoid redundant sends)
private int _lastSentLedMode = -1;      // -1 = never sent
private int _lastSentBrightness = -1;
private int _lastSentBlinkSpeed = -1;
private int _lastSentPulseDelay = -1;

// Send LED Mode (keep-alive: every 3 seconds OR when changed)
bool shouldSendLedMode = _lastSentLedMode != ledMode ||
                        (DateTime.Now - _lastLedModeKeepAlive) >= _keepAliveInterval;

if (shouldSendLedMode)
{
    List<byte> ledModeData = new List<byte>
    {
        0x00, 0x00,          // CMD: 0x0000 (LED Mode)
        (byte)ledMode,       // LED Mode (0-4)
        0x20, 0x00           // FLAGS (0x20, CMD_L)
    };
    await SendHIDBytes(ledModeData, false, 0, 2000, 5);
    _lastSentLedMode = ledMode;
    _lastLedModeKeepAlive = DateTime.Now;
}

// Send Brightness (only if changed)
if (_lastSentBrightness != brightness)
{
    List<byte> brightnessData = new List<byte>
    {
        0x00, 0x01,              // CMD: 0x0001 (Brightness)
        (byte)brightness,        // Brightness value (0-255)
        0x20, 0x01               // FLAGS (0x20, CMD_L)
    };
    await SendHIDBytes(brightnessData, false, 0, 2000, 5);
    _lastSentBrightness = brightness;
}

// Send Blink Speed (only if changed)
if (_lastSentBlinkSpeed != blinkSpeed)
{
    List<byte> blinkSpeedData = new List<byte>
    {
        0x00, 0x02,              // CMD: 0x0002 (Blink Speed)
        (byte)blinkSpeed,        // Blink Speed value
        0x20, 0x02               // FLAGS (0x20, CMD_L)
    };
    await SendHIDBytes(blinkSpeedData, false, 0, 2000, 5);
    _lastSentBlinkSpeed = blinkSpeed;
}

// Send Pulse Delay (only if changed)
if (_lastSentPulseDelay != pulseDelay)
{
    List<byte> pulseDelayData = new List<byte>
    {
        0x00, 0x03,              // CMD: 0x0003 (Pulse Delay)
        (byte)pulseDelay,        // Pulse Delay value
        0x20, 0x03               // FLAGS (0x20, CMD_L)
    };
    await SendHIDBytes(pulseDelayData, false, 0, 2000, 5);
    _lastSentPulseDelay = pulseDelay;
}
```

#### UI Page - Configuração

**JJB999.cs / JJB01_V2.cs (Pages/Devices/):**
```csharp
private void LoadFormData()
{
    // Configure LED Mode ComboBox with DataSource
    var ledModes = new[]
    {
        new { Text = "Desligado", Value = 0 },
        new { Text = "Sempre Ligado", Value = 1 },
        new { Text = "Pulsando", Value = 2 },
        new { Text = "Piscando", Value = 3 },
        new { Text = "SimHub Sync", Value = 4 }
    };

    cmdBoxLedMode.DataSource = ledModes;
    cmdBoxLedMode.DisplayMember = "Text";
    cmdBoxLedMode.ValueMember = "Value";

    // Load from profile with JSON type checking
    if (_device.Profile.Data.ContainsKey("led_mode"))
    {
        switch (_device.Profile.Data["led_mode"].GetValueKind())
        {
            case JsonValueKind.String:
                string ledModeString = _device.Profile.Data["led_mode"].GetValue<string>();
                if (int.TryParse(ledModeString, out int ledModeValue))
                {
                    cmdBoxLedMode.SelectedValue = ledModeValue;
                }
                break;
            case JsonValueKind.Number:
                cmdBoxLedMode.SelectedValue = _device.Profile.Data["led_mode"].GetValue<int>();
                break;
        }
    }
}

private void SaveConfig(bool closeWindow = false)
{
    // Get current values or defaults
    int blinkSpeed = _device.Profile.Data.ContainsKey("blink_speed") ?
        _device.Profile.Data["blink_speed"].GetValue<int>() : 5;
    int pulseDelay = _device.Profile.Data.ContainsKey("pulse_delay") ?
        _device.Profile.Data["pulse_delay"].GetValue<int>() : 5;

    JsonObject jsonData = new JsonObject{
        { "led_mode", cmdBoxLedMode.SelectedIndex },
        { "brightness", sldLedBrightness.Value},
        { "blink_speed", blinkSpeed },
        { "pulse_delay", pulseDelay }
    };

    _device.Profile.Update(new JsonObject { { "data", jsonData } });
}
```

### Sistema Keep-Alive

**Firmware (Arduino):**
- Timeout: 5 segundos sem receber comandos
- Ação: Desliga LED automaticamente (modo 0)
- Implementação: `checkTimeout()` chamado no loop principal

**JJManager (C#):**
- Keep-alive: Envia LED Mode a cada 3 segundos
- Garante que firmware não desligue o LED por timeout
- Mesmo que valor não tenha mudado, envia para manter conexão ativa

### Change Tracking (Otimização)

Para evitar tráfego HID desnecessário:
- **LED Mode:** Enviado a cada 3s (keep-alive) OU quando muda
- **Brightness:** Enviado apenas quando muda
- **Blink Speed:** Enviado apenas quando muda
- **Pulse Delay:** Enviado apenas quando muda

Variáveis de tracking:
```csharp
private int _lastSentLedMode = -1;      // -1 = never sent
private int _lastSentBrightness = -1;
private int _lastSentBlinkSpeed = -1;
private int _lastSentPulseDelay = -1;
```

### SimHub Integration

Quando LED Mode = 4 (SimHub Sync), o sistema usa outputs configurados:

```csharp
// Get LED outputs (filter first to optimize)
var ledOutputs = _profile.Outputs
    .Where(o => o.Mode == OutputClass.OutputMode.Leds && o.Led != null)
    .ToList();

// Iterate LED outputs from last to first (reverse order)
// LAST active output in the list has priority
for (int i = ledOutputs.Count - 1; i >= 0; i--)
{
    var output = ledOutputs[i];
    string property = output.Led.Property;

    if (lastData.ContainsKey(property))
    {
        dynamic value = lastData[property]?.GetValue<dynamic>() ?? false;
        bool isActive = output.Led.SetActivatedValue(value);

        if (isActive && output.Led.LedsGrouped != null &&
            output.Led.LedsGrouped.Contains(targetLedIndex))
        {
            activeOutputForLed = output;
            break;  // Found the last active output for this LED
        }
    }
}

// Use active output configuration or turn off if no output is active
int activeLedMode = activeOutputForLed?.Led.ModeIfEnabled ?? 0;
int activeBrightness = activeOutputForLed?.Led.Brightness ?? brightness;
// ... etc
```

### Dispositivos Implementados

| Dispositivo | Firmware Path | JJManager Class | Status |
|-------------|---------------|-----------------|--------|
| **JJB999** | `D:\OneDrive\JohnJohn3D\02 - Producao\01 - Sim Racing\16 - ButtonBox JJB-999\02 - Source\02 - Programacao\JJB999_code_2025-12-26\` | `Class/Devices/JJB999.cs` | ✅ Implementado |
| **JJB01_V2** | `D:\OneDrive\JohnJohn3D\02 - Producao\01 - Sim Racing\18 - ButtonBox JJB-01 V2\02 - Source\02 - Programacao\Code_JJB01_V2_2025-12-25\` | `Class/Devices/JJB01_V2 .cs` | ✅ Implementado |

### Checklist de Migração para Novos Dispositivos

Para aplicar este protocolo em outros dispositivos JJB:

#### Firmware (Arduino):
- [ ] Adicionar `KEEP_ALIVE_TIMEOUT_MS` em `Configs.hpp`
- [ ] Atualizar `JJHID.hpp` com tracking de keep-alive
- [ ] Reescrever `JJHID.cpp`:
  - [ ] `convertMessage()`: Parser byte-based (remover JSON)
  - [ ] `sendToJJManager()`: Usar FLAG_L = CMD_L
  - [ ] `receiveData()`: Processar pacotes HID diretamente
  - [ ] `checkTimeout()`: Implementar timeout de 5s
- [ ] Adicionar `jjhid.checkTimeout()` no loop principal do `.ino`

#### JJManager (C#):
- [ ] Device Class (`Class/Devices/[DEVICE].cs`):
  - [ ] Adicionar `using System.Linq;`
  - [ ] Adicionar variáveis de keep-alive tracking
  - [ ] Adicionar variáveis de change tracking
  - [ ] Reescrever `SendData()` para byte-based protocol
  - [ ] Implementar envio com FLAGS corretos (0x20, CMD_L)
  - [ ] Implementar change tracking
  - [ ] Implementar keep-alive (LED Mode a cada 3s)
  - [ ] Suporte SimHub (se aplicável)

- [ ] UI Page (`Pages/Devices/[DEVICE].cs`):
  - [ ] Atualizar using statements
  - [ ] Implementar `LoadFormData()` com DataSource pattern
  - [ ] Atualizar `SaveConfig()` para salvar todos os 4 parâmetros
  - [ ] Implementar slider change handler para real-time update
  - [ ] Suporte a DataGridView para SimHub outputs (opcional)

### Vantagens do Protocolo Byte-Based

1. **Performance:** Menor overhead comparado a JSON
2. **Validação:** FLAG_L = CMD_L fornece validação básica
3. **Eficiência:** Change tracking evita envios redundantes
4. **Confiabilidade:** Keep-alive garante LED não desligue inesperadamente
5. **Escalabilidade:** Fácil adicionar novos comandos (0x0004, 0x0005, etc.)
6. **Debugging:** Formato simples facilita análise com monitor serial

### Exemplo de Debug

**Monitor Serial (Firmware):**
```
>>> CMD: 0x0000 | Payload: 1 bytes
LED Mode: 1

>>> CMD: 0x0001 | Payload: 1 bytes
Brightness: 100

>>> CMD: 0x0002 | Payload: 1 bytes
Blink Speed: 5
```

**Captura HID (Wireshark/USBPcap):**
```
00 00 01 20 00  → LED Mode = 1
00 01 64 20 01  → Brightness = 100 (0x64)
00 02 05 20 02  → Blink Speed = 5
00 03 05 20 03  → Pulse Delay = 5
```

---

## 16. Sistema de Filtragem de Sessões de Áudio por Input (2026-01-09)

### Problema Solucionado

**Sintomas:**
- Troca de perfil causava travamento/desconexão do dispositivo JJM01
- AudioController não atualizava corretamente após troca de perfil
- Inputs de diferentes perfis podiam controlar os mesmos apps simultaneamente

**Causa Raiz:**
- Sessões de áudio estáticas (`_sharedSessionsGrouped`) não eram filtradas por `_toManage`
- Cada dispositivo recriava MMDeviceEnumerator ao conectar/reconectar
- Dispose do enumerator compartilhado quebrava comunicação de todos os dispositivos

### Solução Implementada

**Arquitetura: Sessões Globais + Filtragem On-Demand**

1. **Inicialização Única (Fase 0)**
   - Sessões globais criadas UMA VEZ na primeira conexão de qualquer dispositivo
   - Dispositivos subsequentes reutilizam as mesmas sessões
   - Callbacks (SessionCreated/Disconnected) mantêm sincronização automática

2. **Filtragem On-Demand por Input (Fase 1-3)**
   - Propriedade `SessionsToControl` filtra em tempo real baseado em `_toManage`
   - Método `ChangeAppVolume()` valida que app está em `_toManage` antes de controlar
   - Zero cache = dados sempre atualizados

### Arquivos Modificados

**AudioController.cs:**
- Linhas 53-55: Adicionadas flags `_sharedInitialized`, `_sharedDeviceEnumerator`
- Linhas 620-768: GetNewCoreAudioController() modificado para reutilizar sessões globais
- Linhas 79-117: SessionsToControl agora filtra on-demand (sem cache)
- Linhas 947-983: Novo método FindSessionForApp() com validação de _toManage
- Linhas 1203-1214: ChangeAppVolume() valida _toManage antes de buscar sessões

**JJM01.cs:**
- Linhas 71-73: CleanupAudioResources() não faz mais dispose do enumerator compartilhado
- Linhas 319-324: UpdateCoreAudioController() só faz dispose se enumerator for diferente

### Benefícios

✅ **Inicialização Única:** Menos overhead, menos bugs
✅ **Dados Sempre Atualizados:** Zero stale data (nenhum cache)
✅ **Isolamento por Input:** Cada input controla apenas apps em seu `_toManage`
✅ **Simplicidade:** Sem sincronização complexa, sem variáveis extras
✅ **Fix Definitivo:** Troca de perfil não trava mais o dispositivo

### Comportamento Esperado

**Troca de Perfil:**
- Perfil A: Input 1 controla Firefox → Perfil B: Input 1 controla Chrome
- Mover potenciômetro 1 sempre controla SOMENTE o app do perfil ativo
- Device não trava/desconecta durante troca

**Inicialização:**
- Primeira conexão: Log "Inicializando sessões globais pela primeira vez"
- Reconexões: Log "Reutilizando sessões globais já inicializadas"

**Isolamento:**
- Input 1 com Firefox + Input 2 com Chrome = ambos funcionam independentemente
- Input só controla apps listados em seu `_toManage` (validação automática)

---

## 17. Sistema de Aplicação Automática de Volume e Melhorias no AudioMonitor (2026-01-09)

### Problemas Solucionados

**Sintomas:**
1. DefaultDeviceChanged causava loop infinito com CPU/memory leak
2. Apps novos não eram detectados (sessões de dispositivos secundários não capturadas)
3. Volume não era aplicado automaticamente quando apps abriam ou dispositivos eram habilitados
4. UI não bloqueava durante troca de perfil (permitindo cliques indesejados)
5. Troca de perfil rápida (P1 → P2 → P1) travava comunicação do dispositivo

**Causas Raiz:**
1. AudioMonitor era criado por instância, gerando múltiplos callbacks duplicados
2. Stop/Start do monitor dentro do DefaultDeviceChanged retriggava o evento
3. GetNewCoreAudioController() só enumerava DeviceState.Active (ignorava dispositivos secundários)
4. Profile era alterado diretamente da UI thread durante execução do device loop
5. Nenhuma aplicação automática de volume em callbacks de mudança de estado/volume

### Soluções Implementadas

#### 1. AudioMonitor Estático (Singleton)

**Arquivo:** AudioController.cs linha 44
```csharp
private static AudioMonitor _audioMonitor = null; // Shared static monitor for all AudioControllers
```

**Benefícios:**
- ✅ UMA única instância de AudioMonitor para todos os AudioControllers
- ✅ Callbacks registrados apenas uma vez (não duplicados)
- ✅ Menor overhead de memória e CPU
- ✅ Elimina race conditions entre múltiplos monitors

#### 2. Lista de AudioControllers Ativos

**Arquivo:** AudioController.cs linhas 57-58
```csharp
private static List<AudioController> _activeControllers = new List<AudioController>();
```

**Uso:** Rastrear todos os AudioControllers ativos para aplicar volume inicial quando:
- Nova sessão é criada (app abre)
- Sessão muda de estado (ativo/inativo)
- Volume de sessão é alterado externamente
- Novo dispositivo é adicionado
- Dispositivo muda de estado (ativo/desabilitado)

**Registro:** AudioController.cs linhas 168-187 (InitializeClassAttribs)

#### 3. Captura de Sessões de TODOS os Dispositivos

**Arquivo:** AudioController.cs linhas 555-569

**ANTES:**
```csharp
var playbackDevices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
```

**DEPOIS:**
```csharp
var playbackDevices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.All);

// Skip apenas dispositivos desabilitados/inválidos
if (device == null || device.State == DeviceState.Disabled || device.State == DeviceState.NotPresent)
{
    continue;
}
```

**Benefícios:**
- ✅ Captura sessões de dispositivos secundários (headphones desabilitados, etc.)
- ✅ Apps em execução em dispositivos não-default são detectados
- ✅ Volume pode ser aplicado mesmo antes de trocar dispositivo padrão

#### 4. DefaultDeviceChanged Simplificado

**Arquivo:** AudioController.cs linhas 491-496

**ANTES:**
```csharp
_audioMonitor.DefaultDeviceChanged += (sender, e) =>
{
    if (_audioMonitor.IsMonitoring)
    {
        _audioMonitor.StopMonitoring();
    }
    _audioMonitor.StartMonitoring();
};
```

**DEPOIS:**
```csharp
_audioMonitor.DefaultDeviceChanged += (sender, e) =>
{
    // DefaultDeviceChanged: Não fazer nada
    // As sessões são mantidas automaticamente pelos callbacks SessionCreated/Disconnected
    // O volume será aplicado quando o usuário mover o potenciômetro (RequestData → Execute)
};
```

**Benefícios:**
- ✅ Elimina loop infinito (Stop → trigger DefaultDeviceChanged → Start → trigger DefaultDeviceChanged...)
- ✅ Zero CPU/memory leak
- ✅ Sessões mantidas pelos callbacks específicos (SessionCreated/Disconnected)

#### 5. Sistema de Aplicação Automática de Volume (5 Callbacks)

##### 5.1. SessionCreated (linhas 196-262)
**Quando:** Nova sessão de áudio é criada (app abre)
**Ação:** Itera todos os AudioControllers ativos, verifica se app está em `_toManage`, aplica volume configurado

##### 5.2. SessionVolumeChanged (linhas 283-333)
**Quando:** Volume de uma sessão é alterado externamente (outro app, mixer do Windows)
**Ação:** Reaplica volume configurado (sobrescreve mudança externa)

##### 5.3. SessionStateChanged (linhas 335-391)
**Quando:** Sessão muda de estado (especialmente quando fica AudioSessionStateActive)
**Ação:** Aplica volume configurado quando sessão fica ativa

##### 5.4. DeviceAdded (linhas 393-450)
**Quando:** Novo dispositivo de áudio é adicionado ao sistema
**Ação:** Aplica volume configurado para inputs que gerenciam aquele device (modo DevicePlayback/DeviceRecord)

##### 5.5. DeviceStateChanged (linhas 475-534)
**Quando:** Dispositivo muda de estado (desabilitado → ativo)
**Ação:** Aplica volume configurado quando dispositivo fica ativo

**Código Compartilhado (Padrão):**
```csharp
// Buscar nome da sessão/dispositivo
string sessionName = ...;

// Obter cópia da lista de controllers ativos
List<AudioController> controllers;
lock (_sharedLock)
{
    controllers = _activeControllers.ToList();
}

// Iterar e aplicar volume onde aplicável
foreach (var controller in controllers)
{
    var managedApp = controller._toManage?.FirstOrDefault(app => ...);
    if (managedApp != null && controller._settedVolume >= 0 && controller._audioMode == AudioMode.Application)
    {
        await controller.ChangeAppVolume(managedApp, controller._settedVolume);
    }
}
```

#### 6. UI Blocking Durante Troca de Perfil

**Arquivos Modificados:**
- **Pages/Devices/JJM01.cs** (UI Form)
- **Class/Devices/JJM01.cs** (Device Class)

##### 6.1. UI Form - Flags e Timer

**Linhas 28-30:**
```csharp
private System.Windows.Forms.Timer _profileSwitchMonitor = null;
private bool _isProfileSwitching = false;
private bool _loadingProfiles = false;
```

**Linhas 100-152:** Métodos BlockUIControls() e UnblockUIControls()
- Desabilita dropdown de perfil, botões de input, botões de gerenciamento
- Reabilita após profile switch completo E comunicação saudável

**Linhas 157-186:** Timer ProfileSwitchMonitor_Tick
- Verifica se NeedsUpdate = false E IsCommunicationHealthy = true
- Desbloqueia UI apenas quando ambas condições satisfeitas

##### 6.2. Device Class - RequestProfileChange

**Class/Devices/JJM01.cs linhas 27, 30:**
```csharp
private volatile string _pendingProfileName = null;
public bool IsCommunicationHealthy => _actualConnectionTimeout == 0;
```

**Linhas 63-66:** Método RequestProfileChange()
```csharp
public void RequestProfileChange(string profileName)
{
    _pendingProfileName = profileName;
}
```

**Linhas 130-161:** Handling no INÍCIO do ActionMainLoop
- Verifica `_pendingProfileName != null` ANTES de qualquer outra operação
- Cria novo perfil de forma thread-safe (dentro do device loop)
- Sinaliza UpdateSessionsToControl para todos os inputs
- Marca NeedsUpdate = false após conclusão
- **IMPORTANTE:** Pula para próxima iteração (não executa mais nada durante troca)

##### 6.3. UI Form - SelectedIndexChanged

**Linhas 222-254:**
```csharp
// Don't process if we're just loading the profile list
if (_loadingProfiles)
{
    return;
}

// Block UI during profile switch
BlockUIControls();

// Request profile change in a thread-safe manner
var jjm01Device = _device as Class.Devices.JJM01;
if (jjm01Device != null)
{
    jjm01Device.RequestProfileChange(CmbBoxSelectProfile.SelectedItem.ToString());
}

// Start monitoring for profile switch completion
_profileSwitchMonitor.Start();
```

#### 7. Profile Setter com Auto NeedsUpdate

**Arquivo:** JJDevice.cs linhas 102-117

**ANTES:**
```csharp
public ProfileClass Profile
{
    get => _profile;
    set => _profile = value;
}
```

**DEPOIS:**
```csharp
public ProfileClass Profile
{
    get => _profile;
    set
    {
        if (_profile != value)
        {
            _profile = value;
            if (_profile != null)
            {
                _profile.NeedsUpdate = true;
            }
        }
    }
}
```

**Benefícios:**
- ✅ Setter automático de NeedsUpdate quando perfil muda
- ✅ Garante que device loop detecta mudança de perfil
- ✅ Elimina necessidade de setar flag manualmente

### Fluxo Completo de Troca de Perfil

```
1. Usuário clica dropdown e seleciona novo perfil
   ↓
2. CmbBoxSelectProfile_SelectedIndexChanged
   - BlockUIControls() desabilita toda a UI
   - RequestProfileChange(newProfileName) seta _pendingProfileName
   - Timer _profileSwitchMonitor.Start() inicia monitoramento
   ↓
3. ActionMainLoop detecta _pendingProfileName != null
   - Adquire lock _updatingAudioController
   - Cria novo ProfileClass com nome solicitado
   - Sinaliza UpdateSessionsToControl para todos inputs
   - SendInputs() envia configurações para device
   - NeedsUpdate = false
   - Pula para próxima iteração (não executa RequestData)
   ↓
4. RequestData() em próximas iterações
   - Valida que device ainda está conectado
   - Recebe dados de volume dos potenciômetros
   - IsCommunicationHealthy = true quando timeout = 0
   ↓
5. ProfileSwitchMonitor_Tick (a cada 100ms)
   - Verifica NeedsUpdate = false (perfil processado)
   - Verifica IsCommunicationHealthy = true (comunicação OK)
   - Quando AMBOS true: Stop() timer e UnblockUIControls()
   ↓
6. UI desbloqueada, usuário pode continuar usando
```

### Arquivos Modificados (Resumo)

| Arquivo | Mudanças | Linhas |
|---------|----------|--------|
| **AudioController.cs** | AudioMonitor estático, _activeControllers, 5 callbacks, DeviceState.All | 44, 57-58, 168-187, 196-534, 555-569 |
| **JJM01.cs** (Class) | _pendingProfileName, IsCommunicationHealthy, RequestProfileChange(), ActionMainLoop handling | 21, 27, 30, 63-66, 130-161 |
| **JJM01.cs** (UI) | UI blocking flags/timer, BlockUIControls, UnblockUIControls, monitor tick, SelectedIndexChanged | 28-30, 100-186, 222-254 |
| **JJDevice.cs** | Profile setter com auto NeedsUpdate | 102-117 |

### Benefícios Consolidados

✅ **Zero CPU/Memory Leak:** AudioMonitor estático elimina duplicação de callbacks
✅ **Detecção Completa:** Captura sessões de TODOS os dispositivos (não só default)
✅ **Volume Automático:** 5 callbacks aplicam volume inicial em todos os cenários
✅ **UI Bloqueada:** Impossível causar race conditions durante troca de perfil
✅ **Thread-Safe:** RequestProfileChange() garante que profile change acontece no device loop
✅ **Comunicação Estável:** Troca de perfil não trava/desconecta dispositivo
✅ **UX Melhorada:** Feedback visual de bloqueio durante operações críticas

### Testes Recomendados

1. **Troca Rápida de Perfil:** P1 → P2 → P1 → P2 (sem travamentos)
2. **Abrir App Durante Uso:** Volume aplicado automaticamente
3. **Alterar Volume Externamente:** JJManager reaplica volume configurado
4. **Habilitar Dispositivo Desabilitado:** Volume aplicado quando fica ativo
5. **Trocar Dispositivo Padrão:** Sem loop infinito, sem lag
6. **Múltiplos Inputs Independentes:** Cada input controla apenas seus apps

---

*Documento consolidado automaticamente para contexto de desenvolvimento*
*Atualizado em: 2026-01-09*
