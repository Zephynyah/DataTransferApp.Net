using System;

namespace DataTransferApp.Net.Models
{
    /// <summary>
    /// Represents an error that occurred during a RoboSharp transfer operation.
    /// </summary>
    public class RoboSharpError
    {
        /// <summary>
        /// Gets or sets the type of error that occurred.
        /// </summary>
        public RoboSharpErrorType ErrorType { get; set; }

        /// <summary>
        /// Gets or sets the error code or exit code from Robocopy.
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets human-readable error message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file path associated with the error, if applicable.
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Gets or sets the directory path associated with the error, if applicable.
        /// </summary>
        public string? DirectoryPath { get; set; }

        /// <summary>
        /// Gets or sets timestamp when the error occurred.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets full exception details if available.
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether indicates whether this error is recoverable with retry.
        /// </summary>
        public bool IsRecoverable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether indicates whether this error caused the operation to fail completely.
        /// </summary>
        public bool IsFatal { get; set; }

        /// <summary>
        /// Gets or sets additional context or details about the error.
        /// </summary>
        public string? AdditionalInfo { get; set; }

        /// <summary>
        /// Creates a RoboSharpError from a Robocopy exit code.
        /// </summary>
        /// <param name="exitCode">The Robocopy exit code.</param>
        /// <param name="sourcePath">Source path for context.</param>
        /// <param name="destPath">Destination path for context.</param>
        /// <returns>A RoboSharpError instance with details from the exit code.</returns>
        public static RoboSharpError FromExitCode(int exitCode, string sourcePath = "", string destPath = "")
        {
            var error = new RoboSharpError
            {
                ErrorCode = exitCode,
                Timestamp = DateTime.Now,
                FilePath = sourcePath,
                DirectoryPath = destPath
            };

            if ((exitCode & 16) != 0)
            {
                error.ErrorType = RoboSharpErrorType.FatalError;
                error.Message = "Serious error occurred - no files were copied";
                error.IsFatal = true;
                error.IsRecoverable = false;
            }
            else if ((exitCode & 8) != 0)
            {
                error.ErrorType = RoboSharpErrorType.CopyError;
                error.Message = "Some files or directories could not be copied";
                error.IsFatal = false;
                error.IsRecoverable = true;
            }
            else if ((exitCode & 4) != 0)
            {
                error.ErrorType = RoboSharpErrorType.Mismatch;
                error.Message = "Some mismatched files or directories were detected";
                error.IsFatal = false;
                error.IsRecoverable = false;
            }
            else if ((exitCode & 2) != 0)
            {
                error.ErrorType = RoboSharpErrorType.ExtraFiles;
                error.Message = "Extra files or directories were detected in destination";
                error.IsFatal = false;
                error.IsRecoverable = false;
            }
            else if (exitCode == 1 || exitCode == 0)
            {
                error.ErrorType = RoboSharpErrorType.None;
                error.Message = exitCode == 0 ? "No errors" : "Files copied successfully";
                error.IsFatal = false;
                error.IsRecoverable = false;
            }
            else
            {
                error.ErrorType = RoboSharpErrorType.Unknown;
                error.Message = $"Unknown exit code: {exitCode}";
                error.IsFatal = false;
                error.IsRecoverable = true;
            }

            return error;
        }

        /// <summary>
        /// Returns a detailed string representation of the error.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var details = $"[{ErrorType}] {Message}";

            if (!string.IsNullOrEmpty(FilePath))
            {
                details += $" - File: {FilePath}";
            }

            if (!string.IsNullOrEmpty(DirectoryPath))
            {
                details += $" - Directory: {DirectoryPath}";
            }

            if (ErrorCode != 0)
            {
                details += $" (Code: {ErrorCode})";
            }

            return details;
        }
    }

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