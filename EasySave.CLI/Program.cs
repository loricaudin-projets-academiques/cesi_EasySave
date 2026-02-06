using EasySave.Core.Settings;
using EasySave.CLI.Commands;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;


// ============ CONFIGURATION ============

var config = Config.Load();

// ============ INJECTION DE DÉPENDANCES ============

var services = new ServiceCollection();
services.AddSingleton(config);

// Commandes
services.AddSingleton<AddCommand>();
services.AddSingleton<DeleteCommand>();
services.AddSingleton<ListCommand>();
services.AddSingleton<ModifyCommand>();
services.AddSingleton<RunCommand>();
services.AddSingleton<ConfigCommand>();

// ============ APPLICATION ============

var app = new CommandApp(new TypeRegistrar(services));

app.Configure(c =>
{
    c.SetApplicationName("EasySave");

    c.AddCommand<ListCommand>("list").WithDescription("Afficher les travaux");
    c.AddCommand<AddCommand>("add").WithDescription("Ajouter un travail");
    c.AddCommand<RunCommand>("run").WithDescription("Exécuter les travaux");
    c.AddCommand<DeleteCommand>("delete").WithDescription("Supprimer un travail");
    c.AddCommand<ModifyCommand>("modify").WithDescription("Modifier un travail");
    c.AddCommand<ConfigCommand>("config").WithDescription("Afficher la config");
});

return app.Run(args);

// ============ DI INFRASTRUCTURE ============

internal class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _services;

    public TypeRegistrar(IServiceCollection services) => _services = services;

    public ITypeResolver Build() => new TypeResolver(_services.BuildServiceProvider());

    public void Register(Type service, Type implementation) => _services.AddSingleton(service, implementation);
    public void Register(Type service) => _services.AddSingleton(service);
    public void RegisterInstance(Type service, object implementation) => _services.AddSingleton(service, implementation);
    public void RegisterLazy(Type service, Func<object> factory) => _services.AddSingleton(service, _ => factory());
}

internal class TypeResolver : ITypeResolver
{
    private readonly ServiceProvider _provider;

    public TypeResolver(ServiceProvider provider) => _provider = provider;

    public object? Resolve(Type? type) => _provider.GetService(type);

    public void Dispose() => _provider?.Dispose();
}
