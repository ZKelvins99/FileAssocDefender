using FileAssocDefender.Services;
using FileAssocDefender.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FileAssocDefender.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileAssocDefenderServices(this IServiceCollection services)
    {
        services.AddSingleton<RegistryHelper>();
        services.AddSingleton<IconResolver>();
        services.AddSingleton<OfficeDetector>();
        services.AddSingleton<PresetStore>();
        services.AddSingleton<AssociationApi>();
        services.AddSingleton<LogService>();
        services.AddSingleton<AssociationScanner>();
        services.AddSingleton<AssociationFixer>();
        services.AddSingleton<Guardian>();

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<AssociationListViewModel>();
        services.AddSingleton<LogViewModel>();
        services.AddSingleton<SettingsViewModel>();

        return services;
    }
}
