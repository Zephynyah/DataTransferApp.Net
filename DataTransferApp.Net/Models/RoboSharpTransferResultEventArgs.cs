namespace DataTransferApp.Net.Models
{
    /// <summary>
    /// Event arguments for RoboSharp transfer completion events.
    /// </summary>
    public class RoboSharpTransferResultEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the transfer result.
        /// </summary>
        public RoboSharpTransferResult Result { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoboSharpTransferResultEventArgs"/> class.
        /// </summary>
        /// <param name="result">The transfer result.</param>
        public RoboSharpTransferResultEventArgs(RoboSharpTransferResult result)
        {
            Result = result ?? throw new ArgumentNullException(nameof(result));
        }
    }
}
