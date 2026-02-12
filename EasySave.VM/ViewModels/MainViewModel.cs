using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Localization;
using EasySave.Core.Models;
using EasySave.Core.Settings;
using System.Collections.ObjectModel;

namespace EasySave.VM.ViewModels;

/// <summary>
/// ViewModel principal de l'application.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly BackupWorkList _backupWorkList;
    private readonly Config _config;
    private readonly ILocalizationService _localization;

    /// <summary>
    /// Liste des sauvegardes affichées.
    /// </summary>
    public ObservableCollection<BackupWorkViewModel> Backups { get; } = new();

    /// <summary>
    /// Langues disponibles pour le ComboBox.
    /// </summary>
    public ObservableCollection<LanguageOption> AvailableLanguages { get; } = new()
    {
        new LanguageOption(Language.French, "Français"),
        new LanguageOption(Language.English, "English")
    };

    /// <summary>
    /// Langue sélectionnée - BINDÉE au ComboBox.
    /// </summary>
    [ObservableProperty]
    private LanguageOption _selectedLanguage = null!;

    /// <summary>
    /// Sauvegarde actuellement sélectionnée.
    /// </summary>
    [ObservableProperty]
    private BackupWorkViewModel? _selectedBackup;

    /// <summary>
    /// Message de statut affiché à l'utilisateur.
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // ========== TEXTES LOCALISÉS (bindés dans le XAML) ==========

    [ObservableProperty]
    private string _selectAllButtonText = "Tout sélectionner";

    [ObservableProperty]
    private string _addButtonText = "Ajouter";

    [ObservableProperty]
    private string _runAllButtonText = "Tout exécuter";

    [ObservableProperty]
    private string _languageLabel = "Langue :";

    [ObservableProperty]
    private string _logFormatLabel = "Format des logs :";

    public MainViewModel()
    {
        _config = Config.Load();
        _localization = _config.Localization;
        _backupWorkList = new BackupWorkList();

        // Charger la langue depuis la config persistée
        _selectedLanguage = AvailableLanguages.First(l => l.Value == _config.Language);

        UpdateLocalizedTexts();
        LoadBackups();
    }

    /// <summary>
    /// Met à jour tous les textes quand la langue change.
    /// </summary>
    private void UpdateLocalizedTexts()
    {
        SelectAllButtonText = _localization.Get("gui.buttons.select_all");
        AddButtonText = _localization.Get("gui.buttons.add");
        RunAllButtonText = _localization.Get("gui.buttons.run_all");
        LanguageLabel = _localization.Get("gui.labels.language");
        LogFormatLabel = _localization.Get("gui.labels.log_format");
        StatusMessage = _localization.Get("gui.status.ready");
    }

    /// <summary>
    /// ?? Appelé automatiquement quand SelectedLanguage change via le binding !
    /// </summary>
    partial void OnSelectedLanguageChanged(LanguageOption value)
    {
        if (value == null) return;

        // 1. Sauvegarder dans la config
        _config.Language = value.Value;
        _config.Save();  // Persiste dans appsettings.json

        // 2. Changer la langue du service
        _localization.SetLanguage(value.Value);

        // 3. Rafraîchir l'interface
        UpdateLocalizedTexts();

        // 4. Message de confirmation
        StatusMessage = $"? Langue changée : {value.DisplayName}";
    }

    /// <summary>
    /// Charge les sauvegardes depuis le fichier JSON.
    /// </summary>
    private void LoadBackups()
    {
        Backups.Clear();
        foreach (var backup in _backupWorkList.GetAllWorks())
        {
            Backups.Add(new BackupWorkViewModel(backup));
        }
        StatusMessage = $"{Backups.Count} sauvegarde(s) chargée(s)";
    }

    /// <summary>
    /// Rafraîchit la liste des sauvegardes.
    /// </summary>
    [RelayCommand]
    private void Refresh()
    {
        LoadBackups();
    }

    /// <summary>
    /// Exécute toutes les sauvegardes.
    /// </summary>
    [RelayCommand]
    private async Task RunAllAsync()
    {
        StatusMessage = _localization.Get("gui.status.running");
        foreach (var backup in Backups)
        {
            await backup.RunAsync();
        }
        StatusMessage = _localization.Get("gui.status.completed");
    }
}

/// <summary>
/// Option de langue pour le ComboBox.
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
