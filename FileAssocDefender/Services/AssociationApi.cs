using FileAssocDefender.Models;

namespace FileAssocDefender.Services;

public sealed class AssociationApi
{
    public bool TrySetDefault(string extension, string progId)
    {
        // Phase 3: 接入 IApplicationAssociationRegistration COM API。
        // 骨架阶段返回 false，由 UI 提示后续实现。
        _ = extension;
        _ = progId;
        return false;
    }
}
