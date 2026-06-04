using FileAssocDefender.Infrastructure;
using FileAssocDefender.Services;
using FileAssocDefender.ViewModels;
using FileAssocDefender.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using Wpf.Ui.Appearance;

namespace FileAssocDefender;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 引导页关闭时主窗口尚未 Show，须避免 OnLastWindowClose 导致进程直接退出
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        ApplicationThemeManager.ApplySystemTheme();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddFileAssocDefenderServices();
        Services = serviceCollection.BuildServiceProvider();

        Services.GetRequiredService<PresetStore>().Load();

        var appSettings = Services.GetRequiredService<AppSettingsService>();
        appSettings.Load();

        if (!appSettings.Settings.HasCompletedWelcome)
        {
            var viewModel = Services.GetRequiredService<WelcomeViewModel>();
            var welcome = new WelcomeWindow
            {
                DataContext = viewModel
            };

            welcome.ShowDialog();

            if (!viewModel.DialogResult)
            {
                Shutdown();
                return;
            }
        }

        ShowMainWindow();
    }

    private static void ShowMainWindow()
    {
        var mainWindow = new MainWindow
        {
            DataContext = Services.GetRequiredService<MainViewModel>()
        };

        Current.MainWindow = mainWindow;
        Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        mainWindow.Show();

        if (mainWindow.DataContext is MainViewModel viewModel)
        {
            viewModel.Initialize();
        }
    }
}
