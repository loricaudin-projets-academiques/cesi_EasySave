namespace EasySave.Core.Services
{
    /// <summary>
    /// Event arguments for successful file transfers.
    /// </summary>
    public class FileTransferredEventArgs : EventArgs
    {
        public string BackupName { get; set; } = string.Empty;
        public string SourceFile { get; set; } = string.Empty;
        public string TargetFile { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public double TransferTimeMs { get; set; }
    }

    /// <summary>
    /// Event arguments for file transfer errors.
    /// </summary>
    public class FileTransferErrorEventArgs : EventArgs
    {
        public string BackupName { get; set; } = string.Empty;
        public string SourceFile { get; set; } = string.Empty;
        public string TargetFile { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public Exception Exception { get; set; } = null!;
    }
}
