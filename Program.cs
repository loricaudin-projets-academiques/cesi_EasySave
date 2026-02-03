using System;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("No argument provided.");
        }

        BackupWorkList list = new BackupWorkList();

        if (args[0] == "AddBackupWork")
        {
            list.AddBackupWork(new BackupWork("Backup1", "C:\\source", "D:\\dest", BackupType.FULL_BACKUP));
        }

        else if (args[0] == "ModifyBackupWork")
        {

            list.ModifyBackupWork(new BackupWork("Backup1", "C:\\source", "D:\\dest", BackupType.FULL_BACKUP));

        }

        else if (args[0] == "RemoveBackupWork")
        {
            list.RemoveBackupWork();

        }
        else if (args[0] == "Execute")
        {
            list.Execute(new BackupWork("Backup1", "C:\\source", "D:\\dest", BackupType.FULL_BACKUP));

        }
        else
        {
            Console.WriteLine("Unknown command");
        }
        
    }
}
