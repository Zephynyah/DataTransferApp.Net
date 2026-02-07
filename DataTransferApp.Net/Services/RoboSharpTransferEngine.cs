using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataTransferApp.Net.Models;
using RoboSharp;
using RoboSharp.EventArgObjects;
using RoboSharp.Interfaces;

namespace DataTransferApp.Net.Services
{
    /// <summary>
    /// High-performance file transfer engine using RoboSharp (Robocopy wrapper).
    /// Provides multithreaded, resilient file transfer operations with detailed progress tracking.
    /// </summary>
    public class RoboSharpTransferEngine : IRoboSharpTransferEngine
    {
        /// <summary>
        /// Event raised when an error occurs during transfer.
        /// </summary>
        public event EventHandler<RoboSharpErrorEventArgs>? OnError;

        /// <summary>
        /// Event raised when transfer completes.
        /// </summary>
        public event EventHandler<RoboSharpTransferResultEventArgs>? OnCompleted;

        /// <summary>
        /// Transfers an entire folder and its contents from source to destination.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<RoboSharpTransferResult> TransferFolderAsync(
            string sourcePath,
            string destinationPath,
            RoboSharpOptions options,
            IProgress<TransferProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.Now;

            try
            {
                LoggingService.Info($"Starting RoboSharp transfer: {sourcePath} -> {destinationPath}");
                LoggingService.Debug($"Options: Threads={options.ThreadCount}, Retries={options.RetryCount}");

                // Validate paths
                if (!Directory.Exists(sourcePath))
                {
                    throw new DirectoryNotFoundException($"Source directory not found: {sourcePath}");
                }

                // Create destination directory if needed
                Directory.CreateDirectory(destinationPath);

                // Create and configure RoboCommand
                var command = CreateRoboCommand(sourcePath, destinationPath, options);

                // Set up progress tracking with accurate totals from list-only scan
                var progressAdapter = await SetupProgressTrackingAsync(sourcePath, destinationPath, options, command, progress, cancellationToken);

                // Start the transfer - StartAsync() returns Task<RoboCopyResults>
                progressAdapter.Start();
                var roboResult = await command.StartAsync();

                // Build result from RoboCopyResults
                var result = BuildResult(roboResult, sourcePath, destinationPath, startTime, options);

                LoggingService.Info($"RoboSharp transfer completed: ExitCode={roboResult.Status.ExitCodeValue}, Files={roboResult.FilesStatistic.Copied}");

                OnCompleted?.Invoke(this, new RoboSharpTransferResultEventArgs(result));
                return result;
            }
            catch (OperationCanceledException)
            {
                return HandleCancellation(sourcePath, destinationPath, startTime);
            }
            catch (Exception ex)
            {
                return HandleTransferException(ex, sourcePath, destinationPath, startTime);
            }
        }

        /// <summary>
        /// Transfers specific files from source root to destination.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<RoboSharpTransferResult> TransferFilesAsync(
            string[] filePaths,
            string sourceRoot,
            string destinationPath,
            RoboSharpOptions options,
            IProgress<TransferProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.Now;

            try
            {
                LoggingService.Info($"Starting RoboSharp file transfer: {filePaths.Length} files from {sourceRoot} -> {destinationPath}");

                if (filePaths.Length == 0)
                {
                    return RoboSharpTransferResult.CreateSuccess(sourceRoot, destinationPath, 0, 0, startTime, DateTime.Now);
                }

                // For selective file transfer, we'll use RoboSharp with file filters
                // Extract unique file names
                var fileNames = filePaths.Select(Path.GetFileName).Where(f => !string.IsNullOrEmpty(f)).Cast<string>().ToList();

                // Configure options to include only these files
                var fileOptions = new RoboSharpOptions
                {
                    ThreadCount = options.ThreadCount,
                    RetryCount = options.RetryCount,
                    RetryWaitSeconds = options.RetryWaitSeconds,
                    CopySubdirectories = true,
                    CopyEmptySubdirectories = false,
                    ContinueOnError = options.ContinueOnError,
                    IncludeFiles = fileNames
                };

                // Transfer using folder method with file filter
                return await TransferFolderAsync(sourceRoot, destinationPath, fileOptions, progress, cancellationToken);
            }
            catch (Exception ex)
            {
                LoggingService.Error("RoboSharp file transfer failed", ex);
                return RoboSharpTransferResult.CreateFailure(
                    sourceRoot,
                    destinationPath,
                    -1,
                    ex.Message,
                    startTime);
            }
        }

        /// <summary>
        /// Estimates transfer size and file count without actually copying.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<RoboSharpTransferResult> EstimateTransferAsync(
            string sourcePath,
            RoboSharpOptions options,
            CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.Now;

            try
            {
                LoggingService.Debug($"Estimating transfer from: {sourcePath}");

                // Create options with ListOnly mode
                var estimateOptions = new RoboSharpOptions
                {
                    ListOnly = true,
                    CopySubdirectories = options.CopySubdirectories,
                    CopyEmptySubdirectories = options.CopyEmptySubdirectories,
                    ExcludeFiles = options.ExcludeFiles,
                    ExcludeDirectories = options.ExcludeDirectories,
                    IncludeFiles = options.IncludeFiles
                };

                // Use a temporary destination for estimation
                var tempDest = Path.Combine(Path.GetTempPath(), "_robosharp_estimate_");

                var command = CreateRoboCommand(sourcePath, tempDest, estimateOptions);
                var roboResult = await command.StartAsync();

                var result = BuildResult(roboResult, sourcePath, tempDest, startTime, estimateOptions);
                LoggingService.Debug($"Estimate: {result.FilesScanned} files, {result.BytesTotal} bytes");

                return result;
            }
            catch (Exception ex)
            {
                LoggingService.Error("RoboSharp estimation failed", ex);
                return RoboSharpTransferResult.CreateFailure(
                    sourcePath,
                    string.Empty,
                    -1,
                    ex.Message,
                    startTime);
            }
        }

        /// <summary>
        /// Creates and configures a RoboCommand with the specified options.
        /// </summary>
        private static RoboCommand CreateRoboCommand(string sourcePath, string destinationPath, RoboSharpOptions options)
        {
            var command = new RoboCommand();

            ConfigureCopyOptions(command, sourcePath, destinationPath, options);
            ConfigureRetryOptions(command);
            ConfigureSelectionFilters(command, options);
            ConfigureLogging(command, options);

            return command;
        }

        private static void ConfigureCopyOptions(RoboCommand command, string sourcePath, string destinationPath, RoboSharpOptions options)
        {
            command.CopyOptions.Source = sourcePath;
            command.CopyOptions.Destination = destinationPath;
            command.CopyOptions.MultiThreadedCopiesCount = options.ThreadCount;
            command.CopyOptions.CopySubdirectories = options.CopySubdirectories;
            command.CopyOptions.CopySubdirectoriesIncludingEmpty = options.CopyEmptySubdirectories;
            command.CopyOptions.Purge = options.PurgeDestination;
            command.CopyOptions.Mirror = options.MirrorMode;
            command.CopyOptions.MoveFiles = options.MoveFiles;
            command.CopyOptions.MoveFilesAndDirectories = options.MoveTree;

            if (options.InterPacketGapMs > 0)
            {
                command.CopyOptions.InterPacketGap = options.InterPacketGapMs;
            }
        }

        private static void ConfigureRetryOptions(RoboCommand command)
        {
            // Disable RoboSharp's internal retry - we handle all retries at TransferService level
            // with exponential backoff for better resilience and predictable behavior
            command.RetryOptions.RetryCount = 0;
            command.RetryOptions.RetryWaitTime = 0;
        }

        private static void ConfigureSelectionFilters(RoboCommand command, RoboSharpOptions options)
        {
            if (options.ExcludeFiles != null && options.ExcludeFiles.Count > 0)
            {
                command.SelectionOptions.ExcludedFiles.AddRange(options.ExcludeFiles);
            }

            if (options.ExcludeDirectories != null && options.ExcludeDirectories.Count > 0)
            {
                command.SelectionOptions.ExcludedDirectories.AddRange(options.ExcludeDirectories);
            }
        }

        private static void ConfigureLogging(RoboCommand command, RoboSharpOptions options)
        {
            command.LoggingOptions.VerboseOutput = options.VerboseOutput;
            command.LoggingOptions.ListOnly = options.ListOnly;
            command.LoggingOptions.ShowEstimatedTimeOfArrival = true; // Enable RoboCopy's built-in ETA for files

            if (!string.IsNullOrEmpty(options.LogFilePath))
            {
                command.LoggingOptions.LogPath = options.LogFilePath;
            }
        }

        /// <summary>
        /// Handles operation cancellation during transfer.
        /// </summary>
        private static RoboSharpTransferResult HandleCancellation(string sourcePath, string destinationPath, DateTime startTime)
        {
            LoggingService.Warning("RoboSharp transfer cancelled by user");
            return RoboSharpTransferResult.CreateFailure(
                sourcePath,
                destinationPath,
                -1,
                "Transfer cancelled by user",
                startTime);
        }

        /// <summary>
        /// Pre-scans a directory to calculate total file count and size for accurate ETA.
        /// </summary>
        private static (int TotalFiles, long TotalBytes) CalculateDirectoryTotals(string path)
        {
            try
            {
                var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                var totalFiles = files.Length;
                var totalBytes = files.Sum(f => new FileInfo(f).Length);

                return (totalFiles, totalBytes);
            }
            catch (Exception ex)
            {
                LoggingService.Warning($"Failed to pre-scan directory for totals: {ex.Message}");
                return (0, 0); // Return zeros on error, RoboSharp's estimator will provide fallback
            }
        }

        /// <summary>
        /// Attaches event handlers to RoboCommand for progress and error tracking.
        /// </summary>
        private void AttachEventHandlers(RoboCommand command, RoboSharpProgressAdapter progressAdapter, CancellationToken cancellationToken)
        {
            // Progress events
            command.OnFileProcessed += progressAdapter.OnFileProcessed;
            command.OnProgressEstimatorCreated += progressAdapter.OnProgressEstimatorCreated;
            command.OnCopyProgressChanged += progressAdapter.OnCopyProgressChanged;

            // Error events
            command.OnError += (sender, e) =>
            {
                var error = new RoboSharpError
                {
                    ErrorType = RoboSharpErrorType.CopyError,
                    Message = e.Error ?? "Unknown error",
                    AdditionalInfo = e.ErrorDescription,
                    FilePath = e.ErrorPath ?? string.Empty,
                    Timestamp = e.DateTime,
                    IsRecoverable = true,
                    IsFatal = false
                };

                LoggingService.Warning($"RoboSharp error: {e.Error}");
                OnError?.Invoke(this, new RoboSharpErrorEventArgs(error, false));
            };

            // Completion event
            command.OnCommandCompleted += (sender, e) =>
            {
                progressAdapter.ReportComplete();
                LoggingService.Debug($"RoboSharp command completed with exit code: {e.Results.Status.ExitCodeValue}");
            };

            // Handle cancellation
            cancellationToken.Register(() =>
            {
                try
                {
                    command.Stop();
                    LoggingService.Info("RoboSharp command stopped due to cancellation");
                }
                catch (Exception ex)
                {
                    LoggingService.Warning($"Error stopping RoboSharp command: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Builds a RoboSharpTransferResult from RoboSharp's native result.
        /// </summary>
        private RoboSharpTransferResult BuildResult(
            RoboSharp.Results.RoboCopyResults roboResults,
            string sourcePath,
            string destinationPath,
            DateTime startTime,
            RoboSharpOptions options)
        {
            var endTime = DateTime.Now;
            var exitCode = roboResults.Status.ExitCodeValue;

            var result = new RoboSharpTransferResult
            {
                Success = exitCode <= 1, // 0 = no changes, 1 = files copied
                ExitCode = exitCode,
                SourcePath = sourcePath,
                DestinationPath = destinationPath,
                StartTime = startTime,
                EndTime = endTime,

                // Directory stats
                DirectoriesScanned = (int)roboResults.DirectoriesStatistic.Total,
                DirectoriesCopied = (int)roboResults.DirectoriesStatistic.Copied,
                DirectoriesSkipped = (int)roboResults.DirectoriesStatistic.Skipped,
                DirectoriesFailed = (int)roboResults.DirectoriesStatistic.Failed,

                // File stats
                FilesScanned = (int)roboResults.FilesStatistic.Total,
                FilesCopied = (int)roboResults.FilesStatistic.Copied,
                FilesSkipped = (int)roboResults.FilesStatistic.Skipped,
                FilesFailed = (int)roboResults.FilesStatistic.Failed,
                FilesExtra = (int)roboResults.FilesStatistic.Extras,
                FilesMismatch = (int)roboResults.FilesStatistic.Mismatch,

                // Byte stats
                BytesTotal = roboResults.BytesStatistic.Total,
                BytesCopied = roboResults.BytesStatistic.Copied,
                BytesSkipped = roboResults.BytesStatistic.Skipped,
                BytesFailed = roboResults.BytesStatistic.Failed,

                // Log file
                LogFilePath = options.LogFilePath,

                // Native result
                RoboSharpNativeResult = roboResults
            };

            // Parse exit code for errors
            if (exitCode >= 8)
            {
                var error = RoboSharpError.FromExitCode(exitCode, sourcePath, destinationPath);
                result.Errors.Add(error);
                result.ErrorMessage = error.Message;
            }

            return result;
        }

        /// <summary>
        /// Sets up progress tracking using RoboSharp's list-only mode for accurate totals.
        /// </summary>
        private async Task<RoboSharpProgressAdapter> SetupProgressTrackingAsync(
            string sourcePath,
            string destinationPath,
            RoboSharpOptions options,
            RoboCommand command,
            IProgress<TransferProgress>? progress,
            CancellationToken cancellationToken)
        {
            long totalFiles = 0;
            long totalBytes = 0;

            try
            {
                // Use RoboSharp's /L (list only) for accurate totals that respect all filters
                LoggingService.Debug("Running list-only scan for accurate totals...");
                var listCommand = CreateRoboCommand(sourcePath, destinationPath, options);
                var listResults = await listCommand.StartAsync_ListOnly();
                
                totalFiles = listResults.FilesStatistic.Total;
                totalBytes = listResults.BytesStatistic.Total;
                LoggingService.Debug($"List-only scan complete: {totalFiles} files, {totalBytes:N0} bytes");
            }
            catch (Exception ex)
            {
                // Fall back to directory scan if list-only fails
                LoggingService.Warning($"List-only scan failed, using directory scan fallback: {ex.Message}");
                var totals = CalculateDirectoryTotals(sourcePath);
                totalFiles = totals.TotalFiles;
                totalBytes = totals.TotalBytes;
                LoggingService.Debug($"Fallback scan complete: {totalFiles} files, {totalBytes:N0} bytes");
            }

            // Set up progress tracking with accurate totals
            var progressAdapter = new RoboSharpProgressAdapter(progress);
            progressAdapter.SetTotals((int)totalFiles, totalBytes);
            AttachEventHandlers(command, progressAdapter, cancellationToken);

            return progressAdapter;
        }

        /// <summary>
        /// Handles exceptions during transfer.
        /// </summary>
        private RoboSharpTransferResult HandleTransferException(Exception ex, string sourcePath, string destinationPath, DateTime startTime)
        {
            LoggingService.Error("RoboSharp transfer failed", ex);

            var error = new RoboSharpError
            {
                ErrorType = RoboSharpErrorType.FatalError,
                Message = ex.Message,
                Exception = ex,
                IsFatal = true
            };

            OnError?.Invoke(this, new RoboSharpErrorEventArgs(error, true));

            return RoboSharpTransferResult.CreateFailure(
                sourcePath,
                destinationPath,
                -1,
                ex.Message,
                startTime);
        }
    }
}