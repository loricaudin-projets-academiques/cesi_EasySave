using EasySave.Core.DependencyInjection;
using EasySave.Core.Settings;
using EasySave.GUI.Services;
using EasySave.VM.DependencyInjection;
using EasySave.VM.ViewModels;
using EasySave.VM.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace EasySave.GUI;

public partial class App : System.Windows.Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        var config = Config.Load();

        var services = new ServiceCollection()
            .AddEasySaveCore(config)
            .AddEasySaveLogging(config)
            .AddViewModels();  // VM registrations

        // GUI-only services (strict MVVM abstractions)
        services.AddSingleton<IUiDispatcher, WpfUiDispatcher>();
        services.AddSingleton<IFolderPickerService, FolderPickerService>();

        Services = services.BuildServiceProvider();
        Services.WireEasySaveServices();

        // Subscribe GUI VM to backup events (progress/state) without coupling Core to WPF
        var backupService = Services.GetRequiredService<EasySave.Core.Services.BackupWorkService>();
        var backupListVm = Services.GetRequiredService<BackupListViewModel>();
        backupService.AddObserver(backupListVm);

        var mainWindow = new MainWindow
        {
            DataContext = Services.GetRequiredService<MainViewModel>()
        };
        mainWindow.Show();

        base.OnStartup(e);
    }
}
