using System;

namespace WindowsCalculator.Models
{
    public class CalculatorModel
    {
        public double Add(double a, double b) => a + b;

        public double Subtract(double a, double b) => a - b;

        public double Multiply(double a, double b) => a * b;

        public double Divide(double a, double b) => a / b;

        public double Percentage(double a, double b) => a * (b / 100);

        public double SquareRoot(double a) => Math.Sqrt(a);

        public double Square(double a) => a * a;

        public double Reciprocal(double a) => 1 / a;
    }
}