using EasySave.Core.Settings;
using Spectre.Console;
using Spectre.Console.Cli;

namespace EasySave.CLI.Commands
{
    public class ListCommand : Command<CommandSettings>
    {
        private readonly Config _config;

        public ListCommand(Config config) => _config = config;

        public override int Execute(CommandContext context, CommandSettings settings, CancellationToken cancellationToken)
        {
            try
            {
                AnsiConsole.MarkupLine($"[grey]{_config.Localization.Get("messages.loading")}[/]");

                var table = new Table();
                table.Title = new TableTitle(_config.Localization.Get("commands.list.header"));
                table.AddColumn(_config.Localization.Get("commands.list.column_id"));
                table.AddColumn(_config.Localization.Get("commands.list.column_name"));
                table.AddColumn(_config.Localization.Get("commands.list.column_source"));
                table.AddColumn(_config.Localization.Get("commands.list.column_destination"));
                table.AddColumn(_config.Localization.Get("commands.list.column_type"));

                table.AddRow("1", "MonProjet", "C:/Source", "D:/Dest", $"[green]{_config.Localization.Get("backup_types.full")}[/]");
                table.AddRow("2", "Images", "C:/Photos", "E:/Save", $"[yellow]{_config.Localization.Get("backup_types.diff")}[/]");

                AnsiConsole.Write(table);
                AnsiConsole.MarkupLine($"[grey]{_config.Localization.Get("commands.list.total", 2)}[/]");

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



