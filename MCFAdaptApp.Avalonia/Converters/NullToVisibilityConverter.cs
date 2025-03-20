using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia;

namespace MCFAdaptApp.Avalonia.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isNull = value == null;
            bool invert = false;
            
            // Check if parameter indicates inversion
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
            
            // Return visibility based on null check and inversion
            if (invert)
            {
                return isNull;
            }
            
            return !isNull;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
