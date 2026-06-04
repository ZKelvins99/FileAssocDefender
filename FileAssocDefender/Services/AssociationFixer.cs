using FileAssocDefender.Models;

namespace FileAssocDefender.Services;

public sealed class AssociationFixer
{
    private readonly AssociationApi _associationApi;
    private readonly LogService _logService;

    public AssociationFixer(AssociationApi associationApi, LogService logService)
    {
        _associationApi = associationApi;
        _logService = logService;
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

        var success = _associationApi.TrySetDefault(item.Extension, item.TargetProgId);
        if (!success)
        {
            _logService.Error($"修复失败: {item.Extension}");
            return new FixResult
            {
                Extension = item.Extension,
                Status = FixStatus.Failed,
                Message = "修复失败，请确认以管理员身份运行"
            };
        }

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
}
