using EasySave.Core.Services;
using EasySave.Core.Localization;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace EasySave.CLI.Commands
{
    public class ModifySettings : CommandSettings
    {
        [CommandArgument(0, "<ID>")]
        [Description("ID du travail à modifier")]
        public int Id { get; set; }

        [CommandOption("-n|--name")]
        [Description("Nouveau nom")]
        public string? NewName { get; set; }

        [CommandOption("-s|--source")]
        [Description("Nouveau chemin source")]
        public string? NewSource { get; set; }

        [CommandOption("-d|--destination")]
        [Description("Nouveau chemin destination")]
        public string? NewDestination { get; set; }

        [CommandOption("-t|--type")]
        [Description("Nouveau type (full/diff)")]
        public string? NewType { get; set; }
    }

    public class ModifyCommand : Command<ModifySettings>
    {
        private readonly BackupWorkService _backupService;
        private readonly ILocalizationService _localization;

        public ModifyCommand(BackupWorkService backupService, ILocalizationService localization)
        {
            _backupService = backupService;
            _localization = localization;
        }

        public override int Execute(CommandContext context, ModifySettings settings, CancellationToken cancellationToken)
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

                if (string.IsNullOrEmpty(settings.NewName) && string.IsNullOrEmpty(settings.NewSource) &&
                    string.IsNullOrEmpty(settings.NewDestination) && string.IsNullOrEmpty(settings.NewType))
                {
                    AnsiConsole.MarkupLine($"[yellow]{_localization.Get("commands.modify.no_changes")}[/]");
                    return 1;
                }

                if (_backupService.ModifyWork(index, settings.NewName, settings.NewSource, settings.NewDestination, settings.NewType))
                {
                    AnsiConsole.MarkupLine($"[green]{_localization.Get("commands.modify.success", work.GetName())}[/]");
                    if (settings.NewName != null)
                        AnsiConsole.MarkupLine($"   [grey]Nom:[/] {settings.NewName}");
                    if (settings.NewSource != null)
                        AnsiConsole.MarkupLine($"   [grey]Source:[/] {settings.NewSource}");
                    if (settings.NewDestination != null)
                        AnsiConsole.MarkupLine($"   [grey]Destination:[/] {settings.NewDestination}");
                    if (settings.NewType != null)
                        AnsiConsole.MarkupLine($"   [grey]Type:[/] {(settings.NewType == "full" ? _localization.Get("backup_types.full") : _localization.Get("backup_types.diff"))}");
                    return 0;
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]{_localization.Get("commands.modify.error")}[/]");
                    return 1;
                }
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
                AnsiConsole.MarkupLine($"[red]{_localization.Get("errors.general", ex.Message)}[/]");
                return 1;
            }
        }
    }
}


