using EasySave.Core.Localization;
using System.Text.Json;

namespace EasySave.Core.Settings
{
    /// <summary>
    /// Application configuration loaded from appsettings.json.
    /// </summary>
    public class Config
    {
        /// <summary>Current language setting.</summary>
        public Language Language { get; set; } = Language.French;
        
        /// <summary>Localization service instance.</summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public ILocalizationService Localization => _localization ??= new LocalizationService(Language);
        private ILocalizationService? _localization;

        /// <summary>
        /// Loads configuration from appsettings.json.
        /// </summary>
        /// <returns>Config instance with loaded settings.</returns>
        public static Config Load()
        {
            try
            {
                var configPath = FindConfigFile();
                if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
                    return new Config { Language = Language.French };

                var json = File.ReadAllText(configPath);
                using var doc = JsonDocument.Parse(json);
                var appSettings = doc.RootElement.GetProperty("AppSettings");
                var languageStr = appSettings.GetProperty("Language").GetString() ?? "fr";

                return new Config 
                { 
                    Language = languageStr.ToLowerInvariant() switch 
                    { 
                        "en" or "english" => Language.English, 
                        _ => Language.French 
                    } 
                };
            }
            catch
            {
                return new Config { Language = Language.French };
            }
        }

        /// <summary>
        /// Saves current configuration to appsettings.json.
        /// </summary>
        public void Save()
        {
            var configPath = FindConfigFile() ?? "appsettings.json";
            var json = JsonSerializer.Serialize(new 
            { 
                AppSettings = new { Language = Language.GetCode() } 
            }, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, json);
            
            _localization = null;
        }

        private static string? FindConfigFile()
        {
            var paths = new[] { 
                "appsettings.json",
                Path.Combine(AppContext.BaseDirectory, "appsettings.json")
            };
            return paths.FirstOrDefault(File.Exists);
        }
    }
}




