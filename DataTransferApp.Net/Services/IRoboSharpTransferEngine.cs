using System;
using System.Threading;
using System.Threading.Tasks;
using DataTransferApp.Net.Models;

namespace DataTransferApp.Net.Services
{
    /// <summary>
    /// Interface for RoboSharp-based file transfer engine.
    /// Provides high-performance, reliable file transfer operations using Robocopy.
    /// </summary>
    public interface IRoboSharpTransferEngine
    {
        /// <summary>
        /// Transfers an entire folder and its contents from source to destination.
        /// </summary>
        /// <param name="sourcePath">Source folder path.</param>
        /// <param name="destinationPath">Destination folder path.</param>
        /// <param name="options">Transfer options and configuration.</param>
        /// <param name="progress">Progress reporter for UI updates.</param>
        /// <param name="cancellationToken">Cancellation token to abort transfer.</param>
        /// <returns>Transfer result with statistics and error details.</returns>
        Task<RoboSharpTransferResult> TransferFolderAsync(
            string sourcePath,
            string destinationPath,
            RoboSharpOptions options,
            IProgress<TransferProgress>? progress = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Transfers specific files from source root to destination.
        /// Preserves relative path structure.
        /// </summary>
        /// <param name="filePaths">List of file paths to transfer.</param>
        /// <param name="sourceRoot">Source root directory.</param>
        /// <param name="destinationPath">Destination folder path.</param>
        /// <param name="options">Transfer options and configuration.</param>
        /// <param name="progress">Progress reporter for UI updates.</param>
        /// <param name="cancellationToken">Cancellation token to abort transfer.</param>
        /// <returns>Transfer result with statistics and error details.</returns>
        Task<RoboSharpTransferResult> TransferFilesAsync(
            string[] filePaths,
            string sourceRoot,
            string destinationPath,
            RoboSharpOptions options,
            IProgress<TransferProgress>? progress = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Estimates transfer size and file count without actually copying.
        /// Useful for pre-transfer validation and progress estimation.
        /// </summary>
        /// <param name="sourcePath">Source folder path.</param>
        /// <param name="options">Transfer options (filters will be applied).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result with file count and byte statistics (no actual copy performed).</returns>
        Task<RoboSharpTransferResult> EstimateTransferAsync(
            string sourcePath,
            RoboSharpOptions options,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Event raised when an error occurs during transfer.
        /// </summary>
        event EventHandler<RoboSharpErrorEventArgs>? OnError;

        /// <summary>
        /// Event raised when transfer completes (success or failure).
        /// </summary>
        event EventHandler<RoboSharpTransferResultEventArgs>? OnCompleted;
    }
}
