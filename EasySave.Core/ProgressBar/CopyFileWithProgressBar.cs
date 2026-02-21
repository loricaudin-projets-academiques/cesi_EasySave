using EasySave.Core.Models;
using System.Diagnostics;

namespace EasySave.Core.ProgressBar
{
    /// <summary>
    /// Handles file copying with progress bar display and event notifications.
    /// Pure utility - no service dependencies. Encryption is handled by the service layer.
    /// </summary>
    internal class CopyFileWithProgressBar : ProgressBar
    {
        private long totalBytesLocal;      // Taille des fichiers du dossier courant
        private long copiedBytesLocal;     // Bytes copiés dans le dossier courant

        private long totalBytesGlobal;     // Taille totale de tous les fichiers du backup
        private long copiedBytesGlobal;    // Bytes copiés depuis le début du backup

        private readonly BackupState State;

        /// <summary>Event raised when file copy progress updates.</summary>
        public event EventHandler<FileProgressEventArgs>? FileProgress;

        /// <summary>Event raised when a file is successfully transferred.</summary>
        public event EventHandler<FileCopiedEventArgs>? FileTransferred;

        /// <summary>Event raised when a file transfer fails.</summary>
        public event EventHandler<FileCopyErrorEventArgs>? FileTransferError;

        /// <summary>Called between chunks to check if the backup should pause. Returns true if paused.</summary>
        public Func<bool>? PauseChecker { get; set; }

        /// <summary>Token checked between chunks to support immediate stop.</summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>Event raised when the backup is paused due to business software.</summary>
        public event Action? Paused;

        /// <summary>Event raised when the backup resumes after business software closes.</summary>
        public event Action? Resumed;

        public CopyFileWithProgressBar(BackupState backupState)
        {
            this.State = backupState;
        }

        /// <summary>
        /// Sets the total number of bytes for the entire backup (global progress).
        /// </summary>
        public void SetGlobalTotalBytes(long total)
        {
            this.totalBytesGlobal = total;
            this.copiedBytesGlobal = 0;
        }

        /// <summary>
        /// Copies multiple files from source to destination with progress tracking.
        /// </summary>
        /// <param name="source">Source directory path.</param>
        /// <param name="dest">Destination directory path.</param>
        /// <param name="files">Array of file paths to copy.</param>
        public void CopyFiles(string source, string dest, string[] files)
        {
            // Progression locale (dossier courant)
            this.totalBytesLocal = files.Sum(f => new FileInfo(f).Length);
            this.copiedBytesLocal = 0;

            this.State.SetLastActionTimestamp(DateTime.UtcNow);
            this.State.SetTotalFiles(files.Length);
            this.State.SetFileSize(this.totalBytesLocal);

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(dest, fileName);

                if (File.Exists(destFile))
                    File.Delete(destFile);

                this.CopyFile(file, destFile);
            }
        }

        private void CopyFile(string source, string dest)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                this.CopyWithProgress(source, dest, bytesCopiedInFile =>
                {
                    // Mise à jour locale
                    this.copiedBytesLocal += bytesCopiedInFile;

                    // Mise à jour globale
                    this.copiedBytesGlobal += bytesCopiedInFile;

                    // Progression globale
                    double progress = (double)this.copiedBytesGlobal / this.totalBytesGlobal;

                    this.SetProgressBar(progress);
                    this.State.SetProgress(progress * 100);

                    OnFileProgress(new FileProgressEventArgs
                    {
                        SourceFile = source,
                        DestFile = dest,
                        CurrentProgress = progress * 100
                    });
                });

                stopwatch.Stop();

                OnFileTransferred(new FileCopiedEventArgs
                {
                    SourceFile = source,
                    DestFile = dest,
                    FileSize = new FileInfo(source).Length,
                    TransferTimeMs = stopwatch.Elapsed.TotalMilliseconds
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                OnFileTransferError(new FileCopyErrorEventArgs
                {
                    SourceFile = source,
                    DestFile = dest,
                    FileSize = new FileInfo(source).Length,
                    Exception = ex
                });

                throw;
            }
        }

        private void CopyWithProgress(string source, string dest, Action<long> onChunkCopied)
        {
            const int bufferSize = 1024 * 1024; // 1 MB buffer
            byte[] buffer = new byte[bufferSize];

            using var input = new FileStream(source, FileMode.Open, FileAccess.Read);
            using var output = new FileStream(dest, FileMode.Create, FileAccess.Write);

            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                // Check for cancellation (Stop)
                CancellationToken.ThrowIfCancellationRequested();

                // Check for pause between chunks
                if (PauseChecker != null && PauseChecker())
                {
                    Paused?.Invoke();
                    while (PauseChecker() && !CancellationToken.IsCancellationRequested)
                        Thread.Sleep(250);
                    CancellationToken.ThrowIfCancellationRequested();
                    Resumed?.Invoke();
                }

                try
                {
                    output.Write(buffer, 0, read);
                }
                catch (IOException)
                {
                    throw new Exception("An error occurred during backup. Check if folders are accessible and there is enough disk space.");
                }

                onChunkCopied?.Invoke(read);
            }
        }

        protected virtual void OnFileProgress(FileProgressEventArgs e) => FileProgress?.Invoke(this, e);
        protected virtual void OnFileTransferred(FileCopiedEventArgs e) => FileTransferred?.Invoke(this, e);
        protected virtual void OnFileTransferError(FileCopyErrorEventArgs e) => FileTransferError?.Invoke(this, e);
    }

    /// <summary>Event arguments for file progress updates.</summary>
    public class FileProgressEventArgs : EventArgs
    {
        public string SourceFile { get; set; } = string.Empty;
        public string DestFile { get; set; } = string.Empty;
        public double CurrentProgress { get; set; }
    }


    /// <summary>Event arguments for successful file transfers.</summary>
    public class FileCopiedEventArgs : EventArgs
    {
        public string SourceFile { get; set; } = string.Empty;
        public string DestFile { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public double TransferTimeMs { get; set; }
    }

    /// <summary>Event arguments for file transfer errors.</summary>
    public class FileCopyErrorEventArgs : EventArgs
    {
        public string SourceFile { get; set; } = string.Empty;
        public string DestFile { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public Exception Exception { get; set; } = null!;
    }
}
