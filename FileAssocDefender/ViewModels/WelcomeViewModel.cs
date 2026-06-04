using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileAssocDefender.Services;

namespace FileAssocDefender.ViewModels;

public partial class WelcomeViewModel : ObservableObject
{
    private readonly OfficeDetector _officeDetector;
    private readonly AppSettingsService _appSettingsService;

    public WelcomeViewModel(OfficeDetector officeDetector, AppSettingsService appSettingsService)
    {
        _officeDetector = officeDetector;
        _appSettingsService = appSettingsService;
        LoadOfficeStatus();
    }

    [ObservableProperty]
    private string _officeStatus = "正在检测 Microsoft Office...";

    [ObservableProperty]
    private bool _skipNextTime = true;

    public bool DialogResult { get; private set; }

    private void LoadOfficeStatus()
    {
        var office = _officeDetector.Detect();
        if (!office.IsInstalled)
        {
            OfficeStatus =
                "未检测到 Microsoft Office。\n" +
                "仍可查看当前文件关联，但「修复为 Office」可能无法生效。";
            return;
        }

        OfficeStatus =
            "已检测到 Microsoft Office：\n" +
            $"Word: {FormatPath(office.WordExePath)}\n" +
            $"Excel: {FormatPath(office.ExcelExePath)}\n" +
            $"PowerPoint: {FormatPath(office.PowerPointExePath)}";
    }

    [RelayCommand]
    private void Continue()
    {
        _appSettingsService.CompleteWelcome(SkipNextTime);
        DialogResult = true;
        CloseRequested?.Invoke(this, true);
    }

    public event EventHandler<bool>? CloseRequested;

    private static string FormatPath(string path)
        => string.IsNullOrWhiteSpace(path) ? "未找到" : path;
}
