using EasySave.Core.Settings;
using EasySave.Core.Services;
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
        private readonly BackupWorkService _backupService;

        public AddCommand(Config config, BackupWorkService backupService) 
        {
            _config = config;
            _backupService = backupService;
        }

        public override int Execute(CommandContext context, AddSettings settings, CancellationToken cancellationToken)
        {
            try
            {
                if (settings.Type != "full" && settings.Type != "diff")
                {
                    AnsiConsole.MarkupLine($"[red]{_config.Localization.Get("commands.add.error_invalid_type", settings.Type)}[/]");
                    return 1;
                }

                _backupService.AddWork(settings.Name, settings.Source, settings.Destination, settings.Type);

                AnsiConsole.MarkupLine($"[green]{_config.Localization.Get("commands.add.success", settings.Name)}[/]");
                AnsiConsole.MarkupLine($"   [grey]Source:[/] {settings.Source}");
                AnsiConsole.MarkupLine($"   [grey]Destination:[/] {settings.Destination}");
                AnsiConsole.MarkupLine($"   [grey]Type:[/] {(settings.Type == "full" ? _config.Localization.Get("backup_types.full") : _config.Localization.Get("backup_types.diff"))}");

                return 0;
            }
            catch (DirectoryNotFoundException ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
                return 1;
            }
            catch (ArgumentException ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
                return 1;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{_config.Localization.Get("errors.general", ex.Message)}[/]");
                return 1;
            }
        }
    }
}



