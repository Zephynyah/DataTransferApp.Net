using System;
using DataTransferApp.Net.Models;

namespace DataTransferApp.Net.Services
{
    public class TransferResult
    {
        public bool Success { get; set; }

        public string? ErrorMessage { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string? DestinationPath { get; set; }

        public TransferLog? TransferLog { get; set; }
    }
}