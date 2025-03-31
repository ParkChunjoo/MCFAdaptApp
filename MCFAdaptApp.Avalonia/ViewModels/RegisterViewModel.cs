using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MCFAdaptApp.Domain.Models;
using MCFAdaptApp.Domain.Services;
using MCFAdaptApp.Avalonia.Commands;
using MCFAdaptApp.Avalonia.Helpers;

namespace MCFAdaptApp.Avalonia.ViewModels
{
    /// <summary>
    /// Register 탭의 ViewModel
    /// </summary>
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly IDicomService _dicomService;
        private ReferenceCT? _cbct;
        private ReferenceCT? _referenceCT;
        private string _patientId = string.Empty;
        private Patient? _patient;
        private AnatomyModel? _selectedAnatomyModel;
        private ReferencePlan? _selectedReferencePlan;
        private bool _isLoading;
        private string? _statusMessage;
        private bool _hasError;
        private string _errorMessage = string.Empty;

        /// <summary>
        /// CBCT 이미지 데이터
        /// </summary>
        public ReferenceCT CBCT
        {
            get => _cbct;
            set
            {
                _cbct = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 참조 CT 이미지 데이터
        /// </summary>
        public ReferenceCT ReferenceCT
        {
            get => _referenceCT;
            set
            {
                _referenceCT = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 환자 ID
        /// </summary>
        public string PatientId
        {
            get => _patientId;
            set
            {
                _patientId = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 환자 정보
        /// </summary>
        public Patient? Patient
        {
            get => _patient;
            set
            {
                _patient = value;
                if (value != null)
                {
                    PatientId = value.PatientId;
                }
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 선택된 해부학적 모델
        /// </summary>
        public AnatomyModel? SelectedAnatomyModel
        {
            get => _selectedAnatomyModel;
            set
            {
                _selectedAnatomyModel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 선택된 참조 계획
        /// </summary>
        public ReferencePlan? SelectedReferencePlan
        {
            get => _selectedReferencePlan;
            set
            {
                _selectedReferencePlan = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 로딩 중 여부
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                UpdateCommandsCanExecute();
            }
        }

        /// <summary>
        /// 상태 메시지
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 오류 발생 여부
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
        /// 오류 메시지
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
                HasError = !string.IsNullOrEmpty(value);
            }
        }

        /// <summary>
        /// DICOM 파일 로드 명령
        /// </summary>
        public ICommand LoadDicomFilesCommand { get; private set; }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="dicomService">DICOM 서비스</param>
        public RegisterViewModel(IDicomService dicomService)
        {
            _dicomService = dicomService ?? throw new ArgumentNullException(nameof(dicomService));
            
            // 명령 초기화
            LoadDicomFilesCommand = new AsyncRelayCommand(LoadDicomFilesAsync, () => !IsLoading);
        }

        /// <summary>
        /// DICOM 파일 로드
        /// </summary>
        private async Task LoadDicomFilesAsync()
        {
            if (string.IsNullOrEmpty(PatientId))
            {
                ErrorMessage = "환자 ID가 지정되지 않았습니다.";
                return;
            }

            try
            {
                IsLoading = true;
                ClearError();
                StatusMessage = "DICOM 파일을 로드하는 중...";

                // CBCT 로드
                CBCT = await _dicomService.LoadCBCTAsync(PatientId);
                
                // 참조 CT 로드
                ReferenceCT = await _dicomService.LoadReferenceCTAsync(PatientId);

                // If we have a selected reference plan but haven't loaded its CT yet,
                // try to load it specifically
                if (SelectedReferencePlan != null && ReferenceCT == null)
                {
                    // This is a placeholder for loading CT from a specific plan
                    // You might want to modify your IDicomService to support this
                    // ReferenceCT = await _dicomService.LoadReferenceCTFromPlanAsync(SelectedReferencePlan);
                }

                if (CBCT == null && ReferenceCT == null)
                {
                    ErrorMessage = "DICOM 파일을 찾을 수 없습니다.";
                    return;
                }

                StatusMessage = "DICOM 파일 로드 완료";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"DICOM 파일 로드 중 오류 발생: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
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
        /// 명령 실행 가능 여부 업데이트
        /// </summary>
        private void UpdateCommandsCanExecute()
        {
            (LoadDicomFilesCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// 환자 ID 설정 및 DICOM 파일 로드
        /// </summary>
        /// <param name="patientId">환자 ID</param>
        public async Task InitializeAsync(string patientId)
        {
            PatientId = patientId;
            await LoadDicomFilesAsync();
        }

        /// <summary>
        /// 환자 정보, 선택된 해부학적 모델 및 참조 계획 설정
        /// </summary>
        /// <param name="patient">환자 정보</param>
        /// <param name="anatomyModel">선택된 해부학적 모델</param>
        /// <param name="referencePlan">선택된 참조 계획</param>
        public async Task InitializeAsync(Patient patient, AnatomyModel anatomyModel = null, ReferencePlan referencePlan = null)
        {
            // Log the full details of what we received
            LogHelper.Log("RegisterViewModel.InitializeAsync: Received data:");
            LogHelper.Log($"Patient: {patient?.PatientId ?? "null"} - {patient?.FirstName ?? ""} {patient?.LastName ?? ""}");
            LogHelper.Log($"AnatomyModel: {anatomyModel?.Name ?? "null"}");
            LogHelper.Log($"ReferencePlan: {referencePlan?.Name ?? "null"}");

            Patient = patient;
            SelectedAnatomyModel = anatomyModel;
            SelectedReferencePlan = referencePlan;
            
            // If PatientId is set (from the Patient object), load DICOM files
            if (!string.IsNullOrEmpty(PatientId))
            {
                await LoadDicomFilesAsync();
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
