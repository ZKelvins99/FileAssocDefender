using FileAssocDefender.Models;

namespace FileAssocDefender.Services;

public sealed class AssociationFixer
{
    private readonly AssociationApi _associationApi;
    private readonly RegistryHelper _registryHelper;
    private readonly PresetStore _presetStore;
    private readonly LogService _logService;
    private readonly WpsGuardService _wpsGuardService;

    public AssociationFixer(
        AssociationApi associationApi,
        RegistryHelper registryHelper,
        PresetStore presetStore,
        LogService logService,
        WpsGuardService wpsGuardService)
    {
        _associationApi = associationApi;
        _registryHelper = registryHelper;
        _presetStore = presetStore;
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

        if (string.IsNullOrWhiteSpace(item.TargetProgId))
        {
            return new FixResult
            {
                Extension = item.Extension,
                Status = FixStatus.Failed,
                Message = "未找到 Microsoft Office 目标 ProgID"
            };
        }

        var backup = _registryHelper.BackupAssociation(item.Extension);
        _logService.Info($"已备份 {item.Extension} 关联: {backup.ProgId}");

        var hijackProgIds = _presetStore.LoadHijackSignatures().ProgIds
            .Where(p => !string.Equals(p, item.TargetProgId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var stopped = _wpsGuardService.TryStopGuardProcesses();
        if (stopped > 0)
        {
            _logService.Info($"已终止 {stopped} 个 WPS 守护相关进程");
        }

        if (_associationApi.TrySetDefault(item.Extension, item.TargetProgId)
            || _registryHelper.TryRepairViaRegistry(item.Extension, item.TargetProgId, hijackProgIds))
        {
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

        _logService.Error($"修复失败: {item.Extension}");
        return new FixResult
        {
            Extension = item.Extension,
            Status = FixStatus.Failed,
            Message = "修复失败，请确认以管理员身份运行"
        };
    }

    public IReadOnlyList<FixResult> FixAll(IEnumerable<AssociationInfo> items)
        => items
            .Where(i => i.Status == AssociationStatus.Hijacked)
            .Select(Fix)
            .ToList();
}
