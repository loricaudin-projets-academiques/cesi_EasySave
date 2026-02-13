using EasySave.Core.DependencyInjection;
using EasySave.Core.Settings;
using EasySave.VM.DependencyInjection;
using EasySave.VM.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace EasySave.GUI;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        var config = Config.Load();

        var services = new ServiceCollection()
            .AddEasySaveCore(config)
            .AddEasySaveLogging(config)
            .AddViewModels();  // Extension locale GUI

        Services = services.BuildServiceProvider();
        Services.WireEasySaveServices();

        var mainWindow = new MainWindow
        {
            DataContext = Services.GetRequiredService<MainViewModel>()
        };
        mainWindow.Show();

        base.OnStartup(e);
    }
}
