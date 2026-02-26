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
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private bool _isBusinessBlocked;

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
        _engine.JobInfoChanged += OnJobInfoChanged;
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
            Backups.Add(new SelectableBackupItem(work, _localization));

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
        if (IsRunning || IsBusinessBlocked) return;
        await LaunchJobs(Enumerable.Range(0, Backups.Count));
    }

    [RelayCommand]
    private async Task RunSelectedAsync()
    {
        if (IsBusinessBlocked) return;
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
        if (item == null || IsBusinessBlocked) return;
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
            case JobState.Pausing:
                // Already pausing, ignore
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
    private void PauseAll()
    {
        if (!IsBusinessBlocked) _engine.PauseAll();
    }

    [RelayCommand]
    private void ResumeAll()
    {
        if (!IsBusinessBlocked) _engine.ResumeAll();
    }

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

            // Reset individual progress bar on stop/error
            if (item != null && (runner.State == JobState.Stopped || runner.State == JobState.Error))
                item.Progress = 0;

            // Update global status message
            StatusMessage = runner.State switch
            {
                JobState.Running => _localization.Get("gui.status.running_backup", runner.Name),
                JobState.Pausing => _localization.Get("gui.status.pausing", runner.Name),
                JobState.Paused => _localization.Get("gui.status.paused", runner.Name),
                JobState.Stopped => _localization.Get("gui.status.stopped", runner.Name),
                JobState.Done => _localization.Get("gui.status.backup_completed", runner.Name),
                JobState.Error => _localization.Get("gui.status.error", runner.Name),
                _ => StatusMessage
            };

            // Track business software blocking
            IsBusinessBlocked = _engine.IsAnyBusinessBlocked;

            // Propagate global block to ALL items so every Play button is greyed
            foreach (var b in Backups)
                b.IsGloballyBlocked = IsBusinessBlocked;

            if (runner.IsBusinessBlocked && item != null)
            {
                item.BlockReason = BlockReason.BusinessSoftware;
                StatusMessage = _localization.Get("gui.status.blocked_business", _engine.BusinessSoftwareName ?? "");
            }
            else if (!runner.IsBusinessBlocked && item != null && item.BlockReason == BlockReason.BusinessSoftware)
            {
                item.BlockReason = BlockReason.None;
            }

            // Track IsRunning based on whether any job is still active
            IsRunning = _engine.IsAnyActive;
        });
    }

    private void OnJobProgressChanged(BackupJobRunner runner, double progress)
    {
        _ui.Invoke(() =>
        {
            // Ignore late progress events from stopped/error runners
            if (runner.State == JobState.Stopped || runner.State == JobState.Error)
                return;

            var item = Backups.ElementAtOrDefault(runner.Index);
            if (item != null)
                item.Progress = progress;

        });
    }

    private void OnJobInfoChanged(BackupJobRunner runner)
    {
        _ui.Invoke(() =>
        {
            var item = Backups.ElementAtOrDefault(runner.Index);
            if (item == null) return;

            item.CurrentFile = runner.CurrentFile;
            item.BlockReason = runner.CurrentBlockReason;
        });
    }

    private void OnAllJobsCompleted()
    {
        _ui.Invoke(() =>
        {
            IsRunning = false;
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

