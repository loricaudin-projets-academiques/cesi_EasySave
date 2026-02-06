using System;

namespace EasySave.Core.Services
{
    /// <summary>
    /// Arguments pour l'événement de transfert réussi
    /// Permet à BackupWorkService de notifier les listeners (logger, stats, etc.)
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
    /// Arguments pour l'événement d'erreur de transfert
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
