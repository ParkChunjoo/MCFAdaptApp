using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MCFAdaptApp.Domain.Models;
using MCFAdaptApp.Domain.Services;
using MCFAdaptApp.Infrastructure.Services;
using MCFAdaptApp.Avalonia.Commands;
using System.Linq;
using System.IO;
using System.Collections.Concurrent;
using Avalonia.Threading;
using System.Collections.Generic;
using MCFAdaptApp.Avalonia.Helpers;

namespace MCFAdaptApp.Avalonia.ViewModels
{
    /// <summary>
    /// ViewModel for the patient selection screen
    /// </summary>
    public class SelectPatientViewModel : INotifyPropertyChanged
    {
        #region Private Fields

        private readonly IPatientService _patientService;
        private ObservableCollection<Patient> _patients = new();
        private Patient? _selectedPatient;
        private bool _isLoading;
        private bool _hasError;
        private string _errorMessage = string.Empty;
        private readonly ConcurrentDictionary<string, ObservableCollection<AnatomyModel>> _anatomyModelCache = new();
        private bool _showLoadingOverlay;
        private string _loadingStatusText = "Loading...";

        #endregion

        #region Properties

        /// <summary>
        /// Collection of patients to display
        /// </summary>
        public ObservableCollection<Patient> Patients
        {
            get => _patients;
            set
            {
                _patients = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Currently selected patient
        /// </summary>
        public Patient? SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                // If the same reference, no need to update
                if (ReferenceEquals(_selectedPatient, value))
                    return;

                _selectedPatient = value;

                // Immediately set AnatomyModels from cache if patient is not null
                if (value != null && _anatomyModelCache.TryGetValue(value.PatientId, out var cachedModels))
                {
                    LogHelper.Log($"Setting AnatomyModels from cache immediately: {cachedModels.Count} models");
                    value.AnatomyModels = cachedModels;
                }

                // Notify UI of change
                OnPropertyChanged();
                LogHelper.Log($"Patient selected: {value?.DisplayName ?? "None"}");

                // Update command execution status
                (ViewDetailsCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Indicates if data is currently loading
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                LogHelper.Log($"Loading state changed: {value}");
                // Update command execution status when loading state changes
                (RefreshCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (ViewDetailsCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Indicates if an error has occurred
        /// </summary>
        public bool HasError
        {
            get => _hasError;
            set
            {
                _hasError = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Error message to display
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
                HasError = !string.IsNullOrEmpty(value);

                if (!string.IsNullOrEmpty(value))
                {
                    LogHelper.LogError(value);
                }
            }
        }

        /// <summary>
        /// Command to refresh the patient list
        /// </summary>
        public ICommand RefreshCommand { get; private set; }

        /// <summary>
        /// Command to view patient details
        /// </summary>
        public ICommand ViewDetailsCommand { get; private set; }

        /// <summary>
        /// Command to exit the application
        /// </summary>
        public ICommand? ExitCommand { get; set; }

        /// <summary>
        /// Command to show recent patients sorted by date
        /// </summary>
        public ICommand ShowRecentPatientsCommand { get; private set; }

        /// <summary>
        /// Command to show all patients sorted by last name
        /// </summary>
        public ICommand ShowAllPatientsCommand { get; private set; }

        /// <summary>
        /// "Select As Reference Plan" 버튼 명령
        /// </summary>
        public ICommand SelectAsReferencePlanCommand { get; private set; }

        /// <summary>
        /// Command to select a reference plan and navigate to the Register tab
        /// </summary>
        public ICommand SelectReferencePlanCommand { get; private set; }

        /// <summary>
        /// Reference to the loading overlay in the view
        /// </summary>
        public bool ShowLoadingOverlay
        {
            get => _showLoadingOverlay;
            set
            {
                _showLoadingOverlay = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Text displayed in the loading overlay
        /// </summary>
        public string LoadingStatusText
        {
            get => _loadingStatusText;
            set
            {
                _loadingStatusText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Register 탭으로 이동 이벤트
        /// </summary>
        public event EventHandler<string>? NavigateToRegister;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of SelectPatientViewModel
        /// </summary>
        /// <param name="patientService">Optional patient service for dependency injection</param>
        public SelectPatientViewModel(IPatientService? patientService = null)
        {
            LogHelper.Log("Initializing SelectPatientViewModel");

            // Dependency injection or create default service
            _patientService = patientService ?? new FilePatientService();

            // Initialize commands
            RefreshCommand = new AsyncRelayCommand(LoadPatientsAsync, CanRefresh);
            ViewDetailsCommand = new AsyncRelayCommand(ViewPatientDetailsAsync, CanViewDetails);
            ShowRecentPatientsCommand = new RelayCommand(_ => ShowRecentPatients());
            ShowAllPatientsCommand = new RelayCommand(_ => ShowAllPatients());
            SelectAsReferencePlanCommand = new AsyncRelayCommand(SelectAsReferencePlanAsync, CanSelectAsReferencePlan);
            SelectReferencePlanCommand = new AsyncRelayCommand<ReferencePlan>(SelectReferencePlanAsync);

            // Initialize properties
            ErrorMessage = string.Empty;

            LogHelper.Log("SelectPatientViewModel initialized");

            // Load patients when view model is created
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            // Load patient list first
            await LoadPatientsAsync();

            // After loading patients, preload anatomy models immediately
            await PreloadAllAnatomyModelsAsync();

            LogHelper.Log("Initialization complete with preloaded models");
        }

        /// <summary>
        /// Preloads anatomy models for all patients to improve UI responsiveness 
        /// when selecting patients
        /// </summary>
        private async Task PreloadAllAnatomyModelsAsync()
        {
            try
            {
                LogHelper.Log("Starting preload of anatomy models for all patients");

                // Create tasks for all patients
                var tasks = new List<Task>();

                foreach (var patient in Patients)
                {
                    // If already in cache, skip
                    if (_anatomyModelCache.ContainsKey(patient.PatientId))
                        continue;

                    // Create a task for each patient to load models
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            // Get models from service
                            var models = await _patientService.GetAnatomyModelsAsync(patient.PatientId);

                            if (models != null && models.Count > 0)
                            {
                                // Add to cache
                                var anatomyModels = new ObservableCollection<AnatomyModel>(models);
                                _anatomyModelCache.TryAdd(patient.PatientId, anatomyModels);
                                LogHelper.Log($"Preloaded {anatomyModels.Count} anatomy models for patient: {patient.PatientId}");
                            }
                            else
                            {
                                // Create sample models if none found
                                var sampleModels = CreateSampleModels(patient.PatientId);
                                _anatomyModelCache.TryAdd(patient.PatientId, new ObservableCollection<AnatomyModel>(sampleModels));
                                LogHelper.Log($"Preloaded sample anatomy models for patient: {patient.PatientId}");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.LogError($"Error preloading for patient {patient.PatientId}: {ex.Message}");
                        }
                    }));
                }

                // Wait for a maximum of 3 seconds for preloading to complete
                if (tasks.Count > 0)
                {
                    await Task.WhenAny(
                        Task.WhenAll(tasks),
                        Task.Delay(3000)
                    );
                }

                LogHelper.Log("Completed preloading anatomy models for all patients");
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error in PreloadAllAnatomyModelsAsync: {ex.Message}");
            }
        }

        #endregion

        #region Command Methods

        /// <summary>
        /// Determines if the refresh command can execute
        /// </summary>
        private bool CanRefresh()
        {
            return !IsLoading;
        }

        /// <summary>
        /// Determines if the view details command can execute
        /// </summary>
        private bool CanViewDetails()
        {
            return SelectedPatient != null && !IsLoading;
        }

        /// <summary>
        /// Loads the list of patients asynchronously
        /// </summary>
        public async Task LoadPatientsAsync()
        {
            LogHelper.Log("Loading patients...");

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Get patient data from service
                var patients = await _patientService.GetAllPatientsAsync();

                // Update UI
                Patients.Clear();
                foreach (var patient in patients)
                {
                    Patients.Add(patient);
                }

                LogHelper.Log($"Loaded {Patients.Count} patients");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading patient data: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Views details for the selected patient
        /// </summary>
        private async Task ViewPatientDetailsAsync()
        {
            if (SelectedPatient == null)
                return;

            LogHelper.Log($"Viewing details for patient: {SelectedPatient.DisplayName}");

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Get latest patient data
                var patient = await _patientService.GetPatientByIdAsync(SelectedPatient.PatientId);

                if (patient == null)
                {
                    ErrorMessage = "Selected patient information not found.";
                    return;
                }

                // Here you would implement navigation to detail view
                // For Avalonia, we'll use a dialog service or similar approach

                // Temporary message display - will be replaced with Avalonia dialog
                LogHelper.Log($"Patient Details: {patient.DisplayName}\n" +
                                 $"ID: {patient.PatientId}\n" +
                                 $"Date of Birth: {patient.DateOfBirth?.ToShortDateString() ?? "Not available"}\n" +
                                 $"Gender: {patient.Gender ?? "Not available"}");

                // TODO: Implement Avalonia dialog for patient details

                LogHelper.Log($"Displayed details for patient: {patient.DisplayName}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading patient details: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Shows patients sorted by most recent CBCT scan time
        /// </summary>
        private void ShowRecentPatients()
        {
            LogHelper.Log("Sorting patients by recent CBCT scan time");

            var sortedPatients = new ObservableCollection<Patient>(
                Patients.OrderByDescending(p => p.LastCBCTScanTime ?? DateTime.MinValue)
            );

            Patients = sortedPatients;
        }

        /// <summary>
        /// Shows all patients sorted by last name
        /// </summary>
        private void ShowAllPatients()
        {
            LogHelper.Log("Sorting patients by last name");

            var sortedPatients = new ObservableCollection<Patient>(
                Patients.OrderBy(p => p.LastName)
            );

            Patients = sortedPatients;
        }

        /// <summary>
        /// Loads anatomy models for a specific patient ID
        /// </summary>
        public async Task LoadAnatomyModelsAsync(string patientId)
        {
            if (string.IsNullOrEmpty(patientId))
            {
                LogHelper.Log("LoadAnatomyModelsAsync: Invalid patient ID");
                return;
            }

            LogHelper.Log($"LoadAnatomyModelsAsync called for patient: {patientId}");

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Find the patient by ID
                var patient = Patients.FirstOrDefault(p => p.PatientId == patientId);
                if (patient != null)
                {
                    // Load the patient's anatomy models
                    await LoadPatientPlansAsync(patient);
                }
                else
                {
                    LogHelper.Log($"Patient not found with ID: {patientId}");
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error loading anatomy models: {ex.Message}");
                ErrorMessage = $"Error loading anatomy models: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 선택된 환자의 PlanInfo.txt 파일을 로드하여 AnatomyModel과 ReferencePlan을 설정합니다.
        /// </summary>
        private async Task LoadPatientPlansAsync(Patient patient)
        {
            if (patient == null)
            {
                LogHelper.Log("LoadPatientPlansAsync: Patient is null");
                return;
            }

            LogHelper.Log($"Starting to load plans for patient: {patient.PatientId}");

            try
            {
                // First, check cache - for immediate response
                if (_anatomyModelCache.TryGetValue(patient.PatientId, out var cachedModels) &&
                    cachedModels != null && cachedModels.Count > 0)
                {
                    LogHelper.Log($"Using cached anatomy models for patient: {patient.PatientId}, Count: {cachedModels.Count}");

                    // Set models immediately without async delay for best responsiveness
                    patient.AnatomyModels = cachedModels;

                    // Force UI update on the UI thread - do this immediately, using a direct property path
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        // Update the patient instance's AnatomyModels directly
                        LogHelper.Log("Updated UI with cached models");
                        OnPropertyChanged(nameof(SelectedPatient));

                        // Also explicitly notify about AnatomyModels property
                        OnPropertyChanged("SelectedPatient.AnatomyModels");
                    }, DispatcherPriority.Render);

                    return;
                }

                // Not in cache, need to load from service
                LogHelper.Log("No cached models found, loading from service");

                // For better UX, show loading indicator and immediately create a placeholder
                // to show something right away
                var placeholderModels = CreateSampleModels(patient.PatientId);
                patient.AnatomyModels = new ObservableCollection<AnatomyModel>(placeholderModels);

                // Force UI update with placeholder data immediately
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    LogHelper.Log("Updated UI with placeholder models");
                    OnPropertyChanged(nameof(SelectedPatient));
                    OnPropertyChanged("SelectedPatient.AnatomyModels");
                }, DispatcherPriority.Render);

                // Now load the real data in background
                var models = await _patientService.GetAnatomyModelsAsync(patient.PatientId);

                if (models == null || models.Count == 0)
                {
                    // Already showing placeholder data, so just add to cache
                    _anatomyModelCache.TryAdd(patient.PatientId, new ObservableCollection<AnatomyModel>(placeholderModels));
                    LogHelper.Log("Using placeholder models as no models found from service");
                    return;
                }

                // Got real data, update UI and cache
                var anatomyModels = new ObservableCollection<AnatomyModel>(models);

                // Add to cache
                _anatomyModelCache.TryAdd(patient.PatientId, anatomyModels);

                // Only update if this is still the selected patient
                if (_selectedPatient?.PatientId == patient.PatientId)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        patient.AnatomyModels = anatomyModels;
                        LogHelper.Log("Updated UI with real models from service");

                        // Notify about both properties
                        OnPropertyChanged(nameof(SelectedPatient));
                        OnPropertyChanged("SelectedPatient.AnatomyModels");
                    }, DispatcherPriority.Render);
                }

                LogHelper.Log($"Loaded {anatomyModels.Count} anatomy models for patient: {patient.PatientId}");
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error loading PlanInfo.txt: {ex.Message}");
                ErrorMessage = $"Error loading patient plans: {ex.Message}";
            }
        }

        // Helper method to create sample models for testing
        private List<AnatomyModel> CreateSampleModels(string patientId)
        {
            LogHelper.Log($"Creating sample anatomy models for patient: {patientId}");

            var models = new List<AnatomyModel>();

            // Create a sample anatomy model
            var model = new AnatomyModel
            {
                Name = "Default Anatomy Model",
                Status = "Available",
                ModifiedDate = DateTime.Now,
                ReferencePlans = new ObservableCollection<ReferencePlan>()
            };

            // Add some sample reference plans
            model.ReferencePlans.Add(new ReferencePlan
            {
                Name = "Default Reference Plan",
                Description = "Sample reference plan for testing purposes",
                Status = "Available",
                ModifiedDate = DateTime.Now
            });

            models.Add(model);

            return models;
        }

        /// <summary>
        /// "Select As Reference Plan" 버튼 명령이 실행 가능한지 확인
        /// </summary>
        private bool CanSelectAsReferencePlan()
        {
            return SelectedPatient != null && !IsLoading;
        }

        /// <summary>
        /// "Select As Reference Plan" 버튼 명령 실행
        /// </summary>
        private async Task SelectAsReferencePlanAsync()
        {
            if (SelectedPatient == null)
            {
                ErrorMessage = "환자가 선택되지 않았습니다.";
                return;
            }

            try
            {
                IsLoading = true;
                ClearError();

                LogHelper.Log($"Navigating to Register tab for patient: {SelectedPatient.PatientId}");

                // Register 탭으로 이동하는 이벤트 발생
                OnNavigateToRegister(SelectedPatient.PatientId);

                LogHelper.Log("Navigation to Register tab completed");
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error navigating to Register tab: {ex.Message}");
                ErrorMessage = $"Register 탭으로 이동 중 오류 발생: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Register 탭으로 이동 이벤트 발생
        /// </summary>
        protected virtual void OnNavigateToRegister(string patientId)
        {
            NavigateToRegister?.Invoke(this, patientId);
        }

        /// <summary>
        /// 오류 메시지 초기화
        /// </summary>
        private void ClearError()
        {
            ErrorMessage = string.Empty;
            HasError = false;
        }

        /// <summary>
        /// Processes a reference plan selection and navigates to the Register tab
        /// </summary>
        /// <param name="referencePlan">The selected reference plan</param>
        private async Task SelectReferencePlanAsync(ReferencePlan? referencePlan)
        {
            if (referencePlan == null)
            {
                LogHelper.Log("SelectReferencePlanAsync: ReferencePlan is null");
                ErrorMessage = "No reference plan selected.";
                return;
            }

            try
            {
                // Update UI to show loading
                LogHelper.Log($"Reference plan selected: {referencePlan.Name}");
                ShowLoadingOverlay = true;
                ClearError();

                // First phase - Loading CBCT projections
                LoadingStatusText = "Loading CBCT Projections...";
                LogHelper.Log("Loading CBCT Projections...");
                await Task.Delay(2000); // Simulate work for 2 seconds

                // Second phase - Loading reference plan data
                LoadingStatusText = "Loading Reference Plan Data...";
                LogHelper.Log("Loading Reference Plan Data...");
                await Task.Delay(2000); // Simulate work for 2 seconds

                // Hide loading overlay
                ShowLoadingOverlay = false;

                // Store selected reference plan if needed
                // This could be used in the Register tab
                LogHelper.Log($"Loading complete, storing reference plan: {referencePlan.Name}");

                // Finally, navigate to the Register tab (ensure patient is selected)
                if (SelectedPatient != null)
                {
                    LogHelper.Log($"Triggering navigation to Register tab for patient: {SelectedPatient.PatientId}");
                    OnNavigateToRegister(SelectedPatient.PatientId);
                }
                else
                {
                    LogHelper.Log("WARNING: Cannot navigate - SelectedPatient is null");
                    ErrorMessage = "No patient is selected. Please select a patient.";
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error processing reference plan selection: {ex.Message}");
                ErrorMessage = $"Error processing reference plan: {ex.Message}";
                ShowLoadingOverlay = false;
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        /// <summary>
        /// Event raised when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
