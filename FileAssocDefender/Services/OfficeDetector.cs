using System.IO;
using FileAssocDefender.Models;
using Microsoft.Win32;

namespace FileAssocDefender.Services;

public sealed class OfficeDetector
{
    public OfficeInstallation Detect()
    {
        var word = DetectApplication("Word", "WINWORD.EXE");
        var excel = DetectApplication("Excel", "EXCEL.EXE");
        var powerPoint = DetectApplication("PowerPoint", "POWERPNT.EXE");

        var installed = !string.IsNullOrWhiteSpace(word)
            || !string.IsNullOrWhiteSpace(excel)
            || !string.IsNullOrWhiteSpace(powerPoint);

        return new OfficeInstallation
        {
            IsInstalled = installed,
            WordExePath = word,
            ExcelExePath = excel,
            PowerPointExePath = powerPoint
        };
    }

    private static string DetectApplication(string appName, string exeName)
    {
        var clickToRun = ReadRegistryValue(
            @"SOFTWARE\Microsoft\Office\ClickToRun\Configuration",
            $"{appName}ExePath");

        if (IsValidExe(clickToRun))
        {
            return clickToRun;
        }

        foreach (var version in new[] { "16.0", "15.0", "14.0" })
        {
            var installRoot = ReadRegistryValue(
                $@"SOFTWARE\Microsoft\Office\{version}\{appName}\InstallRoot",
                "Path",
                RegistryHive.LocalMachine);

            if (string.IsNullOrWhiteSpace(installRoot))
            {
                continue;
            }

            var candidate = Path.Combine(installRoot, exeName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var candidatePath = Path.Combine(programFiles, "Microsoft Office", "root", "Office16", exeName);
        return File.Exists(candidatePath) ? candidatePath : string.Empty;
    }

    private static string ReadRegistryValue(string path, string valueName, RegistryHive hive = RegistryHive.LocalMachine)
    {
        var baseKey = hive == RegistryHive.LocalMachine ? Registry.LocalMachine : Registry.CurrentUser;
        using var key = baseKey.OpenSubKey(path);
        return key?.GetValue(valueName)?.ToString()?.Trim() ?? string.Empty;
    }

    private static bool IsValidExe(string path)
        => !string.IsNullOrWhiteSpace(path) && File.Exists(path);
}
