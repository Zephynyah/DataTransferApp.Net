namespace DataTransferApp.Net.Services
{
    public class TransferProgress
    {
        public string CurrentFile { get; set; } = string.Empty;

        public int CompletedFiles { get; set; }

        public int TotalFiles { get; set; }

        public int PercentComplete { get; set; }

        /// <summary>
        /// Number of bytes transferred so far.
        /// </summary>
        public long BytesTransferred { get; set; }

        /// <summary>
        /// Total bytes to transfer.
        /// </summary>
        public long TotalBytes { get; set; }

        /// <summary>
        /// Current transfer speed in bytes per second.
        /// </summary>
        public double BytesPerSecond { get; set; }

        /// <summary>
        /// Current transfer speed in megabytes per second.
        /// </summary>
        public double MBPerSecond => BytesPerSecond / (1024.0 * 1024.0);

        /// <summary>
        /// Estimated time remaining for the transfer.
        /// </summary>
        public System.TimeSpan? EstimatedTimeRemaining { get; set; }
    }
}