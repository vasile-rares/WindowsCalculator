using System;
using System.Globalization;
using System.Windows.Data;

namespace WindowsCalculator.Converters
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string enumValue = value.ToString();
            string targetValue = parameter.ToString();

            return enumValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;

            if (value is bool boolValue && boolValue)
            {
                if (Enum.TryParse(targetType, parameter.ToString(), out object enumValue))
                    return enumValue;
            }

            return null;
        }
    }
} 