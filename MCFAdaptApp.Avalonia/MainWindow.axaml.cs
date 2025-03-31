using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MCFAdaptApp.Avalonia.ViewModels;
using MCFAdaptApp.Domain.Models;
using System;

namespace MCFAdaptApp.Avalonia
{
    public partial class MainWindow : Window
    {
        private TabControl? _mainTabControl;
        private Button? _minimizeButton;
        private Button? _restoreMaximizeButton;
        private Button? _closeButton;
        private Grid? _headerBar;
        
        // Patient info panel controls
        private StackPanel? _patientInfoPanel;
        private StackPanel? _patientInfoTextPanel;
        private TextBlock? _patientInitialsText;
        private TextBlock? _patientNameText;
        private TextBlock? _patientDetailsText;
        private TextBlock? _patientIdText;
        private TextBlock? _anatomyModelText;
        private TextBlock? _referencePlanText;
        private TextBlock? _logoText;

        // Content views
        private Control? _patientView;
        private Control? _registerView;
        private Control? _contourView;
        private Control? _planView;
        private Control? _reviewView;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            SetupWindowControls();
        }

        public MainWindow(SelectPatientViewModel viewModel) : this()
        {
            // If a view model is provided, set it as DataContext for the SelectPatientView
            if (this.FindControl<Control>("PatientView") is Control patientView)
            {
                patientView.DataContext = viewModel;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            _mainTabControl = this.FindControl<TabControl>("MainTabControl");
            _minimizeButton = this.FindControl<Button>("MinimizeButton");
            _restoreMaximizeButton = this.FindControl<Button>("RestoreMaximizeButton");
            _closeButton = this.FindControl<Button>("CloseButton");
            _headerBar = this.FindControl<Grid>("HeaderBar");
            
            // Initialize patient info panel controls - updated to match new structure
            _patientInfoPanel = this.FindControl<StackPanel>("PatientInfoPanel");
            _patientInfoTextPanel = this.FindControl<StackPanel>("PatientInfoTextPanel");
            _patientInitialsText = this.FindControl<TextBlock>("PatientInitialsText");
            _patientNameText = this.FindControl<TextBlock>("PatientNameText");
            _patientDetailsText = this.FindControl<TextBlock>("PatientDetailsText");
            _patientIdText = this.FindControl<TextBlock>("PatientIdText");
            _anatomyModelText = this.FindControl<TextBlock>("AnatomyModelText");
            _referencePlanText = this.FindControl<TextBlock>("ReferencePlanText");
            _logoText = this.FindControl<TextBlock>("LogoText");
            
            // Initialize content views
            _patientView = this.FindControl<Control>("PatientView");
            _registerView = this.FindControl<Control>("RegisterView");
            _contourView = this.FindControl<Control>("ContourView");
            _planView = this.FindControl<Control>("PlanView");
            _reviewView = this.FindControl<Control>("ReviewView");
            
            // Attach tab selection changed event
            if (_mainTabControl != null)
            {
                _mainTabControl.SelectionChanged += MainTabControl_SelectionChanged;
            }
            
            // Check if all controls were found
            if (_patientInfoPanel == null || _patientInitialsText == null || _patientNameText == null || 
                _patientDetailsText == null || _patientIdText == null || _anatomyModelText == null || 
                _referencePlanText == null || _logoText == null)
            {
                Console.WriteLine("[ERROR] MainWindow.InitializeComponent: Failed to find one or more patient info controls.");
                
                // Log which controls are missing
                Console.WriteLine($"  PatientInfoPanel: {_patientInfoPanel != null}");
                Console.WriteLine($"  PatientInitialsText: {_patientInitialsText != null}");
                Console.WriteLine($"  PatientNameText: {_patientNameText != null}");
                Console.WriteLine($"  PatientDetailsText: {_patientDetailsText != null}");
                Console.WriteLine($"  PatientIdText: {_patientIdText != null}");
                Console.WriteLine($"  AnatomyModelText: {_anatomyModelText != null}");
                Console.WriteLine($"  ReferencePlanText: {_referencePlanText != null}");
                Console.WriteLine($"  LogoText: {_logoText != null}");
            }
            else
            {
                Console.WriteLine("[LOG] MainWindow.InitializeComponent: Successfully found all patient info controls.");
            }
        }

        private void MainTabControl_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_mainTabControl != null)
            {
                int index = _mainTabControl.SelectedIndex;
                Console.WriteLine($"[LOG] MainWindow.MainTabControl_SelectionChanged: Tab index changed to {index}");
                UpdateContentVisibility(index);
            }
        }

        private void UpdateContentVisibility(int selectedIndex)
        {
            // Hide all views first
            if (_patientView != null) _patientView.IsVisible = false;
            if (_registerView != null) _registerView.IsVisible = false;
            if (_contourView != null) _contourView.IsVisible = false;
            if (_planView != null) _planView.IsVisible = false;
            if (_reviewView != null) _reviewView.IsVisible = false;
            
            // Show only the selected view
            switch (selectedIndex)
            {
                case 0: // Patient
                    if (_patientView != null) _patientView.IsVisible = true;
                    break;
                case 1: // Register
                    if (_registerView != null) _registerView.IsVisible = true;
                    break;
                case 2: // Contour
                    if (_contourView != null) _contourView.IsVisible = true;
                    break;
                case 3: // Plan
                    if (_planView != null) _planView.IsVisible = true;
                    break;
                case 4: // Review
                    if (_reviewView != null) _reviewView.IsVisible = true;
                    break;
            }
        }

        private void SetupWindowControls()
        {
            if (_headerBar != null)
            {
                _headerBar.PointerPressed += (sender, e) =>
                {
                    if (e.GetCurrentPoint(_headerBar).Properties.IsLeftButtonPressed)
                    {
                        BeginMoveDrag(e);
                    }
                };
            }
            else
            {
                Console.WriteLine("[ERROR] MainWindow.SetupWindowControls: _headerBar is null");
            }

            if (_minimizeButton != null)
            {
                _minimizeButton.Click += (sender, e) => 
                {
                    WindowState = WindowState.Minimized;
                };
            }
            else
            {
                Console.WriteLine("[ERROR] MainWindow.SetupWindowControls: _minimizeButton is null");
            }

            if (_restoreMaximizeButton != null)
            {
                _restoreMaximizeButton.Click += (sender, e) => 
                {
                    WindowState = WindowState == WindowState.Maximized 
                        ? WindowState.Normal 
                        : WindowState.Maximized;
                };
            }
            else
            {
                Console.WriteLine("[ERROR] MainWindow.SetupWindowControls: _restoreMaximizeButton is null");
            }

            if (_closeButton != null)
            {
                _closeButton.Click += (sender, e) => 
                {
                    Close();
                };
            }
            else
            {
                Console.WriteLine("[ERROR] MainWindow.SetupWindowControls: _closeButton is null");
            }
        }

        public void NavigateToTab(string tabName)
        {
            Console.WriteLine($"[LOG] MainWindow.NavigateToTab: Called with tabName: '{tabName}'.");
            if (_mainTabControl == null) 
            {
                Console.WriteLine("[LOG-WARNING] MainWindow.NavigateToTab: _mainTabControl is null. Cannot navigate.");
                return;
            }

            int targetIndex = -1;
            switch (tabName.ToLower())
            {
                case "patient":
                    targetIndex = 0;
                    break;
                case "register":
                    targetIndex = 1;
                    break;
                case "contour":
                    targetIndex = 2;
                    break;
                case "plan":
                    targetIndex = 3;
                    break;
                case "review":
                    targetIndex = 4;
                    break;
                default:
                    Console.WriteLine($"[LOG-WARNING] MainWindow.NavigateToTab: Unknown tabName '{tabName}'.");
                    break;
            }

            if (targetIndex != -1)
            {
                Console.WriteLine($"[LOG] MainWindow.NavigateToTab: Setting SelectedIndex to {targetIndex}.");
                _mainTabControl.SelectedIndex = targetIndex;
                UpdateContentVisibility(targetIndex);
                Console.WriteLine($"[LOG] MainWindow.NavigateToTab: SelectedIndex set to {_mainTabControl.SelectedIndex}.");
            }
        }
        
        /// <summary>
        /// Updates the patient information panel with the provided data
        /// </summary>
        public void UpdatePatientInfo(Patient patient, AnatomyModel? anatomyModel = null, ReferencePlan? referencePlan = null)
        {
            Console.WriteLine($"[LOG] MainWindow.UpdatePatientInfo: Updating patient info panel for {patient.DisplayName}");
            
            // Check if all required controls are available
            if (_patientInfoPanel == null)
            {
                Console.WriteLine("[ERROR] MainWindow.UpdatePatientInfo: _patientInfoPanel is null");
                return;
            }
            
            if (_patientInitialsText == null)
            {
                Console.WriteLine("[ERROR] MainWindow.UpdatePatientInfo: _patientInitialsText is null");
                return;
            }
            
            if (_patientNameText == null)
            {
                Console.WriteLine("[ERROR] MainWindow.UpdatePatientInfo: _patientNameText is null");
                return;
            }
            
            if (_patientDetailsText == null)
            {
                Console.WriteLine("[ERROR] MainWindow.UpdatePatientInfo: _patientDetailsText is null");
                return;
            }
            
            if (_patientIdText == null)
            {
                Console.WriteLine("[ERROR] MainWindow.UpdatePatientInfo: _patientIdText is null");
                return;
            }
            
            if (_logoText == null)
            {
                Console.WriteLine("[ERROR] MainWindow.UpdatePatientInfo: _logoText is null");
                return;
            }
            
            try
            {
                // Get patient initials
                string firstInitial = !string.IsNullOrEmpty(patient.FirstName) ? patient.FirstName[0].ToString() : "";
                string lastInitial = !string.IsNullOrEmpty(patient.LastName) ? patient.LastName[0].ToString() : "";
                string initials = (firstInitial + lastInitial).ToUpper();
                
                // Update the UI elements
                _patientInitialsText.Text = initials;
                _patientNameText.Text = $"{patient.LastName} {patient.FirstName}";
                
                // Format date of birth if available
                string dobText = patient.DateOfBirth.HasValue 
                    ? patient.DateOfBirth.Value.ToString("MM/dd/yy") 
                    : "";
                
                _patientDetailsText.Text = dobText;
                _patientIdText.Text = $"ID: {patient.PatientId}";
                
                // Only update these if they exist (they're hidden but might be used later)
                if (_anatomyModelText != null && anatomyModel != null)
                {
                    _anatomyModelText.Text = $"Model: {anatomyModel.Name}";
                }
                
                if (_referencePlanText != null && referencePlan != null)
                {
                    _referencePlanText.Text = $"Plan: {referencePlan.Name}";
                }
                
                // Show the panel and hide the logo
                _patientInfoPanel.IsVisible = true;
                _logoText.IsVisible = false;
                
                Console.WriteLine("[LOG] MainWindow.UpdatePatientInfo: Patient info panel updated successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] MainWindow.UpdatePatientInfo: Exception occurred: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
        
        /// <summary>
        /// Hides the patient information panel and shows the logo
        /// </summary>
        public void HidePatientInfo()
        {
            Console.WriteLine("[LOG] MainWindow.HidePatientInfo: Hiding patient info panel");
            
            if (_patientInfoPanel == null)
            {
                Console.WriteLine("[ERROR] MainWindow.HidePatientInfo: _patientInfoPanel is null");
                return;
            }
            
            if (_logoText == null)
            {
                Console.WriteLine("[ERROR] MainWindow.HidePatientInfo: _logoText is null");
                return;
            }
            
            try
            {
                _patientInfoPanel.IsVisible = false;
                _logoText.IsVisible = true;
                
                Console.WriteLine("[LOG] MainWindow.HidePatientInfo: Patient info panel hidden successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] MainWindow.HidePatientInfo: Exception occurred: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
