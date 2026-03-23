# Sigil Launcher for Windows

Native Windows launcher that manages ephemeral NixOS virtual machine workspaces via Hyper-V. Built with WinUI 3 and .NET 8.

Sigil Launcher creates, configures, and monitors a NixOS VM running `sigild` (the Sigil intelligence daemon), then connects the Sigil Shell frontend to it. The host filesystem is shared into the VM via SMB, so your code stays on the host while the AI-assisted development environment runs inside the VM.

## Prerequisites

- **Windows 11** (or Windows 10 with Hyper-V support)
- **.NET 8 SDK** (8.0 or later)
- **Hyper-V** enabled (see [Enabling Hyper-V](#enabling-hyper-v))
- **Nix** package manager (native Windows install or via WSL) for building VM images
- **OpenSSH client** (ships with Windows 10+)

## Quick Start

```powershell
# Clone the repository
git clone https://github.com/sigil-tech/sigil-launcher-windows.git
cd sigil-launcher-windows

# Build
dotnet build SigilLauncher.sln -c Debug -p:Platform=x64

# Run (creates required directories first)
make run

# Run tests
dotnet test SigilLauncher.Tests/SigilLauncher.Tests.csproj
```

Or use the Makefile targets:

```powershell
make build      # Debug build
make release    # Release build
make run        # Build + run (creates app directories)
make clean      # Clean build artifacts
```

## Architecture

```
┌──────────────────────────────────────────────┐
│  WinUI 3 App (SigilLauncher)                 │
│  ┌────────────┐  ┌────────────────────────┐  │
│  │   Views     │  │  LauncherViewModel     │  │
│  │  (XAML)     │──│  (MVVM via             │  │
│  │             │  │   CommunityToolkit)    │  │
│  └────────────┘  └──────────┬─────────────┘  │
│                             │                │
│  ┌──────────────────────────▼─────────────┐  │
│  │  VMManager                             │  │
│  │  - Start/Stop VM lifecycle             │  │
│  │  - Health check polling (30s interval) │  │
│  │  - Crash detection                     │  │
│  └──────┬────────────────────┬────────────┘  │
│         │                    │               │
│  ┌──────▼──────┐  ┌─────────▼────────────┐  │
│  │ VMConfig    │  │  HealthChecker       │  │
│  │ (PowerShell │  │  - SSH poll          │  │
│  │  scripts)   │  │  - Daemon poll       │  │
│  └──────┬──────┘  └─────────┬────────────┘  │
│         │                    │               │
│         ▼                    ▼               │
│    Hyper-V API          SSH (ssh.exe)        │
│    (PowerShell)         to NixOS VM          │
└──────────────────────────────────────────────┘
                    │
                    ▼
       ┌────────────────────────┐
       │  NixOS VM (Hyper-V)   │
       │  - sigild daemon      │
       │  - llama.cpp (local   │
       │    inference)         │
       │  - SMB mounts for     │
       │    /workspace,        │
       │    /profile, /models  │
       └────────────────────────┘
```

### Key Components

| Component | Path | Purpose |
|-----------|------|---------|
| **Models** | `Models/` | `LauncherProfile` (settings), `ModelCatalog` (GGUF models), `VMState` (lifecycle enum) |
| **Services** | `Services/` | `HardwareDetector` (host capabilities), `ProcessRunner` (PowerShell/exec), `ImageBuilder` (Nix builds), `ModelManager` (GGUF downloads), `SshClient` (VM communication) |
| **VM** | `VM/` | `VMManager` (lifecycle + health), `VMConfiguration` (Hyper-V PowerShell scripts), `HealthChecker` (SSH/daemon polling), `CredentialBootstrap` (TLS setup) |
| **ViewModels** | `ViewModels/` | `LauncherViewModel` (MVVM bindings via CommunityToolkit.Mvvm) |
| **Views** | `Views/` | `LauncherView` (main window), `ConfigurationView` (settings), `SetupWizard` (first-run), `ReadinessIndicator` (status) |

## Directory Structure

```
sigil-launcher-windows/
├── SigilLauncher/                 # Main WinUI 3 application
│   ├── Models/
│   │   ├── LauncherProfile.cs     # Persisted settings (JSON)
│   │   ├── ModelCatalog.cs        # Available GGUF models
│   │   └── VMState.cs             # VM lifecycle states
│   ├── Services/
│   │   ├── HardwareDetector.cs    # Host hardware detection
│   │   ├── ImageBuilder.cs        # Nix-based VM image builder
│   │   ├── ModelManager.cs        # Model download manager
│   │   ├── ProcessRunner.cs       # Process execution wrapper
│   │   └── SshClient.cs           # SSH command runner
│   ├── VM/
│   │   ├── CredentialBootstrap.cs  # TLS credential setup
│   │   ├── HealthChecker.cs       # SSH + daemon health polling
│   │   ├── VMConfiguration.cs     # Hyper-V PowerShell generators
│   │   └── VMManager.cs           # VM lifecycle management
│   ├── ViewModels/
│   │   └── LauncherViewModel.cs   # MVVM view model
│   ├── Views/
│   │   ├── LauncherView.xaml      # Main launcher UI
│   │   ├── ConfigurationView.xaml # Settings panel
│   │   ├── SetupWizard.xaml       # First-run wizard
│   │   └── ReadinessIndicator.xaml
│   ├── App.xaml.cs                # Application entry + converters
│   ├── Program.cs                 # WinUI bootstrap
│   └── SigilLauncher.csproj
├── SigilLauncher.Tests/           # xUnit test project
│   ├── LauncherProfileTests.cs
│   ├── ModelCatalogTests.cs
│   ├── HardwareDetectorTests.cs
│   ├── VMStateTests.cs
│   └── SigilLauncher.Tests.csproj
├── .github/workflows/ci.yml      # GitHub Actions CI
├── SigilLauncher.sln
├── Makefile
└── global.json
```

## First-Run Wizard

On first launch, the setup wizard guides you through six steps:

1. **Welcome** -- overview of what Sigil does
2. **Hardware Detection** -- scans CPU, RAM, disk, GPU; checks minimum requirements (8 GB RAM, 10 GB disk)
3. **Resource Allocation** -- recommends VM resources (50% RAM capped at 12 GB, 50% cores min 2)
4. **Tool Selection** -- choose editor (VS Code / Neovim / both / none), container engine, shell, notification level
5. **Model Selection** -- pick a local GGUF model for on-device inference (filtered by available RAM)
6. **Build** -- generates a Nix flake wrapper and builds the NixOS VM image

After the wizard completes, your settings are saved to `%APPDATA%\Sigil\launcher\settings.json`.

## Configuration

Settings are stored at `%APPDATA%\Sigil\launcher\settings.json`. Key fields:

| Field | Default | Description |
|-------|---------|-------------|
| `memorySize` | 4 GB (bytes) | RAM allocated to the VM |
| `cpuCount` | 2 | CPU cores allocated |
| `workspacePath` | `%USERPROFILE%\workspace` | Host directory mounted as `/workspace` |
| `editor` | `vscode` | Editor in VM: `vscode`, `neovim`, `both`, `none` |
| `containerEngine` | `docker` | Container engine: `docker`, `none` |
| `shell` | `zsh` | Default shell: `zsh`, `bash` |
| `notificationLevel` | 2 | 0=silent, 1=digest, 2=ambient, 3=conversational, 4=autonomous |
| `modelId` | `null` | Local GGUF model ID, or null for cloud-only |
| `vmName` | `SigilVM` | Hyper-V VM name |
| `vmSwitchName` | `Default Switch` | Hyper-V virtual switch |

Changing `editor`, `containerEngine`, `shell`, or `modelId` requires a VM image rebuild. Changes to `memorySize` and `cpuCount` take effect on next VM start without rebuilding.

## Enabling Hyper-V

Hyper-V is required to run the NixOS VM. To enable it:

### Via PowerShell (Administrator)

```powershell
Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V -All -Restart
```

### Via Settings

1. Open **Settings > Apps > Optional Features > More Windows Features**
2. Check **Hyper-V** (both Management Tools and Platform)
3. Restart your computer

### Requirements

- Windows 11 Pro, Enterprise, or Education (Home edition does not include Hyper-V)
- Hardware virtualization enabled in BIOS/UEFI (Intel VT-x or AMD-V)
- At least 8 GB system RAM

## Available Models

The launcher includes three GGUF models for local inference:

| Model | Parameters | Size | Min VM RAM | Best For |
|-------|-----------|------|------------|----------|
| Qwen 2.5 1.5B | 1.5B | 1.0 GB | 3 GB | Fast suggestions, constrained hardware |
| Phi-3 Mini 3.8B | 3.8B | 2.5 GB | 5 GB | Balance of speed and quality |
| LLaMA 3.1 8B | 8B | 4.5 GB | 8 GB | Best quality, needs 8+ GB VM RAM |

Models are downloaded to `%APPDATA%\Sigil\models\` and shared into the VM via SMB.

## Troubleshooting

### "Hyper-V is not available"
- Ensure Hyper-V is enabled (see above)
- Verify hardware virtualization is on in BIOS
- Windows Home does not support Hyper-V; upgrade to Pro

### VM fails to start
- Check that the VM image has been built (`%APPDATA%\Sigil\images\vmlinuz` exists)
- Run `Get-VM` in PowerShell to see if the VM exists and its state
- Check the Default Switch: `Get-VMSwitch`

### SSH connection fails
- The VM needs 10-30 seconds to boot and get an IP address
- Check `Get-VMNetworkAdapter -VMName SigilVM` for assigned IPs
- Ensure Windows OpenSSH client is installed: `ssh -V`

### sigild not starting
- SSH into the VM: `ssh sigil@<vm-ip>`
- Check daemon status: `systemctl status sigild`
- View logs: `journalctl -u sigild -f`

### Build fails (Nix)
- Ensure Nix is installed and on PATH
- Check that `nix --version` works
- The build requires internet access to fetch NixOS packages

### Model download stalls
- Large models (up to 4.5 GB) may take time on slow connections
- Cancel and retry via the configuration panel
- Check available disk space in `%APPDATA%\Sigil\models\`

## License

Apache License 2.0
