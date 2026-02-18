using EasyLog.Services;

namespace EasySave.Core.Services.Logging
{
    /// <summary>
    /// Listens to BackupWorkService events and logs file transfers.
    /// </summary>
    public class FileTransferLogger
    {
        private readonly EasyLogServerLogger _logger;

        public FileTransferLogger(EasyLogServerLogger? logger = null)
        {
            _logger = logger ?? new EasyLogServerLogger();
        }

        /// <summary>
        /// Subscribes to backup service events.
        /// </summary>
        /// <param name="backupService">The backup service to subscribe to.</param>
        public void Subscribe(BackupWorkService backupService)
        {
            backupService.FileTransferred += OnFileTransferred;
            backupService.FileTransferError += OnFileTransferError;
        }

        /// <summary>
        /// Unsubscribes from backup service events.
        /// </summary>
        /// <param name="backupService">The backup service to unsubscribe from.</param>
        public void Unsubscribe(BackupWorkService backupService)
        {
            backupService.FileTransferred -= OnFileTransferred;
            backupService.FileTransferError -= OnFileTransferError;
        }

        private void OnFileTransferred(object? sender, FileTransferredEventArgs e)
        {
            try
            {
                _logger.LogFileTransfer(e.BackupName, e.SourceFile, e.TargetFile, e.FileSize, e.TransferTimeMs);
            }
            catch
            {
            }
        }

        private void OnFileTransferError(object? sender, FileTransferErrorEventArgs e)
        {
            try
            {
                _logger.LogFileTransferError(e.BackupName, e.SourceFile, e.TargetFile, e.FileSize);
            }
            catch
            {
            }
        }
    }
}
