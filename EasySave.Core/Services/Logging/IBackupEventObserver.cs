namespace EasySave.Core.Services.Logging
{
    /// <summary>
    /// Observer interface for backup events.
    /// Enables loose coupling between BackupWorkService and consumers.
    /// </summary>
    public interface IBackupEventObserver
    {
        /// <summary>Called when a backup starts.</summary>
        void OnBackupStarted(string backupName, long totalFiles, long totalSize);

        /// <summary>Called when a file is successfully transferred.</summary>
        void OnFileTransferred(string backupName, string sourceFile, string targetFile, 
                             long fileSize, double transferTimeMs);

        /// <summary>Called when a file transfer fails.</summary>
        void OnFileTransferError(string backupName, string sourceFile, string targetFile, 
                                long fileSize, Exception ex);

        /// <summary>Called to update progress.</summary>
        void OnProgressUpdated(string backupName, string currentFile, string targetFile,
                              long filesLeft, long sizeLeft, double progression);

        /// <summary>Called when a backup completes.</summary>
        void OnBackupCompleted(string backupName);

        /// <summary>Called when a backup fails.</summary>
        void OnBackupError(string backupName, Exception ex);

        /// <summary>Called when a backup is paused.</summary>
        void OnBackupPaused(string backupName);

        /// <summary>Called when a backup resumes.</summary>
        void OnBackupResumed(string backupName);
    }
}
