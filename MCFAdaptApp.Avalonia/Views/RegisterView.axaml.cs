using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MCFAdaptApp.Avalonia.ViewModels;
using MCFAdaptApp.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MCFAdaptApp.Avalonia.Views
{
    public partial class RegisterView : UserControl
    {
        public RegisterView()
        {
            InitializeComponent();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] RegisterView initialized");

            // Set DataContext
            DataContext = App.Current.Services.GetService<RegisterViewModel>() ??
                          new RegisterViewModel(App.Current.Services.GetService<IDicomService>());

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] RegisterView DataContext set to RegisterViewModel");
        }

        public RegisterView(RegisterViewModel viewModel) : this()
        {
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] RegisterView DataContext set to provided RegisterViewModel");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
