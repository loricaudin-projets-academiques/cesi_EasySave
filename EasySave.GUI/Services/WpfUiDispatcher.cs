using EasySave.VM.Services;

namespace EasySave.GUI.Services;

public sealed class WpfUiDispatcher : IUiDispatcher
{
    public void Invoke(Action action)
    {
        if (System.Windows.Application.Current?.Dispatcher == null)
        {
            action();
            return;
        }

        if (System.Windows.Application.Current.Dispatcher.CheckAccess())
            action();
        else
            System.Windows.Application.Current.Dispatcher.Invoke(action);
    }
}

