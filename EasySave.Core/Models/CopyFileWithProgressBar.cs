using EasySave.Core.Models;
using System.Diagnostics;

internal class CopyFileWithProgressBar : ProgressBar
{
    private long totalBytes;
    private long copiedBytes;
    private readonly BackupState State;
    
    // ✅ Événement pour notifier la progression (fichier courant)
    public event EventHandler<FileProgressEventArgs>? FileProgress;
    
    // ✅ Événement pour notifier quand un fichier est transféré
    public event EventHandler<FileCopiedEventArgs>? FileTransferred;
    public event EventHandler<FileCopyErrorEventArgs>? FileTransferError;

    public CopyFileWithProgressBar(BackupState backupState)
    {
        this.State = backupState;
    }

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
                
                // ✅ Émettre l'événement de progression avec les chemins courants
                OnFileProgress(new FileProgressEventArgs
                {
                    SourceFile = source,
                    DestFile = dest,
                    CurrentProgress = progress * 100
                });
            });

            stopwatch.Stop();

            // ✅ Émettre l'événement de transfert réussi
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

            // ✅ Émettre l'événement d'erreur
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
        const int bufferSize = 1024 * 1024; // 1 Mo
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
            catch (IOException e)
            {
                throw new Exception($"A error is occured during backup work. Check if folders is always accessible and there is enough space disk.");
            }

            onChunkCopied?.Invoke(read);
        }
    }

    // ============ EVENT EMISSION ============

    protected virtual void OnFileProgress(FileProgressEventArgs e)
    {
        FileProgress?.Invoke(this, e);
    }

    protected virtual void OnFileTransferred(FileCopiedEventArgs e)
    {
        FileTransferred?.Invoke(this, e);
    }

    protected virtual void OnFileTransferError(FileCopyErrorEventArgs e)
    {
        FileTransferError?.Invoke(this, e);
    }
}

// ============ EVENT ARGS ============

public class FileProgressEventArgs : EventArgs
{
    public string SourceFile { get; set; } = string.Empty;
    public string DestFile { get; set; } = string.Empty;
    public double CurrentProgress { get; set; }
}

public class FileCopiedEventArgs : EventArgs
{
    public string SourceFile { get; set; } = string.Empty;
    public string DestFile { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public double TransferTimeMs { get; set; }
}

public class FileCopyErrorEventArgs : EventArgs
{
    public string SourceFile { get; set; } = string.Empty;
    public string DestFile { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public Exception Exception { get; set; } = null!;
}





