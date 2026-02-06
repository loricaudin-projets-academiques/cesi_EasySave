using System;
using EasyLog.Formatters;

namespace EasyLog.Services
{
    /// <summary>
    /// Usine (Factory) pour créer les formatters
    /// Pattern Factory: centralise la création des formatters
    /// Facile d'ajouter de nouveaux formats sans modifier le code existant
    /// </summary>
    public class FormatterFactory
    {
        private static readonly Dictionary<string, ILogFormatter> _formatters = new()
        {
            { "json", new JsonLogFormatter() },
            // Prêt pour XML, CSV, etc.
            // { "xml", new XmlLogFormatter() },
            // { "csv", new CsvLogFormatter() }
        };

        /// <summary>
        /// Obtient un formatter pour un format donné
        /// </summary>
        public static ILogFormatter GetFormatter(string format)
        {
            var normalizedFormat = format.ToLowerInvariant();
            
            if (_formatters.TryGetValue(normalizedFormat, out var formatter))
                return formatter;

            throw new NotSupportedException(
                $"Format '{format}' non supporté. Formats disponibles: {string.Join(", ", _formatters.Keys)}"
            );
        }

        /// <summary>
        /// Enregistre un nouveau formatter (extensibilité)
        /// </summary>
        public static void RegisterFormatter(string format, ILogFormatter formatter)
        {
            _formatters[format.ToLowerInvariant()] = formatter;
        }

        /// <summary>
        /// Retourne les formats disponibles
        /// </summary>
        public static IEnumerable<string> GetAvailableFormats() => _formatters.Keys;
    }
}
