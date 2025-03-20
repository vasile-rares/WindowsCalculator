using System;
using System.ComponentModel;
using System.Windows.Input;
using WindowsCalculator.Commands;
using WindowsCalculator.Models;
using System.Globalization;
using System.Collections.ObjectModel;

namespace WindowsCalculator.ViewModels
{
    public class CalculatorViewModel : BaseViewModel
    {
        private readonly CalculatorModel _calculatorModel;
        private double _firstNumber;
        private double _secondNumber;
        private double _result;
        private string _operation = "";
        private string _displayText = "0";
        private string _expressionText = "";
        private bool _isNewCalculation = true;
        private bool _isOperationSelected = false;
        private bool _hasDecimalPoint = false;
        private bool _useDigitGrouping = true; // Adăugăm o proprietate pentru activarea/dezactivarea grupării cifrelor

        public ObservableCollection<string> MemoryValues { get; } = new ObservableCollection<string>();
        public bool IsMemoryEmpty => MemoryValues.Count == 0;

        public CalculatorViewModel()
        {
            _calculatorModel = new CalculatorModel();

            // Initialize commands
            NumberCommand = new RelayCommand(ExecuteNumberCommand);
            OperationCommand = new RelayCommand(ExecuteOperationCommand);
            EqualsCommand = new RelayCommand(ExecuteEqualsCommand);
            ClearCommand = new RelayCommand(ExecuteClearCommand);
            ClearEntryCommand = new RelayCommand(ExecuteClearEntryCommand);
            BackspaceCommand = new RelayCommand(ExecuteBackspaceCommand);
            DecimalPointCommand = new RelayCommand(ExecuteDecimalPointCommand);
            SpecialFunctionCommand = new RelayCommand(ExecuteSpecialFunctionCommand);
            NegateCommand = new RelayCommand(ExecuteNegateCommand);
            PercentageCommand = new RelayCommand(ExecutePercentageCommand);

            // Memory commands
            MemoryClearCommand = new RelayCommand(ExecuteMemoryClearCommand);
            MemoryRecallCommand = new RelayCommand(ExecuteMemoryRecallCommand);
            MemoryAddCommand = new RelayCommand(ExecuteMemoryAddCommand);
            MemorySubtractCommand = new RelayCommand(ExecuteMemorySubtractCommand);
            MemoryStoreCommand = new RelayCommand(ExecuteMemoryStoreCommand);

            // Adăugăm comandă pentru activarea/dezactivarea grupării cifrelor
            ToggleDigitGroupingCommand = new RelayCommand(ExecuteToggleDigitGroupingCommand);
        }

        // Properties with notification
        public string DisplayText
        {
            get => _displayText;
            set => SetProperty(ref _displayText, value);
        }

        public string ExpressionText
        {
            get => _expressionText;
            set => SetProperty(ref _expressionText, value);
        }

        // Proprietate pentru gruparea cifrelor
        public bool UseDigitGrouping
        {
            get => _useDigitGrouping;
            set
            {
                if (SetProperty(ref _useDigitGrouping, value))
                {
                    // Actualizează afișarea atunci când setarea se schimbă
                    if (!_isNewCalculation)
                    {
                        // Reformatează numărul curent afișat
                        DisplayText = FormatNumber(double.Parse(
                            DisplayText.Replace(CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator, ""),
                            CultureInfo.CurrentCulture));

                        // Actualizează și ExpressionText dacă acesta există
                        if (!string.IsNullOrEmpty(ExpressionText))
                        {
                            // Verifică dacă avem un text de expresie cu operație
                            if (_operation != "")
                            {
                                if (ExpressionText.EndsWith("="))
                                {
                                    // Cazul când avem un rezultat complet (cu "=")
                                    ExpressionText = $"{FormatNumber(_firstNumber)} {_operation} {FormatNumber(_secondNumber)} =";
                                }
                                else
                                {
                                    // Cazul când avem doar o operație în desfășurare
                                    ExpressionText = $"{FormatNumber(_firstNumber)} {_operation}";
                                }
                            }
                            else if (ExpressionText.Contains("(") && ExpressionText.Contains(")"))
                            {
                                // Cazul funcțiilor speciale (radical, putere, etc.)
                                if (ExpressionText.StartsWith("1/("))
                                {
                                    // Reciprocal
                                    double originalNumber = 1 / _result;
                                    ExpressionText = $"1/({FormatNumber(originalNumber)})";
                                }
                                else if (ExpressionText.StartsWith("sqr("))
                                {
                                    // Square
                                    double originalNumber = Math.Sqrt(_result);
                                    ExpressionText = $"sqr({FormatNumber(originalNumber)})";
                                }
                                else if (ExpressionText.StartsWith("√("))
                                {
                                    // Square root
                                    double originalNumber = _result * _result;
                                    ExpressionText = $"√({FormatNumber(originalNumber)})";
                                }
                            }
                            else if (ExpressionText.EndsWith("="))
                            {
                                // Doar un rezultat simplu
                                ExpressionText = $"{FormatNumber(_result)} =";
                            }
                        }
                    }
                }
            }
        }

        // Commands
        public ICommand NumberCommand { get; }

        public ICommand OperationCommand { get; }
        public ICommand EqualsCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand ClearEntryCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand DecimalPointCommand { get; }
        public ICommand SpecialFunctionCommand { get; }
        public ICommand NegateCommand { get; }
        public ICommand PercentageCommand { get; }

        // Memory commands
        public ICommand MemoryClearCommand { get; }

        public ICommand MemoryRecallCommand { get; }
        public ICommand MemoryAddCommand { get; }
        public ICommand MemorySubtractCommand { get; }
        public ICommand MemoryStoreCommand { get; }

        public ICommand ToggleDigitGroupingCommand { get; }

        // Command implementations
        private void ExecuteNumberCommand(object? parameter)
        {
            if (parameter is not string digit) return;

            if (_isNewCalculation || _isOperationSelected || DisplayText == "0")
            {
                if (_isOperationSelected)
                {
                    DisplayText = digit;
                    _isOperationSelected = false;
                }
                else if (_isNewCalculation || DisplayText == "0")
                {
                    DisplayText = digit;
                    _isNewCalculation = false;
                }

                UpdateCurrentNumber();
            }
            else
            {
                // Get the current display text without formatting
                string unformattedText = DisplayText;

                // Remove any existing grouping separators
                string groupSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
                unformattedText = unformattedText.Replace(groupSeparator, "");

                // Append the new digit
                unformattedText += digit;

                // Try to parse as a number
                if (double.TryParse(unformattedText, NumberStyles.Any, CultureInfo.CurrentCulture, out double value))
                {
                    // If we have a decimal point, we need to handle formatting differently
                    if (_hasDecimalPoint)
                    {
                        // Keep the cursor position at the end when there's a decimal part
                        string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                        string[] parts = unformattedText.Split(decimalSeparator);

                        if (parts.Length == 2)
                        {
                            // Get the decimal part which should not have formatting
                            string decimalPart = parts[1];

                            if (_useDigitGrouping)
                            {
                                // Format the integer part with grouping
                                double integerValue = double.Parse(parts[0], CultureInfo.CurrentCulture);
                                string formattedInteger = integerValue.ToString("N0", CultureInfo.CurrentCulture);

                                // Combine them
                                DisplayText = formattedInteger + decimalSeparator + decimalPart;
                            }
                            else
                            {
                                // Format without grouping
                                DisplayText = parts[0] + decimalSeparator + decimalPart;
                            }
                        }
                    }
                    else
                    {
                        if (_useDigitGrouping)
                        {
                            // No decimal point, format with grouping
                            DisplayText = value.ToString("N0", CultureInfo.CurrentCulture);
                        }
                        else
                        {
                            // No grouping
                            DisplayText = unformattedText;
                        }
                    }
                }
                else
                {
                    // If parsing fails, just add the digit without formatting
                    DisplayText += digit;
                }

                UpdateCurrentNumber();
            }
        }

        private void ExecuteOperationCommand(object? parameter)
        {
            if (parameter is not string operation) return;

            if (!_isOperationSelected && !_isNewCalculation)
            {
                if (_operation != "")
                {
                    ExecuteEqualsCommand(null);
                }
                _firstNumber = double.Parse(DisplayText);
                _operation = operation;
                ExpressionText = $"{FormatNumber(_firstNumber)} {operation}";
                _isOperationSelected = true;
            }
            else if (_isOperationSelected)
            {
                _operation = operation;
                ExpressionText = $"{FormatNumber(_firstNumber)} {operation}";
            }
            else // _isNewCalculation
            {
                _operation = operation;
                ExpressionText = $"{FormatNumber(_result)} {operation}";
                _firstNumber = _result;
                _isNewCalculation = false;
                _isOperationSelected = true;
            }
            _hasDecimalPoint = false;
        }

        private void ExecuteEqualsCommand(object? parameter)
        {
            double currentNumber;

            if (!double.TryParse(DisplayText, out currentNumber))
            {
                DisplayText = "Invalid Input";
                _isNewCalculation = true;
                return;
            }

            if (_isOperationSelected)
            {
                _secondNumber = currentNumber;
            }

            if (_operation == "")
            {
                _result = currentNumber;
                ExpressionText = $"{FormatNumber(_result)} =";
                DisplayText = FormatNumber(_result);
            }
            else if (!_isNewCalculation)
            {
                try
                {
                    switch (_operation)
                    {
                        case "+":
                            _result = _calculatorModel.Add(_firstNumber, currentNumber);
                            break;

                        case "-":
                            _result = _calculatorModel.Subtract(_firstNumber, currentNumber);
                            break;

                        case "×":
                            _result = _calculatorModel.Multiply(_firstNumber, currentNumber);
                            break;

                        case "÷":
                            if (currentNumber == 0)
                            {
                                DisplayText = "Cannot divide by zero.";
                                _isNewCalculation = true;
                                break;
                            }
                            _result = _calculatorModel.Divide(_firstNumber, currentNumber);
                            break;
                    }

                    ExpressionText = $"{FormatNumber(_firstNumber)} {_operation} {FormatNumber(currentNumber)} =";
                    DisplayText = FormatNumber(_result);
                }
                catch (Exception ex)
                {
                    DisplayText = "Eroare: " + ex.Message;
                }
            }

            _isNewCalculation = true;
            _hasDecimalPoint = DisplayText.Contains(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
        }

        private void ExecuteClearCommand(object? parameter)
        {
            _firstNumber = 0;
            _secondNumber = 0;
            _result = 0;
            _operation = "";
            DisplayText = "0";
            ExpressionText = "";
            _isNewCalculation = true;
            _isOperationSelected = false;
            _hasDecimalPoint = false;
        }

        private void ExecuteClearEntryCommand(object? parameter)
        {
            DisplayText = "0";
            _hasDecimalPoint = false;
            UpdateCurrentNumber();
        }

        private void ExecuteBackspaceCommand(object? parameter)
        {
            if (_isNewCalculation || _isOperationSelected)
            {
                return;
            }

            if (DisplayText.Length <= 1 || (DisplayText.Length == 2 && DisplayText[0] == '-'))
            {
                DisplayText = "0";
                _hasDecimalPoint = false;
            }
            else
            {
                char removedChar = DisplayText[DisplayText.Length - 1];
                if (removedChar.ToString() == CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
                {
                    _hasDecimalPoint = false;
                }
                DisplayText = DisplayText.Remove(DisplayText.Length - 1);
            }

            UpdateCurrentNumber();
        }

        private void ExecuteDecimalPointCommand(object? parameter)
        {
            string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            if (_isNewCalculation || _isOperationSelected)
            {
                DisplayText = "0" + decimalSeparator;
                _isNewCalculation = false;
                _isOperationSelected = false;
                _hasDecimalPoint = true;
            }
            else if (!_hasDecimalPoint)
            {
                DisplayText += decimalSeparator;
                _hasDecimalPoint = true;
            }
        }

        private void ExecuteSpecialFunctionCommand(object? parameter)
        {
            if (parameter is not string function) return;

            double currentNumber = double.Parse(DisplayText);

            try
            {
                switch (function)
                {
                    case "1/x":
                        if (currentNumber == 0)
                        {
                            DisplayText = "Cannot compute reciprocal of zero.";
                            break;
                        }
                        _result = _calculatorModel.Reciprocal(currentNumber);
                        ExpressionText = $"1/({FormatNumber(currentNumber)})";
                        break;

                    case "x²":
                        _result = _calculatorModel.Square(currentNumber);
                        ExpressionText = $"sqr({FormatNumber(currentNumber)})";
                        break;

                    case "√":
                        if (currentNumber < 0)
                        {
                            DisplayText = "Cannot compute square root of a negative number.";
                            break;
                        }
                        _result = _calculatorModel.SquareRoot(currentNumber);
                        ExpressionText = $"√({FormatNumber(currentNumber)})";
                        break;
                }

                DisplayText = FormatNumber(_result);
            }
            catch (Exception ex)
            {
                DisplayText = ex.Message;
            }

            _isNewCalculation = true;
            _hasDecimalPoint = DisplayText.Contains(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
        }

        private void ExecuteNegateCommand(object? parameter)
        {
            if (DisplayText != "0")
            {
                if (DisplayText.StartsWith("-"))
                {
                    DisplayText = DisplayText.Substring(1);
                }
                else
                {
                    DisplayText = "-" + DisplayText;
                }
                UpdateCurrentNumber();
            }
        }

        private void ExecutePercentageCommand(object? parameter)
        {
            double currentNumber = double.Parse(DisplayText);

            if (_operation != "" && !_isNewCalculation)
            {
                double percentValue = _calculatorModel.Percentage(_firstNumber, currentNumber);
                DisplayText = FormatNumber(percentValue);
                UpdateCurrentNumber();
            }
        }

        // Memory functions
        private void ExecuteMemoryClearCommand(object? parameter)
        {
            MemoryValues.Clear();
            OnPropertyChanged(nameof(IsMemoryEmpty));
        }

        private void ExecuteMemoryRecallCommand(object? parameter)
        {
            if (MemoryValues.Count > 0)
            {
                DisplayText = MemoryValues[^1]; // Recall last stored value
                _hasDecimalPoint = DisplayText.Contains(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            }
        }

        private void ExecuteMemoryAddCommand(object? parameter)
        {
            if (double.TryParse(DisplayText, out double value))
            {
                MemoryValues.Add(value.ToString());
                OnPropertyChanged(nameof(IsMemoryEmpty));
            }
        }

        private void ExecuteMemorySubtractCommand(object? parameter)
        {
            if (MemoryValues.Count > 0 && double.TryParse(DisplayText, out double value))
            {
                double lastValue = double.Parse(MemoryValues[^1]);
                MemoryValues[^1] = (lastValue - value).ToString();
                OnPropertyChanged(nameof(IsMemoryEmpty));
            }
        }

        private void ExecuteMemoryStoreCommand(object? parameter)
        {
            MemoryValues.Add(DisplayText);
            OnPropertyChanged(nameof(IsMemoryEmpty));
        }

        //private bool CanExecuteMemoryCommand(object? parameter)
        //{
        //    return _hasMemoryValue;
        //}

        private void ExecuteToggleDigitGroupingCommand(object? parameter)
        {
            UseDigitGrouping = !UseDigitGrouping;
        }

        // Helper methods
        private void UpdateCurrentNumber()
        {
            // Remove formatting when parsing the display text
            string unformattedText = DisplayText;
            string groupSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
            unformattedText = unformattedText.Replace(groupSeparator, "");

            if (double.TryParse(unformattedText, NumberStyles.Any, CultureInfo.CurrentCulture, out double currentValue))
            {
                if (_isOperationSelected || (_operation != "" && !_isNewCalculation))
                {
                    _secondNumber = currentValue;
                }
                else
                {
                    _firstNumber = currentValue;
                }
            }
        }

        private string FormatNumber(double number)
        {
            // Get the current culture's number format
            NumberFormatInfo numberFormat = CultureInfo.CurrentCulture.NumberFormat;

            // Dacă gruparea cifrelor este dezactivată, folosim un format simplu
            if (!_useDigitGrouping)
            {
                if (number == Math.Floor(number))
                {
                    return ((long)number).ToString(CultureInfo.CurrentCulture);
                }
                return number.ToString(CultureInfo.CurrentCulture);
            }

            // For whole numbers, format with no decimal part but with digit grouping
            if (number == Math.Floor(number))
            {
                // Format the number with thousand separators but no decimal places
                return ((long)number).ToString("N0", numberFormat);
            }

            // For decimal numbers, include the decimal part
            // Use "N" format which includes digit grouping/thousand separators
            return number.ToString("N", numberFormat);
        }
    }
}