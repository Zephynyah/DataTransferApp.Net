using System;

namespace DataTransferApp.Net.Models
{
    public class TransferInfo
    {
        public string DTA { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.Now;

        public string Employee { get; set; } = string.Empty;

        public string Origin { get; set; } = string.Empty;

        public string Destination { get; set; } = string.Empty;

        public string FolderName { get; set; } = string.Empty;

        public string SourcePath { get; set; } = string.Empty;

        public string DestinationPath { get; set; } = string.Empty;
    }
}