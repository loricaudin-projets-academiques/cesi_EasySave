namespace EasySave.Core.Models
{
    /// <summary>
    /// Represents the current state of a backup operation.
    /// </summary>
    public class BackupState
    {
        private string BackupName { get; set; }
        private DateTime LastActionTimestamp { get; set; }
        private int TotalFiles { get; set; }
        private double Progress { get; set; }
        private long FileSize { get; set; }
        private string PathSourceFile { get; set; }
        private string PathDestinationFile { get; set; }

        /// <summary>
        /// Creates a new backup state.
        /// </summary>
        /// <param name="backupName">Name of the backup job.</param>
        /// <param name="lastActionTimestamp">Timestamp of last action.</param>
        /// <param name="totalFiles">Total number of files.</param>
        /// <param name="progress">Current progress percentage.</param>
        /// <param name="fileSize">Total file size in bytes.</param>
        /// <param name="pathSourceFile">Source file path.</param>
        /// <param name="pathDestinationFile">Destination file path.</param>
        public BackupState(string backupName, DateTime lastActionTimestamp, int totalFiles, 
            double progress, long fileSize, string pathSourceFile, string pathDestinationFile)
        {
            this.BackupName = backupName;
            this.LastActionTimestamp = lastActionTimestamp;
            this.TotalFiles = totalFiles;
            this.Progress = progress;
            this.FileSize = fileSize;
            this.PathSourceFile = pathSourceFile;
            this.PathDestinationFile = pathDestinationFile;
        }

        public void SetBackupName(string backupName) => this.BackupName = backupName;
        public void SetLastActionTimestamp(DateTime lastActionTimestamp) => this.LastActionTimestamp = lastActionTimestamp;
        internal void SetTotalFiles(int totalFiles) => this.TotalFiles = totalFiles;
        internal void SetFileSize(long fileSize) => this.FileSize = fileSize;
        internal void SetProgress(double progress) => this.Progress = progress;
    }
}
