using System.Drawing;
using System.Windows;
using FileAssocDefender.Models;
using FileAssocDefender.ViewModels;
using Hardcodet.Wpf.TaskbarNotification;
using WpfControls = System.Windows.Controls;

namespace FileAssocDefender.Services;

public sealed class TrayService : IDisposable
{
    private readonly Guardian _guardian;
    private readonly AssociationListViewModel _associationListViewModel;
    private TaskbarIcon? _icon;

    public TrayService(Guardian guardian, AssociationListViewModel associationListViewModel)
    {
        _guardian = guardian;
        _associationListViewModel = associationListViewModel;
    }

    public void Initialize(Window mainWindow)
    {
        _icon = new TaskbarIcon
        {
            ToolTipText = "FileAssocDefender",
            ContextMenu = CreateContextMenu(mainWindow),
            Visibility = Visibility.Visible
        };

        SetNormalIcon();

        _guardian.HijackDetected += OnHijackDetected;
        _guardian.ScanCompleted += OnScanCompleted;
    }

    private WpfControls.ContextMenu CreateContextMenu(Window mainWindow)
    {
        var menu = new WpfControls.ContextMenu();

        var showItem = new WpfControls.MenuItem { Header = "显示主窗口" };
        showItem.Click += (_, _) => ShowMainWindow(mainWindow);
        menu.Items.Add(showItem);

        var scanItem = new WpfControls.MenuItem { Header = "立即扫描" };
        scanItem.Click += (_, _) => _associationListViewModel.RefreshCommand.Execute(null);
        menu.Items.Add(scanItem);

        menu.Items.Add(new WpfControls.Separator());

        var exitItem = new WpfControls.MenuItem { Header = "退出" };
        exitItem.Click += (_, _) =>
        {
            if (Application.Current.MainWindow is MainWindow window)
            {
                window.RequestExit();
            }

            Application.Current.Shutdown();
        };
        menu.Items.Add(exitItem);

        return menu;
    }

    private void OnHijackDetected(AssociationInfo info)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _icon?.ShowBalloonTip(
                "检测到文件关联被劫持",
                $"{info.Extension} 当前为 {info.CurrentAppName}",
                BalloonIcon.Warning);
        });
    }

    private void OnScanCompleted(IReadOnlyList<AssociationInfo> results)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var hijacked = results.Count(r => r.Status == AssociationStatus.Hijacked);
            if (hijacked > 0)
            {
                SetWarningIcon();
            }
            else
            {
                SetNormalIcon();
            }
        });
    }

    private void SetNormalIcon()
    {
        if (_icon is null)
        {
            return;
        }

        _icon.Icon = SystemIcons.Shield;
    }

    private void SetWarningIcon()
    {
        if (_icon is null)
        {
            return;
        }

        _icon.Icon = SystemIcons.Warning;
    }

    private static void ShowMainWindow(Window mainWindow)
    {
        mainWindow.Show();
        mainWindow.WindowState = WindowState.Normal;
        mainWindow.Activate();
    }

    public void Dispose()
    {
        _guardian.HijackDetected -= OnHijackDetected;
        _guardian.ScanCompleted -= OnScanCompleted;
        _icon?.Dispose();
    }
}
