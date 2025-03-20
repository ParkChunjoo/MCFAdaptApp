using System;
using System.Windows;
using System.Windows.Controls;
using MCFAdaptApp.WPF.ViewModels;
using MCFAdaptApp.WPF.Commands;
using System.Windows.Threading;

namespace MCFAdaptApp.WPF.Views
{
    /// <summary>
    /// Interaction logic for SelectPatientView.xaml
    /// </summary>
    public partial class SelectPatientView : Window
    {
        private readonly SelectPatientViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the SelectPatientView
        /// </summary>
        public SelectPatientView()
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Initializing SelectPatientView");
            
            InitializeComponent();
            
            // Create ViewModel instance and set as DataContext
            _viewModel = new SelectPatientViewModel();
            DataContext = _viewModel;
            
            // Add exit command
            _viewModel.ExitCommand = new RelayCommand(_ => ExitApplication());
            
            // Load patient data
            _ = _viewModel.LoadPatientsAsync();
            
            // Register event handlers
            Loaded += SelectPatientView_Loaded;
            StateChanged += SelectPatientView_StateChanged;
            
            // 생성자 내부에 이벤트 핸들러 등록 추가
            _viewModel.NavigateToRegister += ViewModel_NavigateToRegister;
            
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] SelectPatientView initialized");
        }

        /// <summary>
        /// Handles the Loaded event
        /// </summary>
        private void SelectPatientView_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] SelectPatientView loaded");
            
            // 화면 크기에 맞게 창 크기 조정
            AdjustWindowToScreenSize();
            
            // 초기 창 상태에 따라 복원/최대화 버튼 텍스트 설정
            UpdateRestoreMaximizeButtonText();
        }
        
        /// <summary>
        /// 화면 크기에 맞게 창 크기 조정
        /// </summary>
        private void AdjustWindowToScreenSize()
        {
            // 현재 화면의 작업 영역 가져오기 (작업 표시줄 등을 제외한 영역)
            double screenWidth = SystemParameters.WorkArea.Width;
            double screenHeight = SystemParameters.WorkArea.Height;
            
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Screen size: {screenWidth}x{screenHeight}");
            
            // 창이 이미 최대화되어 있으므로 추가 조정은 필요 없음
            // 필요한 경우 여기에 추가 레이아웃 조정 코드 추가
            
            // 레이아웃이 완전히 로드된 후 DataGrid 크기 조정을 위해 Dispatcher 사용
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // 메인 그리드의 컬럼 정의 조정
                // 세 개의 동일한 크기의 컬럼으로 설정 (1:1:1 비율)
                var mainGrid = this.FindName("MainContent") as Grid;
                if (mainGrid != null && mainGrid.ColumnDefinitions.Count >= 3)
                {
                    mainGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                    mainGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                    mainGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
                    
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Adjusted main grid columns: 1:1:1 ratio");
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
        
        /// <summary>
        /// 현재 창 상태에 따라 복원/최대화 버튼 텍스트 업데이트
        /// </summary>
        private void UpdateRestoreMaximizeButtonText()
        {
            RestoreMaximizeButton.Content = (WindowState == WindowState.Maximized) ? "❐" : "□";
        }
        
        /// <summary>
        /// Exits the application
        /// </summary>
        private void ExitApplication()
        {
            System.Windows.Application.Current.Shutdown();
        }
        
        /// <summary>
        /// 최근 환자 탭 버튼 클릭 이벤트 핸들러
        /// </summary>
        private void RecentPatientsButton_Click(object sender, RoutedEventArgs e)
        {
            // 최근 환자 탭 활성화
            RecentPatientsBorder.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x3F, 0x3F, 0x46));
            AllPatientsBorder.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x2D, 0x2D, 0x30));
        }
        
        /// <summary>
        /// 모든 환자 탭 버튼 클릭 이벤트 핸들러
        /// </summary>
        private void AllPatientsButton_Click(object sender, RoutedEventArgs e)
        {
            // 모든 환자 탭 활성화
            RecentPatientsBorder.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x2D, 0x2D, 0x30));
            AllPatientsBorder.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x3F, 0x3F, 0x46));
        }
        
        /// <summary>
        /// 최소화 버튼 클릭 이벤트 핸들러
        /// </summary>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        
        /// <summary>
        /// 복원/최대화 버튼 클릭 이벤트 핸들러
        /// </summary>
        private void RestoreMaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                // 현재 화면의 작업 영역 가져오기
                double screenWidth = SystemParameters.WorkArea.Width;
                double screenHeight = SystemParameters.WorkArea.Height;
                
                // 창 크기를 화면의 절반 정도로 설정
                Width = screenWidth * 0.6;
                Height = screenHeight * 0.6;
                
                // 창을 화면 중앙에 위치시킴
                Left = (screenWidth - Width) / 2;
                Top = (screenHeight - Height) / 2;
                
                // 창 상태를 일반으로 변경
                WindowState = WindowState.Normal;
                
                // 버튼 텍스트 변경
                RestoreMaximizeButton.Content = "□";
            }
            else
            {
                // 창 상태를 최대화로 변경
                WindowState = WindowState.Maximized;
                
                // 버튼 텍스트 변경
                RestoreMaximizeButton.Content = "❐";
            }
        }

        /// <summary>
        /// 창 상태 변경 이벤트 핸들러
        /// </summary>
        private void SelectPatientView_StateChanged(object? sender, EventArgs e)
        {
            UpdateRestoreMaximizeButtonText();
        }

        /// <summary>
        /// DataGrid 행 클릭 이벤트 핸들러
        /// </summary>
        private void DataGridRow_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.DataContext is MCFAdaptApp.Domain.Models.Patient patient)
            {
                // 이미 선택된 환자인지 확인
                bool isSamePatient = _viewModel?.SelectedPatient?.PatientId == patient.PatientId;
                
                // 행을 선택 상태로 설정
                row.IsSelected = true;
                
                // 이벤트가 처리되었음을 표시하여 다른 이벤트 핸들러가 호출되지 않도록 함
                e.Handled = true;
                
                // 선택된 환자 정보 업데이트
                if (_viewModel != null)
                {
                    // 같은 환자를 다시 클릭한 경우, 현재 선택을 null로 설정했다가 다시 설정하여 리스트 초기화
                    if (isSamePatient)
                    {
                        _viewModel.SelectedPatient = null;
                        _viewModel.SelectedPatient = patient;
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Same patient clicked again, refreshing AnatomyModels");
                    }
                    else
                    {
                        _viewModel.SelectedPatient = patient;
                    }
                }
            }
        }
        
        /// <summary>
        /// DataGrid 선택 변경 이벤트 핸들러
        /// </summary>
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid && dataGrid.SelectedItem != null)
            {
                var patient = dataGrid.SelectedItem as MCFAdaptApp.Domain.Models.Patient;
                if (patient != null)
                {
                    // 선택된 항목이 뷰포트에 보이도록 스크롤
                    dataGrid.ScrollIntoView(patient);
                    
                    // Set the selected patient in the view model
                    if (_viewModel != null)
                    {
                        _viewModel.SelectedPatient = patient;
                    }
                    
                    // 환자 이니셜 설정
                    UpdatePatientInitials(patient);
                }
            }
        }
        
        /// <summary>
        /// 환자 이니셜을 업데이트합니다.
        /// </summary>
        private void UpdatePatientInitials(MCFAdaptApp.Domain.Models.Patient patient)
        {
            if (patient != null && PatientInitials != null)
            {
                // 이니셜 생성 (첫 글자만 사용)
                string firstInitial = !string.IsNullOrEmpty(patient.FirstName) ? patient.FirstName.Substring(0, 1) : "";
                string lastInitial = !string.IsNullOrEmpty(patient.LastName) ? patient.LastName.Substring(0, 1) : "";
                
                // 이니셜 설정
                PatientInitials.Text = $"{firstInitial}{lastInitial}";
                
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Updated patient initials: {PatientInitials.Text}");
            }
        }

        /// <summary>
        /// Register 탭으로 이동 이벤트 핸들러
        /// </summary>
        private void ViewModel_NavigateToRegister(object? sender, string patientId)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ViewModel_NavigateToRegister called with patientId: {patientId}");
            
            // 메인 윈도우의 탭 컨트롤 찾기
            var mainWindow = Application.Current.MainWindow;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] MainWindow found: {mainWindow != null}");
            
            var tabControl = mainWindow?.FindName("MainTabControl") as TabControl;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] MainTabControl found: {tabControl != null}");
            
            if (tabControl != null)
            {
                // Register 탭 찾기 (인덱스 1로 가정)
                if (tabControl.Items.Count > 1)
                {
                    // Register 탭으로 전환
                    tabControl.SelectedIndex = 1;
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] TabControl.SelectedIndex set to 1");
                    
                    // RegisterView의 ViewModel 초기화
                    var registerTab = tabControl.Items[1] as TabItem;
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] RegisterTab found: {registerTab != null}");
                    
                    if (registerTab != null)
                    {
                        var registerView = registerTab.Content as RegisterView;
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] RegisterView found: {registerView != null}");
                        
                        if (registerView != null)
                        {
                            var registerViewModel = registerView.DataContext as RegisterViewModel;
                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] RegisterViewModel found: {registerViewModel != null}");
                            
                            if (registerViewModel != null)
                            {
                                // 환자 ID 설정 및 DICOM 파일 로드
                                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Calling InitializeAsync with patientId: {patientId}");
                                _ = registerViewModel.InitializeAsync(patientId);
                            }
                        }
                    }
                    
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Switched to Register tab");
                }
                else
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Register tab not found, TabControl.Items.Count: {tabControl.Items.Count}");
                }
            }
            else
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] MainTabControl not found");
            }
        }
    }
} 