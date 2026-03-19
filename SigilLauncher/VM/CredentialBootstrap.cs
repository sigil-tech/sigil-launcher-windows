using SigilLauncher.Models;
using SigilLauncher.Services;

namespace SigilLauncher.VM;

/// <summary>
/// Bootstraps TLS credentials for sigil-shell → sigild connection.
/// On first run, generates a credential via sigilctl and writes it to the profile dir.
/// </summary>
public static class CredentialBootstrap
{
    public static async Task RunAsync(string host)
    {
        var credPath = Path.Combine(LauncherProfile.ProfileDir, "credentials.json");

        // Skip if credentials already exist
        if (File.Exists(credPath))
            return;

        var (exitCode, output, _) = await SshClient.RunCommandAsync(
            host, "sigilctl credential add sigil-shell", timeoutSeconds: 10);

        if (exitCode != 0 || string.IsNullOrWhiteSpace(output))
            return;

        // Write credential JSON from sigilctl output
        Directory.CreateDirectory(Path.GetDirectoryName(credPath)!);
        await File.WriteAllTextAsync(credPath, output);

        // Write daemon-settings.json for sigil-shell
        var shellConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "sigil-shell");
        Directory.CreateDirectory(shellConfigDir);

        var settings = $@"{{
  ""transport"": ""tcp"",
  ""tcp_credential_path"": ""{credPath.Replace("\\", "\\\\")}"",
  ""tcp_addr_override"": ""{host}:7773""
}}";
        await File.WriteAllTextAsync(Path.Combine(shellConfigDir, "daemon-settings.json"), settings);
    }
}
