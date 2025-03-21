using System.Windows.Controls;
using System.Windows.Input;

namespace WindowsCalculator
{
    /// <summary>
    /// Interaction logic for ProgrammerCalculatorView.xaml
    /// </summary>
    public partial class ProgrammerCalculatorView : UserControl
    {
        public ProgrammerCalculatorView()
        {
            InitializeComponent();

            // Asiguram focusul pe control
            this.Loaded += (s, e) => Keyboard.Focus(this);
            this.GotFocus += (s, e) => e.Handled = true;
        }
    }
}