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

    }
}
