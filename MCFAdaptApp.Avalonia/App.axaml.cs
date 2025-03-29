using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MCFAdaptApp.Domain.Services;
using MCFAdaptApp.Infrastructure.Services;
using MCFAdaptApp.Avalonia.Views;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MCFAdaptApp.Avalonia
{
    public partial class App : global::Avalonia.Application
    {
        private ServiceProvider? _serviceProvider;

        public static new App? Current => global::Avalonia.Application.Current as App;

        public IServiceProvider? Services => _serviceProvider;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Application starting up");

                var loginView = _serviceProvider.GetService<LoginView>() ?? throw new InvalidOperationException("Failed to resolve LoginView from service provider");
                desktop.MainWindow = loginView;

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Login view displayed");
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Register services
            services.AddSingleton<IAuthenticationService, SimpleAuthenticationService>();
            services.AddSingleton<IPatientService, FilePatientService>();
            services.AddSingleton<IDicomService, DicomService>();

            // Register views
            services.AddTransient<LoginView>();
            services.AddTransient<SelectPatientView>();
            services.AddTransient<RegisterView>();

            // Register viewmodels
            services.AddTransient<ViewModels.LoginViewModel>();
            services.AddTransient<ViewModels.SelectPatientViewModel>();
            services.AddTransient<ViewModels.RegisterViewModel>();
        }
    }
}
