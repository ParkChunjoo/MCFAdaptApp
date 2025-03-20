using System;
using System.Windows.Input;

namespace MCFAdaptApp.WPF.Commands
{
    /// <summary>
    /// A command whose sole purpose is to relay its functionality to other
    /// objects by invoking delegates
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object>? _canExecute;

        /// <summary>
        /// Creates a new command that can always execute
        /// </summary>
        /// <param name="execute">The execution logic</param>
        public RelayCommand(Action<object> execute) : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command
        /// </summary>
        /// <param name="execute">The execution logic</param>
        /// <param name="canExecute">The execution status logic</param>
        public RelayCommand(Action<object> execute, Predicate<object>? canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Determines if this command can execute in its current state
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter!);
        }

        /// <summary>
        /// Executes the command
        /// </summary>
        public void Execute(object? parameter)
        {
            _execute(parameter!);
        }

        /// <summary>
        /// Event raised when the ability to execute the command changes
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
