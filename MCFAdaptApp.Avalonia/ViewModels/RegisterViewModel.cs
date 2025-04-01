using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.Media;
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
        private RTStructure? _rtStructure;
        private RTPlan? _rtPlan;
        private RTDose? _rtDose;
        private string _patientId = string.Empty;
        private Patient? _patient;
        private AnatomyModel? _selectedAnatomyModel;
        private ReferencePlan? _selectedReferencePlan;
        private bool _isLoading;
        private string? _statusMessage;
        private bool _hasError;
        private string _errorMessage = string.Empty;
        private Bitmap? _cbctCenterSliceBitmap;
        private Bitmap? _refCtCenterSliceBitmap;

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
        /// </summary>
        public RTStructure RTStructure
        {
            get => _rtStructure;
            set
            {
                _rtStructure = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// </summary>
        public RTPlan RTPlan
        {
            get => _rtPlan;
            set
            {
                _rtPlan = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// </summary>
        public RTDose RTDose
        {
            get => _rtDose;
            set
            {
                _rtDose = value;
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
        /// Bitmap of the center slice of the CBCT volume
        /// </summary>
        public Bitmap? CbctCenterSliceBitmap
        {
            get => _cbctCenterSliceBitmap;
            private set // Private setter as it's updated internally
            {
                _cbctCenterSliceBitmap = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Bitmap of the center slice of the Reference CT volume
        /// </summary>
        public Bitmap? RefCtCenterSliceBitmap
        {
            get => _refCtCenterSliceBitmap;
            private set // Private setter as it's updated internally
            {
                _refCtCenterSliceBitmap = value;
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
                CbctCenterSliceBitmap = null; // Clear previous bitmaps
                RefCtCenterSliceBitmap = null;

                // CBCT 로드
                StatusMessage = "CBCT 프로젝션 로드 중...";
                CBCT = await _dicomService.LoadCBCTAsync(PatientId);
                if (CBCT?.PixelData != null)
                {
                    StatusMessage = "CBCT 슬라이스 생성 중...";
                    // Run bitmap creation on a background thread to avoid blocking UI
                    CbctCenterSliceBitmap = await Task.Run(() => CreateBitmapFromSlice(CBCT));
                }

                // 참조 CT 로드
                StatusMessage = "참조 CT 로드 중...";
                ReferenceCT = await _dicomService.LoadReferenceCTAsync(PatientId);
                if (ReferenceCT?.PixelData != null)
                {
                    StatusMessage = "참조 CT 슬라이스 생성 중...";
                    // Run bitmap creation on a background thread
                    RefCtCenterSliceBitmap = await Task.Run(() => CreateBitmapFromSlice(ReferenceCT));
                }

                StatusMessage = "RT Structure 로드 중...";
                RTStructure = await _dicomService.LoadRTStructureAsync(PatientId);

                StatusMessage = "RT Plan 로드 중...";
                RTPlan = await _dicomService.LoadRTPlanAsync(PatientId);

                StatusMessage = "RT Dose 로드 중...";
                RTDose = await _dicomService.LoadRTDoseAsync(PatientId);

                if (CBCT == null && ReferenceCT == null)
                {
                    ErrorMessage = "DICOM 파일을 찾을 수 없습니다.";
                    StatusMessage = "오류 발생";
                    return;
                }

                StatusMessage = "DICOM 파일 로드 완료";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"DICOM 파일 로드 중 오류 발생: {ex.Message}";
                LogHelper.LogException(ex);
                StatusMessage = "오류 발생";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private Bitmap? CreateBitmapFromSlice(ReferenceCT ctVolume)
        {
            if (ctVolume?.PixelData == null || ctVolume.Width <= 0 || ctVolume.Height <= 0 || ctVolume.Depth <= 0)
            {
                return null;
            }

            try
            {
                int width = ctVolume.Width;
                int height = ctVolume.Height;
                int centerSliceIndex = ctVolume.Depth / 2;
                int sliceSize = width * height;
                int startOffset = centerSliceIndex * sliceSize;

                // Extract center slice data
                short[] slicePixels = new short[sliceSize];
                Buffer.BlockCopy(ctVolume.PixelData, startOffset * sizeof(short), slicePixels, 0, sliceSize * sizeof(short));

                // Find min/max pixel values for windowing (simple approach)
                short minPixel = short.MaxValue;
                short maxPixel = short.MinValue;
                for (int i = 0; i < slicePixels.Length; i++)
                {
                    if (slicePixels[i] < minPixel) minPixel = slicePixels[i];
                    if (slicePixels[i] > maxPixel) maxPixel = slicePixels[i];
                }

                // Define window level/width (adjust these for better contrast if needed)
                double windowCenter = (maxPixel + minPixel) / 2.0;
                double windowWidth = maxPixel - minPixel;
                if (windowWidth == 0) windowWidth = 1; // Avoid division by zero
                double windowMin = windowCenter - windowWidth / 2.0;

                // Create a WriteableBitmap
                var bitmap = new WriteableBitmap(
                    new PixelSize(width, height),
                    new Vector(96, 96),
                    PixelFormat.Bgra8888,
                    AlphaFormat.Premul);

                using (var frameBuffer = bitmap.Lock())
                {
                    unsafe // Use unsafe context for direct pointer access to framebuffer
                    {
                        uint* pixels = (uint*)frameBuffer.Address;
                        for (int i = 0; i < sliceSize; i++)
                        {
                            // Get the 16-bit pixel value
                            short rawValue = slicePixels[i];

                            // Apply windowing to map to 0-255 grayscale
                            double scaledValue = (rawValue - windowMin) / windowWidth;
                            byte grayValue = (byte)Math.Clamp(scaledValue * 255.0, 0, 255);

                            // Set Bgra8888 pixel (Alpha = 255, R=G=B=grayValue)
                            pixels[i] = (uint)((255 << 24) | (grayValue << 16) | (grayValue << 8) | grayValue);
                        }
                    }
                }

                return bitmap;
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error creating bitmap from slice for {ctVolume.Name}: {ex.Message}");
                LogHelper.LogException(ex);
                return null;
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
