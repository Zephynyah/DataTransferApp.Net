using System.Collections.Generic;

namespace DataTransferApp.Net.Models
{
    /// <summary>
    /// Configuration options for RoboSharp file transfer operations.
    /// Maps to Robocopy command-line switches for fine-grained control.
    /// </summary>
    public class RoboSharpOptions
    {
        // Threading Configuration
        /// <summary>
        /// Number of threads for multithreaded copying (/MT:n switch).
        /// Default: 8 threads for optimal performance.
        /// Range: 1-128 threads.
        /// </summary>
        public int ThreadCount { get; set; } = 8;

        // Retry Logic
        /// <summary>
        /// Number of retries on failed copies (/R:n switch).
        /// Default: 5 retries.
        /// </summary>
        public int RetryCount { get; set; } = 5;

        /// <summary>
        /// Wait time between retries in seconds (/W:n switch).
        /// Default: 10 seconds.
        /// </summary>
        public int RetryWaitSeconds { get; set; } = 10;

        // Copy Options
        /// <summary>
        /// Copy files in restartable mode (/Z switch).
        /// Recommended for large files over network - allows resume after interruption.
        /// </summary>
        public bool UseRestartableMode { get; set; } = true;

        /// <summary>
        /// Copy files in backup mode (/B switch).
        /// Bypasses file security, requires backup privileges.
        /// </summary>
        public bool UseBackupMode { get; set; } = true;

        /// <summary>
        /// Copy subdirectories (/S switch).
        /// Default: true to preserve folder structure.
        /// </summary>
        public bool CopySubdirectories { get; set; } = true;

        /// <summary>
        /// Copy empty subdirectories (/E switch).
        /// Includes empty folders in the copy operation.
        /// </summary>
        public bool CopyEmptySubdirectories { get; set; } = true;

        /// <summary>
        /// Copy file information - data, attributes, timestamps (/COPY:DAT).
        /// Default: true to preserve all file metadata.
        /// </summary>
        public bool CopyFileInfo { get; set; } = true;

        /// <summary>
        /// Copy file attributes (/A switch and /DCOPY:DA).
        /// Preserves read-only, hidden, system, archive attributes.
        /// </summary>
        public bool CopyAttributes { get; set; } = true;

        /// <summary>
        /// Copy timestamps (/DCOPY:T).
        /// Preserves creation, modification, and access times.
        /// </summary>
        public bool CopyTimestamps { get; set; } = true;

        // File/Directory Filters
        /// <summary>
        /// List of file patterns to exclude (/XF switch).
        /// Example: ["*.exe", "*.dll", "*.tmp"]
        /// </summary>
        public List<string> ExcludeFiles { get; set; } = new();

        /// <summary>
        /// List of directory patterns to exclude (/XD switch).
        /// Example: ["temp", "cache", ".git"]
        /// </summary>
        public List<string> ExcludeDirectories { get; set; } = new();

        /// <summary>
        /// List of file patterns to include (file filter).
        /// Example: ["*.pdf", "*.docx"]
        /// If empty, all files are included (except excluded ones).
        /// </summary>
        public List<string> IncludeFiles { get; set; } = new();

        // Behavior Options
        /// <summary>
        /// Continue copying even if errors occur.
        /// Default: true for resilient transfers.
        /// </summary>
        public bool ContinueOnError { get; set; } = true;

        /// <summary>
        /// Enable detailed logging of operations.
        /// Provides verbose output for debugging and audit trails.
        /// </summary>
        public bool EnableDetailedLogging { get; set; } = true;

        /// <summary>
        /// Purge destination files/folders that no longer exist in source (/PURGE).
        /// WARNING: Deletes files in destination not present in source.
        /// </summary>
        public bool PurgeDestination { get; set; } = false;

        /// <summary>
        /// Mirror source to destination (/MIR switch = /E + /PURGE).
        /// WARNING: Makes destination identical to source, including deletions.
        /// </summary>
        public bool MirrorMode { get; set; } = false;

        // Performance Options
        /// <summary>
        /// Buffer size for file copying in KB.
        /// Default: 128 KB for optimal throughput.
        /// </summary>
        public int BufferSizeKB { get; set; } = 128;

        /// <summary>
        /// Inter-packet gap in milliseconds (/IPG:n switch).
        /// Adds delay between packets to free up bandwidth.
        /// Useful for background transfers. 0 = no throttling.
        /// </summary>
        public int InterPacketGapMs { get; set; } = 0;

        // Verification Options
        /// <summary>
        /// Verify copied files by comparing source and destination.
        /// Adds overhead but ensures data integrity.
        /// </summary>
        public bool VerifyCopy { get; set; } = false;

        // Special Modes
        /// <summary>
        /// List-only mode (/L switch).
        /// Simulates the copy operation without actually copying.
        /// Useful for pre-transfer estimation and validation.
        /// </summary>
        public bool ListOnly { get; set; } = false;

        /// <summary>
        /// Move files instead of copying (/MOVE switch).
        /// Deletes source files after successful copy.
        /// </summary>
        public bool MoveFiles { get; set; } = false;

        /// <summary>
        /// Move files and directories (/MOVE + /E switch).
        /// Removes entire source tree after successful copy.
        /// </summary>
        public bool MoveTree { get; set; } = false;

        // Logging Options
        /// <summary>
        /// Path to log file for Robocopy output.
        /// If null or empty, logging to file is disabled.
        /// </summary>
        public string? LogFilePath { get; set; }

        /// <summary>
        /// Append to log file instead of overwriting (/LOG+ switch).
        /// </summary>
        public bool AppendLog { get; set; } = false;

        /// <summary>
        /// Include verbose output in logs (/V switch).
        /// Shows skipped files and detailed progress.
        /// </summary>
        public bool VerboseOutput { get; set; } = false;

        /// <summary>
        /// Creates a default RoboSharpOptions instance optimized for typical data transfer scenarios.
        /// </summary>
        /// <returns>Configured RoboSharpOptions with recommended defaults.</returns>
        public static RoboSharpOptions CreateDefault()
        {
            return new RoboSharpOptions
            {
                ThreadCount = 8,
                RetryCount = 5,
                RetryWaitSeconds = 10,
                UseRestartableMode = true,
                UseBackupMode = true,
                CopySubdirectories = true,
                CopyEmptySubdirectories = true,
                CopyFileInfo = true,
                CopyAttributes = true,
                CopyTimestamps = true,
                ContinueOnError = true,
                EnableDetailedLogging = true,
                BufferSizeKB = 128,
                VerifyCopy = false
            };
        }

        /// <summary>
        /// Creates options optimized for large file transfers over network.
        /// </summary>
        public static RoboSharpOptions CreateForNetworkTransfer()
        {
            return new RoboSharpOptions
            {
                ThreadCount = 4, // Lower thread count for network stability
                RetryCount = 10, // More retries for unreliable networks
                RetryWaitSeconds = 30, // Longer wait between retries
                UseRestartableMode = true, // Essential for resumable transfers
                UseBackupMode = true,
                CopySubdirectories = true,
                CopyEmptySubdirectories = true,
                ContinueOnError = true,
                BufferSizeKB = 256, // Larger buffer for network transfers
                VerifyCopy = true // Verify integrity over network
            };
        }

        /// <summary>
        /// Creates options optimized for quick local transfers.
        /// </summary>
        public static RoboSharpOptions CreateForLocalTransfer()
        {
            return new RoboSharpOptions
            {
                ThreadCount = 16, // Max threads for local speed
                RetryCount = 3, // Fewer retries needed locally
                RetryWaitSeconds = 5,
                UseRestartableMode = false, // Not needed for local
                UseBackupMode = false,
                CopySubdirectories = true,
                CopyEmptySubdirectories = true,
                ContinueOnError = true,
                BufferSizeKB = 64, // Smaller buffer for local I/O
                VerifyCopy = false
            };
        }
    }
}
