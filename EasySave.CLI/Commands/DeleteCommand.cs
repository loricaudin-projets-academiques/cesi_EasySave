using EasySave.Core.Settings;
using EasySave.Core.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace EasySave.CLI.Commands
{
    public class DeleteSettings : CommandSettings
    {
        [CommandArgument(0, "<ID>")]
        [Description("ID du travail à supprimer")]
        public int Id { get; set; }
    }

    public class DeleteCommand : Command<DeleteSettings>
    {
        private readonly Config _config;
        private readonly BackupWorkService _backupService;

        public DeleteCommand(Config config, BackupWorkService backupService)
        {
            _config = config;
            _backupService = backupService;
        }

        public override int Execute(CommandContext context, DeleteSettings settings, CancellationToken cancellationToken)
        {
            try
            {
                // Convertir l'ID utilisateur (1-basé) en index (0-basé)
                int index = settings.Id - 1;

                var work = _backupService.GetWorkByIndex(index);
                if (work == null)
                {
                    AnsiConsole.MarkupLine($"[red]{_config.Localization.Get("errors.work_not_found", settings.Id.ToString())}[/]");
                    return 1;
                }

                if (_backupService.RemoveWorkByIndex(index))
                {
                    AnsiConsole.MarkupLine($"[green]{_config.Localization.Get("commands.delete.success", work.GetName())}[/]");
                    return 0;
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]{_config.Localization.Get("commands.delete.error")}[/]");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{_config.Localization.Get("errors.general", ex.Message)}[/]");
                return 1;
            }
        }
    }
}



