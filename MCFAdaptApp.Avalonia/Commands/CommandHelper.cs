using System;
using Avalonia.Threading;

namespace MCFAdaptApp.Avalonia.Commands
{
    public static class CommandHelper
    {
        public static void RaiseCanExecuteChanged(object sender, EventHandler? canExecuteChanged)
        {
            Dispatcher.UIThread.Post(() => canExecuteChanged?.Invoke(sender, EventArgs.Empty));
        }
    }
}
