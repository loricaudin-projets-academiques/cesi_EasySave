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
    public ObservableCollection<string> PriorityExtensions { get; } = new();
    public ObservableCollection<string> RunningProcesses { get; } = new();

    [ObservableProperty] private LanguageOption _selectedLanguage = null!;
    [ObservableProperty] private string _selectedLogFormat = "json";

    [ObservableProperty] private string _encryptExtensionsRaw = string.Empty;
    [ObservableProperty] private string _newExtension = string.Empty;

    [ObservableProperty] private string _priorityExtensionsRaw = string.Empty;
    [ObservableProperty] private string _newPriorityExtension = string.Empty;

    [ObservableProperty] private string _businessSoftware = string.Empty;
    [ObservableProperty] private string _largeFileThreshold = "0";

    [ObservableProperty] private string _statusMessage = string.Empty;

    // Localized texts
    [ObservableProperty] private string _pageTitle = string.Empty;
    [ObservableProperty] private string _languageLabel = string.Empty;
    [ObservableProperty] private string _logFormatLabel = string.Empty;
    [ObservableProperty] private string _encryptExtensionsLabel = string.Empty;
    [ObservableProperty] private string _businessSoftwareLabel = string.Empty;
    [ObservableProperty] private string _runningProcessesLabel = string.Empty;
    [ObservableProperty] private string _largeFileThresholdLabel = string.Empty;
    [ObservableProperty] private string _priorityExtensionsLabel = string.Empty;
    [ObservableProperty] private string _addExtensionButtonText = string.Empty;
    [ObservableProperty] private string _removeButtonText = string.Empty;
    [ObservableProperty] private string _refreshProcessesButtonText = string.Empty;
    [ObservableProperty] private string _saveButtonText = string.Empty;

    [ObservableProperty] private string? _selectedProcess;

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
        EncryptExtensionsLabel = _localization.Get("gui.settings.encrypt_extensions");
        BusinessSoftwareLabel = _localization.Get("gui.settings.business_software");
        RunningProcessesLabel = _localization.Get("gui.pages.running_processes");
        LargeFileThresholdLabel = _localization.Get("gui.settings.large_file_threshold");
        PriorityExtensionsLabel = _localization.Get("gui.settings.priority_extensions");
        AddExtensionButtonText = _localization.Get("gui.buttons.add_extension");
        RemoveButtonText = _localization.Get("gui.buttons.remove");
        RefreshProcessesButtonText = _localization.Get("gui.buttons.refresh");
        SaveButtonText = _localization.Get("gui.buttons.save");
    }

    public void RefreshFromConfig()
    {
        SelectedLanguage = AvailableLanguages.First(l => l.Value == _config.Language);
        SelectedLogFormat = AvailableLogFormats.Contains(_config.LogType) ? _config.LogType : "json";

        BusinessSoftware = _config.BusinessSoftware;
        LargeFileThreshold = _config.LargeFileThresholdKB.ToString();

        EncryptExtensionsRaw = _config.EncryptExtensions;
        SyncExtensionsCollection();

        PriorityExtensionsRaw = _config.PriorityExtensions;
        SyncPriorityExtensionsCollection();

        RefreshRunningProcesses();

        StatusMessage = _localization.Get("gui.status.ready");
    }

    private void SyncExtensionsCollection()
    {
        EncryptExtensions.Clear();
        foreach (var ext in _config.GetEncryptExtensionsList())
            EncryptExtensions.Add(ext);
    }

    private void SyncPriorityExtensionsCollection()
    {
        PriorityExtensions.Clear();
        foreach (var ext in _config.GetPriorityExtensionsList())
            PriorityExtensions.Add(ext);
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

    [RelayCommand]
    private void SelectProcess()
    {
        if (!string.IsNullOrWhiteSpace(SelectedProcess))
        {
            BusinessSoftware = SelectedProcess;
        }
    }

    [RelayCommand]
    private void ClearBusinessSoftware()
    {
        BusinessSoftware = string.Empty;
        SelectedProcess = null;
    }

    [RelayCommand]
    private void SaveSettings()
    {
        var normalized = (BusinessSoftware ?? string.Empty).Replace(".exe", "", StringComparison.OrdinalIgnoreCase).Trim();
        _config.BusinessSoftware = normalized;

        if (long.TryParse(LargeFileThreshold, out var threshold) && threshold >= 0)
            _config.LargeFileThresholdKB = threshold;
        else
            _config.LargeFileThresholdKB = 0;

        _config.Save();
        StatusMessage = _localization.Get("gui.status.settings_saved");
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

    [CommunityToolkit.Mvvm.Input.RelayCommand]
    private void AddPriorityExtension()
    {
        if (string.IsNullOrWhiteSpace(NewPriorityExtension))
            return;

        var ok = _config.AddPriorityExtension(NewPriorityExtension);
        if (ok)
        {
            _config.Save();
            PriorityExtensionsRaw = _config.PriorityExtensions;
            SyncPriorityExtensionsCollection();
            NewPriorityExtension = string.Empty;
        }
    }

    [CommunityToolkit.Mvvm.Input.RelayCommand]
    private void RemovePriorityExtension(string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return;

        var ok = _config.RemovePriorityExtension(extension);
        if (ok)
        {
            _config.Save();
            PriorityExtensionsRaw = _config.PriorityExtensions;
            SyncPriorityExtensionsCollection();
        }
    }
}

