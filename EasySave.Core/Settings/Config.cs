using EasySave.Core.Localization;
using System.Text.Json;

namespace EasySave.Core.Settings
{
    /// <summary>
    /// Application configuration loaded from appsettings.json.
    /// Contains ONLY data/settings - services are injected via DI.
    /// </summary>
    public class Config
    {
        /// <summary>Current language setting.</summary>
        public Language Language { get; set; } = Language.French;
        
        /// <summary>Log format type (json or xml).</summary>
        public string LogType { get; set; } = "json";

        /// <summary>
        /// File extensions to encrypt (comma-separated, e.g., ".txt,.docx,.pdf").
        /// Empty string means no encryption.
        /// </summary>
        public string EncryptExtensions { get; set; } = string.Empty;

        /// <summary>
        /// Path to CryptoSoft executable.
        /// </summary>
        public string CryptoSoftPath { get; set; } = "CryptoSoft.exe";

        /// <summary>
        /// Business software process name that blocks backups when running.
        /// Empty string means no blocking.
        /// </summary>
        public string BusinessSoftware { get; set; } = string.Empty;

        /// <summary>
        /// Large file threshold in KB. Files larger than this cannot be transferred
        /// in parallel (only one at a time). 0 = disabled (no restriction).
        /// </summary>
        public long LargeFileThresholdKB { get; set; } = 0;

        /// <summary>
        /// Priority file extensions (comma-separated, e.g., ".docx,.pdf").
        /// Files with these extensions are copied first; non-priority files are blocked
        /// until all priority files across all jobs are done.
        /// Empty string means no priority ordering.
        /// </summary>
        public string PriorityExtensions { get; set; } = string.Empty;
        
        /// <summary>Path to the loaded config file.</summary>
        public string ConfigFilePath { get; private set; } = string.Empty;


        /// <summary></summary>
        public string LogServerUrl { get; set; } = "127.0.0.1";

        /// <summary></summary>
        public int LogServerPort { get; set; } = 5000;

        /// <summary></summary>
        public bool LogOnServer { get; set; } = false;

        /// <summary></summary>
        public bool LogInLocal { get; set; } = true;

        #region Encryption Extensions Management

        /// <summary>
        /// Gets the list of extensions to encrypt.
        /// </summary>
        public string[] GetEncryptExtensionsList()
        {
            if (string.IsNullOrWhiteSpace(EncryptExtensions))
                return Array.Empty<string>();


            return EncryptExtensions
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(ext => ext.StartsWith('.') ? ext.ToLowerInvariant() : $".{ext.ToLowerInvariant()}")
                .ToArray();
        }

        /// <summary>
        /// Checks if a file should be encrypted based on its extension.
        /// </summary>
        public bool ShouldEncrypt(string filePath)
        {
            if (string.IsNullOrWhiteSpace(EncryptExtensions))
                return false;

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return GetEncryptExtensionsList().Contains(extension);
        }

        /// <summary>
        /// Adds an extension to the encryption list.
        /// </summary>
        /// <param name="extension">Extension to add (with or without dot).</param>
        /// <returns>True if added, false if already exists.</returns>
        public bool AddEncryptExtension(string extension)
        {
            var ext = extension.StartsWith('.') ? extension.ToLowerInvariant() : $".{extension.ToLowerInvariant()}";
            var currentList = GetEncryptExtensionsList().ToList();

            if (currentList.Contains(ext))
                return false;

            currentList.Add(ext);
            EncryptExtensions = string.Join(",", currentList);
            return true;
        }

        /// <summary>
        /// Removes an extension from the encryption list.
        /// </summary>
        /// <param name="extension">Extension to remove (with or without dot).</param>
        /// <returns>True if removed, false if not found.</returns>
        public bool RemoveEncryptExtension(string extension)
        {
            var ext = extension.StartsWith('.') ? extension.ToLowerInvariant() : $".{extension.ToLowerInvariant()}";
            var currentList = GetEncryptExtensionsList().ToList();

            if (!currentList.Contains(ext))
                return false;

            currentList.Remove(ext);
            EncryptExtensions = string.Join(",", currentList);
            return true;
        }

        /// <summary>
        /// Clears all encryption extensions.
        /// </summary>
        public void ClearEncryptExtensions()
        {
            EncryptExtensions = string.Empty;
        }

        /// <summary>
        /// Common file extensions that might need encryption (for GUI suggestions).
        /// </summary>
        public static readonly string[] CommonEncryptExtensions = new[]
        {
            ".txt", ".doc", ".docx", ".pdf", ".xls", ".xlsx",
            ".ppt", ".pptx", ".csv", ".xml", ".json", ".zip",
            ".rar", ".7z", ".sql", ".db", ".mdb", ".accdb"
        };

        #endregion

        #region Priority Extensions Management

        /// <summary>
        /// Gets the list of priority extensions.
        /// </summary>
        public string[] GetPriorityExtensionsList()
        {
            if (string.IsNullOrWhiteSpace(PriorityExtensions))
                return Array.Empty<string>();

            return PriorityExtensions
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(ext => ext.StartsWith('.') ? ext.ToLowerInvariant() : $".{ext.ToLowerInvariant()}")
                .ToArray();
        }

        /// <summary>
        /// Checks if a file has a priority extension.
        /// </summary>
        public bool IsPriorityFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(PriorityExtensions))
                return false;

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return GetPriorityExtensionsList().Contains(extension);
        }

        /// <summary>
        /// Adds an extension to the priority list.
        /// </summary>
        public bool AddPriorityExtension(string extension)
        {
            var ext = extension.StartsWith('.') ? extension.ToLowerInvariant() : $".{extension.ToLowerInvariant()}";
            var currentList = GetPriorityExtensionsList().ToList();

            if (currentList.Contains(ext))
                return false;

            currentList.Add(ext);
            PriorityExtensions = string.Join(",", currentList);
            return true;
        }

        /// <summary>
        /// Removes an extension from the priority list.
        /// </summary>
        public bool RemovePriorityExtension(string extension)
        {
            var ext = extension.StartsWith('.') ? extension.ToLowerInvariant() : $".{extension.ToLowerInvariant()}";
            var currentList = GetPriorityExtensionsList().ToList();

            if (!currentList.Contains(ext))
                return false;

            currentList.Remove(ext);
            PriorityExtensions = string.Join(",", currentList);
            return true;
        }

        /// <summary>
        /// Clears all priority extensions.
        /// </summary>
        public void ClearPriorityExtensions()
        {
            PriorityExtensions = string.Empty;
        }

        #endregion

        #region Business Software Management

        /// <summary>
        /// Sets the business software process name.
        /// </summary>
        /// <param name="processName">Process name (with or without .exe).</param>
        public void SetBusinessSoftware(string processName)
        {
            BusinessSoftware = processName.Replace(".exe", "", StringComparison.OrdinalIgnoreCase).Trim();
        }

        /// <summary>
        /// Clears the business software (disables blocking).
        /// </summary>
        public void ClearBusinessSoftware()
        {
            BusinessSoftware = string.Empty;
        }

        /// <summary>
        /// Checks if business software blocking is configured.
        /// </summary>
        public bool HasBusinessSoftware()
        {
            return !string.IsNullOrWhiteSpace(BusinessSoftware);
        }

        /// <summary>
        /// Common business software names (for GUI suggestions).
        /// </summary>
        public static readonly string[] CommonBusinessSoftware = new[]
        {
            "calc", "notepad", "excel", "winword", "outlook",
            "chrome", "firefox", "msedge", "teams", "slack"
        };

        /// <summary>
        /// Gets a list of currently running processes with a visible window.
        /// Useful for GUI to let user select from running applications.
        /// </summary>
        public static string[] GetRunningProcesses()
        {
            try
            {
                return System.Diagnostics.Process.GetProcesses()
                    .Where(p => p.MainWindowHandle != IntPtr.Zero)
                    .Select(p => p.ProcessName)
                    .Distinct()
                    .OrderBy(n => n)
                    .ToArray();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        #endregion


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
                var encryptExtensions = appSettings.TryGetProperty("EncryptExtensions", out var encryptProp) 
                    ? encryptProp.GetString() ?? "" : "";
                var cryptoSoftPath = appSettings.TryGetProperty("CryptoSoftPath", out var cryptoProp) 
                    ? cryptoProp.GetString() ?? "CryptoSoft.exe" : "CryptoSoft.exe";
                var businessSoftware = appSettings.TryGetProperty("BusinessSoftware", out var businessProp) 
                    ? businessProp.GetString() ?? "" : "";
                var largeFileThreshold = appSettings.TryGetProperty("LargeFileThresholdKB", out var lfProp) 
                    ? lfProp.GetInt64() : 0;
                var logServerUrl = appSettings.TryGetProperty("LogServerUrl", out var logServerUrlProp)
                    ? logServerUrlProp.GetString() ?? "127.0.0.1" : "127.0.0.1";
                var logServerPort = appSettings.TryGetProperty("LogServerPort", out var logServerPortProp)
                    ? logServerPortProp.GetInt32() : 5000;
                var logOnServer = appSettings.TryGetProperty("LogOnServer", out var logOnServerProp)
                    ? logOnServerProp.GetBoolean() : false;
                var logInLocal = appSettings.TryGetProperty("LogInLocal", out var logInLocalProp)
                    ? logInLocalProp.GetBoolean() : false;
                var priorityExtensions = appSettings.TryGetProperty("PriorityExtensions", out var prioProp) 
                    ? prioProp.GetString() ?? "" : "";

                return new Config 
                { 
                    Language = languageStr.ToLowerInvariant() switch 
                    { 
                        "en" or "english" => Language.English, 
                        _ => Language.French 
                    },
                    EncryptExtensions = encryptExtensions,
                    CryptoSoftPath = cryptoSoftPath,
                    BusinessSoftware = businessSoftware,
                    LargeFileThresholdKB = largeFileThreshold,
                    PriorityExtensions = priorityExtensions,
                    LogType = logType.ToLowerInvariant(),
                    ConfigFilePath = Path.GetFullPath(configPath),
                    LogServerUrl = logServerUrl,
                    LogServerPort = logServerPort,
                    LogOnServer = logOnServer,
                    LogInLocal = logInLocal,
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
                    LogType = LogType,
                    EncryptExtensions = EncryptExtensions,
                    CryptoSoftPath = CryptoSoftPath,
                    BusinessSoftware = BusinessSoftware,
                    LargeFileThresholdKB = LargeFileThresholdKB,
                    LogServerUrl = LogServerUrl,
                    LogServerPort = LogServerPort,
                    LogOnServer = LogOnServer,
                    LogInLocal = LogInLocal,
                    PriorityExtensions = PriorityExtensions
                } 
            }, new JsonSerializerOptions { WriteIndented = true });
            
            File.WriteAllText(configPath, json);
            ConfigFilePath = configPath;
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
