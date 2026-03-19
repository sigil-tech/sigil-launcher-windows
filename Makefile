SIGIL_OS_DIR ?= ../sigil-os
IMAGE_DIR ?= $(APPDATA)\Sigil\images
PROFILE_DIR ?= $(APPDATA)\Sigil\profiles\default

.PHONY: build run clean release setup extract-vm-image

# Build the C# launcher
build:
	dotnet build SigilLauncher.sln -c Debug -p:Platform=x64

# Run the launcher (debug)
run: setup
	dotnet run --project SigilLauncher/SigilLauncher.csproj -c Debug -p:Platform=x64

# Build release binary
release:
	dotnet build SigilLauncher.sln -c Release -p:Platform=x64

clean:
	dotnet clean SigilLauncher.sln

# Create required directories for first run
setup:
	@if not exist "$(IMAGE_DIR)" mkdir "$(IMAGE_DIR)"
	@if not exist "$(PROFILE_DIR)" mkdir "$(PROFILE_DIR)"

# Build the NixOS VHDX image for Hyper-V.
# Requires: nix on a Linux builder (native or remote).
# Run this once, or whenever the VM config changes.
extract-vm-image:
	@echo Building launcher VM system (x86_64-linux)...
	cd $(SIGIL_OS_DIR) && nix build .#nixosConfigurations.sigil-launcher-windows.config.system.build.toplevel --out-link result-launcher-windows
	@echo System closure built. Convert to VHDX for Hyper-V.
	@echo Steps:
	@echo   1. Create a VHDX with GPT: EFI (sda1, FAT32) + root (sda2, ext4)
	@echo   2. Install the system closure to the root partition
	@echo   3. Install GRUB EFI to the EFI partition
	@echo   4. Copy the VHDX to $(IMAGE_DIR)\sigil-vm.vhdx
