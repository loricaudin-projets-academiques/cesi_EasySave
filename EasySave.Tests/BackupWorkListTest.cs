using EasySave.Core.Models;
using Xunit;

namespace EasySave.Tests
{
    public class BackupWorkListTest
    {
        [Fact]
        
        public void EditBackupWork_Test()
        {
            // Arrange
            BackupWorkList list = new BackupWorkList(new List<BackupWork>());
            BackupWork oldBackup = new BackupWork("OldName", @"C:\OldSource", @"C:\OldDest", BackupType.DIFFERENTIAL_BACKUP);
            BackupWork newBackup = new BackupWork("NewName", @"C:\NewSource", @"C:\NewDest", BackupType.FULL_BACKUP);
            list.AddBackupWork(oldBackup);

            // Act
            BackupWork? result = list.EditBackupWork(oldBackup, newBackup);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, list.GetCount());
            Assert.Equal("NewName", result!.GetName());
            Assert.Equal(@"C:\NewSource", result.GetSourcePath());
            Assert.Equal(@"C:\NewDest", result.GetDestinationPath());
            Assert.Equal(BackupType.FULL_BACKUP, result.GetBackupType());
        }

        [Fact]
        public void RemoveBackupWork_Test()
        {

            BackupWorkList list = new BackupWorkList(new List<BackupWork>());
            BackupWork backupWork = new BackupWork(@"C:\Test_1", @"C:\Test_2", "TestBackup", BackupType.DIFFERENTIAL_BACKUP);

            list.AddBackupWork(backupWork);


            bool result = list.RemoveBackupWork(backupWork);


            Assert.True(result);
            Assert.Equal(0, list.GetCount());
        }

        [Fact]
        public void AddBackupWork_Test()
        {
            BackupWorkList list = new BackupWorkList(new List<BackupWork>());

 
            Assert.Equal(0, list.GetCount());

            for (int i = 0; i < 10; i++)
            {
                list.AddBackupWork(new BackupWork($"Backup{i}", @"C:\Source", @"C:\Destination", BackupType.FULL_BACKUP));
            }

            Assert.Equal(10, list.GetCount());
        }

        [Fact]
        public void GetAllWorks()
        {
            
            BackupWorkList list = new BackupWorkList(new List<BackupWork>());

           
            List<BackupWork> works = list.GetAllWorks();

            Assert.NotNull(works);
            Assert.Empty(works);
        }
    }
}
