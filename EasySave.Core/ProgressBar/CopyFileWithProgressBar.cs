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

        /// <summary>Called between chunks to check if the backup should pause (business software). Returns true if paused.</summary>
        public Func<bool>? PauseChecker { get; set; }

        /// <summary>Gate for manual pause: reset = paused, set = running. Checked between files (not mid-file).</summary>
        public ManualResetEventSlim? ManualPauseGate { get; set; }

        /// <summary>Token checked between chunks to support immediate stop.</summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>Shared lock that prevents parallel transfer of large files.</summary>
        public Services.LargeFileTransferLock? LargeFileLock { get; set; }

        /// <summary>Shared gate that blocks non-priority files until all priority files are done.</summary>
        public Services.PriorityFileGate? PriorityGate { get; set; }

        /// <summary>Event raised when the backup is paused due to business software.</summary>
        public event Action? Paused;

        /// <summary>Event raised when the backup resumes after business software closes.</summary>
        public event Action? Resumed;

        /// <summary>Event raised when manual pause takes effect (after current file).</summary>
        public event Action? ManualPaused;

        /// <summary>Event raised when manual pause is released.</summary>
        public event Action? ManualResumed;

        /// <summary>Event raised when a non-priority file starts waiting for priority files to complete.</summary>
        public event Action? PriorityWaiting;

        /// <summary>Event raised when a non-priority file finishes waiting (priority gate opened).</summary>
        public event Action? PriorityResumed;

        /// <summary>Event raised when a large file starts waiting for the transfer lock.</summary>
        public event Action? LargeFileWaiting;

        /// <summary>Event raised when the large file lock is acquired.</summary>
        public event Action? LargeFileAcquired;

        /// <summary>Event raised when a new file starts being copied. Contains the source file path.</summary>
        public event Action<string>? FileCopyStarted;

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
                // Check for cancellation before starting next file
                CancellationToken.ThrowIfCancellationRequested();

                // Manual pause: wait between files (after current file finished)
                if (ManualPauseGate != null && !ManualPauseGate.IsSet)
                {
                    ManualPaused?.Invoke();
                    ManualPauseGate.Wait(CancellationToken);
                    ManualResumed?.Invoke();
                }

                // Priority gate: block non-priority files until all priority files are done
                if (PriorityGate != null && PriorityGate.IsEnabled && !PriorityGate.IsPriority(file) && PriorityGate.HasPendingPriority)
                {
                    PriorityWaiting?.Invoke();
                    PriorityGate.WaitIfNonPriority(file, CancellationToken);
                    PriorityResumed?.Invoke();
                }
                else
                {
                    PriorityGate?.WaitIfNonPriority(file, CancellationToken);
                }

                FileCopyStarted?.Invoke(file);

                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(dest, fileName);

                if (File.Exists(destFile))
                    File.Delete(destFile);

                bool isPriority = PriorityGate != null && PriorityGate.IsPriority(file);

                this.CopyFile(file, destFile);

                // Mark priority file as completed so the gate can open for non-priority files
                if (isPriority)
                    PriorityGate!.MarkPriorityFileCompleted();
            }
        }

        private void CopyFile(string source, string dest)
        {
            var stopwatch = Stopwatch.StartNew();
            var fileSize = new FileInfo(source).Length;
            bool lockAcquired = false;

            try
            {
                // Acquire large file lock if needed (blocks until no other large file is transferring)
                if (LargeFileLock != null && LargeFileLock.IsLargeFile(fileSize))
                {
                    LargeFileWaiting?.Invoke();
                    lockAcquired = LargeFileLock.Acquire(fileSize, CancellationToken);
                    LargeFileAcquired?.Invoke();
                }
                else if (LargeFileLock != null)
                {
                    lockAcquired = LargeFileLock.Acquire(fileSize, CancellationToken);
                }

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
                    FileSize = fileSize,
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
                    FileSize = fileSize,
                    Exception = ex
                });

                throw;
            }
            finally
            {
                // Always release the lock if we acquired it
                if (lockAcquired)
                    LargeFileLock?.Release();
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
