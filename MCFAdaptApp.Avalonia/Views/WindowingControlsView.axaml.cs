using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MCFAdaptApp.Avalonia.ViewModels;

namespace MCFAdaptApp.Avalonia.Views
{
    public partial class WindowingControlsView : UserControl
    {
        public WindowingControlsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
