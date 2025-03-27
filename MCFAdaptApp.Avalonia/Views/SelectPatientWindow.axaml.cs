using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MCFAdaptApp.Avalonia.ViewModels;
using System;

namespace MCFAdaptApp.Avalonia.Views
{
    public partial class SelectPatientWindow : Window
    {
        private Button? _minimizeButton;
        private Button? _restoreMaximizeButton;
        private Button? _closeButton;
        private Grid? _headerBar;
        private SelectPatientView? _patientView;
        private Point _startPoint;
        private bool _isPointerPressed;
        
        public SelectPatientWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            
            // Get references to UI elements
            _minimizeButton = this.FindControl<Button>("MinimizeButton");
            _restoreMaximizeButton = this.FindControl<Button>("RestoreMaximizeButton");
            _closeButton = this.FindControl<Button>("CloseButton");
            _headerBar = this.FindControl<Grid>("HeaderBar");
            _patientView = this.FindControl<SelectPatientView>("PatientView");
            
            // Attach event handlers
            if (_minimizeButton != null) _minimizeButton.Click += MinimizeButton_Click;
            if (_restoreMaximizeButton != null) _restoreMaximizeButton.Click += RestoreMaximizeButton_Click;
            if (_closeButton != null) _closeButton.Click += CloseButton_Click;
            
            // Add window dragging functionality
            if (_headerBar != null)
            {
                _headerBar.PointerPressed += HeaderBar_PointerPressed;
                _headerBar.PointerReleased += HeaderBar_PointerReleased;
                _headerBar.PointerMoved += HeaderBar_PointerMoved;
            }
        }
        
        public SelectPatientWindow(SelectPatientViewModel viewModel) : this()
        {
            if (_patientView != null)
            {
                _patientView.DataContext = viewModel;
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
            // Close the window
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Close button clicked, closing application");
            this.Close();
            
            // If needed, can also exit the application entirely
            if (global::Avalonia.Application.Current?.ApplicationLifetime is global::Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }
        
        // Window dragging functionality
        private void HeaderBar_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                return;
                
            _isPointerPressed = true;
            _startPoint = e.GetPosition(this);
        }
        
        private void HeaderBar_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _isPointerPressed = false;
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
                }
            }
        }
    }
} 