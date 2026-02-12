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
        [Description("Change language (fr/en)")]
        public string? Language { get; set; }

        [CommandOption("-t|--logtype <TYPE>")]
        [Description("Change le format des logs (json/xml)")]
        public string? LogType { get; set; }
    }

    public class ConfigCommand : Command<ConfigCommandSettings>
    {
        private static readonly string[] ValidLogTypes = { "json", "xml" };
        
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
                bool hasChanges = false;

                if (!string.IsNullOrEmpty(settings.Language))
                {
                    ChangeLanguage(settings.Language);
                    hasChanges = true;
                }

                if (!string.IsNullOrEmpty(settings.LogType))
                {
                    ChangeLogType(settings.LogType);
                    hasChanges = true;
                }

                if (!hasChanges)
                {
                    ChangeLogType(settings.LogType);
                    hasChanges = true;
                }

                if (!hasChanges)
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

        private void ChangeLogType(string logType)
        {
            var normalizedType = logType.ToLowerInvariant();
            
            if (!ValidLogTypes.Contains(normalizedType))
            {
                AnsiConsole.MarkupLine($"[red]✗[/] {_localization.Get("commands.config.invalid_logtype", logType, string.Join(", ", ValidLogTypes))}");
                return;
            }

            var oldType = _config.LogType;
            _config.LogType = normalizedType;
            _config.Save();
            
            AnsiConsole.MarkupLine($"[green]✓[/] {_localization.Get("commands.config.logtype_changed", oldType.ToUpperInvariant(), normalizedType.ToUpperInvariant())}");
        }

        private void DisplayConfig()
        {
            var table = new Table();
            table.Title = new TableTitle($"[bold cyan]{_localization.Get("commands.config.title")}[/]");
            table.AddColumn($"[yellow]{_localization.Get("commands.config.parameter")}[/]");
            table.AddColumn($"[cyan]{_localization.Get("commands.config.value")}[/]");

            table.AddRow(_localization.Get("commands.config.language"), $"[green]{_config.Language.GetDisplayName()}[/]");
            table.AddRow(_localization.Get("commands.config.logtype"), $"[green]{_config.LogType.ToUpperInvariant()}[/]");

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[grey]{_localization.Get("commands.config.change_hint")}[/]");
            AnsiConsole.MarkupLine($"[grey dim]{_localization.Get("commands.config.file_path", _config.ConfigFilePath)}[/]");
        }
    }
}
