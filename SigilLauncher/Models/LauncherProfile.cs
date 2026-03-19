using System.Text.Json;

namespace SigilLauncher.Models;

/// <summary>
/// Persisted launcher settings stored at %APPDATA%\Sigil\launcher\settings.json.
/// </summary>
public class LauncherProfile
{
    /// <summary>RAM allocated to the VM in bytes.</summary>
    public long MemorySize { get; set; }

    /// <summary>Number of CPU cores allocated to the VM.</summary>
    public int CpuCount { get; set; }

    /// <summary>Host directory shared as /workspace in the VM.</summary>
    public string WorkspacePath { get; set; } = string.Empty;

    /// <summary>Path to the VHDX disk image.</summary>
    public string DiskImagePath { get; set; } = string.Empty;

    /// <summary>Hyper-V VM name.</summary>
    public string VmName { get; set; } = "SigilVM";

    /// <summary>Hyper-V virtual switch name.</summary>
    public string VmSwitchName { get; set; } = "Default Switch";

    public static string SettingsDir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sigil", "launcher");

    public static string SettingsPath =>
        Path.Combine(SettingsDir, "settings.json");

    public static string ProfileDir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sigil", "profiles", "default");

    public static LauncherProfile Default => new()
    {
        MemorySize = 4L * 1024 * 1024 * 1024, // 4 GB
        CpuCount = 2,
        WorkspacePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "workspace"),
        DiskImagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sigil", "images", "sigil-vm.vhdx"),
        VmName = "SigilVM",
        VmSwitchName = "Default Switch",
    };

    public static LauncherProfile Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<LauncherProfile>(json) ?? Default;
            }
        }
        catch
        {
            // Fall through to default
        }

        return Default;
    }

    public void Save()
    {
        Directory.CreateDirectory(SettingsDir);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsPath, json);
    }
}
