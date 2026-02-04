using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Core.Models
{
    public class BackupWorkList
    {
        // Constructeur par défaut
        public BackupWorkList()
        {
            this.liste = new List<BackupWork>();
        }

        private List <BackupWork> List { get; set; }

        public BackupWorkList(List<BackupWork> list)
        {
            this.List = list;
        }

        public void AddBackupWork(BackupWork backupWork)
        {
            if (liste.Count >= 5)
            {
                throw new Exception("Cannot add more than 5 backup works.");
            }
            // Add the BackupWork object to the list
            this.List.Add(backupWork);
        }
              
        public bool UpdateBackupWork(BackupWork backupWork, string name, string sourcePath, string destinationPath, BackupType type)
        {
            try
            {
                backupWork.SetName(name);
                backupWork.SetSourcePath(sourcePath);
                backupWork.SetDestinationPath(destinationPath);
                backupWork.SetType(type);

                // Update succeeded
                return true;
            }
            catch
            {
                return false;
            }
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
public List<BackupWork> GetAllWorks()
        {
            return this.liste;
        }

        public int GetCount()
        {
            return this.liste.Count;
        }

        public void ExecuteBackupWork(int index)
        {
            if (index >= 0 && index < liste.Count)
            {
                string result = liste[index].Execute();
                Console.WriteLine(result);
            }
            else
            {
                Console.WriteLine("Invalid backup work index.");
            }
        }

        public void ExecuteAllBackupWorks()
        {
            foreach (BackupWork work in liste)
            {
                string result = work.Execute();
                Console.WriteLine(result);
            }
        }
    }
}