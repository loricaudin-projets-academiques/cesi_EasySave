using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Localization;
using EasySave.Core.Models;
using EasySave.Core.Services;
using EasySave.Core.Services.Logging;
using EasySave.Core.ProgressBar;
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

    // ========== CONTRÔLE TEMPS RÉEL ==========
    private readonly Dictionary<BackupWorkViewModel, CancellationTokenSource> _runningBackups = new();
    private CancellationTokenSource? _globalCancellationToken;

    /// <summary>Liste des backups avec contrôle individuel.</summary>
    public ObservableCollection<BackupWorkViewModel> Backups { get; } = new();

    [ObservableProperty] private BackupWorkViewModel? _selectedBackup;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private double _currentProgress;
    [ObservableProperty] private bool _isRunning;

    // Inline delete confirmation (no popup)
    [ObservableProperty] private bool _isDeleteConfirmVisible;
    [ObservableProperty] private BackupWorkViewModel? _pendingDeleteBackup;

    // Localized texts
    [ObservableProperty] private string _pageTitle = string.Empty;
    [ObservableProperty] private string _addButtonText = string.Empty;
    [ObservableProperty] private string _runAllButtonText = string.Empty;
    [ObservableProperty] private string _pauseAllButtonText = string.Empty;
    [ObservableProperty] private string _stopAllButtonText = string.Empty;
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

        // S'enregistrer comme observer
        _backupService.AddObserver(this);

        UpdateLocalizedTexts();
        Refresh();
        StatusMessage = _localization.Get("gui.status.ready");

        _events.LocalizationChanged += (_, __) => _ui.Invoke(UpdateLocalizedTexts);
    }

    public void Refresh()
    {
        Backups.Clear();
        var works = _backupService.GetAllWorks();

        for (int i = 0; i < works.Count; i++)
        {
            var work = works[i];
            Backups.Add(new BackupWorkViewModel
            {
                Index = i,
                Name = work.Name,
                SourcePath = work.SourcePath,
                DestinationPath = work.DestinationPath,
                Type = _backupService.GetLocalizedBackupTypeName(work.GetBackupType()),
                IsRunning = false,
                IsPaused = false,
                Progress = 0
            });
        }

        StatusMessage = _localization.Get("gui.status.loaded", Backups.Count);
    }

    public void UpdateLocalizedTexts()
    {
        PageTitle = _localization.Get("gui.pages.backups_title");
        AddButtonText = _localization.Get("gui.buttons.add");
        RunAllButtonText = _localization.Get("gui.buttons.run_all");
        PauseAllButtonText = "⏸ Pause All";
        StopAllButtonText = "⏹ Stop All";
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
    private void EditBackup(BackupWorkViewModel? backup)
    {
        if (backup == null || IsRunning) return;
        _navigation.RequestNavigate(NavigationTarget.EditorEdit, backup.Index);
    }

    [RelayCommand]
    private void RequestDeleteBackup(BackupWorkViewModel? backup)
    {
        if (backup == null || IsRunning || backup.IsRunning) return;
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
        if (PendingDeleteBackup == null || IsRunning || PendingDeleteBackup.IsRunning) return;

        _backupService.RemoveWorkByIndex(PendingDeleteBackup.Index);
        StatusMessage = _localization.Get("gui.status.deleted", PendingDeleteBackup.Name);

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

    // ============================================================
    // ============ CONTRÔLE INDIVIDUEL D'UN BACKUP ============
    // ============================================================

    /// <summary>
    /// Lance un backup individuel avec contrôle temps réel.
    /// </summary>
    [RelayCommand]
    private async Task RunBackupAsync(BackupWorkViewModel? backup)
    {
        if (backup == null || backup.IsRunning) return;

        backup.IsRunning = true;
        backup.IsPaused = false;
        backup.Progress = 0;

        var cts = new CancellationTokenSource();
        _runningBackups[backup] = cts;

        StatusMessage = _localization.Get("gui.status.running_backup", backup.Name);

        try
        {
            var work = _backupService.GetWorkByIndex(backup.Index);
            if (work == null)
            {
                StatusMessage = "Backup not found";
                return;
            }

            // Écoute les événements de progression
            work.FileProgress += (s, e) =>
            {
                if (e is FileProgressEventArgs progress)
                {
                    _ui.Invoke(() =>
                    {
                        backup.Progress = progress.CurrentProgress;
                        backup.CurrentFile = progress.SourceFile;
                        CurrentProgress = progress.CurrentProgress;
                    });
                }
            };

            // Lance le backup
            await Task.Run(() => _backupService.ExecuteWork(backup.Index), cts.Token);

            StatusMessage = _localization.Get("gui.status.backup_completed", backup.Name);
            backup.Progress = 100;
        }
        catch (OperationCanceledException)
        {
            StatusMessage = $"{backup.Name} stopped";
            backup.Progress = 0;
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
            backup.IsRunning = false;
            backup.IsPaused = false;
            _runningBackups.Remove(backup);
        }
    }

    /// <summary>
    /// Met en pause un backup individuel.
    /// La pause prend effet APRÈS le transfert du fichier en cours.
    /// </summary>
    [RelayCommand]
    private void PauseBackup(BackupWorkViewModel? backup)
    {
        if (backup == null || !backup.IsRunning) return;

        backup.IsPaused = !backup.IsPaused;
        StatusMessage = backup.IsPaused
            ? $"{backup.Name} paused (will finish current file)"
            : $"{backup.Name} resumed";
    }

    /// <summary>
    /// Arrête immédiatement un backup individuel.
    /// </summary>
    [RelayCommand]
    private void StopBackup(BackupWorkViewModel? backup)
    {
        if (backup == null || !backup.IsRunning) return;

        if (_runningBackups.TryGetValue(backup, out var cts))
        {
            cts.Cancel();
            StatusMessage = $"Stopping {backup.Name}...";
        }
    }

    // ============================================================
    // ============ CONTRÔLE GLOBAL (TOUS LES BACKUPS) ============
    // ============================================================

    /// <summary>
    /// Lance tous les backups en parallèle.
    /// </summary>
    [RelayCommand]
    private async Task RunAllAsync()
    {
        if (Backups.Any(b => b.IsRunning))
        {
            StatusMessage = "Some backups are already running";
            return;
        }

        IsRunning = true;
        _globalCancellationToken = new CancellationTokenSource();
        CurrentProgress = 0;
        StatusMessage = _localization.Get("gui.status.running");

        try
        {
            // Lance tous les backups en parallèle
            var tasks = Backups.Select(b => RunBackupAsync(b)).ToArray();
            await Task.WhenAll(tasks);

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

    /// <summary>
    /// Met en pause tous les backups en cours.
    /// </summary>
    [RelayCommand]
    private void PauseAll()
    {
        foreach (var backup in Backups.Where(b => b.IsRunning))
        {
            PauseBackup(backup);
        }
        StatusMessage = "All backups paused";
    }

    /// <summary>
    /// Arrête immédiatement tous les backups en cours.
    /// </summary>
    [RelayCommand]
    private void StopAll()
    {
        _globalCancellationToken?.Cancel();

        foreach (var backup in Backups.Where(b => b.IsRunning).ToList())
        {
            StopBackup(backup);
        }

        StatusMessage = "All backups stopped";
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

            // Met à jour le backup correspondant
            var backup = Backups.FirstOrDefault(b => b.Name == backupName);
            if (backup != null)
            {
                backup.Progress = progression;
                backup.CurrentFile = currentFile;
            }
        });
    }

    public void OnBackupCompleted(string backupName)
    {
        _ui.Invoke(() =>
        {
            var backup = Backups.FirstOrDefault(b => b.Name == backupName);
            if (backup != null)
            {
                backup.Progress = 100;
                backup.IsRunning = false;
            }
            CurrentProgress = 100;
            StatusMessage = _localization.Get("gui.status.backup_completed", backupName);
        });
    }

    public void OnBackupError(string backupName, Exception ex)
    {
        _ui.Invoke(() =>
        {
            var backup = Backups.FirstOrDefault(b => b.Name == backupName);
            if (backup != null)
            {
                backup.IsRunning = false;
            }
            StatusMessage = _localization.Get("gui.status.error", ex.Message);
        });
    }

    public void OnBackupPaused(string backupName)
    {
        _ui.Invoke(() =>
        {
            var backup = Backups.FirstOrDefault(b => b.Name == backupName);
            if (backup != null)
            {
                backup.IsPaused = true;
            }
            StatusMessage = _localization.Get("gui.status.paused", backupName);
        });
    }

    public void OnBackupResumed(string backupName)
    {
        _ui.Invoke(() =>
        {
            var backup = Backups.FirstOrDefault(b => b.Name == backupName);
            if (backup != null)
            {
                backup.IsPaused = false;
            }
            StatusMessage = _localization.Get("gui.status.running_backup", backupName);
        });
    }

    public void OnFileTransferred(string backupName, string sourceFile, string targetFile, long fileSize, double transferTimeMs) { }
    public void OnFileTransferError(string backupName, string sourceFile, string targetFile, long fileSize, Exception ex) { }
}

// ============================================================
// ============ VIEWMODEL D'UN BACKUP INDIVIDUEL ============
// ============================================================

/// <summary>
/// ViewModel pour un backup individuel avec état temps réel.
/// </summary>
public partial class BackupWorkViewModel : ObservableObject
{
    [ObservableProperty] private int _index;
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private string _sourcePath = "";
    [ObservableProperty] private string _destinationPath = "";
    [ObservableProperty] private string _type = "";
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private bool _isPaused;
    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _currentFile = "";
}