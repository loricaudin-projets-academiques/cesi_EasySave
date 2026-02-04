using EasySave.Core.Models;
using EasySave.Core.Services;

namespace EasySave.CLI
{
    internal class Program
    {
        static JsonFileGestion jsonFileGestion = JsonFileGestion.GetInstance();
        static BackupWorkList? backupWorkList;

        static void Main(string[] args)
        {
            try
            {
                List<BackupWork>? backupWorks = jsonFileGestion.Open<List<BackupWork>>(BackupWorkList.JSON_FILE_PATH);
                backupWorkList = new BackupWorkList(backupWorks);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                backupWorkList = new BackupWorkList();
            }

            //Test
            BackupWork bw = new BackupWork(
                    "Test",
                    @"C:\Users\audin\Téléchargements\Dossier 1",
                    @"D:\Dossier 2",
                    BackupType.DIFFERENTIAL_BACKUP
                );

            backupWorkList?.AddBackupWork(bw);
            bw.Execute();
        }
    }
}
