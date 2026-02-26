using System.Text.RegularExpressions;
using System.Windows.Input;

namespace EasySave.GUI.Views.Pages;

public partial class SettingsView : System.Windows.Controls.UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void NumericOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
    }
}

