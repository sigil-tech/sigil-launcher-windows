using System.Diagnostics;

namespace SigilLauncher.Services;

/// <summary>
/// Runs external processes asynchronously and captures output.
/// Uses pwsh.exe (PowerShell 7) with fallback to powershell.exe.
/// </summary>
public static class ProcessRunner
{
    private static readonly string PowerShellPath = FindPowerShell();

    private static string FindPowerShell()
    {
        // Prefer PowerShell 7 (pwsh.exe), fall back to Windows PowerShell
        var pwsh = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "PowerShell", "7", "pwsh.exe");
        if (File.Exists(pwsh)) return pwsh;
        return "powershell.exe";
    }

    /// <summary>
    /// Runs a PowerShell command and returns stdout.
    /// </summary>
    public static async Task<(int ExitCode, string Output, string Error)> RunPowerShellAsync(
        string command, CancellationToken ct = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = PowerShellPath,
            Arguments = $"-NoProfile -NonInteractive -Command \"{command.Replace("\"", "\\\"")}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        return await RunAsync(psi, ct);
    }

    /// <summary>
    /// Runs an arbitrary executable and returns stdout.
    /// </summary>
    public static async Task<(int ExitCode, string Output, string Error)> RunAsync(
        string fileName, string arguments, CancellationToken ct = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        return await RunAsync(psi, ct);
    }

    private static async Task<(int ExitCode, string Output, string Error)> RunAsync(
        ProcessStartInfo psi, CancellationToken ct)
    {
        using var process = new Process { StartInfo = psi };
        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync(ct);
        var errorTask = process.StandardError.ReadToEndAsync(ct);

        await process.WaitForExitAsync(ct);

        var output = await outputTask;
        var error = await errorTask;

        return (process.ExitCode, output.Trim(), error.Trim());
    }
}
