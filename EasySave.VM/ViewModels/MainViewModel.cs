using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Localization;
using EasySave.Core.Models;
using EasySave.Core.Services;
using EasySave.Core.Settings;
using System.Collections.ObjectModel;

namespace EasySave.VM.ViewModels;

/// <summary>
/// Main application ViewModel.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly Config _config;
    private readonly BackupWorkService _backupService;
    private readonly ILocalizationService _localization;

    /// <summary>
    /// Displayed backups list.
    /// </summary>
    public ObservableCollection<BackupWorkViewModel> Backups { get; } = new();

    /// <summary>
    /// Available languages for ComboBox.
    /// </summary>
    public ObservableCollection<LanguageOption> AvailableLanguages { get; } = new()
    {
        new LanguageOption(Language.French, "Français"),
        new LanguageOption(Language.English, "English")
    };

    /// <summary>
    /// Selected language - bound to ComboBox.
    /// </summary>
    [ObservableProperty]
    private LanguageOption _selectedLanguage = null!;

    /// <summary>
    /// Currently selected backup.
    /// </summary>
    [ObservableProperty]
    private BackupWorkViewModel? _selectedBackup;

    /// <summary>
    /// Status message displayed to user.
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // ========== TEXTES LOCALISÉS (bindés dans le XAML) ==========
    
    [ObservableProperty]
    private string _refreshButtonText = "Refresh";

    [ObservableProperty]
    private string _runAllButtonText = "Run All";

    [ObservableProperty]
    private string _selectAllButtonText = "Tout sélectionner";

    [ObservableProperty]
    private string _addButtonText = "Ajouter";

    [ObservableProperty]
    private string _deleteSelectionText = "Supprimer la sélection";

    [ObservableProperty]
    private string _runAllButtonText = "Tout exécuter";

    [ObservableProperty]
    private string _languageLabel = "Language:";

    [ObservableProperty]
    private string _logFormatLabel = "Format des logs :";

    [ObservableProperty]
    private string _aboutButtonText = "À propos";

    //[ObservableProperty]
    //private string _executeButtonText = "Exécuter";

    //[ObservableProperty]
    //private string _editButtonText = "Modifier";

    //[ObservableProperty]
    //private string _deleteButtonText = "Supprimer";

    public MainViewModel(Config config, BackupWorkService backupService, ILocalizationService localization)
    {
        _config = config;
        _backupService = backupService;
        _localization = localization;

        _selectedLanguage = AvailableLanguages.First(l => l.Value == _config.Language);
        UpdateLocalizedTexts();
        LoadBackups();
    }
    
    /// <summary>
    /// Updates all texts when language changes.
    /// </summary>
    private void UpdateLocalizedTexts()
    {
        SelectAllButtonText = _localization.Get("gui.buttons.select_all");
        AddButtonText = _localization.Get("gui.buttons.add");
        DeleteSelectionText = _localization.Get("gui.buttons.delete_selection");
        RunAllButtonText = _localization.Get("gui.buttons.run_section");
        LanguageLabel = _localization.Get("gui.labels.language");
        LogFormatLabel = _localization.Get("gui.labels.log_format");
        StatusMessage = _localization.Get("gui.status.ready");
        AboutButtonText = _localization.Get("gui.buttons.about");
        _selectedBackup.ExecuteButtonText = _localization.Get("gui.buttons.execute");
        _selectedBackup.EditButtonText = _localization.Get("gui.buttons.execute");
        _selectedBackup.DeleteButtonText = _localization.Get("gui.buttons.execute");
        //ExecuteButtonText = _localization.Get("gui.buttons.execute");
        //EditButtonText = _localization.Get("gui.buttons.edit");
        //DeleteButtonText = _localization.Get("gui.buttons.delete");
    }

    /// <summary>
    /// Called automatically when SelectedLanguage changes via binding.
    /// </summary>
    partial void OnSelectedLanguageChanged(LanguageOption value)
    {
        if (value == null) return;

        // 1. Save to config
        _config.Language = value.Value;
        _config.Save();

        // 2. Change service language
        _localization.SetLanguage(value.Value);

        // 3. Refresh UI
        UpdateLocalizedTexts();
        //SelectedBackup.UpdateLocalizedTexts();

        // 4. Confirmation message
        StatusMessage = _localization.Get("gui.status.language_changed", value.DisplayName);
    }

    /// <summary>
    /// Loads backups from service.
    /// </summary>
    private void LoadBackups()
    {
        Backups.Clear();
        foreach (var work in _backupService.GetAllWorks())
        {
            Backups.Add(new BackupWorkViewModel(work));
        }
        StatusMessage = _localization.Get("gui.status.loaded", Backups.Count);
    }

    /// <summary>
    /// Refreshes the backup list.
    /// </summary>
    [RelayCommand]
    private void Refresh()
    {
        LoadBackups();
    }

    /// <summary>
    /// Runs all backups sequentially.
    /// </summary>
    [RelayCommand]
    private async Task RunAllAsync()
    {
        StatusMessage = _localization.Get("gui.status.running");
        
        for (int i = 0; i < Backups.Count; i++)
        {
            await Backups[i].RunAsync();
        }
        
        StatusMessage = _localization.Get("gui.status.completed");
        LoadBackups();
    }

    /// <summary>
    /// Affiche la page "à propos".
    /// </summary>
    [RelayCommand]
    private async Task AboutAsync()
    {
        //_dialogService.ShowAbout();
    }
}

/// <summary>
/// Language option for ComboBox.
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
