namespace EasyLog.Models
{
    /// <summary>
    /// Represents a daily log entry for file transfer actions during backups.
    /// </summary>
    public class LogEntry
    {
        /// <summary>Backup job name.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Source file full path (UNC format).</summary>
        public string FileSource { get; set; } = string.Empty;

        /// <summary>Target file full path (UNC format).</summary>
        public string FileTarget { get; set; } = string.Empty;

        /// <summary>File size in bytes.</summary>
        public long FileSize { get; set; }

        /// <summary>File transfer time in milliseconds. Negative if error.</summary>
        public double FileTransferTime { get; set; }

        /// <summary>Timestamp of the action (format: dd/MM/yyyy HH:mm:ss).</summary>
        public string Time { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

        /// <summary>Backup type (Full or Differential).</summary>
        public string BackupType { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Name} ({BackupType}) | {FileSource} -> {FileTarget} | {FileSize} bytes | {FileTransferTime}ms";
        }
    }
}
