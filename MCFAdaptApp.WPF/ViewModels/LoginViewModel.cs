using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MCFAdaptApp.Domain.Services;
using MCFAdaptApp.Infrastructure.Services;
using MCFAdaptApp.WPF.Commands;
using MCFAdaptApp.WPF.Views;

namespace MCFAdaptApp.WPF.ViewModels
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
                    
                    // Navigate to patient selection screen
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Creating SelectPatientView");
                    var patientView = new SelectPatientView();
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Showing SelectPatientView");
                    patientView.Show();
                    
                    // Close the login window
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Closing login window");
                    CloseWindow();
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
        /// Closes the login window
        /// </summary>
        private void CloseWindow()
        {
            // Find the login window and close it
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }

        /// <summary>
        /// Exits the application
        /// </summary>
        private void ExitApplication(object parameter)
        {
            System.Windows.Application.Current.Shutdown();
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
