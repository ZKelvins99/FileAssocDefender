using FileAssocDefender.Models;

namespace FileAssocDefender.Services;

public sealed class AssociationFixer
{
    private readonly AssociationApi _associationApi;
    private readonly RegistryHelper _registryHelper;
    private readonly OfficeDetector _officeDetector;
    private readonly AssociationBackupStore _backupStore;
    private readonly LogService _logService;
    private readonly WpsGuardService _wpsGuardService;

    public AssociationFixer(
        AssociationApi associationApi,
        RegistryHelper registryHelper,
        OfficeDetector officeDetector,
        AssociationBackupStore backupStore,
        LogService logService,
        WpsGuardService wpsGuardService)
    {
        _associationApi = associationApi;
        _registryHelper = registryHelper;
        _officeDetector = officeDetector;
        _backupStore = backupStore;
        _logService = logService;
        _wpsGuardService = wpsGuardService;
    }

    public FixResult Fix(AssociationInfo item)
    {
        if (item.Status == AssociationStatus.Healthy)
        {
            return new FixResult
            {
                Extension = item.Extension,
                Status = FixStatus.Skipped,
                Message = "当前关联正常，无需修复"
            };
        }

        var backup = _registryHelper.BackupAssociation(item.Extension);
        _backupStore.Save(backup);
        _logService.Info($"已备份 {item.Extension} 关联: {backup.ProgId}");

        var targetProgId = item.TargetProgId;
        var targetExe = item.TargetExePath;

        if (string.IsNullOrWhiteSpace(targetExe))
        {
            var office = _officeDetector.Detect();
            targetExe = item.Category switch
            {
                "Word" => office.WordExePath,
                "Excel" => office.ExcelExePath,
                "PowerPoint" => office.PowerPointExePath,
                _ => string.Empty
            };
        }

        if (string.IsNullOrWhiteSpace(targetProgId) && !string.IsNullOrWhiteSpace(targetExe))
        {
            targetProgId = _registryHelper.FindProgIdForExe(targetExe);
        }

        if (string.IsNullOrWhiteSpace(targetProgId) && string.IsNullOrWhiteSpace(targetExe))
        {
            return new FixResult
            {
                Extension = item.Extension,
                Status = FixStatus.Failed,
                Message = "未检测到 Microsoft Office，无法修复"
            };
        }

        var stopped = _wpsGuardService.TryStopGuardProcesses();
        if (stopped > 0)
        {
            _logService.Info($"已终止 {stopped} 个 WPS 守护相关进程");
        }

        var success = !string.IsNullOrWhiteSpace(targetProgId)
            && _associationApi.TrySetDefault(item.Extension, targetProgId);

        if (!success && !string.IsNullOrWhiteSpace(targetExe))
        {
            success = _associationApi.TrySetDefaultByExe(item.Extension, targetExe);
        }

        if (!success)
        {
            RestoreBackup(backup);
            _logService.Error($"修复失败且已尝试还原: {item.Extension}");
            return new FixResult
            {
                Extension = item.Extension,
                Status = FixStatus.Failed,
                Message = "修复失败，未改写注册表。请确认以管理员身份运行，或在 Windows 设置中手动选择默认应用。"
            };
        }

        _registryHelper.InvalidateCache();
        _logService.AssociationChanged(
            item.Extension,
            item.CurrentAppName,
            item.TargetAppName);

        return new FixResult
        {
            Extension = item.Extension,
            Status = FixStatus.Success,
            Message = $"已修复为 {item.TargetAppName}"
        };
    }

    public IReadOnlyList<FixResult> FixAll(IEnumerable<AssociationInfo> items)
        => items
            .Where(i => i.Status == AssociationStatus.Hijacked)
            .Select(Fix)
            .ToList();

    private void RestoreBackup(AssociationBackup backup)
    {
        if (string.IsNullOrWhiteSpace(backup.ProgId))
        {
            return;
        }

        if (_associationApi.TrySetDefault(backup.Extension, backup.ProgId))
        {
            _logService.Warn($"已还原 {backup.Extension} 为 {backup.ProgId}");
            _registryHelper.InvalidateCache();
        }
    }
}
