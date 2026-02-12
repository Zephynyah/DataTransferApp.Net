using CommunityToolkit.Mvvm.ComponentModel;
using DataTransferApp.Net.Services;

namespace DataTransferApp.Net.ViewModels;

/// <summary>
/// Base class for all ViewModels in the application.
/// Provides common functionality like snackbar notifications, status management, and error handling.
/// </summary>
public partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool _isSnackbarVisible = false;

    [ObservableProperty]
    private string _snackbarMessage = string.Empty;

    [ObservableProperty]
    private string _snackbarBackground = "#E62ECC71"; // Default success color

    [ObservableProperty]
    private double _snackbarOpacity = 1.0; // Default opacity for notifications

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private bool _isProcessing = false;

    /// <summary>
    /// Shows a snackbar notification with the specified message and type.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="type">The type of notification (success, error, warning, info).</param>
    /// <param name="durationMs">How long to show the snackbar in milliseconds (default 4000).</param>
    /// <returns>A task that completes when the snackbar is hidden.</returns>
    protected async Task ShowSnackbar(string message, string type = "success", int durationMs = 4000)
    {
        SnackbarMessage = message;
        SnackbarBackground = type switch
        {
            "success" => "#E62ECC71",  // 90% opacity green
            "error" => "#E6E74C3C",    // 90% opacity red
            "warning" => "#E6F39C12",  // 90% opacity orange
            "info" => "#E63498DB",     // 90% opacity blue
            _ => "#E62ECC71"
        };
        IsSnackbarVisible = true;

        await Task.Delay(durationMs);
        IsSnackbarVisible = false;
    }

    /// <summary>
    /// Updates the status message.
    /// </summary>
    /// <param name="message">The new status message.</param>
    protected void SetStatus(string message)
    {
        StatusMessage = message;
    }

    /// <summary>
    /// Handles exceptions with logging and user notification.
    /// </summary>
    /// <param name="ex">The exception that occurred.</param>
    /// <param name="context">Context description for the error.</param>
    /// <param name="showSnackbar">Whether to show a snackbar notification.</param>
    /// <returns>A task that completes when the error handling is done.</returns>
    protected async Task HandleError(Exception ex, string context, bool showSnackbar = true)
    {
        LoggingService.Error($"{context}: {ex.Message}", ex);
        SetStatus($"Error: {context}");

        if (showSnackbar)
        {
            await ShowSnackbar($"Error: {context}", "error");
        }
    }

    /// <summary>
    /// Executes an async operation with loading state management and error handling.
    /// </summary>
    /// <param name="operation">The async operation to execute.</param>
    /// <param name="loadingMessage">Message to show while loading.</param>
    /// <param name="successMessage">Message to show on success.</param>
    /// <param name="context">Context for error logging.</param>
    /// <returns>A task that completes when the operation is finished.</returns>
    protected async Task ExecuteWithLoading(Func<Task> operation, string loadingMessage = "Loading...", string? successMessage = null, string context = "Operation")
    {
        IsLoading = true;
        SetStatus(loadingMessage);

        try
        {
            await operation();

            if (successMessage != null)
            {
                SetStatus(successMessage);
                await ShowSnackbar(successMessage, "success");
            }
        }
        catch (Exception ex)
        {
            await HandleError(ex, context);
        }
        finally
        {
            IsLoading = false;
        }
    }
}