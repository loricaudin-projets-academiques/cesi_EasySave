using EasySave.Core.Settings;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace EasySave.CLI.Commands
{
    public class ModifySettings : CommandSettings
    {
        [CommandArgument(0, "<ID>")]
        public int Id { get; set; }

        [CommandOption("-n|--name")]
        [Description("Nouveau nom")]
        public string? NewName { get; set; }
    }

    public class ModifyCommand : Command<ModifySettings>
    {
        private readonly Config _config;

        public ModifyCommand(Config config) => _config = config;

        public override int Execute(CommandContext context, ModifySettings settings, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(settings.NewName))
                {
                    AnsiConsole.MarkupLine($"[yellow]{_config.Localization.Get("errors.invalid_argument", "Aucune modification demandée")}[/]");
                    return 1;
                }

                AnsiConsole.MarkupLine($"[green]{_config.Localization.Get("commands.modify.success", settings.Id.ToString())}[/]");
                AnsiConsole.MarkupLine($"   [grey]Nom:[/] {settings.NewName}");
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


