using SigilLauncher.Services;
using Xunit;

namespace SigilLauncher.Tests;

public class HardwareDetectorTests
{
    [Fact]
    public void Recommend_16GB8Cores_Returns8GB4Cores()
    {
        var hw = new HardwareInfo { TotalRAMGB = 16, CpuCores = 8, CpuArch = "x64", DiskAvailableGB = 100 };
        var rec = HardwareDetector.Recommend(hw);
        Assert.Equal(8, rec.MemoryGB);
        Assert.Equal(4, rec.Cpus);
        Assert.Equal(20, rec.DiskGB);
    }

    [Fact]
    public void Recommend_8GB4Cores_Returns4GB2Cores()
    {
        var hw = new HardwareInfo { TotalRAMGB = 8, CpuCores = 4, CpuArch = "x64", DiskAvailableGB = 50 };
        var rec = HardwareDetector.Recommend(hw);
        Assert.Equal(4, rec.MemoryGB);
        Assert.Equal(2, rec.Cpus);
    }

    [Fact]
    public void Recommend_ClampsMaxMemoryAt12GB()
    {
        var hw = new HardwareInfo { TotalRAMGB = 64, CpuCores = 16, CpuArch = "x64", DiskAvailableGB = 500 };
        var rec = HardwareDetector.Recommend(hw);
        Assert.Equal(12, rec.MemoryGB);
    }

    [Fact]
    public void Recommend_ClampsMinMemoryAt4GB()
    {
        var hw = new HardwareInfo { TotalRAMGB = 4, CpuCores = 2, CpuArch = "x64", DiskAvailableGB = 50 };
        var rec = HardwareDetector.Recommend(hw);
        Assert.Equal(4, rec.MemoryGB);
    }

    [Fact]
    public void Recommend_ClampsMinCpusAt2()
    {
        var hw = new HardwareInfo { TotalRAMGB = 8, CpuCores = 2, CpuArch = "x64", DiskAvailableGB = 50 };
        var rec = HardwareDetector.Recommend(hw);
        Assert.Equal(2, rec.Cpus);
    }

    [Fact]
    public void MeetsMinimum_FailsBelow8GB()
    {
        var hw = new HardwareInfo { TotalRAMGB = 4, CpuCores = 4, CpuArch = "x64", DiskAvailableGB = 50 };
        var (meets, reason) = HardwareDetector.MeetsMinimumRequirements(hw);
        Assert.False(meets);
        Assert.NotNull(reason);
        Assert.Contains("8GB", reason!);
    }

    [Fact]
    public void MeetsMinimum_FailsBelow10GBDisk()
    {
        var hw = new HardwareInfo { TotalRAMGB = 16, CpuCores = 8, CpuArch = "x64", DiskAvailableGB = 5 };
        var (meets, reason) = HardwareDetector.MeetsMinimumRequirements(hw);
        Assert.False(meets);
        Assert.NotNull(reason);
        Assert.Contains("10GB", reason!);
    }

    [Fact]
    public void MeetsMinimum_PassesAt8GB()
    {
        var hw = new HardwareInfo { TotalRAMGB = 8, CpuCores = 4, CpuArch = "x64", DiskAvailableGB = 50 };
        var (meets, reason) = HardwareDetector.MeetsMinimumRequirements(hw);
        Assert.True(meets);
        Assert.Null(reason);
    }

    [Fact]
    public void MeetsMinimum_PassesWithLargeSystem()
    {
        var hw = new HardwareInfo { TotalRAMGB = 64, CpuCores = 16, CpuArch = "x64", DiskAvailableGB = 500 };
        var (meets, reason) = HardwareDetector.MeetsMinimumRequirements(hw);
        Assert.True(meets);
        Assert.Null(reason);
    }
}
