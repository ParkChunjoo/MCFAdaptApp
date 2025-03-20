using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MCFAdaptApp.WPF.Converters
{
    /// <summary>
    /// Converts a null value to a Visibility value
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a null value to a Visibility value
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNull = value == null;
            bool invert = false;
            
            // 파라미터가 제공되면 결과를 반전시킵니다.
            if (parameter != null)
            {
                if (parameter is string param && (param.ToLower() == "true" || param.ToLower() == "invert"))
                {
                    invert = true;
                }
                else if (parameter is bool boolParam)
                {
                    invert = boolParam;
                }
            }
            
            // 반전 여부에 따라 결과를 결정합니다.
            if (invert)
            {
                return isNull ? Visibility.Visible : Visibility.Collapsed;
            }
            
            return isNull ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Converts a Visibility value back to a null value (not implemented)
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 