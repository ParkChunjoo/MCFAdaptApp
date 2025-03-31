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
using MCFAdaptApp.Domain.Models;

namespace MCFAdaptApp.Avalonia.Views
{
    public partial class SelectPatientView : UserControl
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
        private DataGrid? _patientsDataGrid;
        private ContentControl? _viewContent;
        private Grid? _loadingOverlay;
        private TextBlock? _loadingStatusText;
        
        // Views
        private Grid? _patientView;
        private RegisterView? _registerView;
        private UserControl? _contourView;
        private UserControl? _planView;
        private UserControl? _reviewView;

        public SelectPatientView()
        {
            InitializeComponent();
            
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
            _patientsDataGrid = this.FindControl<DataGrid>("PatientsDataGrid");
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
            if (_patientsDataGrid != null) _patientsDataGrid.SelectionChanged += DataGrid_SelectionChanged;
            
            // Attach the event handler for the Select Reference Plan button
            // This will be done after the visual tree is constructed
            this.AttachedToVisualTree += (s, e) => AttachSelectPlanButtonHandlers();
            
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
                    
                    // Find the parent MainWindow
                    if (this.GetVisualRoot() is MainWindow mainWindow)
                    {
                        // Navigate to Register tab using MainWindow's NavigateToTab method
                        mainWindow.NavigateToTab("register");
                        
                        // Set the patient ID in register view if needed
                        var registerView = mainWindow.FindControl<RegisterView>("RegisterView");
                        if (registerView != null && registerView.DataContext is RegisterViewModel registerViewModel)
                        {
                            registerViewModel.PatientId = patientId;
                        }
                    }
                    else
                    {
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
                        
                            // Set PatientId in RegisterViewModel
                            if (_registerView.DataContext is RegisterViewModel registerViewModel)
                            {
                                registerViewModel.PatientId = patientId;
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
            // Find the parent MainWindow
            if (this.GetVisualRoot() is MainWindow mainWindow)
            {
                // Navigate to Patient tab using MainWindow's NavigateToTab method
                mainWindow.NavigateToTab("patient");
            }
            else
            {
                // Fallback to internal navigation if not hosted in MainWindow
            // Reset all tab buttons
            ResetTabButtons();
            
            // Set this button as active
            if (_patientTabButton != null)
            {
                _patientTabButton.Classes.Remove("tabButton");
                _patientTabButton.Classes.Add("activeTabButton");
            }
            
            // Show the Patient view
            if (_viewContent != null && _patientView != null)
            {
                _viewContent.Content = _patientView;
                }
            }
        }
        
        private void RegisterTabButton_Click(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("[LOG] RegisterTabButton_Click: Handler triggered.");
            // Find the parent MainWindow
            var visualRoot = this.GetVisualRoot();
            Console.WriteLine($"[LOG] RegisterTabButton_Click: VisualRoot type: {visualRoot?.GetType().Name ?? "null"}");

            if (visualRoot is MainWindow mainWindow)
            {
                Console.WriteLine("[LOG] RegisterTabButton_Click: Found MainWindow. Attempting navigation via MainWindow.");
                // Navigate to Register tab using MainWindow's NavigateToTab method
                mainWindow.NavigateToTab("register");
                Console.WriteLine("[LOG] RegisterTabButton_Click: Called mainWindow.NavigateToTab('register').");
                
                // Set the patient ID in register view if needed
                Console.WriteLine("[LOG] RegisterTabButton_Click: Attempting to set PatientId in RegisterView.");
                if (DataContext is SelectPatientViewModel viewModel && viewModel.SelectedPatient != null)
                {
                    var registerView = mainWindow.FindControl<RegisterView>("RegisterView");
                    Console.WriteLine($"[LOG] RegisterTabButton_Click: Found RegisterView in MainWindow: {registerView != null}");
                    if (registerView != null && registerView.DataContext is RegisterViewModel registerViewModel)
                    {
                        Console.WriteLine($"[LOG] RegisterTabButton_Click: Setting PatientId to {viewModel.SelectedPatient.PatientId}");
                        registerViewModel.PatientId = viewModel.SelectedPatient.PatientId;
                    }
                    else
                    {
                        Console.WriteLine("[LOG-WARNING] RegisterTabButton_Click: Could not find RegisterView or its ViewModel in MainWindow.");
                    }
                }
                else 
                {
                     Console.WriteLine("[LOG-WARNING] RegisterTabButton_Click: Could not get SelectPatientViewModel or SelectedPatient from DataContext.");
                }
            }
            else
            {
                Console.WriteLine("[LOG-WARNING] RegisterTabButton_Click: Did not find MainWindow. Using fallback logic (if any).");
                // Fallback logic would go here if needed
            }
            Console.WriteLine("[LOG] RegisterTabButton_Click: Handler finished.");
        }
        
        private void ContourTabButton_Click(object? sender, RoutedEventArgs e)
        {
            // Find the parent MainWindow
            if (this.GetVisualRoot() is MainWindow mainWindow)
            {
                // Navigate to Contour tab using MainWindow's NavigateToTab method
                mainWindow.NavigateToTab("contour");
            }
            else
            {
                // Fallback to internal navigation if not hosted in MainWindow
            // Reset all tab buttons
            ResetTabButtons();
            
            // Set this button as active
            if (_contourTabButton != null)
            {
                _contourTabButton.Classes.Remove("tabButton");
                _contourTabButton.Classes.Add("activeTabButton");
            }
            
            // Show the Contour view
            if (_viewContent != null && _contourView != null)
            {
                _viewContent.Content = _contourView;
                }
            }
        }
        
        private void PlanTabButton_Click(object? sender, RoutedEventArgs e)
        {
            // Find the parent MainWindow
            if (this.GetVisualRoot() is MainWindow mainWindow)
            {
                // Navigate to Plan tab using MainWindow's NavigateToTab method
                mainWindow.NavigateToTab("plan");
            }
            else
            {
                // Fallback to internal navigation if not hosted in MainWindow
            // Reset all tab buttons
            ResetTabButtons();
            
            // Set this button as active
            if (_planTabButton != null)
            {
                _planTabButton.Classes.Remove("tabButton");
                _planTabButton.Classes.Add("activeTabButton");
            }
            
            // Show the Plan view
            if (_viewContent != null && _planView != null)
            {
                _viewContent.Content = _planView;
                }
            }
        }
        
        private void ReviewTabButton_Click(object? sender, RoutedEventArgs e)
        {
            // Find the parent MainWindow
            if (this.GetVisualRoot() is MainWindow mainWindow)
            {
                // Navigate to Review tab using MainWindow's NavigateToTab method
                mainWindow.NavigateToTab("review");
            }
            else
            {
                // Fallback to internal navigation if not hosted in MainWindow
            // Reset all tab buttons
            ResetTabButtons();
            
            // Set this button as active
            if (_reviewTabButton != null)
            {
                _reviewTabButton.Classes.Remove("tabButton");
                _reviewTabButton.Classes.Add("activeTabButton");
            }
            
            // Show the Review view
            if (_viewContent != null && _reviewView != null)
            {
                _viewContent.Content = _reviewView;
                }
            }
        }
        
        private void DataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is SelectPatientViewModel viewModel)
            {
                if (viewModel.SelectedPatient != null)
                {
                    // Update the patient initials
                    UpdatePatientInitials(viewModel.SelectedPatient);
                    
                    // If this patient has anatomy models, fetch them
                    if (viewModel.SelectedPatient.AnatomyModels == null || !viewModel.SelectedPatient.AnatomyModels.Any())
                    {
                        Task.Run(async () =>
                        {
                            try
                            {
                                // Show loading overlay
                                Dispatcher.UIThread.Post(() =>
                                {
                                    if (_loadingOverlay != null) _loadingOverlay.IsVisible = true;
                                    if (_loadingStatusText != null) _loadingStatusText.Text = "Loading patient data...";
                                });
                                
                                // Simulate loading delay - replace with actual data loading
                                await Task.Delay(1000);
                                
                                // Hide loading overlay
                                Dispatcher.UIThread.Post(() =>
                                {
                                    if (_loadingOverlay != null) _loadingOverlay.IsVisible = false;
                                });
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error loading patient data: {ex.Message}");
                                
                                // Hide loading overlay
                                Dispatcher.UIThread.Post(() =>
                                {
                                    if (_loadingOverlay != null) _loadingOverlay.IsVisible = false;
                                });
                            }
                        });
                    }
                }
            }
        }
        
        private void UpdatePatientInitials(MCFAdaptApp.Domain.Models.Patient patient)
        {
            // Get the patient initials
            var firstInitial = !string.IsNullOrEmpty(patient.FirstName) ? patient.FirstName[0].ToString() : "";
            var lastInitial = !string.IsNullOrEmpty(patient.LastName) ? patient.LastName[0].ToString() : "";
            var initials = (firstInitial + lastInitial).ToUpper();
            
            // Find the TextBlock and update it
            var initialsTextBlock = this.FindControl<TextBlock>("PatientInitials");
            if (initialsTextBlock != null)
            {
                initialsTextBlock.Text = initials;
            }
        }
        
        private void ResetTabButtons()
        {
            // Reset all tab buttons to default style
            if (_patientTabButton != null)
            {
                _patientTabButton.Classes.Remove("activeTabButton");
                _patientTabButton.Classes.Add("tabButton");
            }
            
            if (_registerTabButton != null)
            {
                _registerTabButton.Classes.Remove("activeTabButton");
                _registerTabButton.Classes.Add("tabButton");
            }
            
            if (_contourTabButton != null)
            {
                _contourTabButton.Classes.Remove("activeTabButton");
                _contourTabButton.Classes.Add("tabButton");
            }
            
            if (_planTabButton != null)
            {
                _planTabButton.Classes.Remove("activeTabButton");
                _planTabButton.Classes.Add("tabButton");
            }
            
            if (_reviewTabButton != null)
            {
                _reviewTabButton.Classes.Remove("activeTabButton");
                _reviewTabButton.Classes.Add("tabButton");
            }
        }
        
        private void AttachSelectPlanButtonHandlers()
        {
            Console.WriteLine("[LOG] AttachSelectPlanButtonHandlers: Attempting to find and attach handlers.");
            try
            {
                // Find all buttons with the name "SelectReferencePlanButton"
                var buttons = this.GetVisualDescendants()
                    .OfType<Button>()
                    .Where(b => b.Name == "SelectReferencePlanButton")
                    .ToList();
                
                Console.WriteLine($"[LOG] AttachSelectPlanButtonHandlers: Found {buttons.Count} Select Reference Plan buttons.");
                
                // Attach the click handler to each button
                foreach (var button in buttons)
                {
                    Console.WriteLine("[LOG] AttachSelectPlanButtonHandlers: Attaching Click handler to a button.");
                    button.Click -= SelectReferencePlanButton_Click; // Prevent double attachment
                    button.Click += SelectReferencePlanButton_Click;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOG-ERROR] AttachSelectPlanButtonHandlers: Error attaching handlers: {ex.Message}");
            }
            Console.WriteLine("[LOG] AttachSelectPlanButtonHandlers: Finished attaching handlers.");
        }
        
        private async void SelectReferencePlanButton_Click(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("[LOG] SelectReferencePlanButton_Click: Handler triggered.");
            try
            {
                Console.WriteLine("[LOG] SelectReferencePlanButton_Click: Attempting to show loading overlay.");
                // Show loading overlay
                if (_loadingOverlay != null) 
                {
                    _loadingOverlay.IsVisible = true;
                    Console.WriteLine("[LOG] SelectReferencePlanButton_Click: Loading overlay visibility set to true.");
                }
                else
                {
                    Console.WriteLine("[LOG-WARNING] SelectReferencePlanButton_Click: _loadingOverlay is null.");
                }
                
                // Phase 1: Loading CBCT Projections (2 seconds)
                Console.WriteLine("[LOG] SelectReferencePlanButton_Click: Phase 1 - Updating status text.");
                if (_loadingStatusText != null) 
                {
                    _loadingStatusText.Text = "Loading CBCT Projections...";
                    Console.WriteLine("[LOG] SelectReferencePlan_Click: Status text: Loading CBCT Projections...");
                }
                else
                {
                    Console.WriteLine("[LOG-WARNING] SelectReferencePlan_Click: _loadingStatusText is null.");
                }
                
                Console.WriteLine("[LOG] SelectReferencePlan_Click: Phase 1 - Starting delay.");
                await Task.Delay(2000);
                Console.WriteLine("[LOG] SelectReferencePlan_Click: Phase 1 - Delay finished.");
                
                // Phase 2: Loading Reference Plan Data (2 seconds)
                Console.WriteLine("[LOG] SelectReferencePlan_Click: Phase 2 - Updating status text.");
                if (_loadingStatusText != null)
                {
                    _loadingStatusText.Text = "Loading Reference Plan Data...";
                    Console.WriteLine("[LOG] SelectReferencePlan_Click: Status text: Loading Reference Plan Data...");
                }
                
                Console.WriteLine("[LOG] SelectReferencePlan_Click: Phase 2 - Starting delay.");
                await Task.Delay(2000);
                Console.WriteLine("[LOG] SelectReferencePlan_Click: Phase 2 - Delay finished.");
                
                // --- MODIFIED NAVIGATION LOGIC --- 
                Console.WriteLine("[LOG] SelectReferencePlan_Click: Attempting navigation via MainWindow.");
                var visualRoot = this.GetVisualRoot();
                Console.WriteLine($"[LOG] SelectReferencePlan_Click: VisualRoot type: {visualRoot?.GetType().Name ?? "null"}");

                if (visualRoot is MainWindow mainWindow)
                {
                    Console.WriteLine("[LOG] SelectReferencePlan_Click: Found MainWindow. Navigating to 'register' tab.");
                    
                    // Get the required objects from SelectPatientViewModel
                    if (DataContext is SelectPatientViewModel viewModel && viewModel.SelectedPatient != null)
                    {
                        // Find the RegisterView in MainWindow
                        var registerView = mainWindow.FindControl<RegisterView>("RegisterView");
                        Console.WriteLine($"[LOG] SelectReferencePlan_Click: Found RegisterView in MainWindow: {registerView != null}");
                        
                        if (registerView != null && registerView.DataContext is RegisterViewModel registerViewModel)
                        {
                            // Get the selected patient
                            var patient = viewModel.SelectedPatient;
                            Console.WriteLine($"[LOG] SelectReferencePlan_Click: Got Patient: {patient.DisplayName} (ID: {patient.PatientId})");
                            Console.WriteLine($"[LOG] SELECTED PATIENT: ID={patient.PatientId}, Name={patient.FirstName} {patient.LastName}");
                            
                            // Find selected anatomy model (assuming there's one selected or using the first one)
                            AnatomyModel? anatomyModel = null;
                            if (patient.AnatomyModels != null && patient.AnatomyModels.Count > 0)
                            {
                                anatomyModel = patient.AnatomyModels[0]; // For now, pick the first one
                                Console.WriteLine($"[LOG] SelectReferencePlan_Click: Got AnatomyModel: {anatomyModel.Name}");
                                
                                // Find selected reference plan (assuming there's one selected or using the first one)
                                ReferencePlan? referencePlan = null;
                                if (anatomyModel.ReferencePlans != null && anatomyModel.ReferencePlans.Count > 0)
                                {
                                    referencePlan = anatomyModel.ReferencePlans[0]; // For now, pick the first one
                                    Console.WriteLine($"[LOG] SelectReferencePlan_Click: Got ReferencePlan");
                                    
                                    if (referencePlan != null)
                                    {
                                        Console.WriteLine($"[LOG] SELECTED REFERENCE PLAN: {referencePlan.Name}");
                                    }
                                    else
                                    {
                                        Console.WriteLine("[LOG] SELECTED REFERENCE PLAN: None");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("[LOG-WARNING] SelectReferencePlan_Click: No ReferencePlans found in the AnatomyModel.");
                                    Console.WriteLine("[LOG] SELECTED REFERENCE PLAN: None");
                                }
                                
                                // Update patient info in MainWindow
                                Console.WriteLine("[LOG] SelectReferencePlan_Click: Updating patient info in MainWindow");
                                mainWindow.UpdatePatientInfo(patient, anatomyModel, referencePlan);
                                
                                // Set everything on the RegisterViewModel
                                Console.WriteLine("[LOG] SelectReferencePlan_Click: Initializing RegisterViewModel with full objects.");
                                _ = registerViewModel.InitializeAsync(patient, anatomyModel, referencePlan);
                            }
                            else
                            {
                                Console.WriteLine("[LOG-WARNING] SelectReferencePlan_Click: No AnatomyModels found for the Patient.");
                                Console.WriteLine("[LOG] SELECTED REFERENCE PLAN: None");
                                
                                // Update patient info in MainWindow with just the patient
                                Console.WriteLine("[LOG] SelectReferencePlan_Click: Updating patient info in MainWindow (patient only)");
                                mainWindow.UpdatePatientInfo(patient);
                                
                                // Even without anatomy model, set the Patient
                                _ = registerViewModel.InitializeAsync(patient);
                            }
                            
                            // Now navigate to the Register tab
                            mainWindow.NavigateToTab("register");
                            Console.WriteLine("[LOG] SelectReferencePlan_Click: Navigated to Register tab.");
                        }
                        else
                        {
                            Console.WriteLine("[LOG-WARNING] SelectReferencePlan_Click: Could not find RegisterView or its ViewModel in MainWindow.");
                        }
                    }
                    else 
                    {
                         Console.WriteLine("[LOG-WARNING] SelectReferencePlan_Click: Could not get SelectPatientViewModel or SelectedPatient from DataContext.");
                    }
                }
                else
                {
                    Console.WriteLine("[LOG-WARNING] SelectReferencePlan_Click: Did not find MainWindow. Cannot navigate.");
                }
                // --- END OF MODIFIED NAVIGATION LOGIC --- 
                
                // Hide loading overlay
                Console.WriteLine("[LOG] SelectReferencePlan_Click: Attempting to hide loading overlay.");
                if (_loadingOverlay != null)
                {
                    _loadingOverlay.IsVisible = false;
                    Console.WriteLine("[LOG] SelectReferencePlan_Click: Loading overlay visibility set to false.");
                }
                else
                {
                    Console.WriteLine("[LOG-WARNING] SelectReferencePlan_Click: _loadingOverlay is null (when trying to hide).");
                }
                Console.WriteLine("[LOG] SelectReferencePlan_Click: Handler finished.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOG-ERROR] SelectReferencePlan_Click: Exception caught: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Attempt to hide loading overlay even on error
                if (_loadingOverlay != null) _loadingOverlay.IsVisible = false;
            }
        }
    }
}
