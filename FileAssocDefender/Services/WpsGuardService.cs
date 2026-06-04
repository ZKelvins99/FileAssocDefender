using System.Diagnostics;

namespace FileAssocDefender.Services;

public sealed class WpsGuardService
{
    private static readonly string[] GuardProcessNames =
    [
        "wpscloudsvr",
        "wpscenter",
        "WpsDesktopApp",
        "promecefpluginhost"
    ];

    public int TryStopGuardProcesses()
    {
        var stopped = 0;

        foreach (var processName in GuardProcessNames)
        {
            foreach (var process in Process.GetProcessesByName(processName))
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                    stopped++;
                }
                catch
                {
                    // 部分 WPS 进程可能受保护，忽略失败继续。
                }
            }
        }

        return stopped;
    }
}
