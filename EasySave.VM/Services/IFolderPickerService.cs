namespace EasySave.VM.Services;

public interface IFolderPickerService
{
    /// <summary>Returns selected folder path or null if cancelled.</summary>
    string? PickFolder(string? initialDirectory = null);
}

