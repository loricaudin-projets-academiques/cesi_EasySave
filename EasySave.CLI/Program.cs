using EasySave.Core.Models;

namespace EasySave.CLI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Test
            BackupWork bw = new BackupWork(
                    @"C:\Users\audin\Téléchargements\Dossier 1",
                    @"D:\Dossier 2",
                    "Test",
                    BackupType.DIFFERENTIAL_BACKUP
                );
            bw.Execute();
        }
    }
}
