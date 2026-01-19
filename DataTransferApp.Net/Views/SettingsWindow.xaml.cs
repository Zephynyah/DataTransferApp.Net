using DataTransferApp.Net.Models;
using DataTransferApp.Net.Services;
using System.Windows;

namespace DataTransferApp.Net.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsService _settingsService;
        private readonly AppSettings _settings;

        public SettingsWindow(SettingsService settingsService, AppSettings settings)
        {
            InitializeComponent();
            _settingsService = settingsService;
            _settings = settings;
            DataContext = settings;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _settingsService.SaveSettings(_settings);
                
                // Update logging level
                var logLevel = LoggingService.ParseLogLevel(_settings.LogLevel);
                
                MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoggingService.Info("Settings saved by user");
                
                DialogResult = true;
                Close();
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

        private void AddExcludedFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var inputDialog = new InputDialog("Add Excluded Folder", "Enter folder name to exclude:");
            if (inputDialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(inputDialog.InputText))
            {
                var folderName = inputDialog.InputText.Trim();
                
                // Check if already exists (case-insensitive)
                if (_settings.ExcludedFolders.Exists(f => f.Equals(folderName, System.StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show($"'{folderName}' is already in the exclusion list.", "Duplicate", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                _settings.ExcludedFolders.Add(folderName);
                ExcludedFoldersListBox.Items.Refresh();
                LoggingService.Info($"Added excluded folder: {folderName}");
            }
        }

        private void RemoveExcludedFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (ExcludedFoldersListBox.SelectedItem is string selectedFolder)
            {
                var result = MessageBox.Show(
                    $"Remove '{selectedFolder}' from excluded folders?",
                    "Confirm Remove",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _settings.ExcludedFolders.Remove(selectedFolder);
                    ExcludedFoldersListBox.Items.Refresh();
                    LoggingService.Info($"Removed excluded folder: {selectedFolder}");
                }
            }
        }
    }
}
