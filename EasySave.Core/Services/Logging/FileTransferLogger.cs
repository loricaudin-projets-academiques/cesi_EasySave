using EasyLog.Services;

namespace EasySave.Core.Services.Logging
{
    /// <summary>
    /// Écoute les événements de BackupWorkService et logue les journaliers
    /// ? RESPONSABILITÉ UNIQUE: Logger les transferts de fichiers
    /// ? COUPLAGE FAIBLE: Via événements, pas de paramètres polluant
    /// </summary>
    public class FileTransferLogger
    {
        private readonly EasyLogger _logger;

        public FileTransferLogger(EasyLogger? logger = null)
        {
            _logger = logger ?? new EasyLogger();
        }

        /// <summary>
        /// S'abonne aux événements du service de sauvegarde
        /// Appelé une seule fois au démarrage
        /// </summary>
        public void Subscribe(BackupWorkService backupService)
        {
            backupService.FileTransferred += OnFileTransferred;
            backupService.FileTransferError += OnFileTransferError;
        }

        /// <summary>
        /// Se désabonne des événements
        /// </summary>
        public void Unsubscribe(BackupWorkService backupService)
        {
            backupService.FileTransferred -= OnFileTransferred;
            backupService.FileTransferError -= OnFileTransferError;
        }

        // ============ EVENT HANDLERS ============

        private void OnFileTransferred(object? sender, FileTransferredEventArgs e)
        {
            try
            {
                _logger.LogFileTransfer(e.BackupName, e.SourceFile, e.TargetFile, e.FileSize, e.TransferTimeMs);
                Console.WriteLine($"? Journalier: {Path.GetFileName(e.SourceFile)} ({FormatSize(e.FileSize)}) - {e.TransferTimeMs:F2}ms");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Erreur lors du logging: {ex.Message}");
            }
        }

        private void OnFileTransferError(object? sender, FileTransferErrorEventArgs e)
        {
            try
            {
                _logger.LogFileTransferError(e.BackupName, e.SourceFile, e.TargetFile, e.FileSize);
                Console.WriteLine($"? Journalier ERROR: {Path.GetFileName(e.SourceFile)} - {e.Exception.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Erreur lors du logging: {ex.Message}");
            }
        }

        private static string FormatSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:F2} {sizes[order]}";
        }
    }
}
