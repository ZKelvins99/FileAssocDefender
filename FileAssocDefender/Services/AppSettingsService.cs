using System.IO;
using System.Text.Json;
using FileAssocDefender.Models;

namespace FileAssocDefender.Services;

public sealed class AppSettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _settingsPath;

    public AppSettingsService()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FileAssocDefender");
        Directory.CreateDirectory(folder);
        _settingsPath = Path.Combine(folder, "settings.json");
    }

    public AppSettings Settings { get; private set; } = new();

    public void Load()
    {
        if (!File.Exists(_settingsPath))
        {
            Settings = new AppSettings();
            return;
        }

        var json = File.ReadAllText(_settingsPath);
        Settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(Settings, JsonOptions);
        File.WriteAllText(_settingsPath, json);
    }

    public void CompleteWelcome(bool skipFutureWelcome)
    {
        if (skipFutureWelcome)
        {
            Settings.HasCompletedWelcome = true;
        }

        Save();
    }
}
