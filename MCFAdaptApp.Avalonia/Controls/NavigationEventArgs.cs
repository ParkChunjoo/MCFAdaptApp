using Avalonia;
using System;

namespace MCFAdaptApp.Avalonia.Controls
{
    /// <summary>
    /// Navigation type for medical image view
    /// </summary>
    public enum NavigationType
    {
        Pan,
        Zoom,
        WindowLevel
    }

    /// <summary>
    /// Event arguments for navigation events in medical image view
    /// </summary>
    public class NavigationEventArgs : EventArgs
    {
        /// <summary>
        /// Type of navigation that occurred
        /// </summary>
        public NavigationType NavigationType { get; set; }
        
        /// <summary>
        /// Delta for pan operations
        /// </summary>
        public Point PanDelta { get; set; }
        
        /// <summary>
        /// Delta for zoom operations
        /// </summary>
        public double ZoomDelta { get; set; }
        
        /// <summary>
        /// Window width delta for windowing operations
        /// </summary>
        public double WindowWidthDelta { get; set; }
        
        /// <summary>
        /// Window center delta for windowing operations
        /// </summary>
        public double WindowCenterDelta { get; set; }
    }
}
