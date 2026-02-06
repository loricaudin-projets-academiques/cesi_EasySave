using EasySave.Core.Localization;
using System.Text.Json;

namespace EasySave.Core.Settings
{
    /// <summary>
    /// Configuration unique - Charge depuis appsettings.json à la racine.
    /// Réutilisable pour CLI, Web, Desktop, etc.
    /// </summary>
    public class Config
    {
        public Language Language { get; set; } = Language.French;
        public ILocalizationService Localization { get; set; } = null!;
        public string LogPath { get; set; } = "./logs/";
        public string LogType { get; set; } = "json";

        public static Config Load()
        {
            try
            {
                // Chercher appsettings.json à la racine du projet
                // Depuis bin/Debug/net8.0, remonte jusqu'à la racine
                var configPath = Path.Combine(
                    AppContext.BaseDirectory,
                    "..", "..", "..",
                    "appsettings.json"
                );

                configPath = Path.GetFullPath(configPath);

                if (!File.Exists(configPath))
                {
                    throw new FileNotFoundException($"appsettings.json introuvable à: {configPath}");
                }

                var json = File.ReadAllText(configPath);
                using var doc = JsonDocument.Parse(json);
                var appSettings = doc.RootElement.GetProperty("AppSettings");

                var languageStr = appSettings.GetProperty("Language").GetString() ?? "fr";
                var logPath = appSettings.GetProperty("LogPath").GetString() ?? "./logs/";
                var logType = appSettings.GetProperty("LogType").GetString() ?? "json";

                var language = languageStr.ToLowerInvariant() switch
                {
                    "en" or "english" => Language.English,
                    _ => Language.French
                };

                return new Config
                {
                    Language = language,
                    Localization = new LocalizationService(language),
                    LogPath = logPath,
                    LogType = logType
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erreur lors du chargement de appsettings.json", ex);
            }
        }
    }
}



