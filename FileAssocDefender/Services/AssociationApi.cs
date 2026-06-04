using System.IO;
using System.Runtime.InteropServices;
using FileAssocDefender.Services.Interop;

namespace FileAssocDefender.Services;

public sealed class AssociationApi
{
    private readonly RegistryHelper _registryHelper;

    public AssociationApi(RegistryHelper registryHelper)
    {
        _registryHelper = registryHelper;
    }

    public bool TrySetDefault(string extension, string progId)
    {
        if (string.IsNullOrWhiteSpace(extension) || string.IsNullOrWhiteSpace(progId))
        {
            return false;
        }

        var normalized = extension.StartsWith('.') ? extension : $".{extension}";

        try
        {
            var registration = (IApplicationAssociationRegistration)new ApplicationAssociationRegistration();
            registration.SetAppAsDefault(progId, normalized, AssociationType.FileExtension);
            _registryHelper.InvalidateCache();
            return _registryHelper.IsAssociationConfigured(normalized, progId);
        }
        catch (COMException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    public bool TrySetDefaultByExe(string extension, string exePath)
    {
        if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
        {
            return false;
        }

        var progId = _registryHelper.FindProgIdForExe(exePath);
        return !string.IsNullOrWhiteSpace(progId) && TrySetDefault(extension, progId);
    }
}
