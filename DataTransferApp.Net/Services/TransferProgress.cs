namespace DataTransferApp.Net.Services
{
    public class TransferProgress
    {
        public string CurrentFile { get; set; } = string.Empty;

        public int CompletedFiles { get; set; }

        public int TotalFiles { get; set; }

        public int PercentComplete { get; set; }
    }
}