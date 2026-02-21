using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Models;
using EasySave.Core.Services;

namespace EasySave.VM.ViewModels;

public partial class SelectableBackupItem : ObservableObject
{
    public BackupWork Backup { get; }

    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private double _progress;
    [ObservableProperty] private JobState _jobState = JobState.Idle;

    /// <summary>
    /// Icon for the toggle button: Play → Pause → Play (resume).
    /// Segoe MDL2 Assets: E768=Play, E769=Pause.
    /// </summary>
    public string ToggleIcon => JobState switch
    {
        JobState.Running => "\uE769",  // Pause icon
        _ => "\uE768"                   // Play icon
    };

    /// <summary>
    /// Whether the stop button should be enabled (only when running or paused).
    /// </summary>
    public bool IsStopVisible => JobState == JobState.Running || JobState == JobState.Paused;

    public SelectableBackupItem(BackupWork backup)
    {
        Backup = backup;
    }

    partial void OnJobStateChanged(JobState value)
    {
        OnPropertyChanged(nameof(ToggleIcon));
        OnPropertyChanged(nameof(IsStopVisible));
    }
}
