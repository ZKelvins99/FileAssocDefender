using CommunityToolkit.Mvvm.ComponentModel;
using FileAssocDefender.Models;
using FileAssocDefender.Services;

namespace FileAssocDefender.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly OfficeDetector _officeDetector;
    private readonly Guardian _guardian;

    public SettingsViewModel(OfficeDetector officeDetector, Guardian guardian)
    {
        _officeDetector = officeDetector;
        _guardian = guardian;
        LoadOfficeInfo();
    }

    [ObservableProperty]
    private bool _autoRepair;

    [ObservableProperty]
    private int _pollingIntervalSeconds = 10;

    [ObservableProperty]
    private bool _startWithWindows;

    [ObservableProperty]
    private string _officeSummary = "检测中...";

    partial void OnAutoRepairChanged(bool value) => _guardian.AutoRepair = value;

    partial void OnPollingIntervalSecondsChanged(int value)
        => _guardian.PollingIntervalMs = Math.Max(5, value) * 1000;

    private void LoadOfficeInfo()
    {
        var office = _officeDetector.Detect();
        if (!office.IsInstalled)
        {
            OfficeSummary = "未检测到 Microsoft Office";
            return;
        }

        OfficeSummary =
            $"Word: {DisplayPath(office.WordExePath)}\n" +
            $"Excel: {DisplayPath(office.ExcelExePath)}\n" +
            $"PowerPoint: {DisplayPath(office.PowerPointExePath)}";
    }

    private static string DisplayPath(string path)
        => string.IsNullOrWhiteSpace(path) ? "未找到" : path;
}
