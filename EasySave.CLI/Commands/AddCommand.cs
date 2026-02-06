using EasySave.Core.Settings;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace EasySave.CLI.Commands
{
    public class AddSettings : CommandSettings
    {
        [CommandArgument(0, "<NAME>")]
        [Description("Nom du travail")]
        public string Name { get; set; }

        [CommandArgument(1, "<SOURCE>")]
        [Description("Chemin source")]
        public string Source { get; set; }

        [CommandArgument(2, "<DEST>")]
        [Description("Chemin destination")]
        public string Destination { get; set; }

        [CommandOption("-t|--type")]
        [Description("Type de sauvegarde (full/diff)")]
        [DefaultValue("full")]
        public string Type { get; set; }
    }

    public class AddCommand : Command<AddSettings>
    {
        private readonly Config _config;

        public AddCommand(Config config) => _config = config;

        public override int Execute(CommandContext context, AddSettings settings, CancellationToken cancellationToken)
        {
            try
            {
                if (settings.Type != "full" && settings.Type != "diff")
                {
                    AnsiConsole.MarkupLine($"[red]{_config.Localization.Get("commands.add.error_invalid_type", settings.Type)}[/]");
                    return 1;
                }

                AnsiConsole.MarkupLine($"[green]{_config.Localization.Get("commands.add.success", settings.Name)}[/]");
                AnsiConsole.MarkupLine($"   [grey]Source:[/] {settings.Source}");
                AnsiConsole.MarkupLine($"   [grey]Destination:[/] {settings.Destination}");
                AnsiConsole.MarkupLine($"   [grey]Type:[/] {(settings.Type == "full" ? _config.Localization.Get("backup_types.full") : _config.Localization.Get("backup_types.diff"))}");

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



