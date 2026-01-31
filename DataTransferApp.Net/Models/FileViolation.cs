namespace DataTransferApp.Net.Models
{
    public class FileViolation
    {
        public string File { get; set; } = string.Empty;

        public string Extension { get; set; } = string.Empty;

        public string RelativePath { get; set; } = string.Empty;
    }
}