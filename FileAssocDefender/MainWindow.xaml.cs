using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace FileAssocDefender;

public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();
        SystemThemeWatcher.Watch(this, WindowBackdropType.Mica);
    }

    protected override void OnClosed(EventArgs e)
    {
        App.Services.GetService<Services.Guardian>()?.Dispose();
        base.OnClosed(e);
    }
}
