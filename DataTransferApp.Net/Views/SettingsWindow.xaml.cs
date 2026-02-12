using System.Windows;
using DataTransferApp.Net.Models;
using DataTransferApp.Net.Services;

namespace DataTransferApp.Net.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// Provides UI for configuring application settings, RoboSharp presets, and transfer options.
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly SettingsService _settingsService;
        private readonly AppSettings _settings;
        private readonly Action? _onSettingsSaved;

        /// <summary>
        /// Gets a value indicating whether settings were successfully saved.
        /// Used by the calling window to determine if settings should be reloaded.
        /// </summary>
        public bool SettingsWereSaved { get; private set; } = false;

        public SettingsWindow(SettingsService settingsService, AppSettings settings, Action? onSettingsSaved = null)
        {
            InitializeComponent();
            _settingsService = settingsService;
            _settings = settings;
            _onSettingsSaved = onSettingsSaved;
            DataContext = settings;

            // Trigger initial validation
            ValidateAllSettings();
        }

        private void ValidateAllSettings()
        {
            // Trigger validation for directory path properties
            _settings.ValidateAllProperties();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _settingsService.SaveSettings(_settings);
                LoggingService.Info("Settings saved by user");

                SettingsWereSaved = true;

                // Invoke callback immediately when settings are saved
                _onSettingsSaved?.Invoke();

                // Close window only if the user has enabled auto-close
                if (_settings.CloseSettingsOnSave)
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    // Keep window open - DialogResult remains unset so ShowDialog() returns null/false
                    // But SettingsWereSaved is true so main window knows to reload settings
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LoggingService.Error("Error saving settings", ex);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #region RoboSharp Presets

        private void ApplyFastPreset_Checked(object sender, RoutedEventArgs e)
        {
            _settings.RoboSharpPresetMode = "Fast";
            _settings.RobocopyThreadCount = 16;
            _settings.RobocopyRetries = 3;
            _settings.RobocopyRetryWaitSeconds = 5;
            _settings.UseRestartableMode = false;
            _settings.UseBackupMode = false;
            _settings.VerifyRobocopy = false;
            _settings.RobocopyVerboseOutput = false;
            _settings.RobocopyDetailedLogging = false;

            LoggingService.Info("RoboSharp Fast preset applied");
        }

        private void ApplySafePreset_Checked(object sender, RoutedEventArgs e)
        {
            _settings.RoboSharpPresetMode = "Safe";
            _settings.RobocopyThreadCount = 8;
            _settings.RobocopyRetries = 5;
            _settings.RobocopyRetryWaitSeconds = 10;
            _settings.UseRestartableMode = true;
            _settings.UseBackupMode = true;
            _settings.VerifyRobocopy = false;
            _settings.RobocopyVerboseOutput = false;
            _settings.RobocopyDetailedLogging = true;

            LoggingService.Info("RoboSharp Safe preset applied");
        }

        private void ApplyNetworkPreset_Checked(object sender, RoutedEventArgs e)
        {
            _settings.RoboSharpPresetMode = "Network";
            _settings.RobocopyThreadCount = 4;
            _settings.RobocopyRetries = 10;
            _settings.RobocopyRetryWaitSeconds = 30;
            _settings.UseRestartableMode = true;
            _settings.UseBackupMode = true;
            _settings.VerifyRobocopy = true;
            _settings.RobocopyVerboseOutput = false;
            _settings.RobocopyDetailedLogging = true;

            LoggingService.Info("RoboSharp Network preset applied");
        }

        private void ApplyArchivePreset_Checked(object sender, RoutedEventArgs e)
        {
            _settings.RoboSharpPresetMode = "Archive";
            _settings.RobocopyThreadCount = 8;
            _settings.RobocopyRetries = 5;
            _settings.RobocopyRetryWaitSeconds = 10;
            _settings.UseRestartableMode = true;
            _settings.UseBackupMode = true;
            _settings.VerifyRobocopy = true;
            _settings.RobocopyVerboseOutput = true;
            _settings.RobocopyDetailedLogging = true;

            LoggingService.Info("RoboSharp Archive preset applied");
        }

        #endregion

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to reset all settings to defaults?",
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _settingsService.ResetToDefaults();
                var newSettings = _settingsService.GetSettings();
                DataContext = newSettings;
                MessageBox.Show("Settings reset to defaults.", "Reset Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}