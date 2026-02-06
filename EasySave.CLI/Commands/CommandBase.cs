using EasySave.Core.Settings;
using EasySave.Core.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace EasySave.CLI.Commands
{
    /// <summary>
    /// Classe de base pour tous les Commands
    /// ? Centralise: injection, localization, gestion d'erreurs
    /// ? Réduit la duplication de code
    /// </summary>
    public abstract class CommandBase<TSettings> : Command<TSettings> where TSettings : CommandSettings
    {
        protected readonly Config Config;
        protected readonly BackupWorkService BackupService;

        protected CommandBase(Config config, BackupWorkService backupService)
        {
            Config = config;
            BackupService = backupService;
        }

        /// <summary>
        /// Affiche une erreur localisée
        /// </summary>
        protected void ShowError(string localizationKey, params object[] args)
        {
            var message = Config.Localization.Get(localizationKey, args);
            AnsiConsole.MarkupLine($"[red]{message}[/]");
        }

        /// <summary>
        /// Affiche un message de succès
        /// </summary>
        protected void ShowSuccess(string localizationKey, params object[] args)
        {
            var message = Config.Localization.Get(localizationKey, args);
            AnsiConsole.MarkupLine($"[green]{message}[/]");
        }

        /// <summary>
        /// Affiche un message d'avertissement
        /// </summary>
        protected void ShowWarning(string localizationKey, params object[] args)
        {
            var message = Config.Localization.Get(localizationKey, args);
            AnsiConsole.MarkupLine($"[yellow]{message}[/]");
        }

        /// <summary>
        /// Affiche un message info
        /// </summary>
        protected void ShowInfo(string localizationKey, params object[] args)
        {
            var message = Config.Localization.Get(localizationKey, args);
            AnsiConsole.MarkupLine($"[blue]{message}[/]");
        }

        /// <summary>
        /// Wraps l'exécution avec gestion d'erreurs automatique
        /// </summary>
        protected int ExecuteWithErrorHandling(Func<int> action)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                ShowError("errors.general", ex.Message);
                return 1;
            }
        }
    }
}
