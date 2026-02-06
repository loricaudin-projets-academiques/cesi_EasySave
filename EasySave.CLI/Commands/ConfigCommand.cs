using EasySave.Core.Localization;
using EasySave.Core.Settings;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace EasySave.CLI.Commands
{
    public class ConfigCommandSettings : CommandSettings
    {
        [CommandOption("-l|--lang <LANGUAGE>")]
        [Description("Changer la langue (fr/en)")]
        public string? Language { get; set; }
    }

    public class ConfigCommand : Command<ConfigCommandSettings>
    {
        private readonly Config _config;
        private readonly ILocalizationService _localization;

        public ConfigCommand(Config config)
        {
            _config = config;
            _localization = config.Localization;
        }

        public override int Execute(CommandContext context, ConfigCommandSettings settings, CancellationToken cancellationToken)
        {
            try
            {
                if (!string.IsNullOrEmpty(settings.Language))
                {
                    ChangeLanguage(settings.Language);
                }
                else
                {
                    DisplayConfig();
                }
                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]✗ {_localization.Get("errors.generic", ex.Message)}[/]");
                return 1;
            }
        }

        private void ChangeLanguage(string langCode)
        {
            var newLang = LanguageExtensions.GetEnumByCode(langCode);
            var oldLang = _config.Language;
            
            _config.Language = newLang;
            _config.Save();
            
            AnsiConsole.MarkupLine($"[green]✓[/] {_localization.Get("commands.config.language_changed", oldLang.GetDisplayName(), newLang.GetDisplayName())}");
        }

        private void DisplayConfig()
        {
            var table = new Table();
            table.Title = new TableTitle($"[bold cyan]{_localization.Get("commands.config.title")}[/]");
            table.AddColumn($"[yellow]{_localization.Get("commands.config.parameter")}[/]");
            table.AddColumn($"[cyan]{_localization.Get("commands.config.value")}[/]");

            table.AddRow(_localization.Get("commands.config.language"), $"[green]{_config.Language.GetDisplayName()}[/]");

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[grey]{_localization.Get("commands.config.change_hint")}[/]");
        }
    }
}
