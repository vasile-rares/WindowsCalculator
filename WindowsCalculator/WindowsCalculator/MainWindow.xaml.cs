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

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new CalculatorViewModel();
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
            From = -205,
            To = 0, // Width of the menu
            Duration = TimeSpan.FromMilliseconds(250),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        // Apply animation to the TranslateTransform
        var translateTransform = (TranslateTransform)HamburgerMenuPanel.RenderTransform;
        translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);

        _isMenuOpen = true;

        // Ensure hamburger button stays on top
        HamburgerButton.SetValue(Panel.ZIndexProperty, 100);
    }

    private void CloseMenu()
    {
        // Create animation to slide the menu back left
        var animation = new DoubleAnimation
        {
            From = 0,
            To = -205,
            Duration = TimeSpan.FromMilliseconds(250),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        // Apply animation to the TranslateTransform
        var translateTransform = (TranslateTransform)HamburgerMenuPanel.RenderTransform;
        translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);

        _isMenuOpen = false;
    }
}