using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using MCFAdaptApp.Avalonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using MCFAdaptApp.Avalonia;
using System.ComponentModel;
using Avalonia.Threading;
using System.Threading.Tasks;
using Avalonia.Input;

namespace MCFAdaptApp.Avalonia.Views
{
    public partial class SelectPatientView : Window
    {
        private Button? _recentPatientsButton;
        private Button? _allPatientsButton;
        private Border? _recentPatientsBorder;
        private Border? _allPatientsBorder;
        private Button? _registerTabButton;
        private Button? _minimizeButton;
        private Button? _restoreMaximizeButton;
        private Button? _closeButton;
        private DataGrid? _patientsDataGrid;
        private Grid? _headerBar;
        private Point _startPoint;
        private bool _isPointerPressed;

        public SelectPatientView()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] SelectPatientView initialized");

            // Get references to UI elements
            _recentPatientsButton = this.FindControl<Button>("RecentPatientsButton");
            _allPatientsButton = this.FindControl<Button>("AllPatientsButton");
            _recentPatientsBorder = this.FindControl<Border>("RecentPatientsBorder");
            _allPatientsBorder = this.FindControl<Border>("AllPatientsBorder");
            _registerTabButton = this.FindControl<Button>("RegisterTabButton");
            _minimizeButton = this.FindControl<Button>("MinimizeButton");
            _restoreMaximizeButton = this.FindControl<Button>("RestoreMaximizeButton");
            _closeButton = this.FindControl<Button>("CloseButton");
            _patientsDataGrid = this.FindControl<DataGrid>("PatientsDataGrid");
            _headerBar = this.FindControl<Grid>("HeaderBar");

            // Attach event handlers
            if (_recentPatientsButton != null) _recentPatientsButton.Click += RecentPatientsButton_Click;
            if (_allPatientsButton != null) _allPatientsButton.Click += AllPatientsButton_Click;
            if (_registerTabButton != null) _registerTabButton.Click += RegisterTabButton_Click;
            if (_minimizeButton != null) _minimizeButton.Click += MinimizeButton_Click;
            if (_restoreMaximizeButton != null) _restoreMaximizeButton.Click += RestoreMaximizeButton_Click;
            if (_closeButton != null) _closeButton.Click += CloseButton_Click;
            if (_patientsDataGrid != null) _patientsDataGrid.SelectionChanged += DataGrid_SelectionChanged;

            // Add window dragging functionality
            if (_headerBar != null)
            {
                _headerBar.PointerPressed += HeaderBar_PointerPressed;
                _headerBar.PointerReleased += HeaderBar_PointerReleased;
                _headerBar.PointerMoved += HeaderBar_PointerMoved;
            }

            // Set DataContext
            if (global::Avalonia.Application.Current is App app)
            {
                DataContext = app.Services.GetService(typeof(SelectPatientViewModel)) as SelectPatientViewModel ?? new SelectPatientViewModel();
            }
            else
            {
                DataContext = new SelectPatientViewModel();
            }

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] SelectPatientView DataContext set to SelectPatientViewModel");
        }

        public SelectPatientView(SelectPatientViewModel viewModel) : this()
        {
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] SelectPatientView DataContext set to provided SelectPatientViewModel");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void RecentPatientsButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_recentPatientsBorder != null) _recentPatientsBorder.Background = new SolidColorBrush(Color.Parse("#FF3F3F46"));
            if (_allPatientsBorder != null) _allPatientsBorder.Background = new SolidColorBrush(Color.Parse("#FF2D2D30"));
        }

        private void AllPatientsButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_recentPatientsBorder != null) _recentPatientsBorder.Background = new SolidColorBrush(Color.Parse("#FF2D2D30"));
            if (_allPatientsBorder != null) _allPatientsBorder.Background = new SolidColorBrush(Color.Parse("#FF3F3F46"));
        }

        private void RegisterTabButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is SelectPatientViewModel viewModel && viewModel.SelectedPatient != null)
            {
                if (global::Avalonia.Application.Current?.ApplicationLifetime is global::Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var mainWindow = desktop.MainWindow as MainWindow;
                    if (mainWindow != null)
                    {
                        var tabControl = mainWindow.FindControl<TabControl>("MainTabControl");
                        if (tabControl != null)
                        {
                            tabControl.SelectedIndex = 1;
                            
                            var registerTab = tabControl.Items[1] as TabItem;
                            if (registerTab?.Content is RegisterView registerView)
                            {
                                if (registerView.DataContext is RegisterViewModel registerViewModel)
                                {
                                    registerViewModel.PatientId = viewModel.SelectedPatient.PatientId;
                                    _ = registerViewModel.InitializeAsync(viewModel.SelectedPatient.PatientId);
                                }
                            }
                        }
                    }
                }
            }
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

        private void DataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Only process if we have added items (new selection)
                if (e.AddedItems == null || e.AddedItems.Count == 0)
                    return;

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] DataGrid_SelectionChanged triggered");

                var patient = e.AddedItems[0] as MCFAdaptApp.Domain.Models.Patient;
                if (patient == null || !(DataContext is SelectPatientViewModel viewModel))
                    return;

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Patient selected in DataGrid: {patient.PatientId}");

                // Update patient initials immediately for responsive UI
                UpdatePatientInitials(patient);

                // Set the selected patient in view model - this will trigger the SelectedPatient setter
                // which now immediately sets the AnatomyModels from cache
                viewModel.SelectedPatient = patient;

                // Force immediate UI update on the UI thread with highest priority
                Dispatcher.UIThread.Post(() =>
                {
                    // Explicitly notify UI that these properties changed
                    viewModel.OnPropertyChanged("SelectedPatient");
                    if (patient.AnatomyModels != null)
                    {
                        // Force anatomy models to update
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Forcing AnatomyModels UI update");
                        viewModel.OnPropertyChanged("SelectedPatient.AnatomyModels");
                    }
                }, DispatcherPriority.Render);

                // Load anatomy models in background without awaiting
                _ = Task.Run(async () =>
                {
                    try
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Loading anatomy models in background");
                        await viewModel.LoadAnatomyModelsAsync(patient.PatientId);

                        // Force another UI update after background loading is complete
                        Dispatcher.UIThread.Post(() =>
                        {
                            viewModel.OnPropertyChanged("SelectedPatient");
                            if (patient.AnatomyModels != null)
                            {
                                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Background loaded models UI update");
                                viewModel.OnPropertyChanged("SelectedPatient.AnatomyModels");
                            }
                        }, DispatcherPriority.Render);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Background loading error: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ERROR in DataGrid_SelectionChanged: {ex.Message}");
            }
        }

        private void UpdatePatientInitials(MCFAdaptApp.Domain.Models.Patient patient)
        {
            var initialsTextBlock = this.FindControl<TextBlock>("PatientInitials");
            if (initialsTextBlock != null)
            {
                string firstName = patient.FirstName ?? string.Empty;
                string lastName = patient.LastName ?? string.Empty;

                string initials = string.Empty;
                if (!string.IsNullOrEmpty(firstName) && firstName.Length > 0)
                {
                    initials += firstName[0];
                }

                if (!string.IsNullOrEmpty(lastName) && lastName.Length > 0)
                {
                    initials += lastName[0];
                }

                // Update UI immediately on UI thread
                Dispatcher.UIThread.Post(() =>
                {
                    initialsTextBlock.Text = initials.ToUpper();
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Updated patient initials to: {initials.ToUpper()}");
                }, DispatcherPriority.Render);
            }
        }

        // Window dragging functionality
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
    }
}
