namespace EasySave.VM.Services;

public enum NavigationTarget
{
    Backups,
    Settings,
    About,
    EditorCreate,
    EditorEdit
}

public sealed class NavigationRequest : EventArgs
{
    public NavigationTarget Target { get; }
    public int? BackupIndex { get; }

    public NavigationRequest(NavigationTarget target, int? backupIndex = null)
    {
        Target = target;
        BackupIndex = backupIndex;
    }
}

public interface IShellNavigationService
{
    event EventHandler<NavigationRequest>? NavigationRequested;
    void RequestNavigate(NavigationTarget target, int? backupIndex = null);
}

public sealed class ShellNavigationService : IShellNavigationService
{
    public event EventHandler<NavigationRequest>? NavigationRequested;

    public void RequestNavigate(NavigationTarget target, int? backupIndex = null)
    {
        NavigationRequested?.Invoke(this, new NavigationRequest(target, backupIndex));
    }
}

