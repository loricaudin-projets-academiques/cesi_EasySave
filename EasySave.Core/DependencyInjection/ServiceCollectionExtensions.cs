using EasySave.Core.Settings;
using EasySave.Core.Services;
using EasySave.Core.Services.Logging;
using EasySave.Core.Models;
using EasySave.Core.Localization;
using EasyLog.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasySave.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all Core services shared between CLI and GUI.
    /// </summary>
    public static IServiceCollection AddEasySaveCore(this IServiceCollection services, Config? config = null)
    {
        config ??= Config.Load();
        
        services.AddSingleton(config);
        services.AddSingleton<ILocalizationService>(_ => new LocalizationService(config.Language));
        services.AddSingleton<BackupWorkList>();
        
        // CryptoSoft service for file encryption
        services.AddSingleton<CryptoSoftService>();

        // Business software detection service
        services.AddSingleton<BusinessSoftwareService>();
        
        // BackupWorkService with all service injections
        services.AddSingleton<BackupWorkService>(sp => new BackupWorkService(
            sp.GetRequiredService<ILocalizationService>(),
            sp.GetRequiredService<BackupWorkList>(),
            sp.GetService<EasyLog.Services.EasyLogger>(),
            sp.GetService<CryptoSoftService>(),
            sp.GetService<BusinessSoftwareService>()
        ));

        // Parallel backup job engine
        services.AddSingleton<BackupJobEngine>(sp => new BackupJobEngine(
            sp.GetRequiredService<BackupWorkService>(),
            sp.GetService<BusinessSoftwareService>()
        ));
        
        return services;
    }

    /// <summary>
    /// Adds logging services.
    /// </summary>
    public static IServiceCollection AddEasySaveLogging(this IServiceCollection services, Config? config = null)
    {
        config ??= Config.Load();
        
        var logConfig = new LogConfiguration { LogFormat = config.LogType };
        services.AddSingleton(_ => new EasyLog.Services.EasyLogger(logConfig));
        // FileTransferLogger removed - BackupWorkService handles logging with EncryptionTime
        services.AddSingleton<IBackupEventObserver, EasyLogObserver>();
        
        return services;
    }

    /// <summary>
    /// Configures subscriptions between services.
    /// </summary>
    public static IServiceProvider WireEasySaveServices(this IServiceProvider provider)
    {
        var backupService = provider.GetRequiredService<BackupWorkService>();
        var observer = provider.GetRequiredService<IBackupEventObserver>();

        // FileTransferLogger subscription removed - logging done directly in BackupWorkService
        backupService.AddObserver(observer);
        
        return provider;
    }
}