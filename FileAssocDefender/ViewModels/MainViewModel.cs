using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileAssocDefender.Models;
using FileAssocDefender.Services;

namespace FileAssocDefender.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly Guardian _guardian;

    public MainViewModel(
        Guardian guardian,
        AssociationListViewModel associationListViewModel,
        LogViewModel logViewModel,
        SettingsViewModel settingsViewModel)
    {
        _guardian = guardian;
        AssociationList = associationListViewModel;
        Log = logViewModel;
        Settings = settingsViewModel;
    }

    public AssociationListViewModel AssociationList { get; }
    public LogViewModel Log { get; }
    public SettingsViewModel Settings { get; }

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
