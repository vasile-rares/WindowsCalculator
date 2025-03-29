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
        _viewModel = new CalculatorViewModel();
        _viewModel.SettingsSaved += ViewModel_SettingsSaved;

        DataContext = _viewModel;

        InitializeComponent();

        _standardCalculatorView = new StandardCalculatorView { DataContext = _viewModel };
        _programmerCalculatorView = new ProgrammerCalculatorView { DataContext = _viewModel };

        if (_viewModel.IsStandardMode)
        {
            CalculatorViewContent.Content = _standardCalculatorView;
        }
        else
        {
            CalculatorViewContent.Content = _programmerCalculatorView;
        }

        // Event Handler
        this.KeyDown += MainWindow_KeyDown;
    }

    // Handle settings saved events
    private void ViewModel_SettingsSaved(object sender, EventArgs e)
    {
        ShowSettingsSavedAnimation();
    }

    private void ShowSettingsSavedAnimation()
    {
        var fadeInAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(300)
        };

        var fadeOutAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            BeginTime = TimeSpan.FromSeconds(1.5), // Wait before fading out
            Duration = TimeSpan.FromMilliseconds(300)
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(fadeInAnimation);
        storyboard.Children.Add(fadeOutAnimation);
        Storyboard.SetTarget(fadeInAnimation, SettingsStatusText);
        Storyboard.SetTarget(fadeOutAnimation, SettingsStatusText);
        Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(UIElement.OpacityProperty));
        Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Begin();
    }

    // Handler pentru evenimentele tastatura
    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && AboutOverlay.Visibility == Visibility.Visible)
        {
            CloseAboutButton_Click(this, new RoutedEventArgs());
            e.Handled = true;
            return;
        }

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

        if (e.Key == Key.Enter || e.Key == Key.Return)
        {
            if (_viewModel.EqualsCommand.CanExecute(null))
            {
                _viewModel.EqualsCommand.Execute(null);
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Escape)
        {
            if (_viewModel.ClearCommand.CanExecute(null))
            {
                _viewModel.ClearCommand.Execute(null);
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Back || e.Key == Key.Delete)
        {
            if (_viewModel.BackspaceCommand.CanExecute(null))
            {
                _viewModel.BackspaceCommand.Execute(null);
                e.Handled = true;
            }
        }
        else if (e.Key >= Key.D0 && e.Key <= Key.D9 && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
        {
            string digit = (e.Key - Key.D0).ToString();
            if (_viewModel.NumberCommand.CanExecute(digit))
            {
                _viewModel.NumberCommand.Execute(digit);
                e.Handled = true;
            }
        }
        else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
        {
            string digit = (e.Key - Key.NumPad0).ToString();
            if (_viewModel.NumberCommand.CanExecute(digit))
            {
                _viewModel.NumberCommand.Execute(digit);
                e.Handled = true;
            }
        }
        else if (!_viewModel.IsStandardMode && ((e.Key >= Key.A && e.Key <= Key.F)))
        {
            string digit = e.Key.ToString();

            if (_viewModel.NumberCommand.CanExecute(digit))
            {
                _viewModel.NumberCommand.Execute(digit);
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Add ||
                (e.Key == Key.OemPlus && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))) ||
                (e.Key == Key.OemPlus && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift)))
        {
            // Operator adunare (+): NumPad+, Shift++ sau +
            if (_viewModel.OperationCommand.CanExecute("+"))
            {
                _viewModel.OperationCommand.Execute("+");
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
        {
            // Operator scadere (-): NumPad- sau -
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
            // Operator inmultire (×): NumPad* sau Shift+8
            if (_viewModel.OperationCommand.CanExecute("×"))
            {
                _viewModel.OperationCommand.Execute("×");
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Divide || e.Key == Key.OemQuestion || e.Key == Key.Oem2 ||
                (e.Key == Key.D7 && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))) // Characterul '/' de pe tastatura românească
        {
            // Operator impartire (÷): NumPad/ sau Shift+7
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

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            DragMove();
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnClosed(EventArgs e) // Pentru memory leaks
    {
        if (_viewModel != null)
        {
            _viewModel.SettingsSaved -= ViewModel_SettingsSaved;
        }

        base.OnClosed(e);
    }

    private void HamburgerButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isMenuOpen)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }

    private void OpenMenu()
    {
        var animation = new DoubleAnimation
        {
            From = -255,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(150),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var translateTransform = (TranslateTransform)HamburgerMenuPanel.RenderTransform;
        translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);

        _isMenuOpen = true;
    }

    private void CloseMenu()
    {
        var animation = new DoubleAnimation
        {
            From = 0,
            To = -255,
            Duration = TimeSpan.FromMilliseconds(150),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        var translateTransform = (TranslateTransform)HamburgerMenuPanel.RenderTransform;
        translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);

        _isMenuOpen = false;
    }

    private void StandardButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.IsStandardMode = true;
        _viewModel.CurrentBase = CalculatorViewModel.NumberBase.DEC;

        _viewModel.ClearCommand.Execute(null);

        CalculatorViewContent.Content = _standardCalculatorView;
        CloseMenu();

        _standardCalculatorView.Focus();
    }

    private void ProgrammerButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.IsStandardMode = false;
        _viewModel.ClearCommand.Execute(null);

        CalculatorViewContent.Content = _programmerCalculatorView;
        CloseMenu();

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

    // About handlers
    private void AboutButton_Click(object sender, RoutedEventArgs e)
    {
        AboutOverlay.Visibility = Visibility.Visible;

        CloseMenu();

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
        var fadeOut = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(200)
        };

        fadeOut.Completed += (s, args) => AboutOverlay.Visibility = Visibility.Collapsed;

        AboutOverlay.BeginAnimation(UIElement.OpacityProperty, fadeOut);
    }

    private void AboutOverlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        CloseAboutButton_Click(this, new RoutedEventArgs());
    }

    private void AboutDialog_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }
}