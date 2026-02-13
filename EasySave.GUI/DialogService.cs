using EasySave.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EasySave.GUI
{
    public class DialogService : IDialogService
    {
        public void ShowAbout()
        {
            MessageBox.Show(
                "EasySave\nVersion 2.0",
                "À propos",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        public bool Confirm(string message, string title)
        {
            return MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            ) == MessageBoxResult.Yes;
        }

        public void ShowError(string message, string title = "Erreur")
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }

        public void ShowInfo(string message, string title = "Information")
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }

}
