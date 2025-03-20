using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MCFAdaptApp.Domain.Models;
using MCFAdaptApp.Domain.Services;
using MCFAdaptApp.Infrastructure.Services;
using MCFAdaptApp.WPF.Commands;
using System.Linq;
using System.IO;
using System.Collections.Concurrent;

namespace MCFAdaptApp.WPF.ViewModels
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
                _selectedPatient = value;
                OnPropertyChanged();
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Patient selected: {value?.DisplayName ?? "None"}");
                // Update command execution status when selected patient changes
                (ViewDetailsCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                
                // 환자가 선택되면 PlanInfo.txt 파일을 로드합니다.
                if (value != null)
                {
                    _ = LoadPatientPlansAsync(value);
                }
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
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Loading state changed: {value}");
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
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ERROR: {value}");
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
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Initializing SelectPatientViewModel");
            
            // Dependency injection or create default service
            _patientService = patientService ?? new FilePatientService();
            
            // Initialize commands
            RefreshCommand = new AsyncRelayCommand(LoadPatientsAsync, CanRefresh);
            ViewDetailsCommand = new AsyncRelayCommand(ViewPatientDetailsAsync, CanViewDetails);
            ShowRecentPatientsCommand = new RelayCommand(_ => ShowRecentPatients());
            ShowAllPatientsCommand = new RelayCommand(_ => ShowAllPatients());
            SelectAsReferencePlanCommand = new AsyncRelayCommand(SelectAsReferencePlanAsync, CanSelectAsReferencePlan);
            
            // Initialize properties
            ErrorMessage = string.Empty;
            
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] SelectPatientViewModel initialized");
            
            // Load patients when view model is created
            _ = LoadPatientsAsync();
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
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Loading patients...");
            
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
                
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Loaded {Patients.Count} patients");
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

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Viewing details for patient: {SelectedPatient.DisplayName}");
            
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
                // Example: _navigationService.NavigateTo(new PatientDetailsView(patient));
                
                // Temporary message display
                System.Windows.MessageBox.Show(
                    $"Patient Details: {patient.DisplayName}\n" +
                    $"ID: {patient.PatientId}\n" +
                    $"Date of Birth: {patient.DateOfBirth?.ToShortDateString() ?? "Not available"}\n" +
                    $"Gender: {patient.Gender ?? "Not available"}",
                    "Patient Details",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
                
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Displayed details for patient: {patient.DisplayName}");
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
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Sorting patients by recent CBCT scan time");
            
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
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Sorting patients by last name");
            
            var sortedPatients = new ObservableCollection<Patient>(
                Patients.OrderBy(p => p.LastName)
            );
            
            Patients = sortedPatients;
        }

        /// <summary>
        /// 선택된 환자의 PlanInfo.txt 파일을 로드하여 AnatomyModel과 ReferencePlan을 설정합니다.
        /// </summary>
        private async Task LoadPatientPlansAsync(Patient patient)
        {
            if (patient == null)
                return;
                
            try
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Starting to load plans for patient: {patient.PatientId}");
                
                // 캐시에서 확인 (즉시 반환)
                if (_anatomyModelCache.TryGetValue(patient.PatientId, out var cachedModels))
                {
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        patient.AnatomyModels = cachedModels;
                    });
                    IsLoading = false;
                    return;
                }
                
                // Get anatomy models from service
                var models = await _patientService.GetAnatomyModelsAsync(patient.PatientId);
                
                if (models == null || models.Count == 0)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] No anatomy models found for patient: {patient.PatientId}");
                    return;
                }
                
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var anatomyModels = new ObservableCollection<AnatomyModel>(models);
                    _anatomyModelCache.TryAdd(patient.PatientId, anatomyModels);
                    patient.AnatomyModels = anatomyModels;
                });
                
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Loaded {patient.AnatomyModels.Count} anatomy models for patient: {patient.PatientId}");
                
                // 속성 변경 알림
                OnPropertyChanged(nameof(SelectedPatient));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Error loading PlanInfo.txt: {ex.Message}");
                ErrorMessage = $"Error loading patient plans: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
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
                
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Navigating to Register tab for patient: {SelectedPatient.PatientId}");
                
                // Register 탭으로 이동하는 이벤트 발생
                OnNavigateToRegister(SelectedPatient.PatientId);
                
                // 비동기 작업 추가
                await Task.Delay(100); // 최소한의 비동기 작업 추가
                
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Navigation to Register tab completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Error navigating to Register tab: {ex.Message}");
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

        #endregion

        #region INotifyPropertyChanged Implementation

        /// <summary>
        /// Event raised when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}