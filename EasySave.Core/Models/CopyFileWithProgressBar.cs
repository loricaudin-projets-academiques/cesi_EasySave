using EasySave.Core.Models;
using System.Diagnostics;

namespace EasySave.Core.Models
{
    /// <summary>
    /// Handles file copying with progress bar display and event notifications.
    /// </summary>
    internal class CopyFileWithProgressBar : ProgressBar
    {
        private long totalBytes;
        private long copiedBytes;
        private readonly BackupState State;
        
        /// <summary>Event raised when file copy progress updates.</summary>
        public event EventHandler<FileProgressEventArgs>? FileProgress;
        
        /// <summary>Event raised when a file is successfully transferred.</summary>
        public event EventHandler<FileCopiedEventArgs>? FileTransferred;
        
        /// <summary>Event raised when a file transfer fails.</summary>
        public event EventHandler<FileCopyErrorEventArgs>? FileTransferError;

        public CopyFileWithProgressBar(BackupState backupState)
        {
            this.State = backupState;
        }

        /// <summary>
        /// Copies multiple files from source to destination with progress tracking.
        /// </summary>
        /// <param name="source">Source directory path.</param>
        /// <param name="dest">Destination directory path.</param>
        /// <param name="files">Array of file paths to copy.</param>
        public void CopyFiles(string source, string dest, string[] files)
        {
            this.totalBytes = files.Sum(f => new FileInfo(f).Length);
            this.copiedBytes = 0;

            this.State.SetLastActionTimestamp(DateTime.UtcNow);
            this.State.SetTotalFiles(files.Length);
            this.State.SetFileSize(this.totalBytes);

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
                    this.copiedBytes += bytesCopiedInFile;

                    double progress = (double)this.copiedBytes / this.totalBytes;
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





