using System;
using System.Globalization;
using System.Windows.Data;

namespace WindowsCalculator.Converters
{
    public class ContainsTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string stringValue = value.ToString();
            string textToCheck = parameter.ToString();

            return stringValue.Contains(textToCheck, StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}