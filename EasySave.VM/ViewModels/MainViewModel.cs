using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Localization;
using EasySave.Core.Settings;
using EasySave.VM.Services;

namespace EasySave.VM.ViewModels;

/// <summary>
/// Main application shell ViewModel.
/// Hosts navigation and the currently displayed page ViewModel.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly Config _config;
    private readonly ILocalizationService _localization;
    private readonly IShellNavigationService _navigation;
    private readonly IAppEvents _events;

    private readonly BackupListViewModel _backupListViewModel;
    private readonly BackupEditorViewModel _backupEditorViewModel;
    private readonly SettingsViewModel _settingsViewModel;
    private readonly AboutViewModel _aboutViewModel;

    // ========== LOCALIZED TEXTS ==========
    
    [ObservableProperty] private string _backupsNavText = string.Empty;
    [ObservableProperty] private string _settingsNavText = string.Empty;
    [ObservableProperty] private string _aboutNavText = string.Empty;

    /// <summary>Currently displayed page ViewModel.</summary>
    [ObservableProperty] private ObservableObject _currentPage = null!;

    public MainViewModel(
        Config config,
        ILocalizationService localization,
        IShellNavigationService navigation,
        IAppEvents events,
        BackupListViewModel backupListViewModel,
        BackupEditorViewModel backupEditorViewModel,
        SettingsViewModel settingsViewModel,
        AboutViewModel aboutViewModel)
    {
        _config = config;
        _localization = localization;
        _navigation = navigation;
        _events = events;
        _backupListViewModel = backupListViewModel;
        _backupEditorViewModel = backupEditorViewModel;
        _settingsViewModel = settingsViewModel;
        _aboutViewModel = aboutViewModel;

        UpdateLocalizedTexts();

        _navigation.NavigationRequested += OnNavigationRequested;
        _events.LocalizationChanged += (_, __) => UpdateLocalizedTexts();

        // Default page
        CurrentPage = _backupListViewModel;
    }
    
    private void UpdateLocalizedTexts()
    {
        BackupsNavText = _localization.Get("gui.nav.backups");
        SettingsNavText = _localization.Get("gui.nav.settings");
        AboutNavText = _localization.Get("gui.nav.about");
    }

    [RelayCommand]
    private void NavigateBackups() => _navigation.RequestNavigate(NavigationTarget.Backups);

    [RelayCommand]
    private void NavigateSettings()
    {
        _navigation.RequestNavigate(NavigationTarget.Settings);
    }

    [RelayCommand]
    private void NavigateAbout() => _navigation.RequestNavigate(NavigationTarget.About);

    private void OnNavigationRequested(object? sender, NavigationRequest e)
    {
        switch (e.Target)
        {
            case NavigationTarget.Backups:
                CurrentPage = _backupListViewModel;
                _backupListViewModel.Refresh();
                break;
            case NavigationTarget.Settings:
                CurrentPage = _settingsViewModel;
                _settingsViewModel.RefreshFromConfig();
                break;
            case NavigationTarget.About:
                CurrentPage = _aboutViewModel;
                break;
            case NavigationTarget.EditorCreate:
                _backupEditorViewModel.BeginCreate();
                CurrentPage = _backupEditorViewModel;
                break;
            case NavigationTarget.EditorEdit:
                if (e.BackupIndex is int idx)
                    _backupEditorViewModel.BeginEdit(idx);
                CurrentPage = _backupEditorViewModel;
                break;
            default:
                CurrentPage = _backupListViewModel;
                break;
        }
    }
}
