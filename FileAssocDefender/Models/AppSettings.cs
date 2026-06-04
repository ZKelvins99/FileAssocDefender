namespace FileAssocDefender.Models;

public sealed class AppSettings
{
    public bool HasCompletedWelcome { get; set; }
    public bool StartWithWindows { get; set; }
    public bool AutoRepair { get; set; }
    public int PollingIntervalSeconds { get; set; } = 10;
}
