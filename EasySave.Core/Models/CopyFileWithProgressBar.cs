using EasySave.Core.Models;

internal class CopyFileWithProgressBar : ProgressBar
{
    private long totalBytes;
    private long copiedBytes;

    private BackupState State { get; }

    public CopyFileWithProgressBar(BackupState backupState)
    {
        this.State = backupState;
    }

    public void CopyFiles(string source, string dest, string[] files)
    {
        // Create the destination folder if it doesn't exist
        Directory.CreateDirectory(dest);

        // Calcul du total d'octets à copier
        this.totalBytes = files.Sum(f => new FileInfo(f).Length);
        this.copiedBytes = 0;

        this.State.SetLastActionTimestamp(DateTime.Now);
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
        this.CopyWithProgress(source, dest, bytesCopiedInFile =>
        {
            this.copiedBytes += bytesCopiedInFile;

            double progress = (double)this.copiedBytes / this.totalBytes;
            this.SetProgressBar(progress);
            this.State.SetProgress(progress * 100);
        });
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
            output.Write(buffer, 0, read);

            onChunkCopied?.Invoke(read);
        }
    }
}
