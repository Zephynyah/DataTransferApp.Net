using System.Diagnostics;
using RoboSharp;
using RoboSharp.EventArgObjects;
using RoboSharp.Interfaces;

namespace DataTransferApp.Net.Services
{
    /// <summary>
    /// Adapter that translates RoboSharp events to application's TransferProgress model.
    /// Provides real-time progress updates with transfer speed and ETA calculations.
    /// </summary>
    public class RoboSharpProgressAdapter
    {
        private readonly IProgress<TransferProgress>? _progress;
        private readonly Stopwatch _stopwatch;

        // S1450: Field needs to persist across event handlers for event subscription
#pragma warning disable S1450
        private IProgressEstimator? _estimator;
#pragma warning restore S1450
        private long _totalBytes;
        private long _copiedBytes;
        private int _totalFiles;
        private int _copiedFiles;
        private string _currentFile = string.Empty;

        // Per-file progress tracking for fine-grained updates
        private long _completedBytesFromPreviousFiles;
        private long _currentFileSize;
        private string? _lastProcessedFile;

        private DateTime _lastUpdateTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoboSharpProgressAdapter"/> class.
        /// Initializes a new progress adapter.
        /// </summary>
        /// <param name="progress">Progress reporter for UI updates.</param>
        public RoboSharpProgressAdapter(IProgress<TransferProgress>? progress)
        {
            _progress = progress;
            _stopwatch = new Stopwatch();
            _lastUpdateTime = DateTime.Now;
        }

        /// <summary>
        /// Starts tracking transfer progress.
        /// </summary>
        public void Start()
        {
            _stopwatch.Start();
            _lastUpdateTime = DateTime.Now;
        }

        /// <summary>
        /// Stops tracking transfer progress.
        /// </summary>
        public void Stop()
        {
            _stopwatch.Stop();
        }

        /// <summary>
        /// Sets the total statistics for the transfer.
        /// </summary>
        /// <param name="totalFiles">Total number of files to transfer.</param>
        /// <param name="totalBytes">Total number of bytes to transfer.</param>
        public void SetTotals(int totalFiles, long totalBytes)
        {
            _totalFiles = totalFiles;
            _totalBytes = totalBytes;
            LoggingService.Debug($"Progress adapter totals set: {totalFiles} files, {totalBytes:N0} bytes");
        }

        /// <summary>
        /// Handles RoboSharp OnFileProcessed event.
        /// Tracks file completion and updates completed bytes counter.
        /// </summary>
        /// <param name="sender">The RoboCommand instance that raised the event.</param>
        /// <param name="e">Event arguments containing processed file information.</param>
        public void OnFileProcessed(IRoboCommand sender, FileProcessedEventArgs e)
        {
            var fileName = e.ProcessedFile?.Name ?? string.Empty;
            _currentFile = fileName;

            // When a file completes, add its size to completed bytes
            // This helps track overall progress across multiple files
            if (!string.IsNullOrEmpty(fileName) && fileName != _lastProcessedFile)
            {
                _lastProcessedFile = fileName;

                // If we have a current file size, add it to completed bytes
                if (_currentFileSize > 0)
                {
                    _completedBytesFromPreviousFiles += _currentFileSize;
                    LoggingService.Debug($"File completed: {fileName}, Size: {_currentFileSize:N0}, Total completed: {_completedBytesFromPreviousFiles:N0}");
                    _currentFileSize = 0; // Reset for next file
                }
            }
        }

        /// <summary>
        /// Handles RoboSharp OnProgressEstimatorCreated event.
        /// This is the correct way to get accurate progress per RoboSharp wiki.
        /// </summary>
        /// <param name="sender">The RoboCommand instance that raised the event.</param>
        /// <param name="e">Event arguments containing the progress estimator.</param>
        public void OnProgressEstimatorCreated(IRoboCommand sender, ProgressEstimatorCreatedEventArgs e)
        {
            if (e.ResultsEstimate != null)
            {
                _estimator = e.ResultsEstimate;

                // Subscribe to ValuesUpdated event (fires every ~150ms per RoboSharp wiki)
                _estimator.ValuesUpdated += OnEstimatorValuesUpdated;

                LoggingService.Info($"Progress estimator created and subscribed to ValuesUpdated");
            }
        }

        /// <summary>
        /// Handles IProgressEstimator.ValuesUpdated event.
        /// This fires every ~150ms with accurate file/byte counts from RoboSharp.
        /// </summary>
        private void OnEstimatorValuesUpdated(object? sender, IProgressEstimatorUpdateEventArgs e)
        {
            // Update copied counts from RoboSharp's statistics
            _copiedFiles = (int)e.FilesStatistic.Copied;
            _copiedBytes = e.BytesStatistic.Copied;

            // Only use estimator totals if we don't have pre-scan totals
            // (Pre-scan totals from list-only are more accurate and respect all RoboCopy filters)
            if (_totalFiles == 0)
            {
                _totalFiles = (int)e.FilesStatistic.Total;
            }

            if (_totalBytes == 0)
            {
                _totalBytes = e.BytesStatistic.Total;
            }

            LoggingService.Debug($"Estimator update: Files {_copiedFiles}/{_totalFiles}, Bytes {_copiedBytes:N0}/{_totalBytes:N0}");

            ReportProgress();
        }

        /// <summary>
        /// Handles RoboSharp OnCopyProgressChanged event.
        /// This provides per-file copy progress for fine-grained incremental updates (0%, 1%, 2%...100%).
        /// </summary>
        /// <param name="sender">The RoboCommand instance that raised the event.</param>
        /// <param name="e">Event arguments containing copy progress information.</param>
        public void OnCopyProgressChanged(IRoboCommand sender, CopyProgressEventArgs e)
        {
            // Update current file name if available
            if (e.CurrentFile != null)
            {
                _currentFile = e.CurrentFile.Name;
                _currentFileSize = e.CurrentFile.Size;
            }

            // CopyProgressEventArgs provides:
            // - CurrentFileProgress: progress percentage (0-100) for the current file
            // - CurrentFile: ProcessedFileInfo with Name, Size, etc.

            // Calculate bytes transferred for current file based on progress percentage
            var currentFileProgress = e.CurrentFileProgress; // 0-100
            var currentFileBytesTransferred = (long)(_currentFileSize * (currentFileProgress / 100.0));

            // Calculate overall bytes: completed files + current file progress
            _copiedBytes = _completedBytesFromPreviousFiles + currentFileBytesTransferred;

            // Report progress with throttling (ReportProgress has built-in 50ms throttle)
            ReportProgress();
        }

        /// <summary>
        /// Manually updates progress (for scenarios without file events).
        /// </summary>
        /// <param name="completedFiles">The number of files that have been completed.</param>
        /// <param name="copiedBytes">The number of bytes that have been transferred.</param>
        /// <param name="currentFile">The name of the file currently being transferred, if available.</param>
        public void UpdateProgress(int completedFiles, long copiedBytes, string? currentFile = null)
        {
            _copiedFiles = completedFiles;
            _copiedBytes = copiedBytes;

            if (!string.IsNullOrEmpty(currentFile))
            {
                _currentFile = currentFile;
            }

            ReportProgress();
        }

        /// <summary>
        /// Reports final progress at 100%.
        /// </summary>
        public void ReportComplete()
        {
            _progress?.Report(new TransferProgress
            {
                CurrentFile = "Transfer complete",
                CompletedFiles = _totalFiles,
                TotalFiles = _totalFiles,
                BytesTransferred = _copiedBytes,
                TotalBytes = _totalBytes,
                PercentComplete = 100,
                BytesPerSecond = CalculateSpeed(),
                EstimatedTimeRemaining = TimeSpan.Zero,
                IsCompleted = true
            });
        }

        /// <summary>
        /// Reports progress to the UI (with throttling to avoid excessive updates).
        /// </summary>
        private void ReportProgress()
        {
            // OnEstimatorValuesUpdated already fires at ~150ms intervals, so minimal throttling
            // Only prevent duplicate updates within 50ms window
            var now = DateTime.Now;
            if ((now - _lastUpdateTime).TotalMilliseconds < 50)
            {
                return;
            }

            _lastUpdateTime = now;

            var speed = CalculateSpeed();
            var eta = CalculateETA(speed);
            var percent = CalculatePercent();

            LoggingService.Debug($"Progress: {_copiedFiles}/{_totalFiles} files, {_copiedBytes:N0}/{_totalBytes:N0} bytes, {speed:F0} B/s, ETA={eta?.TotalSeconds:F0}s");

            _progress?.Report(new TransferProgress
            {
                CurrentFile = _currentFile,
                CompletedFiles = _copiedFiles,
                TotalFiles = _totalFiles,
                BytesTransferred = _copiedBytes,
                TotalBytes = _totalBytes,
                PercentComplete = percent,
                BytesPerSecond = speed,
                EstimatedTimeRemaining = eta
            });
        }

        /// <summary>
        /// Calculates current transfer speed in bytes per second.
        /// </summary>
        private double CalculateSpeed()
        {
            var elapsedSeconds = _stopwatch.Elapsed.TotalSeconds;

            // Need at least 0.1 seconds of data to calculate speed
            if (elapsedSeconds < 0.1)
            {
                return 0;
            }

            // Average speed over entire transfer
            return _copiedBytes / elapsedSeconds;
        }

        /// <summary>
        /// Calculates estimated time remaining.
        /// </summary>
        private TimeSpan? CalculateETA(double bytesPerSecond)
        {
            // If we don't have valid totals yet, return null
            if (_totalBytes <= 0)
            {
                LoggingService.Debug($"ETA: Waiting for totals (_totalBytes={_totalBytes})");
                return null;
            }

            // If speed is zero, return null temporarily
            if (bytesPerSecond <= 0)
            {
                // If we've been running for more than 2 seconds with no bytes, something is wrong
                if (_stopwatch.Elapsed.TotalSeconds > 2)
                {
                    LoggingService.Debug($"ETA: No bytes transferred after {_stopwatch.Elapsed.TotalSeconds:F1}s");
                }

                return null;
            }

            var remainingBytes = Math.Max(_totalBytes - _copiedBytes, 0);

            // If no bytes remaining, transfer is complete or nearly complete
            if (remainingBytes <= 0)
            {
                LoggingService.Debug($"ETA: Transfer complete (_copiedBytes={_copiedBytes}, _totalBytes={_totalBytes})");
                return TimeSpan.Zero;
            }

            var secondsRemaining = remainingBytes / bytesPerSecond;

            // Sanity check: don't show crazy ETAs
            if (secondsRemaining < 0)
            {
                LoggingService.Debug($"ETA: Negative seconds calculated (remainingBytes={remainingBytes}, speed={bytesPerSecond})");
                return null;
            }

            // Cap at 24 hours for unrealistic estimates
            if (secondsRemaining > 86400)
            {
                LoggingService.Debug($"ETA: Capping unrealistic estimate of {secondsRemaining:F0}s");
                return TimeSpan.FromHours(24);
            }

            LoggingService.Debug($"ETA: {secondsRemaining:F0}s (remaining: {remainingBytes:N0} bytes, speed: {bytesPerSecond:F0} B/s)");
            return TimeSpan.FromSeconds(secondsRemaining);
        }

        /// <summary>
        /// Calculates completion percentage.
        /// </summary>
        private int CalculatePercent()
        {
            if (_totalBytes > 0)
            {
                var percent = (int)Math.Round((_copiedBytes / (double)_totalBytes) * 100);
                return Math.Clamp(percent, 0, 100);
            }

            if (_totalFiles > 0)
            {
                var percent = (int)Math.Round((_copiedFiles / (double)_totalFiles) * 100);
                return Math.Clamp(percent, 0, 100);
            }

            return 0;
        }
    }
}