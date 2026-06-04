using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        var menu = CreateContextMenu(mainWindow);

        _icon = new TaskbarIcon
        {
            ToolTipText = "FileAssocDefender",
            ContextMenu = menu,
            Visibility = Visibility.Visible
        };

        _icon.PreviewTrayContextMenuOpen += (_, _) =>
        {
            Application.Current.Dispatcher.BeginInvoke(
                () => AlignContextMenuToCursor(menu),
                System.Windows.Threading.DispatcherPriority.Send);
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

        // Opened 作为兜底：库在 WM_CONTEXTMENU 上可能给出错误坐标
        menu.Opened += (_, _) => AlignContextMenuToCursor(menu);

        return menu;
    }

    /// <summary>
    /// Hardcodet 从 WM_CONTEXTMENU 的 wParam 取坐标，在 Win10/11 上常错误，导致菜单出现在屏幕右下角。
    /// </summary>
    private static void AlignContextMenuToCursor(ContextMenu menu)
    {
        if (!GetCursorPos(out var cursor))
        {
            return;
        }

        var (scaleX, scaleY) = GetDpiScale();
        menu.Placement = PlacementMode.AbsolutePoint;
        menu.HorizontalOffset = cursor.X / scaleX;
        menu.VerticalOffset = cursor.Y / scaleY;
    }

    private static (double X, double Y) GetDpiScale()
    {
        var window = Application.Current.MainWindow;
        if (window is not null)
        {
            var source = PresentationSource.FromVisual(window);
            if (source?.CompositionTarget is not null)
            {
                var matrix = source.CompositionTarget.TransformToDevice;
                return (matrix.M11, matrix.M22);
            }
        }

        using var graphics = Graphics.FromHwnd(IntPtr.Zero);
        return (graphics.DpiX / 96.0, graphics.DpiY / 96.0);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetCursorPos(out NativePoint lpPoint);

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
