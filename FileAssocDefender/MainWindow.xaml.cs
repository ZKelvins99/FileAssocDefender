using System.ComponentModel;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace FileAssocDefender;

public partial class MainWindow : FluentWindow
{
    private bool _allowClose;
    private Services.TrayService? _trayService;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        SystemThemeWatcher.Watch(this, WindowBackdropType.Mica);
    }

    public void RequestExit() => _allowClose = true;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _trayService = App.Services.GetRequiredService<Services.TrayService>();
        _trayService.Initialize(this);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_allowClose)
        {
            e.Cancel = true;
            Hide();
        }

        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        _trayService?.Dispose();
        App.Services.GetService<Services.Guardian>()?.Dispose();
        base.OnClosed(e);
    }
}
