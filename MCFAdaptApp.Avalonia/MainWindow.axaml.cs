using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MCFAdaptApp.Avalonia.Helpers;
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
        private Border? _logoContainer;
        private Image? _logoImage;

        // Tool buttons panel
        private StackPanel? _toolButtonsPanel;
        private Button? _zoomButton;
        private Button? _panButton;
        private Button? _measureButton;
        private Button? _autoRigidButton;
        private Button? _manualRigidButton;
        private Button? _deformButton;

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
            _logoContainer = this.FindControl<Border>("LogoContainer");
            _logoImage = this.FindControl<Image>("LogoImage");

            // Initialize tool buttons panel
            _toolButtonsPanel = this.FindControl<StackPanel>("ToolButtonsPanel");
            _zoomButton = this.FindControl<Button>("ZoomButton");
            _panButton = this.FindControl<Button>("PanButton");
            _measureButton = this.FindControl<Button>("MeasureButton");
            _autoRigidButton = this.FindControl<Button>("AutoRigidButton");
            _manualRigidButton = this.FindControl<Button>("ManualRigidButton");
            _deformButton = this.FindControl<Button>("DeformButton");

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
                _referencePlanText == null || _logoContainer == null || _logoImage == null)
            {
                LogHelper.LogError("MainWindow.InitializeComponent: Failed to find one or more patient info controls.");

                // Log which controls are missing
                LogHelper.Log($"  PatientInfoPanel: {_patientInfoPanel != null}");
                LogHelper.Log($"  PatientInitialsText: {_patientInitialsText != null}");
                LogHelper.Log($"  PatientNameText: {_patientNameText != null}");
                LogHelper.Log($"  PatientDetailsText: {_patientDetailsText != null}");
                LogHelper.Log($"  PatientIdText: {_patientIdText != null}");
                LogHelper.Log($"  AnatomyModelText: {_anatomyModelText != null}");
                LogHelper.Log($"  ReferencePlanText: {_referencePlanText != null}");
                LogHelper.Log($"  LogoContainer: {_logoContainer != null}");
                LogHelper.Log($"  LogoImage: {_logoImage != null}");
            }
            else
            {
                LogHelper.Log("MainWindow.InitializeComponent: Successfully found all patient info controls.");
            }
        }

        private void MainTabControl_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_mainTabControl != null)
            {
                int index = _mainTabControl.SelectedIndex;
                LogHelper.Log($"MainWindow.MainTabControl_SelectionChanged: Tab index changed to {index}");
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

            // Hide tool buttons by default
            if (_toolButtonsPanel != null) _toolButtonsPanel.IsVisible = false;

            // Show only the selected view
            switch (selectedIndex)
            {
                case 0: // Patient
                    if (_patientView != null) _patientView.IsVisible = true;
                    break;
                case 1: // Register
                    if (_registerView != null) _registerView.IsVisible = true;
                    // Show tool buttons for Register view
                    if (_toolButtonsPanel != null) _toolButtonsPanel.IsVisible = true;
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
                LogHelper.LogError("MainWindow.SetupWindowControls: _headerBar is null");
            }

            // Setup tool buttons
            SetupToolButtons();

            if (_minimizeButton != null)
            {
                _minimizeButton.Click += (sender, e) =>
                {
                    WindowState = WindowState.Minimized;
                };
            }
            else
            {
                LogHelper.LogError("MainWindow.SetupWindowControls: _minimizeButton is null");
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
                LogHelper.LogError("MainWindow.SetupWindowControls: _restoreMaximizeButton is null");
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
                LogHelper.LogError("MainWindow.SetupWindowControls: _closeButton is null");
            }
        }

        public void NavigateToTab(string tabName)
        {
            LogHelper.Log($"MainWindow.NavigateToTab: Called with tabName: '{tabName}'.");
            if (_mainTabControl == null)
            {
                LogHelper.LogWarning("MainWindow.NavigateToTab: _mainTabControl is null. Cannot navigate.");
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
                    LogHelper.LogWarning($"MainWindow.NavigateToTab: Unknown tabName '{tabName}'.");
                    break;
            }

            if (targetIndex != -1)
            {
                LogHelper.Log($"MainWindow.NavigateToTab: Setting SelectedIndex to {targetIndex}.");
                _mainTabControl.SelectedIndex = targetIndex;
                UpdateContentVisibility(targetIndex);
                LogHelper.Log($"MainWindow.NavigateToTab: SelectedIndex set to {_mainTabControl.SelectedIndex}.");
            }
        }

        /// <summary>
        /// Updates the patient information panel with the provided data
        /// </summary>
        public void UpdatePatientInfo(Patient patient, RTStructure? structure = null, RTPlan? plan = null)
        {
            LogHelper.Log($"MainWindow.UpdatePatientInfo: Updating patient info panel for {patient.DisplayName}");

            // Check if all required controls are available
            if (_patientInfoPanel == null)
            {
                LogHelper.LogError("MainWindow.UpdatePatientInfo: _patientInfoPanel is null");
                return;
            }

            if (_patientInitialsText == null)
            {
                LogHelper.LogError("MainWindow.UpdatePatientInfo: _patientInitialsText is null");
                return;
            }

            if (_patientNameText == null)
            {
                LogHelper.LogError("MainWindow.UpdatePatientInfo: _patientNameText is null");
                return;
            }

            if (_patientDetailsText == null)
            {
                LogHelper.LogError("MainWindow.UpdatePatientInfo: _patientDetailsText is null");
                return;
            }

            if (_patientIdText == null)
            {
                LogHelper.LogError("MainWindow.UpdatePatientInfo: _patientIdText is null");
                return;
            }

            if (_logoContainer == null || _logoImage == null)
            {
                LogHelper.LogError("MainWindow.UpdatePatientInfo: _logoContainer or _logoImage is null");
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
                if (_anatomyModelText != null && structure != null)
                {
                    _anatomyModelText.Text = $"Structure: {structure.Name}";
                }

                if (_referencePlanText != null && plan != null)
                {
                    _referencePlanText.Text = $"Plan: {plan.Name}";
                }

                // Show the panel and hide the logo
                _patientInfoPanel.IsVisible = true;
                _logoContainer.IsVisible = false;

                LogHelper.Log("MainWindow.UpdatePatientInfo: Patient info panel updated successfully");
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"MainWindow.UpdatePatientInfo: Exception occurred: {ex.Message}");
                LogHelper.Log(ex.StackTrace);
            }
        }

        /// <summary>
        /// Hides the patient information panel and shows the logo
        /// </summary>
        public void HidePatientInfo()
        {
            LogHelper.Log("MainWindow.HidePatientInfo: Hiding patient info panel");

            // Check if all required controls are available
            if (_patientInfoPanel == null)
            {
                LogHelper.LogError("MainWindow.HidePatientInfo: _patientInfoPanel is null");
                return;
            }

            if (_logoContainer == null || _logoImage == null)
            {
                LogHelper.LogError("MainWindow.HidePatientInfo: _logoContainer or _logoImage is null");
                return;
            }

            try
            {
                // Hide the panel and show the logo
                _patientInfoPanel.IsVisible = false;
                _logoContainer.IsVisible = true;

                LogHelper.Log("MainWindow.HidePatientInfo: Patient info panel hidden successfully");
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"MainWindow.HidePatientInfo: Exception occurred: {ex.Message}");
                LogHelper.Log(ex.StackTrace);
            }
        }

        /// <summary>
        /// Sets up the tool buttons with their event handlers
        /// </summary>
        private void SetupToolButtons()
        {
            // Check if buttons are available
            if (_zoomButton == null || _panButton == null || _measureButton == null ||
                _autoRigidButton == null || _manualRigidButton == null || _deformButton == null)
            {
                LogHelper.LogWarning("MainWindow.SetupToolButtons: One or more tool buttons are null");
                return;
            }

            // Add click event handlers for each button
            _zoomButton.Click += (sender, e) => {
                LogHelper.Log("Zoom button clicked");
                // TODO: Implement zoom functionality
            };

            _panButton.Click += (sender, e) => {
                LogHelper.Log("Pan button clicked");
                // TODO: Implement pan functionality
            };

            _measureButton.Click += (sender, e) => {
                LogHelper.Log("Measure button clicked");
                // TODO: Implement measure functionality
            };

            _autoRigidButton.Click += (sender, e) => {
                LogHelper.Log("Auto Rigid button clicked");
                // TODO: Implement auto rigid registration functionality
            };

            _manualRigidButton.Click += (sender, e) => {
                LogHelper.Log("Manual Rigid button clicked");
                // TODO: Implement manual rigid registration functionality
            };

            _deformButton.Click += (sender, e) => {
                LogHelper.Log("Deform button clicked");
                // TODO: Implement deformable registration functionality
            };

            LogHelper.Log("MainWindow.SetupToolButtons: Tool buttons setup complete");
        }
    }
}
