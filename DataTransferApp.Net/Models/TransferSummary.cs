using System;

namespace DataTransferApp.Net.Models
{
    public class TransferSummary
    {
        public int TotalFiles { get; set; }

        public long TotalSize { get; set; }

        public DateTime TransferStarted { get; set; }

        public DateTime TransferCompleted { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}