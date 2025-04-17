using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MCFAdaptApp.Avalonia.Commands;
using MCFAdaptApp.Avalonia.Controls;
using MCFAdaptApp.Avalonia.Helpers;
using MCFAdaptApp.Avalonia.ViewModels;
using MCFAdaptApp.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MCFAdaptApp.Avalonia.Views
{
    public partial class RegisterView : UserControl
    {
        public RegisterView()
        {
            InitializeComponent();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] RegisterView initialized");

            // Set DataContext
            DataContext = App.Current.Services.GetService<RegisterViewModel>() ??
                          new RegisterViewModel(App.Current.Services.GetService<IDicomService>());

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] RegisterView DataContext set to RegisterViewModel");
        }

        public RegisterView(RegisterViewModel viewModel) : this()
        {
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] RegisterView DataContext set to provided RegisterViewModel");
        }

        private MedicalImageView? _refCtView;
        private MedicalImageView? _cbctView;
        private bool _measurementModeActive = false;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            // Get references to image views
            _refCtView = this.FindControl<MedicalImageView>("RefCtView");
            _cbctView = this.FindControl<MedicalImageView>("CbctView");

            if (_refCtView != null && _cbctView != null)
            {
                // Set up view synchronization
                _refCtView.ViewNavigated += OnRefCtViewNavigated;
                _cbctView.ViewNavigated += OnCbctViewNavigated;

                LogHelper.Log("RegisterView: MedicalImageView controls initialized and event handlers set up");
            }
            else
            {
                LogHelper.LogError("RegisterView: Failed to find MedicalImageView controls");
            }
        }

        /// <summary>
        /// Handle navigation events from Reference CT view
        /// </summary>
        private void OnRefCtViewNavigated(object? sender, NavigationEventArgs e)
        {
            if (DataContext is RegisterViewModel vm && vm.SyncViews && _cbctView != null)
            {
                // Apply same navigation to CBCT view
                if (e.NavigationType == NavigationType.Pan)
                {
                    _cbctView.PanOffset = new Point(
                        _cbctView.PanOffset.X + e.PanDelta.X,
                        _cbctView.PanOffset.Y + e.PanDelta.Y);
                }
                else if (e.NavigationType == NavigationType.Zoom)
                {
                    _cbctView.ZoomFactor = Math.Max(0.1,
                        Math.Min(10.0, _cbctView.ZoomFactor + e.ZoomDelta));
                }
            }
        }

        /// <summary>
        /// Handle navigation events from CBCT view
        /// </summary>
        private void OnCbctViewNavigated(object? sender, NavigationEventArgs e)
        {
            if (DataContext is RegisterViewModel vm && vm.SyncViews && _refCtView != null)
            {
                // Apply same navigation to Reference CT view
                if (e.NavigationType == NavigationType.Pan)
                {
                    _refCtView.PanOffset = new Point(
                        _refCtView.PanOffset.X + e.PanDelta.X,
                        _refCtView.PanOffset.Y + e.PanDelta.Y);
                }
                else if (e.NavigationType == NavigationType.Zoom)
                {
                    _refCtView.ZoomFactor = Math.Max(0.1,
                        Math.Min(10.0, _refCtView.ZoomFactor + e.ZoomDelta));
                }
            }
        }

        /// <summary>
        /// Toggle measurement mode on both views
        /// </summary>
        public void ToggleMeasurementMode()
        {
            _measurementModeActive = !_measurementModeActive;

            if (_refCtView != null)
                _refCtView.MeasurementMode = _measurementModeActive;

            if (_cbctView != null)
                _cbctView.MeasurementMode = _measurementModeActive;

            LogHelper.Log($"Measurement mode {(_measurementModeActive ? "activated" : "deactivated")}");
        }

        /// <summary>
        /// Connect the ToggleMeasurementModeCommand from ViewModel to the local method
        /// </summary>
        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is RegisterViewModel viewModel)
            {
                // Connect the command to our local method
                viewModel.ToggleMeasurementModeCommand = new RelayCommand(_ => ToggleMeasurementMode());
            }
        }
    }
}
