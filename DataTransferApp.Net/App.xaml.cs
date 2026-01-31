using System;
using System.IO;
using System.Windows;
using DataTransferApp.Net.Models;
using DataTransferApp.Net.Services;
using DataTransferApp.Net.ViewModels;

namespace DataTransferApp.Net;

/// <summary>
/// Interaction logic for App.xaml.
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
            InitializeApplicationData();
            InitializeServices();
            InitializeLogging();
            ShowMainWindow();
        }
        catch (Exception ex)
        {
            HandleStartupError(ex);
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

    private static void InitializeApplicationData()
    {
#if DEBUG
        // Set debug working directory to project root for easier debugging
        var appDataPath = Path.Combine("appDataPath", "DataTransferApp");
#else
        // Set application data path
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DataTransferApp");
#endif

        // Ensure directory exists
        Directory.CreateDirectory(appDataPath);
    }

    private static void InitializeServices()
    {
        // Initialize settings service
        var appDataPath = GetAppDataPath();
        var dbPath = Path.Combine(appDataPath, "settings.db");
        SettingsService = new SettingsService(dbPath);
        Settings = SettingsService.GetSettings();
    }

    private static void InitializeLogging()
    {
        var appDataPath = GetAppDataPath();
        var logPath = Path.Combine(appDataPath, "Logs", $"app-{DateTime.Now:yyyyMMdd}.log");
        var logLevel = LoggingService.ParseLogLevel(Settings!.LogLevel);
        LoggingService.Initialize(logPath, logLevel);

        LoggingService.Info("=== Application Starting ===");
        LoggingService.Info($"Version: {AppSettings.ApplicationVersion}");
        LoggingService.Info($"DTA: {Settings!.DataTransferAgent}");
        LoggingService.Info($"AppData: {appDataPath}");
    }

    private static string GetAppDataPath()
    {
#if DEBUG
        return Path.Combine("appDataPath", "DataTransferApp");
#else
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DataTransferApp");
#endif
    }

    private void ShowMainWindow()
    {
#if DEBUG
        // Create main window with ViewModel
        var mainWindow = new Views.MainWindow
        {
            DataContext = new MainViewModel(Settings!)
        };

        mainWindow.Show();
        Application.Current.MainWindow = mainWindow;
#else
        // Create splash screen
        var splash = new Views.SplashScreenWindow(() =>
        {
            // Create main window with ViewModel
            var mainWindow = new Views.MainWindow
            {
                DataContext = new MainViewModel(Settings!)
            };

            mainWindow.Show();
            Application.Current.MainWindow = mainWindow;
        });

        Application.Current.MainWindow = splash;
        splash.Show();
#endif
    }

    private void HandleStartupError(Exception ex)
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