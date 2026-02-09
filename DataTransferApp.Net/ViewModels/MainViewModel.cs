using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataTransferApp.Net.Helpers;
using DataTransferApp.Net.Models;
using DataTransferApp.Net.Services;
using DataTransferApp.Net.Views;
using Ookii.Dialogs.Wpf;

namespace DataTransferApp.Net.ViewModels
{
    /// <summary>
    /// Main view model for the Data Transfer Application.
    /// Handles folder auditing, transfer operations, drive management, and UI state.
    /// </summary>
    public partial class MainViewModel : ViewModelBase
    {
        private readonly FileService _fileService;
        private readonly AuditService _auditService;
        private readonly TransferService _transferService;
        private readonly ArchiveService _archiveService;
        private readonly AppSettings _settings;
        private readonly DispatcherTimer _driveDetectionTimer;
        private readonly DispatcherTimer _timeUpdateTimer;

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
        private string _progressIssues = "Idle";

        [ObservableProperty]
        private bool _isTransferActive = false;

        [ObservableProperty]
        private int _progressPercent = 0;

        private CancellationTokenSource? _transferCancellationTokenSource;

        [ObservableProperty]
        private int _totalFolders = 0;

        [ObservableProperty]
        private int _readyFolders = 0;

        [ObservableProperty]
        private bool _isRetentionCleanupRunning = false;

        [ObservableProperty]
        private int _cautionFolders = 0;

        [ObservableProperty]
        private int _failedFolders = 0;

        [ObservableProperty]
        private int _transferredCount = 0;

        [ObservableProperty]
        private string _totalSize = "0 MB";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CancelTransferCommand))]
        private bool _isProcessing = false;

        [ObservableProperty]
        private bool _isSnackbarVisible = false;

        [ObservableProperty]
        private string _snackbarMessage = string.Empty;

        [ObservableProperty]
        private string _snackbarBackground = "#E62ECC71";

        [ObservableProperty]
        private double _snackbarOpacity = 1.0;  // Adjust this value (0.0 to 1.0) to control notification brightness

        [ObservableProperty]
        private bool _showFolderAuditDetailsIcon = true;

        [ObservableProperty]
        private bool _showAuditSummaryAsCards = true;

        [ObservableProperty]
        private string _currentUser = $"{Environment.MachineName}\\{Environment.UserName}";

        [ObservableProperty]
        private string _appVersion = VersionHelper.GetVersion();

        [ObservableProperty]
        private string _appTitle = "Data Transfer Application";

        [ObservableProperty]
        private string _retentionStatus = "Idle";

        [ObservableProperty]
        private string _appDescription = "Collateral L2H Data Transfer Application";

        [ObservableProperty]
        private string _appFooter = $"Data Transfer Application ({VersionHelper.GetVersionWithPrefix()})";

        [ObservableProperty]
        private string _currentDateTime = DateTime.Now.ToString("MMMM dd, yyyy hh:mm tt", CultureInfo.CurrentCulture);

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

            // Initialize drive detection timer (check every 10 seconds)
            _driveDetectionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            _driveDetectionTimer.Tick += (s, e) => { _ = DetectDrives(); };
            _driveDetectionTimer.Start();

            // Initialize time update timer (update every second)
            _timeUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timeUpdateTimer.Tick += (s, e) => CurrentDateTime = DateTime.Now.ToString("MMMM dd, yyyy hh:mm tt", CultureInfo.CurrentCulture);
            _timeUpdateTimer.Start();

            // Load initial data
            _ = LoadDataAsync();

            // Initial drive detection
            _ = DetectDrives();

            // Initialize commands
            RunRetentionCleanupAsyncCommand = new AsyncRelayCommand(RunRetentionCleanupAsync, () => !IsRetentionCleanupRunning);
        }

        private enum DriveAction
        {
            Append,
            Clear,
            Abort
        }

        public IAsyncRelayCommand RunRetentionCleanupAsyncCommand { get; private set; }

        public AppSettings Settings => _settings;

        public async Task RunRetentionCleanupAsync()
        {
            LoggingService.Info("RunRetentionCleanupAsync command triggered");
            RetentionStatus = "Running...";
            IsRetentionCleanupRunning = true;
            try
            {
                await _transferService.CleanupRetentionAsync();
                RetentionStatus = "Idle";
            }
            catch (Exception ex)
            {
                RetentionStatus = "Error";
                _ = ShowSnackbar($"Retention cleanup failed: {ex.Message}", "error");
                LoggingService.Error("Retention cleanup failed", ex);
            }
            finally
            {
                IsRetentionCleanupRunning = false;
            }
        }

        private static void UpdateFolderAuditStatuses(FolderData folder, AuditResult result)
        {
            // Combine naming and dataset validation into naming status
            bool namingPassed = result.NameValidation?.IsValid == true && result.DatasetValidation?.IsValid == true;
            folder.NamingAuditStatus = namingPassed ? "Passed" : "Failed";

            // Combine failure reasons
            var namingReasons = new List<string>();
            if (result.NameValidation?.IsValid == false)
            {
                namingReasons.Add(result.NameValidation.Message);
            }

            if (result.DatasetValidation?.IsValid == false)
            {
                namingReasons.Add(result.DatasetValidation.Message);
            }

            folder.NamingFailureReason = string.Join(". ", namingReasons);

            folder.DatasetAuditStatus = result.DatasetValidation?.IsValid == true ? "Passed" : "Failed";
            folder.DatasetFailureReason = result.DatasetValidation?.IsValid == true ? string.Empty : result.DatasetValidation?.Message ?? "Unknown dataset error";
            folder.BlacklistAuditStatus = result.ExtensionValidation?.IsValid == true ? "Passed" : "Failed";
            folder.BlacklistViolationCount = result.ExtensionValidation?.Violations.Count ?? 0;
        }

        private static void CountAndUpdateCompressedFiles(FolderData folder)
        {
            var compressedCount = folder.Files.Count(f =>
                AppConstants.CompressedFileExtensions.Contains(f.Extension.ToLowerInvariant()) ||
                f.FileName.ToLowerInvariant().EndsWith(".tar.gz", StringComparison.Ordinal) || f.FileName.ToLowerInvariant().EndsWith(".tgz", StringComparison.Ordinal) ||
                f.FileName.ToLowerInvariant().EndsWith(".tar.xz", StringComparison.Ordinal) || f.FileName.ToLowerInvariant().EndsWith(".txz", StringComparison.Ordinal) ||
                f.FileName.ToLowerInvariant().EndsWith(".tar.bz2", StringComparison.Ordinal) || f.FileName.ToLowerInvariant().EndsWith(".tbz2", StringComparison.Ordinal));
            folder.CompressedFileCount = compressedCount;
            folder.CompressedAuditStatus = compressedCount > 0 ? "Caution" : "Passed";
        }

        private static void DetermineFolderOverallStatus(FolderData folder, AuditResult result)
        {
            // Determine overall status: if all checks pass but there are compressed files, set to Caution
            if (result.OverallStatus == "Passed" && folder.CompressedFileCount > 0)
            {
                folder.AuditStatus = "Caution";
            }
            else
            {
                folder.AuditStatus = result.OverallStatus;
            }
        }

        private static void UpdateFileStatuses(FolderData folder, AuditResult result)
        {
            // Reset file flags and errors
            foreach (var file in folder.Files)
            {
                file.IsBlacklisted = false;
                file.IsCompressed = false;
                file.Status = "Ready";
                file.ErrorMessage = null;
                file.ErrorDetails = null;
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
                        file.ErrorMessage = $"Extension '{violation.Extension}' is not allowed for transfer";
                        file.ErrorDetails = $"This file extension is blacklisted. Remove this folder from the working directory or override.";
                        file.RecommendedActions = new List<string>
                        {
                            "Do not transfer this folder",
                            "Remove the blacklisted file from the folder",
                            "Follow organizational procedures for handling blacklisted contents"
                        };
                    }
                }
            }

            // Mark compressed files
            foreach (var file in folder.Files)
            {
                var fileName = file.FileName.ToLowerInvariant();
                if (AppConstants.CompressedFileExtensions.Contains(file.Extension.ToLowerInvariant()) ||
                    Array.Exists(AppConstants.MultiPartCompressedExtensions, ext => fileName.EndsWith(ext, StringComparison.Ordinal)))
                {
                    file.IsCompressed = true;
                    if (file.Status == "Ready")
                    {
                        file.Status = "Compressed";
                        file.ErrorMessage = $"Compressed archive detected: {file.Extension}. Review with the view 👁️ button recommended.";
                        file.ErrorDetails = "Use the view button to inspect contents of compressed files, or verify archive integrity before transfer if possible.";
                        file.RecommendedActions = new List<string>
                        {
                            "Inspect archive contents before transfer",
                            "Verify the archive is not password protected",
                            "Verify the archive is not corrupted",
                            "Check if files inside meet all requirements",
                            "Follow organizational procedures for handling compressed contents"
                        };
                    }
                }
            }
        }

        private static string DetermineOverallAuditStatus(AuditResult result, int compressedCount)
        {
            // Determine overall status: if all checks pass but there are compressed files, set to Caution
            if (result.OverallStatus == "Passed" && compressedCount > 0)
            {
                return "Caution";
            }
            else
            {
                return result.OverallStatus;
            }
        }

        partial void OnIsRetentionCleanupRunningChanged(bool value)
        {
            RunRetentionCleanupAsyncCommand?.NotifyCanExecuteChanged();
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

        partial void OnSelectedDriveChanged(RemovableDrive? value)
        {
            // Notify transfer commands to re-evaluate when selected drive changes
            TransferFolderCommand.NotifyCanExecuteChanged();
            TransferFolderWithOverrideCommand.NotifyCanExecuteChanged();
            TransferAllFoldersCommand.NotifyCanExecuteChanged();
            ClearDriveCommand.NotifyCanExecuteChanged();
        }

        partial void OnIsProcessingChanged(bool value)
        {
            // Notify all commands to re-evaluate when processing state changes
            AuditFolderCommand.NotifyCanExecuteChanged();
            TransferFolderCommand.NotifyCanExecuteChanged();
            TransferFolderWithOverrideCommand.NotifyCanExecuteChanged();
            TransferAllFoldersCommand.NotifyCanExecuteChanged();
            ClearDriveCommand.NotifyCanExecuteChanged();
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
                AutoSelectFirstFolder();

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
        private async Task DetectDrives()
        {
            try
            {
                var drives = await Task.Run(() => _transferService.GetRemovableDrives());
                var currentDriveLetters = RemovableDrives.Select(d => d.DriveLetter).ToList();
                var newDriveLetters = drives.Select(d => d.DriveLetter).ToList();

                // Only update if drives changed
                if (!currentDriveLetters.SequenceEqual(newDriveLetters))
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
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
                    });

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
            if (SelectedFolder == null)
            {
                return;
            }

            IsProcessing = true;
            StatusMessage = $"Auditing {SelectedFolder.FolderName}...";

            try
            {
                var result = await _auditService.AuditFolderAsync(
                    SelectedFolder.FolderPath,
                    SelectedFolder.FolderName);

                SelectedFolder.AuditResult = result;

                UpdateAuditResults(result);
                var compressedCount = CountCompressedFiles();
                SelectedFolder.AuditStatus = DetermineOverallAuditStatus(result, compressedCount);
                UpdateFileStatuses(result);
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

        private void UpdateAuditResults(AuditResult result)
        {
            // Update individual audit statuses
            // Combine naming and dataset validation into naming status
            bool namingPassed = result.NameValidation?.IsValid == true && result.DatasetValidation?.IsValid == true;
            SelectedFolder!.NamingAuditStatus = namingPassed ? "Passed" : "Failed";

            // Combine failure reasons
            var namingReasons = new List<string>();
            if (result.NameValidation?.IsValid == false)
            {
                namingReasons.Add(result.NameValidation.Message);
            }

            if (result.DatasetValidation?.IsValid == false)
            {
                namingReasons.Add(result.DatasetValidation.Message);
            }

            SelectedFolder.NamingFailureReason = string.Join(". ", namingReasons);

            SelectedFolder.DatasetAuditStatus = result.DatasetValidation?.IsValid == true ? "Passed" : "Failed";
            SelectedFolder.DatasetFailureReason = result.DatasetValidation?.IsValid == true ? string.Empty : result.DatasetValidation?.Message ?? "Unknown dataset error";
            SelectedFolder.BlacklistAuditStatus = result.ExtensionValidation?.IsValid == true ? "Passed" : "Failed";
            SelectedFolder.BlacklistViolationCount = result.ExtensionValidation?.Violations.Count ?? 0;
        }

        private int CountCompressedFiles()
        {
            var compressedCount = SelectedFolder!.Files.Count(f =>
                AppConstants.CompressedFileExtensions.Contains(f.Extension.ToLowerInvariant()) ||
                f.FileName.ToLowerInvariant().EndsWith(".tar.gz", StringComparison.Ordinal) || f.FileName.ToLowerInvariant().EndsWith(".tgz", StringComparison.Ordinal) ||
                f.FileName.ToLowerInvariant().EndsWith(".tar.xz", StringComparison.Ordinal) || f.FileName.ToLowerInvariant().EndsWith(".txz", StringComparison.Ordinal) ||
                f.FileName.ToLowerInvariant().EndsWith(".tar.bz2", StringComparison.Ordinal) || f.FileName.ToLowerInvariant().EndsWith(".tbz2", StringComparison.Ordinal));
            SelectedFolder.CompressedFileCount = compressedCount;
            SelectedFolder.CompressedAuditStatus = compressedCount > 0 ? "Caution" : "Passed";
            return compressedCount;
        }

        private void UpdateFileStatuses(AuditResult result)
        {
            // Update file statuses and flags
            // Reset all files first
            foreach (var file in SelectedFolder!.Files)
            {
                file.IsBlacklisted = false;
                file.IsCompressed = false;
                file.Status = "Ready";
                file.ErrorMessage = null;
                file.ErrorDetails = null;
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
                        file.ErrorMessage = $"Extension '{violation.Extension}' is not allowed for transfer";
                        file.ErrorDetails = $"This file extension is blacklisted. Remove the file or convert to an allowed format before transfer.";
                        file.RecommendedActions = new List<string>
                        {
                            "Do not transfer this folder",
                            "Remove the blacklisted file from the folder",
                            "Follow organizational procedures for handling blacklisted contents"
                        };
                    }
                }
            }

            // Mark compressed files
            foreach (var file in SelectedFolder.Files)
            {
                var fileName = file.FileName.ToLowerInvariant();
                if (AppConstants.CompressedFileExtensions.Contains(file.Extension.ToLowerInvariant()) ||
                    Array.Exists(AppConstants.MultiPartCompressedExtensions, ext => fileName.EndsWith(ext, StringComparison.Ordinal)))
                {
                    file.IsCompressed = true;
                    if (file.Status == "Ready")
                    {
                        file.Status = "Compressed";
                        file.ErrorMessage = $"Compressed archive detected: {file.Extension}. Review with the view 👁️ button recommended.";
                        file.ErrorDetails = "Use the view button to inspect contents of compressed files, or verify archive integrity before transfer if possible.";
                        file.RecommendedActions = new List<string>
                        {
                            "Inspect archive contents before transfer",
                            "Verify the archive is not corrupted",
                            "Check if files inside meet all requirements",
                            "Follow organizational procedures for handling compressed contents"
                        };
                    }
                }
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

                    await ProcessSingleFolderAuditAsync(folder);
                }

                UpdateStatistics();
                StatusMessage = $"Audit complete: {ReadyFolders} passed, {CautionFolders} caution, {FailedFolders} failed";
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

        private async Task ProcessSingleFolderAuditAsync(FolderData folder)
        {
            var result = await _auditService.AuditFolderAsync(folder.FolderPath, folder.FolderName);
            folder.AuditResult = result;

            UpdateFolderAuditStatuses(folder, result);
            CountAndUpdateCompressedFiles(folder);
            DetermineFolderOverallStatus(folder, result);
            UpdateFileStatuses(folder, result);
        }

        [RelayCommand(CanExecute = nameof(CanTransferAllFolders))]
        private async Task TransferAllFoldersAsync()
        {
            if (!ValidateTransferPrerequisites(out var passedFolders))
            {
                return;
            }

            var action = await HandleDrivePreparationAsync();
            if (action == DriveAction.Abort)
            {
                return;
            }

            await ProcessFolderTransfersAsync(passedFolders);
        }

        private bool ValidateTransferPrerequisites(out List<FolderData> passedFolders)
        {
            passedFolders = null!;

            if (SelectedDrive == null)
            {
                _ = ShowSnackbar("Please select a destination drive", "error");
                return false;
            }

            passedFolders = FolderList.Where(f => f.CanTransfer).ToList();
            if (!passedFolders.Any())
            {
                _ = ShowSnackbar("No folders passed audit. Run audit first.", "warning");
                return false;
            }

            return true;
        }

        private async Task<DriveAction> HandleDrivePreparationAsync()
        {
            var action = await CheckDriveContentsAsync();
            if (action == DriveAction.Clear)
            {
                // Clear drive first (no confirmation needed - user already confirmed in Task Dialog)
                await ClearDriveInternalAsync(showConfirmation: false);
                if (IsProcessing)
                {
                    return DriveAction.Abort; // If clear failed or is still running
                }
            }

            return action;
        }

        private async Task ProcessFolderTransfersAsync(List<FolderData> passedFolders)
        {
            IsProcessing = true;
            _transferCancellationTokenSource = new CancellationTokenSource();
            CancelTransferCommand.NotifyCanExecuteChanged();
            var total = passedFolders.Count;
            var completed = 0;
            var failed = 0;
            var skipped = 0;

            try
            {
                // ToList to avoid collection modification issues
                foreach (var folder in passedFolders.ToList())
                {
                    // Check for cancellation before starting next folder
                    _transferCancellationTokenSource.Token.ThrowIfCancellationRequested();

                    completed++;
                    StatusMessage = $"Transferring {completed}/{total}: {folder.FolderName}";

                    // Don't pass cancellation token - let current folder complete
                    var transferResult = await TransferSingleFolderAsync(folder, completed, total);

                    if (transferResult.Success)
                    {
                        if (transferResult.ErrorMessage == "Skipped - folder already exists")
                        {
                            skipped++;
                            LoggingService.Info($"Skipped existing folder: {folder.FolderName}");
                        }

                        TransferredList.Add(folder);
                        FolderList.Remove(folder);

                        // Update statistics after each folder transfer so cards update in real-time
                        UpdateStatistics();
                    }
                    else
                    {
                        failed++;
                        LoggingService.Warning($"Transfer failed for {folder.FolderName}: {transferResult.ErrorMessage}");
                    }
                }

                UpdateTransferResults(total, completed, failed, skipped);
            }
            catch (OperationCanceledException)
            {
                // Update statistics when cancelled so cards reflect actual state
                UpdateStatistics();
                StatusMessage = $"Transfer cancelled - {completed} of {total} completed";
                LoggingService.Warning($"Batch transfer cancelled by user. Completed {completed} of {total} folders");
                _ = ShowSnackbar($"Transfer cancelled. {completed} of {total} folders completed.", "warning");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Transfer All error: {ex.Message}";
                LoggingService.Error("Transfer all error", ex);
                _ = ShowSnackbar($"Transfer All failed: {ex.Message}", "error");
            }
            finally
            {
                IsProcessing = false;
                IsTransferActive = false;
                ProgressPercent = 0;
                ProgressText = "Ready";
                ProgressIssues = "Idle";
                _transferCancellationTokenSource?.Dispose();
                _transferCancellationTokenSource = null;
                CancelTransferCommand.NotifyCanExecuteChanged();
            }
        }

        private async Task<TransferResult> TransferSingleFolderAsync(FolderData folder, int completed, int total, CancellationToken cancellationToken = default)
        {
            var progress = new Progress<TransferProgress>(p =>
            {
                ProgressText = $"[{completed}/{total}] {p.CurrentFile} ({p.CompletedFiles}/{p.TotalFiles})";
                StatusMessage = $"Copying: {folder.FolderName}";
                ProgressPercent = p.PercentComplete;
                UpdateTransferStatus(p);
            });

            return await _transferService.TransferFolderAsync(
                folder,
                SelectedDrive!.DriveLetter,
                progress,
                cancellationToken);
        }

        private void UpdateTransferResults(int total, int completed, int failed, int skipped)
        {
            UpdateStatistics();
            var successCount = completed - failed;
            StatusMessage = $"Transfer All complete: {successCount} succeeded, {failed} failed, {skipped} skipped";
            _ = ShowSnackbar($"Transferred {successCount} of {total} folders ({skipped} skipped)", failed > 0 ? "warning" : "success");

            // Auto-select first folder if available after transfers
            AutoSelectFirstFolder();
        }

        [RelayCommand(CanExecute = nameof(CanTransferFolder))]
        private async Task TransferFolderAsync()
        {
            if (!ValidateSingleTransferPrerequisites(out var folderName))
            {
                return;
            }

            var action = await HandleDrivePreparationAsync();
            if (action == DriveAction.Abort)
            {
                return;
            }

            await ProcessSingleTransferAsync(folderName);
        }

        private bool ValidateSingleTransferPrerequisites(out string folderName)
        {
            folderName = null!;

            if (SelectedFolder == null || SelectedDrive == null)
            {
                return false;
            }

            folderName = SelectedFolder.FolderName; // Store name before folder is removed
            return true;
        }

        private async Task ProcessSingleTransferAsync(string folderName)
        {
            IsProcessing = true;
            ProgressPercent = 0;
            _transferCancellationTokenSource = new CancellationTokenSource();
            CancelTransferCommand.NotifyCanExecuteChanged();

            try
            {
                // Mark transfer as starting
                IsTransferActive = true;
                ProgressIssues = _settings.UseRoboSharp ? "Robocopy" : "Legacy";

                var progress = new Progress<TransferProgress>(p =>
                {
                    ProgressText = $"Transferring: {p.CurrentFile} ({p.CompletedFiles}/{p.TotalFiles})";
                    StatusMessage = $"Copying: {folderName}";
                    ProgressPercent = p.PercentComplete;
                    UpdateTransferStatus(p);
                });

                var result = await _transferService.TransferFolderAsync(
                    SelectedFolder!,
                    SelectedDrive!.DriveLetter,
                    progress,
                    _transferCancellationTokenSource.Token);

                UpdateSingleTransferResults(result, folderName);
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Transfer cancelled";
                LoggingService.Warning($"Transfer cancelled by user: {folderName}");
                _ = ShowSnackbar("Transfer cancelled by user", "warning");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Transfer error: {ex.Message}";
                LoggingService.Error("Transfer error", ex);
                _ = ShowSnackbar($"Transfer error: {ex.Message}", "error");
            }
            finally
            {
                IsProcessing = false;
                IsTransferActive = false;
                ProgressPercent = 0;
                ProgressText = "Ready";
                ProgressIssues = "Idle";
                _transferCancellationTokenSource?.Dispose();
                _transferCancellationTokenSource = null;
                CancelTransferCommand.NotifyCanExecuteChanged();
            }
        }

        private void UpdateTransferStatus(TransferProgress progress)
        {
            // Only activate transfer if not already active and not a completion update
            // This prevents race condition where completion progress arrives after finally block
            if (!IsTransferActive && !progress.IsCompleted)
            {
                IsTransferActive = true;
            }

            var engine = _settings.UseRoboSharp ? "Robocopy" : "Legacy";

            if (progress.BytesPerSecond > 0)
            {
                var speedMBps = progress.MBPerSecond;
                string eta;

                if (progress.EstimatedTimeRemaining.HasValue)
                {
                    var timeRemaining = progress.EstimatedTimeRemaining.Value;

                    // Only show "Complete" when OnCommandCompleted has fired
                    if (progress.IsCompleted && timeRemaining == TimeSpan.Zero)
                    {
                        eta = "Moving folder...";
                    }
                    else if (timeRemaining.TotalHours >= 1)
                    {
                        // Show hours for long transfers
                        eta = timeRemaining.ToString(@"h\:mm\:ss", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        // Show minutes:seconds for shorter transfers
                        eta = timeRemaining.ToString(@"mm\:ss", System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    // Show "--:--" when ETA is not yet available
                    eta = "--:--";
                }

                if (eta == "Moving folder...")
                {
                    ProgressIssues = $"{engine} • {speedMBps:F1} MB/s • {eta}";
                }
                else
                {
                    ProgressIssues = $"{engine} • {speedMBps:F1} MB/s • ETA {eta}";
                }
            }
            else if (progress.CompletedFiles > 0 || progress.BytesTransferred > 0)
            {
                // Show progress even without speed calculation
                var percentCompleted = progress.PercentComplete;
                ProgressIssues = $"{engine} • Transferring... {percentCompleted}%";
            }
            else
            {
                ProgressIssues = $"{engine} • Starting...";
            }
        }

        private void UpdateSingleTransferResults(TransferResult result, string folderName)
        {
            if (result.Success)
            {
                TransferredList.Add(SelectedFolder!);
                FolderList.Remove(SelectedFolder!);
                UpdateStatistics();

                var message = result.ErrorMessage == "Skipped - folder already exists"
                    ? $"Skipped (already exists): {folderName}"
                    : $"Transfer complete: {folderName}";

                StatusMessage = message;
                _ = ShowSnackbar(message, result.ErrorMessage != null ? "info" : "success");

                // Auto-select first folder if available after transfer
                AutoSelectFirstFolder();
            }
            else
            {
                StatusMessage = $"Transfer failed: {result.ErrorMessage}";
                _ = ShowSnackbar($"Transfer failed: {result.ErrorMessage}", "error");
            }
        }

        private bool CanTransferFolder() =>
            SelectedFolder != null &&
            SelectedFolder.CanTransfer &&
            SelectedDrive != null &&
            !IsProcessing;

        private bool CanTransferAllFolders() =>
            SelectedDrive != null && !IsProcessing; // Only enabled when not processing

        [RelayCommand(CanExecute = nameof(CanCancelTransfer))]
        private void CancelTransfer()
        {
            if (_transferCancellationTokenSource != null && !_transferCancellationTokenSource.Token.IsCancellationRequested)
            {
                _transferCancellationTokenSource.Cancel();
                StatusMessage = "Cancelling transfer...";
                LoggingService.Warning("Transfer cancellation requested by user");
            }
        }

        private bool CanCancelTransfer() => IsProcessing && _transferCancellationTokenSource != null && !_transferCancellationTokenSource.Token.IsCancellationRequested;

        [RelayCommand(CanExecute = nameof(CanTransferWithOverride))]
        private async Task TransferFolderWithOverrideAsync()
        {
            if (!ValidateSingleTransferPrerequisites(out var folderName))
            {
                return;
            }

            if (!ShowOverrideConfirmation(folderName))
            {
                return;
            }

            var action = await HandleDrivePreparationAsync();
            if (action == DriveAction.Abort)
            {
                return;
            }

            await ProcessOverrideTransferAsync(folderName);
        }

        private bool ShowOverrideConfirmation(string folderName)
        {
            if (Application.Current.MainWindow is Views.MainWindow mainWindow)
            {
                return mainWindow.ShowOverrideAuditDialog(folderName, SelectedFolder!.AuditStatus);
            }

            // Fallback to MessageBox if MainWindow is not available
            var result = MessageBox.Show(
                $"This folder has failed audit. Are you sure you want to transfer '{folderName}' anyway?\n\nAudit Status: {SelectedFolder!.AuditStatus}",
                "Override Audit - Confirm Transfer",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            return result == MessageBoxResult.Yes;
        }

        private async Task ProcessOverrideTransferAsync(string folderName)
        {
            IsProcessing = true;
            ProgressPercent = 0;
            _transferCancellationTokenSource = new CancellationTokenSource();
            CancelTransferCommand.NotifyCanExecuteChanged();

            try
            {
                // Mark transfer as starting
                IsTransferActive = true;
                ProgressIssues = _settings.UseRoboSharp ? "Robocopy" : "Legacy";

                var progress = new Progress<TransferProgress>(p =>
                {
                    ProgressText = $"Transferring (Override): {p.CurrentFile} ({p.CompletedFiles}/{p.TotalFiles})";
                    StatusMessage = $"Copying (Override): {folderName}";
                    ProgressPercent = p.PercentComplete;
                    UpdateTransferStatus(p);
                });

                var transferResult = await _transferService.TransferFolderAsync(
                    SelectedFolder!,
                    SelectedDrive!.DriveLetter,
                    progress,
                    _transferCancellationTokenSource.Token);

                UpdateOverrideResults(transferResult, folderName);
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Transfer cancelled";
                LoggingService.Warning($"Override transfer cancelled by user: {folderName}");
                _ = ShowSnackbar("Transfer cancelled by user", "warning");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Transfer error: {ex.Message}";
                LoggingService.Error("Transfer override error", ex);
                _ = ShowSnackbar($"Transfer error: {ex.Message}", "error");
            }
            finally
            {
                IsProcessing = false;
                IsTransferActive = false;
                ProgressPercent = 0;
                ProgressText = "Ready";
                ProgressIssues = "Idle";
                _transferCancellationTokenSource?.Dispose();
                _transferCancellationTokenSource = null;
                CancelTransferCommand.NotifyCanExecuteChanged();
            }
        }

        private void UpdateOverrideResults(TransferResult transferResult, string folderName)
        {
            if (transferResult.Success)
            {
                TransferredList.Add(SelectedFolder!);
                FolderList.Remove(SelectedFolder!);
                UpdateStatistics();
                StatusMessage = $"Transfer complete (Override): {folderName}";
                _ = ShowSnackbar($"Override transfer completed", "warning");

                // Auto-select first folder if available after transfer
                AutoSelectFirstFolder();
            }
            else
            {
                StatusMessage = $"Transfer failed: {transferResult.ErrorMessage}";
                _ = ShowSnackbar($"Transfer failed: {transferResult.ErrorMessage}", "error");
            }
        }

        private bool CanTransferWithOverride() =>
            SelectedFolder != null &&
            !SelectedFolder.CanTransfer &&
            SelectedDrive != null &&
            !IsProcessing;

        private bool CanClearDrive() => SelectedDrive != null && !IsProcessing;

        [RelayCommand(CanExecute = nameof(CanClearDrive))]
        private async Task ClearDriveAsync()
        {
            await ClearDriveInternalAsync(showConfirmation: true);
        }

#pragma warning disable MA0051 // Method is too long
        private async Task ClearDriveInternalAsync(bool showConfirmation)
#pragma warning restore MA0051 // Method is too long
        {
            if (SelectedDrive == null)
            {
                return;
            }

            if (showConfirmation)
            {
                bool userConfirmed;

                using (TaskDialog dialog = new TaskDialog())
                {
                    dialog.WindowTitle = "Confirm Clear Drive";
                    dialog.Width = 300;
                    dialog.MainInstruction = $"Clear all data from {SelectedDrive.DisplayText}?";
                    dialog.Content = "This action will permanently delete all files and folders on the selected drive.\n\nThis operation cannot be undone.";
                    dialog.MainIcon = TaskDialogIcon.Warning;
                    dialog.ButtonStyle = TaskDialogButtonStyle.CommandLinks;

                    var clearButton = new TaskDialogButton("Clear Drive");
                    clearButton.CommandLinkNote = "Delete all files and folders from the drive";
                    dialog.Buttons.Add(clearButton);

                    var cancelButton = new TaskDialogButton(ButtonType.Cancel);
                    dialog.Buttons.Add(cancelButton);

                    var result = dialog.ShowDialog();
                    userConfirmed = result == clearButton;

                    LoggingService.Info($"Clear drive confirmation: {(userConfirmed ? "Confirmed" : "Cancelled")} for drive {SelectedDrive.DriveLetter}");
                }

                if (!userConfirmed)
                {
                    return;
                }
            }

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
                    ProgressIssues = $"Deleting items...";
                });

                await _transferService.ClearDriveAsync(SelectedDrive.DriveLetter, progress);

                StatusMessage = $"Drive cleared: {SelectedDrive.DriveLetter}";
                _ = ShowSnackbar($"Drive cleared successfully: {SelectedDrive.DriveLetter}", "success");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error clearing drive: {ex.Message}";
                _ = ShowSnackbar($"Error clearing drive: {ex.Message}", "error");
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
                IsTransferActive = false;
                ProgressPercent = 0;
                ProgressText = "Ready";
                ProgressIssues = "Idle";
            }
        }

        [RelayCommand]
        private void ViewFileContents(FileData? file)
        {
            if (file == null)
            {
                return;
            }

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
                    var content = FileService.ReadTextFile(file.FullPath);
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
                    _ = ShowSnackbar("Settings saved successfully!", "success");
                    LoggingService.Info("Settings updated successfully");
                }
            }
            catch (Exception ex)
            {
                _ = ShowSnackbar($"Error opening settings: {ex.Message}", "error");
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
                _ = ShowSnackbar($"Error opening about: {ex.Message}", "error");
                LoggingService.Error("Error opening about", ex);
            }
        }

        [RelayCommand]
        private void OpenChanges()
        {
            try
            {
                var changesWindow = new ChangesWindow();
                changesWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                _ = ShowSnackbar($"Error opening changes: {ex.Message}", "error");
                LoggingService.Error("Error opening changes", ex);
            }
        }

        [RelayCommand]
        private void OpenHelp()
        {
            try
            {
                var helpWindow = new HelpWindow();
                helpWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                _ = ShowSnackbar($"Error opening help: {ex.Message}", "error");
                LoggingService.Error("Error opening help", ex);
            }
        }

        [RelayCommand]
        private void ViewTransferHistory()
        {
            try
            {
                var databasePath = string.IsNullOrWhiteSpace(_settings.TransferHistoryDatabasePath)
                    ? null
                    : _settings.TransferHistoryDatabasePath;
                var historyWindow = new TransferHistoryWindow(databasePath);
                historyWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                _ = ShowSnackbar($"Error opening transfer history: {ex.Message}", "error");
                LoggingService.Error("Error opening transfer history", ex);
            }
        }

        private void AutoSelectFirstFolder()
        {
            // Auto-select first folder if available
            if (FolderList.Count > 0)
            {
                SelectedFolder = FolderList[0];
            }
        }

        private async Task ShowSnackbar(string message, string type = "success")
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

            await Task.Delay(4000);
            IsSnackbarVisible = false;
        }

        private async Task<DriveAction> CheckDriveContentsAsync()
        {
            if (SelectedDrive == null)
            {
                return DriveAction.Append;
            }

            if (TransferredCount == 0 && TransferService.DriveHasContents(SelectedDrive.DriveLetter))
            {
                var driveCount = TransferService.GetTransferredFolderCount(SelectedDrive.DriveLetter);

                // Get the MainWindow to show the Task Dialog
                if (Application.Current.MainWindow is Views.MainWindow mainWindow)
                {
                    var result = mainWindow.ShowDriveContentsDialog(
                        SelectedDrive.DriveLetter,
                        driveCount);

                    // Map DriveContentAction to DriveAction
                    return result switch
                    {
                        DriveContentAction.Append => DriveAction.Append,
                        DriveContentAction.Clear => DriveAction.Clear,
                        DriveContentAction.Cancel => DriveAction.Abort,
                        _ => DriveAction.Abort
                    };
                }
                else
                {
                    // Fallback to MessageBox if MainWindow is not available (shouldn't happen in normal operation)
                    var messageResult = MessageBox.Show(
                        $"The drive {SelectedDrive.DriveLetter} already contains {driveCount} folder(s).\n\n" +
                        "Do you want to APPEND to existing contents?\n\n" +
                        "YES = Append/Add to existing\n" +
                        "NO = Clear drive first\n" +
                        "CANCEL = Abort transfer",
                        "Drive Contains Data",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);

                    if (messageResult == MessageBoxResult.Cancel)
                    {
                        return DriveAction.Abort;
                    }
                    else if (messageResult == MessageBoxResult.No)
                    {
                        return DriveAction.Clear;
                    }
                    else
                    {
                        return DriveAction.Append;
                    }
                }
            }
            else
            {
                return DriveAction.Append;
            }
        }

        private void UpdateStatistics()
        {
            TotalFolders = FolderList.Count;
            ReadyFolders = FolderList.Count(f => f.AuditStatus == "Passed");
            CautionFolders = FolderList.Count(f => f.AuditStatus == "Caution");
            FailedFolders = FolderList.Count(f => f.AuditStatus == "Failed");
            TransferredCount = TransferredList.Count;
            TotalSize = FileSizeHelper.FormatFileSize(FolderList.Sum(f => f.TotalSize));
        }
    }
}
