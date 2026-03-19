namespace SigilLauncher.Services;

/// <summary>
/// SSH wrapper using the Windows built-in OpenSSH client (ssh.exe).
/// </summary>
public static class SshClient
{
    private const string SshPath = "ssh.exe";

    /// <summary>
    /// Runs a command on the VM via SSH.
    /// </summary>
    public static async Task<(int ExitCode, string Output, string Error)> RunCommandAsync(
        string host, string command, int timeoutSeconds = 5, CancellationToken ct = default)
    {
        var args = string.Join(" ", new[]
        {
            "-o", "StrictHostKeyChecking=no",
            "-o", $"ConnectTimeout={timeoutSeconds}",
            "-o", "BatchMode=yes",
            $"sigil@{host}",
            command,
        });

        return await ProcessRunner.RunAsync(SshPath, args, ct);
    }

    /// <summary>
    /// Tests if SSH is reachable on the given host.
    /// </summary>
    public static async Task<bool> TestConnectionAsync(string host, CancellationToken ct = default)
    {
        var (exitCode, _, _) = await RunCommandAsync(host, "echo ok", timeoutSeconds: 2, ct: ct);
        return exitCode == 0;
    }
}
