using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace MCFAdaptApp.Avalonia
{
    public partial class MainWindow : Window
    {
        private Button? _minimizeButton;
        private Button? _restoreMaximizeButton;
        private Button? _closeButton;
        private Grid? _headerBar;
        private Point _startPoint;
        private bool _isPointerPressed;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            _minimizeButton = this.FindControl<Button>("MinimizeButton");
            _restoreMaximizeButton = this.FindControl<Button>("RestoreMaximizeButton");
            _closeButton = this.FindControl<Button>("CloseButton");
            _headerBar = this.FindControl<Grid>("HeaderBar");

            if (_minimizeButton != null) _minimizeButton.Click += MinimizeButton_Click;
            if (_restoreMaximizeButton != null) _restoreMaximizeButton.Click += RestoreMaximizeButton_Click;
            if (_closeButton != null) _closeButton.Click += CloseButton_Click;

            if (_headerBar != null)
            {
                _headerBar.PointerPressed += HeaderBar_PointerPressed;
                _headerBar.PointerReleased += HeaderBar_PointerReleased;
                _headerBar.PointerMoved += HeaderBar_PointerMoved;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void RestoreMaximizeButton_Click(object? sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Close button clicked, closing application");
            this.Close();

            if (global::Avalonia.Application.Current?.ApplicationLifetime is global::Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }

        private void HeaderBar_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                return;

            _isPointerPressed = true;
            _startPoint = e.GetPosition(this);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] HeaderBar_PointerPressed at {_startPoint}");
        }

        private void HeaderBar_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _isPointerPressed = false;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] HeaderBar_PointerReleased");
        }

        private void HeaderBar_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (_isPointerPressed && this.WindowState != WindowState.Maximized)
            {
                var currentPoint = e.GetPosition(this);
                var offset = currentPoint - _startPoint;

                if (offset.X != 0 || offset.Y != 0)
                {
                    Position = new PixelPoint(Position.X + (int)offset.X, Position.Y + (int)offset.Y);
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Window moved to {Position}");
                }
            }
        }

        public void SelectTab(int index)
        {
            var tabControl = this.FindControl<TabControl>("MainTabControl");
            if (tabControl != null && index >= 0 && index < tabControl.Items.Count)
            {
                tabControl.SelectedIndex = index;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Tab selected: {index}");
            }
            else
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Failed to select tab: {index} - Invalid index or TabControl not found");
            }
        }
    }
}
