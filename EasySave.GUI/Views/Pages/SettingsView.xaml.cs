using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EasySave.VM.ViewModels;

namespace EasySave.GUI.Views.Pages;

public partial class SettingsView : System.Windows.Controls.UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        Loaded += SettingsView_Loaded;
    }

    private void SettingsView_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            EncryptionPasswordBox.Password = vm.EncryptionPassword;
    }

    private bool _isSyncingPassword;

    private void EncryptionPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_isSyncingPassword) return;
        if (DataContext is SettingsViewModel vm && sender is PasswordBox pb)
            vm.EncryptionPassword = pb.Password;
    }

    private void EncryptionPasswordVisible_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (_isSyncingPassword) return;
        if (DataContext is SettingsViewModel vm && sender is System.Windows.Controls.TextBox tb)
            vm.EncryptionPassword = tb.Text;
    }

    private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
    {
        _isSyncingPassword = true;
        bool showPlain = EncryptionPasswordBox.Visibility == Visibility.Visible;

        if (showPlain)
        {
            EncryptionPasswordVisible.Text = EncryptionPasswordBox.Password;
            EncryptionPasswordBox.Visibility = Visibility.Collapsed;
            EncryptionPasswordVisible.Visibility = Visibility.Visible;
            EyeIcon.Text = "\uED1A";  // hide icon — password is visible
        }
        else
        {
            EncryptionPasswordBox.Password = EncryptionPasswordVisible.Text;
            EncryptionPasswordVisible.Visibility = Visibility.Collapsed;
            EncryptionPasswordBox.Visibility = Visibility.Visible;
            EyeIcon.Text = "\uE7B3";  // eye icon — password is hidden
        }
        _isSyncingPassword = false;
    }

    private void NumericOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
    }
}

