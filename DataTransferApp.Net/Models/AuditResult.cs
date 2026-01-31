using System;
using System.Collections.Generic;

namespace DataTransferApp.Net.Models
{
    public class AuditResult
    {
        public string FolderName { get; set; } = string.Empty;

        public string FolderPath { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public NameValidation? NameValidation { get; set; }

        public ExtensionValidation? ExtensionValidation { get; set; }

        public DatasetValidation? DatasetValidation { get; set; }

        public string OverallStatus { get; set; } = "Unknown"; // Unknown, Passed, Failed

        public bool CanTransfer { get; set; }

        public IList<string> Issues { get; set; } = new List<string>();
    }
}