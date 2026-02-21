using CommunityToolkit.Mvvm.ComponentModel;
using EasySave.Core.Models;

namespace EasySave.VM.ViewModels;

public partial class SelectableBackupItem : ObservableObject
{
    public BackupWork Backup { get; }

    [ObservableProperty] private bool _isSelected;

    public SelectableBackupItem(BackupWork backup)
    {
        Backup = backup;
    }
}
