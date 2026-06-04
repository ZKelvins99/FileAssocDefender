using FileAssocDefender.Services;

namespace FileAssocDefender.Tests;

public class AssociationScannerTests
{
    [Fact]
    public void Scan_ReturnsPresetCountResults()
    {
        var presetStore = new PresetStore();
        presetStore.Load();

        var scanner = new AssociationScanner(
            new RegistryHelper(),
            new IconResolver(),
            new OfficeDetector(),
            presetStore);

        var results = scanner.Scan();

        Assert.Equal(presetStore.Presets.Count, results.Count);
        Assert.All(results, r => Assert.False(string.IsNullOrWhiteSpace(r.Extension)));
    }
}
