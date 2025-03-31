using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using MCFAdaptApp.Avalonia.Helpers;

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
                
                LogHelper.Log("AsyncRelayCommand executing");
                await _execute();
                LogHelper.Log("AsyncRelayCommand completed");
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

    /// <summary>
    /// A generic command that asynchronously relays its functionality to other
    /// objects by invoking delegates with parameters
    /// </summary>
    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T?, Task> _execute;
        private readonly Func<T?, bool>? _canExecute;
        private bool _isExecuting;

        /// <summary>
        /// Creates a new command that can always execute
        /// </summary>
        /// <param name="execute">The execution logic with parameter</param>
        public AsyncRelayCommand(Func<T?, Task> execute) : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command
        /// </summary>
        /// <param name="execute">The execution logic with parameter</param>
        /// <param name="canExecute">The execution status logic with parameter</param>
        public AsyncRelayCommand(Func<T?, Task> execute, Func<T?, bool>? canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Determines if this command can execute in its current state
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            if (_isExecuting)
                return false;

            if (_canExecute == null)
                return true;

            if (parameter == null && typeof(T).IsValueType)
                return _canExecute(default);

            if (parameter == null || parameter is T)
                return _canExecute((T?)parameter);

            return false;
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
                
                LogHelper.Log($"AsyncRelayCommand<{typeof(T).Name}> executing with parameter: {parameter}");
                
                if (parameter == null || parameter is T)
                {
                    await _execute((T?)parameter);
                }
                
                LogHelper.Log($"AsyncRelayCommand<{typeof(T).Name}> completed");
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
