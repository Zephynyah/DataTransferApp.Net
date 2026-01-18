using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataTransferApp.Net.Helpers;
using DataTransferApp.Net.Models;
using DataTransferApp.Net.Services;
using DataTransferApp.Net.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DataTransferApp.Net.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly FileService _fileService;
        private readonly AuditService _auditService;
        private readonly TransferService _transferService;
        private readonly ArchiveService _archiveService;
        private readonly AppSettings _settings;
        private readonly DispatcherTimer _driveDetectionTimer;

        [ObservableProperty]
        private ObservableCollection<FolderData> _folderList = new();

        [ObservableProperty]
        private ObservableCollection<FolderData> _transferredList = new();

        [ObservableProperty]
        private FolderData? _selectedFolder;

        [ObservableProperty]
        private ObservableCollection<FileData> _fileList = new();

        [ObservableProperty]
        private ObservableCollection<RemovableDrive> _removableDrives = new();

        [ObservableProperty]
        private RemovableDrive? _selectedDrive;

        [ObservableProperty]
        private string _statusMessage = "Ready";

        [ObservableProperty]
        private string _progressText = "Ready";

        [ObservableProperty]
        private int _progressPercent = 0;

        [ObservableProperty]
        private int _totalFolders = 0;

        [ObservableProperty]
        private int _readyFolders = 0;

        [ObservableProperty]
        private int _failedFolders = 0;

        [ObservableProperty]
        private int _transferredCount = 0;

        [ObservableProperty]
        private string _totalSize = "0 MB";

        [ObservableProperty]
        private bool _isProcessing = false;

        [ObservableProperty]
        private bool _isSnackbarVisible = false;

        [ObservableProperty]
        private string _snackbarMessage = string.Empty;

        [ObservableProperty]
        private string _snackbarBackground = "#2ECC71";

        public MainViewModel(AppSettings settings)
        {
            _settings = settings;
            _fileService = new FileService();
            _auditService = new AuditService(settings);
            _transferService = new TransferService(settings);
            _archiveService = new ArchiveService();

            // Initialize drive detection timer (check every 3 seconds)
            _driveDetectionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _driveDetectionTimer.Tick += (s, e) => DetectDrives();
            _driveDetectionTimer.Start();

            // Load initial data
            _ = LoadDataAsync();
            
            // Initial drive detection
            DetectDrives();
        }

        partial void OnSelectedFolderChanged(FolderData? oldValue, FolderData? newValue)
        {
            // Unsubscribe from old folder's PropertyChanged event
            if (oldValue != null)
            {
                oldValue.PropertyChanged -= OnSelectedFolderPropertyChanged;
            }

            // Subscribe to new folder's PropertyChanged event
            if (newValue != null)
            {
                newValue.PropertyChanged += OnSelectedFolderPropertyChanged;
            }
        }

        private void OnSelectedFolderPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // When folder properties change, notify commands to re-evaluate
            if (e.PropertyName == nameof(FolderData.AuditStatus) || 
                e.PropertyName == nameof(FolderData.CanTransfer))
            {
                AuditFolderCommand.NotifyCanExecuteChanged();
                TransferFolderCommand.NotifyCanExecuteChanged();
            }
        }

        partial void OnSelectedFolderChanged(FolderData? value)
        {
            if (value != null)
            {
                FileList = new ObservableCollection<FileData>(value.Files);
            }
            else
            {
                FileList.Clear();
            }
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            IsProcessing = true;
            StatusMessage = "Loading folders...";

            try
            {
                var folders = await _fileService.ScanStagingDirectoryAsync(_settings.StagingDirectory);
                FolderList = new ObservableCollection<FolderData>(folders);
                UpdateStatistics();
                StatusMessage = $"Loaded {folders.Count} folder(s)";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading folders: {ex.Message}";
                LoggingService.Error("Error loading folders", ex);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        [RelayCommand]
        private void DetectDrives()
        {
            try
            {
                var drives = _transferService.GetRemovableDrives();
                var currentDriveLetters = RemovableDrives.Select(d => d.DriveLetter).ToList();
                var newDriveLetters = drives.Select(d => d.DriveLetter).ToList();
                
                // Only update if drives changed
                if (!currentDriveLetters.SequenceEqual(newDriveLetters))
                {
                    RemovableDrives = new ObservableCollection<RemovableDrive>(drives);
                    
                    if (drives.Count > 0 && SelectedDrive == null)
                    {
                        SelectedDrive = drives[0];
                    }
                    // Clear selection if selected drive was removed
                    else if (SelectedDrive != null && !drives.Any(d => d.DriveLetter == SelectedDrive.DriveLetter))
                    {
                        SelectedDrive = drives.Count > 0 ? drives[0] : null;
                    }
                    
                    LoggingService.Info($"Drive list updated: {drives.Count} removable drive(s) detected");
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error("Error detecting drives", ex);
            }
        }

        [RelayCommand(CanExecute = nameof(CanAuditFolder))]
        private async Task AuditFolderAsync()
        {
            if (SelectedFolder == null) return;

            IsProcessing = true;
            StatusMessage = $"Auditing {SelectedFolder.FolderName}...";

            try
            {
                var result = await _auditService.AuditFolderAsync(
                    SelectedFolder.FolderPath,
                    SelectedFolder.FolderName);

                SelectedFolder.AuditResult = result;
                SelectedFolder.AuditStatus = result.OverallStatus;

                // Update file statuses based on audit
                if (result.ExtensionValidation?.Violations.Count > 0)
                {
                    foreach (var violation in result.ExtensionValidation.Violations)
                    {
                        var file = SelectedFolder.Files.FirstOrDefault(f => f.RelativePath == violation.RelativePath);
                        if (file != null)
                        {
                            file.Status = "Blacklisted";
                        }
                    }
                }

                UpdateStatistics();
                StatusMessage = $"Audit {result.OverallStatus}: {SelectedFolder.FolderName}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Audit error: {ex.Message}";
                LoggingService.Error($"Audit error for {SelectedFolder.FolderName}", ex);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private bool CanAuditFolder() => SelectedFolder != null && !IsProcessing;

        [RelayCommand]
        private async Task AuditAllFoldersAsync()
        {
            IsProcessing = true;
            var total = FolderList.Count;
            var completed = 0;

            try
            {
                foreach (var folder in FolderList)
                {
                    completed++;
                    StatusMessage = $"Auditing {completed}/{total}: {folder.FolderName}";

                    var result = await _auditService.AuditFolderAsync(folder.FolderPath, folder.FolderName);
                    folder.AuditResult = result;
                    folder.AuditStatus = result.OverallStatus;
                }

                UpdateStatistics();
                StatusMessage = $"Audit complete: {ReadyFolders} passed, {FailedFolders} failed";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error during audit: {ex.Message}";
                LoggingService.Error("Audit all error", ex);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        [RelayCommand]
        private async Task TransferAllFoldersAsync()
        {
            if (SelectedDrive == null)
            {
                ShowSnackbar("Please select a destination drive", "error");
                return;
            }

            var passedFolders = FolderList.Where(f => f.CanTransfer).ToList();
            if (!passedFolders.Any())
            {
                ShowSnackbar("No folders passed audit. Run audit first.", "warning");
                return;
            }

            IsProcessing = true;
            var total = passedFolders.Count;
            var completed = 0;
            var failed = 0;

            try
            {
                foreach (var folder in passedFolders.ToList()) // ToList to avoid collection modification issues
                {
                    completed++;
                    StatusMessage = $"Transferring {completed}/{total}: {folder.FolderName}";

                    var progress = new Progress<TransferProgress>(p =>
                    {
                        ProgressText = $"[{completed}/{total}] {p.CurrentFile} ({p.CompletedFiles}/{p.TotalFiles})";
                        ProgressPercent = p.PercentComplete;
                    });

                    var result = await _transferService.TransferFolderAsync(
                        folder,
                        SelectedDrive.DriveLetter,
                        progress);

                    if (result.Success)
                    {
                        TransferredList.Add(folder);
                        FolderList.Remove(folder);
                    }
                    else
                    {
                        failed++;
                        LoggingService.Warning($"Transfer failed for {folder.FolderName}: {result.ErrorMessage}");
                    }
                }

                UpdateStatistics();
                var successCount = completed - failed;
                StatusMessage = $"Transfer All complete: {successCount} succeeded, {failed} failed";
                ShowSnackbar($"Transferred {successCount} of {total} folders", failed > 0 ? "warning" : "success");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Transfer All error: {ex.Message}";
                LoggingService.Error("Transfer all error", ex);
                ShowSnackbar($"Transfer All failed: {ex.Message}", "error");
            }
            finally
            {
                IsProcessing = false;
                ProgressPercent = 0;
                ProgressText = "Ready";
            }
        }

        [RelayCommand(CanExecute = nameof(CanTransferFolder))]
        private async Task TransferFolderAsync()
        {
            if (SelectedFolder == null || SelectedDrive == null) return;

            IsProcessing = true;
            ProgressPercent = 0;

            try
            {
                var progress = new Progress<TransferProgress>(p =>
                {
                    ProgressText = $"Transferring: {p.CurrentFile} ({p.CompletedFiles}/{p.TotalFiles})";
                    ProgressPercent = p.PercentComplete;
                });

                var result = await _transferService.TransferFolderAsync(
                    SelectedFolder,
                    SelectedDrive.DriveLetter,
                    progress);

                if (result.Success)
                {
                    TransferredList.Add(SelectedFolder);
                    FolderList.Remove(SelectedFolder);
                    UpdateStatistics();
                    StatusMessage = $"Transfer complete: {SelectedFolder.FolderName}";
                }
                else
                {
                    StatusMessage = $"Transfer failed: {result.ErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Transfer error: {ex.Message}";
                LoggingService.Error("Transfer error", ex);
            }
            finally
            {
                IsProcessing = false;
                ProgressPercent = 0;
                ProgressText = "Ready";
            }
        }

        private bool CanTransferFolder() =>
            SelectedFolder != null &&
            SelectedFolder.CanTransfer &&
            SelectedDrive != null &&
            !IsProcessing;

        [RelayCommand(CanExecute = nameof(CanTransferWithOverride))]
        private async Task TransferFolderWithOverrideAsync()
        {
            if (SelectedFolder == null || SelectedDrive == null) return;

            var result = MessageBox.Show(
                $"This folder has failed audit. Are you sure you want to transfer '{SelectedFolder.FolderName}' anyway?\n\nAudit Status: {SelectedFolder.AuditStatus}",
                "Override Audit - Confirm Transfer",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            IsProcessing = true;
            ProgressPercent = 0;

            try
            {
                var progress = new Progress<TransferProgress>(p =>
                {
                    ProgressText = $"Transferring (Override): {p.CurrentFile} ({p.CompletedFiles}/{p.TotalFiles})";
                    ProgressPercent = p.PercentComplete;
                });

                var transferResult = await _transferService.TransferFolderAsync(
                    SelectedFolder,
                    SelectedDrive.DriveLetter,
                    progress);

                if (transferResult.Success)
                {
                    TransferredList.Add(SelectedFolder);
                    FolderList.Remove(SelectedFolder);
                    UpdateStatistics();
                    StatusMessage = $"Transfer complete (Override): {SelectedFolder.FolderName}";
                    ShowSnackbar($"Override transfer completed", "warning");
                }
                else
                {
                    StatusMessage = $"Transfer failed: {transferResult.ErrorMessage}";
                    ShowSnackbar($"Transfer failed: {transferResult.ErrorMessage}", "error");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Transfer error: {ex.Message}";
                LoggingService.Error("Transfer override error", ex);
                ShowSnackbar($"Transfer error: {ex.Message}", "error");
            }
            finally
            {
                IsProcessing = false;
                ProgressPercent = 0;
                ProgressText = "Ready";
            }
        }

        private bool CanTransferWithOverride() =>
            SelectedFolder != null &&
            !SelectedFolder.CanTransfer &&
            SelectedDrive != null &&
            !IsProcessing;

        [RelayCommand]
        private async Task ClearDriveAsync()
        {
            if (SelectedDrive == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to clear all data from {SelectedDrive.DisplayText}?",
                "Confirm Clear Drive",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            IsProcessing = true;
            StatusMessage = $"Clearing drive {SelectedDrive.DriveLetter}...";

            try
            {
                var progress = new Progress<string>(msg => StatusMessage = msg);
                await _transferService.ClearDriveAsync(SelectedDrive.DriveLetter, progress);
                StatusMessage = $"Drive cleared: {SelectedDrive.DriveLetter}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error clearing drive: {ex.Message}";
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        [RelayCommand]
        private void ViewFileContents(FileData? file)
        {
            if (file == null) return;

            try
            {
                if (file.IsArchive)
                {
                    var entries = _archiveService.GetArchiveContents(file.FullPath);
                    var window = new ArchiveViewerWindow(file.FileName, file.FullPath, entries);
                    window.ShowDialog();
                }
                else if (file.IsViewable)
                {
                    var content = _fileService.ReadTextFile(file.FullPath);
                    var window = new FileViewerWindow(file.FileName, file.FullPath, content);
                    window.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error viewing file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LoggingService.Error($"Error viewing file: {file.FullPath}", ex);
            }
        }

        [RelayCommand]
        private void OpenSettings()
        {
            try
            {
                var settingsWindow = new SettingsWindow(
                    App.SettingsService!,
                    App.Settings!);
                
                if (settingsWindow.ShowDialog() == true)
                {
                    // Reload settings if they were saved
                    StatusMessage = "Settings updated successfully";
                    ShowSnackbar("Settings saved successfully!", "success");
                    LoggingService.Info("Settings updated successfully");
                }
            }
            catch (Exception ex)
            {
                ShowSnackbar($"Error opening settings: {ex.Message}", "error");
                LoggingService.Error("Error opening settings", ex);
            }
        }

        private void UpdateStatistics()
        {
            TotalFolders = FolderList.Count;
            ReadyFolders = FolderList.Count(f => f.AuditStatus == "Passed");
            FailedFolders = FolderList.Count(f => f.AuditStatus == "Failed");
            TransferredCount = TransferredList.Count;
            TotalSize = FormatFileSize(FolderList.Sum(f => f.TotalSize));
        }

        private async void ShowSnackbar(string message, string type = "success")
        {
            SnackbarMessage = message;
            SnackbarBackground = type switch
            {
                "success" => "#2ECC71",
                "error" => "#E74C3C",
                "warning" => "#F39C12",
                "info" => "#3498DB",
                _ => "#2ECC71"
            };
            IsSnackbarVisible = true;

            await Task.Delay(4000);
            IsSnackbarVisible = false;
        }

        private static string FormatFileSize(long bytes)
        {
            const long GB = 1024 * 1024 * 1024;
            const long MB = 1024 * 1024;

            if (bytes >= GB)
                return $"{bytes / (double)GB:N2} GB";
            if (bytes >= MB)
                return $"{bytes / (double)MB:N2} MB";

            return $"{bytes / 1024.0:N2} KB";
        }
    }
}
