using EasySave.Core.Localization;

namespace EasySave.VM.ViewModels;

public sealed class LanguageOption
{
    public Language Value { get; }
    public string DisplayName { get; }
    public string FlagPath { get; }

    public LanguageOption(Language value, string displayName, string flagPath)
    {
        Value = value;
        DisplayName = displayName;
        FlagPath = flagPath;
    }
}

