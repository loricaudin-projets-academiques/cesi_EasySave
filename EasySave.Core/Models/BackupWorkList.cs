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

        public void AddBackupWork(BackupWork backupWork)
        {
            liste.Add(backupWork);

        }


    }
}