using System;
using System.Globalization;
using System.Windows.Data;
using WindowsCalculator.ViewModels;

namespace WindowsCalculator.Converters
{
    public class ButtonEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            var currentBase = (CalculatorViewModel.NumberBase)value;
            var buttonValue = parameter.ToString();

            return currentBase switch
            {
                CalculatorViewModel.NumberBase.HEX => true,
                CalculatorViewModel.NumberBase.DEC => !"ABCDEF".Contains(buttonValue),
                CalculatorViewModel.NumberBase.OCT => !"89ABCDEF".Contains(buttonValue),
                CalculatorViewModel.NumberBase.BIN => "01".Contains(buttonValue),
                _ => false
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}