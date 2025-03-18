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
    private bool isMenuOpen = false;

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

    // Eveniment pentru butonul hamburger
    private void HamburgerButton_Click(object sender, RoutedEventArgs e)
    {
        if (!isMenuOpen)
            OpenMenu();
        else
            CloseMenu();
    }

    // Eveniment pentru închiderea meniului când se apasă pe overlay
    private void CloseMenu_Click(object sender, MouseButtonEventArgs e)
    {
        CloseMenu();
    }

    // Deschide meniul lateral cu animație
    private void OpenMenu()
    {
        // Creează animația pentru meniu
        DoubleAnimation menuAnimation = new DoubleAnimation
        {
            From = 0,
            To = 205, // Lățimea meniului
            Duration = TimeSpan.FromSeconds(0.3)
        };

        SideMenuTransform.BeginAnimation(TranslateTransform.XProperty, menuAnimation);

        isMenuOpen = true;
    }

    // Închide meniul lateral cu animație
    private void CloseMenu()
    {
        // Creează animația pentru meniu
        DoubleAnimation menuAnimation = new DoubleAnimation
        {
            From = 205, // Lățimea meniului
            To = 0,
            Duration = TimeSpan.FromSeconds(0.3)
        };

        SideMenuTransform.BeginAnimation(TranslateTransform.XProperty, menuAnimation);

        isMenuOpen = false;
    }
}