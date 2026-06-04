using FileAssocDefender.Models;
using FileAssocDefender.Services;

namespace FileAssocDefender.Tests;

public class RegistryHelperTests
{
    [Fact]
    public void GetAssociation_ForTxt_ReturnsSomeResult()
    {
        var helper = new RegistryHelper();
        var raw = helper.GetAssociation(".txt");

        Assert.Equal(".txt", raw.Extension);
        Assert.NotEqual(AssociationSource.None, raw.Source);
    }

    [Fact]
    public void ResolveExePath_FromQuotedCommand_ExtractsPath()
    {
        var helper = new RegistryHelper();
        var path = helper.ResolveExePath(@"""C:\Program Files\App\app.exe"" ""%1""");

        Assert.Equal(@"C:\Program Files\App\app.exe", path);
    }

    [Fact]
    public void BackupAssociation_PreservesCurrentProgId()
    {
        var helper = new RegistryHelper();
        var raw = helper.GetAssociation(".txt");
        var backup = helper.BackupAssociation(".txt");

        Assert.Equal(".txt", backup.Extension);
        Assert.Equal(raw.ProgId, backup.ProgId);
        Assert.Equal(raw.Source, backup.Source);
    }
}
