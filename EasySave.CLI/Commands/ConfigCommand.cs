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
                DisplayCurrentConfiguration();
                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{_config.Localization.Get("errors.general", ex.Message)}[/]");
                return 1;
            }
        }

        private void DisplayCurrentConfiguration()
        {
            AnsiConsole.MarkupLine("[bold cyan]Configuration Actuelle[/]\n");

            AnsiConsole.MarkupLine("[yellow]Localisation:[/]");
            AnsiConsole.MarkupLine($"  Langue: {_config.Language}");
            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine("[yellow]Logs:[/]");
            AnsiConsole.MarkupLine($"  Chemin: {_config.LogPath}");
            AnsiConsole.MarkupLine($"  Format: {_config.LogType}");
            AnsiConsole.WriteLine();
        }

    }
}



