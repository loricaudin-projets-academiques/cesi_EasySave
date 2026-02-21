using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Localization;
using EasySave.Core.Models;
using EasySave.Core.Services;
using EasySave.Core.Services.Logging;
using EasySave.VM.Services;
using System.Collections.ObjectModel;

namespace EasySave.VM.ViewModels;

public partial class BackupListViewModel : ObservableObject, IBackupEventObserver
{
    private readonly BackupWorkService _backupService;
    private readonly BackupJobEngine _engine;
    private readonly ILocalizationService _localization;
    private readonly IShellNavigationService _navigation;
    private readonly IUiDispatcher _ui;
    private readonly IAppEvents _events;

    public ObservableCollection<SelectableBackupItem> Backups { get; } = new();

    [ObservableProperty] private SelectableBackupItem? _selectedBackup;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private double _currentProgress;
    [ObservableProperty] private bool _isRunning;

    // Inline delete confirmation (no popup)
    [ObservableProperty] private bool _isDeleteConfirmVisible;
    [ObservableProperty] private BackupWork? _pendingDeleteBackup;

    // Localized texts
    [ObservableProperty] private string _pageTitle = string.Empty;
    [ObservableProperty] private string _addButtonText = string.Empty;
    [ObservableProperty] private string _runAllButtonText = string.Empty;
    [ObservableProperty] private string _runSelectedButtonText = string.Empty;
    [ObservableProperty] private string _refreshButtonText = string.Empty;
    [ObservableProperty] private string _confirmDeleteText = string.Empty;
    [ObservableProperty] private string _cancelText = string.Empty;

    public BackupListViewModel(
        BackupWorkService backupService,
        BackupJobEngine engine,
        ILocalizationService localization,
        IShellNavigationService navigation,
        IUiDispatcher ui,
        IAppEvents events)
    {
        _backupService = backupService;
        _engine = engine;
        _localization = localization;
        _navigation = navigation;
        _ui = ui;
        _events = events;

        // Subscribe to engine events
        _engine.JobStateChanged += OnJobStateChanged;
        _engine.JobProgressChanged += OnJobProgressChanged;
        _engine.AllJobsCompleted += OnAllJobsCompleted;

        UpdateLocalizedTexts();
        Refresh();
        StatusMessage = _localization.Get("gui.status.ready");

        _events.LocalizationChanged += (_, __) => _ui.Invoke(UpdateLocalizedTexts);
    }

    public void Refresh()
    {
        Backups.Clear();
        foreach (var work in _backupService.GetAllWorks())
            Backups.Add(new SelectableBackupItem(work));

        StatusMessage = _localization.Get("gui.status.loaded", Backups.Count);
    }

    public void UpdateLocalizedTexts()
    {
        PageTitle = _localization.Get("gui.pages.backups_title");
        AddButtonText = _localization.Get("gui.buttons.add");
        RunAllButtonText = _localization.Get("gui.buttons.run_all");
        RunSelectedButtonText = _localization.Get("gui.buttons.run_selected");
        RefreshButtonText = _localization.Get("gui.buttons.refresh");
        ConfirmDeleteText = _localization.Get("gui.buttons.delete_confirm");
        CancelText = _localization.Get("gui.buttons.cancel");
    }

    // ===== Navigation / CRUD commands =====

    [RelayCommand]
    private void AddBackup()
    {
        if (IsRunning) return;
        _navigation.RequestNavigate(NavigationTarget.EditorCreate);
    }

    [RelayCommand]
    private void EditBackup(SelectableBackupItem? item)
    {
        if (item == null || IsRunning) return;
        var index = Backups.IndexOf(item);
        if (index < 0) return;
        _navigation.RequestNavigate(NavigationTarget.EditorEdit, index);
    }

    [RelayCommand]
    private void RequestDeleteBackup(SelectableBackupItem? item)
    {
        if (item == null || IsRunning) return;
        PendingDeleteBackup = item.Backup;
        IsDeleteConfirmVisible = true;
        StatusMessage = _localization.Get("gui.status.delete_confirm", item.Backup.Name);
    }

    [RelayCommand]
    private void CancelDelete()
    {
        PendingDeleteBackup = null;
        IsDeleteConfirmVisible = false;
        StatusMessage = _localization.Get("gui.status.ready");
    }

    [RelayCommand]
    private void ConfirmDelete()
    {
        if (PendingDeleteBackup == null || IsRunning) return;

        var item = Backups.FirstOrDefault(b => b.Backup == PendingDeleteBackup);
        var index = item != null ? Backups.IndexOf(item) : -1;
        if (index >= 0)
        {
            _backupService.RemoveWorkByIndex(index);
            StatusMessage = _localization.Get("gui.status.deleted", PendingDeleteBackup.Name);
        }

        PendingDeleteBackup = null;
        IsDeleteConfirmVisible = false;
        Refresh();
    }

    [RelayCommand]
    private void RefreshList()
    {
        if (IsRunning) return;
        Refresh();
    }

    // ===== Parallel execution commands =====

    [RelayCommand]
    private async Task RunAllAsync()
    {
        if (IsRunning) return;
        await LaunchJobs(Enumerable.Range(0, Backups.Count));
    }

    [RelayCommand]
    private async Task RunSelectedAsync()
    {
        if (IsRunning) return;
        var indices = Backups
            .Select((b, i) => (b, i))
            .Where(x => x.b.IsSelected)
            .Select(x => x.i)
            .ToList();
        if (indices.Count == 0) return;
        await LaunchJobs(indices);
    }

    private async Task LaunchJobs(IEnumerable<int> indices)
    {
        IsRunning = true;
        CurrentProgress = 0;
        StatusMessage = _localization.Get("gui.status.running");

        try
        {
            await _engine.RunJobsAsync(indices);
        }
        catch (Exception ex)
        {
            StatusMessage = _localization.Get("gui.status.error", ex.Message);
        }
    }

    // ===== Per-item Play/Pause toggle + Stop =====

    [RelayCommand]
    private void TogglePlayPause(SelectableBackupItem? item)
    {
        if (item == null) return;
        var index = Backups.IndexOf(item);
        var runner = _engine.GetRunner(index);

        // Not yet launched, or finished (Done/Stopped/Error) → (re)launch
        if (runner == null || runner.State == JobState.Done || runner.State == JobState.Stopped || runner.State == JobState.Error)
        {
            item.JobState = JobState.Idle;
            item.Progress = 0;
            _ = LaunchJobs(new[] { index });
            return;
        }

        switch (runner.State)
        {
            case JobState.Running:
                runner.Pause();
                break;
            case JobState.Paused:
                runner.Resume();
                break;
        }
    }

    [RelayCommand]
    private void StopJob(SelectableBackupItem? item)
    {
        if (item == null) return;
        var index = Backups.IndexOf(item);
        _engine.GetRunner(index)?.Stop();
    }

    // ===== Global controls =====

    [RelayCommand]
    private void PauseAll() => _engine.PauseAll();

    [RelayCommand]
    private void ResumeAll() => _engine.ResumeAll();

    [RelayCommand]
    private void StopAll() => _engine.StopAll();

    // ===== Engine event handlers (called from background threads) =====

    private void OnJobStateChanged(BackupJobRunner runner)
    {
        _ui.Invoke(() =>
        {
            var item = Backups.ElementAtOrDefault(runner.Index);
            if (item != null)
                item.JobState = runner.State;

            // Update global status message
            StatusMessage = runner.State switch
            {
                JobState.Running => _localization.Get("gui.status.running_backup", runner.Name),
                JobState.Paused => _localization.Get("gui.status.paused", runner.Name),
                JobState.Stopped => _localization.Get("gui.status.stopped", runner.Name),
                JobState.Done => _localization.Get("gui.status.backup_completed", runner.Name),
                JobState.Error => _localization.Get("gui.status.error", runner.Name),
                _ => StatusMessage
            };
        });
    }

    private void OnJobProgressChanged(BackupJobRunner runner, double progress)
    {
        _ui.Invoke(() =>
        {
            var item = Backups.ElementAtOrDefault(runner.Index);
            if (item != null)
                item.Progress = progress;

            // Update global progress = average of all runners
            CurrentProgress = _engine.GlobalProgress;
        });
    }

    private void OnAllJobsCompleted()
    {
        _ui.Invoke(() =>
        {
            IsRunning = false;
            CurrentProgress = 100;
            StatusMessage = _localization.Get("gui.status.completed");
        });
    }

    // ===== IBackupEventObserver (kept for logging observer compatibility) =====

    public void OnBackupStarted(string backupName, long totalFiles, long totalSize) { }
    public void OnProgressUpdated(string backupName, string currentFile, string targetFile, long filesLeft, long sizeLeft, double progression) { }
    public void OnBackupCompleted(string backupName) { }
    public void OnBackupError(string backupName, Exception ex) { }
    public void OnBackupPaused(string backupName) { }
    public void OnBackupResumed(string backupName) { }
    public void OnFileTransferred(string backupName, string sourceFile, string targetFile, long fileSize, double transferTimeMs) { }
    public void OnFileTransferError(string backupName, string sourceFile, string targetFile, long fileSize, Exception ex) { }
}

