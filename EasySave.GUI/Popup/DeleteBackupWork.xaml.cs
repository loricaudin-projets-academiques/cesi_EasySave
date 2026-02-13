using EasySave.Core.Models;
using System.Collections.Generic;
using System.Windows;

namespace EasySave.GUI.Popup
{
    public partial class DeleteBackupWork : Window
    {
        private BackupWorkList _backupWorkList;

        public DeleteBackupWork(BackupWorkList backupWorkList)
        {
            InitializeComponent();

            _backupWorkList = backupWorkList;


            BackupComboBox.ItemsSource = _backupWorkList.GetAllWorks();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (BackupComboBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Veuillez sélectionner un backup.",
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            BackupWork selectedBackup = (BackupWork)BackupComboBox.SelectedItem;

            // Message de confirmation
            MessageBoxResult result = MessageBox.Show(
                $"Voulez-vous vraiment supprimer le backup \"{selectedBackup.GetName()}\" ?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _backupWorkList.RemoveBackupWork(selectedBackup);
                MessageBox.Show("Backup supprimé avec succès.", "Succès");

                Close();
            }
        }
    }
}
