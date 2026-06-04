using FileAssocDefender.Infrastructure;
using FileAssocDefender.Services;
using FileAssocDefender.ViewModels;
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

        var mainWindow = new MainWindow
        {
            DataContext = Services.GetRequiredService<MainViewModel>()
        };

        MainWindow = mainWindow;
        mainWindow.Show();

        if (mainWindow.DataContext is MainViewModel viewModel)
        {
            viewModel.Initialize();
        }
    }
}
