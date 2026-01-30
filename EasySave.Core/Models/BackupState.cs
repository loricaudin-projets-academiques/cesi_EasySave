using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Core.Models
{
    public class BackupState
    {
        public string backupName { get; set; }
        public DateTime lastActionTimestamp { get; set; }
        public int totalFiles { get; set; }
        public float progress { get; set; }
        public long fileSize { get; set; }
        public string fullPathSourceFile { get; set; }
        public string fullPathDestinationFile { get; set; }
        public BackupState(string backupName, DateTime lastActionTimestamp, int totalFiles, float progress, long fileSize, string fullPathSourceFile, string fullPathDestinationFile)
        {
            this.backupName = backupName;
            this.lastActionTimestamp = lastActionTimestamp;
            this.totalFiles = totalFiles;
            this.progress = progress;
            this.fileSize = fileSize;
            this.fullPathSourceFile = fullPathSourceFile;
            this.fullPathDestinationFile = fullPathDestinationFile;

        }

    }
}
