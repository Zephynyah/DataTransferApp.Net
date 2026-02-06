using System;
using System.Collections.Generic;

namespace DataTransferApp.Net.Models
{
    /// <summary>
    /// Represents the result of a RoboSharp transfer operation with detailed statistics and error information.
    /// </summary>
    public class RoboSharpTransferResult
    {
        /// <summary>
        /// Indicates whether the transfer completed successfully.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Robocopy exit code (0-16, bit flags).
        /// 0 = No changes, 1 = Files copied, 2 = Extra files, 4 = Mismatches, 8 = Errors, 16 = Fatal error.
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// Primary error message if the transfer failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        // File/Directory Statistics
        /// <summary>
        /// Number of directories scanned at source.
        /// </summary>
        public long DirectoriesScanned { get; set; }

        /// <summary>
        /// Number of directories successfully copied.
        /// </summary>
        public long DirectoriesCopied { get; set; }

        /// <summary>
        /// Number of directories skipped (already exist, excluded).
        /// </summary>
        public long DirectoriesSkipped { get; set; }

        /// <summary>
        /// Number of directories that failed to copy.
        /// </summary>
        public long DirectoriesFailed { get; set; }

        /// <summary>
        /// Total number of files scanned at source.
        /// </summary>
        public long FilesScanned { get; set; }

        /// <summary>
        /// Number of files successfully copied.
        /// </summary>
        public long FilesCopied { get; set; }

        /// <summary>
        /// Number of files skipped (identical, excluded).
        /// </summary>
        public long FilesSkipped { get; set; }

        /// <summary>
        /// Number of files that failed to copy.
        /// </summary>
        public long FilesFailed { get; set; }

        /// <summary>
        /// Number of extra files found at destination (not in source).
        /// </summary>
        public long FilesExtra { get; set; }

        /// <summary>
        /// Number of mismatched files (different size/timestamp).
        /// </summary>
        public long FilesMismatch { get; set; }

        // Byte Statistics
        /// <summary>
        /// Total bytes in scanned files at source.
        /// </summary>
        public long BytesTotal { get; set; }

        /// <summary>
        /// Bytes successfully copied.
        /// </summary>
        public long BytesCopied { get; set; }

        /// <summary>
        /// Bytes skipped (files already identical).
        /// </summary>
        public long BytesSkipped { get; set; }

        /// <summary>
        /// Bytes that failed to copy.
        /// </summary>
        public long BytesFailed { get; set; }

        // Timing Statistics
        /// <summary>
        /// Start time of the transfer operation.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// End time of the transfer operation.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Total duration of the transfer.
        /// </summary>
        public TimeSpan Duration => EndTime - StartTime;

        /// <summary>
        /// Transfer speed in megabytes per second.
        /// </summary>
        public double MBPerSecond
        {
            get
            {
                if (Duration.TotalSeconds <= 0 || BytesCopied <= 0)
                {
                    return 0;
                }

                return (BytesCopied / (1024.0 * 1024.0)) / Duration.TotalSeconds;
            }
        }

        /// <summary>
        /// Transfer speed in megabits per second (for network context).
        /// </summary>
        public double MbpsSpeed => MBPerSecond * 8;

        // Path Information
        /// <summary>
        /// Source path of the transfer.
        /// </summary>
        public string SourcePath { get; set; } = string.Empty;

        /// <summary>
        /// Destination path of the transfer.
        /// </summary>
        public string DestinationPath { get; set; } = string.Empty;

        // Error Details
        /// <summary>
        /// List of all errors encountered during transfer.
        /// </summary>
        public List<RoboSharpError> Errors { get; set; } = new();

        /// <summary>
        /// Indicates if there were any errors (even non-fatal).
        /// </summary>
        public bool HasErrors => Errors.Count > 0 || ExitCode >= 8;

        /// <summary>
        /// Indicates if there were fatal errors that stopped the transfer.
        /// </summary>
        public bool HasFatalErrors => (ExitCode & 16) != 0;

        // RoboSharp Native Result (optional, for advanced scenarios)
        /// <summary>
        /// Raw RoboSharp result object (if available).
        /// </summary>
        public object? RoboSharpNativeResult { get; set; }

        // Log Information
        /// <summary>
        /// Path to the transfer log file, if logging was enabled.
        /// </summary>
        public string? LogFilePath { get; set; }

        /// <summary>
        /// Brief summary line for quick reference.
        /// </summary>
        public string Summary
        {
            get
            {
                if (Success)
                {
                    return $"Copied {FilesCopied} file(s) ({FormatBytes(BytesCopied)}) in {Duration.TotalSeconds:F1}s at {MBPerSecond:F2} MB/s";
                }
                else
                {
                    return $"Failed: {ErrorMessage ?? "Unknown error"} (Exit code: {ExitCode})";
                }
            }
        }

        /// <summary>
        /// Detailed summary with file counts and errors.
        /// </summary>
        public string DetailedSummary
        {
            get
            {
                var lines = new List<string>
                {
                    $"Source: {SourcePath}",
                    $"Destination: {DestinationPath}",
                    $"Duration: {Duration}",
                    $"Files: {FilesCopied} copied, {FilesSkipped} skipped, {FilesFailed} failed",
                    $"Bytes: {FormatBytes(BytesCopied)} copied at {MBPerSecond:F2} MB/s",
                    $"Exit Code: {ExitCode}"
                };

                if (HasErrors)
                {
                    lines.Add($"Errors: {Errors.Count}");
                }

                return string.Join(Environment.NewLine, lines);
            }
        }

        /// <summary>
        /// Determines if the exit code indicates success (0 or 1).
        /// </summary>
        public bool IsExitCodeSuccess => ExitCode <= 1;

        /// <summary>
        /// Formats bytes to human-readable format (KB, MB, GB).
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

        /// <summary>
        /// Creates a success result.
        /// </summary>
        public static RoboSharpTransferResult CreateSuccess(string sourcePath, string destPath, long filesCopied, long bytesCopied, DateTime startTime, DateTime endTime)
        {
            return new RoboSharpTransferResult
            {
                Success = true,
                ExitCode = filesCopied > 0 ? 1 : 0,
                SourcePath = sourcePath,
                DestinationPath = destPath,
                FilesCopied = filesCopied,
                BytesCopied = bytesCopied,
                StartTime = startTime,
                EndTime = endTime
            };
        }

        /// <summary>
        /// Creates a failure result.
        /// </summary>
        public static RoboSharpTransferResult CreateFailure(string sourcePath, string destPath, int exitCode, string errorMessage, DateTime startTime)
        {
            return new RoboSharpTransferResult
            {
                Success = false,
                ExitCode = exitCode,
                SourcePath = sourcePath,
                DestinationPath = destPath,
                ErrorMessage = errorMessage,
                StartTime = startTime,
                EndTime = DateTime.Now
            };
        }
    }
}
