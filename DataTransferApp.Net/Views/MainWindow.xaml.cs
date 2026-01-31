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
using DataTransferApp.Net.Services;
using DataTransferApp.Net.ViewModels;

namespace DataTransferApp.Net.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        LoggingService.Info("MainWindow constructor called");
        InitializeComponent();
        this.Loaded += MainWindow_Loaded;

        // Apply window startup mode based on setting
        var startupMode = App.Settings?.WindowStartupMode ?? "Normal";
        switch (startupMode)
        {
            case "Maximized":
                WindowState = WindowState.Maximized;
                break;
            case "Fullscreen":
                WindowState = WindowState.Maximized;
                WindowStyle = WindowStyle.None;
                UpdateFullScreenUI(true);
                break;
            case "Normal":
            default:
                // Use default window size from XAML
                break;
        }
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        LoggingService.Info("MainWindow_Loaded fired");
        LoggingService.Info($"DataContext type: {DataContext?.GetType().Name}");

        // Find the storyboard resource and begin the animation
        // Storyboard storyboard = (Storyboard)FindResource("RotateStoryboard");
        // storyboard.Begin();

        // Run retention cleanup after window is loaded
        if (DataContext is MainViewModel vm)
        {
            LoggingService.Info("Calling RunRetentionCleanupAsync");
            await vm.RunRetentionCleanupAsync();
        }
        else
        {
            LoggingService.Info("DataContext is not MainViewModel");
        }
    }

    private void UpdateFullScreenUI(bool isFullScreen)
    {
        // Update exit button visibility
        ExitButton.Tag = isFullScreen;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        // Toggle fullscreen with F11
        if (e.Key == Key.F11)
        {
            if (WindowStyle == WindowStyle.None)
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
                UpdateFullScreenUI(false);
            }
            else
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                UpdateFullScreenUI(true);
            }
        }
    }

    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        // Exit fullscreen mode
        WindowStyle = WindowStyle.SingleBorderWindow;
        WindowState = WindowState.Normal;
        UpdateFullScreenUI(false);
    }

    private void MenuButton_Click(object sender, RoutedEventArgs e)
    {
        // Open the context menu dropdown
        if (sender is Button button && button.ContextMenu != null)
        {
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.IsOpen = true;
        }
    }
}