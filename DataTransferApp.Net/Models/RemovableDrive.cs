namespace DataTransferApp.Net.Models
{
    public class RemovableDrive
    {
        public string DriveLetter { get; set; } = string.Empty;

        public string VolumeName { get; set; } = string.Empty;

        public long FreeSpace { get; set; }

        public long TotalSize { get; set; }

        public string DisplayText { get; set; } = string.Empty;
    }
}