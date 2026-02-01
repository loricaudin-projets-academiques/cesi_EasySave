using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;

namespace EasySave.Core.Models
{
    public class BackupWork
    {
        public string name { get; set; }
        public string sourcePath { get; set; }
        public string destinationPath { get; set; }
        public BackupType type { get; set; }

        public BackupWork(string sourcePath, string destinationPath, string name, BackupType type)
        {
            this.name = name;
            this.sourcePath = sourcePath;
            this.destinationPath = destinationPath;
            this.type = type;
        }

        public string Execute()
        {
            if (type == BackupType.DIFFERENTIAL_BACKUP)
            {
                return ExecuteDifferentialBackup();
            }
            else if (type == BackupType.FULL_BACKUP)
            {
                return ExecuteFullBackup();
            }


            return "Unknown backup type.";
        }

        private string ExecuteFullBackup()
        {
            //Set the destination folder for full backup
            destinationPath = @"C:\tmpinst\source\repos\cesi_EassySave\Backups_Complete";

            // Create the destination folder if it doesn't exist
            Directory.CreateDirectory(destinationPath);

            // Get all files from the source folder
            string[] files = Directory.GetFiles(sourcePath);

            // Loop through each file and copy it to the destination folder
            foreach (string file in files)
            {
                // Get the file name from the full path
                string fileName = Path.GetFileName(file);

                // Combine destination path with the file name to get full destination path
                string destFile = Path.Combine(destinationPath, fileName);

                // Copy the file to the destination folder (overwrite not needed here, all files are copied)
                File.Copy(file, destFile);
            }

            // Return a success message
            return "Full backup completed successfully.";
        }

        private string ExecuteDifferentialBackup()
        {
            // Set the destination folder for differential backup
            destinationPath = @"C:\tmpinst\source\repos\cesi_EassySave\Backups_Differential";

            // Create the destination folder if it doesn't exist
            Directory.CreateDirectory(destinationPath);

            // Get all files from the source folder
            string[] files = Directory.GetFiles(sourcePath);

            // Loop through each file in the source folder
            foreach (string file in files)
            {
                // Get the file name from the full path
                string fileName = Path.GetFileName(file);

                // Combine destination path with the file name to get full destination path
                string destFile = Path.Combine(destinationPath, fileName);

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
