using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;

namespace EasySave.Core.Models
{
    public class BackupWork
    {
        private string Name { get; set; }
        private string SourcePath { get; set; }
        private string DestinationPath { get; set; }
        private BackupType Type { get; set; }
        private BackupState State { get; set; }

        public BackupWork(string sourcePath, string destinationPath, string name, BackupType type)
        {
            this.Name = name;
            this.SourcePath = sourcePath;
            this.DestinationPath = destinationPath;
            this.Type = type;
            this.State = new BackupState(this.Name, DateTime.Now, 0, 0.0, 0, this.SourcePath, this.DestinationPath);
        }

        public void Execute()
        {
            if (!Directory.Exists(this.SourcePath))
            {
                throw new Exception($"Source path is invalid or not accessible: {this.SourcePath}");
            }
            if (!Directory.Exists(this.DestinationPath))
            {
                throw new Exception($"Destination path is invalid or not accessible: {this.DestinationPath}");
            }

            if (this.Type == BackupType.DIFFERENTIAL_BACKUP)
            {
                ExecuteDifferentialBackup();
            }
            else if (this.Type == BackupType.FULL_BACKUP)
            {
                ExecuteFullBackup();
            }
            else
            {
                throw new Exception("Unknown backup type.");
            }
        }

        private void ExecuteFullBackup()
        {
            // Get all files from the source folder
            string[] files = Directory.GetFiles(this.SourcePath);

            CopyFileWithProgressBar cp = new CopyFileWithProgressBar(this.State);
            cp.InitProgressBar($"Full Backup in progress for : {this.Name}");
            cp.CopyFiles(this.SourcePath, this.DestinationPath, files);
        }

        private void ExecuteDifferentialBackup()
        {
            // Get all files from the source folder
            string[] files = Directory.GetFiles(this.SourcePath);
            List<string> filesToUpdate = new List<string>();

            CopyFileWithProgressBar cp = new CopyFileWithProgressBar(this.State);
            cp.InitProgressBar($"Differential Backup in progress for: {this.Name}");

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string sourceFile = Path.Combine(this.SourcePath, fileName);
                string destFile = Path.Combine(this.DestinationPath, fileName);

                if (!File.Exists(destFile) || File.GetLastWriteTime(file) > File.GetLastWriteTime(destFile) || new FileInfo(sourceFile).Length != new FileInfo(destFile).Length)
                {
                    filesToUpdate.Add(file);
                }
            }
            files = filesToUpdate.ToArray();

            cp.CopyFiles(this.SourcePath, this.DestinationPath, files);
        }
    }
}
