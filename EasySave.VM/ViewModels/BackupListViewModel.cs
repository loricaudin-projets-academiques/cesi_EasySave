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
    private readonly ILocalizationService _localization;
    private readonly IShellNavigationService _navigation;
    private readonly IUiDispatcher _ui;
    private readonly IAppEvents _events;

    public ObservableCollection<BackupWork> Backups { get; } = new();

    [ObservableProperty] private BackupWork? _selectedBackup;
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
    [ObservableProperty] private string _refreshButtonText = string.Empty;
    [ObservableProperty] private string _confirmDeleteText = string.Empty;
    [ObservableProperty] private string _cancelText = string.Empty;

    public BackupListViewModel(
        BackupWorkService backupService,
        ILocalizationService localization,
        IShellNavigationService navigation,
        IUiDispatcher ui,
        IAppEvents events)
    {
        _backupService = backupService;
        _localization = localization;
        _navigation = navigation;
        _ui = ui;
        _events = events;

        UpdateLocalizedTexts();
        Refresh();
        StatusMessage = _localization.Get("gui.status.ready");

        _events.LocalizationChanged += (_, __) => _ui.Invoke(UpdateLocalizedTexts);
    }

    public void Refresh()
    {
        Backups.Clear();
        foreach (var work in _backupService.GetAllWorks())
            Backups.Add(work);

        StatusMessage = _localization.Get("gui.status.loaded", Backups.Count);
    }

    public void UpdateLocalizedTexts()
    {
        PageTitle = _localization.Get("gui.pages.backups_title");
        AddButtonText = _localization.Get("gui.buttons.add");
        RunAllButtonText = _localization.Get("gui.buttons.run_all");
        RefreshButtonText = _localization.Get("gui.buttons.refresh");
        ConfirmDeleteText = _localization.Get("gui.buttons.delete_confirm");
        CancelText = _localization.Get("gui.buttons.cancel");
    }

    [RelayCommand]
    private void AddBackup()
    {
        if (IsRunning) return;
        _navigation.RequestNavigate(NavigationTarget.EditorCreate);
    }

    [RelayCommand]
    private void EditBackup(BackupWork? backup)
    {
        if (backup == null || IsRunning) return;
        var index = Backups.IndexOf(backup);
        if (index < 0) return;
        _navigation.RequestNavigate(NavigationTarget.EditorEdit, index);
    }

    [RelayCommand]
    private void RequestDeleteBackup(BackupWork? backup)
    {
        if (backup == null || IsRunning) return;
        PendingDeleteBackup = backup;
        IsDeleteConfirmVisible = true;
        StatusMessage = _localization.Get("gui.status.delete_confirm", backup.Name);
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

        var index = Backups.IndexOf(PendingDeleteBackup);
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

    [RelayCommand]
    private async Task RunBackupAsync(BackupWork? backup)
    {
        if (backup == null || IsRunning) return;
        var index = Backups.IndexOf(backup);
        if (index < 0) return;

        IsRunning = true;
        CurrentProgress = 0;
        StatusMessage = _localization.Get("gui.status.running_backup", backup.Name);

        try
        {
            await Task.Run(() => _backupService.ExecuteWork(index));
        }
        catch (BusinessSoftwareRunningException ex)
        {
            StatusMessage = _localization.Get("gui.status.blocked_business", ex.SoftwareName);
        }
        catch (Exception ex)
        {
            StatusMessage = _localization.Get("gui.status.error", ex.Message);
        }
        finally
        {
            IsRunning = false;
        }
    }

    [RelayCommand]
    private async Task RunAllAsync()
    {
        if (IsRunning) return;

        IsRunning = true;
        CurrentProgress = 0;
        StatusMessage = _localization.Get("gui.status.running");

        try
        {
            for (int i = 0; i < Backups.Count; i++)
            {
                var name = Backups[i].Name;
                StatusMessage = _localization.Get("gui.status.running_backup", name);
                await Task.Run(() => _backupService.ExecuteWork(i));
            }

            CurrentProgress = 100;
            StatusMessage = _localization.Get("gui.status.completed");
        }
        catch (BusinessSoftwareRunningException ex)
        {
            StatusMessage = _localization.Get("gui.status.blocked_business", ex.SoftwareName);
        }
        catch (Exception ex)
        {
            StatusMessage = _localization.Get("gui.status.error", ex.Message);
        }
        finally
        {
            IsRunning = false;
        }
    }

    // ===== Observer callbacks (called from background threads) =====

    public void OnBackupStarted(string backupName, long totalFiles, long totalSize)
    {
        _ui.Invoke(() =>
        {
            CurrentProgress = 0;
            StatusMessage = _localization.Get("gui.status.running_backup", backupName);
        });
    }

    public void OnProgressUpdated(string backupName, string currentFile, string targetFile, long filesLeft, long sizeLeft, double progression)
    {
        _ui.Invoke(() =>
        {
            CurrentProgress = progression;
        });
    }

    public void OnBackupCompleted(string backupName)
    {
        _ui.Invoke(() =>
        {
            CurrentProgress = 100;
            StatusMessage = _localization.Get("gui.status.backup_completed", backupName);
        });
    }

    public void OnBackupError(string backupName, Exception ex)
    {
        _ui.Invoke(() =>
        {
            StatusMessage = _localization.Get("gui.status.error", ex.Message);
        });
    }

    public void OnBackupPaused(string backupName)
    {
        _ui.Invoke(() =>
        {
            StatusMessage = _localization.Get("gui.status.paused", backupName);
        });
    }

    public void OnBackupResumed(string backupName)
    {
        _ui.Invoke(() =>
        {
            StatusMessage = _localization.Get("gui.status.running_backup", backupName);
        });
    }

    public void OnFileTransferred(string backupName, string sourceFile, string targetFile, long fileSize, double transferTimeMs) { }
    public void OnFileTransferError(string backupName, string sourceFile, string targetFile, long fileSize, Exception ex) { }
}

