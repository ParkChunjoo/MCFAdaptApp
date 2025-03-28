using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MCFAdaptApp.Avalonia.ViewModels;

namespace MCFAdaptApp.Avalonia
{
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; }

        public MainWindow()
        {
            ViewModel = new MainWindowViewModel();
            DataContext = ViewModel;
            
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
