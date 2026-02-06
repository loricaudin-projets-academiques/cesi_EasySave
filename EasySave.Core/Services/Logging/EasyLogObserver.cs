using EasySave.Core.Localization;

namespace EasySave.Core.Services.Logging
{
    /// <summary>
    /// Observer pour le temps réel (state.json)
    /// Les logs journaliers sont gérés directement par BackupWorkService
    /// ? Utilise le système de localization
    /// </summary>
    public class EasyLogObserver : IBackupEventObserver
    {
        private readonly ILocalizationService _localization;

        public EasyLogObserver(ILocalizationService? localization = null)
        {
            _localization = localization ?? new LocalizationService();
        }

        public void OnBackupStarted(string backupName, long totalFiles, long totalSize)
        {
            // Localisé via EasyLogger directement
        }

        public void OnProgressUpdated(string backupName, string currentFile, string targetFile, long filesLeft, long sizeLeft, double progression)
        {
            // Mis à jour en temps réel via EasyLogger
        }

        public void OnBackupCompleted(string backupName)
        {
            // Localisé via EasyLogger directement
        }

        public void OnBackupError(string backupName, Exception ex)
        {
            // Erreur gérée via BackupWorkService
        }

        public void OnBackupPaused(string backupName)
        {
            // Non utilisé pour V1
        }

        public void OnBackupResumed(string backupName)
        {
            // Non utilisé pour V1
        }

        // Non utilisé - logs journaliers gérés directement
        public void OnFileTransferred(string backupName, string sourceFile, string targetFile, long fileSize, double transferTimeMs) { }
        public void OnFileTransferError(string backupName, string sourceFile, string targetFile, long fileSize, Exception ex) { }
    }
}




