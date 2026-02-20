using CommunityToolkit.Mvvm.ComponentModel;
using EasyLog.Configuration;
using EasySave.Core.Localization;
using EasySave.Core.Settings;

namespace EasySave.VM.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    private readonly Config _config;
    private readonly ILocalizationService _localization;

    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _configPathLabel = string.Empty;
    [ObservableProperty] private string _logsPathLabel = string.Empty;
    [ObservableProperty] private string _configPath = string.Empty;
    [ObservableProperty] private string _logsPath = string.Empty;

    public AboutViewModel(Config config, ILocalizationService localization)
    {
        _config = config;
        _localization = localization;

        Title = _localization.Get("gui.about.title");
        ConfigPathLabel = _localization.Get("gui.about.config_path");
        LogsPathLabel = _localization.Get("gui.about.logs_path");

        ConfigPath = _config.ConfigFilePath;
        LogsPath = new LogConfiguration().LogDirectory;
    }
}

