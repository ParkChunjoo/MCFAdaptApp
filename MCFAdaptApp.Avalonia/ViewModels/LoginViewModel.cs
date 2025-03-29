using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System.Windows.Input;
using MCFAdaptApp.Domain.Services;
using MCFAdaptApp.Infrastructure.Services;
using MCFAdaptApp.Avalonia.Commands;
using MCFAdaptApp.Avalonia.Views;
using MCFAdaptApp.Avalonia;

namespace MCFAdaptApp.Avalonia.ViewModels
{
    /// <summary>
    /// ViewModel for the login screen
    /// </summary>
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IAuthenticationService _authService;
        private string _userId = string.Empty;
        private string _password = string.Empty;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private bool _hasError;
        private bool _isPasswordVisible;

        /// <summary>
        /// User ID for login
        /// </summary>
        public string UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                OnPropertyChanged();
                ClearError();
                (LoginCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Password for login
        /// </summary>
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                ClearError();
                (LoginCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Indicates if the password is visible as plain text
        /// </summary>
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                _isPasswordVisible = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Indicates if login is in progress
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                // Update command execution status when loading state changes
                (LoginCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Error message to display
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
                HasError = !string.IsNullOrEmpty(value);
            }
        }

        /// <summary>
        /// Indicates if an error has occurred
        /// </summary>
        public bool HasError
        {
            get => _hasError;
            set
            {
                _hasError = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Command to execute login
        /// </summary>
        public ICommand LoginCommand { get; private set; }

        /// <summary>
        /// Command to exit the application
        /// </summary>
        public ICommand ExitCommand { get; private set; }

        /// <summary>
        /// Command to toggle password visibility
        /// </summary>
        public ICommand TogglePasswordVisibilityCommand { get; private set; }

        /// <summary>
        /// Creates a new instance of LoginViewModel
        /// </summary>
        /// <param name="authService">Optional authentication service for dependency injection</param>
        public LoginViewModel(IAuthenticationService? authService = null)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Initializing LoginViewModel");

            // Dependency injection or create default service
            _authService = authService ?? new SimpleAuthenticationService();

            // Initialize commands
            LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
            ExitCommand = new RelayCommand(ExitApplication);
            TogglePasswordVisibilityCommand = new RelayCommand(TogglePasswordVisibility);

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] LoginViewModel initialized");
        }

        /// <summary>
        /// Toggles the visibility of the password
        /// </summary>
        private void TogglePasswordVisibility(object parameter)
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        /// <summary>
        /// Determines if login can be executed
        /// </summary>
        private bool CanLogin()
        {
            return !IsLoading && !string.IsNullOrWhiteSpace(UserId) && !string.IsNullOrWhiteSpace(Password);
        }

        /// <summary>
        /// Executes the login process
        /// </summary>
        private async Task LoginAsync()
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Login attempt for user: {UserId}");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Password length: {Password?.Length ?? 0}");

            try
            {
                IsLoading = true;
                ClearError();

                // Authenticate user
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Calling AuthenticateAsync with UserId: '{UserId}' and Password: '{Password}'");
                var isAuthenticated = await _authService.AuthenticateAsync(UserId, Password ?? string.Empty);
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Authentication result: {isAuthenticated}");

                if (isAuthenticated)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Login successful");

                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Creating MainWindow");

                    var mainWindow = new MainWindow();

                    // Initialize view properties
                    mainWindow.WindowStartupLocation = global::Avalonia.Controls.WindowStartupLocation.CenterScreen;
                    mainWindow.WindowState = global::Avalonia.Controls.WindowState.Maximized;

                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Showing MainWindow");

                    if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        // Get the current login window
                        var currentWindow = desktop.MainWindow;

                        desktop.MainWindow = mainWindow;

                        mainWindow.Show();

                        mainWindow.SelectTab(0);

                        // Close the login window
                        currentWindow?.Close();

                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Main window switched to MainWindow");
                    }
                    else
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Application lifetime is not IClassicDesktopStyleApplicationLifetime");
                    }
                }
                else
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Login failed: Invalid credentials");
                    ErrorMessage = "Invalid user ID or password.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Login error: {ex.Message}");
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Exception details: {ex}");
                ErrorMessage = $"Login error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Clears any error message
        /// </summary>
        private void ClearError()
        {
            if (HasError)
            {
                ErrorMessage = string.Empty;
            }
        }

        /// <summary>
        /// Exits the application
        /// </summary>
        private void ExitApplication(object parameter)
        {
            if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }

        /// <summary>
        /// Event raised when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
