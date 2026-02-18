using EasySave.Core.Services;
using EasySave.Core.Localization;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace EasySave.CLI.Commands
{
    public class RunSettings : CommandSettings
    {
        [CommandArgument(0, "<IDS>")]
        [Description("Liste des IDs (ex: 1-3 ou 1;3)")]
        public string Ids { get; set; }
    }

    public class RunCommand : Command<RunSettings>
    {
        private readonly BackupWorkService _backupService;
        private readonly ILocalizationService _localization;

        public RunCommand(BackupWorkService backupService, ILocalizationService localization)
        {
            _backupService = backupService;
            _localization = localization;
        }

        public override int Execute(CommandContext context, RunSettings settings, CancellationToken cancellationToken)
        {
            try
            {
                var indices = ParseSelection(settings.Ids);

                if (indices.Count == 0)
                {
                    AnsiConsole.MarkupLine($"[red]{_localization.Get("commands.run.error_invalid_indices", settings.Ids)}[/]");
                    return 1;
                }

                foreach (var id in indices)
                {
                    int index = id - 1;
                    var work = _backupService.GetWorkByIndex(index);

                    if (work == null)
                    {
                        AnsiConsole.MarkupLine($"[yellow]{_localization.Get("errors.work_not_found", id.ToString())}[/]");
                        continue;
                    }

                    AnsiConsole.MarkupLine($"[blue]{_localization.Get("commands.run.starting", work.GetName())}[/]");
                    
                    try
                    {
                        _backupService.ExecuteWork(index);
                        AnsiConsole.MarkupLine($"[green]{_localization.Get("commands.run.completed")}[/]");
                    }
                    catch (BusinessSoftwareRunningException ex)
                    {
                        AnsiConsole.MarkupLine($"[red]Backup blocked: {ex.SoftwareName} is running[/]");
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[red]{_localization.Get("errors.execution_failed", ex.Message)}[/]");
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{_localization.Get("errors.general", ex.Message)}[/]");
                return 1;
            }
        }

        private List<int> ParseSelection(string selection)
        {
            var result = new List<int>();

            if (selection.Contains('-'))
            {
                var parts = selection.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end))
                {
                    for (int i = start; i <= end; i++) result.Add(i);
                }
            }
            else if (selection.Contains(';'))
            {
                foreach (var part in selection.Split(';'))
                {
                    if (int.TryParse(part, out int id)) result.Add(id);
                }
            }
            else if (int.TryParse(selection, out int single))
            {
                result.Add(single);
            }

            return result.Distinct().OrderBy(x => x).ToList();
        }
    }
}



