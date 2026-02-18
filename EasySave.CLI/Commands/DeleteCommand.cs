using EasySave.Core.Services;
using EasySave.Core.Localization;
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
        private readonly BackupWorkService _backupService;
        private readonly ILocalizationService _localization;

        public DeleteCommand(BackupWorkService backupService, ILocalizationService localization)
        {
            _backupService = backupService;
            _localization = localization;
        }

        public override int Execute(CommandContext context, DeleteSettings settings, CancellationToken cancellationToken)
        {
            try
            {
                int index = settings.Id - 1;

                var work = _backupService.GetWorkByIndex(index);
                if (work == null)
                {
                    AnsiConsole.MarkupLine($"[red]{_localization.Get("errors.work_not_found", settings.Id.ToString())}[/]");
                    return 1;
                }

                if (_backupService.RemoveWorkByIndex(index))
                {
                    AnsiConsole.MarkupLine($"[green]{_localization.Get("commands.delete.success", work.GetName())}[/]");
                    return 0;
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]{_localization.Get("commands.delete.error")}[/]");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{_localization.Get("errors.general", ex.Message)}[/]");
                return 1;
            }
        }
    }
}



