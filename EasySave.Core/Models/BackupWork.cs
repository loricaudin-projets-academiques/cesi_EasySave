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

        public string Execute()
        {
            if (this.Type == BackupType.DIFFERENTIAL_BACKUP)
            {
                return ExecuteDifferentialBackup();
            }
            else if (this.Type == BackupType.FULL_BACKUP)
            {
                return ExecuteFullBackup();
            }


            return "Unknown backup type.";
        }

        private string ExecuteFullBackup()
        {
            // Get all files from the source folder
            string[] files = Directory.GetFiles(this.SourcePath);

            CopyFileWithProgressBar cp = new CopyFileWithProgressBar(this.State);
            cp.InitProgressBar($"Full Backup in progress for : {this.Name}");
            cp.CopyFiles(this.SourcePath, this.DestinationPath, files);

            // Return a success message
            return "Full backup completed successfully.";
        }

        private string ExecuteDifferentialBackup()
        {
            // Get all files from the source folder
            string[] files = Directory.GetFiles(this.SourcePath);

            // Loop through each file in the source folder
            foreach (string file in files)
            {
                // Get the file name from the full path
                string fileName = Path.GetFileName(file);

                // Combine destination path with the file name to get full destination path
                string destFile = Path.Combine(this.DestinationPath, fileName);

                // If the file does not exist in the backup, copy it
                if (!File.Exists(destFile) || File.GetLastWriteTime(file) > File.GetLastWriteTime(destFile))
                {
                    File.Copy(file, destFile, true); // true = overwrite the old file
                }
            }

            //  Return a success message
            return "Differential backup completed successfully.";
        }
    }
}
