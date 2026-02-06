using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;

namespace EasySave.Core.Models
{
    public class BackupWork
    {
        public string Name { get; private set; }
        public string SourcePath { get; private set; }
        public string DestinationPath { get; private set; }
        public BackupType Type { get; private set; }
        public BackupState State { get; private set; }
        
        // ? Événements pour transmettre les transferts de fichiers
        public event EventHandler? FileProgress;
        public event EventHandler? FileTransferred;
        public event EventHandler? FileTransferError;

        public BackupWork(string name, string sourcePath, string destinationPath, BackupType type)
        {
            this.Name = name;
            this.SourcePath = sourcePath;
            this.DestinationPath = destinationPath;
            this.Type = type;
            this.State = new BackupState(this.Name, DateTime.UtcNow, 0, 0.0, 0, this.SourcePath, this.DestinationPath);
        }
      
        public string GetName()
        {
            return this.Name;
        }

        public string GetDestinationPath()
        {
            return this.DestinationPath;
        }

        public string GetSourcePath()
        {
            return this.SourcePath;
        }

        public BackupType GetBackupType()
        {
            return this.Type;
        }

        public void SetName(string name)
        {
            Name = name;
        }

        public void SetDestinationPath(string destinationPath)
        {
            DestinationPath = destinationPath;
        }

        public void SetSourcePath(string sourcePath)
        {
            SourcePath = sourcePath;
        }

        public void SetType(BackupType type)
        {
            Type = type;
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
            string[] files = Directory.GetFiles(this.SourcePath);

            CopyFileWithProgressBar cp = new CopyFileWithProgressBar(this.State);
            
            // ? Transmettre les événements
            cp.FileProgress += (sender, args) => FileProgress?.Invoke(sender, args);
            cp.FileTransferred += (sender, args) => FileTransferred?.Invoke(sender, args);
            cp.FileTransferError += (sender, args) => FileTransferError?.Invoke(sender, args);
            
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