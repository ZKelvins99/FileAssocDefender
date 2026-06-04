using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileAssocDefender.Models;

namespace FileAssocDefender.ViewModels;

public partial class AssociationCardViewModel : ObservableObject
{
    private readonly Func<AssociationInfo, Task> _fixAction;
    private readonly Action<AssociationInfo> _showDetailAction;

    public AssociationCardViewModel(
        AssociationInfo model,
        Func<AssociationInfo, Task> fixAction,
        Action<AssociationInfo> showDetailAction)
    {
        Model = model;
        _fixAction = fixAction;
        _showDetailAction = showDetailAction;
    }

    public AssociationInfo Model { get; private set; }

    public string Extension => Model.Extension;
    public string FileTypeLabel => Model.FileTypeLabel;
    public string CurrentAppName => Model.CurrentAppName;
    public string CurrentExePath => Model.CurrentExePath;
    public string TargetAppName => Model.TargetAppName;
    public string StatusMessage => Model.StatusMessage;
    public AssociationStatus Status => Model.Status;

    public System.Windows.Media.ImageSource? CurrentIcon => Model.CurrentIcon;
    public System.Windows.Media.ImageSource? TargetIcon => Model.TargetIcon;

    public bool IsHijacked => Status == AssociationStatus.Hijacked;
    public bool IsHealthy => Status == AssociationStatus.Healthy;
    public bool IsUnknown => Status == AssociationStatus.Unknown;
    public bool CanFix => Status == AssociationStatus.Hijacked;

    [RelayCommand(CanExecute = nameof(CanFix))]
    private async Task FixAsync()
    {
        await _fixAction(Model);
    }

    [RelayCommand]
    private void ShowDetail()
    {
        _showDetailAction(Model);
    }

    public void Update(AssociationInfo model)
    {
        Model = model;
        OnPropertyChanged(string.Empty);
        FixCommand.NotifyCanExecuteChanged();
    }
}
