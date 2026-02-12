namespace DataTransferApp.Net.Services
{
    public class TransferProgress
    {
        public string CurrentFile { get; set; } = string.Empty;

        public int CompletedFiles { get; set; }

        public int TotalFiles { get; set; }

        public int PercentComplete { get; set; }

        /// <summary>
        /// Gets or sets number of bytes transferred so far.
        /// </summary>
        public long BytesTransferred { get; set; }

        /// <summary>
        /// Gets or sets total bytes to transfer.
        /// </summary>
        public long TotalBytes { get; set; }

        /// <summary>
        /// Gets or sets current transfer speed in bytes per second.
        /// </summary>
        public double BytesPerSecond { get; set; }

        /// <summary>
        /// Gets current transfer speed in megabytes per second.
        /// </summary>
        public double MBPerSecond => BytesPerSecond / (1024.0 * 1024.0);

        /// <summary>
        /// Gets or sets estimated time remaining for the transfer.
        /// </summary>
        public System.TimeSpan? EstimatedTimeRemaining { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the transfer has completed.
        /// Only true when OnCommandCompleted has fired.
        /// </summary>
        public bool IsCompleted { get; set; }
    }
}