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
        private RTCT? _cbct;
        private RTCT? _referenceCT;
        private RTStructure? _rtStructure;
        private RTPlan? _rtPlan;
        private RTDose? _rtDose;
        private string _patientId = string.Empty;
        private Patient? _patient;
        private bool _isLoading;
        private string? _statusMessage;
        private bool _hasError;
        private string _errorMessage = string.Empty;
        private Bitmap? _cbctCenterSliceBitmap;
        private Bitmap? _refCtCenterSliceBitmap;

        // Synchronization and caching properties
        private bool _syncViews = false;
        private Dictionary<string, Bitmap> _imageCache = new Dictionary<string, Bitmap>();
        private const int MAX_CACHED_IMAGES = 10;

        // Properties for GPU-accelerated image view
        private double _cbctWindowWidth = 2000;
        private double _cbctWindowCenter = 0;
        private double _refCtWindowWidth = 2000;
        private double _refCtWindowCenter = 0;
        private double _cbctZoomFactor = 1.0;
        private double _refCtZoomFactor = 1.0;
        private Point _cbctPanOffset = new Point(0, 0);
        private Point _refCtPanOffset = new Point(0, 0);
        private bool _showGrid = true;

        /// <summary>
        /// CBCT image data
        /// </summary>
        public RTCT CBCT
        {
            get => _cbct;
            set
            {
                _cbct = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Reference CT image data
        /// </summary>
        public RTCT ReferenceCT
        {
            get => _referenceCT;
            set
            {
                _referenceCT = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Currently selected RT structure
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
        /// Currently selected RT plan
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
        /// Currently selected RT dose
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
        /// Whether to synchronize views between Reference CT and CBCT
        /// </summary>
        public bool SyncViews
        {
            get => _syncViews;
            set
            {
                _syncViews = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Window width for CBCT image
        /// </summary>
        public double CbctWindowWidth
        {
            get => _cbctWindowWidth;
            set
            {
                _cbctWindowWidth = value;
                OnPropertyChanged();

                // Synchronize with Reference CT if enabled
                if (SyncViews)
                {
                    RefCtWindowWidth = value;
                }
            }
        }

        /// <summary>
        /// Window center for CBCT image
        /// </summary>
        public double CbctWindowCenter
        {
            get => _cbctWindowCenter;
            set
            {
                _cbctWindowCenter = value;
                OnPropertyChanged();

                // Synchronize with Reference CT if enabled
                if (SyncViews)
                {
                    RefCtWindowCenter = value;
                }
            }
        }

        /// <summary>
        /// Window width for Reference CT image
        /// </summary>
        public double RefCtWindowWidth
        {
            get => _refCtWindowWidth;
            set
            {
                _refCtWindowWidth = value;
                OnPropertyChanged();

                // Synchronize with CBCT if enabled
                if (SyncViews)
                {
                    _cbctWindowWidth = value; // Use field to avoid infinite recursion
                    OnPropertyChanged(nameof(CbctWindowWidth));
                }
            }
        }

        /// <summary>
        /// Window center for Reference CT image
        /// </summary>
        public double RefCtWindowCenter
        {
            get => _refCtWindowCenter;
            set
            {
                _refCtWindowCenter = value;
                OnPropertyChanged();

                // Synchronize with CBCT if enabled
                if (SyncViews)
                {
                    _cbctWindowCenter = value; // Use field to avoid infinite recursion
                    OnPropertyChanged(nameof(CbctWindowCenter));
                }
            }
        }

        /// <summary>
        /// Zoom factor for CBCT image
        /// </summary>
        public double CbctZoomFactor
        {
            get => _cbctZoomFactor;
            set
            {
                _cbctZoomFactor = value;
                OnPropertyChanged();

                // Synchronize with Reference CT if enabled
                if (SyncViews)
                {
                    RefCtZoomFactor = value;
                }
            }
        }

        /// <summary>
        /// Zoom factor for Reference CT image
        /// </summary>
        public double RefCtZoomFactor
        {
            get => _refCtZoomFactor;
            set
            {
                _refCtZoomFactor = value;
                OnPropertyChanged();

                // Synchronize with CBCT if enabled
                if (SyncViews)
                {
                    _cbctZoomFactor = value; // Use field to avoid infinite recursion
                    OnPropertyChanged(nameof(CbctZoomFactor));
                }
            }
        }

        /// <summary>
        /// Pan offset for CBCT image
        /// </summary>
        public Point CbctPanOffset
        {
            get => _cbctPanOffset;
            set
            {
                _cbctPanOffset = value;
                OnPropertyChanged();

                // Synchronize with Reference CT if enabled
                if (SyncViews)
                {
                    RefCtPanOffset = value;
                }
            }
        }

        /// <summary>
        /// Pan offset for Reference CT image
        /// </summary>
        public Point RefCtPanOffset
        {
            get => _refCtPanOffset;
            set
            {
                _refCtPanOffset = value;
                OnPropertyChanged();

                // Synchronize with CBCT if enabled
                if (SyncViews)
                {
                    _cbctPanOffset = value; // Use field to avoid infinite recursion
                    OnPropertyChanged(nameof(CbctPanOffset));
                }
            }
        }

        /// <summary>
        /// Whether to show grid overlay on images
        /// </summary>
        public bool ShowGrid
        {
            get => _showGrid;
            set
            {
                _showGrid = value;
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
        /// Command to apply window presets (Bone, Lung, Soft Tissue, Brain)
        /// </summary>
        public RelayCommand<string> ApplyWindowPresetCommand { get; private set; }

        /// <summary>
        /// Command to synchronize all view parameters between Reference CT and CBCT
        /// </summary>
        public RelayCommand<string> SynchronizeAllViewParametersCommand { get; private set; }

        /// <summary>
        /// Command to toggle measurement mode
        /// </summary>
        public RelayCommand ToggleMeasurementModeCommand { get; set; }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="dicomService">DICOM 서비스</param>
        public RegisterViewModel(IDicomService dicomService)
        {
            _dicomService = dicomService ?? throw new ArgumentNullException(nameof(dicomService));

            // 명령 초기화
            LoadDicomFilesCommand = new AsyncRelayCommand(LoadDicomFilesAsync, () => !IsLoading);
            ApplyWindowPresetCommand = new RelayCommand<string>(ApplyWindowPreset);
            SynchronizeAllViewParametersCommand = new RelayCommand<string>(param =>
                SynchronizeAllViewParameters(param == "true" || string.IsNullOrEmpty(param)));
            ToggleMeasurementModeCommand = new RelayCommand(obj => ToggleMeasurementMode());
        }

        /// <summary>
        /// Apply a window preset (Bone, Lung, Soft Tissue, Brain)
        /// </summary>
        private void ApplyWindowPreset(string preset)
        {
            switch (preset)
            {
                case "Bone":
                    RefCtWindowWidth = 2000;
                    RefCtWindowCenter = 500;
                    break;
                case "Lung":
                    RefCtWindowWidth = 1500;
                    RefCtWindowCenter = -600;
                    break;
                case "SoftTissue":
                    RefCtWindowWidth = 400;
                    RefCtWindowCenter = 40;
                    break;
                case "Brain":
                    RefCtWindowWidth = 80;
                    RefCtWindowCenter = 40;
                    break;
            }
        }

        /// <summary>
        /// Synchronize all view parameters between Reference CT and CBCT
        /// </summary>
        /// <param name="fromRefToCbct">If true, copy Ref CT parameters to CBCT; otherwise, copy CBCT parameters to Ref CT</param>
        public void SynchronizeAllViewParameters(bool fromRefToCbct = true)
        {
            if (fromRefToCbct)
            {
                // Copy Ref CT parameters to CBCT
                _cbctWindowWidth = RefCtWindowWidth;
                _cbctWindowCenter = RefCtWindowCenter;
                _cbctZoomFactor = RefCtZoomFactor;
                _cbctPanOffset = RefCtPanOffset;

                // Notify property changes
                OnPropertyChanged(nameof(CbctWindowWidth));
                OnPropertyChanged(nameof(CbctWindowCenter));
                OnPropertyChanged(nameof(CbctZoomFactor));
                OnPropertyChanged(nameof(CbctPanOffset));
            }
            else
            {
                // Copy CBCT parameters to Ref CT
                _refCtWindowWidth = CbctWindowWidth;
                _refCtWindowCenter = CbctWindowCenter;
                _refCtZoomFactor = CbctZoomFactor;
                _refCtPanOffset = CbctPanOffset;

                // Notify property changes
                OnPropertyChanged(nameof(RefCtWindowWidth));
                OnPropertyChanged(nameof(RefCtWindowCenter));
                OnPropertyChanged(nameof(RefCtZoomFactor));
                OnPropertyChanged(nameof(RefCtPanOffset));
            }
        }

        /// <summary>
        /// Toggle measurement mode on/off
        /// </summary>
        private void ToggleMeasurementMode()
        {
            // This will be handled in the view by binding to a property in the MedicalImageView control
            // We'll implement this in the view code-behind
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
                RTStructure = null;
                RTPlan = null;
                RTDose = null;
                CBCT = null;
                ReferenceCT = null;

                // 1. Load RT Plan first to get isocenter
                StatusMessage = "RT Plan 로드 중...";
                RTPlan = await _dicomService.LoadRTPlanAsync(PatientId);
                double? isocenterZ = RTPlan?.IsocenterPosition?.Length == 3 ? RTPlan.IsocenterPosition[2] : null;

                // Log detailed isocenter information
                if (RTPlan?.IsocenterPosition != null && RTPlan.IsocenterPosition.Length == 3)
                {
                    LogHelper.Log($"Using RTPlan isocenter position: X={RTPlan.IsocenterPosition[0]}, Y={RTPlan.IsocenterPosition[1]}, Z={RTPlan.IsocenterPosition[2]}");
                }
                else
                {
                    LogHelper.Log("RTPlan isocenter position not available - IsocenterPosition tag may be missing from RT Plan file");
                }

                // 2. Load CBCT
                StatusMessage = "CBCT 프로젝션 로드 중...";
                CBCT = await _dicomService.LoadCBCTAsync(PatientId);
                if (CBCT?.PixelData != null)
                {
                    StatusMessage = "CBCT 슬라이스 생성 중...";
                    CbctCenterSliceBitmap = await Task.Run(() => CreateBitmapFromSlice(CBCT));

                    // Set appropriate windowing values for CBCT
                    // CBCT typically has lower contrast than diagnostic CT
                    CbctWindowWidth = 1500;
                    CbctWindowCenter = 400;
                    CbctZoomFactor = 1.0;
                    CbctPanOffset = new Point(0, 0);
                }

                // 3. Load Reference CT (pass isocenter Z)
                StatusMessage = "참조 CT 로드 중...";
                ReferenceCT = await _dicomService.LoadReferenceCTAsync(PatientId, isocenterZ);
                if (ReferenceCT?.PixelData != null)
                {
                    StatusMessage = "참조 CT 슬라이스 생성 중...";
                    RefCtCenterSliceBitmap = await Task.Run(() => CreateBitmapFromSlice(ReferenceCT));

                    // Set appropriate windowing values for Reference CT
                    // Standard CT windowing for soft tissue
                    RefCtWindowWidth = 400;
                    RefCtWindowCenter = 40;
                    RefCtZoomFactor = 1.0;
                    RefCtPanOffset = new Point(0, 0);
                }

                // 4. Load other RT data
                StatusMessage = "RT Structure 로드 중...";
                RTStructure = await _dicomService.LoadRTStructureAsync(PatientId);

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

        private Bitmap? CreateBitmapFromSlice(RTCT ctVolume)
        {
            if (ctVolume?.PixelData == null || ctVolume.Width <= 0 || ctVolume.Height <= 0 || ctVolume.Depth <= 0)
            {
                return null;
            }

            try
            {
                int width = ctVolume.Width;
                int height = ctVolume.Height;
                // Use the pre-calculated display slice index
                int displaySliceIndex = ctVolume.DisplaySliceIndex;
                if (displaySliceIndex < 0 || displaySliceIndex >= ctVolume.Depth)
                {
                    LogHelper.LogWarning($"Invalid DisplaySliceIndex ({displaySliceIndex}) for volume depth {ctVolume.Depth}. Defaulting to center.");
                    displaySliceIndex = ctVolume.Depth / 2; // Fallback to center if index is invalid
                }

                // Check cache first
                string cacheKey = $"{ctVolume.Id}_{displaySliceIndex}";
                if (_imageCache.TryGetValue(cacheKey, out var cachedBitmap))
                {
                    LogHelper.Log($"Using cached bitmap for {ctVolume.Name}, slice {displaySliceIndex}");
                    return cachedBitmap;
                }

                // If cache is full, remove oldest entry
                if (_imageCache.Count >= MAX_CACHED_IMAGES)
                {
                    var oldestKey = _imageCache.Keys.First();
                    _imageCache[oldestKey].Dispose();
                    _imageCache.Remove(oldestKey);
                }

                int sliceSize = width * height;
                int startOffset = displaySliceIndex * sliceSize;

                // Extract slice data using the determined index
                short[] slicePixels = new short[sliceSize];
                Buffer.BlockCopy(ctVolume.PixelData, startOffset * sizeof(short), slicePixels, 0, sliceSize * sizeof(short));

                // Find min/max pixel values for windowing (simple approach)
                short minPixel = short.MaxValue;
                short maxPixel = short.MinValue;

                // Use sampling for large slices to improve performance
                int samplingRate = sliceSize > 1000000 ? 10 : 1;
                for (int i = 0; i < slicePixels.Length; i += samplingRate)
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

                        // Process in chunks for better cache utilization
                        const int chunkSize = 1024;
                        for (int chunk = 0; chunk < sliceSize; chunk += chunkSize)
                        {
                            int end = Math.Min(chunk + chunkSize, sliceSize);
                            for (int i = chunk; i < end; i++)
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
                }

                // Add to cache
                _imageCache[cacheKey] = bitmap;
                LogHelper.Log($"Added bitmap to cache for {ctVolume.Name}, slice {displaySliceIndex}");

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
        /// Initialize with patient information, selected structure and plan
        /// </summary>
        /// <param name="patient">Patient information</param>
        /// <param name="structure">Selected RT structure</param>
        /// <param name="plan">Selected RT plan</param>
        public async Task InitializeAsync(Patient patient, RTStructure structure = null, RTPlan plan = null)
        {
            // Log the full details of what we received
            LogHelper.Log("RegisterViewModel.InitializeAsync: Received data:");
            LogHelper.Log($"Patient: {patient?.PatientId ?? "null"} - {patient?.FirstName ?? ""} {patient?.LastName ?? ""}");
            LogHelper.Log($"RTStructure: {structure?.Name ?? "null"}");
            LogHelper.Log($"RTPlan: {plan?.Name ?? "null"}");

            Patient = patient;
            RTStructure = structure;
            RTPlan = plan;

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
