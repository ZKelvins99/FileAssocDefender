using System.IO;
using System.Text.RegularExpressions;
using FileAssocDefender.Models;

namespace FileAssocDefender.Services;

public sealed class AssociationScanner
{
    private readonly RegistryHelper _registryHelper;
    private readonly IconResolver _iconResolver;
    private readonly OfficeDetector _officeDetector;
    private readonly PresetStore _presetStore;
    private readonly HijackSignatures _signatures;

    public AssociationScanner(
        RegistryHelper registryHelper,
        IconResolver iconResolver,
        OfficeDetector officeDetector,
        PresetStore presetStore)
    {
        _registryHelper = registryHelper;
        _iconResolver = iconResolver;
        _officeDetector = officeDetector;
        _presetStore = presetStore;
        _signatures = presetStore.LoadHijackSignatures();
    }

    public IReadOnlyList<AssociationInfo> Scan()
    {
        var office = _officeDetector.Detect();
        var results = new List<AssociationInfo>();

        foreach (var preset in _presetStore.Presets)
        {
            var raw = _registryHelper.GetAssociation(preset.Extension);
            var target = ResolveTarget(preset, office);

            var currentAppName = _iconResolver.GetFriendlyName(raw.ExePath, raw.ProgId);
            var currentIcon = _iconResolver.GetIconFromExe(raw.ExePath)
                ?? _iconResolver.GetIconFromProgId(raw.ProgId);

            var targetIcon = _iconResolver.GetIconFromExe(target.ExePath)
                ?? _iconResolver.GetIconFromProgId(target.ProgId);

            var status = EvaluateStatus(raw, preset, target, office);
            var message = BuildStatusMessage(status, currentAppName, target.AppName, office.IsInstalled);

            results.Add(new AssociationInfo
            {
                Extension = preset.Extension,
                FileTypeLabel = preset.DisplayName,
                Category = preset.Category,
                CurrentProgId = raw.ProgId,
                CurrentAppName = string.IsNullOrWhiteSpace(currentAppName) ? "未关联" : currentAppName,
                CurrentExePath = raw.ExePath,
                CurrentIcon = currentIcon,
                TargetProgId = target.ProgId,
                TargetAppName = target.AppName,
                TargetExePath = target.ExePath,
                TargetIcon = targetIcon,
                Status = status,
                Source = raw.Source,
                StatusMessage = message,
                CommandLine = raw.CommandLine
            });
        }

        return results;
    }

    private (string ProgId, string ExePath, string AppName) ResolveTarget(Preset preset, OfficeInstallation office)
    {
        var exePath = preset.Category switch
        {
            "Word" => office.WordExePath,
            "Excel" => office.ExcelExePath,
            "PowerPoint" => office.PowerPointExePath,
            _ => string.Empty
        };

        var progId = preset.ProgId;
        var appName = preset.Category switch
        {
            "Word" => "Microsoft Word",
            "Excel" => "Microsoft Excel",
            "PowerPoint" => "Microsoft PowerPoint",
            _ => "Microsoft Office"
        };

        return (progId, exePath, appName);
    }

    private AssociationStatus EvaluateStatus(
        AssociationRaw raw,
        Preset preset,
        (string ProgId, string ExePath, string AppName) target,
        OfficeInstallation office)
    {
        if (!office.IsInstalled || string.IsNullOrWhiteSpace(target.ExePath))
        {
            return AssociationStatus.Unknown;
        }

        if (string.IsNullOrWhiteSpace(raw.ProgId) && string.IsNullOrWhiteSpace(raw.ExePath))
        {
            return AssociationStatus.Unknown;
        }

        if (IsOfficeMatch(raw, preset, target))
        {
            return AssociationStatus.Healthy;
        }

        if (IsKnownHijack(raw))
        {
            return AssociationStatus.Hijacked;
        }

        return string.Equals(raw.ProgId, preset.ProgId, StringComparison.OrdinalIgnoreCase)
            ? AssociationStatus.Healthy
            : AssociationStatus.Hijacked;
    }

    private bool IsOfficeMatch(
        AssociationRaw raw,
        Preset preset,
        (string ProgId, string ExePath, string AppName) target)
    {
        if (!string.IsNullOrWhiteSpace(raw.ExePath)
            && !string.IsNullOrWhiteSpace(target.ExePath)
            && string.Equals(
                Path.GetFullPath(raw.ExePath),
                Path.GetFullPath(target.ExePath),
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(raw.ProgId, preset.ProgId, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsKnownHijack(AssociationRaw raw)
    {
        if (_signatures.ProgIds.Any(p =>
                string.Equals(p, raw.ProgId, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(raw.ExePath))
        {
            return false;
        }

        return _signatures.ExePatterns.Any(pattern =>
            MatchPattern(raw.ExePath, pattern));
    }

    private static bool MatchPattern(string path, string pattern)
    {
        var normalizedPattern = pattern.Replace("*\\", "*", StringComparison.Ordinal);
        var fileName = Path.GetFileName(path);
        var simplePattern = pattern.Contains('\\')
            ? pattern
            : $"*{pattern.Trim('*')}";

        if (simplePattern.Contains('*'))
        {
            var regex = "^" + Regex.Escape(simplePattern).Replace("\\*", ".*") + "$";
            return Regex.IsMatch(path, regex, RegexOptions.IgnoreCase);
        }

        return path.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildStatusMessage(
        AssociationStatus status,
        string currentAppName,
        string targetAppName,
        bool officeInstalled)
    {
        return status switch
        {
            AssociationStatus.Healthy => $"当前默认打开方式为 {currentAppName}",
            AssociationStatus.Hijacked => $"已被 {currentAppName} 劫持，建议修复为 {targetAppName}",
            AssociationStatus.Unknown when !officeInstalled => "未检测到 Microsoft Office 安装",
            AssociationStatus.Unknown => "无法确定当前关联状态",
            _ => string.Empty
        };
    }
}
