using System;

namespace EasyLog.Configuration
{
    /// <summary>
    /// Configuration centralisée pour EasyLog
    /// Définit les chemins, formats et comportements
    /// </summary>
    public class LogConfiguration
    {
        /// <summary>
        /// Répertoire de base pour les logs
        /// Exemple: \\serveur\logs\ ou /var/logs/ ou ./logs/
        /// </summary>
        public string LogDirectory { get; set; } = GetDefaultLogDirectory();

        /// <summary>
        /// Nom du répertoire pour les logs journaliers
        /// Crée un sous-dossier: {LogDirectory}/daily_logs/{yyyy-MM-dd}.json
        /// </summary>
        public string DailyLogsSubfolder { get; set; } = "daily_logs";

        /// <summary>
        /// Nom du fichier d'état en temps réel
        /// Chemin complet: {LogDirectory}/state.json
        /// </summary>
        public string StateFileName { get; set; } = "state";

        /// <summary>
        /// Format des logs (json, xml, csv, etc.)
        /// </summary>
        public string LogFormat { get; set; } = "json";

        /// <summary>
        /// Taille maximale d'un fichier log journalier (en MB)
        /// Si dépassé, crée un nouveau fichier avec suffix
        /// </summary>
        public int MaxDailyLogSizeMB { get; set; } = 100;

        /// <summary>
        /// Crée automatiquement les répertoires s'ils n'existent pas
        /// </summary>
        public bool AutoCreateDirectories { get; set; } = true;

        /// <summary>
        /// Chemin complet du répertoire des logs journaliers
        /// </summary>
        public string GetDailyLogsPath()
        {
            return Path.Combine(LogDirectory, DailyLogsSubfolder);
        }

        /// <summary>
        /// Chemin complet du fichier d'état
        /// </summary>
        public string GetStateFilePath()
        {
            var extension = LogFormat.ToLowerInvariant();
            return Path.Combine(LogDirectory, $"{StateFileName}.{extension}");
        }

        /// <summary>
        /// Chemin complet du fichier log journalier pour une date
        /// </summary>
        public string GetDailyLogPath(DateTime date)
        {
            var filename = date.ToString("yyyy-MM-dd");
            var extension = LogFormat.ToLowerInvariant();
            return Path.Combine(GetDailyLogsPath(), $"{filename}.{extension}");
        }

        private static string GetDefaultLogDirectory()
        {
            // Utiliser AppData\Roaming sur Windows, /var/log sur Linux
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ProSoft", "EasySave", "logs"
                );
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                return "/var/log/easysave";
            }
            else
            {
                return Path.Combine(".", "logs");
            }
        }


    }
}
