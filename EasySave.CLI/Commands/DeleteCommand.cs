using EasySave.Core.Settings;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace EasySave.CLI.Commands
{
    public class DeleteSettings : CommandSettings
    {
        [CommandArgument(0, "<ID>")]
        [Description("ID du travail à supprimer")]
        public int Id { get; set; }
    }

    public class DeleteCommand : Command<DeleteSettings>
    {
        private readonly Config _config;

        public DeleteCommand(Config config) => _config = config;

        public override int Execute(CommandContext context, DeleteSettings settings, CancellationToken cancellationToken)
        {
            try
            {
                AnsiConsole.MarkupLine($"[green]{_config.Localization.Get("commands.delete.success", settings.Id.ToString())}[/]");
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



