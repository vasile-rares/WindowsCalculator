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

        // Adaugam proprietati pentru afisarea valorii in toate bazele
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
                    // Dacă suntem în modul Standard, nu permite schimbarea bazei
                    if (IsStandardMode && value != NumberBase.DEC)
                    {
                        _currentBase = NumberBase.DEC;
                        return;
                    }

                    // Convert the stored value to the new base
                    DisplayText = ConvertToBase(_currentValue, _currentValue);

                    // Actualizăm și ExpressionText pentru a reflecta noua bază în modul Programmer
                    if (!IsStandardMode && !string.IsNullOrEmpty(ExpressionText))
                    {
                        // Parsăm expresia pentru a identifica numerele și operatorii
                        UpdateExpressionForNewBase();
                    }

                    // Actualizăm valorile în toate bazele
                    UpdateAllBaseValues();

                    // Save the setting
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

            // Adăugăm comandă pentru activarea/dezactivarea grupării cifrelor
            ToggleDigitGroupingCommand = new RelayCommand(ExecuteToggleDigitGroupingCommand);

            // Clipboard commands
            CutCommand = new RelayCommand(ExecuteCutCommand);
            CopyCommand = new RelayCommand(ExecuteCopyCommand);
            PasteCommand = new RelayCommand(ExecutePasteCommand);

            // Inițializăm valorile în toate bazele
            UpdateAllBaseValues();
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
            if (parameter is not string digit) return;

            // Validate digit based on current base
            if (!IsValidDigitForBase(digit))
            {
                return;
            }

            // Clear expression after equals and for a new calculation
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

                // Parse the initial digit for the current value
                TryParseNumberInBase(digit, out _currentValue);
                UpdateCurrentNumber();
            }
            else
            {
                // Get the current display text without formatting
                string unformattedText = DisplayText;

                // Remove any grouping separator (spaces) for non-decimal bases
                if (CurrentBase != NumberBase.DEC)
                {
                    unformattedText = unformattedText.Replace(" ", "");
                }
                else
                {
                    // Remove any formatting for decimal
                    string groupSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
                    unformattedText = unformattedText.Replace(groupSeparator, "");
                }

                // Append the new digit
                unformattedText += digit;

                // Try to parse as a number in the current base
                if (TryParseNumberInBase(unformattedText, out double value))
                {
                    _currentValue = value;
                    DisplayText = ConvertToBase(value, value);
                    UpdateCurrentNumber();
                }
            }

            // Update values for all bases
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

            // Curățăm numărul de spații și alte caractere posibile
            number = RemoveBasePrefix(number);
            number = number.Replace(" ", "").Replace(",", ".");

            // Verificăm lungimea maximă pentru diferite baze pentru a evita depășirea
            if (!IsStandardMode)
            {
                int maxLength = CurrentBase switch
                {
                    NumberBase.HEX => 16, // 64 bits
                    NumberBase.DEC => 16, // ~64 bits
                    NumberBase.OCT => 22, // ~64 bits
                    NumberBase.BIN => 64, // 64 bits
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
                        // Pentru HEX, verificăm dacă sunt doar caractere valide (0-9, A-F)
                        if (number.Any(c => !((c >= '0' && c <= '9') || (char.ToUpper(c) >= 'A' && char.ToUpper(c) <= 'F'))))
                            return false;

                        // Convertim la long folosind baza 16
                        result = Convert.ToInt64(number, 16);
                        return true;

                    case NumberBase.DEC:
                        // Pentru DEC, folosim double.TryParse pentru a permite și zecimale
                        return double.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out result);

                    case NumberBase.OCT:
                        // Pentru OCT, verificăm dacă sunt doar cifre între 0-7
                        if (number.Any(c => c < '0' || c > '7'))
                            return false;

                        // Convertim la long folosind baza 8
                        result = Convert.ToInt64(number, 8);
                        return true;

                    case NumberBase.BIN:
                        // Pentru BIN, verificăm dacă sunt doar cifre 0 și 1
                        if (number.Any(c => c != '0' && c != '1'))
                            return false;

                        // Convertim la long folosind baza 2
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
            // Dacă suntem în modul Standard, permitem doar cifre DEC
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

            // În modul Programmer, folosim doar partea întreagă
            if (!IsStandardMode)
            {
                value = Math.Round(value);
            }

            // Convert to long for integer operations
            long longValue = (long)value;

            // For Standard DEC mode, let's be consistent with FormatNumber method
            if (IsStandardMode && CurrentBase == NumberBase.DEC)
            {
                // Use our FormatNumber method which already has correct digit grouping logic
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

            // Apply digit grouping if enabled
            if (_useDigitGrouping)
            {
                // For decimal, use the culture's number format
                if (CurrentBase == NumberBase.DEC)
                {
                    if (IsStandardMode)
                    {
                        return value == Math.Floor(value) ? longValue.ToString("N0", CultureInfo.CurrentCulture) : value.ToString("G", CultureInfo.CurrentCulture);
                    }
                    return longValue.ToString("N0", CultureInfo.CurrentCulture);
                }
                // For HEX, group by 4 digits
                else if (CurrentBase == NumberBase.HEX)
                {
                    StringBuilder result = new StringBuilder();
                    int digitCount = 0;

                    // Process digits from right to left
                    for (int i = numberInBase.Length - 1; i >= 0; i--)
                    {
                        result.Insert(0, numberInBase[i]);
                        digitCount++;

                        // Add space after every 4 digits (but not at the beginning)
                        if (digitCount % 4 == 0 && i > 0)
                        {
                            result.Insert(0, ' ');
                        }
                    }

                    return result.ToString();
                }
                // For OCT, group by 3 digits
                else if (CurrentBase == NumberBase.OCT)
                {
                    StringBuilder result = new StringBuilder();
                    int digitCount = 0;

                    // Process digits from right to left
                    for (int i = numberInBase.Length - 1; i >= 0; i--)
                    {
                        result.Insert(0, numberInBase[i]);
                        digitCount++;

                        // Add space after every 3 digits (but not at the beginning)
                        if (digitCount % 3 == 0 && i > 0)
                        {
                            result.Insert(0, ' ');
                        }
                    }

                    return result.ToString();
                }
                // For BIN, group by 4 digits
                else if (CurrentBase == NumberBase.BIN)
                {
                    StringBuilder result = new StringBuilder();
                    int digitCount = 0;

                    // Process digits from right to left
                    for (int i = numberInBase.Length - 1; i >= 0; i--)
                    {
                        result.Insert(0, numberInBase[i]);
                        digitCount++;

                        // Add space after every 4 digits (but not at the beginning)
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

        private void ExecuteOperationCommand(object? parameter)
        {
            if (parameter is not string operation) return;

            // Handle different scenarios differently
            if (!_isNewCalculation && !_isOperationSelected && !string.IsNullOrEmpty(_operation))
            {
                // Case 1: We have a pending operation and now need to calculate the result
                // before starting a new operation (e.g., 2 + 3 × ...)
                double currentNumber = _currentValue;
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

                    // Update display and state
                    _currentValue = _result;
                    _firstNumber = _result;
                    DisplayText = ConvertToBase(_result, _result);

                    // Update the expression
                    if (!string.IsNullOrEmpty(ExpressionText))
                    {
                        if (IsStandardMode)
                        {
                            if (ExpressionText.Contains(" = "))
                            {
                                // Start new expression after equals
                                ExpressionText = $"{FormatNumberForExpression(_firstNumber)} {operation} ";
                            }
                            else if (ExpressionText.EndsWith(" "))
                            {
                                // Add the number and operation
                                ExpressionText = $"{FormatNumberForExpression(_result)} {operation} ";
                            }
                            else
                            {
                                // Add space, number, and operation
                                ExpressionText = $" {FormatNumberForExpression(_result)} {operation} ";
                            }
                        }
                        else
                        {
                            if (ExpressionText.Contains(" = "))
                            {
                                // Start new expression after equals
                                ExpressionText = $"{FormatNumberForExpression(_firstNumber)} {operation} ";
                            }
                            else if (ExpressionText.EndsWith(" "))
                            {
                                // Add the number and operation
                                ExpressionText += $"{FormatNumberForExpression(currentNumber)} {operation} ";
                            }
                            else
                            {
                                // Add space, number, and operation
                                ExpressionText += $" {FormatNumberForExpression(currentNumber)} {operation} ";
                            }
                        }
                    }
                    else
                    {
                        // Start fresh
                        ExpressionText = $"{FormatNumberForExpression(_firstNumber)} {operation} ";
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
                // Case 2: An operation was already selected, replace it with new one
                _operation = operation;

                // Fix the expression text to show the new operation
                if (!string.IsNullOrEmpty(ExpressionText))
                {
                    // Replace the last operation in the expression
                    if (ExpressionText.EndsWith(" + ") || ExpressionText.EndsWith(" - ") ||
                        ExpressionText.EndsWith(" × ") || ExpressionText.EndsWith(" ÷ "))
                    {
                        ExpressionText = ExpressionText.Substring(0, ExpressionText.Length - 3) + $" {operation} ";
                    }
                    else if (ExpressionText.EndsWith(" "))
                    {
                        // If it ends with a space but not a full operation, add the operation
                        ExpressionText += $"{operation} ";
                    }
                    else
                    {
                        // Otherwise create a brand new expression
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
                // Case 3: This is either a new calculation or we're starting from a previous result

                // Store the first number and update state
                if (_isNewCalculation)
                {
                    _firstNumber = _result; // Use previous result
                }
                else
                {
                    _firstNumber = _currentValue; // Use current value
                }

                // Update the expression
                if (!string.IsNullOrEmpty(ExpressionText) && !ExpressionText.Contains(" = "))
                {
                    // Continue existing expression
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
                    // Start a new expression
                    ExpressionText = $"{FormatNumberForExpression(_firstNumber)} {operation} ";
                }

                _operation = operation;
            }

            // Update state for all cases
            _isOperationSelected = true;
            _isNewCalculation = false;
            _hasDecimalPoint = false;
        }

        private void ExecuteEqualsCommand(object? parameter)
        {
            // Folosim valoarea curentă stocată pentru operații
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
                    // Adăugăm rezultatul la expresia existentă
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

                    // În modul Programmer, rotunjim rezultatul la cel mai apropiat număr întreg
                    if (!IsStandardMode)
                    {
                        _result = Math.Round(_result);
                        if (!string.IsNullOrEmpty(ExpressionText) && !ExpressionText.EndsWith(" = "))
                        {
                            // Adăugăm ultimul număr și "=" la expresia existentă
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

            // Actualizăm valorile în toate bazele
            UpdateAllBaseValues();
        }

        private string FormatNumberForExpression(double number)
        {
            // Dacă suntem în modul Programmer, folosim conversia în baza curentă
            if (!IsStandardMode)
            {
                return ConvertToBase(number, number);
            }

            // Pentru modul Standard, formatăm numărul fără zecimale dacă este întreg
            if (number == Math.Floor(number))
            {
                return ((long)number).ToString(CultureInfo.CurrentCulture);
            }

            // Pentru numere cu zecimale, folosim formatul standard
            return number.ToString(CultureInfo.CurrentCulture);
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

            // Resetăm valorile în toate bazele
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
                // Remove formatting for processing
                string unformattedText = DisplayText;

                // Remove spaces for non-decimal bases
                if (CurrentBase != NumberBase.DEC)
                {
                    unformattedText = unformattedText.Replace(" ", "");
                }
                else
                {
                    // Remove grouping for decimal
                    string groupSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
                    unformattedText = unformattedText.Replace(groupSeparator, "");
                }

                // Remove last digit
                string newText = unformattedText.Remove(unformattedText.Length - 1);

                if (newText.Length == 0 || newText == "-")
                {
                    DisplayText = "0";
                    _currentValue = 0;
                }
                else
                {
                    // Reparse the new value
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

            // Actualizăm valorile în toate bazele
            UpdateAllBaseValues();
        }

        private void ExecuteDecimalPointCommand(object? parameter)
        {
            // Punctul decimal este disponibil doar în modul Standard
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

            // If we have a pending operation, preserve it
            if (shouldPreservePendingOperation)
            {
                _currentValue = _result;
                _isNewCalculation = false;
                _firstNumber = savedFirstNumber;
                _operation = savedOperation;
                _isOperationSelected = false; // Allow entering the next number
            }
            else
            {
                _isNewCalculation = true;
                _currentValue = _result;
            }

            _hasDecimalPoint = DisplayText.Contains(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
        }

        private void ExecuteNegateCommand(object? parameter)
        {
            if (DisplayText != "0")
            {
                // Get the current display text without formatting
                string unformattedText = DisplayText;

                // Remove any grouping separator (spaces) for non-decimal bases
                if (CurrentBase != NumberBase.DEC)
                {
                    unformattedText = unformattedText.Replace(" ", "");
                }
                else
                {
                    // Remove any formatting for decimal
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

                // Try to parse as a number in the current base
                if (TryParseNumberInBase(unformattedText, out double value))
                {
                    _currentValue = value;
                    DisplayText = ConvertToBase(value, value);
                    UpdateCurrentNumber();

                    // Actualizăm valorile în toate bazele
                    UpdateAllBaseValues();
                }
            }
        }

        private void ExecutePercentageCommand(object? parameter)
        {
            if (_operation != "" && !_isNewCalculation)
            {
                try
                {
                    // Get the current display text without formatting
                    string unformattedText = DisplayText;

                    // Remove any grouping separator (spaces) for non-decimal bases
                    if (CurrentBase != NumberBase.DEC)
                    {
                        unformattedText = unformattedText.Replace(" ", "");
                    }
                    else
                    {
                        // Remove any formatting for decimal
                        string groupSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
                        unformattedText = unformattedText.Replace(groupSeparator, "");
                    }

                    // Parse the current number
                    if (TryParseNumberInBase(unformattedText, out double currentNumber))
                    {
                        double percentValue = _calculatorModel.Percentage(_firstNumber, currentNumber);

                        // În modul Programmer, rotunjim rezultatul la cel mai apropiat număr întreg
                        if (!IsStandardMode)
                        {
                            percentValue = Math.Round(percentValue);
                        }

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
                // Dacă nu există operație anterioară, resetăm numărul la 0
                _currentValue = 0;
                DisplayText = "0";
                _isNewCalculation = true;
                _hasDecimalPoint = false;
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

                // Format the result properly
                MemoryValues[^1] = FormatNumberForMemory(result.ToString());
                OnPropertyChanged(nameof(IsMemoryEmpty));
            }
        }

        private void ExecuteMemoryStoreCommand(object? parameter)
        {
            // Format the display text properly for memory storage
            string formattedNumber = FormatNumberForMemory(DisplayText);
            MemoryValues.Add(formattedNumber);
            OnPropertyChanged(nameof(IsMemoryEmpty));
        }

        // Helper method to properly format numbers for memory operations
        private string FormatNumberForMemory(string numberText)
        {
            if (double.TryParse(numberText, out double number))
            {
                // Verificam daca numarul este intreg (fara zecimale)
                bool isInteger = number == Math.Floor(number);

                if (isInteger)
                {
                    // Pentru numere intregi, returnam fara zecimale
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
                    // Pentru numere cu zecimale, pastram doar zecimalele semnificative
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

                    return result.TrimEnd('0').TrimEnd('.');
                }
            }

            // In caz ca nu poate fi parsat, returnam textul original
            return numberText;
        }

        //private bool CanExecuteMemoryCommand(object? parameter)
        //{
        //    return _hasMemoryValue;
        //}

        private void ExecuteToggleDigitGroupingCommand(object? parameter)
        {
            UseDigitGrouping = !UseDigitGrouping;
            // SaveSettings() is called in the UseDigitGrouping property setter
        }

        // Clipboard command implementations
        private void ExecuteCutCommand(object? parameter)
        {
            // Save the current display text to clipboard
            Clipboard.SetText(DisplayText);

            // Clear the display
            _currentValue = 0;
            DisplayText = "0";
            _isNewCalculation = true;
            _hasDecimalPoint = false;
            UpdateCurrentNumber();
        }

        private void ExecuteCopyCommand(object? parameter)
        {
            // Copy the current display text to clipboard
            Clipboard.SetText(DisplayText);
        }

        private void ExecutePasteCommand(object? parameter)
        {
            try
            {
                // Get text from clipboard
                if (Clipboard.ContainsText())
                {
                    string clipboardText = Clipboard.GetText().Trim();

                    // Eliminăm caracterele non-numerice și prefixele bazelor, păstrând doar ce este valid
                    string cleanedText = CleanupPastedText(clipboardText);

                    // Verificăm dacă valoarea curățată este goală sau diferită semnificativ de valoarea originală
                    if (string.IsNullOrEmpty(cleanedText) ||
                        (clipboardText.Length > 0 && cleanedText.Length < clipboardText.Length / 2))
                    {
                        // Afișăm mesajul de eroare
                        DisplayText = "Invalid input";
                        _isNewCalculation = true;
                        return;
                    }

                    // Încearcă să analizeze textul ca un număr valid
                    if (TryParseNumberInBase(cleanedText, out double pastedValue))
                    {
                        // Reset current state
                        _currentValue = pastedValue;
                        DisplayText = ConvertToBase(pastedValue, pastedValue);
                        _isNewCalculation = false;

                        // Setăm hasDecimalPoint doar dacă suntem în modul Standard și numărul are zecimale
                        _hasDecimalPoint = IsStandardMode && cleanedText.Contains('.');

                        UpdateCurrentNumber();
                        UpdateAllBaseValues();
                    }
                    else
                    {
                        // Nu s-a putut parsa numărul - afișăm eroare
                        DisplayText = "Invalid input";
                        _isNewCalculation = true;
                    }
                }
            }
            catch (Exception)
            {
                // În caz de eroare, afișăm un mesaj de eroare
                DisplayText = "Invalid input";
                _isNewCalculation = true;
            }
        }

        // Metodă ajutătoare pentru curățarea textului copiat
        private string CleanupPastedText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Eliminăm prefixele comune ale bazelor numerice (0x, 0b, etc.)
            text = RemoveCommonPrefixes(text);

            // Elimină caracterele albe și formatarea
            text = text.Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\n", "");

            // În modul Standard, acceptăm doar numere zecimale
            if (IsStandardMode)
            {
                // Normalizăm separatorul de zecimale
                text = text.Replace(",", ".");

                // Pentru DEC, păstrăm doar cifrele, . și -, eliminând alte caractere
                StringBuilder sb = new StringBuilder();
                bool hasDot = false;

                if (text.StartsWith("-"))
                {
                    sb.Append("-");
                    text = text.Substring(1);
                }

                foreach (char c in text)
                {
                    if (char.IsDigit(c))
                    {
                        sb.Append(c);
                    }
                    else if (c == '.' && !hasDot)
                    {
                        sb.Append(c);
                        hasDot = true;
                    }
                }

                return sb.ToString();
            }
            else
            {
                // În modul Programmer, curățăm textul în funcție de baza actuală
                switch (CurrentBase)
                {
                    case NumberBase.HEX:
                        // Pentru HEX, păstrăm doar 0-9 și A-F (case insensitive)
                        var hexChars = text.Where(c =>
                            (c >= '0' && c <= '9') ||
                            (char.ToUpper(c) >= 'A' && char.ToUpper(c) <= 'F')).Select(c => char.ToUpper(c)).ToArray();

                        // Verificăm dacă textul original conținea litere A-F (majuscule sau minuscule)
                        bool containsHexLetters = text.Any(c =>
                            (char.ToUpper(c) >= 'A' && char.ToUpper(c) <= 'F'));

                        // Dacă textul original nu conținea litere hex, dar are alte litere, este probabil invalid
                        if (!containsHexLetters && text.Any(char.IsLetter))
                        {
                            // Returnăm un șir gol pentru a semnala că textul nu este potrivit pentru HEX
                            return string.Empty;
                        }

                        return new string(hexChars);

                    case NumberBase.DEC:
                        // Pentru DEC, păstrăm doar cifrele 0-9
                        var decChars = text.Where(c => c >= '0' && c <= '9').ToArray();

                        // Dacă textul conține litere, este probabil invalid pentru DEC
                        if (text.Any(char.IsLetter))
                        {
                            return string.Empty;
                        }

                        return new string(decChars);

                    case NumberBase.OCT:
                        // Pentru OCT, păstrăm doar cifrele 0-7
                        var octChars = text.Where(c => c >= '0' && c <= '7').ToArray();

                        // Dacă textul conține litere sau cifre > 7, este probabil invalid pentru OCT
                        if (text.Any(char.IsLetter) || text.Any(c => c > '7' && char.IsDigit(c)))
                        {
                            return string.Empty;
                        }

                        return new string(octChars);

                    case NumberBase.BIN:
                        // Pentru BIN, păstrăm doar 0 și 1
                        var binChars = text.Where(c => c == '0' || c == '1').ToArray();

                        // Dacă textul conține litere sau cifre > 1, este probabil invalid pentru BIN
                        if (text.Any(char.IsLetter) || text.Any(c => c > '1' && char.IsDigit(c)))
                        {
                            return string.Empty;
                        }

                        return new string(binChars);

                    default:
                        return string.Empty;
                }
            }
        }

        // Metodă pentru a elimina prefixele comune ale bazelor numerice
        private string RemoveCommonPrefixes(string text)
        {
            // Eliminăm prefixele standard pentru bazele numerice
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ||
                text.StartsWith("&h", StringComparison.OrdinalIgnoreCase) ||
                text.StartsWith("#", StringComparison.OrdinalIgnoreCase))
            {
                // Prefix hexazecimal (C/C++, VB, etc.)
                return text.Substring(2);
            }
            else if (text.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
            {
                // Prefix binar
                return text.Substring(2);
            }
            else if (text.StartsWith("0o", StringComparison.OrdinalIgnoreCase) ||
                     text.StartsWith("0", StringComparison.OrdinalIgnoreCase) && text.Length > 1 && text[1] >= '0' && text[1] <= '7')
            {
                // Prefix octal
                return text.Substring(text.StartsWith("0o") ? 2 : 1);
            }

            return text;
        }

        // Helper methods
        private void UpdateCurrentNumber()
        {
            // Folosim valoarea curentă stocată
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
            // Get the current culture's number format
            NumberFormatInfo numberFormat = CultureInfo.CurrentCulture.NumberFormat;

            // Dacă gruparea cifrelor este dezactivată, folosim un format simplu
            if (!_useDigitGrouping)
            {
                if (number == Math.Floor(number))
                {
                    return ((long)number).ToString(CultureInfo.CurrentCulture);
                }
                return number.ToString("G", CultureInfo.CurrentCulture);
            }

            // For whole numbers, format with no decimal part but with digit grouping
            if (number == Math.Floor(number))
            {
                // Format the number with thousand separators but no decimal places
                return ((long)number).ToString("N0", numberFormat);
            }

            // For decimal numbers, include the decimal part without trailing zeros
            return number.ToString("G", numberFormat);
        }

        // Metodă pentru actualizarea ExpressionText când se schimbă baza
        private void UpdateExpressionForNewBase()
        {
            if (string.IsNullOrEmpty(ExpressionText))
                return;

            // Nu actualizăm expresii care conțin funcții speciale
            if (ExpressionText.Contains("(") && ExpressionText.Contains(")"))
                return;

            // Verificăm dacă expresia conține un rezultat final
            bool hasResult = ExpressionText.EndsWith(" = ");

            if (hasResult && _isNewCalculation)
            {
                // Dacă este un rezultat final, actualizăm doar afișarea rezultatului
                ExpressionText = $"{FormatNumberForExpression(_result)} = ";
                return;
            }

            // Încercăm să reconstruim expresia cu numerele convertite în noua bază
            if (_operation != "" && !_isNewCalculation)
            {
                // Expresie în desfășurare cu o operație
                ExpressionText = $"{FormatNumberForExpression(_firstNumber)} {_operation}";
                return;
            }
            else if (_operation != "" && _isOperationSelected)
            {
                // Operator selectat, dar nu a fost introdus al doilea număr
                ExpressionText = $"{FormatNumberForExpression(_firstNumber)} {_operation}";
                return;
            }
            else if (hasResult)
            {
                // Expresie completă cu rezultat
                ExpressionText = $"{FormatNumberForExpression(_firstNumber)} {_operation} {FormatNumberForExpression(_secondNumber)} = ";
                return;
            }
            else if (ExpressionText.Contains("+") || ExpressionText.Contains("-") ||
                    ExpressionText.Contains("×") || ExpressionText.Contains("÷"))
            {
                // Expresie complexă cu mai multe operații - o reconstruim pe cât posibil
                string[] parts = ExpressionText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    StringBuilder newExpression = new StringBuilder();

                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (parts[i] == "+" || parts[i] == "-" || parts[i] == "×" || parts[i] == "÷" || parts[i] == "=")
                        {
                            // Adăugăm operatorul așa cum este
                            newExpression.Append($" {parts[i]} ");
                        }
                        else if (parts[i].EndsWith("%"))
                        {
                            // Tratăm cazul special pentru procente
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
                            // Convertim numărul în noua bază
                            newExpression.Append(FormatNumberForExpression(num));
                        }
                        else
                        {
                            // Păstrăm partea așa cum este dacă nu e număr
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