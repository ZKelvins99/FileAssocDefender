using System.Runtime.InteropServices;

namespace FileAssocDefender.Services.Interop;

internal enum AssociationType
{
    FileExtension = 0,
    UrlProtocol = 1,
    StartMenuClient = 2
}

[ComImport]
[Guid("591209c7-ef6f-4c2e-8f84-9773b512db35")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IApplicationAssociationRegistration
{
    [PreserveSig]
    int QueryCurrentDefault(
        [MarshalAs(UnmanagedType.LPWStr)] string query,
        AssociationType queryType,
        AssociationType queryHint,
        [MarshalAs(UnmanagedType.LPWStr)] out string association);

    [PreserveSig]
    int QueryAppIsDefault(
        [MarshalAs(UnmanagedType.LPWStr)] string query,
        AssociationType queryType,
        AssociationType queryHint,
        [MarshalAs(UnmanagedType.LPWStr)] string appRegistryName,
        out bool isDefault);

    void SetAppAsDefault(
        [MarshalAs(UnmanagedType.LPWStr)] string appRegistryName,
        [MarshalAs(UnmanagedType.LPWStr)] string set,
        AssociationType setType);

    void SetAppAsDefaultAll([MarshalAs(UnmanagedType.LPWStr)] string appRegistryName);

    void ClearUserAssociations();
}

[ComImport]
[Guid("debd7aa0-0eae-4b57-8306-6914f363fb73")]
internal class ApplicationAssociationRegistration;
