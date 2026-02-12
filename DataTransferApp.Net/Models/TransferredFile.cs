namespace DataTransferApp.Net.Models
{
    public class TransferredFile
    {
        public string FileName { get; set; } = string.Empty;

        public string Extension { get; set; } = string.Empty;

        public long Size { get; set; }

        public DateTime Modified { get; set; }

        public string? FileHash { get; set; }

        public string RelativePath { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}