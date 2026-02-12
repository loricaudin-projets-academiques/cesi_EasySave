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
        
        /// <summary>Log format type (json or xml).</summary>
        public string LogType { get; set; } = "json";
        
        /// <summary>Path to the loaded config file.</summary>
        public string ConfigFilePath { get; private set; } = string.Empty;
        
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
                {
                    // Create default config file
                    configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                    var defaultConfig = new Config { ConfigFilePath = configPath };
                    defaultConfig.Save();
                    return defaultConfig;
                }

                var json = File.ReadAllText(configPath);
                using var doc = JsonDocument.Parse(json);
                var appSettings = doc.RootElement.GetProperty("AppSettings");
                
                var languageStr = appSettings.TryGetProperty("Language", out var langProp) 
                    ? langProp.GetString() ?? "fr" : "fr";
                var logType = appSettings.TryGetProperty("LogType", out var logProp) 
                    ? logProp.GetString() ?? "json" : "json";

                return new Config 
                { 
                    Language = languageStr.ToLowerInvariant() switch 
                    { 
                        "en" or "english" => Language.English, 
                        _ => Language.French 
                    },
                    LogType = logType.ToLowerInvariant(),
                    ConfigFilePath = Path.GetFullPath(configPath)
                };
            }
            catch
            {
                var defaultPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                return new Config { ConfigFilePath = defaultPath };
            }
        }

        /// <summary>
        /// Saves current configuration to appsettings.json.
        /// </summary>
        public void Save()
        {
            var configPath = !string.IsNullOrEmpty(ConfigFilePath) 
                ? ConfigFilePath 
                : Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                
            var json = JsonSerializer.Serialize(new 
            { 
                AppSettings = new 
                { 
                    Language = Language.GetCode(),
                    LogType = LogType
                } 
            }, new JsonSerializerOptions { WriteIndented = true });
            
            File.WriteAllText(configPath, json);
            ConfigFilePath = configPath;
            _localization = null;
        }

        private static string? FindConfigFile()
        {
            var paths = new[] { 
                Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
                "appsettings.json"
            };
            return paths.FirstOrDefault(File.Exists);
        }
    }
}




