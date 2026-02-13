using EasySave.VM.ViewModels;
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
        services.AddTransient<MainViewModel>();
        
        return services;
    }
}
