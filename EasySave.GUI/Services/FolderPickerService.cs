using EasySave.VM.Services;
using System.Windows.Forms;

namespace EasySave.GUI.Services;

public sealed class FolderPickerService : IFolderPickerService
{
    public string? PickFolder(string? initialDirectory = null)
    {
        using var dialog = new FolderBrowserDialog
        {
            UseDescriptionForTitle = true,
            Description = "Select a folder",
            SelectedPath = string.IsNullOrWhiteSpace(initialDirectory) ? string.Empty : initialDirectory
        };

        var result = dialog.ShowDialog();
        if (result != DialogResult.OK)
            return null;

        return string.IsNullOrWhiteSpace(dialog.SelectedPath) ? null : dialog.SelectedPath;
    }
}

