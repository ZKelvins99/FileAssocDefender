using FileAssocDefender.Services;

namespace FileAssocDefender.Tests;

public class RegistryHelperCacheTests
{
    [Fact]
    public void GetAssociation_UsesCacheUntilInvalidated()
    {
        var helper = new RegistryHelper();
        var first = helper.GetAssociation(".txt");
        var second = helper.GetAssociation(".txt");

        Assert.Same(first, second);

        helper.InvalidateCache();
        var third = helper.GetAssociation(".txt");

        Assert.NotSame(first, third);
        Assert.Equal(first.ProgId, third.ProgId);
    }
}
