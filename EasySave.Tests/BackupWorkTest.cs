using Xunit;
using EasySave.Core.Models;

namespace EasySave.Tests
{
    public class BackupWorkTest
    {
        [Fact]
        public void BackupWork_Execute()
        {
            string sourcePath = @"C:\Test_1";
            string sourceDest = @"C:\Test_2";

            BackupWork backupWork_1 = new BackupWork("Test", sourcePath, sourceDest, BackupType.FULL_BACKUP);
            BackupWork backupWork_2 = new BackupWork("Test", sourcePath, sourceDest, BackupType.DIFFERENTIAL_BACKUP);


            try
            {
                Directory.CreateDirectory(sourcePath);
                Directory.CreateDirectory(sourceDest);
                File.WriteAllText(sourcePath + "\\test1.txt", "Bonjour");
                backupWork_1.Execute();

                bool result = Directory.GetFiles(sourcePath) != Directory.GetFiles(sourceDest);

                Directory.Delete(sourcePath, true);
                Directory.Delete(sourceDest, true);

                Assert.True(result);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            try
            {
                Directory.CreateDirectory(sourcePath);
                Directory.CreateDirectory(sourceDest);
                backupWork_2.Execute();

                bool result = Directory.GetFiles(sourcePath) != Directory.GetFiles(sourceDest);

                Directory.Delete(sourcePath, true);
                Directory.Delete(sourceDest, true);

                Assert.True(result);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            try
            {
                backupWork_1.Execute();
                Assert.Fail("Backup works must not succeed");
            }
            catch (Exception e)
            {
            }
            try
            {
                backupWork_2.Execute();
                Assert.Fail("Backup works must not succeed");
            }
            catch (Exception e)
            {
            }
        }
    }
}
