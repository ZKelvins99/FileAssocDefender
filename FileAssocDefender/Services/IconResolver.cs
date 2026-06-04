using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using Microsoft.Win32;

namespace FileAssocDefender.Services;

public sealed class IconResolver
{
    private readonly RegistryHelper _registryHelper;
    private readonly Dictionary<string, ImageSource?> _iconCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _nameCache = new(StringComparer.OrdinalIgnoreCase);

    public IconResolver(RegistryHelper registryHelper)
    {
        _registryHelper = registryHelper;
    }

    public void ClearCache()
    {
        _iconCache.Clear();
        _nameCache.Clear();
    }

    public ImageSource? GetIconFromExe(string exePath, int size = 32)
    {
        if (string.IsNullOrWhiteSpace(exePath))
        {
            return null;
        }

        var cacheKey = $"exe:{exePath}:{size}";
        if (_iconCache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var icon = ShellIconHelper.GetIconFromPath(exePath, size);
        _iconCache[cacheKey] = icon;
        return icon;
    }

    public ImageSource? GetIconFromProgId(string progId, int size = 32)
    {
        if (string.IsNullOrWhiteSpace(progId))
        {
            return null;
        }

        var cacheKey = $"prog:{progId}:{size}";
        if (_iconCache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var iconPath = _registryHelper.ResolveDefaultIconPath(progId);
        if (!string.IsNullOrWhiteSpace(iconPath) && File.Exists(iconPath))
        {
            var icon = ShellIconHelper.GetIconFromPath(iconPath, size);
            _iconCache[cacheKey] = icon;
            return icon;
        }

        var command = _registryHelper.ResolveCommand(progId);
        var exePath = _registryHelper.ResolveExePath(command);
        var fallback = GetIconFromExe(exePath, size);
        _iconCache[cacheKey] = fallback;
        return fallback;
    }

    public string GetFriendlyName(string exePath, string progId)
    {
        var cacheKey = $"{exePath}|{progId}";
        if (_nameCache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var name = ResolveFriendlyName(exePath, progId);
        _nameCache[cacheKey] = name;
        return name;
    }

    private static string ResolveFriendlyName(string exePath, string progId)
    {
        if (!string.IsNullOrWhiteSpace(exePath) && File.Exists(exePath))
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(exePath);
            if (!string.IsNullOrWhiteSpace(versionInfo.ProductName))
            {
                return versionInfo.ProductName;
            }

            if (!string.IsNullOrWhiteSpace(versionInfo.FileDescription))
            {
                return versionInfo.FileDescription;
            }
        }

        using var key = Registry.ClassesRoot.OpenSubKey(progId);
        var friendly = key?.GetValue(null)?.ToString();
        if (!string.IsNullOrWhiteSpace(friendly))
        {
            return friendly;
        }

        return progId;
    }
}
