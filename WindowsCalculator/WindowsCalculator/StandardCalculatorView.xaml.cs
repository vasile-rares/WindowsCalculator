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
            // Asigurăm focusul pe control
            this.Loaded += (s, e) => Keyboard.Focus(this);
            this.GotFocus += (s, e) => e.Handled = true;
        }

        private void MemoryListButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle the popup visibility - shows the popup from bottom to top
            if (MemoryPopup != null)
            {
                MemoryPopup.IsOpen = !MemoryPopup.IsOpen;
            }
        }
        
        /// <summary>
        /// Handler pentru butonul Memory Clear pe un element specific din memorie
        /// </summary>
        private void MemoryItemClear_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && ViewModel != null)
            {
                // Obținem valoarea din tag și găsim indexul în colecția de memorie
                string memoryValue = button.Tag as string;
                int index = ViewModel.MemoryValues.IndexOf(memoryValue);
                
                if (index >= 0)
                {
                    // Ștergem elementul specific
                    ViewModel.MemoryValues.RemoveAt(index);
                    ViewModel.OnPropertyChanged(nameof(ViewModel.IsMemoryEmpty));
                    
                    // Dacă lista este acum goală, închidem popup-ul
                    if (ViewModel.IsMemoryEmpty)
                    {
                        MemoryPopup.IsOpen = false;
                    }
                }
            }
        }
        
        /// <summary>
        /// Handler pentru butonul Memory Add pe un element specific din memorie
        /// </summary>
        private void MemoryItemAdd_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && ViewModel != null)
            {
                string memoryValue = button.Tag as string;
                int index = ViewModel.MemoryValues.IndexOf(memoryValue);
                
                if (index >= 0 && double.TryParse(memoryValue, out double storedValue) && 
                    double.TryParse(ViewModel.DisplayText, out double displayValue))
                {
                    // Adăugăm valoarea curentă la elementul salvat în memorie
                    double result = storedValue + displayValue;
                    
                    // Formatăm numărul folosind metoda personalizată
                    string formattedResult = FormatNumberForDisplay(result);
                    ViewModel.MemoryValues[index] = formattedResult;
                    
                    // Forțăm refresh-ul UI-ului
                    MemoryList.Items.Refresh();
                }
            }
        }
        
        /// <summary>
        /// Handler pentru butonul Memory Subtract pe un element specific din memorie
        /// </summary>
        private void MemoryItemSubtract_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && ViewModel != null)
            {
                string memoryValue = button.Tag as string;
                int index = ViewModel.MemoryValues.IndexOf(memoryValue);
                
                if (index >= 0 && double.TryParse(memoryValue, out double storedValue) && 
                    double.TryParse(ViewModel.DisplayText, out double displayValue))
                {
                    // Scădem valoarea curentă din elementul salvat în memorie
                    double result = storedValue - displayValue;
                    
                    // Formatăm numărul folosind metoda personalizată
                    string formattedResult = FormatNumberForDisplay(result);
                    ViewModel.MemoryValues[index] = formattedResult;
                    
                    // Forțăm refresh-ul UI-ului
                    MemoryList.Items.Refresh();
                }
            }
        }
        
        /// <summary>
        /// Handler pentru butonul Memory Recall pe un element specific din memorie
        /// </summary>
        private void MemoryItemRecall_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && ViewModel != null)
            {
                string memoryValue = button.Tag as string;
                
                if (!string.IsNullOrEmpty(memoryValue))
                {
                    // Setăm valoarea din memorie ca valoare curentă de afișare
                    ViewModel.DisplayText = memoryValue;
                    
                    // Verificăm dacă numărul conține punct zecimal
                    ViewModel.HasDecimalPoint = memoryValue.Contains(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                    
                    // Închidem popup-ul după ce am făcut recall
                    MemoryPopup.IsOpen = false;
                }
            }
        }
        
        /// <summary>
        /// Formatează un număr pentru afișare, folosind convenția de grupare a cifrelor dacă este activată
        /// </summary>
        private string FormatNumberForDisplay(double number)
        {
            if (ViewModel == null) 
                return number.ToString();
                
            // Verificam daca numarul este intreg (fara zecimale)
            bool isInteger = number == Math.Floor(number);
            
            // Folosim aceeași logică de formatare ca în ViewModel, dar eliminam ".00" pentru numere intregi
            if (ViewModel.UseDigitGrouping)
            {
                // Formatul cu grupare a cifrelor
                if (isInteger)
                {
                    // Pentru numere intregi: 1,234,567
                    return ((long)number).ToString("N0", CultureInfo.CurrentCulture);
                }
                else
                {
                    // Pentru numere cu zecimale: 1,234,567.89
                    return number.ToString("N", CultureInfo.CurrentCulture);
                }
            }
            else
            {
                // Formatul fără grupare a cifrelor
                if (isInteger)
                {
                    // Pentru numere intregi: 1234567
                    return ((long)number).ToString(CultureInfo.CurrentCulture);
                }
                else
                {
                    // Pentru numere cu zecimale: 1234567.89
                    // Format G to avoid group separators, then trim trailing zeros
                    string result = number.ToString("G", CultureInfo.CurrentCulture);
                    
                    // Make sure there are no group separators
                    result = result.Replace(CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator, "");
                    
                    // Elimina zerourile inutile la final (ex: 1234.5000 -> 1234.5)
                    return result.TrimEnd('0').TrimEnd('.');
                }
            }
        }
    }
}