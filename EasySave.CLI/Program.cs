using EasySave.Core.DependencyInjection;
using EasySave.Core.Settings;
using EasySave.CLI.Commands;
using EasySave.CLI.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

// ============ CONFIGURATION ============

var config = Config.Load();

// ============ INJECTION DE DÉPENDANCES ============

var services = new ServiceCollection()
    .AddEasySaveCore(config)
    .AddEasySaveLogging(config)
    .AddCliCommands();

// ============ CONSTRUCTION DU PROVIDER (une seule fois) ============

var provider = services.BuildServiceProvider();
provider.WireEasySaveServices();

// ============ APPLICATION ============

var app = new CommandApp(new TypeRegistrar(provider));

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
    private readonly IServiceProvider _provider;

    public TypeRegistrar(IServiceProvider provider) => _provider = provider;

    public ITypeResolver Build() => new TypeResolver(_provider);

    // Ces méthodes ne sont plus utilisées car le provider est déjà construit
    public void Register(Type service, Type implementation) { }
    public void Register(Type service) { }
    public void RegisterInstance(Type service, object implementation) { }
    public void RegisterLazy(Type service, Func<object> factory) { }
}

internal class TypeResolver : ITypeResolver
{
    private readonly IServiceProvider _provider;

    public TypeResolver(IServiceProvider provider) => _provider = provider;

    public object? Resolve(Type? type) => type is not null ? _provider.GetService(type) : null;

    public void Dispose() { }
}

