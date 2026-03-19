namespace SigilLauncher.Models;

/// <summary>
/// Represents the lifecycle state of the NixOS virtual machine.
/// </summary>
public enum VMState
{
    Stopped,
    Starting,
    Running,
    Stopping,
    Error,
}

public static class VMStateExtensions
{
    public static string DisplayName(this VMState state) => state switch
    {
        VMState.Stopped => "Stopped",
        VMState.Starting => "Starting...",
        VMState.Running => "Running",
        VMState.Stopping => "Stopping...",
        VMState.Error => "Error",
        _ => "Unknown",
    };

    public static bool IsTransitioning(this VMState state) =>
        state is VMState.Starting or VMState.Stopping;
}
