using DataTransferApp.Net.Models;
using DataTransferApp.Net.Services;
using DataTransferApp.Net.ViewModels;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace DataTransferApp.Net;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static SettingsService? SettingsService { get; private set; }
    public static AppSettings? Settings { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            // Get AppData path
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DataTransferApp");

            // Ensure directory exists
            Directory.CreateDirectory(appDataPath);

            // Initialize settings service
            var dbPath = Path.Combine(appDataPath, "settings.db");
            SettingsService = new SettingsService(dbPath);
            Settings = SettingsService.GetSettings();

            // Initialize logging
            var logPath = Path.Combine(appDataPath, "Logs", $"app-{DateTime.Now:yyyyMMdd}.log");
            var logLevel = LoggingService.ParseLogLevel(Settings.LogLevel);
            LoggingService.Initialize(logPath, logLevel);

            LoggingService.Info("=== Application Starting ===");
            LoggingService.Info($"Version: {Settings.ApplicationVersion}");
            LoggingService.Info($"DTA: {Settings.DataTransferAgent}");
            LoggingService.Info($"AppData: {appDataPath}");

            // Ensure DPI awareness is set programmatically (backup to manifest)
            try
            {
                SetProcessDpiAwareness();
            }
            catch (Exception ex)
            {
                LoggingService.Error("Failed to set DPI awareness programmatically", ex);
            }

            // Create main window with ViewModel
            var mainWindow = new Views.MainWindow
            {
                DataContext = new MainViewModel(Settings)
            };

            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Application startup error:\n\n{ex.Message}",
                "Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            LoggingService.Error("Application startup failed", ex);
            Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            LoggingService.Info("=== Application Exiting ===");
            SettingsService?.Dispose();
            LoggingService.Dispose();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during shutdown: {ex.Message}");
        }

        base.OnExit(e);
    }

    #region DPI Awareness

    private enum PROCESS_DPI_AWARENESS
    {
        PROCESS_DPI_UNAWARE = 0,
        PROCESS_SYSTEM_DPI_AWARE = 1,
        PROCESS_PER_MONITOR_DPI_AWARE = 2
    }

    private enum DPI_AWARENESS_CONTEXT
    {
        DPI_AWARENESS_CONTEXT_UNAWARE = -1,
        DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = -2,
        DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = -3,
        DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS value);

    [DllImport("user32.dll")]
    private static extern bool SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT value);

    private void SetProcessDpiAwareness()
    {
        try
        {
            // Try PerMonitorV2 first (Windows 10 1607+)
            if (!SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2))
            {
                // Fallback to PerMonitor (Windows 8.1+)
                SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE);
            }

            LoggingService.Info("DPI awareness set to PerMonitorV2");
        }
        catch (Exception ex)
        {
            LoggingService.Error("Failed to set DPI awareness", ex);
        }
    }

    #endregion
}

