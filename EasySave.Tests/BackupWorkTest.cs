using Xunit;
using EasySave.Core.Models;

namespace EasySave.Tests
{
    public class BackupWorkTest
    {
        [Fact]
        public void BackupWork_Create()
        {
            // Arrange : préparation des données
            BackupWork backupWork = new BackupWork("Test", @"C:\Test_1", @"C:\Test_2", BackupType.DIFFERENTIAL_BACKUP);

            // Act : exécution de l’action à tester
            var result = backupWork.GetName() == "Test";

            // Assert : vérification du résultat
            Assert.True(result);
        }
    }
}
