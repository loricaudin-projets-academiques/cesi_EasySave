using System.Xml;
using System.Xml.Serialization;
using EasyLog.Models;

namespace EasyLog.Formatters
{
    /// <summary>
    /// XML implementation of the log formatter.
    /// Formats logs in XML format with proper indentation.
    /// </summary>
    public class XmlLogFormatter : ILogFormatter
    {
        private static readonly string RootLogEntry = "LogEntry";
        private static readonly string RootDailyLog = "DailyLog";
        private static readonly string RootStateEntry = "StateEntry";
        private static readonly string RootBackupStates = "BackupStates";
        private static readonly string IndentChars = "  ";

        public string Format => "xml";

        #region Log Entries

        public string FormatLogEntry(LogEntry entry)
        {
            return SerializeToXml(entry, RootLogEntry);
        }

        public string FormatLogEntries(List<LogEntry> entries)
        {
            return SerializeToXml(new LogEntryList { Entries = entries }, RootDailyLog);
        }

        public LogEntry? ParseLogEntry(string content)
        {
            try
            {
                return DeserializeFromXml<LogEntry>(content);
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
                if (string.IsNullOrWhiteSpace(content))
                    return new List<LogEntry>();

                var wrapper = DeserializeFromXml<LogEntryList>(content);
                return wrapper?.Entries ?? new List<LogEntry>();
            }
            catch
            {
                return new List<LogEntry>();
            }
        }

        #endregion

        #region State Entries

        public string FormatStateEntry(StateEntry entry)
        {
            return SerializeToXml(entry, RootStateEntry);
        }

        public string FormatStateEntries(List<StateEntry> entries)
        {
            return SerializeToXml(new StateEntryList { Entries = entries }, RootBackupStates);
        }

        public StateEntry? ParseStateEntry(string content)
        {
            try
            {
                return DeserializeFromXml<StateEntry>(content);
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
                if (string.IsNullOrWhiteSpace(content))
                    return new List<StateEntry>();

                var wrapper = DeserializeFromXml<StateEntryList>(content);
                return wrapper?.Entries ?? new List<StateEntry>();
            }
            catch
            {
                return new List<StateEntry>();
            }
        }

        #endregion

        #region Serialization Helpers

        private static string SerializeToXml<T>(T obj, string rootName)
        {
            var serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(rootName));
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = IndentChars,
                OmitXmlDeclaration = false
            };

            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, settings);
            
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);
            
            serializer.Serialize(xmlWriter, obj, namespaces);
            return stringWriter.ToString();
        }

        private static T? DeserializeFromXml<T>(string xml) where T : class
        {
            if (string.IsNullOrWhiteSpace(xml))
                return null;

            var serializer = new XmlSerializer(typeof(T));
            using var stringReader = new StringReader(xml);
            return serializer.Deserialize(stringReader) as T;
        }

        #endregion
    }

    #region Wrapper Classes for XML Serialization

    /// <summary>
    /// Wrapper class for serializing a list of log entries.
    /// </summary>
    [XmlRoot("DailyLog")]
    public class LogEntryList
    {
        [XmlElement("LogEntry")]
        public List<LogEntry> Entries { get; set; } = new();
    }

    /// <summary>
    /// Wrapper class for serializing a list of state entries.
    /// </summary>
    [XmlRoot("BackupStates")]
    public class StateEntryList
    {
        [XmlElement("StateEntry")]
        public List<StateEntry> Entries { get; set; } = new();
    }

    #endregion
}
