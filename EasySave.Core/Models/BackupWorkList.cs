using EasySave.Core.Services.Serialization;

namespace EasySave.Core.Models
{
    /// <summary>
    /// Manages a collection of backup works with persistence to JSON.
    /// </summary>
    public class BackupWorkList
    {
        private List<BackupWork> List { get; set; }
        private readonly JsonFileGestion jsonFileGestion;

        /// <summary>Path to the JSON configuration file.</summary>
        public static readonly string JSON_FILE_PATH = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
            "AppData", "Roaming", "ProSoft", "EasySave", "config", "BackupWorks.json");

        /// <summary>
        /// Creates a new BackupWorkList and loads existing works from JSON.
        /// </summary>
        public BackupWorkList()
        {
            this.jsonFileGestion = JsonFileGestion.GetInstance();
            this.List = LoadFromJson() ?? new List<BackupWork>();
        }

        /// <summary>
        /// Creates a new BackupWorkList with the specified list (for testing).
        /// </summary>
        /// <param name="list">Initial list of backup works.</param>
        public BackupWorkList(List<BackupWork> list)
        {
            this.List = list;
            this.jsonFileGestion = JsonFileGestion.GetInstance();
        }

        /// <summary>
        /// Loads the backup works list from JSON file.
        /// </summary>
        /// <returns>List of backup works or null if file doesn't exist.</returns>
        private List<BackupWork>? LoadFromJson()
        {
            try
            {
                if (File.Exists(JSON_FILE_PATH))
                    return jsonFileGestion.Open<List<BackupWork>>(JSON_FILE_PATH);
            }
            catch
            {
            }
            return null;
        }

        /// <summary>
        /// Adds a new backup work to the list.
        /// </summary>
        /// <param name="backupWork">The backup work to add.</param>
        public void AddBackupWork(BackupWork backupWork)
        {
            this.List.Add(backupWork);
            this.jsonFileGestion.Save(JSON_FILE_PATH, this.List);
        }

        /// <summary>
        /// Replaces an existing backup work with a new one.
        /// </summary>
        /// <param name="oldBackupWork">The work to replace.</param>
        /// <param name="newBackupWork">The new work.</param>
        /// <returns>The new backup work if successful, null otherwise.</returns>
        public BackupWork? EditBackupWork(BackupWork oldBackupWork, BackupWork newBackupWork)
        {
            int index = this.List.IndexOf(oldBackupWork);
            if (index == -1)
                return null;

            this.List[index] = newBackupWork;
            this.jsonFileGestion.Save(JSON_FILE_PATH, this.List);
            return newBackupWork;
        }

        /// <summary>
        /// Removes a backup work from the list.
        /// </summary>
        /// <param name="backupWork">The work to remove.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public bool RemoveBackupWork(BackupWork backupWork)
        {
            try
            {
                this.List.Remove(backupWork);
                this.jsonFileGestion.Save(JSON_FILE_PATH, this.List);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Removes a backup work by its index.
        /// </summary>
        /// <param name="id">Index of the work to remove.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public bool RemoveBackupWorkById(int id)
        {
            try
            {
                this.List.RemoveAt(id);
                this.jsonFileGestion.Save(JSON_FILE_PATH, this.List);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets all backup works.
        /// </summary>
        /// <returns>List of all backup works.</returns>
        public List<BackupWork> GetAllWorks() => this.List;

        /// <summary>
        /// Gets the count of backup works.
        /// </summary>
        /// <returns>Number of backup works.</returns>
        public int GetCount() => this.List.Count;

        /// <summary>
        /// Executes a backup work by its index.
        /// </summary>
        /// <param name="index">Index of the work to execute.</param>
        public void ExecuteBackupWork(int index)
        {
            if (index < 0 || index >= List.Count)
            {
                Console.WriteLine("Invalid backup work index.");
                return;
            }

            try
            {
                List[index].Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Executes all backup works sequentially.
        /// </summary>
        public void ExecuteAllBackupWorks()
        {
            foreach (BackupWork work in List)
            {
                try
                {
                    work.Execute();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}