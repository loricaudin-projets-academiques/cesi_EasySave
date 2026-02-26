namespace EasySave.VM.Services;

/// <summary>
/// UI thread dispatcher abstraction for strict MVVM (VM does not depend on WPF).
/// </summary>
public interface IUiDispatcher
{
    void Invoke(Action action);
}

