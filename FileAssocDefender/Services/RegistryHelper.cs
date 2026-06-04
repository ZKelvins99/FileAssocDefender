using System.Text.RegularExpressions;
using System.Windows.Media;
using FileAssocDefender.Models;
using Microsoft.Win32;

namespace FileAssocDefender.Services;

public sealed class RegistryHelper
{
    public AssociationRaw GetAssociation(string extension)
    {
        var normalized = NormalizeExtension(extension);

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

    private static AssociationRaw BuildRaw(string extension, string progId, AssociationSource source)
    {
        var helper = new RegistryHelper();
        var command = helper.ResolveCommand(progId);
        var exePath = helper.ResolveExePath(command);

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
