using CommunityToolkit.Mvvm.ComponentModel;
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

    public SettingsViewModel(
        Config config,
        ILocalizationService localization,
        BusinessSoftwareService businessSoftware,
        IAppEvents events)
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
    }

    public void RefreshFromConfig()
    {
        SelectedLanguage = AvailableLanguages.First(l => l.Value == _config.Language);
        SelectedLogFormat = AvailableLogFormats.Contains(_config.LogType) ? _config.LogType : "json";

        CryptoSoftPath = _config.CryptoSoftPath;
        BusinessSoftware = _config.BusinessSoftware;

        EncryptExtensionsRaw = _config.EncryptExtensions;
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

    partial void OnSelectedLanguageChanged(LanguageOption value)
    {
        if (value == null) return;

        _config.Language = value.Value;
        _config.Save();
        _localization.SetLanguage(value.Value);
        RefreshLocalizedTexts();

        _events.RaiseLocalizationChanged();
        StatusMessage = _localization.Get("gui.status.language_changed", value.DisplayName);
    }

    partial void OnSelectedLogFormatChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == _config.LogType)
            return;

        if (!AvailableLogFormats.Contains(value))
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

    partial void OnCryptoSoftPathChanged(string value)
    {
        if (value == _config.CryptoSoftPath) return;
        _config.CryptoSoftPath = value ?? string.Empty;
        _config.Save();
    }

    partial void OnBusinessSoftwareChanged(string value)
    {
        var normalized = (value ?? string.Empty).Replace(".exe", "", StringComparison.OrdinalIgnoreCase).Trim();
        if (normalized == _config.BusinessSoftware) return;
        _config.BusinessSoftware = normalized;
        _config.Save();
    }

    [CommunityToolkit.Mvvm.Input.RelayCommand]
    private void RefreshProcesses()
    {
        RefreshRunningProcesses();
        StatusMessage = _localization.Get("gui.status.ready");
    }

    [CommunityToolkit.Mvvm.Input.RelayCommand]
    private void AddExtension()
    {
        if (string.IsNullOrWhiteSpace(NewExtension))
            return;

        var ok = _config.AddEncryptExtension(NewExtension);
        if (ok)
        {
            _config.Save();
            EncryptExtensionsRaw = _config.EncryptExtensions;
            SyncExtensionsCollection();
            NewExtension = string.Empty;
        }
    }

    [CommunityToolkit.Mvvm.Input.RelayCommand]
    private void RemoveExtension(string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return;

        var ok = _config.RemoveEncryptExtension(extension);
        if (ok)
        {
            _config.Save();
            EncryptExtensionsRaw = _config.EncryptExtensions;
            SyncExtensionsCollection();
        }
    }
}

