using System.Collections.Generic;

namespace DataTransferApp.Net.Models
{
    public class ExtensionValidation
    {
        public bool IsValid { get; set; }

        public IList<FileViolation> Violations { get; set; } = new List<FileViolation>();

        public string Message { get; set; } = string.Empty;
    }
}