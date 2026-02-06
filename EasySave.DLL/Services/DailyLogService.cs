using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using EasyLog.Configuration;
using EasyLog.Formatters;
using EasyLog.Models;

namespace EasyLog.Services
{
    /// <summary>
    /// Service de gestion des logs journaliers
    /// Écrit en temps réel les actions de sauvegarde
    /// Un fichier par jour (yyyy-MM-dd.json)
    /// </summary>
    public class DailyLogService
    {
        private readonly LogConfiguration _config;
        private readonly ILogFormatter _formatter;
        private readonly object _lockObject = new();

        public DailyLogService(LogConfiguration config)
        {
            _config = config;
            _formatter = FormatterFactory.GetFormatter(config.LogFormat);
            EnsureDirectoriesExist();
        }

        /// <summary>
        /// Ajoute une entrée au log journalier
        /// Crée le fichier s'il n'existe pas, sinon ajoute à la fin
        /// </summary>
        public void AddLogEntry(LogEntry entry)
        {
            lock (_lockObject)
            {
                try
                {
                    var logPath = _config.GetDailyLogPath(DateTime.Now);
                    var entries = LoadExistingEntries(logPath);
                    
                    // Ajouter la nouvelle entrée
                    entries.Add(entry);
                    
                    // Sauvegarder
                    SaveEntries(logPath, entries);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Ajoute plusieurs entrées au log journalier
        /// </summary>
        public void AddLogEntries(List<LogEntry> entries)
        {
            foreach (var entry in entries)
            {
                AddLogEntry(entry);
            }
        }

        /// <summary>
        /// Récupère tous les logs d'une journée
        /// </summary>
        public List<LogEntry> GetDayLogs(DateTime date)
        {
            lock (_lockObject)
            {
                try
                {
                    var logPath = _config.GetDailyLogPath(date);
                    return LoadExistingEntries(logPath);
                }
                catch
                {
                    return new List<LogEntry>();
                }
            }
        }

        /// <summary>
        /// Récupère les logs d'une plage de dates
        /// </summary>
        public List<LogEntry> GetLogs(DateTime startDate, DateTime endDate)
        {
            var allLogs = new List<LogEntry>();
            var current = startDate;

            while (current <= endDate)
            {
                allLogs.AddRange(GetDayLogs(current));
                current = current.AddDays(1);
            }

            return allLogs;
        }

        /// <summary>
        /// Efface les logs d'une journée
        /// </summary>
        public void ClearDayLogs(DateTime date)
        {
            lock (_lockObject)
            {
                try
                {
                    var logPath = _config.GetDailyLogPath(date);
                    if (File.Exists(logPath))
                        File.Delete(logPath);
                }
                catch
                {
                }
            }
        }

        // ============ MÉTHODES PRIVÉES ============

        private List<LogEntry> LoadExistingEntries(string logPath)
        {
            if (!File.Exists(logPath))
                return new List<LogEntry>();

            try
            {
                var content = File.ReadAllText(logPath);
                return _formatter.ParseLogEntries(content);
            }
            catch
            {
                return new List<LogEntry>();
            }
        }

        private void SaveEntries(string logPath, List<LogEntry> entries)
        {
            try
            {
                // S'assurer que le répertoire parent existe
                var directory = Path.GetDirectoryName(logPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                }
                
                var content = _formatter.FormatLogEntries(entries);
                File.WriteAllText(logPath, content);
            }
            catch
            {
            }
        }



        private void EnsureDirectoriesExist()
        {
            if (_config.AutoCreateDirectories)
            {
                try
                {
                    Directory.CreateDirectory(_config.LogDirectory);
                }
                catch
                {
                }
                
                try
                {
                    Directory.CreateDirectory(_config.GetDailyLogsPath());
                }
                catch
                {
                }
            }
        }

    }
}
