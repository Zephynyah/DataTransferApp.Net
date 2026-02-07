using System;
using System.Diagnostics;
using DataTransferApp.Net.Models;
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
        private const int UpdateIntervalMs = 500; // Update UI every 500ms

        private readonly IProgress<TransferProgress>? _progress;
        private readonly Stopwatch _stopwatch;

        private long _totalBytes;
        private long _copiedBytes;
        private int _totalFiles;
        private int _copiedFiles;
        private string _currentFile = string.Empty;

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
        public void SetTotals(int totalFiles, long totalBytes)
        {
            _totalFiles = totalFiles;
            _totalBytes = totalBytes;
            LoggingService.Debug($"Progress adapter totals set: {totalFiles} files, {totalBytes:N0} bytes");
        }

        /// <summary>
        /// Handles RoboSharp OnFileProcessed event.
        /// </summary>
        public void OnFileProcessed(IRoboCommand sender, FileProcessedEventArgs e)
        {
            // In v1.6.0: FileProcessedEventArgs has ProcessedFile property
            _copiedFiles++;
            _copiedBytes += e.ProcessedFile.Size;
            _currentFile = e.ProcessedFile.Name;

            ReportProgress();
        }

        /// <summary>
        /// Handles RoboSharp OnProgressEstimatorCreated event.
        /// Note: In RoboSharp 1.6.0, this provides access to the ProgressEstimator.
        /// </summary>
        public void OnProgressEstimatorCreated(IRoboCommand sender, ProgressEstimatorCreatedEventArgs e)
        {
            // Access progress estimator for total counts
            if (e.ResultsEstimate != null)
            {
                var estimatedFiles = (int)e.ResultsEstimate.FilesStatistic.Total;
                var estimatedBytes = e.ResultsEstimate.BytesStatistic.Total;

                LoggingService.Debug($"Progress estimator created: {estimatedFiles} files, {estimatedBytes} bytes");

                // Only update totals if RoboSharp provides valid estimates (> 0)
                // Don't overwrite pre-scanned totals with zeros
                if (estimatedBytes > 0)
                {
                    _totalFiles = estimatedFiles;
                    _totalBytes = estimatedBytes;
                    LoggingService.Info($"Updated totals from RoboSharp estimator: {_totalFiles} files, {_totalBytes:N0} bytes");
                }
                else if (_totalBytes == 0)
                {
                    // Only log warning if we don't already have valid totals from pre-scan
                    LoggingService.Warning("RoboSharp estimator returned 0 bytes; using pre-scanned totals");
                }
            }
        }

        /// <summary>
        /// Handles RoboSharp OnCopyProgressChanged event.
        /// </summary>
        public void OnCopyProgressChanged(IRoboCommand sender, CopyProgressEventArgs e)
        {
            // This event provides per-file progress
            if (e.CurrentFile != null)
            {
                _currentFile = e.CurrentFile.Name;
            }

            ReportProgress();
        }

        /// <summary>
        /// Manually updates progress (for scenarios without file events).
        /// </summary>
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
                EstimatedTimeRemaining = TimeSpan.Zero
            });
        }

        /// <summary>
        /// Reports progress to the UI (with throttling to avoid excessive updates).
        /// </summary>
        private void ReportProgress()
        {
            // Throttle updates to avoid UI flooding
            var now = DateTime.Now;
            if ((now - _lastUpdateTime).TotalMilliseconds < UpdateIntervalMs && _copiedFiles < _totalFiles)
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

            var remainingBytes = _totalBytes - _copiedBytes;

            // Check if we're complete or bytes exceeded total (should only happen at end)
            if (remainingBytes <= 0)
            {
                LoggingService.Debug($"ETA: Transfer complete or bytes exceeded (_copiedBytes={_copiedBytes}, _totalBytes={_totalBytes})");
                return TimeSpan.Zero;
            }

            var secondsRemaining = remainingBytes / bytesPerSecond;

            // Sanity check: don't show crazy ETAs
            if (secondsRemaining < 0)
            {
                LoggingService.Debug($"ETA: Negative seconds calculated (remainingBytes={remainingBytes}, speed={bytesPerSecond})");
                return null;
            }

            if (secondsRemaining > 86400) // More than 24 hours
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
                return (int)((_copiedBytes / (double)_totalBytes) * 100);
            }

            if (_totalFiles > 0)
            {
                return (int)((_copiedFiles / (double)_totalFiles) * 100);
            }

            return 0;
        }
    }
}