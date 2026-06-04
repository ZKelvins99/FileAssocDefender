using FileAssocDefender.Services;

namespace FileAssocDefender.Tests;

public class AppSettingsServiceTests : IDisposable
{
    private readonly string _tempPath;

    public AppSettingsServiceTests()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), $"fad-settings-{Guid.NewGuid():N}.json");
    }

    [Fact]
    public void SaveAndLoad_PreservesSettings()
    {
        var service = new AppSettingsService(_tempPath);
        service.Load();
        service.Settings.HasCompletedWelcome = true;
        service.Settings.AutoRepair = true;
        service.Settings.PollingIntervalSeconds = 30;
        service.Save();

        var reloaded = new AppSettingsService(_tempPath);
        reloaded.Load();

        Assert.True(reloaded.Settings.HasCompletedWelcome);
        Assert.True(reloaded.Settings.AutoRepair);
        Assert.Equal(30, reloaded.Settings.PollingIntervalSeconds);
    }

    public void Dispose()
    {
        if (File.Exists(_tempPath))
        {
            File.Delete(_tempPath);
        }
    }
}
