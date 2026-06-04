using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileAssocDefender.Models;
using FileAssocDefender.Services;

namespace FileAssocDefender.ViewModels;

public partial class DetailDrawerViewModel : ObservableObject
{
    private readonly RegistryHelper _registryHelper;

    public DetailDrawerViewModel(RegistryHelper registryHelper)
    {
        _registryHelper = registryHelper;
    }

    [ObservableProperty]
    private bool _isOpen;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _extension = string.Empty;

    [ObservableProperty]
    private string _currentProgId = string.Empty;

    [ObservableProperty]
    private string _targetProgId = string.Empty;

    [ObservableProperty]
    private string _sourceLayer = string.Empty;

    [ObservableProperty]
    private string _commandLine = string.Empty;

    [ObservableProperty]
    private string _openWithProgIds = string.Empty;

    [ObservableProperty]
    private string _diagnosticText = string.Empty;

    public void Show(AssociationInfo info)
    {
        Title = $"{info.FileTypeLabel} ({info.Extension})";
        Extension = info.Extension;
        CurrentProgId = info.CurrentProgId;
        TargetProgId = info.TargetProgId;
        SourceLayer = info.Source switch
        {
            AssociationSource.UserChoice => "UserChoice（用户手动设置，优先级最高）",
            AssociationSource.HkcuClasses => "HKCU\\Software\\Classes",
            AssociationSource.Hkcr => "HKCR（系统级）",
            _ => "未检测到"
        };
        CommandLine = info.CommandLine;

        var openWith = _registryHelper.GetOpenWithProgIds(info.Extension);
        OpenWithProgIds = openWith.Count == 0
            ? "（无）"
            : string.Join(Environment.NewLine, openWith);

        DiagnosticText =
            $"Extension: {info.Extension}{Environment.NewLine}" +
            $"Status: {info.Status}{Environment.NewLine}" +
            $"Current ProgID: {info.CurrentProgId}{Environment.NewLine}" +
            $"Target ProgID: {info.TargetProgId}{Environment.NewLine}" +
            $"Source: {SourceLayer}{Environment.NewLine}" +
            $"Command: {info.CommandLine}{Environment.NewLine}" +
            $"OpenWithProgids:{Environment.NewLine}{OpenWithProgIds}";

        IsOpen = true;
    }

    [RelayCommand]
    private void Close() => IsOpen = false;

    [RelayCommand]
    private void CopyDiagnostic()
    {
        if (!string.IsNullOrWhiteSpace(DiagnosticText))
        {
            System.Windows.Clipboard.SetText(DiagnosticText);
        }
    }
}
