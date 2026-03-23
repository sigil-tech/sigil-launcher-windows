using SigilLauncher.Models;
using Xunit;

namespace SigilLauncher.Tests;

public class ModelCatalogTests
{
    [Fact]
    public void Catalog_HasThreeModels()
    {
        Assert.Equal(3, ModelCatalog.Models.Count);
    }

    [Fact]
    public void AllModels_HaveRequiredFields()
    {
        foreach (var model in ModelCatalog.Models)
        {
            Assert.False(string.IsNullOrEmpty(model.Id));
            Assert.False(string.IsNullOrEmpty(model.Name));
            Assert.False(string.IsNullOrEmpty(model.Description));
            Assert.False(string.IsNullOrEmpty(model.DownloadURL));
            Assert.False(string.IsNullOrEmpty(model.Filename));
            Assert.False(string.IsNullOrEmpty(model.Quantization));
            Assert.False(string.IsNullOrEmpty(model.Parameters));
            Assert.True(model.SizeGB > 0);
            Assert.True(model.MinRAMGB > 0);
        }
    }

    [Fact]
    public void AllModels_HaveUniqueIds()
    {
        var ids = ModelCatalog.Models.Select(m => m.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void AvailableModels_4GB_ReturnsOnlySmallest()
    {
        var available = ModelCatalog.AvailableModels(4);
        Assert.Single(available);
        Assert.Equal("qwen2.5-1.5b-q4", available[0].Id);
    }

    [Fact]
    public void AvailableModels_10GB_ReturnsAll()
    {
        var available = ModelCatalog.AvailableModels(10);
        Assert.Equal(3, available.Count);
    }

    [Fact]
    public void AvailableModels_2GB_ReturnsEmpty()
    {
        var available = ModelCatalog.AvailableModels(2);
        Assert.Empty(available);
    }

    [Fact]
    public void AvailableModels_ReservesOverheadForOS()
    {
        // With 3GB RAM, availableForModel = 1.0 GB.
        // Qwen MinRAMGB=3.0 <= 3 and SizeGB=1.0 <= 1.0, so it qualifies.
        var available = ModelCatalog.AvailableModels(3);
        Assert.Single(available);
        Assert.Equal("qwen2.5-1.5b-q4", available[0].Id);
    }
}
