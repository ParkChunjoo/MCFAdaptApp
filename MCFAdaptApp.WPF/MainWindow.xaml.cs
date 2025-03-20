using System;
using System.Windows;
using MCFAdaptApp.Domain.Services;
using MCFAdaptApp.WPF.ViewModels;
using MCFAdaptApp.WPF.Views;
using Microsoft.Extensions.DependencyInjection;

namespace MCFAdaptApp.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // 뷰모델 설정
        SetupViewModels();
    }
    
    private void SetupViewModels()
    {
        try
        {
            // 서비스 프로바이더 가져오기
            var serviceProvider = ((App)System.Windows.Application.Current)._serviceProvider;
            
            // Register 탭의 뷰모델 설정
            var registerTab = MainTabControl.Items[1] as System.Windows.Controls.TabItem;
            if (registerTab != null)
            {
                var registerView = registerTab.Content as RegisterView;
                if (registerView != null)
                {
                    // 의존성 주입을 통해 RegisterViewModel 생성
                    var dicomService = serviceProvider.GetService<IDicomService>() ?? throw new InvalidOperationException("Failed to resolve IDicomService from service provider");
                    var registerViewModel = new RegisterViewModel(dicomService);
                    
                    // 뷰의 DataContext 설정
                    registerView.DataContext = registerViewModel;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Error setting up view models: {ex.Message}");
        }
    }
}