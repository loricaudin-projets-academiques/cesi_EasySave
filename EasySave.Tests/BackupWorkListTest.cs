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
            string tempDir = Path.GetTempPath();
            BackupWork oldBackup = new BackupWork("OldName", Path.Combine(tempDir, "OldSource"), Path.Combine(tempDir, "OldDest"), BackupType.DIFFERENTIAL_BACKUP);
            BackupWork newBackup = new BackupWork("NewName", Path.Combine(tempDir, "NewSource"), Path.Combine(tempDir, "NewDest"), BackupType.FULL_BACKUP);
            list.AddBackupWork(oldBackup);

            // Act
            BackupWork? result = list.EditBackupWork(oldBackup, newBackup);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, list.GetCount());
            Assert.Equal("NewName", result!.GetName());
            Assert.Equal(Path.Combine(tempDir, "NewSource"), result.GetSourcePath());
            Assert.Equal(Path.Combine(tempDir, "NewDest"), result.GetDestinationPath());
            Assert.Equal(BackupType.FULL_BACKUP, result.GetBackupType());
        }

        [Fact]
        public void RemoveBackupWork_Test()
        {

            BackupWorkList list = new BackupWorkList(new List<BackupWork>());
            BackupWork backupWork = new BackupWork(Path.Combine(Path.GetTempPath(), "Test_1"), Path.Combine(Path.GetTempPath(), "Test_2"), "TestBackup", BackupType.DIFFERENTIAL_BACKUP);

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
                list.AddBackupWork(new BackupWork($"Backup{i}", Path.Combine(Path.GetTempPath(), "Source"), Path.Combine(Path.GetTempPath(), "Destination"), BackupType.FULL_BACKUP));
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
