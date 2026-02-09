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
using DataTransferApp.Net.Controls;
using DataTransferApp.Net.Models;
using DataTransferApp.Net.Services;
using DataTransferApp.Net.ViewModels;
using FontAwesome.Sharp;
using Ookii.Dialogs.Wpf;

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

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        // Show confirmation dialog
        var result = MessageBox.Show(
            "Are you sure you want to exit the Data Transfer Application?",
            "Confirm Exit",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.No);

        if (result == MessageBoxResult.No)
        {
            // Cancel the closing
            e.Cancel = true;
            LoggingService.Info("Application close cancelled by user");
            return;
        }

        // User confirmed exit - perform cleanup
        LoggingService.Info("Application closing confirmed by user");

        try
        {
            // Cancel any running operations in ViewModel
            if (DataContext is MainViewModel vm)
            {
                // Check if any transfers are running
                if (vm.IsProcessing)
                {
                    var confirmTransfer = MessageBox.Show(
                        "A transfer is currently in progress. Are you sure you want to exit? This may result in incomplete transfers.",
                        "Transfer in Progress",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning,
                        MessageBoxResult.No);

                    if (confirmTransfer == MessageBoxResult.No)
                    {
                        e.Cancel = true;
                        LoggingService.Info("Application close cancelled due to active transfer");
                        return;
                    }
                }

                LoggingService.Info("Performing cleanup before exit");
            }

            LoggingService.Info("MainWindow closing - cleanup completed");
        }
        catch (Exception ex)
        {
            LoggingService.Error("Error during application shutdown", ex);
        }
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

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        LoggingService.Info("MainWindow_Loaded fired");
        LoggingService.Info($"DataContext type: {DataContext?.GetType().Name}");

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

    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        // Exit fullscreen mode
        WindowStyle = WindowStyle.SingleBorderWindow;
        WindowState = WindowState.Normal;
        UpdateFullScreenUI(false);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarAnalyzer.CSharp", "S2325:Make 'MenuButton_Click' a static method", Justification = "Event handler must be instance method")]
    private void MenuButton_Click(object sender, RoutedEventArgs e)
    {
        // Open the context menu dropdown
        if (sender is Button button && button.ContextMenu != null)
        {
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.IsOpen = true;
        }
    }

    /// <summary>
    /// Shows a Task Dialog asking user what to do with existing drive contents.
    /// </summary>
    /// <param name="driveLetter">The drive letter</param>
    /// <param name="folderCount">Number of folders on the drive</param>
    /// <returns>DriveContentAction enum indicating user's choice</returns>
    public DriveContentAction ShowDriveContentsDialog(string driveLetter, int folderCount)
    {
        if (TaskDialog.OSSupportsTaskDialogs)
        {

            string label = folderCount == 1 ? "folder" : "folders";

            using (TaskDialog dialog = new TaskDialog())
            {
                dialog.WindowTitle = "Drive Contains Data";
                dialog.MainInstruction = $"The drive {driveLetter} already contains {folderCount} {label}.";
                dialog.Content = "What would you like to do?";
                dialog.ButtonStyle = TaskDialogButtonStyle.CommandLinks;
                dialog.MainIcon = TaskDialogIcon.Warning;

                // Create command link buttons with descriptions
                TaskDialogButton clearButton = new TaskDialogButton($"Clear {driveLetter} Drive First");
                clearButton.CommandLinkNote = "Delete all existing contents on the drive before transfer";
                TaskDialogButton appendButton = new TaskDialogButton("Append to Existing Contents");
                appendButton.CommandLinkNote = "Add new folders alongside existing ones. Use with caution to avoid mixing with old data";
                TaskDialogButton cancelButton = new TaskDialogButton(ButtonType.Cancel);

                // Add buttons in order of appearance
                dialog.Buttons.Add(appendButton);
                dialog.Buttons.Add(clearButton);
                dialog.Buttons.Add(cancelButton);

                // Show dialog and get result
                TaskDialogButton button = dialog.ShowDialog(this);

                if (button == clearButton)
                {
                    LoggingService.Info($"User chose to clear drive {driveLetter} before transfer");
                    return DriveContentAction.Clear;
                }
                else if (button == appendButton)
                {
                    LoggingService.Info($"User chose to append to drive {driveLetter}");
                    return DriveContentAction.Append;
                }
                else
                {
                    LoggingService.Info($"User cancelled drive contents dialog for {driveLetter}");
                    return DriveContentAction.Cancel;
                }
            }
        }

        // Default fallback for systems that don't support TaskDialog
        LoggingService.Warning($"TaskDialog not supported on this OS; defaulting to Cancel for drive {driveLetter}");
        return DriveContentAction.Cancel;
    }
}
