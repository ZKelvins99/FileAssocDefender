using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileAssocDefender.Models;
using FileAssocDefender.Services;

namespace FileAssocDefender.ViewModels;

public partial class AssociationListViewModel : ObservableObject
{
    private readonly AssociationScanner _scanner;
    private readonly AssociationFixer _fixer;
    private readonly LogService _logService;
    private readonly DetailDrawerViewModel _detailDrawer;

    public AssociationListViewModel(
        AssociationScanner scanner,
        AssociationFixer fixer,
        LogService logService,
        DetailDrawerViewModel detailDrawer)
    {
        _scanner = scanner;
        _fixer = fixer;
        _logService = logService;
        _detailDrawer = detailDrawer;
        Items = new ObservableCollection<AssociationCardViewModel>();
    }

    public ObservableCollection<AssociationCardViewModel> Items { get; }

    [ObservableProperty]
    private int _healthyCount;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private bool _hasHijackedItems;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _lastScanTime = "尚未扫描";

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var results = await Task.Run(() => _scanner.Scan());
            Items.Clear();
            foreach (var item in results)
            {
                Items.Add(new AssociationCardViewModel(item, FixItemAsync, ShowDetail));
            }

            TotalCount = results.Count;
            HealthyCount = results.Count(r => r.Status == AssociationStatus.Healthy);
            HasHijackedItems = results.Any(r => r.Status == AssociationStatus.Hijacked);
            LastScanTime = DateTime.Now.ToString("HH:mm:ss");
            _logService.Info($"扫描完成：{HealthyCount}/{TotalCount} 正常");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(HasHijackedItems))]
    private async Task FixAllAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var hijacked = Items
                .Where(i => i.Status == AssociationStatus.Hijacked)
                .Select(i => i.Model)
                .ToList();

            var results = await Task.Run(() => _fixer.FixAll(hijacked));
            await RefreshAsync();
            ShowFixSummary(results);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task FixItemAsync(AssociationInfo item)
    {
        var result = await Task.Run(() => _fixer.Fix(item));
        await RefreshAsync();
        if (result.Status == FixStatus.Failed)
        {
            MessageBox.Show(result.Message, "修复失败", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private static void ShowFixSummary(IReadOnlyList<FixResult> results)
    {
        var failed = results.Where(r => r.Status == FixStatus.Failed).ToList();
        if (failed.Count == 0)
        {
            return;
        }

        var message = string.Join(Environment.NewLine, failed.Select(f => $"{f.Extension}: {f.Message}"));
        MessageBox.Show(message, "部分修复失败", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void ShowDetail(AssociationInfo item) => _detailDrawer.Show(item);

    partial void OnHasHijackedItemsChanged(bool value) => FixAllCommand.NotifyCanExecuteChanged();
}
