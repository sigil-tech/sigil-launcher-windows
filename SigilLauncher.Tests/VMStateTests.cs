using SigilLauncher.Models;
using Xunit;

namespace SigilLauncher.Tests;

public class VMStateTests
{
    [Theory]
    [InlineData(VMState.Stopped, "Stopped")]
    [InlineData(VMState.Starting, "Starting...")]
    [InlineData(VMState.Running, "Running")]
    [InlineData(VMState.Stopping, "Stopping...")]
    [InlineData(VMState.Error, "Error")]
    public void DisplayName_ReturnsExpectedString(VMState state, string expected)
    {
        Assert.Equal(expected, state.DisplayName());
    }

    [Theory]
    [InlineData(VMState.Starting, true)]
    [InlineData(VMState.Stopping, true)]
    [InlineData(VMState.Stopped, false)]
    [InlineData(VMState.Running, false)]
    [InlineData(VMState.Error, false)]
    public void IsTransitioning_ReturnsExpectedValue(VMState state, bool expected)
    {
        Assert.Equal(expected, state.IsTransitioning());
    }
}
