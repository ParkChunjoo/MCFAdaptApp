using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia;

namespace MCFAdaptApp.Avalonia.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // If parameter is provided and is "Invert", invert the boolean value
                if (parameter is string param && param == "Invert")
                {
                    boolValue = !boolValue;
                }

                return boolValue;
            }

            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool visibility)
            {
                bool result = visibility;

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
