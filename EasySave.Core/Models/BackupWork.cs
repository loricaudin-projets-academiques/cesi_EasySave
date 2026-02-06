using System.Text.Json.Serialization;

namespace EasySave.Core.Models
{
    /// <summary>
    /// Represents a backup work/job with source, destination and backup type.
    /// </summary>
    public class BackupWork
    {
        public string Name { get; private set; }
        public string SourcePath { get; private set; }
        public string DestinationPath { get; private set; }
        
        /// <summary>Backup type serialized as string (FULL_BACKUP / DIFFERENTIAL_BACKUP).</summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BackupType Type { get; private set; }
        
        /// <summary>Runtime state, excluded from JSON serialization.</summary>
        [JsonIgnore]
        public BackupState State { get; private set; }
        
        /// <summary>Event raised when file copy progress updates.</summary>
        public event EventHandler? FileProgress;
        
        /// <summary>Event raised when a file transfer completes successfully.</summary>
        public event EventHandler? FileTransferred;
        
        /// <summary>Event raised when a file transfer fails.</summary>
        public event EventHandler? FileTransferError;

        /// <summary>
        /// Creates a new backup work.
        /// </summary>
        /// <param name="name">Name of the backup job.</param>
        /// <param name="sourcePath">Source directory path.</param>
        /// <param name="destinationPath">Destination directory path.</param>
        /// <param name="type">Type of backup (Full or Differential).</param>
        public BackupWork(string name, string sourcePath, string destinationPath, BackupType type)
        {
            this.Name = name;
            this.SourcePath = sourcePath;
            this.DestinationPath = destinationPath;
            this.Type = type;
            this.State = new BackupState(this.Name, DateTime.UtcNow, 0, 0.0, 0, this.SourcePath, this.DestinationPath);
        }
      
        public string GetName() => this.Name;
        public string GetDestinationPath() => this.DestinationPath;
        public string GetSourcePath() => this.SourcePath;
        public BackupType GetBackupType() => this.Type;

        public void SetName(string name) => Name = name;
        public void SetDestinationPath(string destinationPath) => DestinationPath = destinationPath;
        public void SetSourcePath(string sourcePath) => SourcePath = sourcePath;
        public void SetType(BackupType type) => Type = type;

        /// <summary>
        /// Executes the backup based on its type.
        /// </summary>
        /// <exception cref="Exception">Thrown when paths are invalid or backup type is unknown.</exception>
        public void Execute()
        {
            if (!Directory.Exists(this.SourcePath))
                throw new Exception($"Source path is invalid or not accessible: {this.SourcePath}");
            
            if (!Directory.Exists(this.DestinationPath))
                throw new Exception($"Destination path is invalid or not accessible: {this.DestinationPath}");

            switch (this.Type)
            {
                case BackupType.DIFFERENTIAL_BACKUP:
                    ExecuteDifferentialBackup();
                    break;
                case BackupType.FULL_BACKUP:
                    ExecuteFullBackup();
                    break;
                default:
                    throw new Exception("Unknown backup type.");
            }
        }

        private void ExecuteFullBackup()
        {
            string[] files = Directory.GetFiles(this.SourcePath);

            var cp = new CopyFileWithProgressBar(this.State);
            
            cp.FileProgress += (sender, args) => FileProgress?.Invoke(sender, args);
            cp.FileTransferred += (sender, args) => FileTransferred?.Invoke(sender, args);
            cp.FileTransferError += (sender, args) => FileTransferError?.Invoke(sender, args);
            
            cp.InitProgressBar($"Full Backup in progress for: {this.Name}");
            cp.CopyFiles(this.SourcePath, this.DestinationPath, files);
        }

        private void ExecuteDifferentialBackup()
        {
            string[] files = Directory.GetFiles(this.SourcePath);
            var filesToUpdate = new List<string>();

            var cp = new CopyFileWithProgressBar(this.State);
            cp.InitProgressBar($"Differential Backup in progress for: {this.Name}");

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string sourceFile = Path.Combine(this.SourcePath, fileName);
                string destFile = Path.Combine(this.DestinationPath, fileName);

                bool needsUpdate = !File.Exists(destFile) 
                    || File.GetLastWriteTime(file) > File.GetLastWriteTime(destFile) 
                    || new FileInfo(sourceFile).Length != new FileInfo(destFile).Length;

                if (needsUpdate)
                    filesToUpdate.Add(file);
            }

            cp.CopyFiles(this.SourcePath, this.DestinationPath, filesToUpdate.ToArray());
        }
    }
}