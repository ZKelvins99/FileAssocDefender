using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileAssocDefender.Services;

namespace FileAssocDefender.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly Guardian _guardian;

    public MainViewModel(
        Guardian guardian,
        AssociationListViewModel associationListViewModel,
        LogViewModel logViewModel,
        SettingsViewModel settingsViewModel,
        DetailDrawerViewModel detailDrawerViewModel)
    {
        _guardian = guardian;
        AssociationList = associationListViewModel;
        Log = logViewModel;
        Settings = settingsViewModel;
        DetailDrawer = detailDrawerViewModel;
    }

    public AssociationListViewModel AssociationList { get; }
    public LogViewModel Log { get; }
    public SettingsViewModel Settings { get; }
    public DetailDrawerViewModel DetailDrawer { get; }

    [ObservableProperty]
    private int _selectedPageIndex;

    [RelayCommand]
    private void NavigateToOverview() => SelectedPageIndex = 0;

    [RelayCommand]
    private void NavigateToLogs() => SelectedPageIndex = 1;

    [RelayCommand]
    private void NavigateToSettings() => SelectedPageIndex = 2;

    public void Initialize()
    {
        AssociationList.RefreshCommand.Execute(null);
        _guardian.Start();
    }
}
