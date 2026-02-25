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
    [ObservableProperty] private string _generalLabel = string.Empty;
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
    [ObservableProperty] private string _logsLabel = string.Empty;
    [ObservableProperty] private string _logsLocalLabel = string.Empty;
    [ObservableProperty] private string _saveLogsLocalLabel = string.Empty;
    [ObservableProperty] private string _logsServerLabel = string.Empty;
    [ObservableProperty] private string _saveLogsServerLabel = string.Empty;
    [ObservableProperty] private string _serverAddressLabel = string.Empty;
    [ObservableProperty] private string _serverPortLabel = string.Empty;
    [ObservableProperty] private string _largeFileDisabledLabel = string.Empty;
    [ObservableProperty] private string _selectedLabel = string.Empty;
    [ObservableProperty] private string _clearLabel = string.Empty;
    [ObservableProperty] private string _useSelectedProcessLabel = string.Empty;
    [ObservableProperty] private string _removeLabel = string.Empty;
    

    [ObservableProperty] private string? _selectedProcess;

    [ObservableProperty] private bool _saveLogsLocal;
    [ObservableProperty] private bool _saveLogsServer;
    [ObservableProperty] private string _serverAddress = string.Empty;
    [ObservableProperty] private string _serverPort = "0";


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
        GeneralLabel = _localization.Get("gui.sections.general");
        LogsLabel = _localization.Get("gui.sections.logs");
        LogsLocalLabel = _localization.Get("gui.logs.local");
        SaveLogsLocalLabel = _localization.Get("gui.logs.save_local");
        LogFormatLabel = _localization.Get("gui.labels.log_format");
        LogsServerLabel = _localization.Get("gui.logs.server");
        SaveLogsServerLabel = _localization.Get("gui.logs.save_server");
        ServerAddressLabel = _localization.Get("gui.labels.server_address");
        ServerPortLabel = _localization.Get("gui.labels.server_port");
        EncryptExtensionsLabel = _localization.Get("gui.settings.encrypt_extensions");
        BusinessSoftwareLabel = _localization.Get("gui.settings.business_software");
        RunningProcessesLabel = _localization.Get("gui.pages.running_processes");
        LargeFileThresholdLabel = _localization.Get("gui.settings.large_file_threshold");
        PriorityExtensionsLabel = _localization.Get("gui.settings.priority_extensions");
        AddExtensionButtonText = _localization.Get("gui.buttons.add_extension");
        RemoveLabel = _localization.Get("gui.buttons.remove");
        BusinessSoftwareLabel = _localization.Get("gui.settings.business_software");
        SelectedLabel = _localization.Get("gui.labels.selected");
        ClearLabel = _localization.Get("gui.buttons.clear");
        UseSelectedProcessLabel = _localization.Get("gui.buttons.use_selected_process");
        RunningProcessesLabel = _localization.Get("gui.pages.running_processes");
        RefreshProcessesButtonText = _localization.Get("gui.buttons.refresh");
        LargeFileThresholdLabel = _localization.Get("gui.settings.large_file_threshold");
        LargeFileDisabledLabel = _localization.Get("gui.labels.disabled_hint");
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

        SaveLogsLocal = _config.LogInLocal;
        SaveLogsServer = _config.LogOnServer;
        ServerAddress = _config.LogServerUrl;
        ServerPort = _config.LogServerPort.ToString();

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
        var normalized = (BusinessSoftware ?? string.Empty)
            .Replace(".exe", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        _config.BusinessSoftware = normalized;

        if (long.TryParse(LargeFileThreshold, out var threshold) && threshold >= 0)
            _config.LargeFileThresholdKB = threshold;
        else
            _config.LargeFileThresholdKB = 0;

        _config.LogInLocal = SaveLogsLocal;
        _config.LogOnServer = SaveLogsServer;
        _config.LogServerUrl = ServerAddress;

        if (int.TryParse(ServerPort, out var port))
            _config.LogServerPort = port;
        else
            _config.LogServerPort = 0;

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

