using EasySave.Core.Settings;
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
        private readonly Config _config;

        public RunCommand(Config config) => _config = config;

        public override int Execute(CommandContext context, RunSettings settings, CancellationToken cancellationToken)
        {
            try
            {
                var indices = ParseSelection(settings.Ids);

                if (indices.Count == 0)
                {
                    AnsiConsole.MarkupLine($"[red]{_config.Localization.Get("commands.run.error_invalid_indices", settings.Ids)}[/]");
                    return 1;
                }

                foreach (var id in indices)
                {
                    AnsiConsole.MarkupLine($"[blue]{_config.Localization.Get("commands.run.starting", id.ToString())}[/]");
                    AnsiConsole.MarkupLine($"[green]{_config.Localization.Get("commands.run.completed")}[/]");
                }

                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{_config.Localization.Get("errors.general", ex.Message)}[/]");
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



