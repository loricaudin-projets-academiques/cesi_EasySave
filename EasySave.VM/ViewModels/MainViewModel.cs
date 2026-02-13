using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Localization;
using EasySave.Core.Models;
using EasySave.Core.Services;
using EasySave.Core.Settings;
using System.Collections.ObjectModel;

namespace EasySave.VM.ViewModels;

/// <summary>
/// Main application ViewModel - one VM per page pattern.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly Config _config;
    private readonly BackupWorkService _backupService;
    private readonly ILocalizationService _localization;

    /// <summary>Displayed backups list (direct models).</summary>
    public ObservableCollection<BackupWork> Backups { get; } = new();

    /// <summary>Available languages for ComboBox.</summary>
    public ObservableCollection<LanguageOption> AvailableLanguages { get; } = new()
    {
        new LanguageOption(Language.French, "Français"),
        new LanguageOption(Language.English, "English")
    };

    /// <summary>Selected language.</summary>
    [ObservableProperty]
    private LanguageOption _selectedLanguage = null!;

    /// <summary>Currently selected backup.</summary>
    [ObservableProperty]
    private BackupWork? _selectedBackup;

    /// <summary>Status message.</summary>
    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>Current backup progress (0-100).</summary>
    [ObservableProperty]
    private double _currentProgress;

    /// <summary>Indicates if a backup is running.</summary>
    [ObservableProperty]
    private bool _isRunning;

    /// <summary>Available log formats for ComboBox.</summary>
    public ObservableCollection<string> AvailableLogFormats { get; } = new()
    {
        "json",
        "xml"
    };

    /// <summary>Selected log format.</summary>
    [ObservableProperty]
    private string _selectedLogFormat = string.Empty;

    // ========== LOCALIZED TEXTS ==========
    
    [ObservableProperty] private string _refreshButtonText = string.Empty;
    [ObservableProperty] private string _runAllButtonText = string.Empty;
    [ObservableProperty] private string _selectAllButtonText = string.Empty;
    [ObservableProperty] private string _addButtonText = string.Empty;
    [ObservableProperty] private string _deleteButtonText = string.Empty;
    [ObservableProperty] private string _editButtonText = string.Empty;
    [ObservableProperty] private string _executeButtonText = string.Empty;
    [ObservableProperty] private string _languageLabel = string.Empty;
    [ObservableProperty] private string _logFormatLabel = string.Empty;
    [ObservableProperty] private string _aboutButtonText = string.Empty;

    public MainViewModel(Config config, BackupWorkService backupService, ILocalizationService localization)
    {
        _config = config;
        _backupService = backupService;
        _localization = localization;

        _selectedLanguage = AvailableLanguages.First(l => l.Value == _config.Language);
        _selectedLogFormat = AvailableLogFormats.FirstOrDefault(f => f == _config.LogType) ?? "json";
        UpdateLocalizedTexts();
        LoadBackups();
    }
    
    private void UpdateLocalizedTexts()
    {
        RefreshButtonText = _localization.Get("gui.buttons.refresh");
        SelectAllButtonText = _localization.Get("gui.buttons.select_all");
        AddButtonText = _localization.Get("gui.buttons.add");
        DeleteButtonText = _localization.Get("gui.buttons.delete");
        EditButtonText = _localization.Get("gui.buttons.edit");
        ExecuteButtonText = _localization.Get("gui.buttons.execute");
        RunAllButtonText = _localization.Get("gui.buttons.run_all");
        LanguageLabel = _localization.Get("gui.labels.language");
        LogFormatLabel = _localization.Get("gui.labels.log_format");
        AboutButtonText = _localization.Get("gui.buttons.about");
        StatusMessage = _localization.Get("gui.status.ready");
    }
    partial void OnSelectedLogFormatChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == _config.LogType)
            return;

        var valid = AvailableLogFormats.Contains(value);
        if (!valid)
        {
            StatusMessage = _localization.Get("gui.status.invalid_logtype", value, string.Join(", ", AvailableLogFormats));
            SelectedLogFormat = _config.LogType;
            return;
        }

        var oldType = _config.LogType;
        _config.LogType = value;
        _config.Save();
        StatusMessage = _localization.Get("gui.status.logtype_changed", oldType.ToUpperInvariant(), value.ToUpperInvariant());
    }

    partial void OnSelectedLanguageChanged(LanguageOption value)
    {
        if (value == null) return;

        _config.Language = value.Value;
        _config.Save();
        _localization.SetLanguage(value.Value);
        UpdateLocalizedTexts();
        StatusMessage = _localization.Get("gui.status.language_changed", value.DisplayName);
    }

    private void LoadBackups()
    {
        Backups.Clear();
        foreach (var work in _backupService.GetAllWorks())
        {
            Backups.Add(work);
        }
        StatusMessage = _localization.Get("gui.status.loaded", Backups.Count);
    }

    [RelayCommand]
    private void Refresh() => LoadBackups();

    /// <summary>
    /// Runs a specific backup (parameterized command).
    /// </summary>
    [RelayCommand]
    private async Task RunBackupAsync(BackupWork? backup)
    {
        if (backup == null || IsRunning) return;
        
        IsRunning = true;
        CurrentProgress = 0;
        StatusMessage = _localization.Get("gui.status.running_backup", backup.Name);

        try
        {
            await Task.Run(() => backup.Execute());
            CurrentProgress = 100;
            StatusMessage = _localization.Get("gui.status.backup_completed", backup.Name);
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
        StatusMessage = _localization.Get("gui.status.running");
        
        for (int i = 0; i < Backups.Count; i++)
        {
            CurrentProgress = (double)i / Backups.Count * 100;
            await Task.Run(() => Backups[i].Execute());
        }
        
        CurrentProgress = 100;
        StatusMessage = _localization.Get("gui.status.completed");
        IsRunning = false;
    }

    /// <summary>
    /// Deletes a backup (parameterized command).
    /// </summary>
    [RelayCommand]
    private void DeleteBackup(BackupWork? backup)
    {
        if (backup == null) return;
        
        var index = Backups.IndexOf(backup);
        if (index >= 0)
        {
            _backupService.RemoveWorkByIndex(index);
            LoadBackups();
            StatusMessage = _localization.Get("gui.status.deleted", backup.Name);
        }
    }

    [RelayCommand]
    private void About()
    {
        // TODO: Show about dialog
    }
}

/// <summary>
/// Language option for ComboBox binding.
/// </summary>
public class LanguageOption
{
    public Language Value { get; }
    public string DisplayName { get; }

    public LanguageOption(Language value, string displayName)
    {
        Value = value;
        DisplayName = displayName;
    }
}
