using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Core.Models
{
    public class BackupState
    {
        private string BackupName { get; set; }
        private DateTime LastActionTimestamp { get; set; }
        private int TotalFiles { get; set; }
        private double Progress { get; set; }
        private long FileSize { get; set; }
        private string PathSourceFile { get; set; }
        private string PathDestinationFile { get; set; }

        public BackupState(string backupName, DateTime lastActionTimestamp, int totalFiles, double progress, long fileSize, string pathSourceFile, string pathDestinationFile)
        {
            this.BackupName = backupName;
            this.LastActionTimestamp = lastActionTimestamp;
            this.TotalFiles = totalFiles;
            this.Progress = progress;
            this.FileSize = fileSize;
            this.PathSourceFile = pathSourceFile;
            this.PathDestinationFile = pathDestinationFile;
        }

        public void SetBackupName(string backupName)
        {
            this.BackupName = backupName;
        }

        public void SetLastActionTimestamp(DateTime lastActionTimestamp)
        {
            this.LastActionTimestamp = lastActionTimestamp;
        }

        internal void SetTotalFiles(int totalFiles)
        {
            this.TotalFiles = totalFiles;
        }

        internal void SetFileSize(long fileSize)
        {
            this.FileSize = fileSize;
        }

        internal void SetProgress(double progress)
        {
            this.Progress = progress;
        }
    }
}
