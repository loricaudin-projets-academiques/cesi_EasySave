namespace EasySave.VM.Services;

public interface IAppEvents
{
    event EventHandler? LocalizationChanged;
    void RaiseLocalizationChanged();
}

public sealed class AppEvents : IAppEvents
{
    public event EventHandler? LocalizationChanged;
    public void RaiseLocalizationChanged() => LocalizationChanged?.Invoke(this, EventArgs.Empty);
}

