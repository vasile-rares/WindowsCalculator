using System;
using System.ComponentModel;
using System.Windows.Input;
using WindowsCalculator.Commands;
using WindowsCalculator.Models;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace WindowsCalculator.ViewModels
{
    public class CalculatorViewModel : BaseViewModel
    {
        public event EventHandler? SettingsSaved;

        private readonly CalculatorModel _calculatorModel;
        private readonly CalculatorSettings _settings;
        private double _firstNumber;
        private double _secondNumber;
        private double _result;
        private string _operation = "";
        private string _displayText = "0";
        private string _expressionText = "";
        private bool _isNewCalculation = true;
        private bool _isOperationSelected = false;
        private bool _hasDecimalPoint = false;
        private bool _useDigitGrouping = true;
        private NumberBase _currentBase = NumberBase.DEC;
        private double _currentValue = 0;
        private bool _isStandardMode = true;

        private string _hexValue = "0";
        private string _decValue = "0";
        private string _octValue = "0";
        private string _binValue = "0";

        public enum NumberBase
        {
            HEX,
            DEC,
            OCT,
            BIN
        }

        public bool IsStandardMode
        {
            get => _isStandardMode;
            set
            {
                if (SetProperty(ref _isStandardMode, value))
                {
                    if (value)
                    {
                        CurrentBase = NumberBase.DEC;
                    }

                    ExecuteClearCommand(null);

                    SaveSettings();
                }
            }
        }

        public NumberBase CurrentBase
        {
            get => _currentBase;
            set
            {
                if (SetProperty(ref _currentBase, value))
                {
                    if (IsStandardMode && value != NumberBase.DEC)
                    {
                        _currentBase = NumberBase.DEC;
                        return;
                    }

                    DisplayText = ConvertToBase(_currentValue, _currentValue);

                    if (!IsStandardMode && !string.IsNullOrEmpty(ExpressionText))
                    {
                        UpdateExpressionForNewBase();
                    }

                    UpdateAllBaseValues();
                    SaveSettings();
                }
            }
        }

        public ObservableCollection<string> MemoryValues { get; } = new ObservableCollection<string>();
        public bool IsMemoryEmpty => MemoryValues.Count == 0;

        // Proprietate pentru accesarea stării punctului zecimal
        public bool HasDecimalPoint
        {
            get => _hasDecimalPoint;
            set => _hasDecimalPoint = value;
        }

        // Proprietăți pentru valoarea în fiecare bază
        public string HexValue
        {
            get => _hexValue;
            private set => SetProperty(ref _hexValue, value);
        }

        public string DecValue
        {
            get => _decValue;
            private set => SetProperty(ref _decValue, value);
        }

        public string OctValue
        {
            get => _octValue;
            private set => SetProperty(ref _octValue, value);
        }

        public string BinValue
        {
            get => _binValue;
            private set => SetProperty(ref _binValue, value);
        }

        public CalculatorViewModel()
        {
            _calculatorModel = new CalculatorModel();

            // Load settings
            _settings = CalculatorSettings.Load();

            // Apply loaded settings
            _useDigitGrouping = _settings.UseDigitGrouping;
            _isStandardMode = _settings.IsStandardMode;
            _currentBase = _settings.CurrentBase;

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

            ToggleDigitGroupingCommand = new RelayCommand(ExecuteToggleDigitGroupingCommand);

            // Clipboard commands
            CutCommand = new RelayCommand(ExecuteCutCommand);
            CopyCommand = new RelayCommand(ExecuteCopyCommand);
            PasteCommand = new RelayCommand(ExecutePasteCommand);

            UpdateAllBaseValues();
        }

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

        public bool UseDigitGrouping
        {
            get => _useDigitGrouping;
            set
            {
                if (SetProperty(ref _useDigitGrouping, value))
                {
                    if (!_isNewCalculation)
                    {
                        // Reformateaza numarul curent afisat
                        DisplayText = FormatNumber(double.Parse(
                            DisplayText.Replace(CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator, ""),
                            CultureInfo.CurrentCulture));

                        if (!string.IsNullOrEmpty(ExpressionText))
                        {
                            if (_operation != "")
                            {
                                if (ExpressionText.EndsWith("="))
                                {
                                    ExpressionText = $"{FormatNumber(_firstNumber)} {_operation} {FormatNumber(_secondNumber)} =";
                                }
                                else
                                {
                                    ExpressionText = $"{FormatNumber(_firstNumber)} {_operation}";
                                }
                            }
                            else if (ExpressionText.Contains("(") && ExpressionText.Contains(")"))
                            {
                                if (ExpressionText.StartsWith("1/("))
                                {
                                    double originalNumber = 1 / _result;
                                    ExpressionText = $"1/({FormatNumber(originalNumber)})";
                                }
                                else if (ExpressionText.StartsWith("sqr("))
                                {
                                    double originalNumber = Math.Sqrt(_result);
                                    ExpressionText = $"sqr({FormatNumber(originalNumber)})";
                                }
                                else if (ExpressionText.StartsWith("√("))
                                {
                                    double originalNumber = _result * _result;
                                    ExpressionText = $"√({FormatNumber(originalNumber)})";
                                }
                            }
                            else if (ExpressionText.EndsWith("="))
                            {
                                ExpressionText = $"{FormatNumber(_result)} =";
                            }
                        }
                    }

                    SaveSettings();
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

        // Clipboard commands
        public ICommand CutCommand { get; }

        public ICommand CopyCommand { get; }
        public ICommand PasteCommand { get; }

        private void SaveSettings()
        {
            _settings.UseDigitGrouping = _useDigitGrouping;
            _settings.IsStandardMode = _isStandardMode;
            _settings.CurrentBase = _currentBase;

            _settings.Save();

            // Notificare UI
            SettingsSaved?.Invoke(this, EventArgs.Empty);
        }

        // Comenzi
        private void ExecuteNumberCommand(object? parameter)
        {
            if (parameter is not string digit)
                return;

            if (!IsValidDigitForBase(digit))
            {
                return;
            }

            if (_isNewCalculation && ExpressionText.EndsWith(" = "))
            {
                ExpressionText = "";
            }

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

                TryParseNumberInBase(digit, out _currentValue);
                UpdateCurrentNumber();
            }
            else
            {
                string unformattedText = DisplayText;

                // Formatare
                if (CurrentBase != NumberBase.DEC)
                {
                    unformattedText = unformattedText.Replace(" ", "");
                }
                else
                {
                    string groupSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
                    unformattedText = unformattedText.Replace(groupSeparator, "");
                }

                unformattedText += digit;

                if (TryParseNumberInBase(unformattedText, out double value))
                {
                    _currentValue = value;
                    DisplayText = ConvertToBase(value, value);
                    UpdateCurrentNumber();
                }
            }

            UpdateAllBaseValues();
        }

        private string RemoveBasePrefix(string number)
        {
            return number;
        }

        private bool TryParseNumberInBase(string number, out double result)
        {
            result = 0;

            if (string.IsNullOrWhiteSpace(number))
                return false;

            // Cleanup la numar
            number = RemoveBasePrefix(number);
            number = number.Replace(" ", "").Replace(",", ".");

            if (!IsStandardMode)
            {
                int maxLength = CurrentBase switch
                {
                    NumberBase.HEX => 16,
                    NumberBase.DEC => 16,
                    NumberBase.OCT => 22,
                    NumberBase.BIN => 64,
                    _ => 20
                };

                if (number.Length > maxLength)
                {
                    return false;
                }
            }

            try
            {
                switch (CurrentBase)
                {
                    case NumberBase.HEX:
                        if (number.Any(c => !((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F'))))
                            return false;

                        result = Convert.ToInt64(number, 16);
                        return true;

                    case NumberBase.DEC:
                        return double.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out result);

                    case NumberBase.OCT:
                        if (number.Any(c => c < '0' || c > '7'))
                            return false;

                        result = Convert.ToInt64(number, 8);
                        return true;

                    case NumberBase.BIN:
                        if (number.Any(c => c != '0' && c != '1'))
                            return false;

                        result = Convert.ToInt64(number, 2);
                        return true;

                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidDigitForBase(string digit)
        {
            if (IsStandardMode)
            {
                return "0123456789".Contains(digit);
            }

            return CurrentBase switch
            {
                NumberBase.HEX => "0123456789ABCDEF".Contains(digit),
                NumberBase.DEC => "0123456789".Contains(digit),
                NumberBase.OCT => "01234567".Contains(digit),
                NumberBase.BIN => "01".Contains(digit),
                _ => false
            };
        }

        private string ConvertToBase(double value, double displayValue)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return "Error";
            }

            if (!IsStandardMode)
            {
                value = Math.Round(value);
            }

            long longValue = (long)value;

            if (IsStandardMode && CurrentBase == NumberBase.DEC)
            {
                return FormatNumber(value);
            }

            string numberInBase = CurrentBase switch
            {
                NumberBase.HEX => Convert.ToString(longValue, 16).ToUpper(),
                NumberBase.DEC => IsStandardMode ?
                    FormatNumber(value)
                    : longValue.ToString(),
                NumberBase.OCT => Convert.ToString(longValue, 8),
                NumberBase.BIN => Convert.ToString(longValue, 2),
                _ => longValue.ToString()
            };

            if (_useDigitGrouping)
            {
                // Format cu virgula penbtru DEC
                if (CurrentBase == NumberBase.DEC)
                {
                    if (IsStandardMode)
                    {
                        return value == Math.Floor(value) ? longValue.ToString("N0", CultureInfo.CurrentCulture) : value.ToString("G", CultureInfo.CurrentCulture);
                    }
                    return longValue.ToString("N0", CultureInfo.CurrentCulture);
                }
                else if (CurrentBase == NumberBase.HEX)
                {
                    StringBuilder result = new StringBuilder();
                    int digitCount = 0;

                    for (int i = numberInBase.Length - 1; i >= 0; i--)
                    {
                        result.Insert(0, numberInBase[i]);
                        digitCount++;

                        // Spacing
                        if (digitCount % 4 == 0 && i > 0)
                        {
                            result.Insert(0, ' ');
                        }
                    }

                    return result.ToString();
                }
                else if (CurrentBase == NumberBase.OCT)
                {
                    StringBuilder result = new StringBuilder();
                    int digitCount = 0;

                    for (int i = numberInBase.Length - 1; i >= 0; i--)
                    {
                        result.Insert(0, numberInBase[i]);
                        digitCount++;

                        if (digitCount % 3 == 0 && i > 0)
                        {
                            result.Insert(0, ' ');
                        }
                    }

                    return result.ToString();
                }
                else if (CurrentBase == NumberBase.BIN)
                {
                    StringBuilder result = new StringBuilder();
                    int digitCount = 0;

                    for (int i = numberInBase.Length - 1; i >= 0; i--)
                    {
                        result.Insert(0, numberInBase[i]);
                        digitCount++;

                        if (digitCount % 4 == 0 && i > 0)
                        {
                            result.Insert(0, ' ');
                        }
                    }

                    return result.ToString();
                }
            }

            return numberInBase;
        }

        private void ExecuteEqualsCommand(object? parameter)
        {
            double currentNumber = _currentValue;

            if (_isOperationSelected)
            {
                _secondNumber = currentNumber;
            }

            if (_operation == "")
            {
                _result = currentNumber;
                if (!IsStandardMode && !string.IsNullOrEmpty(ExpressionText) && !ExpressionText.EndsWith(" = "))
                {
                    ExpressionText += $" = ";
                }
                else
                {
                    ExpressionText = $"{FormatNumberForExpression(_result)} = ";
                }
                _currentValue = _result;
                DisplayText = ConvertToBase(_result, _result);
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

                    if (!IsStandardMode)
                    {
                        _result = Math.Round(_result);
                        if (!string.IsNullOrEmpty(ExpressionText) && !ExpressionText.EndsWith(" = "))
                        {
                            if (ExpressionText.EndsWith(" "))
                            {
                                ExpressionText += $"{FormatNumberForExpression(currentNumber)} = ";
                            }
                            else
                            {
                                ExpressionText += $" {FormatNumberForExpression(currentNumber)} = ";
                            }
                        }
                        else
                        {
                            ExpressionText = $"{FormatNumberForExpression(_firstNumber)} {_operation} {FormatNumberForExpression(currentNumber)} = ";
                        }
                    }
                    else
                    {
                        ExpressionText = $"{FormatNumberForExpression(_firstNumber)} {_operation} {FormatNumberForExpression(currentNumber)} = ";
                    }

                    _currentValue = _result;
                    DisplayText = ConvertToBase(_result, _result);
                }
                catch (Exception ex)
                {
                    DisplayText = "Eroare: " + ex.Message;
                }
            }

            _isNewCalculation = true;
            _hasDecimalPoint = DisplayText.Contains(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

            UpdateAllBaseValues();
        }

        private void ExecuteClearCommand(object? parameter)
        {
            _firstNumber = 0;
            _secondNumber = 0;
            _result = 0;
            _currentValue = 0;
            _operation = "";
            DisplayText = "0";
            ExpressionText = "";
            _isNewCalculation = true;
            _isOperationSelected = false;
            _hasDecimalPoint = false;

            UpdateAllBaseValues();
        }

        private void ExecuteClearEntryCommand(object? parameter)
        {
            DisplayText = "0";
            _currentValue = 0;
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
                _currentValue = 0;
                _hasDecimalPoint = false;
            }
            else
            {
                string unformattedText = DisplayText;

                if (CurrentBase != NumberBase.DEC)
                {
                    unformattedText = unformattedText.Replace(" ", "");
                }
                else
                {
                    string groupSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
                    unformattedText = unformattedText.Replace(groupSeparator, "");
                }

                string newText = unformattedText.Remove(unformattedText.Length - 1);

                if (newText.Length == 0 || newText == "-")
                {
                    DisplayText = "0";
                    _currentValue = 0;
                }
                else
                {
                    if (TryParseNumberInBase(newText, out double value))
                    {
                        _currentValue = value;
                        DisplayText = ConvertToBase(value, value);
                    }
                    else
                    {
                        _currentValue = 0;
                        DisplayText = "0";
                    }
                }
            }

            _hasDecimalPoint = false;
            UpdateCurrentNumber();
            UpdateAllBaseValues();
        }

        private void ExecuteOperationCommand(object? parameter)
        {
            if (parameter is not string operation)
                return;

            bool isCurrentOperationHigherPrecedence = IsStandardMode && (operation == "×" || operation == "÷");
            bool isPreviousOperationLowerPrecedence = IsStandardMode && (_operation == "+" || _operation == "-");

            if (!_isNewCalculation && !_isOperationSelected && !string.IsNullOrEmpty(_operation))
            {
                double currentNumber = _currentValue;
                try
                {
                    if (isCurrentOperationHigherPrecedence && isPreviousOperationLowerPrecedence)
                    {
                        switch (_operation)
                        {
                            case "+":
                                _result = _calculatorModel.Add(_firstNumber, currentNumber);
                                break;

                            case "-":
                                _result = _calculatorModel.Subtract(_firstNumber, currentNumber);
                                break;
                        }

                        _firstNumber = _result;
                        _currentValue = _result;
                        DisplayText = ConvertToBase(_result, _result);
                        _operation = operation;
                        ExpressionText = $"{FormatNumberForExpression(_firstNumber)} {operation} ";
                    }
                    else
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
                                    DisplayText = "Cannot divide by zero";
                                    _isNewCalculation = true;
                                    return;
                                }
                                _result = _calculatorModel.Divide(_firstNumber, currentNumber);
                                break;
                        }

                        if (!IsStandardMode)
                        {
                            _result = Math.Round(_result);
                        }

                        _currentValue = _result;
                        _firstNumber = _result;
                        DisplayText = ConvertToBase(_result, _result);

                        if (!string.IsNullOrEmpty(ExpressionText))
                        {
                            if (IsStandardMode)
                            {
                                if (ExpressionText.Contains(" = "))
                                {
                                    ExpressionText = $"{FormatNumberForExpression(_firstNumber)} {operation} ";
                                }
                                else if (ExpressionText.EndsWith(" "))
                                {
                                    ExpressionText = $"{FormatNumberForExpression(_result)} {operation} ";
                                }
                                else
                                {
                                    ExpressionText = $" {FormatNumberForExpression(_result)} {operation} ";
                                }
                            }
                            else
                            {
                                if (ExpressionText.Contains(" = "))
                                {
                                    ExpressionText = $"{FormatNumberForExpression(_firstNumber)} {operation} ";
                                }
                                else if (ExpressionText.EndsWith(" "))
                                {
                                    ExpressionText += $"{FormatNumberForExpression(currentNumber)} {operation} ";
                                }
                                else
                                {
                                    ExpressionText += $" {FormatNumberForExpression(currentNumber)} {operation} ";
                                }
                            }
                        }
                        else
                        {
                            ExpressionText = $"{FormatNumberForExpression(_firstNumber)} {operation} ";
                        }

                        _operation = operation;
                    }
                }
                catch (Exception ex)
                {
                    DisplayText = "Error: " + ex.Message;
                    _isNewCalculation = true;
                    return;
                }
            }
            else if (_isOperationSelected)
            {
                _operation = operation;

                if (!string.IsNullOrEmpty(ExpressionText))
                {
                    if (ExpressionText.EndsWith(" + ") || ExpressionText.EndsWith(" - ") ||
                        ExpressionText.EndsWith(" × ") || ExpressionText.EndsWith(" ÷ "))
                    {
                        ExpressionText = ExpressionText.Substring(0, ExpressionText.Length - 3) + $" {operation} ";
                    }
                    else if (ExpressionText.EndsWith(" "))
                    {
                        ExpressionText += $"{operation} ";
                    }
                    else
                    {
                        ExpressionText = $"{FormatNumberForExpression(_firstNumber)} {operation} ";
                    }
                }
                else
                {
                    ExpressionText = $"{FormatNumberForExpression(_firstNumber)} {operation} ";
                }
            }
            else
            {
                if (_isNewCalculation)
                {
                    _firstNumber = _result;
                }
                else
                {
                    _firstNumber = _currentValue;
                }

                if (!string.IsNullOrEmpty(ExpressionText) && !ExpressionText.Contains(" = "))
                {
                    if (ExpressionText.EndsWith(" "))
                    {
                        ExpressionText += $"{operation} ";
                    }
                    else
                    {
                        ExpressionText += $" {operation} ";
                    }
                }
                else
                {
                    ExpressionText = $"{FormatNumberForExpression(_firstNumber)} {operation} ";
                }

                _operation = operation;
            }

            _isOperationSelected = true;
            _isNewCalculation = false;
            _hasDecimalPoint = false;
        }

        // Functiile 1/x, x², √
        private void ExecuteSpecialFunctionCommand(object? parameter)
        {
            if (parameter is not string function) return;

            double currentNumber = _currentValue;
            bool shouldPreservePendingOperation = !string.IsNullOrEmpty(_operation) && !_isNewCalculation;
            double savedFirstNumber = _firstNumber;
            string savedOperation = _operation;
            string savedExpression = ExpressionText;

            try
            {
                switch (function)
                {
                    case "1/x":
                        if (currentNumber == 0)
                        {
                            DisplayText = "Cannot divide by zero";
                            _isNewCalculation = true;
                            return;
                        }
                        _result = _calculatorModel.Reciprocal(currentNumber);
                        ExpressionText = shouldPreservePendingOperation
                            ? savedExpression
                            : $"1/({FormatNumberForExpression(currentNumber)})";
                        break;

                    case "x²":
                        _result = _calculatorModel.Square(currentNumber);
                        ExpressionText = shouldPreservePendingOperation
                            ? savedExpression
                            : $"sqr({FormatNumberForExpression(currentNumber)})";
                        break;

                    case "√":
                        if (currentNumber < 0)
                        {
                            DisplayText = "Cannot compute square root of a negative number.";
                            _isNewCalculation = true;
                            return;
                        }
                        _result = _calculatorModel.SquareRoot(currentNumber);
                        ExpressionText = shouldPreservePendingOperation
                            ? savedExpression
                            : $"√({FormatNumberForExpression(currentNumber)})";
                        break;
                }

                DisplayText = ConvertToBase(_result, _result);
            }
            catch (Exception ex)
            {
                DisplayText = "Error: " + ex.Message;
                _isNewCalculation = true;
                return;
            }

            if (shouldPreservePendingOperation)
            {
                _currentValue = _result;
                _isNewCalculation = false;
                _firstNumber = savedFirstNumber;
                _operation = savedOperation;
                _isOperationSelected = false;
            }
            else
            {
                _isNewCalculation = true;
                _currentValue = _result;
            }

            _hasDecimalPoint = DisplayText.Contains(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
        }

        private void ExecutePercentageCommand(object? parameter)
        {
            if (_operation != "" && !_isNewCalculation)
            {
                try
                {
                    string unformattedText = DisplayText;

                    // Sterge formatarile pentru alte baze
                    if (CurrentBase != NumberBase.DEC)
                    {
                        unformattedText = unformattedText.Replace(" ", "");
                    }
                    else
                    {
                        // Sterge formatarile pentru DEC
                        string groupSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
                        unformattedText = unformattedText.Replace(groupSeparator, "");
                    }

                    if (TryParseNumberInBase(unformattedText, out double currentNumber))
                    {
                        double percentValue = _calculatorModel.Percentage(_firstNumber, currentNumber);

                        _currentValue = percentValue;
                        DisplayText = ConvertToBase(percentValue, percentValue);
                        ExpressionText = $"{FormatNumberForExpression(_firstNumber)} {_operation} {FormatNumberForExpression(currentNumber)}% =";
                        _isNewCalculation = true;
                        UpdateCurrentNumber();
                    }
                }
                catch (Exception ex)
                {
                    DisplayText = "Error: " + ex.Message;
                }
            }
            else
            {
                _currentValue = 0;
                DisplayText = "0";
                _isNewCalculation = true;
                _hasDecimalPoint = false;
                UpdateCurrentNumber();
            }
        }

        // .
        private void ExecuteDecimalPointCommand(object? parameter)
        {
            if (!IsStandardMode)
            {
                return;
            }

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

        // ±
        private void ExecuteNegateCommand(object? parameter)
        {
            if (DisplayText != "0")
            {
                string unformattedText = DisplayText;

                if (CurrentBase != NumberBase.DEC)
                {
                    unformattedText = unformattedText.Replace(" ", "");
                }
                else
                {
                    string groupSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
                    unformattedText = unformattedText.Replace(groupSeparator, "");
                }

                if (unformattedText.StartsWith("-"))
                {
                    unformattedText = unformattedText.Substring(1);
                }
                else
                {
                    unformattedText = "-" + unformattedText;
                }

                if (TryParseNumberInBase(unformattedText, out double value))
                {
                    _currentValue = value;
                    DisplayText = ConvertToBase(value, value);
                    UpdateCurrentNumber();
                    UpdateAllBaseValues();
                }
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
                DisplayText = MemoryValues[^1];
                _hasDecimalPoint = DisplayText.Contains(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            }
        }

        private void ExecuteMemoryAddCommand(object? parameter)
        {
            if (double.TryParse(DisplayText, out double value))
            {
                string formattedNumber = FormatNumberForMemory(value.ToString());
                MemoryValues.Add(formattedNumber);
                OnPropertyChanged(nameof(IsMemoryEmpty));
            }
        }

        private void ExecuteMemorySubtractCommand(object? parameter)
        {
            if (MemoryValues.Count > 0 && double.TryParse(DisplayText, out double value))
            {
                double lastValue = double.Parse(MemoryValues[^1]);
                double result = lastValue - value;

                MemoryValues[^1] = FormatNumberForMemory(result.ToString());
                OnPropertyChanged(nameof(IsMemoryEmpty));
            }
        }

        private void ExecuteMemoryStoreCommand(object? parameter)
        {
            string formattedNumber = FormatNumberForMemory(DisplayText);
            MemoryValues.Add(formattedNumber);
            OnPropertyChanged(nameof(IsMemoryEmpty));
        }

        private void ExecuteToggleDigitGroupingCommand(object? parameter)
        {
            UseDigitGrouping = !UseDigitGrouping;
        }

        // Clipboard command implementations
        private void ExecuteCutCommand(object? parameter)
        {
            Clipboard.SetText(DisplayText);

            _currentValue = 0;
            DisplayText = "0";
            _isNewCalculation = true;
            _hasDecimalPoint = false;
            UpdateCurrentNumber();
        }

        private void ExecuteCopyCommand(object? parameter)
        {
            Clipboard.SetText(DisplayText);
        }

        private void ExecutePasteCommand(object? parameter)
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    string clipboardText = Clipboard.GetText().Trim();

                    if (TryParseNumberInBase(clipboardText, out double pastedValue))
                    {
                        _currentValue = pastedValue;
                        DisplayText = ConvertToBase(pastedValue, pastedValue);
                        _isNewCalculation = false;

                        _hasDecimalPoint = IsStandardMode && clipboardText.Contains('.');

                        UpdateCurrentNumber();
                        UpdateAllBaseValues();
                    }
                    else
                    {
                        DisplayText = "Invalid input";
                        _isNewCalculation = true;
                    }
                }
            }
            catch (Exception)
            {
                DisplayText = "Invalid input";
                _isNewCalculation = true;
            }
        }

        // Helper methods
        private void UpdateCurrentNumber()
        {
            if (_isOperationSelected || (_operation != "" && !_isNewCalculation))
            {
                _secondNumber = _currentValue;
            }
            else
            {
                _firstNumber = _currentValue;
            }
        }

        private string FormatNumber(double number)
        {
            NumberFormatInfo numberFormat = CultureInfo.CurrentCulture.NumberFormat;

            // Formatare fara Digit Grouping
            if (!_useDigitGrouping)
            {
                if (number == Math.Floor(number))
                {
                    return ((long)number).ToString(CultureInfo.CurrentCulture);
                }
                return number.ToString("G", CultureInfo.CurrentCulture);
            }

            // Formatare cu Digit Grouping
            if (number == Math.Floor(number))
            {
                return ((long)number).ToString("N0", numberFormat);
            }

            return number.ToString("G", numberFormat);
        }

        private string FormatNumberForExpression(double number)
        {
            if (!IsStandardMode)
            {
                return ConvertToBase(number, number);
            }

            // Formatare fara zecimale pentru numere intregi
            if (number == Math.Floor(number))
            {
                if (_useDigitGrouping)
                {
                    return ((long)number).ToString("N0", CultureInfo.CurrentCulture);
                }
                return ((long)number).ToString(CultureInfo.CurrentCulture);
            }

            // Format standard
            if (_useDigitGrouping)
            {
                return number.ToString("G", CultureInfo.CurrentCulture);
            }
            return number.ToString("G", CultureInfo.CurrentCulture)
                .Replace(CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator, "");
        }

        private string FormatNumberForMemory(string numberText)
        {
            if (double.TryParse(numberText, out double number))
            {
                bool isInteger = (number == Math.Floor(number));

                if (isInteger)
                {
                    if (_useDigitGrouping)
                    {
                        return ((long)number).ToString("N0", CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        return ((long)number).ToString(CultureInfo.CurrentCulture);
                    }
                }
                else
                {
                    string result;
                    if (_useDigitGrouping)
                    {
                        result = number.ToString("G", CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        result = number.ToString("G", CultureInfo.CurrentCulture)
                            .Replace(CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator, "");
                    }

                    return result.TrimEnd('0').TrimEnd('.'); // Scoate zerourile finale
                }
            }

            return numberText;
        }

        private void UpdateExpressionForNewBase()
        {
            if (string.IsNullOrEmpty(ExpressionText))
                return;

            // Nu actulizam expresia daca este o expresie simpla
            if (ExpressionText.Contains("(") && ExpressionText.Contains(")"))
                return;

            bool hasResult = ExpressionText.EndsWith(" = ");

            if (hasResult && _isNewCalculation)
            {
                ExpressionText = $"{FormatNumberForExpression(_result)} = ";
                return;
            }

            // Reconstruim expresia cu numerele convertite in noua baza
            if (_operation != "" && !_isNewCalculation)
            {
                ExpressionText = $"{FormatNumberForExpression(_firstNumber)} {_operation}";
                return;
            }
            else if (_operation != "" && _isOperationSelected)
            {
                ExpressionText = $"{FormatNumberForExpression(_firstNumber)} {_operation}";
                return;
            }
            else if (hasResult)
            {
                ExpressionText = $"{FormatNumberForExpression(_firstNumber)} {_operation} {FormatNumberForExpression(_secondNumber)} = ";
                return;
            }
            else if (ExpressionText.Contains("+") || ExpressionText.Contains("-") ||
                    ExpressionText.Contains("×") || ExpressionText.Contains("÷"))
            {
                // Expresie complexa cu mai multe operatii (pentru Programmer)
                string[] parts = ExpressionText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    StringBuilder newExpression = new StringBuilder();

                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (parts[i] == "+" || parts[i] == "-" || parts[i] == "×" || parts[i] == "÷" || parts[i] == "=")
                        {
                            newExpression.Append($" {parts[i]} ");
                        }
                        else if (parts[i].EndsWith("%"))
                        {
                            // Cazul special %
                            string numStr = parts[i].TrimEnd('%');
                            if (double.TryParse(numStr, out double num))
                            {
                                newExpression.Append($"{FormatNumberForExpression(num)}%");
                            }
                            else
                            {
                                newExpression.Append(parts[i]);
                            }
                        }
                        else if (double.TryParse(parts[i], out double num))
                        {
                            newExpression.Append(FormatNumberForExpression(num));
                        }
                        else
                        {
                            newExpression.Append(parts[i]);
                        }
                    }

                    ExpressionText = newExpression.ToString().Trim();
                }
            }
        }

        private void UpdateAllBaseValues()
        {
            var currentBase = _currentBase;

            _currentBase = NumberBase.HEX;
            HexValue = ConvertToBase(_currentValue, _currentValue);

            _currentBase = NumberBase.DEC;
            DecValue = ConvertToBase(_currentValue, _currentValue);

            _currentBase = NumberBase.OCT;
            OctValue = ConvertToBase(_currentValue, _currentValue);

            _currentBase = NumberBase.BIN;
            BinValue = ConvertToBase(_currentValue, _currentValue);

            _currentBase = currentBase;
        }
    }
}