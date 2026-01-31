namespace DataTransferApp.Net.Models
{
    public class DatasetValidation
    {
        public bool IsValid { get; set; }

        public string? Dataset { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}