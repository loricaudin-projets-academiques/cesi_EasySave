using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Localization;
using EasySave.Core.Models;
using EasySave.Core.Services;
using System.IO;

namespace EasySave.VM.ViewModels;

public partial class SelectableBackupItem : ObservableObject
{
    public BackupWork Backup { get; }
    private readonly ILocalizationService? _localization;

    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private double _progress;
    [ObservableProperty] private JobState _jobState = JobState.Idle;
    [ObservableProperty] private string _currentFile = string.Empty;
    [ObservableProperty] private BlockReason _blockReason = BlockReason.None;

    /// <summary>
    /// Icon for the toggle button: Play → Pause → Play (resume).
    /// Segoe MDL2 Assets: E768=Play, E769=Pause.
    /// </summary>
    public string ToggleIcon => JobState switch
    {
        JobState.Running => "\uE769",   // Pause icon
        JobState.Pausing => "\uE769",   // Pause icon (still finishing file)
        _ => "\uE768"                    // Play icon
    };

    /// <summary>
    /// Whether the stop button should be enabled (only when active).
    /// </summary>
    public bool IsStopVisible => JobState == JobState.Running || JobState == JobState.Paused || JobState == JobState.Pausing;

    /// <summary>
    /// Whether this job is currently blocked (priority or large file), preventing Play interactions.
    /// </summary>
    public bool IsBlocked => BlockReason != BlockReason.None;

    /// <summary>
    /// Whether the Play/Pause button should be enabled.
    /// Disabled when: pausing (waiting for current file), or blocked by system (priority/large file/business).
    /// </summary>
    public bool IsPlayPauseEnabled => JobState != JobState.Pausing && BlockReason == BlockReason.None;

    /// <summary>
    /// Short display name for the current file (filename only).
    /// </summary>
    public string CurrentFileName => string.IsNullOrEmpty(CurrentFile) ? "" : Path.GetFileName(CurrentFile);

    /// <summary>
    /// Localized display text for the current job state.
    /// </summary>
    public string StateDisplayText => JobState switch
    {
        JobState.Idle => _localization?.Get("gui.job_state.idle") ?? "—",
        JobState.Running => BlockReason switch
        {
            BlockReason.PriorityFile => _localization?.Get("gui.job_state.waiting_priority") ?? "Waiting (priority)",
            BlockReason.LargeFile => _localization?.Get("gui.job_state.waiting_large_file") ?? "Waiting (large file)",
            _ => _localization?.Get("gui.job_state.running") ?? "Running"
        },
        JobState.Pausing => _localization?.Get("gui.job_state.pausing") ?? "Pausing...",
        JobState.Paused => BlockReason switch
        {
            BlockReason.BusinessSoftware => _localization?.Get("gui.job_state.blocked_business") ?? "Blocked (software)",
            _ => _localization?.Get("gui.job_state.paused") ?? "Paused"
        },
        JobState.Stopped => _localization?.Get("gui.job_state.stopped") ?? "Stopped",
        JobState.Done => _localization?.Get("gui.job_state.done") ?? "Completed",
        JobState.Error => _localization?.Get("gui.job_state.error") ?? "Error",
        _ => "—"
    };

    /// <summary>
    /// Hex color for the state badge background.
    /// </summary>
    public string StateColor => JobState switch
    {
        JobState.Running => BlockReason != BlockReason.None ? "#FFF3E0" : "#E8F5E9",
        JobState.Pausing => "#FFF3E0",
        JobState.Paused => "#FFF3E0",
        JobState.Stopped => "#FFEBEE",
        JobState.Done => "#E3F2FD",
        JobState.Error => "#FFEBEE",
        _ => "#F5F5F5"
    };

    /// <summary>
    /// Hex color for the state badge text.
    /// </summary>
    public string StateTextColor => JobState switch
    {
        JobState.Running => BlockReason != BlockReason.None ? "#E65100" : "#2E7D32",
        JobState.Pausing => "#E65100",
        JobState.Paused => "#E65100",
        JobState.Stopped => "#C62828",
        JobState.Done => "#1565C0",
        JobState.Error => "#C62828",
        _ => "#757575"
    };

    /// <summary>
    /// Localized display name for the backup type.
    /// </summary>
    public string TypeDisplayName => Backup.Type == BackupType.FULL_BACKUP
        ? (_localization?.Get("backup_types.full") ?? "Full")
        : (_localization?.Get("backup_types.diff") ?? "Differential");

    public SelectableBackupItem(BackupWork backup, ILocalizationService? localization = null)
    {
        Backup = backup;
        _localization = localization;
    }

    partial void OnJobStateChanged(JobState value)
    {
        OnPropertyChanged(nameof(ToggleIcon));
        OnPropertyChanged(nameof(IsStopVisible));
        OnPropertyChanged(nameof(IsBlocked));
        OnPropertyChanged(nameof(IsPlayPauseEnabled));
        OnPropertyChanged(nameof(StateDisplayText));
        OnPropertyChanged(nameof(StateColor));
        OnPropertyChanged(nameof(StateTextColor));
    }

    partial void OnCurrentFileChanged(string value)
    {
        OnPropertyChanged(nameof(CurrentFileName));
    }

    partial void OnBlockReasonChanged(BlockReason value)
    {
        OnPropertyChanged(nameof(IsBlocked));
        OnPropertyChanged(nameof(IsPlayPauseEnabled));
        OnPropertyChanged(nameof(StateDisplayText));
        OnPropertyChanged(nameof(StateColor));
        OnPropertyChanged(nameof(StateTextColor));
    }
}
