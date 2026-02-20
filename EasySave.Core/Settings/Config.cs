using EasySave.Core.Localization;
using System.Text.Json;

namespace EasySave.Core.Settings
{
    public class Config
    {
        public Language Language { get; set; } = Language.French;
        public string LogType { get; set; } = "json";
        public string EncryptExtensions { get; set; } = string.Empty;
        public string CryptoSoftPath { get; set; } = "CryptoSoft.exe";
        public string BusinessSoftware { get; set; } = string.Empty;

        // ← NOUVEAU : max file size en Ko
        public int MaxFileSizeKo { get; set; } = 1024;

        public string ConfigFilePath { get; private set; } = string.Empty;

        #region Encrypt Extensions Management
        public string[] GetEncryptExtensionsList()
        {
            if (string.IsNullOrWhiteSpace(EncryptExtensions))
                return Array.Empty<string>();
            return EncryptExtensions
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(ext => ext.StartsWith('.') ? ext.ToLowerInvariant() : $".{ext.ToLowerInvariant()}")
                .ToArray();
        }

        public bool ShouldEncrypt(string filePath)
        {
            if (string.IsNullOrWhiteSpace(EncryptExtensions))
                return false;
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return GetEncryptExtensionsList().Contains(extension);
        }

        public bool AddEncryptExtension(string extension)
        {
            var ext = extension.StartsWith('.') ? extension.ToLowerInvariant() : $".{extension.ToLowerInvariant()}";
            var list = GetEncryptExtensionsList().ToList();
            if (list.Contains(ext)) return false;
            list.Add(ext);
            EncryptExtensions = string.Join(",", list);
            return true;
        }

        public bool RemoveEncryptExtension(string extension)
        {
            var ext = extension.StartsWith('.') ? extension.ToLowerInvariant() : $".{extension.ToLowerInvariant()}";
            var list = GetEncryptExtensionsList().ToList();
            if (!list.Contains(ext)) return false;
            list.Remove(ext);
            EncryptExtensions = string.Join(",", list);
            return true;
        }

        public void ClearEncryptExtensions() => EncryptExtensions = string.Empty;
        #endregion

        #region Business Software
        public void SetBusinessSoftware(string processName) =>
            BusinessSoftware = processName.Replace(".exe", "", StringComparison.OrdinalIgnoreCase).Trim();

        public void ClearBusinessSoftware() => BusinessSoftware = string.Empty;
        public bool HasBusinessSoftware() => !string.IsNullOrWhiteSpace(BusinessSoftware);

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

        public static Config Load()
        {
            try
            {
                var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                if (!File.Exists(configPath))
                {
                    var defaultConfig = new Config { ConfigFilePath = configPath };
                    defaultConfig.Save();
                    return defaultConfig;
                }

                var json = File.ReadAllText(configPath);
                using var doc = JsonDocument.Parse(json);
                var appSettings = doc.RootElement.GetProperty("AppSettings");

                var languageStr = appSettings.TryGetProperty("Language", out var langProp) ? langProp.GetString() ?? "fr" : "fr";
                var logType = appSettings.TryGetProperty("LogType", out var logProp) ? logProp.GetString() ?? "json" : "json";
                var encryptExtensions = appSettings.TryGetProperty("EncryptExtensions", out var encryptProp) ? encryptProp.GetString() ?? "" : "";
                var cryptoSoftPath = appSettings.TryGetProperty("CryptoSoftPath", out var cryptoProp) ? cryptoProp.GetString() ?? "CryptoSoft.exe" : "CryptoSoft.exe";
                var businessSoftware = appSettings.TryGetProperty("BusinessSoftware", out var businessProp) ? businessProp.GetString() ?? "" : "";
                var maxFileSize = appSettings.TryGetProperty("MaxFileSizeKo", out var maxProp) ? maxProp.GetInt32() : 1024;

                return new Config
                {
                    Language = languageStr.ToLowerInvariant() switch { "en" or "english" => Language.English, _ => Language.French },
                    EncryptExtensions = encryptExtensions,
                    CryptoSoftPath = cryptoSoftPath,
                    BusinessSoftware = businessSoftware,
                    LogType = logType.ToLowerInvariant(),
                    MaxFileSizeKo = maxFileSize,
                    ConfigFilePath = Path.GetFullPath(configPath)
                };
            }
            catch
            {
                return new Config { ConfigFilePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json") };
            }
        }

        public void Save()
        {
            var configPath = !string.IsNullOrEmpty(ConfigFilePath) ? ConfigFilePath : Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            var json = JsonSerializer.Serialize(new
            {
                AppSettings = new
                {
                    Language = Language.GetCode(),
                    LogType = LogType,
                    EncryptExtensions = EncryptExtensions,
                    CryptoSoftPath = CryptoSoftPath,
                    BusinessSoftware = BusinessSoftware,
                    MaxFileSizeKo = MaxFileSizeKo
                }
            }, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, json);
            ConfigFilePath = configPath;
        }
    }
}