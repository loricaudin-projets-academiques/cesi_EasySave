using EasySave.CLI.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace EasySave.CLI.Infrastructure;

/// <summary>
/// Extensions DI spécifiques au CLI.
/// </summary>
public static class CliServiceExtensions
{
    /// <summary>
    /// Ajoute toutes les commandes CLI au conteneur DI.
    /// </summary>
    public static IServiceCollection AddCliCommands(this IServiceCollection services)
    {
        services.AddSingleton<AddCommand>();
        services.AddSingleton<DeleteCommand>();
        services.AddSingleton<ListCommand>();
        services.AddSingleton<ModifyCommand>();
        services.AddSingleton<RunCommand>();
        services.AddSingleton<ConfigCommand>();
        
        return services;
    }
}
