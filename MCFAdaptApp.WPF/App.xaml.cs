using System;
using System.Windows;
using MCFAdaptApp.Domain.Services;
using MCFAdaptApp.Infrastructure.Services;
using MCFAdaptApp.WPF.Views;
using Microsoft.Extensions.DependencyInjection;

namespace MCFAdaptApp.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public readonly ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // 서비스 등록
            services.AddSingleton<IAuthenticationService, SimpleAuthenticationService>();
            services.AddSingleton<IPatientService, FilePatientService>();
            services.AddSingleton<IDicomService, DicomService>();

            // 뷰 등록
            services.AddTransient<LoginView>();
            services.AddTransient<SelectPatientView>();
            services.AddTransient<RegisterView>();

            // 뷰모델 등록
            services.AddTransient<ViewModels.LoginViewModel>();
            services.AddTransient<ViewModels.SelectPatientViewModel>();
            services.AddTransient<ViewModels.RegisterViewModel>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // 애플리케이션 시작 시 로그 출력
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Application starting up");
            
            // Create and show the login view
            var loginView = _serviceProvider.GetService<LoginView>() ?? throw new InvalidOperationException("Failed to resolve LoginView from service provider");
            loginView.Show();
            
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Login view displayed");
        }
    }
}