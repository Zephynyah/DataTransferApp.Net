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
        
        [ObservableProperty]
        private bool _showFolderAuditDetailsIcon = true;
        
        [ObservableProperty]
        private bool _showAuditSummaryAsCards = true;
        
        [ObservableProperty]
        private string _currentUser = $"{Environment.MachineName}\\{Environment.UserName}";
        
        [ObservableProperty]
        private string _appVersion = "1.2.0";
        
        [ObservableProperty]
        private string _appTitle = "Data Transfer Application";

        [ObservableProperty]
        private string _appDescription = "Collateral L2H Data Transfer Application";
        
        [ObservableProperty]
        private string _currentDateTime = DateTime.Now.ToString("MMMM dd, yyyy hh:mm tt");
        
        private readonly DispatcherTimer _timeUpdateTimer;

        public MainViewModel(AppSettings settings)
        {
            _settings = settings;
            _fileService = new FileService();
            _auditService = new AuditService(settings);
            _transferService = new TransferService(settings);
            _archiveService = new ArchiveService();
            
            // Initialize settings-based properties
            ShowFolderAuditDetailsIcon = _settings.ShowFolderAuditDetailsIcon;
            ShowAuditSummaryAsCards = _settings.ShowAuditSummaryAsCards;

            // Initialize drive detection timer (check every 3 seconds)
            _driveDetectionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _driveDetectionTimer.Tick += (s, e) => DetectDrives();
            _driveDetectionTimer.Start();
            
            // Initialize time update timer (update every second)
            _timeUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timeUpdateTimer.Tick += (s, e) => CurrentDateTime = DateTime.Now.ToString("MMMM dd, yyyy hh:mm tt");
            _timeUpdateTimer.Start();

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

            // Notify commands to re-evaluate when selected folder changes
            AuditFolderCommand.NotifyCanExecuteChanged();
            TransferFolderCommand.NotifyCanExecuteChanged();
            TransferFolderWithOverrideCommand.NotifyCanExecuteChanged();
        }

        partial void OnSelectedDriveChanged(RemovableDrive? value)
        {
            // Notify transfer commands to re-evaluate when selected drive changes
            TransferFolderCommand.NotifyCanExecuteChanged();
            TransferFolderWithOverrideCommand.NotifyCanExecuteChanged();
        }

        partial void OnIsProcessingChanged(bool value)
        {
            // Notify all commands to re-evaluate when processing state changes
            AuditFolderCommand.NotifyCanExecuteChanged();
            TransferFolderCommand.NotifyCanExecuteChanged();
            TransferFolderWithOverrideCommand.NotifyCanExecuteChanged();
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
                
                // Auto-select first folder if available
                if (FolderList.Count > 0)
                {
                    SelectedFolder = FolderList[0];
                }
                
                UpdateStatistics();
                StatusMessage = $"Loaded {folders.Count} folder(s)";
                
                // Run audit all if enabled in settings
                if (_settings.AutoAuditOnStartup && FolderList.Count > 0)
                {
                    LoggingService.Info("Auto-audit on startup enabled, running audit all...");
                    await AuditAllFoldersAsync();
                }
                else
                {
                    LoggingService.Info("Auto-audit on startup not enabled or no folders to audit.");
                }   

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
                    
                    // Auto-select first drive if none selected or if drives changed
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
                
                // Update individual audit statuses
                // Combine naming and dataset validation into naming status
                bool namingPassed = result.NameValidation?.IsValid == true && result.DatasetValidation?.IsValid == true;
                SelectedFolder.NamingAuditStatus = namingPassed ? "Passed" : "Failed";
                
                // Combine failure reasons
                var namingReasons = new List<string>();
                if (result.NameValidation?.IsValid == false)
                    namingReasons.Add(result.NameValidation.Message);
                if (result.DatasetValidation?.IsValid == false)
                    namingReasons.Add(result.DatasetValidation.Message);
                SelectedFolder.NamingFailureReason = string.Join(". ", namingReasons);
                
                SelectedFolder.DatasetAuditStatus = result.DatasetValidation?.IsValid == true ? "Passed" : "Failed";
                SelectedFolder.DatasetFailureReason = result.DatasetValidation?.IsValid == true ? string.Empty : result.DatasetValidation?.Message ?? "Unknown dataset error";
                SelectedFolder.BlacklistAuditStatus = result.ExtensionValidation?.IsValid == true ? "Passed" : "Failed";
                SelectedFolder.BlacklistViolationCount = result.ExtensionValidation?.Violations.Count ?? 0;
                
                // Count compressed files
                var compressedExtensions = new[] { ".zip", ".rar", ".7z", ".gz", ".tar", ".bz2", ".xz", ".mdzip", ".tar.gz", ".tar.xz", ".tar.bz2", ".tgz", ".tbz2", ".txz" };
                var compressedCount = SelectedFolder.Files.Count(f => 
                    compressedExtensions.Contains(f.Extension.ToLower()) || 
                    f.FileName.ToLower().EndsWith(".tar.gz") || f.FileName.ToLower().EndsWith(".tgz") ||
                    f.FileName.ToLower().EndsWith(".tar.xz") || f.FileName.ToLower().EndsWith(".txz") ||
                    f.FileName.ToLower().EndsWith(".tar.bz2") || f.FileName.ToLower().EndsWith(".tbz2"));
                SelectedFolder.CompressedFileCount = compressedCount;
                SelectedFolder.CompressedAuditStatus = compressedCount > 0 ? "Caution" : "Passed";

                // Update file statuses and flags
                // Reset all files first
                foreach (var file in SelectedFolder.Files)
                {
                    file.IsBlacklisted = false;
                    file.IsCompressed = false;
                    file.Status = "Ready";
                }
                
                // Mark blacklisted files
                if (result.ExtensionValidation?.Violations.Count > 0)
                {
                    foreach (var violation in result.ExtensionValidation.Violations)
                    {
                        var file = SelectedFolder.Files.FirstOrDefault(f => f.RelativePath == violation.RelativePath);
                        if (file != null)
                        {
                            file.Status = "Blacklisted";
                            file.IsBlacklisted = true;
                        }
                    }
                }
                
                // Mark compressed files
                foreach (var file in SelectedFolder.Files)
                {
                    var fileName = file.FileName.ToLower();
                    if (compressedExtensions.Contains(file.Extension.ToLower()) ||
                        fileName.EndsWith(".tar.gz") || fileName.EndsWith(".tgz") ||
                        fileName.EndsWith(".tar.xz") || fileName.EndsWith(".txz") ||
                        fileName.EndsWith(".tar.bz2") || fileName.EndsWith(".tbz2"))
                    {
                        file.IsCompressed = true;
                        if (file.Status == "Ready")
                        {
                            file.Status = "Compressed";
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
                    
                    // Update individual audit statuses
                    // Combine naming and dataset validation into naming status
                    bool namingPassed = result.NameValidation?.IsValid == true && result.DatasetValidation?.IsValid == true;
                    folder.NamingAuditStatus = namingPassed ? "Passed" : "Failed";
                    
                    // Combine failure reasons
                    var namingReasons = new List<string>();
                    if (result.NameValidation?.IsValid == false)
                        namingReasons.Add(result.NameValidation.Message);
                    if (result.DatasetValidation?.IsValid == false)
                        namingReasons.Add(result.DatasetValidation.Message);
                    folder.NamingFailureReason = string.Join(". ", namingReasons);
                    
                    folder.DatasetAuditStatus = result.DatasetValidation?.IsValid == true ? "Passed" : "Failed";
                    folder.DatasetFailureReason = result.DatasetValidation?.IsValid == true ? string.Empty : result.DatasetValidation?.Message ?? "Unknown dataset error";
                    folder.BlacklistAuditStatus = result.ExtensionValidation?.IsValid == true ? "Passed" : "Failed";
                    folder.BlacklistViolationCount = result.ExtensionValidation?.Violations.Count ?? 0;
                    
                    // Count compressed files
                    var compressedExtensions = new[] { ".zip", ".rar", ".7z", ".gz", ".tar", ".bz2", ".xz", ".mdzip", ".tar.gz", ".tar.xz", ".tar.bz2", ".tgz", ".tbz2", ".txz" };
                    var compressedCount = folder.Files.Count(f => 
                        compressedExtensions.Contains(f.Extension.ToLower()) || 
                        f.FileName.ToLower().EndsWith(".tar.gz") || f.FileName.ToLower().EndsWith(".tgz") ||
                        f.FileName.ToLower().EndsWith(".tar.xz") || f.FileName.ToLower().EndsWith(".txz") ||
                        f.FileName.ToLower().EndsWith(".tar.bz2") || f.FileName.ToLower().EndsWith(".tbz2"));
                    folder.CompressedFileCount = compressedCount;
                    folder.CompressedAuditStatus = compressedCount > 0 ? "Caution" : "Passed";
                    
                    // Update file flags
                    foreach (var file in folder.Files)
                    {
                        file.IsBlacklisted = false;
                        file.IsCompressed = false;
                        file.Status = "Ready";
                    }
                    
                    // Mark blacklisted files
                    if (result.ExtensionValidation?.Violations.Count > 0)
                    {
                        foreach (var violation in result.ExtensionValidation.Violations)
                        {
                            var file = folder.Files.FirstOrDefault(f => f.RelativePath == violation.RelativePath);
                            if (file != null)
                            {
                                file.Status = "Blacklisted";
                                file.IsBlacklisted = true;
                            }
                        }
                    }
                    
                    // Mark compressed files
                    foreach (var file in folder.Files)
                    {
                        var fileName = file.FileName.ToLower();
                        if (compressedExtensions.Contains(file.Extension.ToLower()) ||
                            fileName.EndsWith(".tar.gz") || fileName.EndsWith(".tgz") ||
                            fileName.EndsWith(".tar.xz") || fileName.EndsWith(".txz") ||
                            fileName.EndsWith(".tar.bz2") || fileName.EndsWith(".tbz2"))
                        {
                            file.IsCompressed = true;
                            if (file.Status == "Ready")
                            {
                                file.Status = "Compressed";
                            }
                        }
                    }
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

            // Check drive contents if no transfers yet
            if (TransferredCount == 0 && _transferService.DriveHasContents(SelectedDrive.DriveLetter))
            {
                var driveCount = _transferService.GetTransferredFolderCount(SelectedDrive.DriveLetter);
                var result = MessageBox.Show(
                    $"The drive {SelectedDrive.DriveLetter} already contains {driveCount} folder(s).\n\n" +
                    "Do you want to APPEND to existing contents?\n\n" +
                    "YES = Append/Add to existing\n" +
                    "NO = Clear drive first\n" +
                    "CANCEL = Abort transfer",
                    "Drive Contains Data",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
                else if (result == MessageBoxResult.No)
                {
                    // Clear drive first
                    await ClearDriveAsync();
                    if (IsProcessing) return; // If clear failed or is still running
                }
                // If Yes, continue with transfer
            }

            IsProcessing = true;
            var total = passedFolders.Count;
            var completed = 0;
            var failed = 0;
            var skipped = 0;

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
                        if (result.ErrorMessage == "Skipped - folder already exists")
                        {
                            skipped++;
                            LoggingService.Info($"Skipped existing folder: {folder.FolderName}");
                        }
                        
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
                StatusMessage = $"Transfer All complete: {successCount} succeeded, {failed} failed, {skipped} skipped";
                ShowSnackbar($"Transferred {successCount} of {total} folders ({skipped} skipped)", failed > 0 ? "warning" : "success");
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

            var folderName = SelectedFolder.FolderName; // Store name before folder is removed
            
            // Check drive contents if no transfers yet
            if (TransferredCount == 0 && _transferService.DriveHasContents(SelectedDrive.DriveLetter))
            {
                var driveCount = _transferService.GetTransferredFolderCount(SelectedDrive.DriveLetter);
                var result = MessageBox.Show(
                    $"The drive {SelectedDrive.DriveLetter} already contains {driveCount} folder(s).\n\n" +
                    "Do you want to APPEND to existing contents?\n\n" +
                    "YES = Append/Add to existing\n" +
                    "NO = Clear drive first\n" +
                    "CANCEL = Abort transfer",
                    "Drive Contains Data",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
                else if (result == MessageBoxResult.No)
                {
                    // Clear drive first
                    await ClearDriveAsync();
                    if (IsProcessing) return; // If clear failed or is still running
                }
                // If Yes, continue with transfer
            }

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
                    
                    var message = result.ErrorMessage == "Skipped - folder already exists" 
                        ? $"Skipped (already exists): {folderName}" 
                        : $"Transfer complete: {folderName}";
                    
                    StatusMessage = message;
                    ShowSnackbar(message, result.ErrorMessage != null ? "info" : "success");
                }
                else
                {
                    StatusMessage = $"Transfer failed: {result.ErrorMessage}";
                    ShowSnackbar($"Transfer failed: {result.ErrorMessage}", "error");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Transfer error: {ex.Message}";
                LoggingService.Error("Transfer error", ex);
                ShowSnackbar($"Transfer error: {ex.Message}", "error");
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

            var folderName = SelectedFolder.FolderName; // Store name before folder is removed
            
            var result = MessageBox.Show(
                $"This folder has failed audit. Are you sure you want to transfer '{folderName}' anyway?\n\nAudit Status: {SelectedFolder.AuditStatus}",
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
                    StatusMessage = $"Transfer complete (Override): {folderName}";
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
            ProgressPercent = 0;
            StatusMessage = $"Clearing drive {SelectedDrive.DriveLetter}...";

            try
            {
                var progress = new Progress<TransferProgress>(p =>
                {
                    ProgressText = $"Clearing Drive: {p.CurrentFile} ({p.CompletedFiles}/{p.TotalFiles})";
                    ProgressPercent = p.PercentComplete;
                    StatusMessage = $"Clearing: {p.CurrentFile}";
                });

                await _transferService.ClearDriveAsync(SelectedDrive.DriveLetter, progress);
                
                StatusMessage = $"Drive cleared: {SelectedDrive.DriveLetter}";
                ShowSnackbar($"Drive cleared successfully: {SelectedDrive.DriveLetter}", "success");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error clearing drive: {ex.Message}";
                ShowSnackbar($"Error clearing drive: {ex.Message}", "error");
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
                ProgressPercent = 0;
                ProgressText = "Ready";
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
                    // Reload settings from database
                    var updatedSettings = App.SettingsService!.GetSettings();
                    
                    // Update observable properties from reloaded settings
                    ShowFolderAuditDetailsIcon = updatedSettings.ShowFolderAuditDetailsIcon;
                    ShowAuditSummaryAsCards = updatedSettings.ShowAuditSummaryAsCards;
                    
                    // Copy updated settings back to the _settings reference
                    _settings.ShowFolderAuditDetailsIcon = updatedSettings.ShowFolderAuditDetailsIcon;
                    _settings.AutoAuditOnStartup = updatedSettings.AutoAuditOnStartup;
                    _settings.ShowAuditSummaryAsCards = updatedSettings.ShowAuditSummaryAsCards;
                    
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

        [RelayCommand]
        private void OpenAbout()
        {
            try
            {
                var aboutWindow = new AboutViewWindow(this);
                aboutWindow.ShowDialog();

            }
            catch (Exception ex)
            {
                ShowSnackbar($"Error opening about: {ex.Message}", "error");
                LoggingService.Error("Error opening about", ex);
            }
        }

        [RelayCommand]
        private void ViewTransferHistory()
        {
            try
            {
                var historyWindow = new TransferHistoryWindow(_settings.TransferLogsDirectory);
                historyWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                ShowSnackbar($"Error opening transfer history: {ex.Message}", "error");
                LoggingService.Error("Error opening transfer history", ex);
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
