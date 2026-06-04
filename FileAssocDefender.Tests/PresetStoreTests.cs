using FileAssocDefender.Services;

namespace FileAssocDefender.Tests;

public class PresetStoreTests
{
    [Fact]
    public void Load_ShouldProvideOfficePresets()
    {
        var store = new PresetStore();
        store.Load();

        Assert.NotEmpty(store.Presets);
        Assert.Contains(store.Presets, p => p.Extension == ".xlsx");
        Assert.Contains(store.Presets, p => p.Extension == ".docx");
        Assert.Contains(store.Presets, p => p.Extension == ".pptx");
    }

    [Fact]
    public void LoadHijackSignatures_ShouldIncludeWpsProgIds()
    {
        var store = new PresetStore();
        store.Load();

        var signatures = store.LoadHijackSignatures();
        Assert.Contains(signatures.ProgIds, p => p.Contains("KWPS", StringComparison.OrdinalIgnoreCase));
    }
}
