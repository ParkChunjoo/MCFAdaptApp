using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MCFAdaptApp.Avalonia.Helpers;
using MCFAdaptApp.Avalonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MCFAdaptApp.Avalonia.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            LogHelper.Log("LoginView initialized");
            
            // Add keyboard event handling for Enter key
            this.KeyDown += OnKeyDown;
        }

        public LoginView(LoginViewModel viewModel) : this()
        {
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            LogHelper.Log("LoginView DataContext set to LoginViewModel");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Find out which textbox triggered the event
                if (sender is TextBox textBox)
                {
                    if (textBox.Name == "UsernameTextBox")
                    {
                        // Move focus to the password field
                        var passwordBox = this.FindControl<TextBox>("PasswordBox");
                        var passwordVisibleBox = this.FindControl<TextBox>("PasswordVisibleBox");
                        
                        if (passwordBox != null && passwordBox.IsVisible)
                        {
                            passwordBox.Focus();
                        }
                        else if (passwordVisibleBox != null)
                        {
                            passwordVisibleBox.Focus();
                        }
                    }
                    else
                    {
                        // Password field - execute login command
                        if (DataContext is LoginViewModel viewModel && viewModel.LoginCommand.CanExecute(null))
                        {
                            viewModel.LoginCommand.Execute(null);
                        }
                    }
                }
                e.Handled = true;
            }
        }
        
        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Execute the login command when Enter is pressed
                if (DataContext is LoginViewModel viewModel && viewModel.LoginCommand.CanExecute(null))
                {
                    viewModel.LoginCommand.Execute(null);
                }
            }
        }
    }
}
