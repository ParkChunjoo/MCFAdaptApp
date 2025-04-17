using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using MCFAdaptApp.Avalonia.Helpers;
using MCFAdaptApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MCFAdaptApp.Avalonia.Controls
{
    /// <summary>
    /// A medical image view control with windowing controls
    /// </summary>
    public class MedicalImageView : UserControl
    {
        // Define dependency properties
        public static readonly StyledProperty<RTCT?> VolumeProperty =
            AvaloniaProperty.Register<MedicalImageView, RTCT?>(nameof(Volume));

        public static readonly StyledProperty<int> SliceIndexProperty =
            AvaloniaProperty.Register<MedicalImageView, int>(nameof(SliceIndex), -1);

        public static readonly StyledProperty<double> WindowWidthProperty =
            AvaloniaProperty.Register<MedicalImageView, double>(nameof(WindowWidth), 2000);

        public static readonly StyledProperty<double> WindowCenterProperty =
            AvaloniaProperty.Register<MedicalImageView, double>(nameof(WindowCenter), 0);

        public static readonly StyledProperty<double> ZoomFactorProperty =
            AvaloniaProperty.Register<MedicalImageView, double>(nameof(ZoomFactor), 1.0);

        public static readonly StyledProperty<Point> PanOffsetProperty =
            AvaloniaProperty.Register<MedicalImageView, Point>(nameof(PanOffset), new Point(0, 0));

        public static readonly StyledProperty<bool> ShowGridProperty =
            AvaloniaProperty.Register<MedicalImageView, bool>(nameof(ShowGrid), true);

        // Properties
        public RTCT? Volume
        {
            get => GetValue(VolumeProperty);
            set => SetValue(VolumeProperty, value);
        }

        public int SliceIndex
        {
            get => GetValue(SliceIndexProperty);
            set => SetValue(SliceIndexProperty, value);
        }

        public double WindowWidth
        {
            get => GetValue(WindowWidthProperty);
            set => SetValue(WindowWidthProperty, value);
        }

        public double WindowCenter
        {
            get => GetValue(WindowCenterProperty);
            set => SetValue(WindowCenterProperty, value);
        }

        public double ZoomFactor
        {
            get => GetValue(ZoomFactorProperty);
            set => SetValue(ZoomFactorProperty, value);
        }

        public Point PanOffset
        {
            get => GetValue(PanOffsetProperty);
            set => SetValue(PanOffsetProperty, value);
        }

        public bool ShowGrid
        {
            get => GetValue(ShowGridProperty);
            set => SetValue(ShowGridProperty, value);
        }

        // Private fields
        private WriteableBitmap? _bitmap;
        private bool _needsUpdate = true;
        private bool _isUpdating = false;
        private bool _isDragging = false;
        private Point _lastMousePosition;
        private string _imageInfo = string.Empty;
        private Image _image;
        private TextBlock _infoTextBlock;
        private Canvas _gridCanvas;
        private Grid _mainGrid;
        private ProgressBar _loadingIndicator;
        private CancellationTokenSource? _currentUpdateCts;
        private bool _measurementMode = false;
        private Point? _measurementStart;
        private Point? _measurementEnd;
        private Line _measurementLine;
        private TextBlock _measurementText;

        // Event for view synchronization
        public event EventHandler<NavigationEventArgs>? ViewNavigated;

        // Constructor
        public MedicalImageView()
        {
            // Create child controls
            _image = new Image
            {
                Stretch = Stretch.Uniform
            };

            _gridCanvas = new Canvas();

            _infoTextBlock = new TextBlock
            {
                Foreground = Brushes.White,
                Margin = new Thickness(10, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                ZIndex = 10
            };

            _loadingIndicator = new ProgressBar
            {
                IsIndeterminate = true,
                Width = 100,
                Height = 10,
                IsVisible = false,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                ZIndex = 5
            };

            // Initialize measurement tools
            _measurementLine = new Line
            {
                Stroke = new SolidColorBrush(Colors.Yellow),
                StrokeThickness = 2,
                IsVisible = false,
                ZIndex = 20
            };

            _measurementText = new TextBlock
            {
                Foreground = new SolidColorBrush(Colors.Yellow),
                Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                Padding = new Thickness(3),
                IsVisible = false,
                ZIndex = 20
            };

            // Create layout
            _mainGrid = new Grid();
            _mainGrid.Children.Add(_image);
            _mainGrid.Children.Add(_gridCanvas);
            _mainGrid.Children.Add(_infoTextBlock);
            _mainGrid.Children.Add(_loadingIndicator);
            _mainGrid.Children.Add(_measurementLine);
            _mainGrid.Children.Add(_measurementText);

            // Set the content
            this.Content = _mainGrid;

            // Subscribe to property changes
            this.GetObservable(VolumeProperty).Subscribe(_ => InvalidateImage());
            this.GetObservable(SliceIndexProperty).Subscribe(_ => InvalidateImage());
            this.GetObservable(WindowWidthProperty).Subscribe(_ => InvalidateImage());
            this.GetObservable(WindowCenterProperty).Subscribe(_ => InvalidateImage());
            this.GetObservable(ZoomFactorProperty).Subscribe(_ => UpdateImageLayout());
            this.GetObservable(PanOffsetProperty).Subscribe(_ => UpdateImageLayout());
            this.GetObservable(ShowGridProperty).Subscribe(_ => UpdateGrid());

            // Set up mouse handling for pan and zoom
            this.PointerPressed += OnPointerPressed;
            this.PointerReleased += OnPointerReleased;
            this.PointerMoved += OnPointerMoved;
            this.PointerWheelChanged += OnPointerWheelChanged;
        }

        // Mouse event handlers
        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var point = e.GetPosition(this);

            if (_measurementMode)
            {
                if (_measurementStart == null)
                {
                    _measurementStart = point;
                    _measurementEnd = null;
                    _measurementLine.IsVisible = false;
                    _measurementText.IsVisible = false;
                }
                else
                {
                    _measurementEnd = point;
                    CalculateAndDisplayMeasurement();
                }
                e.Handled = true;
            }
            else
            {
                _lastMousePosition = point;
                _isDragging = true;
                e.Pointer.Capture(this);
            }
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _isDragging = false;
            e.Pointer.Capture(null);
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_isDragging)
            {
                var point = e.GetPosition(this);
                var delta = point - _lastMousePosition;

                // Update pan offset
                PanOffset = new Point(
                    PanOffset.X + delta.X,
                    PanOffset.Y + delta.Y);

                _lastMousePosition = point;

                // Notify subscribers for view synchronization
                ViewNavigated?.Invoke(this, new NavigationEventArgs
                {
                    NavigationType = NavigationType.Pan,
                    PanDelta = delta,
                    ZoomDelta = 0
                });
            }
            else if (_measurementMode && _measurementStart != null && _measurementEnd == null)
            {
                // Show preview of measurement line
                var point = e.GetPosition(this);
                _measurementLine.StartPoint = _measurementStart.Value;
                _measurementLine.EndPoint = point;
                _measurementLine.IsVisible = true;
            }
        }

        private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            // Adjust zoom factor based on wheel delta
            var delta = e.Delta.Y * 0.1;
            var newZoom = Math.Max(0.1, Math.Min(10.0, ZoomFactor + delta));

            // Get the position of the mouse relative to the control
            var mousePos = e.GetPosition(this);

            // Calculate the position relative to the center of the control
            var centerX = Bounds.Width / 2;
            var centerY = Bounds.Height / 2;

            // Calculate the new pan offset to keep the point under the mouse in the same position
            var oldRelativeX = (mousePos.X - centerX - PanOffset.X) / ZoomFactor;
            var oldRelativeY = (mousePos.Y - centerY - PanOffset.Y) / ZoomFactor;
            var newPanX = mousePos.X - centerX - oldRelativeX * newZoom;
            var newPanY = mousePos.Y - centerY - oldRelativeY * newZoom;

            // Update properties
            ZoomFactor = newZoom;
            PanOffset = new Point(newPanX, newPanY);

            // Notify subscribers for view synchronization
            ViewNavigated?.Invoke(this, new NavigationEventArgs
            {
                NavigationType = NavigationType.Zoom,
                PanDelta = new Point(0, 0),
                ZoomDelta = delta
            });
        }

        // Invalidate the image when properties change
        private void InvalidateImage()
        {
            _needsUpdate = true;
            UpdateImage();
        }

        // Update the layout based on zoom and pan
        private void UpdateImageLayout()
        {
            if (_image != null)
            {
                // Apply zoom and pan transformations
                var transformGroup = new TransformGroup();
                transformGroup.Children.Add(new ScaleTransform(ZoomFactor, ZoomFactor));
                transformGroup.Children.Add(new TranslateTransform(PanOffset.X, PanOffset.Y));
                _image.RenderTransform = transformGroup;

                // Update grid
                UpdateGrid();

                // Update info text
                UpdateInfoText();
            }
        }

        // Update the grid overlay
        private void UpdateGrid()
        {
            _gridCanvas.Children.Clear();

            if (ShowGrid && _bitmap != null)
            {
                var width = _bitmap.PixelSize.Width * ZoomFactor;
                var height = _bitmap.PixelSize.Height * ZoomFactor;
                var centerX = Bounds.Width / 2 + PanOffset.X;
                var centerY = Bounds.Height / 2 + PanOffset.Y;

                // Horizontal line
                var hLine = new Line
                {
                    StartPoint = new Point(centerX - width / 2, centerY),
                    EndPoint = new Point(centerX + width / 2, centerY),
                    Stroke = new SolidColorBrush(Color.FromArgb(100, 0, 255, 0)),
                    StrokeThickness = 1
                };

                // Vertical line
                var vLine = new Line
                {
                    StartPoint = new Point(centerX, centerY - height / 2),
                    EndPoint = new Point(centerX, centerY + height / 2),
                    Stroke = new SolidColorBrush(Color.FromArgb(100, 0, 255, 0)),
                    StrokeThickness = 1
                };

                _gridCanvas.Children.Add(hLine);
                _gridCanvas.Children.Add(vLine);
            }
        }

        // Update the info text
        private void UpdateInfoText()
        {
            if (Volume != null)
            {
                int sliceIndex = SliceIndex;
                if (sliceIndex < 0 || sliceIndex >= Volume.Depth)
                {
                    sliceIndex = Volume.DisplaySliceIndex;
                    if (sliceIndex < 0 || sliceIndex >= Volume.Depth)
                    {
                        sliceIndex = Volume.Depth / 2;
                    }
                }

                _imageInfo = $"Slice: {sliceIndex + 1}/{Volume.Depth}, " +
                             $"Window: C={WindowCenter:F0}/W={WindowWidth:F0}, " +
                             $"Zoom: {ZoomFactor:F1}x";

                _infoTextBlock.Text = _imageInfo;
            }
            else
            {
                _infoTextBlock.Text = string.Empty;
            }
        }

        // Update the bitmap from the volume data
        private async void UpdateImage()
        {
            if (!_needsUpdate)
                return;

            if (_isUpdating)
                return;

            _isUpdating = true;
            _loadingIndicator.IsVisible = true;

            if (Volume?.PixelData == null || Volume.Width <= 0 || Volume.Height <= 0 || Volume.Depth <= 0)
            {
                _bitmap = null;
                _image.Source = null;
                _loadingIndicator.IsVisible = false;
                _isUpdating = false;
                return;
            }

            try
            {
                // Cancel any previous update operation
                _currentUpdateCts?.Cancel();
                var cts = new CancellationTokenSource();
                _currentUpdateCts = cts;

                // Determine which slice to display
                int sliceIndex = SliceIndex;
                if (sliceIndex < 0 || sliceIndex >= Volume.Depth)
                {
                    sliceIndex = Volume.DisplaySliceIndex;
                    if (sliceIndex < 0 || sliceIndex >= Volume.Depth)
                    {
                        sliceIndex = Volume.Depth / 2;
                    }
                }

                int width = Volume.Width;
                int height = Volume.Height;
                int sliceSize = width * height;
                int sliceOffset = sliceIndex * sliceSize;

                // Create or reuse WriteableBitmap
                bool createNewBitmap = _bitmap == null ||
                                      _bitmap.PixelSize.Width != width ||
                                      _bitmap.PixelSize.Height != height;

                if (createNewBitmap)
                {
                    _bitmap?.Dispose(); // Dispose old bitmap to free memory
                    _bitmap = new WriteableBitmap(
                        new PixelSize(width, height),
                        new Vector(96, 96),
                        PixelFormat.Bgra8888,
                        AlphaFormat.Premul);
                }

                // Extract slice data
                short[] slicePixels = new short[sliceSize];
                Array.Copy(Volume.PixelData, sliceOffset, slicePixels, 0, sliceSize);

                // Apply windowing
                double windowMin = WindowCenter - WindowWidth / 2;
                double windowMax = WindowCenter + WindowWidth / 2;
                double windowWidth = WindowWidth;

                // Update image info
                UpdateInfoText();

                // Process the image on a background thread
                await Task.Run(() =>
                {
                    // Check if operation was cancelled
                    if (cts.IsCancellationRequested)
                        return;

                    using (var buffer = _bitmap.Lock())
                    {
                        unsafe
                        {
                            uint* pixels = (uint*)buffer.Address;

                            // Process all pixels in the slice with SIMD-like optimization
                            // Process in chunks for better cache utilization
                            const int chunkSize = 1024;
                            for (int chunk = 0; chunk < sliceSize; chunk += chunkSize)
                            {
                                // Check for cancellation periodically
                                if (chunk % (chunkSize * 8) == 0 && cts.IsCancellationRequested)
                                    return;

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
                }, cts.Token);

                // Check if operation was cancelled
                if (cts.IsCancellationRequested)
                {
                    _loadingIndicator.IsVisible = false;
                    _isUpdating = false;
                    return;
                }

                // Set the image source on the UI thread
                Dispatcher.UIThread.Post(() =>
                {
                    _image.Source = _bitmap;
                    // Update layout and grid
                    UpdateImageLayout();
                    _loadingIndicator.IsVisible = false;
                    _needsUpdate = false;
                });
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error creating medical image bitmap: {ex.Message}");
                _bitmap = null;
                _image.Source = null;
            }
            finally
            {
                _loadingIndicator.IsVisible = false;
                _isUpdating = false;
            }
        }

        // Measurement mode property and methods
        public bool MeasurementMode
        {
            get => _measurementMode;
            set
            {
                _measurementMode = value;
                _measurementStart = null;
                _measurementEnd = null;
                _measurementLine.IsVisible = false;
                _measurementText.IsVisible = false;
            }
        }

        private void CalculateAndDisplayMeasurement()
        {
            if (_measurementStart == null || _measurementEnd == null || Volume == null)
                return;

            // Update line position
            _measurementLine.StartPoint = _measurementStart.Value;
            _measurementLine.EndPoint = _measurementEnd.Value;
            _measurementLine.IsVisible = true;

            // Calculate distance in pixels
            double pixelDistance = Math.Sqrt(
                Math.Pow(_measurementEnd.Value.X - _measurementStart.Value.X, 2) +
                Math.Pow(_measurementEnd.Value.Y - _measurementStart.Value.Y, 2));

            // Convert to physical distance (mm) using pixel spacing
            double physicalDistance = pixelDistance * Volume.PixelSpacingX / ZoomFactor;

            // Update measurement text
            _measurementText.Text = $"{physicalDistance:F2} mm";

            // Position text near the middle of the line
            double midX = (_measurementStart.Value.X + _measurementEnd.Value.X) / 2;
            double midY = (_measurementStart.Value.Y + _measurementEnd.Value.Y) / 2;
            Canvas.SetLeft(_measurementText, midX + 5);
            Canvas.SetTop(_measurementText, midY - 15);
            _measurementText.IsVisible = true;
        }

        // Memory management - dispose resources when control is detached
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            // Cancel any pending operations
            _currentUpdateCts?.Cancel();
            _currentUpdateCts = null;

            // Dispose bitmap resources
            _bitmap?.Dispose();
            _bitmap = null;

            // Unsubscribe from events to prevent memory leaks
            this.PointerPressed -= OnPointerPressed;
            this.PointerReleased -= OnPointerReleased;
            this.PointerMoved -= OnPointerMoved;
            this.PointerWheelChanged -= OnPointerWheelChanged;
        }

        // Override OnAttachedToVisualTree to update the image when the control is attached
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            InvalidateImage();
        }
    }
}
