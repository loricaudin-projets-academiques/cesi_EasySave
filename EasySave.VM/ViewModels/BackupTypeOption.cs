namespace EasySave.VM.ViewModels;

public sealed class BackupTypeOption
{
    public string Value { get; }
    public string DisplayName { get; }

    public BackupTypeOption(string value, string displayName)
    {
        Value = value;
        DisplayName = displayName;
    }
}

