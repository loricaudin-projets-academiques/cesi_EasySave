using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Localization;
using EasySave.Core.Services;
using EasySave.Core.Settings;
using EasySave.VM.Services;
using System.Collections.ObjectModel;

namespace EasySave.VM.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly Config _config;
    private readonly ILocalizationService _localization;
    private readonly BusinessSoftwareService _businessSoftwareService;
    private readonly IAppEvents _events;

    public ObservableCollection<LanguageOption> AvailableLanguages { get; } = new();
    public ObservableCollection<string> AvailableLogFormats { get; } = new() { "json", "xml" };
    public ObservableCollection<string> EncryptExtensions { get; } = new();
    public ObservableCollection<string> RunningProcesses { get; } = new();

    [ObservableProperty] private LanguageOption _selectedLanguage = null!;
    [ObservableProperty] private string _selectedLogFormat = "json";
    [ObservableProperty] private string _encryptExtensionsRaw = string.Empty;
    [ObservableProperty] private string _newExtension = string.Empty;
    [ObservableProperty] private string _cryptoSoftPath = string.Empty;
    [ObservableProperty] private string _businessSoftware = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;

    // Max file size
    [ObservableProperty] private string _maxFileSizeKo = string.Empty;
    [ObservableProperty] private string _maxFileSizeKoLabel = string.Empty;
    [ObservableProperty] private string _addMaxFileSizeKoButtonText = "Add";

    // Localized texts
    [ObservableProperty] private string _pageTitle = string.Empty;
    [ObservableProperty] private string _languageLabel = string.Empty;
    [ObservableProperty] private string _logFormatLabel = string.Empty;
    [ObservableProperty] private string _cryptoSoftPathLabel = string.Empty;
    [ObservableProperty] private string _encryptExtensionsLabel = string.Empty;
    [ObservableProperty] private string _businessSoftwareLabel = string.Empty;
    [ObservableProperty] private string _runningProcessesLabel = string.Empty;
    [ObservableProperty] private string _addExtensionButtonText = string.Empty;
    [ObservableProperty] private string _removeButtonText = string.Empty;
    [ObservableProperty] private string _refreshProcessesButtonText = string.Empty;

    public SettingsViewModel(Config config, ILocalizationService localization, BusinessSoftwareService businessSoftware, IAppEvents events)
    {
        _config = config;
        _localization = localization;
        _businessSoftwareService = businessSoftware;
        _events = events;

        AvailableLanguages.Add(new LanguageOption(Language.French, "Français", "/Resources/Flags/french_flag_icon.png"));
        AvailableLanguages.Add(new LanguageOption(Language.English, "English", "/Resources/Flags/uk_flag_icon.png"));

        RefreshLocalizedTexts();
        RefreshFromConfig();
    }

    private void RefreshLocalizedTexts()
    {
        PageTitle = _localization.Get("gui.pages.settings_title");
        LanguageLabel = _localization.Get("gui.labels.language");
        LogFormatLabel = _localization.Get("gui.labels.log_format");
        CryptoSoftPathLabel = _localization.Get("gui.settings.crypto_path");
        EncryptExtensionsLabel = _localization.Get("gui.settings.encrypt_extensions");
        BusinessSoftwareLabel = _localization.Get("gui.settings.business_software");
        RunningProcessesLabel = _localization.Get("gui.pages.running_processes");
        AddExtensionButtonText = _localization.Get("gui.buttons.add_extension");
        RemoveButtonText = _localization.Get("gui.buttons.remove");
        RefreshProcessesButtonText = _localization.Get("gui.buttons.refresh");
        MaxFileSizeKoLabel = _localization.Get("gui.labels.max_file_size");
    }

    public void RefreshFromConfig()
    {
        SelectedLanguage = AvailableLanguages.First(l => l.Value == _config.Language);
        SelectedLogFormat = AvailableLogFormats.Contains(_config.LogType) ? _config.LogType : "json";
        CryptoSoftPath = _config.CryptoSoftPath;
        BusinessSoftware = _config.BusinessSoftware;
        EncryptExtensionsRaw = _config.EncryptExtensions;
        MaxFileSizeKo = _config.MaxFileSizeKo.ToString();
        SyncExtensionsCollection();
        RefreshRunningProcesses();
        StatusMessage = _localization.Get("gui.status.ready");
    }

    private void SyncExtensionsCollection()
    {
        EncryptExtensions.Clear();
        foreach (var ext in _config.GetEncryptExtensionsList())
            EncryptExtensions.Add(ext);
    }

    private void RefreshRunningProcesses()
    {
        RunningProcesses.Clear();
        foreach (var p in BusinessSoftwareService.GetRunningProcesses())
            RunningProcesses.Add(p);
    }

    [RelayCommand]
    private void AddMaxFileSizeKo()
    {
        if (string.IsNullOrWhiteSpace(MaxFileSizeKo)) return;

        if (int.TryParse(MaxFileSizeKo, out var value))
        {
            _config.MaxFileSizeKo = value;
            _config.Save();
            StatusMessage = $"Max file size set to {value} Ko";
        }
        else
        {
            StatusMessage = "Invalid number";
        }
    }
}