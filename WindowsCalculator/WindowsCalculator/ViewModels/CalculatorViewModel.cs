using System;
using System.ComponentModel;
using System.Windows.Input;
using WindowsCalculator.Commands;
using WindowsCalculator.Models;

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

        // Memory functionality
        private double _memoryValue = 0;
        private bool _hasMemoryValue = false;

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
            MemoryClearCommand = new RelayCommand(ExecuteMemoryClearCommand, CanExecuteMemoryCommand);
            MemoryRecallCommand = new RelayCommand(ExecuteMemoryRecallCommand, CanExecuteMemoryCommand);
            MemoryAddCommand = new RelayCommand(ExecuteMemoryAddCommand);
            MemorySubtractCommand = new RelayCommand(ExecuteMemorySubtractCommand);
            MemoryStoreCommand = new RelayCommand(ExecuteMemoryStoreCommand);
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
                DisplayText += digit;
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
            double currentNumber = double.Parse(DisplayText);
            
            if (_isOperationSelected)
            {
                _secondNumber = currentNumber;
            }
            
            if (_operation == "")
            {
                _result = currentNumber;
                ExpressionText = $"{FormatNumber(_result)} =";
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
                            _result = _calculatorModel.Divide(_firstNumber, currentNumber);
                            break;
                    }
                    
                    ExpressionText = $"{FormatNumber(_firstNumber)} {_operation} {FormatNumber(currentNumber)} =";
                    DisplayText = FormatNumber(_result);
                }
                catch (Exception ex)
                {
                    DisplayText = ex.Message;
                    _isNewCalculation = true;
                    return;
                }
            }
            
            _isNewCalculation = true;
            _hasDecimalPoint = DisplayText.Contains(".");
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
                if (removedChar == '.')
                {
                    _hasDecimalPoint = false;
                }
                DisplayText = DisplayText.Remove(DisplayText.Length - 1);
            }
            
            UpdateCurrentNumber();
        }

        private void ExecuteDecimalPointCommand(object? parameter)
        {
            if (_isNewCalculation || _isOperationSelected)
            {
                DisplayText = "0.";
                _isNewCalculation = false;
                _isOperationSelected = false;
                _hasDecimalPoint = true;
            }
            else if (!_hasDecimalPoint)
            {
                DisplayText += ".";
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
                        _result = _calculatorModel.Reciprocal(currentNumber);
                        ExpressionText = $"1/({FormatNumber(currentNumber)})";
                        break;
                    case "x²":
                        _result = _calculatorModel.Square(currentNumber);
                        ExpressionText = $"sqr({FormatNumber(currentNumber)})";
                        break;
                    case "√":
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
            _hasDecimalPoint = DisplayText.Contains(".");
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
            _memoryValue = 0;
            _hasMemoryValue = false;
            OnPropertyChanged(nameof(MemoryClearCommand));
            OnPropertyChanged(nameof(MemoryRecallCommand));
        }

        private void ExecuteMemoryRecallCommand(object? parameter)
        {
            if (_hasMemoryValue)
            {
                DisplayText = FormatNumber(_memoryValue);
                if (_isOperationSelected)
                {
                    _secondNumber = _memoryValue;
                    _isOperationSelected = false;
                }
                else
                {
                    _firstNumber = _memoryValue;
                }
                _hasDecimalPoint = DisplayText.Contains(".");
            }
        }

        private void ExecuteMemoryAddCommand(object? parameter)
        {
            double currentNumber = double.Parse(DisplayText);
            _memoryValue += currentNumber;
            _hasMemoryValue = true;
            
            OnPropertyChanged(nameof(MemoryClearCommand));
            OnPropertyChanged(nameof(MemoryRecallCommand));
            
            _isNewCalculation = true;
        }

        private void ExecuteMemorySubtractCommand(object? parameter)
        {
            double currentNumber = double.Parse(DisplayText);
            _memoryValue -= currentNumber;
            _hasMemoryValue = true;
            
            OnPropertyChanged(nameof(MemoryClearCommand));
            OnPropertyChanged(nameof(MemoryRecallCommand));
            
            _isNewCalculation = true;
        }

        private void ExecuteMemoryStoreCommand(object? parameter)
        {
            _memoryValue = double.Parse(DisplayText);
            _hasMemoryValue = true;
            
            OnPropertyChanged(nameof(MemoryClearCommand));
            OnPropertyChanged(nameof(MemoryRecallCommand));
            
            _isNewCalculation = true;
        }

        private bool CanExecuteMemoryCommand(object? parameter)
        {
            return _hasMemoryValue;
        }

        // Helper methods
        private void UpdateCurrentNumber()
        {
            double currentValue = double.Parse(DisplayText);
            
            if (_isOperationSelected || (_operation != "" && !_isNewCalculation))
            {
                _secondNumber = currentValue;
            }
            else
            {
                _firstNumber = currentValue;
            }
        }

        private string FormatNumber(double number)
        {
            // Remove trailing zeros for whole numbers
            if (number == (int)number)
            {
                return ((int)number).ToString();
            }
            
            return number.ToString();
        }
    }
}