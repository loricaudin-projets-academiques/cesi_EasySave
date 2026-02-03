using System.Xml.Linq;

namespace EasySave.Core.Models
{
    public class BackupWork
    {
        public string Name { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public BackupType Type { get; set; }

        public BackupWork(string name, string sourcePath, string destinationPath, BackupType type)
        {
            this.Name = name;
            this.SourcePath = sourcePath;
            this.DestinationPath = destinationPath;
            this.Type = type;
        }

        public string Execute()
        {
            if (Type == BackupType.DIFFERENTIAL_BACKUP)
            {
                return ExecuteDifferentialBackup();
            }
            else if (Type == BackupType.FULL_BACKUP)
            {
                return ExecuteFullBackup();
            }

            return "Unknown backup type.";
        }

        private string ExecuteFullBackup()
        {
            // Set the destination folder for full backup
            string backupDestination = @"C:\tmpinst\source\repos\cesi_EassySave\Backups_Complete";

            // Create the destination folder if it doesn't exist
            Directory.CreateDirectory(backupDestination);

            // Get all files from the source folder
            string[] files = Directory.GetFiles(SourcePath);

            // Loop through each file and copy it to the destination folder
            foreach (string file in files)
            {
                // Get the file name from the full path
                string fileName = Path.GetFileName(file);

                // Combine destination path with the file name to get full destination path
                string destFile = Path.Combine(backupDestination, fileName);

                // Copy the file to the destination folder
                File.Copy(file, destFile, true);
            }

            // Return a success message
            return "Full backup completed successfully.";
        }

        private string ExecuteDifferentialBackup()
        {
            // Set the destination folder for differential backup
            string backupDestination = @"C:\tmpinst\source\repos\cesi_EassySave\Backups_Differential";

            // Create the destination folder if it doesn't exist
            Directory.CreateDirectory(backupDestination);

            // Get all files from the source folder
            string[] files = Directory.GetFiles(SourcePath);

            // Loop through each file in the source folder
            foreach (string file in files)
            {
                // Get the file name from the full path
                string fileName = Path.GetFileName(file);

                // Combine destination path with the file name to get full destination path
                string destFile = Path.Combine(backupDestination, fileName);

                // If the file does not exist in the backup, copy it
                if (!File.Exists(destFile))
                {
                    File.Copy(file, destFile);
                }
                // If the file exists but the source file is newer, overwrite it
                else if (File.GetLastWriteTime(file) > File.GetLastWriteTime(destFile))
                {
                    File.Copy(file, destFile, true); // true = overwrite the old file
                }
            }

            // Return a success message
            return "Differential backup completed successfully.";
        }

        // Getters
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

        // Setters
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
    }
}