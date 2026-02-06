using EasyLog.Models;

namespace EasySave.Core.Services.Logging
{
    /// <summary>
    /// Observer pour les événements de sauvegarde
    /// Permet à d'autres composants de réagir aux événements (logs, IHM, etc.)
    /// Pattern Observer: couplage faible entre BackupWorkService et EasyLog
    /// </summary>
    public interface IBackupEventObserver
    {
        /// <summary>
        /// Appelé au démarrage d'une sauvegarde
        /// </summary>
        void OnBackupStarted(string backupName, long totalFiles, long totalSize);

        /// <summary>
        /// Appelé pour chaque fichier transféré avec succès
        /// </summary>
        void OnFileTransferred(string backupName, string sourceFile, string targetFile, 
                             long fileSize, double transferTimeMs);

        /// <summary>
        /// Appelé en cas d'erreur de transfert
        /// </summary>
        void OnFileTransferError(string backupName, string sourceFile, string targetFile, 
                                long fileSize, Exception ex);

        /// <summary>
        /// Appelé pour mettre à jour la progression
        /// </summary>
        void OnProgressUpdated(string backupName, string currentFile, string targetFile,
                              long filesLeft, long sizeLeft, double progression);

        /// <summary>
        /// Appelé à la fin d'une sauvegarde
        /// </summary>
        void OnBackupCompleted(string backupName);

        /// <summary>
        /// Appelé en cas d'erreur globale
        /// </summary>
        void OnBackupError(string backupName, Exception ex);

        /// <summary>
        /// Appelé quand une sauvegarde est en pause
        /// </summary>
        void OnBackupPaused(string backupName);

        /// <summary>
        /// Appelé quand une sauvegarde reprend
        /// </summary>
        void OnBackupResumed(string backupName);
    }
}
