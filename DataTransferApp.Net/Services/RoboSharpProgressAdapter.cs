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
        private readonly IProgress<TransferProgress>? _progress;
        private readonly Stopwatch _stopwatch;

        private long _totalBytes;
        private long _copiedBytes;
        private long _previousCopiedBytes;
        private int _totalFiles;
        private int _copiedFiles;
        private string _currentFile = string.Empty;

        private DateTime _lastUpdateTime;
        private const int UPDATE_INTERVAL_MS = 500; // Update UI every 500ms

        /// <summary>
        /// Initializes a new progress adapter.
        /// </summary>
        /// <param name="progress">Progress reporter for UI updates</param>
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
                _totalFiles = (int)e.ResultsEstimate.FilesStatistic.Total;
                _totalBytes = e.ResultsEstimate.BytesStatistic.Total;
                LoggingService.Debug($"Progress estimator created: {_totalFiles} files, {_totalBytes} bytes");
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
            if ((now - _lastUpdateTime).TotalMilliseconds < UPDATE_INTERVAL_MS && _copiedFiles < _totalFiles)
            {
                return;
            }

            _lastUpdateTime = now;

            var speed = CalculateSpeed();
            var eta = CalculateETA(speed);
            var percent = CalculatePercent();

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

            _previousCopiedBytes = _copiedBytes;
        }

        /// <summary>
        /// Calculates current transfer speed in bytes per second.
        /// </summary>
        private double CalculateSpeed()
        {
            if (_stopwatch.Elapsed.TotalSeconds <= 0)
            {
                return 0;
            }

            // Average speed over entire transfer
            return _copiedBytes / _stopwatch.Elapsed.TotalSeconds;
        }

        /// <summary>
        /// Calculates estimated time remaining.
        /// </summary>
        private TimeSpan? CalculateETA(double bytesPerSecond)
        {
            if (bytesPerSecond <= 0 || _totalBytes <= 0 || _copiedBytes >= _totalBytes)
            {
                return null;
            }

            var remainingBytes = _totalBytes - _copiedBytes;
            var secondsRemaining = remainingBytes / bytesPerSecond;

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

        /// <summary>
        /// Formats bytes to human-readable string.
        /// </summary>
        private static string FormatBytes(long bytes)
        {
            const long GB = 1024 * 1024 * 1024;
            const long MB = 1024 * 1024;
            const long KB = 1024;

            if (bytes >= GB)
            {
                return $"{bytes / (double)GB:N2} GB";
            }

            if (bytes >= MB)
            {
                return $"{bytes / (double)MB:N2} MB";
            }

            if (bytes >= KB)
            {
                return $"{bytes / (double)KB:N2} KB";
            }

            return $"{bytes} bytes";
        }
    }
}
