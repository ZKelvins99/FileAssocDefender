using CommunityToolkit.Mvvm.ComponentModel;
using FileAssocDefender.Services;

namespace FileAssocDefender.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly OfficeDetector _officeDetector;
    private readonly Guardian _guardian;
    private readonly AppSettingsService _appSettingsService;
    private readonly StartupRegistrationService _startupRegistrationService;
    private bool _isLoading;

    public SettingsViewModel(
        OfficeDetector officeDetector,
        Guardian guardian,
        AppSettingsService appSettingsService,
        StartupRegistrationService startupRegistrationService)
    {
        _officeDetector = officeDetector;
        _guardian = guardian;
        _appSettingsService = appSettingsService;
        _startupRegistrationService = startupRegistrationService;

        LoadFromSettings();
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

    partial void OnAutoRepairChanged(bool value)
    {
        if (_isLoading)
        {
            return;
        }

        _guardian.AutoRepair = value;
        _appSettingsService.Settings.AutoRepair = value;
        _appSettingsService.Save();
    }

    partial void OnPollingIntervalSecondsChanged(int value)
    {
        if (_isLoading)
        {
            return;
        }

        _guardian.PollingIntervalMs = Math.Max(5, value) * 1000;
        _appSettingsService.Settings.PollingIntervalSeconds = Math.Max(5, value);
        _appSettingsService.Save();
    }

    partial void OnStartWithWindowsChanged(bool value)
    {
        if (_isLoading)
        {
            return;
        }

        if (_startupRegistrationService.SetEnabled(value))
        {
            _appSettingsService.Settings.StartWithWindows = value;
            _appSettingsService.Save();
            return;
        }

        _isLoading = true;
        StartWithWindows = _startupRegistrationService.IsEnabled();
        _isLoading = false;
    }

    private void LoadFromSettings()
    {
        _isLoading = true;

        var settings = _appSettingsService.Settings;
        AutoRepair = settings.AutoRepair;
        PollingIntervalSeconds = Math.Max(5, settings.PollingIntervalSeconds);
        StartWithWindows = _startupRegistrationService.IsEnabled();

        _guardian.AutoRepair = AutoRepair;
        _guardian.PollingIntervalMs = PollingIntervalSeconds * 1000;

        if (settings.StartWithWindows != StartWithWindows)
        {
            settings.StartWithWindows = StartWithWindows;
            _appSettingsService.Save();
        }

        _isLoading = false;
    }

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
