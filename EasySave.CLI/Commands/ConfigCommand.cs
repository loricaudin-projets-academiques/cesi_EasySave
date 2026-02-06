using EasySave.Core.Settings;
using Spectre.Console;
using Spectre.Console.Cli;

namespace EasySave.CLI.Commands
{
    public class ConfigCommand : Command<CommandSettings>
    {
        private readonly Config _config;

        public ConfigCommand(Config config) => _config = config;

        public override int Execute(CommandContext context, CommandSettings settings, CancellationToken cancellationToken)
        {
            try
            {
                DisplayConfig();
                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]✗ Erreur: {ex.Message}[/]");
                return 1;
            }
        }

        private void DisplayConfig()
        {
            var table = new Table();
            table.Title = new TableTitle("[bold cyan]Configuration[/]");
            table.AddColumn("[yellow]Paramètre[/]");
            table.AddColumn("[cyan]Valeur[/]");

            table.AddRow("Langue", $"[green]{_config.Language}[/]");

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Les logs sont gérés par LogConfiguration (AppData/ProSoft/EasySave/logs)[/]");
        }
    }
}
