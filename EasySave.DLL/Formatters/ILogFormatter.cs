using System;
using System.Collections.Generic;
using EasyLog.Models;

namespace EasyLog.Formatters
{
    /// <summary>
    /// Interface stratégie pour formater les logs
    /// Permet de supporter JSON, XML, CSV, etc. sans modifier le core
    /// Pattern Strategy: changer facilement de format à l'exécution
    /// </summary>
    public interface ILogFormatter
    {
        /// <summary>
        /// Format supporté (json, xml, csv, etc.)
        /// </summary>
        string Format { get; }

        /// <summary>
        /// Formate une entrée de log en string
        /// </summary>
        string FormatLogEntry(LogEntry entry);

        /// <summary>
        /// Formate une liste d'entrées de logs
        /// </summary>
        string FormatLogEntries(List<LogEntry> entries);

        /// <summary>
        /// Formate une entrée d'état en string
        /// </summary>
        string FormatStateEntry(StateEntry entry);

        /// <summary>
        /// Formate une liste d'entrées d'état
        /// </summary>
        string FormatStateEntries(List<StateEntry> entries);

        /// <summary>
        /// Parse une string en LogEntry
        /// </summary>
        LogEntry? ParseLogEntry(string content);

        /// <summary>
        /// Parse une string en liste de LogEntry
        /// </summary>
        List<LogEntry> ParseLogEntries(string content);

        /// <summary>
        /// Parse une string en StateEntry
        /// </summary>
        StateEntry? ParseStateEntry(string content);

        /// <summary>
        /// Parse une string en liste de StateEntry
        /// </summary>
        List<StateEntry> ParseStateEntries(string content);
    }
}
