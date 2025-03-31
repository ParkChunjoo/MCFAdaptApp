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
using MCFAdaptApp.Avalonia.Helpers;

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
            LogHelper.Log("Initializing LoginViewModel");
            
            // Dependency injection or create default service
            _authService = authService ?? new SimpleAuthenticationService();
            
            // Initialize commands
            LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
            ExitCommand = new RelayCommand(ExitApplication);
            TogglePasswordVisibilityCommand = new RelayCommand(TogglePasswordVisibility);
            
            LogHelper.Log("LoginViewModel initialized");
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
            LogHelper.Log($"Login attempt for user: {UserId}");
            LogHelper.Log($"Password length: {Password?.Length ?? 0}");
            
            try
            {
                IsLoading = true;
                ClearError();

                // Authenticate user
                LogHelper.Log($"Calling AuthenticateAsync with UserId: '{UserId}' and Password: '{Password}'");
                var isAuthenticated = await _authService.AuthenticateAsync(UserId, Password ?? string.Empty);
                LogHelper.Log($"Authentication result: {isAuthenticated}");
                
                if (isAuthenticated)
                {
                    LogHelper.Log("Login successful");
                    
                    // Navigate to main window with patient view
                    LogHelper.Log("Creating MainWindow");
                    
                    // Create the patient view model directly
                    var viewModel = new SelectPatientViewModel(new FilePatientService());
                    
                    // Create and show the new main window with the patient view model
                    var mainWindow = new MainWindow(viewModel);
                    
                    LogHelper.Log("Showing MainWindow");

                    // Set the main window as the application's main window
                    if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        // Get the current login window
                        var currentWindow = desktop.MainWindow;
                        
                        // Set the main window as the application's main window
                        desktop.MainWindow = mainWindow;
                        
                        // Show the main window
                        mainWindow.Show();
                        
                        // Close the login window
                        currentWindow?.Close();
                        
                        LogHelper.Log("Main window switched to MainWindow");
                    }
                    else
                    {
                        LogHelper.Log("Application lifetime is not IClassicDesktopStyleApplicationLifetime");
                    }
                }
                else
                {
                    LogHelper.Log("Login failed: Invalid credentials");
                    ErrorMessage = "Invalid user ID or password.";
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Login error: {ex.Message}");
                LogHelper.LogException(ex);
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
            LogHelper.Log("ExitCommand executed, shutting down application");
            
            if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }

        /// <summary>
        /// Event that is raised when a property changes
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
