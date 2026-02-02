using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Core.Models
{
    public class BackupWorkList

    {
        public List<BackupWork> liste { get; set; }

        public BackupWorkList(List<BackupWork> liste)
        {
            this.liste = liste;


        }


        public bool AddBackupWork(BackupWork backupWork)
        {
            {
                // Add the BackupWork object to the list
                liste.Add(backupWork);
                return true; // The object was successfully added
            }

            return false; // The object already exists in the list, not added
        }

    }
}