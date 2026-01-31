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

    public class NameValidation
    {
        public bool IsValid { get; set; }

        public string? EmployeeId { get; set; }

        public string? Date { get; set; }

        public string? Dataset { get; set; }

        public string? Sequence { get; set; }

        public string Message { get; set; } = string.Empty;
    }

    public class ExtensionValidation
    {
        public bool IsValid { get; set; }

        public IList<FileViolation> Violations { get; set; } = new List<FileViolation>();

        public string Message { get; set; } = string.Empty;
    }

    public class FileViolation
    {
        public string File { get; set; } = string.Empty;

        public string Extension { get; set; } = string.Empty;

        public string RelativePath { get; set; } = string.Empty;
    }

    public class DatasetValidation
    {
        public bool IsValid { get; set; }

        public string? Dataset { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}