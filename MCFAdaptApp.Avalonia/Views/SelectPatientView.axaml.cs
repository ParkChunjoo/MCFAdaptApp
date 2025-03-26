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
using Avalonia.Layout;
using System.Linq;
using Avalonia.VisualTree;

namespace MCFAdaptApp.Avalonia.Views
{
    public partial class SelectPatientView : Window
    {
        private Button? _recentPatientsButton;
        private Button? _allPatientsButton;
        private Border? _recentPatientsBorder;
        private Border? _allPatientsBorder;
        private Button? _patientTabButton;
        private Button? _registerTabButton;
        private Button? _contourTabButton;
        private Button? _planTabButton;
        private Button? _reviewTabButton;
        private Button? _minimizeButton;
        private Button? _restoreMaximizeButton;
        private Button? _closeButton;
        private DataGrid? _patientsDataGrid;
        private Grid? _headerBar;
        private ContentControl? _viewContent;
        private Grid? _loadingOverlay;
        private TextBlock? _loadingStatusText;
        private Point _startPoint;
        private bool _isPointerPressed;
        
        // Views
        private Grid? _patientView;
        private RegisterView? _registerView;
        private UserControl? _contourView;
        private UserControl? _planView;
        private UserControl? _reviewView;

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
            _patientTabButton = this.FindControl<Button>("PatientTabButton");
            _registerTabButton = this.FindControl<Button>("RegisterTabButton");
            _contourTabButton = this.FindControl<Button>("ContourTabButton");
            _planTabButton = this.FindControl<Button>("PlanTabButton");
            _reviewTabButton = this.FindControl<Button>("ReviewTabButton");
            _minimizeButton = this.FindControl<Button>("MinimizeButton");
            _restoreMaximizeButton = this.FindControl<Button>("RestoreMaximizeButton");
            _closeButton = this.FindControl<Button>("CloseButton");
            _patientsDataGrid = this.FindControl<DataGrid>("PatientsDataGrid");
            _headerBar = this.FindControl<Grid>("HeaderBar");
            _viewContent = this.FindControl<ContentControl>("ViewContent");
            _loadingOverlay = this.FindControl<Grid>("LoadingOverlay");
            _loadingStatusText = this.FindControl<TextBlock>("LoadingStatusText");
            
            // Save patient view from content template
            _patientView = _viewContent?.Content as Grid;
            
            // Initialize other views
            InitializeViews();
            
            // Attach event handlers
            if (_recentPatientsButton != null) _recentPatientsButton.Click += RecentPatientsButton_Click;
            if (_allPatientsButton != null) _allPatientsButton.Click += AllPatientsButton_Click;
            if (_patientTabButton != null) _patientTabButton.Click += PatientTabButton_Click;
            if (_registerTabButton != null) _registerTabButton.Click += RegisterTabButton_Click;
            if (_contourTabButton != null) _contourTabButton.Click += ContourTabButton_Click;
            if (_planTabButton != null) _planTabButton.Click += PlanTabButton_Click;
            if (_reviewTabButton != null) _reviewTabButton.Click += ReviewTabButton_Click;
            if (_minimizeButton != null) _minimizeButton.Click += MinimizeButton_Click;
            if (_restoreMaximizeButton != null) _restoreMaximizeButton.Click += RestoreMaximizeButton_Click;
            if (_closeButton != null) _closeButton.Click += CloseButton_Click;
            if (_patientsDataGrid != null) _patientsDataGrid.SelectionChanged += DataGrid_SelectionChanged;
            
            // Attach the event handler for the Select Reference Plan button
            // This will be done after the visual tree is constructed
            this.AttachedToVisualTree += (s, e) => AttachSelectPlanButtonHandlers();
            
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
                var viewModel = app.Services.GetService(typeof(SelectPatientViewModel)) as SelectPatientViewModel;
                
                if (viewModel != null)
                {
                    // Subscribe to navigation events
                    viewModel.NavigateToRegister += ViewModel_NavigateToRegister;
                    DataContext = viewModel;
                }
                else
                {
                    DataContext = new SelectPatientViewModel();
                }
            }
            else
            {
                DataContext = new SelectPatientViewModel();
            }
            
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] SelectPatientView DataContext set to SelectPatientViewModel");
        }

        private void ViewModel_NavigateToRegister(object? sender, string patientId)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ViewModel requested navigation to Register tab for patient: {patientId}");
            
            // Ensure we're on the UI thread
            Dispatcher.UIThread.Post(() => 
            {
                try 
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Executing RegisterTabButton_Click on UI thread");
                    
                    // Make sure RegisterTabButton is not null
                    if (_registerTabButton == null)
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ERROR: RegisterTabButton is null");
                        return;
                    }
                    
                    // Reset tab buttons
                    ResetTabButtons();
                    
                    // Set the Register tab as active
                    _registerTabButton.Classes.Remove("tabButton");
                    _registerTabButton.Classes.Add("activeTabButton");
                    
                    // Directly set the content if we have a valid RegisterView
                    if (_viewContent != null && _registerView != null)
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Directly setting ViewContent to RegisterView");
                        
                        // Set PatientId in RegisterViewModel if needed
                        if (_registerView.DataContext is RegisterViewModel registerViewModel && 
                            DataContext is SelectPatientViewModel viewModel && 
                            viewModel.SelectedPatient != null)
                        {
                            registerViewModel.PatientId = viewModel.SelectedPatient.PatientId;
                        }
                        
                        // Set the content
                        _viewContent.Content = _registerView;
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ViewContent updated successfully");
                    }
                    else
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] WARNING: ViewContent or RegisterView is null");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ERROR during tab navigation: {ex.Message}");
                }
            }, DispatcherPriority.Normal);
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
        
        private void InitializeViews()
        {
            // Create RegisterView
            if (global::Avalonia.Application.Current is App app)
            {
                _registerView = app.Services.GetService(typeof(RegisterView)) as RegisterView;
                
                // Create placeholder views for other tabs
                // In a real application, you'd create proper views for these
                _contourView = new UserControl
                {
                    Content = new TextBlock
                    {
                        Text = "Contour View - Coming Soon",
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = 24,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    Background = new SolidColorBrush(Color.Parse("#FF252526"))
                };
                
                _planView = new UserControl
                {
                    Content = new TextBlock
                    {
                        Text = "Plan View - Coming Soon",
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = 24,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    Background = new SolidColorBrush(Color.Parse("#FF252526"))
                };
                
                _reviewView = new UserControl
                {
                    Content = new TextBlock
                    {
                        Text = "Review View - Coming Soon",
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = 24,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    Background = new SolidColorBrush(Color.Parse("#FF252526"))
                };
            }
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
        
        private void PatientTabButton_Click(object? sender, RoutedEventArgs e)
        {
            ResetTabButtons();
            if (_patientTabButton != null)
            {
                _patientTabButton.Classes.Remove("tabButton");
                _patientTabButton.Classes.Add("activeTabButton");
            }
            
            if (_viewContent != null && _patientView != null)
            {
                _viewContent.Content = _patientView;
            }
        }
        
        private void RegisterTabButton_Click(object? sender, RoutedEventArgs e)
        {
            // Only allow navigation to RegisterView if a patient is selected
            if (DataContext is SelectPatientViewModel viewModel && viewModel.SelectedPatient != null)
            {
                ResetTabButtons();
                if (_registerTabButton != null)
                {
                    _registerTabButton.Classes.Remove("tabButton");
                    _registerTabButton.Classes.Add("activeTabButton");
                }
                
                if (_registerView != null && _viewContent != null)
                {
                    // Set PatientId in RegisterViewModel
                    if (_registerView.DataContext is RegisterViewModel registerViewModel)
                    {
                        registerViewModel.PatientId = viewModel.SelectedPatient.PatientId;
                    }
                    
                    // Display the RegisterView
                    _viewContent.Content = _registerView;
                }
            }
            else
            {
                // Show a message to the user that they need to select a patient
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Cannot navigate to Register tab: No patient selected");
                // You could add more user-friendly notification here
            }
        }
        
        private void ContourTabButton_Click(object? sender, RoutedEventArgs e)
        {
            ResetTabButtons();
            if (_contourTabButton != null)
            {
                _contourTabButton.Classes.Remove("tabButton");
                _contourTabButton.Classes.Add("activeTabButton");
            }
            
            if (_viewContent != null && _contourView != null)
            {
                _viewContent.Content = _contourView;
            }
        }
        
        private void PlanTabButton_Click(object? sender, RoutedEventArgs e)
        {
            ResetTabButtons();
            if (_planTabButton != null)
            {
                _planTabButton.Classes.Remove("tabButton");
                _planTabButton.Classes.Add("activeTabButton");
            }
            
            if (_viewContent != null && _planView != null)
            {
                _viewContent.Content = _planView;
            }
        }
        
        private void ReviewTabButton_Click(object? sender, RoutedEventArgs e)
        {
            ResetTabButtons();
            if (_reviewTabButton != null)
            {
                _reviewTabButton.Classes.Remove("tabButton");
                _reviewTabButton.Classes.Add("activeTabButton");
            }
            
            if (_viewContent != null && _reviewView != null)
            {
                _viewContent.Content = _reviewView;
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
        
        private void ResetTabButtons()
        {
            if (_patientTabButton != null) _patientTabButton.Classes.Remove("activeTabButton");
            if (_patientTabButton != null) _patientTabButton.Classes.Add("tabButton");
            
            if (_registerTabButton != null) _registerTabButton.Classes.Remove("activeTabButton");
            if (_registerTabButton != null) _registerTabButton.Classes.Add("tabButton");
            
            if (_contourTabButton != null) _contourTabButton.Classes.Remove("activeTabButton");
            if (_contourTabButton != null) _contourTabButton.Classes.Add("tabButton");
            
            if (_planTabButton != null) _planTabButton.Classes.Remove("activeTabButton");
            if (_planTabButton != null) _planTabButton.Classes.Add("tabButton");
            
            if (_reviewTabButton != null) _reviewTabButton.Classes.Remove("activeTabButton");
            if (_reviewTabButton != null) _reviewTabButton.Classes.Add("tabButton");
        }

        private void AttachSelectPlanButtonHandlers()
        {
            try
            {
                // Find all buttons with specific content
                var planButtons = this.GetVisualDescendants()
                    .OfType<Button>()
                    .Where(b => b.Content?.ToString() == "Select As Reference Plan");
                
                int count = 0;
                foreach (var button in planButtons)
                {
                    button.Click += SelectReferencePlanButton_Click;
                    count++;
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Attached click handler to Select Plan button {count}");
                }
                
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Found and attached handlers to {count} reference plan buttons");
                
                if (count == 0)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] WARNING: No reference plan buttons found to attach handlers");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Error attaching Select Plan button handlers: {ex.Message}");
            }
        }
        
        private async void SelectReferencePlanButton_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Select As Reference Plan button clicked");
                
                // Get reference to the loading overlay and status text
                var loadingOverlay = this.FindControl<Grid>("LoadingOverlay");
                var statusText = this.FindControl<TextBlock>("LoadingStatusText");
                
                // Show loading overlay
                if (loadingOverlay != null)
                {
                    loadingOverlay.IsVisible = true;
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Loading overlay is now visible");
                }
                
                // Phase 1: Loading CBCT Projections (2 seconds)
                if (statusText != null)
                {
                    statusText.Text = "Loading CBCT Projections...";
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Status: Loading CBCT Projections...");
                }
                
                await Task.Delay(2000); // Wait for 2 seconds
                
                // Phase 2: Loading Reference Plan Data (2 seconds)
                if (statusText != null)
                {
                    statusText.Text = "Loading Reference Plan Data...";
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Status: Loading Reference Plan Data...");
                }
                
                await Task.Delay(2000); // Wait for 2 seconds
                
                // Hide loading overlay
                if (loadingOverlay != null)
                {
                    loadingOverlay.IsVisible = false;
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Loading overlay is now hidden");
                }
                
                // Switch to Register tab
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Directly switching to Register tab");
                
                // Direct tab switching - reuse the tab button click method
                if (_registerTabButton != null)
                {
                    // Reset all tabs first
                    ResetTabButtons();
                    
                    // Set Register tab as active
                    _registerTabButton.Classes.Remove("tabButton");
                    _registerTabButton.Classes.Add("activeTabButton");
                    
                    // Manually set the content control to display RegisterView
                    if (_viewContent != null && _registerView != null)
                    {
                        // Set the Register view as the content
                        _viewContent.Content = _registerView;
                        
                        // Update the RegisterViewModel with patient ID if needed
                        if (_registerView.DataContext is RegisterViewModel registerViewModel &&
                            DataContext is SelectPatientViewModel viewModel && 
                            viewModel.SelectedPatient != null)
                        {
                            registerViewModel.PatientId = viewModel.SelectedPatient.PatientId;
                        }
                        
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Register view now displayed");
                    }
                    else
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ERROR: Cannot switch to Register view - _viewContent or _registerView is null");
                    }
                }
                else
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ERROR: Cannot switch to Register tab - _registerTabButton is null");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ERROR in SelectReferencePlanButton_Click: {ex.Message}");
            }
        }
    }
}
