using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WindowsCalculator
{
    /// <summary>
    /// Interaction logic for StandardCalculatorView.xaml
    /// </summary>
    public partial class StandardCalculatorView : UserControl
    {
        public StandardCalculatorView()
        {
            InitializeComponent();
        }

        private void MemoryListButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle the popup visibility - shows the popup from bottom to top
            if (MemoryPopup != null)
            {
                MemoryPopup.IsOpen = !MemoryPopup.IsOpen;
            }
        }
    }
}