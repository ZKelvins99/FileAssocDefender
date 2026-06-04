using System.Text.RegularExpressions;
using System.Windows.Media;
using FileAssocDefender.Models;
using Microsoft.Win32;

namespace FileAssocDefender.Services;

public sealed class RegistryHelper
{
    private readonly Dictionary<string, AssociationRaw> _associationCache = new(StringComparer.OrdinalIgnoreCase);

    public void InvalidateCache() => _associationCache.Clear();

    public AssociationRaw GetAssociation(string extension)
    {
        var normalized = NormalizeExtension(extension);
        if (_associationCache.TryGetValue(normalized, out var cached))
        {
            return cached;
        }

        var raw = ReadAssociation(normalized);
        _associationCache[normalized] = raw;
        return raw;
    }

    private AssociationRaw ReadAssociation(string normalized)
    {
        var userChoice = ReadProgId($@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{normalized}\UserChoice", "ProgId");
        if (!string.IsNullOrWhiteSpace(userChoice))
        {
            return BuildRaw(normalized, userChoice, AssociationSource.UserChoice);
        }

        var hkcu = ReadProgId($@"Software\Classes\{normalized}", null);
        if (!string.IsNullOrWhiteSpace(hkcu))
        {
            return BuildRaw(normalized, hkcu, AssociationSource.HkcuClasses);
        }

        var hkcr = ReadProgId($@"{normalized}", null, RegistryHive.ClassesRoot);
        if (!string.IsNullOrWhiteSpace(hkcr))
        {
            return BuildRaw(normalized, hkcr, AssociationSource.Hkcr);
        }

        return new AssociationRaw
        {
            Extension = normalized,
            Source = AssociationSource.None
        };
    }

    public IReadOnlyList<AssociationRaw> ScanAll(IEnumerable<string> extensions)
        => extensions.Select(GetAssociation).ToList();

    public string ResolveCommand(string progId)
    {
        using var key = Registry.ClassesRoot.OpenSubKey($@"{progId}\shell\open\command");
        return key?.GetValue(null)?.ToString()?.Trim() ?? string.Empty;
    }

    public string ResolveExePath(string commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine))
        {
            return string.Empty;
        }

        var trimmed = commandLine.Trim();
        if (trimmed.StartsWith('"'))
        {
            var endQuote = trimmed.IndexOf('"', 1);
            if (endQuote > 1)
            {
                return trimmed[1..endQuote];
            }
        }

        var spaceIndex = trimmed.IndexOf(' ');
        return spaceIndex > 0 ? trimmed[..spaceIndex] : trimmed;
    }

    public string ResolveDefaultIconPath(string progId)
    {
        using var key = Registry.ClassesRoot.OpenSubKey($@"{progId}\DefaultIcon");
        var value = key?.GetValue(null)?.ToString() ?? string.Empty;
        return ExpandRegistryPath(value.Split(',')[0]);
    }

    public IReadOnlyList<string> GetOpenWithProgIds(string extension)
    {
        var normalized = NormalizeExtension(extension);
        using var key = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{normalized}\OpenWithProgids");
        if (key is null)
        {
            return [];
        }

        return key.GetValueNames()
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToList();
    }

    public AssociationBackup BackupAssociation(string extension)
    {
        var raw = GetAssociation(extension);
        var normalized = NormalizeExtension(extension);
        var hash = ReadProgId(
            $@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{normalized}\UserChoice",
            "Hash");

        return new AssociationBackup
        {
            Extension = normalized,
            ProgId = raw.ProgId,
            Hash = hash,
            Source = raw.Source
        };
    }

    public bool SetHkcuDefault(string extension, string progId)
    {
        try
        {
            var normalized = NormalizeExtension(extension);
            using var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{normalized}", true);
            key.SetValue(null, progId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool RemoveOpenWithProgId(string extension, string progId)
    {
        try
        {
            var normalized = NormalizeExtension(extension);
            using var key = Registry.CurrentUser.OpenSubKey(
                $@"Software\Classes\{normalized}\OpenWithProgids",
                writable: true);

            if (key is null)
            {
                return true;
            }

            key.DeleteValue(progId, throwOnMissingValue: false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TryClearUserChoice(string extension)
    {
        try
        {
            var normalized = NormalizeExtension(extension);
            var path = $@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{normalized}\UserChoice";
            Registry.CurrentUser.DeleteSubKeyTree(path, throwOnMissingSubKey: false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TryRepairViaRegistry(string extension, string progId, IEnumerable<string> progIdsToRemove)
    {
        var normalized = NormalizeExtension(extension);
        foreach (var hijackProgId in progIdsToRemove)
        {
            RemoveOpenWithProgId(normalized, hijackProgId);
        }

        TryClearUserChoice(normalized);
        var success = SetHkcuDefault(normalized, progId);
        if (success)
        {
            InvalidateCache();
        }

        return success;
    }

    private AssociationRaw BuildRaw(string extension, string progId, AssociationSource source)
    {
        var command = ResolveCommand(progId);
        var exePath = ResolveExePath(command);

        return new AssociationRaw
        {
            Extension = extension,
            ProgId = progId,
            CommandLine = command,
            ExePath = exePath,
            Source = source
        };
    }

    private static string ReadProgId(string subKeyPath, string? valueName, RegistryHive hive = RegistryHive.CurrentUser)
    {
        var baseKey = hive switch
        {
            RegistryHive.ClassesRoot => Registry.ClassesRoot,
            _ => Registry.CurrentUser
        };

        using var key = baseKey.OpenSubKey(subKeyPath);
        if (key is null)
        {
            return string.Empty;
        }

        if (valueName is null)
        {
            return key.GetValue(null)?.ToString()?.Trim() ?? string.Empty;
        }

        return key.GetValue(valueName)?.ToString()?.Trim() ?? string.Empty;
    }

    private static string ExpandRegistryPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        var expanded = Environment.ExpandEnvironmentVariables(path.Trim('"'));
        return Regex.Replace(expanded, @"%([^%]+)%", match =>
        {
            var env = Environment.GetEnvironmentVariable(match.Groups[1].Value);
            return env ?? match.Value;
        });
    }

    private static string NormalizeExtension(string extension)
        => extension.StartsWith('.') ? extension.ToLowerInvariant() : $".{extension.ToLowerInvariant()}";
}
