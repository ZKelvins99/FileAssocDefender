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

        ApplicationThemeManager.ApplySystemTheme();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddFileAssocDefenderServices();
        Services = serviceCollection.BuildServiceProvider();

        Services.GetRequiredService<PresetStore>().Load();

        var appSettings = Services.GetRequiredService<AppSettingsService>();
        appSettings.Load();

        if (!appSettings.Settings.HasCompletedWelcome)
        {
            var welcome = new WelcomeWindow
            {
                DataContext = Services.GetRequiredService<WelcomeViewModel>(),
                Owner = null
            };

            var accepted = welcome.ShowDialog() == true;
            if (!accepted)
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
        mainWindow.Show();

        if (mainWindow.DataContext is MainViewModel viewModel)
        {
            viewModel.Initialize();
        }
    }
}
