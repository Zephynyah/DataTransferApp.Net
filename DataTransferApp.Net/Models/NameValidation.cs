using System;

namespace DataTransferApp.Net.Models
{
    public class NameValidation
    {
        public bool IsValid { get; set; }

        public string? EmployeeId { get; set; }

        public string? Date { get; set; }

        public string? Dataset { get; set; }

        public string? Sequence { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}