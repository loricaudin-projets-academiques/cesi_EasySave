using EasySave.Core.Localization;

namespace EasySave.Core.Services.Logging
{
    /// <summary>
    /// Observer for real-time state updates.
    /// Daily logs are handled directly by BackupWorkService.
    /// </summary>
    public class EasyLogObserver : IBackupEventObserver
    {
        private readonly ILocalizationService _localization;

        public EasyLogObserver(ILocalizationService? localization = null)
        {
            _localization = localization ?? new LocalizationService();
        }

        public void OnBackupStarted(string backupName, long totalFiles, long totalSize) { }
        public void OnProgressUpdated(string backupName, string currentFile, string targetFile, long filesLeft, long sizeLeft, double progression) { }
        public void OnBackupCompleted(string backupName) { }
        public void OnBackupError(string backupName, Exception ex) { }
        public void OnBackupPaused(string backupName) { }
        public void OnBackupResumed(string backupName) { }
        public void OnFileTransferred(string backupName, string sourceFile, string targetFile, long fileSize, double transferTimeMs) { }
        public void OnFileTransferError(string backupName, string sourceFile, string targetFile, long fileSize, Exception ex) { }
    }
}




