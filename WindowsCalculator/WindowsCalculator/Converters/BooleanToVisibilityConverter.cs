using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WindowsCalculator.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // Verificam daca parametrul specifica inversarea
                bool shouldInvert = parameter != null && bool.Parse(parameter.ToString());

                // Aplicam inversarea daca este necesar
                boolValue = shouldInvert ? !boolValue : boolValue;

                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool result = visibility == Visibility.Visible;

                bool shouldInvert = parameter != null && bool.Parse(parameter.ToString());

                return shouldInvert ? !result : result;
            }
            return false;
        }
    }
}