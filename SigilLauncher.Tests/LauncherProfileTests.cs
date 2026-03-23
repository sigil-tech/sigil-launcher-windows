using System.Text.Json;
using SigilLauncher.Models;
using Xunit;

namespace SigilLauncher.Tests;

public class LauncherProfileTests
{
    [Fact]
    public void DefaultProfile_HasExpectedValues()
    {
        var profile = new LauncherProfile();
        Assert.Equal("vscode", profile.Editor);
        Assert.Equal("docker", profile.ContainerEngine);
        Assert.Equal("zsh", profile.Shell);
        Assert.Equal(2, profile.NotificationLevel);
    }

    [Fact]
    public void DefaultProfile_HasSigilVMName()
    {
        var profile = new LauncherProfile();
        Assert.Equal("SigilVM", profile.VmName);
        Assert.Equal("Default Switch", profile.VmSwitchName);
    }

    [Fact]
    public void JsonRoundTrip_PreservesAllFields()
    {
        var profile = new LauncherProfile
        {
            Editor = "neovim",
            ModelId = "test-model",
            ContainerEngine = "none",
            Shell = "bash",
            NotificationLevel = 3,
            MemorySize = 8L * 1024 * 1024 * 1024,
            CpuCount = 4,
            VmName = "TestVM",
        };

        var json = JsonSerializer.Serialize(profile);
        var loaded = JsonSerializer.Deserialize<LauncherProfile>(json);

        Assert.NotNull(loaded);
        Assert.Equal("neovim", loaded!.Editor);
        Assert.Equal("test-model", loaded.ModelId);
        Assert.Equal("none", loaded.ContainerEngine);
        Assert.Equal("bash", loaded.Shell);
        Assert.Equal(3, loaded.NotificationLevel);
        Assert.Equal(8L * 1024 * 1024 * 1024, loaded.MemorySize);
        Assert.Equal(4, loaded.CpuCount);
        Assert.Equal("TestVM", loaded.VmName);
    }

    [Fact]
    public void BackwardCompatibility_MissingFieldsGetDefaults()
    {
        var oldJson = """{"memorySize":4294967296,"cpuCount":2}""";
        var profile = JsonSerializer.Deserialize<LauncherProfile>(oldJson);

        Assert.NotNull(profile);
        Assert.Equal("vscode", profile!.Editor);
        Assert.Equal("docker", profile.ContainerEngine);
        Assert.Equal("zsh", profile.Shell);
        Assert.Equal(2, profile.NotificationLevel);
        Assert.Null(profile.ModelId);
    }

    [Fact]
    public void NeedsRebuild_TrueWhenEditorChanges()
    {
        var a = new LauncherProfile { Editor = "vscode" };
        var b = new LauncherProfile { Editor = "neovim" };
        Assert.True(a.NeedsRebuild(b));
    }

    [Fact]
    public void NeedsRebuild_TrueWhenContainerEngineChanges()
    {
        var a = new LauncherProfile { ContainerEngine = "docker" };
        var b = new LauncherProfile { ContainerEngine = "none" };
        Assert.True(a.NeedsRebuild(b));
    }

    [Fact]
    public void NeedsRebuild_TrueWhenShellChanges()
    {
        var a = new LauncherProfile { Shell = "zsh" };
        var b = new LauncherProfile { Shell = "bash" };
        Assert.True(a.NeedsRebuild(b));
    }

    [Fact]
    public void NeedsRebuild_TrueWhenModelIdChanges()
    {
        var a = new LauncherProfile { ModelId = null };
        var b = new LauncherProfile { ModelId = "qwen2.5-1.5b-q4" };
        Assert.True(a.NeedsRebuild(b));
    }

    [Fact]
    public void NeedsRebuild_FalseWhenOnlyMemoryChanges()
    {
        var a = new LauncherProfile { MemorySize = 4L * 1024 * 1024 * 1024 };
        var b = new LauncherProfile { MemorySize = 8L * 1024 * 1024 * 1024 };
        Assert.False(a.NeedsRebuild(b));
    }

    [Fact]
    public void NeedsRebuild_FalseWhenOnlyCpuChanges()
    {
        var a = new LauncherProfile { CpuCount = 2 };
        var b = new LauncherProfile { CpuCount = 4 };
        Assert.False(a.NeedsRebuild(b));
    }

    [Fact]
    public void NeedsRebuild_FalseWhenIdentical()
    {
        var a = new LauncherProfile();
        var b = new LauncherProfile();
        Assert.False(a.NeedsRebuild(b));
    }
}
