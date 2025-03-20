using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] LoginView initialized");
        }

        public LoginView(LoginViewModel viewModel) : this()
        {
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] LoginView DataContext set to LoginViewModel");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
