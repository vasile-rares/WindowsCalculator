using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WindowsCalculator.ViewModels;
using System.Globalization;

namespace WindowsCalculator
{
    /// <summary>
    /// Interaction logic for StandardCalculatorView.xaml
    /// </summary>
    public partial class StandardCalculatorView : UserControl
    {
        private CalculatorViewModel? ViewModel => DataContext as CalculatorViewModel;

        public StandardCalculatorView()
        {
            InitializeComponent();

            // Asiguram focusul pe control
            this.Loaded += (s, e) => Keyboard.Focus(this);
            this.GotFocus += (s, e) => e.Handled = true;
        }

        private void MemoryListButton_Click(object sender, RoutedEventArgs e)
        {
            if (MemoryPopup != null)
            {
                MemoryPopup.IsOpen = !MemoryPopup.IsOpen;
            }
        }

        private void MemoryItemClear_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && ViewModel != null)
            {
                string memoryValue = button.Tag as string;
                int index = ViewModel.MemoryValues.IndexOf(memoryValue);

                if (index >= 0)
                {
                    ViewModel.MemoryValues.RemoveAt(index);
                    ViewModel.OnPropertyChanged(nameof(ViewModel.IsMemoryEmpty));

                    if (ViewModel.IsMemoryEmpty)
                    {
                        MemoryPopup.IsOpen = false;
                    }
                }
            }
        }

        private void MemoryItemAdd_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && ViewModel != null)
            {
                string memoryValue = button.Tag as string;
                int index = ViewModel.MemoryValues.IndexOf(memoryValue);

                if (index >= 0 && double.TryParse(memoryValue, out double storedValue) &&
                    double.TryParse(ViewModel.DisplayText, out double displayValue))
                {
                    double result = storedValue + displayValue;

                    string formattedResult = FormatNumberForDisplay(result);
                    ViewModel.MemoryValues[index] = formattedResult;

                    MemoryList.Items.Refresh();
                }
            }
        }

        private void MemoryItemSubtract_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && ViewModel != null)
            {
                string memoryValue = button.Tag as string;
                int index = ViewModel.MemoryValues.IndexOf(memoryValue);

                if (index >= 0 && double.TryParse(memoryValue, out double storedValue) &&
                    double.TryParse(ViewModel.DisplayText, out double displayValue))
                {
                    double result = storedValue - displayValue;

                    string formattedResult = FormatNumberForDisplay(result);
                    ViewModel.MemoryValues[index] = formattedResult;

                    MemoryList.Items.Refresh();
                }
            }
        }

        private void MemoryItemRecall_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && ViewModel != null)
            {
                string memoryValue = button.Tag as string;

                if (!string.IsNullOrEmpty(memoryValue))
                {
                    ViewModel.DisplayText = memoryValue;

                    // Verificam daca numarul contine punct zecimal
                    ViewModel.HasDecimalPoint = memoryValue.Contains(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

                    MemoryPopup.IsOpen = false;
                }
            }
        }

        // Metoda personalizata pentru formatarea numarului
        private string FormatNumberForDisplay(double number)
        {
            if (ViewModel == null)
                return number.ToString();

            bool isInteger = number == Math.Floor(number);

            if (ViewModel.UseDigitGrouping)
            {
                // Formatul cu grupare a cifrelor
                if (isInteger)
                {
                    // Pentru numere intregi: 1,234
                    return ((long)number).ToString("N0", CultureInfo.CurrentCulture);
                }
                else
                {
                    // Pentru numere cu zecimale: 1,234.89
                    return number.ToString("N", CultureInfo.CurrentCulture);
                }
            }
            else
            {
                // Formatul fara grupare a cifrelor
                if (isInteger)
                {
                    // Pentru numere intregi: 1234
                    return ((long)number).ToString(CultureInfo.CurrentCulture);
                }
                else
                {
                    // Pentru numere cu zecimale: 1234.89
                    string result = number.ToString("G", CultureInfo.CurrentCulture);
                    result = result.Replace(CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator, "");

                    // Elimina zerourile inutile la final
                    return result.TrimEnd('0').TrimEnd('.');
                }
            }
        }
    }
}