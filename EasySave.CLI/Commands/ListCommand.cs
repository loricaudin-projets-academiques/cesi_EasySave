using EasySave.Core.Settings;
using EasySave.Core.Services;
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
        private readonly Config _config;
        private readonly BackupWorkService _backupService;

        public ListCommand(Config config, BackupWorkService backupService)
        {
            _config = config;
            _backupService = backupService;
        }

        public override int Execute(CommandContext context, ListSettings settings, CancellationToken cancellationToken)
        {
            try
            {
                var works = _backupService.GetAllWorks();

                if (works.Count == 0)
                {
                    AnsiConsole.MarkupLine($"[yellow]{_config.Localization.Get("commands.list.no_works")}[/]");
                    return 0;
                }

                var table = new Table();
                table.Title = new TableTitle(_config.Localization.Get("commands.list.header"));
                table.AddColumn(_config.Localization.Get("commands.list.column_id"));
                table.AddColumn(_config.Localization.Get("commands.list.column_name"));
                table.AddColumn(_config.Localization.Get("commands.list.column_source"));
                table.AddColumn(_config.Localization.Get("commands.list.column_destination"));
                table.AddColumn(_config.Localization.Get("commands.list.column_type"));

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
                AnsiConsole.MarkupLine($"[grey]{_config.Localization.Get("commands.list.total", works.Count)}[/]");

                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{_config.Localization.Get("errors.general", ex.Message)}[/]");
                return 1;
            }
        }
    }
}



