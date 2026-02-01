using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Core.Models
{
    public class BackupWorkList

    {
        public List <BackupWork> liste { get; set; }

        public BackupWorkList(List<BackupWork> liste)
        {
            this.liste = liste;
        }

        public bool RemoveBackupWork(BackupWork backupWork)
        {
            try
            {
                this.liste.Remove(backupWork);
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
                this.liste.RemoveAt(id);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
