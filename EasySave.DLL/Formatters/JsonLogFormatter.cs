using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using EasyLog.Models;

namespace EasyLog.Formatters
{
    /// <summary>
    /// Implémentation du formatter JSON
    /// Formate les logs au format JSON avec indentation
    /// </summary>
    public class JsonLogFormatter : ILogFormatter
    {
        public string Format => "json";

        private readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public string FormatLogEntry(LogEntry entry)
        {
            return JsonSerializer.Serialize(entry, _options);
        }

        public string FormatLogEntries(List<LogEntry> entries)
        {
            // Format avec retours à la ligne entre les objets pour lisibilité
            var json = JsonSerializer.Serialize(entries, _options);
            return json;
        }

        public string FormatStateEntry(StateEntry entry)
        {
            return JsonSerializer.Serialize(entry, _options);
        }

        public string FormatStateEntries(List<StateEntry> entries)
        {
            var json = JsonSerializer.Serialize(entries, _options);
            return json;
        }

        public LogEntry? ParseLogEntry(string content)
        {
            try
            {
                return JsonSerializer.Deserialize<LogEntry>(content, _options);
            }
            catch
            {
                return null;
            }
        }

        public List<LogEntry> ParseLogEntries(string content)
        {
            try
            {
                return JsonSerializer.Deserialize<List<LogEntry>>(content, _options) ?? new List<LogEntry>();
            }
            catch
            {
                return new List<LogEntry>();
            }
        }

        public StateEntry? ParseStateEntry(string content)
        {
            try
            {
                return JsonSerializer.Deserialize<StateEntry>(content, _options);
            }
            catch
            {
                return null;
            }
        }

        public List<StateEntry> ParseStateEntries(string content)
        {
            try
            {
                return JsonSerializer.Deserialize<List<StateEntry>>(content, _options) ?? new List<StateEntry>();
            }
            catch
            {
                return new List<StateEntry>();
            }
        }
    }
}
