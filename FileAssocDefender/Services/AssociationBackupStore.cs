using System.IO;
using System.Text.Json;
using FileAssocDefender.Models;

namespace FileAssocDefender.Services;

public sealed class AssociationBackupStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _backupDir;

    public AssociationBackupStore()
    {
        _backupDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FileAssocDefender",
            "backups");
        Directory.CreateDirectory(_backupDir);
    }

    public void Save(AssociationBackup backup)
    {
        var path = GetPath(backup.Extension);
        File.WriteAllText(path, JsonSerializer.Serialize(backup, JsonOptions));
    }

    public AssociationBackup? Load(string extension)
    {
        var path = GetPath(extension);
        if (!File.Exists(path))
        {
            return null;
        }

        return JsonSerializer.Deserialize<AssociationBackup>(File.ReadAllText(path), JsonOptions);
    }

    private string GetPath(string extension)
    {
        var safeName = extension.TrimStart('.');
        return Path.Combine(_backupDir, $"{safeName}.json");
    }
}
