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
        /// Gets or sets a value indicating whether indicates whether the transfer completed successfully.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets robocopy exit code (0-16, bit flags).
        /// 0 = No changes, 1 = Files copied, 2 = Extra files, 4 = Mismatches, 8 = Errors, 16 = Fatal error.
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// Gets or sets primary error message if the transfer failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        // File/Directory Statistics

        /// <summary>
        /// Gets or sets number of directories scanned at source.
        /// </summary>
        public long DirectoriesScanned { get; set; }

        /// <summary>
        /// Gets or sets number of directories successfully copied.
        /// </summary>
        public long DirectoriesCopied { get; set; }

        /// <summary>
        /// Gets or sets number of directories skipped (already exist, excluded).
        /// </summary>
        public long DirectoriesSkipped { get; set; }

        /// <summary>
        /// Gets or sets number of directories that failed to copy.
        /// </summary>
        public long DirectoriesFailed { get; set; }

        /// <summary>
        /// Gets or sets total number of files scanned at source.
        /// </summary>
        public long FilesScanned { get; set; }

        /// <summary>
        /// Gets or sets number of files successfully copied.
        /// </summary>
        public long FilesCopied { get; set; }

        /// <summary>
        /// Gets or sets number of files skipped (identical, excluded).
        /// </summary>
        public long FilesSkipped { get; set; }

        /// <summary>
        /// Gets or sets number of files that failed to copy.
        /// </summary>
        public long FilesFailed { get; set; }

        /// <summary>
        /// Gets or sets number of extra files found at destination (not in source).
        /// </summary>
        public long FilesExtra { get; set; }

        /// <summary>
        /// Gets or sets number of mismatched files (different size/timestamp).
        /// </summary>
        public long FilesMismatch { get; set; }

        // Byte Statistics

        /// <summary>
        /// Gets or sets total bytes in scanned files at source.
        /// </summary>
        public long BytesTotal { get; set; }

        /// <summary>
        /// Gets or sets bytes successfully copied.
        /// </summary>
        public long BytesCopied { get; set; }

        /// <summary>
        /// Gets or sets bytes skipped (files already identical).
        /// </summary>
        public long BytesSkipped { get; set; }

        /// <summary>
        /// Gets or sets bytes that failed to copy.
        /// </summary>
        public long BytesFailed { get; set; }

        // Timing Statistics

        /// <summary>
        /// Gets or sets start time of the transfer operation.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets end time of the transfer operation.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets total duration of the transfer.
        /// </summary>
        public TimeSpan Duration => EndTime - StartTime;

        /// <summary>
        /// Gets transfer speed in megabytes per second.
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
        /// Gets transfer speed in megabits per second (for network context).
        /// </summary>
        public double MbpsSpeed => MBPerSecond * 8;

        // Path Information

        /// <summary>
        /// Gets or sets source path of the transfer.
        /// </summary>
        public string SourcePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets destination path of the transfer.
        /// </summary>
        public string DestinationPath { get; set; } = string.Empty;

        // Error Details

        /// <summary>
        /// Gets or sets list of all errors encountered during transfer.
        /// </summary>
        public IList<RoboSharpError> Errors { get; set; } = new List<RoboSharpError>();

        /// <summary>
        /// Gets a value indicating whether indicates if there were any errors (even non-fatal).
        /// </summary>
        public bool HasErrors => Errors.Count > 0 || ExitCode >= 8;

        /// <summary>
        /// Gets a value indicating whether indicates if there were fatal errors that stopped the transfer.
        /// </summary>
        public bool HasFatalErrors => (ExitCode & 16) != 0;

        // RoboSharp Native Result (optional, for advanced scenarios)

        /// <summary>
        /// Gets or sets raw RoboSharp result object (if available).
        /// </summary>
        public object? RoboSharpNativeResult { get; set; }

        // Log Information

        /// <summary>
        /// Gets or sets path to the transfer log file, if logging was enabled.
        /// </summary>
        public string? LogFilePath { get; set; }

        /// <summary>
        /// Gets brief summary line for quick reference.
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
        /// Gets detailed summary with file counts and errors.
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
        /// Gets a value indicating whether determines if the exit code indicates success (0 or 1).
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
        /// <returns></returns>
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
        /// <returns></returns>
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