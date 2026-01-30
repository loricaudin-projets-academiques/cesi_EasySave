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



    }
}
