using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using MCFAdaptApp.Domain.Services;
using MCFAdaptApp.Infrastructure.Services;
using MCFAdaptApp.Avalonia.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

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

                var selectPatientView = _serviceProvider.GetService<SelectPatientView>() ?? throw new InvalidOperationException("Failed to resolve SelectPatientView from service provider");
                
                var mainWindow = new MainWindow();
                desktop.MainWindow = mainWindow;
                
                if (selectPatientView.DataContext is ViewModels.SelectPatientViewModel viewModel)
                {
                    viewModel.NavigateToRegister += (sender, patientId) =>
                    {
                        mainWindow.SelectRegisterTab();
                        
                        var tabControl = mainWindow.FindControl<TabControl>("MainTabControl");
                        if (tabControl != null)
                        {
                            var registerTab = tabControl.Items[1] as TabItem;
                            if (registerTab?.Content is Views.RegisterView registerView && 
                                registerView.DataContext is ViewModels.RegisterViewModel registerViewModel)
                            {
                                registerViewModel.PatientId = patientId;
                                Task.Run(() => registerViewModel.InitializeAsync(patientId));
                            }
                        }
                    };
                }

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Main window displayed");
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
