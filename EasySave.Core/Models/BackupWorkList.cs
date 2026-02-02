using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Core.Models
{
    public class BackupWorkList

    {
        private List <BackupWork> List { get; set; }

        public BackupWorkList(List<BackupWork> list)
        {
            this.List = list;
        }


        public void AddBackupWork(BackupWork backupWork)
        {
            // Add the BackupWork object to the list
            this.List.Add(backupWork);
        }

        public bool RemoveBackupWork(BackupWork backupWork)
        {
            try
            {
                this.List.Remove(backupWork);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveBackupWorkById(int id)
        {
            try
            {
                this.List.RemoveAt(id);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}