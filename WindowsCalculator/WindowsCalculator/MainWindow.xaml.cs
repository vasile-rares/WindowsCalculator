using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsCalculator.ViewModels;

namespace WindowsCalculator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private bool _isMenuOpen = false;
    private readonly CalculatorViewModel _viewModel;
    private readonly StandardCalculatorView _standardCalculatorView;
    private readonly ProgrammerCalculatorView _programmerCalculatorView;

    public MainWindow()
    {
        // Creăm ViewModel-ul înainte de inițializarea componentelor
        _viewModel = new CalculatorViewModel();
        
        // Register for settings saved events
        _viewModel.SettingsSaved += ViewModel_SettingsSaved;
        
        // DataContext-ul este setat după ce s-au încărcat setările, astfel încât să nu se suprascrie
        DataContext = _viewModel;

        InitializeComponent();

        // Initialize calculator views și le setăm același ViewModel
        _standardCalculatorView = new StandardCalculatorView { DataContext = _viewModel };
        _programmerCalculatorView = new ProgrammerCalculatorView { DataContext = _viewModel };

        // Set the correct view based on the loaded settings
        if (_viewModel.IsStandardMode)
        {
            CalculatorViewContent.Content = _standardCalculatorView;
        }
        else
        {
            CalculatorViewContent.Content = _programmerCalculatorView;
        }

        // Adăugăm un handler pentru evenimentele de tastatură
        this.KeyDown += MainWindow_KeyDown;
    }

    // Handle settings saved events
    private void ViewModel_SettingsSaved(object sender, EventArgs e)
    {
        // Show settings saved animation
        ShowSettingsSavedAnimation();
    }

    // Show a visual indication that settings were saved
    private void ShowSettingsSavedAnimation()
    {
        // Create show animation
        var fadeInAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(300)
        };

        // Create hide animation
        var fadeOutAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            BeginTime = TimeSpan.FromSeconds(1.5), // Wait before fading out
            Duration = TimeSpan.FromMilliseconds(300)
        };

        // Create storyboard
        var storyboard = new Storyboard();
        storyboard.Children.Add(fadeInAnimation);
        storyboard.Children.Add(fadeOutAnimation);
        
        // Set the target
        Storyboard.SetTarget(fadeInAnimation, SettingsStatusText);
        Storyboard.SetTarget(fadeOutAnimation, SettingsStatusText);
        
        // Set the target property
        Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(UIElement.OpacityProperty));
        Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(UIElement.OpacityProperty));
        
        // Start the animation
        storyboard.Begin();
    }

    // Handler pentru evenimentele de tastatură
    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        // Check if About dialog is open and Escape key is pressed
        if (e.Key == Key.Escape && AboutOverlay.Visibility == Visibility.Visible)
        {
            CloseAboutButton_Click(this, new RoutedEventArgs());
            e.Handled = true;
            return;
        }
        
        // Verificăm scurtăturile pentru clipboard
        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
        {
            if (e.Key == Key.X) // Cut (Ctrl+X)
            {
                if (_viewModel.CutCommand.CanExecute(null))
                {
                    _viewModel.CutCommand.Execute(null);
                    e.Handled = true;
                    return;
                }
            }
            else if (e.Key == Key.C) // Copy (Ctrl+C)
            {
                if (_viewModel.CopyCommand.CanExecute(null))
                {
                    _viewModel.CopyCommand.Execute(null);
                    e.Handled = true;
                    return;
                }
            }
            else if (e.Key == Key.V) // Paste (Ctrl+V)
            {
                if (_viewModel.PasteCommand.CanExecute(null))
                {
                    _viewModel.PasteCommand.Execute(null);
                    e.Handled = true;
                    return;
                }
            }
        }
        
        // Restul codului pentru alte taste
        if (e.Key == Key.Enter || e.Key == Key.Return)
        {
            // Executăm comanda Equal la apăsarea tastei ENTER
            if (_viewModel.EqualsCommand.CanExecute(null))
            {
                _viewModel.EqualsCommand.Execute(null);
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Escape)
        {
            // Executăm comanda Clear la apăsarea tastei ESC
            if (_viewModel.ClearCommand.CanExecute(null))
            {
                _viewModel.ClearCommand.Execute(null);
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Back || e.Key == Key.Delete)
        {
            // Executăm comanda Backspace la apăsarea tastei Backspace sau Delete
            if (_viewModel.BackspaceCommand.CanExecute(null))
            {
                _viewModel.BackspaceCommand.Execute(null);
                e.Handled = true;
            }
        }
        else if (e.Key >= Key.D0 && e.Key <= Key.D9 && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
        {
            // Cifre numerice (0-9) de pe rândul de sus al tastaturii
            string digit = (e.Key - Key.D0).ToString();
            if (_viewModel.NumberCommand.CanExecute(digit))
            {
                _viewModel.NumberCommand.Execute(digit);
                e.Handled = true;
            }
        }
        else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
        {
            // Cifre numerice (0-9) de pe numpad
            string digit = (e.Key - Key.NumPad0).ToString();
            if (_viewModel.NumberCommand.CanExecute(digit))
            {
                _viewModel.NumberCommand.Execute(digit);
                e.Handled = true;
            }
        }
        else if (!_viewModel.IsStandardMode && (
            (e.Key >= Key.A && e.Key <= Key.F))) // Literele A-F mari
        {
            // Cifre hexazecimale (A-F) pentru modul Programmer
            string digit = e.Key.ToString();
            
            if (_viewModel.NumberCommand.CanExecute(digit))
            {
                _viewModel.NumberCommand.Execute(digit);
                e.Handled = true;
            }
        }
        // Gestionăm literele mici a-f pentru hexazecimal (fără a folosi Key.a care nu există)
        else if (!_viewModel.IsStandardMode && e.Key >= Key.A && e.Key <= Key.Z)
        {
            // Obținem caracterul de la tastă
            char keyChar = (char)KeyInterop.VirtualKeyFromKey(e.Key);
            
            // Verificăm dacă este între 'a' și 'f' (case insensitive)
            if (char.ToUpper(keyChar) >= 'A' && char.ToUpper(keyChar) <= 'F')
            {
                // Convertim întotdeauna la majusculă pentru a fi acceptată de calculator
                string digit = char.ToUpper(keyChar).ToString();
                
                if (_viewModel.NumberCommand.CanExecute(digit))
                {
                    _viewModel.NumberCommand.Execute(digit);
                    e.Handled = true;
                }
            }
        }
        else if (e.Key == Key.Add || 
                (e.Key == Key.OemPlus && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))) ||
                (e.Key == Key.OemPlus && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift)))
        {
            // Operator adunare (+): NumPad+, Shift++ sau + (pe unele tastaturi)
            if (_viewModel.OperationCommand.CanExecute("+"))
            {
                _viewModel.OperationCommand.Execute("+");
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
        {
            // Operator scădere (-): NumPad- sau -
            if (_viewModel.OperationCommand.CanExecute("-"))
            {
                _viewModel.OperationCommand.Execute("-");
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Multiply || 
                (e.Key == Key.D8 && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))) ||
                (e.Key == Key.Oem8)) // Depinde de configurația tastaturii
        {
            // Operator înmulțire (×): NumPad* sau Shift+8
            if (_viewModel.OperationCommand.CanExecute("×"))
            {
                _viewModel.OperationCommand.Execute("×");
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Divide || e.Key == Key.OemQuestion || e.Key == Key.Oem2 ||
                (e.Key == Key.D7 && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))) // Characterul '/' de pe tastatura românească
        {
            // Operator împărțire (÷): NumPad/ sau Shift+7 (pe unele tastaturi)
            if (_viewModel.OperationCommand.CanExecute("÷"))
            {
                _viewModel.OperationCommand.Execute("÷");
                e.Handled = true;
            }
        }
        else if (
                (e.Key == Key.D5 && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))))
        {
            // Operator procent (%): Shift+5
            if (_viewModel.PercentageCommand.CanExecute(null))
            {
                _viewModel.PercentageCommand.Execute(null);
                e.Handled = true;
            }
        }
    }

    // Event handler for dragging the window from the custom title bar
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            DragMove();
        }
    }

    // Event handler for minimize button
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    // Event handler for close button
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    // Cleanup when window is closed
    protected override void OnClosed(EventArgs e)
    {
        // Unregister from events to prevent memory leaks
        if (_viewModel != null)
        {
            _viewModel.SettingsSaved -= ViewModel_SettingsSaved;
        }
        
        base.OnClosed(e);
    }

    // Event handler for hamburger menu button
    private void HamburgerButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isMenuOpen)
        {
            // Close menu
            CloseMenu();
        }
        else
        {
            // Open menu
            OpenMenu();
        }
    }

    private void OpenMenu()
    {
        // Create animation to slide the menu in from left to right
        var animation = new DoubleAnimation
        {
            From = -255,
            To = 0, // Width of the menu
            Duration = TimeSpan.FromMilliseconds(150),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        // Apply animation to the TranslateTransform
        var translateTransform = (TranslateTransform)HamburgerMenuPanel.RenderTransform;
        translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);

        _isMenuOpen = true;
    }

    private void CloseMenu()
    {
        // Create animation to slide the menu back left
        var animation = new DoubleAnimation
        {
            From = 0,
            To = -255,
            Duration = TimeSpan.FromMilliseconds(150),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        // Apply animation to the TranslateTransform
        var translateTransform = (TranslateTransform)HamburgerMenuPanel.RenderTransform;
        translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);

        _isMenuOpen = false;
    }

    // Switch to the Standard calculator view
    private void StandardButton_Click(object sender, RoutedEventArgs e)
    {
        // Ensure the calculator is in DEC mode when switching to Standard calculator
        _viewModel.IsStandardMode = true;
        _viewModel.CurrentBase = CalculatorViewModel.NumberBase.DEC;
        
        // Reset calculator values
        _viewModel.ClearCommand.Execute(null);
        
        CalculatorViewContent.Content = _standardCalculatorView;
        CloseMenu();
        
        // Setăm focusul pe calculator
        _standardCalculatorView.Focus();
    }

    // Switch to the Programmer calculator view
    private void ProgrammerButton_Click(object sender, RoutedEventArgs e)
    {
        // Set programmer mode
        _viewModel.IsStandardMode = false;
        
        // Reset calculator values
        _viewModel.ClearCommand.Execute(null);
        
        CalculatorViewContent.Content = _programmerCalculatorView;
        CloseMenu();
        
        // Setăm focusul pe calculator
        _programmerCalculatorView.Focus();
    }
    
    // Clipboard operations
    private void CutButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.CutCommand.CanExecute(null))
        {
            _viewModel.CutCommand.Execute(null);
        }
        CloseMenu();
    }
    
    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.CopyCommand.CanExecute(null))
        {
            _viewModel.CopyCommand.Execute(null);
        }
        CloseMenu();
    }
    
    private void PasteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.PasteCommand.CanExecute(null))
        {
            _viewModel.PasteCommand.Execute(null);
        }
        CloseMenu();
    }
    
    // About dialog handlers
    private void AboutButton_Click(object sender, RoutedEventArgs e)
    {
        // Show the About overlay
        AboutOverlay.Visibility = Visibility.Visible;
        
        // Close the menu
        CloseMenu();
        
        // Add fade-in animation
        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(200)
        };
        AboutOverlay.BeginAnimation(UIElement.OpacityProperty, fadeIn);
    }
    
    private void CloseAboutButton_Click(object sender, RoutedEventArgs e)
    {
        // Add fade-out animation
        var fadeOut = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(200)
        };
        
        // Hide the overlay when animation completes
        fadeOut.Completed += (s, args) => AboutOverlay.Visibility = Visibility.Collapsed;
        
        AboutOverlay.BeginAnimation(UIElement.OpacityProperty, fadeOut);
    }

    private void AboutOverlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Close the About dialog when clicking on the overlay background
        CloseAboutButton_Click(this, new RoutedEventArgs());
    }
    
    private void AboutDialog_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Stop event propagation to prevent closing when clicking on the dialog itself
        e.Handled = true;
    }
}