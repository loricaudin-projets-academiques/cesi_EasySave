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
            this.jsonFileGestion = JsonFileGestion.GetInstance();
            // ✅ Charger depuis JSON au démarrage, sinon liste vide
            this.List = LoadFromJson() ?? new List<BackupWork>();
        }

        private List<BackupWork> List { get; set; }

        private JsonFileGestion jsonFileGestion;

        public static readonly string JSON_FILE_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Roaming", "ProSoft", "EasySave", "config", "BackupWorks.json");

        public BackupWorkList(List<BackupWork> list)
        {
            this.List = list;
            this.jsonFileGestion = JsonFileGestion.GetInstance();
        }

        /// <summary>
        /// Charge la liste des travaux depuis le fichier JSON
        /// </summary>
        private List<BackupWork>? LoadFromJson()
        {
            try
            {
                if (File.Exists(JSON_FILE_PATH))
                {
                    // ✅ Utilise Open() au lieu de Load()
                    var loaded = jsonFileGestion.Open<List<BackupWork>>(JSON_FILE_PATH);
                    return loaded;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"⚠️  Erreur lors du chargement du JSON: {ex.Message}");
            }
            return null;
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

        public BackupWork? EditBackupWork(BackupWork oldBackupWork, BackupWork newBackupWork)
        {
            // 1. On cherche l'index de l'ancien travail de sauvegarde
            int index = this.List.IndexOf(oldBackupWork);

            // 2. Si on le trouve (index n'est pas -1)
            if (index != -1)
            {
                // On remplace l'ancien objet par le nouveau à cet index précis
                this.List[index] = newBackupWork;

                // On sauvegarde la liste modifiée dans le JSON
                this.jsonFileGestion.Save<List<BackupWork>>(JSON_FILE_PATH, this.List);

                // On retourne le nouveau travail pour confirmer
                return newBackupWork;
            }

            // 3. Si non trouvé, on retourne null
            return null;
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