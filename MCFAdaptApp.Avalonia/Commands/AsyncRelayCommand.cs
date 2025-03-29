using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;

namespace MCFAdaptApp.Avalonia.Commands
{
    /// <summary>
    /// A command that asynchronously relays its functionality to other
    /// objects by invoking delegates
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        private bool _isExecuting;

        /// <summary>
        /// Creates a new command that can always execute
        /// </summary>
        /// <param name="execute">The execution logic</param>
        public AsyncRelayCommand(Func<Task> execute) : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command
        /// </summary>
        /// <param name="execute">The execution logic</param>
        /// <param name="canExecute">The execution status logic</param>
        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Determines if this command can execute in its current state
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            return !_isExecuting && (_canExecute == null || _canExecute());
        }

        /// <summary>
        /// Executes the command asynchronously
        /// </summary>
        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            try
            {
                _isExecuting = true;
                RaiseCanExecuteChanged();

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] AsyncRelayCommand executing");
                await _execute();
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] AsyncRelayCommand completed");
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// 명령의 CanExecute 상태가 변경되었음을 알립니다.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            Dispatcher.UIThread.Post(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));
        }

        /// <summary>
        /// Event raised when the ability to execute the command changes
        /// </summary>
        public event EventHandler? CanExecuteChanged;
    }
}
