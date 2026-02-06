using EasySave.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Core.Models
{
    public class BackupWorkList
    {
        public BackupWorkList()
        {
            this.List = new List<BackupWork>();
            this.jsonFileGestion = JsonFileGestion.GetInstance();
        }

        private List<BackupWork> List { get; set; }

        private JsonFileGestion jsonFileGestion;

        public static readonly string JSON_FILE_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Roaming", "ProSoft", "EasySave", "config", "BackupWorks.json");

        public BackupWorkList(List<BackupWork> list)
        {
            this.List = list;
            this.jsonFileGestion = JsonFileGestion.GetInstance();
        }

        public void AddBackupWork(BackupWork backupWork)
        {
            if (List.Count >= 5)
            {
                throw new Exception("Cannot add more than 5 backup works.");
            }
            // Add the BackupWork object to the list
            this.List.Add(backupWork);
            this.jsonFileGestion.Save<List<BackupWork>>(JSON_FILE_PATH, this.List);
        }
              
        public bool UpdateBackupWork(BackupWork backupWork, string name, string sourcePath, string destinationPath, BackupType type)
        {
            try
            {
                backupWork.SetName(name);
                backupWork.SetSourcePath(sourcePath);
                backupWork.SetDestinationPath(destinationPath);
                backupWork.SetType(type);

                this.jsonFileGestion.Save<List<BackupWork>>(JSON_FILE_PATH, this.List);

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

                this.jsonFileGestion.Save<List<BackupWork>>(JSON_FILE_PATH, this.List);

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

                this.jsonFileGestion.Save<List<BackupWork>>(JSON_FILE_PATH, this.List);

                return true;
            }
            catch
            {
                return false;
            }
        }
public List<BackupWork> GetAllWorks()
        {
            return this.List;
        }

        public int GetCount()
        {
            return this.List.Count;
        }

        public void ExecuteBackupWork(int index)
        {
            if (index >= 0 && index < List.Count)
            {
                try
                {
                    List[index].Execute();
                    Console.WriteLine("...");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                Console.WriteLine("Invalid backup work index.");
            }
        }

        public void ExecuteAllBackupWorks()
        {
            foreach (BackupWork work in List)
            {
                try
                {
                    work.Execute();
                    Console.WriteLine("...");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}