namespace EasyLog.Configuration
{
    /// <summary>
    /// Centralized configuration for EasyLog.
    /// Defines paths, formats and behaviors.
    /// </summary>
    public class LogConfiguration
    {
        /// <summary>Base directory for logs.</summary>
        public string LogDirectory { get; set; } = GetDefaultLogDirectory();

        /// <summary>Subfolder name for daily logs.</summary>
        public string DailyLogsSubfolder { get; set; } = "daily_logs";

        /// <summary>State file name (without extension).</summary>
        public string StateFileName { get; set; } = "state";

        /// <summary>Log format (json, xml, csv, etc.).</summary>
        public string LogFormat { get; set; } = "json";

        /// <summary>Maximum daily log file size in MB.</summary>
        public int MaxDailyLogSizeMB { get; set; } = 100;

        /// <summary>Auto-create directories if they don't exist.</summary>
        public bool AutoCreateDirectories { get; set; } = true;

        /// <summary>Gets the full path to the daily logs directory.</summary>
        public string GetDailyLogsPath() => Path.Combine(LogDirectory, DailyLogsSubfolder);

        /// <summary>Gets the full path to the state file.</summary>
        public string GetStateFilePath()
        {
            var extension = LogFormat.ToLowerInvariant();
            return Path.Combine(LogDirectory, $"{StateFileName}.{extension}");
        }

        /// <summary>
        /// Gets the full path to the daily log file for a specific date.
        /// </summary>
        /// <param name="date">Date for the log file.</param>
        /// <returns>Full path to the log file.</returns>
        public string GetDailyLogPath(DateTime date)
        {
            var filename = date.ToString("yyyy-MM-dd");
            var extension = LogFormat.ToLowerInvariant();
            return Path.Combine(GetDailyLogsPath(), $"{filename}.{extension}");
        }

        private static string GetDefaultLogDirectory()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ProSoft", "EasySave", "logs"
                );
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                return "/app/logs";
            }
            else
            {
                return Path.Combine(".", "logs");
            }
        }
    }
}
