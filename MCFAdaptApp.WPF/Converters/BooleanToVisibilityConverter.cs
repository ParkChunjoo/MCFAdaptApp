using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MCFAdaptApp.WPF.Converters
{
    /// <summary>
    /// Converts a boolean value to a Visibility value
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to a Visibility value
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // If parameter is provided and is "Invert", invert the boolean value
                if (parameter is string param && param == "Invert")
                {
                    boolValue = !boolValue;
                }
                
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            
            return Visibility.Collapsed;
        }

        /// <summary>
        /// Converts a Visibility value back to a boolean value
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool result = visibility == Visibility.Visible;
                
                // If parameter is provided and is "Invert", invert the result
                if (parameter is string param && param == "Invert")
                {
                    result = !result;
                }
                
                return result;
            }
            
            return false;
        }
    }
} 