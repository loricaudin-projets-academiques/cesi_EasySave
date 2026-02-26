using Xunit;
using EasySave.Core.Models;
using System.IO;

namespace EasySave.Tests
{
    public class BackupWorkTest
    {
        [Fact]
        public void BackupWork_Execute()
        {
            string testRoot = Path.Combine(Path.GetTempPath(), "EasySave_Tests", Guid.NewGuid().ToString());
            string sourcePath = Path.Combine(testRoot, "Source");
            string sourceDest = Path.Combine(testRoot, "Dest");
            BackupWork backupWork_1 = new BackupWork("Test", sourcePath, sourceDest, BackupType.FULL_BACKUP);
            BackupWork backupWork_2 = new BackupWork("Test", sourcePath, sourceDest, BackupType.DIFFERENTIAL_BACKUP);

            try
            {
                Directory.CreateDirectory(sourcePath);
                Directory.CreateDirectory(sourceDest);
                File.WriteAllText(Path.Combine(sourcePath, "test1.txt"), "Bonjour");

                backupWork_1.Execute();


                string[] sourceFiles = Directory.GetFiles(sourcePath);
                string[] destFiles = Directory.GetFiles(sourceDest);

                bool result = destFiles.Length > 0 &&
                              File.Exists(Path.Combine(sourceDest, "test1.txt"));

                Directory.Delete(sourcePath, true);
                Directory.Delete(sourceDest, true);

                Assert.True(result, "Files should have been copied to destination");
            }
            catch (Exception e)
            {
          
                if (Directory.Exists(sourcePath)) Directory.Delete(sourcePath, true);
                if (Directory.Exists(sourceDest)) Directory.Delete(sourceDest, true);
                Assert.Fail(e.Message);
            }

            try
            {
                Directory.CreateDirectory(sourcePath);
                Directory.CreateDirectory(sourceDest);
                File.WriteAllText(Path.Combine(sourcePath, "test2.txt"), "Hello");

                backupWork_2.Execute();

                string[] destFiles = Directory.GetFiles(sourceDest);
                bool result = destFiles.Length > 0 &&
                              File.Exists(Path.Combine(sourceDest, "test2.txt"));

                Directory.Delete(sourcePath, true);
                Directory.Delete(sourceDest, true);

                Assert.True(result, "Files should have been copied to destination");
            }
            catch (Exception e)
            {
             
                if (Directory.Exists(sourcePath)) Directory.Delete(sourcePath, true);
                if (Directory.Exists(sourceDest)) Directory.Delete(sourceDest, true);
                Assert.Fail(e.Message);
            }

            try
            {
                backupWork_1.Execute();
                Assert.Fail("Backup works must not succeed");
            }
            catch (Exception)
            {
               
            }

            
            try
            {
                backupWork_2.Execute();
                Assert.Fail("Backup works must not succeed");
            }
            catch 
            {
                
            }
        }
    }
}