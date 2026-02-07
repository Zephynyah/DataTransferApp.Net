namespace DataTransferApp.Net.Models
{
    /// <summary>
    /// Types of errors that can occur during RoboSharp operations.
    /// </summary>
    public enum RoboSharpErrorType
    {
        /// <summary>No error occurred.</summary>
        None = 0,

        /// <summary>General copy error - some files failed to copy.</summary>
        CopyError = 1,

        /// <summary>File or directory access was denied.</summary>
        AccessDenied = 2,

        /// <summary>File or directory was not found.</summary>
        NotFound = 3,

        /// <summary>File or directory is locked by another process.</summary>
        FileLocked = 4,

        /// <summary>Insufficient disk space at destination.</summary>
        DiskFull = 5,

        /// <summary>Network path not found or connection lost.</summary>
        NetworkError = 6,

        /// <summary>File or directory name is invalid.</summary>
        InvalidPath = 7,

        /// <summary>File mismatch detected between source and destination.</summary>
        Mismatch = 8,

        /// <summary>Extra files exist in destination that aren't in source.</summary>
        ExtraFiles = 9,

        /// <summary>Fatal error that stopped all operations.</summary>
        FatalError = 10,

        /// <summary>Operation was cancelled by user.</summary>
        Cancelled = 11,

        /// <summary>Timeout occurred during operation.</summary>
        Timeout = 12,

        /// <summary>Unknown or unclassified error.</summary>
        Unknown = 99
    }
}
