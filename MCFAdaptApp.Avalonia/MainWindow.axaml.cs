using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;

namespace MCFAdaptApp.Avalonia
{
    public partial class MainWindow : Window
    {
        private TabControl? _mainTabControl;
        
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            
            _mainTabControl = this.FindControl<TabControl>("MainTabControl");
        }

        public void SelectRegisterTab()
        {
            if (_mainTabControl != null)
            {
                _mainTabControl.SelectedIndex = 1; // Register tab is at index 1
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
