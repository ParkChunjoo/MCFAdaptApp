using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using MCFAdaptApp.WPF.ViewModels;

namespace MCFAdaptApp.WPF.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        private readonly LoginViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the LoginView
        /// </summary>
        public LoginView()
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Initializing LoginView");
            
            InitializeComponent();
            
            // Create ViewModel instance and set as DataContext
            _viewModel = new LoginViewModel();
            DataContext = _viewModel;
            
            // Register event handlers
            Loaded += LoginView_Loaded;
            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
            
            // Set up property changed event handler for password visibility
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] LoginView initialized");
        }

        /// <summary>
        /// Handles the Loaded event
        /// </summary>
        private void LoginView_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] LoginView loaded");
            
            // Set focus to the user ID field
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var firstTextBox = LogicalTreeHelper.FindLogicalNode(this, "UserId") as TextBox;
                firstTextBox?.Focus();
            }));
        }

        /// <summary>
        /// Handles the PasswordChanged event
        /// </summary>
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Update the password in the ViewModel
            if (_viewModel != null)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Password changed, length: {PasswordBox.Password.Length}");
                _viewModel.Password = PasswordBox.Password;
                
                // Update the visible password TextBox
                PasswordVisibleBox.Text = PasswordBox.Password;
                
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ViewModel password updated, length: {_viewModel.Password.Length}");
            }
        }
        
        /// <summary>
        /// Handles property changes in the ViewModel
        /// </summary>
        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LoginViewModel.IsPasswordVisible))
            {
                UpdatePasswordVisibility();
            }
        }
        
        /// <summary>
        /// Updates the visibility of password controls based on the IsPasswordVisible property
        /// </summary>
        private void UpdatePasswordVisibility()
        {
            if (_viewModel.IsPasswordVisible)
            {
                // Show password as plain text
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordVisibleBox.Visibility = Visibility.Visible;
                PasswordVisibleBox.Text = PasswordBox.Password;
                
                // Change eye icon to "eye-off" icon
                EyeIcon.Data = Geometry.Parse("M2,5.27L3.28,4L20,20.72L18.73,22L15.65,18.92C14.5,19.3 13.28,19.5 12,19.5C7,19.5 2.73,16.39 1,12C1.69,10.24 2.79,8.69 4.19,7.46L2,5.27M12,9A3,3 0 0,1 15,12C15,12.35 14.94,12.69 14.83,13L11,9.17C11.31,9.06 11.65,9 12,9M12,4.5C17,4.5 21.27,7.61 23,12C22.18,14.08 20.79,15.88 19,17.19L17.58,15.76C18.94,14.82 20.06,13.54 20.82,12C19.17,8.64 15.76,6.5 12,6.5C10.91,6.5 9.84,6.68 8.84,7L7.3,5.47C8.74,4.85 10.33,4.5 12,4.5M9,12C9,11.65 9.06,11.31 9.17,11L12.83,14.66C12.69,14.83 12.35,15 12,15A3,3 0 0,1 9,12Z");
            }
            else
            {
                // Hide password
                PasswordBox.Visibility = Visibility.Visible;
                PasswordVisibleBox.Visibility = Visibility.Collapsed;
                
                // Change back to normal eye icon
                EyeIcon.Data = Geometry.Parse("M12,9A3,3 0 0,1 15,12A3,3 0 0,1 12,15A3,3 0 0,1 9,12A3,3 0 0,1 12,9M12,4.5C17,4.5 21.27,7.61 23,12C21.27,16.39 17,19.5 12,19.5C7,19.5 2.73,16.39 1,12C2.73,7.61 7,4.5 12,4.5M3.18,12C4.83,15.36 8.24,17.5 12,17.5C15.76,17.5 19.17,15.36 20.82,12C19.17,8.64 15.76,6.5 12,6.5C8.24,6.5 4.83,8.64 3.18,12Z");
                
                // Ensure focus returns to password box if it was visible
                if (PasswordVisibleBox.IsFocused)
                {
                    PasswordBox.Focus();
                }
            }
        }
        
        /// <summary>
        /// Handles the click event of the password visibility toggle button
        /// </summary>
        private void TogglePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.IsPasswordVisible = !_viewModel.IsPasswordVisible;
            UpdatePasswordVisibility();
        }
    }
} 