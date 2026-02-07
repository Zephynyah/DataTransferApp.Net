using System;
using System.Collections.Generic;

namespace DataTransferApp.Net.Models
{
    public class TransferSummary
    {
        public int TotalFiles { get; set; }

        public long TotalSize { get; set; }

        public DateTime TransferStarted { get; set; }

        public DateTime TransferCompleted { get; set; }

        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets transfer method used (Legacy, RoboSharp, etc.)
        /// </summary>
        public string TransferMethod { get; set; } = "Legacy";

        /// <summary>
        /// Gets or sets roboSharp-specific: Exit code from Robocopy operation.
        /// </summary>
        public int? RobocopyExitCode { get; set; }

        /// <summary>
        /// Gets or sets roboSharp-specific: Number of files copied.
        /// </summary>
        public int? FilesCopied { get; set; }

        /// <summary>
        /// Gets or sets roboSharp-specific: Number of files skipped.
        /// </summary>
        public int? FilesSkipped { get; set; }

        /// <summary>
        /// Gets or sets roboSharp-specific: Number of files failed.
        /// </summary>
        public int? FilesFailed { get; set; }

        /// <summary>
        /// Gets or sets roboSharp-specific: Number of directories copied.
        /// </summary>
        public int? DirectoriesCopied { get; set; }

        /// <summary>
        /// Gets or sets roboSharp-specific: Bytes copied.
        /// </summary>
        public long? BytesCopied { get; set; }

        /// <summary>
        /// Gets or sets roboSharp-specific: Average transfer speed in bytes per second.
        /// </summary>
        public double? AverageSpeedBytesPerSecond { get; set; }

        /// <summary>
        /// Gets or sets list of errors encountered during transfer.
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// Gets a formatted speed string (e.g., "45.2 MB/s").
        /// </summary>
        public string FormattedSpeed
        {
            get
            {
                if (AverageSpeedBytesPerSecond == null || AverageSpeedBytesPerSecond <= 0)
                    return "N/A";

                var mbps = AverageSpeedBytesPerSecond.Value / (1024.0 * 1024.0);
                return $"{mbps:F2} MB/s";
            }
        }

        /// <summary>
        /// Gets transfer duration.
        /// </summary>
        public TimeSpan Duration => TransferCompleted - TransferStarted;
    }
}