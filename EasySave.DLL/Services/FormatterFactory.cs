using EasyLog.Formatters;

namespace EasyLog.Services
{
    /// <summary>
    /// Factory for creating log formatters.
    /// Pattern Factory: centralizes formatter creation.
    /// </summary>
    public static class FormatterFactory
    {
        private static readonly Dictionary<string, ILogFormatter> _formatters = new()
        {
            { "json", new JsonLogFormatter() },
            { "xml", new XmlLogFormatter() }
        };

        /// <summary>
        /// Gets a formatter for the specified format.
        /// </summary>
        /// <param name="format">Format name (json, xml).</param>
        /// <returns>The log formatter instance.</returns>
        public static ILogFormatter GetFormatter(string format)
        {
            var normalizedFormat = format.ToLowerInvariant();
            
            if (_formatters.TryGetValue(normalizedFormat, out var formatter))
                return formatter;

            throw new NotSupportedException(
                $"Format '{format}' not supported. Available: {string.Join(", ", _formatters.Keys)}"
            );
        }

        /// <summary>
        /// Registers a new formatter.
        /// </summary>
        public static void RegisterFormatter(string format, ILogFormatter formatter)
        {
            _formatters[format.ToLowerInvariant()] = formatter;
        }

        /// <summary>
        /// Gets all available format names.
        /// </summary>
        public static IEnumerable<string> GetAvailableFormats() => _formatters.Keys;
    }
}
