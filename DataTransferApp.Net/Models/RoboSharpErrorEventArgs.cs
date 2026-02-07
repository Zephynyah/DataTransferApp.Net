using System;

namespace DataTransferApp.Net.Models
{
    /// <summary>
    /// Event arguments for RoboSharp error events.
    /// </summary>
    public class RoboSharpErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the error that occurred.
        /// </summary>
        public RoboSharpError Error { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether indicates if the error is fatal and will stop the transfer.
        /// </summary>
        public bool IsFatal { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoboSharpErrorEventArgs"/> class.
        /// </summary>
        /// <param name="error">The error that occurred.</param>
        /// <param name="isFatal">Whether the error is fatal.</param>
        public RoboSharpErrorEventArgs(RoboSharpError error, bool isFatal = false)
        {
            Error = error;
            IsFatal = isFatal;
        }
    }
}
