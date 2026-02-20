using EasySave.VM.ViewModels;
using EasySave.VM.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EasySave.VM.DependencyInjection;

/// <summary>
/// Extensions DI pour les ViewModels.
/// </summary>
public static class ViewModelExtensions
{
    /// <summary>
    /// Ajoute tous les ViewModels au conteneur DI.
    /// </summary>
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddSingleton<IAppEvents, AppEvents>();
        services.AddSingleton<IShellNavigationService, ShellNavigationService>();

        services.AddSingleton<BackupListViewModel>();
        services.AddSingleton<BackupEditorViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<AboutViewModel>();

        services.AddSingleton<MainViewModel>();
        
        return services;
    }
}
