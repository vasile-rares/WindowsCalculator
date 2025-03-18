using System;

namespace WindowsCalculator.Models
{
    public class CalculatorModel
    {
        public double Add(double a, double b) => a + b;
        
        public double Subtract(double a, double b) => a - b;
        
        public double Multiply(double a, double b) => a * b;
        
        public double Divide(double a, double b)
        {
            if (b == 0)
                throw new DivideByZeroException("Cannot divide by zero.");
            return a / b;
        }
        
        public double Percentage(double a, double b) => a * (b / 100);
        
        public double SquareRoot(double a)
        {
            if (a < 0)
                throw new ArgumentException("Cannot compute square root of a negative number.");
            return Math.Sqrt(a);
        }
        
        public double Square(double a) => a * a;
        
        public double Reciprocal(double a)
        {
            if (a == 0)
                throw new DivideByZeroException("Cannot compute reciprocal of zero.");
            return 1 / a;
        }
    }
}