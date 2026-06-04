using System.Runtime.InteropServices;
using FileAssocDefender.Services.Interop;
using Microsoft.Win32;

namespace FileAssocDefender.Services;

public sealed class AssociationApi
{
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
            return VerifyDefault(normalized, progId);
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

    private static bool VerifyDefault(string extension, string expectedProgId)
    {
        var helper = new RegistryHelper();
        var actual = helper.GetAssociation(extension);
        return string.Equals(actual.ProgId, expectedProgId, StringComparison.OrdinalIgnoreCase);
    }
}
