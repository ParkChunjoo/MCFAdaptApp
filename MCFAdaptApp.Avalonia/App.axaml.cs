using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MCFAdaptApp.Avalonia.Helpers;
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
            LogHelper.Log("Starting OnFrameworkInitializationCompleted");
            try 
            { 
                var services = new ServiceCollection();
                LogHelper.Log("Configuring services...");
                ConfigureServices(services);
                LogHelper.Log("Building service provider...");
                _serviceProvider = services.BuildServiceProvider();
                LogHelper.Log("Service provider built.");

                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    LogHelper.Log("Application lifetime is ClassicDesktopStyleApplicationLifetime.");
                    LogHelper.Log("Resolving LoginView...");
                    var loginView = _serviceProvider.GetService<LoginView>() ?? throw new InvalidOperationException("Failed to resolve LoginView from service provider");
                    LogHelper.Log("LoginView resolved.");
                    desktop.MainWindow = loginView;
                    LogHelper.Log("MainWindow assigned.");
                    
                    LogHelper.Log("Login view displayed");
                }
                else 
                {
                    LogHelper.LogWarning("Application lifetime is NOT ClassicDesktopStyleApplicationLifetime.");
                }

                LogHelper.Log("Calling base.OnFrameworkInitializationCompleted()...");
                base.OnFrameworkInitializationCompleted();
                LogHelper.Log("Finished OnFrameworkInitializationCompleted.");
            }
            catch (Exception ex)
            {
                LogHelper.LogError("FATAL ERROR during application startup in OnFrameworkInitializationCompleted");
                LogHelper.LogException(ex);
                throw; 
            }
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
