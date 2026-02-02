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

        public BackupWork(string sourcePath, string destinationPath, string name, BackupType type)
        {
            this.Name = name;
            this.SourcePath = sourcePath;
            this.DestinationPath = destinationPath;
            this.Type = type;
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
            ProgressBar progressBar = new ProgressBar();
            // Create the destination folder if it doesn't exist
            Directory.CreateDirectory(this.DestinationPath);

            // Get all files from the source folder
            string[] files = Directory.GetFiles(this.SourcePath);

            // Loop through each file and copy it to the destination folder
            foreach (string file in files)
            {
                // Get the file name from the full path
                string fileName = Path.GetFileName(file);

                // Combine destination path with the file name to get full destination path
                string destFile = Path.Combine(this.DestinationPath, fileName);

                // Copy the file to the destination folder (overwrite not needed here, all files are copied)
                File.Copy(file, destFile);
            }

            // Return a success message
            return "Full backup completed successfully.";
        }

        private string ExecuteDifferentialBackup()
        {
            // Create the destination folder if it doesn't exist
            Directory.CreateDirectory(this.DestinationPath);

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

            //  Return a success message
            return "Differential backup completed successfully.";
        }
    }
}
