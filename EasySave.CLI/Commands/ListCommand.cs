using EasySave.Core.Services;
using EasySave.Core.Localization;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace EasySave.CLI.Commands
{
    public class ListSettings : CommandSettings
    {
    }

    public class ListCommand : Command<ListSettings>
    {
        private readonly BackupWorkService _backupService;
        private readonly ILocalizationService _localization;

        public ListCommand(BackupWorkService backupService, ILocalizationService localization)
        {
            _backupService = backupService;
            _localization = localization;
        }

        public override int Execute(CommandContext context, ListSettings settings, CancellationToken cancellationToken)
        {
            try
            {
                var works = _backupService.GetAllWorks();

                if (works.Count == 0)
                {
                    AnsiConsole.MarkupLine($"[yellow]{_localization.Get("commands.list.no_works")}[/]");
                    return 0;
                }

                var table = new Table();
                table.Title = new TableTitle(_localization.Get("commands.list.header"));
                table.AddColumn(_localization.Get("commands.list.column_id"));
                table.AddColumn(_localization.Get("commands.list.column_name"));
                table.AddColumn(_localization.Get("commands.list.column_source"));
                table.AddColumn(_localization.Get("commands.list.column_destination"));
                table.AddColumn(_localization.Get("commands.list.column_type"));

                for (int i = 0; i < works.Count; i++)
                {
                    var work = works[i];
                    var typeColor = work.GetBackupType().ToString().Contains("FULL") ? "green" : "yellow";
                    table.AddRow(
                        (i + 1).ToString(),
                        work.GetName(),
                        work.GetSourcePath(),
                        work.GetDestinationPath(),
                        $"[{typeColor}]{_backupService.GetLocalizedBackupTypeName(work.GetBackupType())}[/]"
                    );
                }

                AnsiConsole.Write(table);
                AnsiConsole.MarkupLine($"[grey]{_localization.Get("commands.list.total", works.Count)}[/]");

                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{_localization.Get("errors.general", ex.Message)}[/]");
                return 1;
            }
        }
    }
}



