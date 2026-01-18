using System;

namespace DataTransferApp.Net.Models
{
    public class FileData
    {
        public string FileName { get; set; } = string.Empty;
        public string DirectoryPath { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public long Size { get; set; }
        public string SizeFormatted => FormatFileSize(Size);
        public DateTime Modified { get; set; }
        public string FullPath { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public string Status { get; set; } = "Ready";
        public string? Hash { get; set; }
        public bool IsViewable { get; set; }
        public bool IsArchive { get; set; }
        
        private static string FormatFileSize(long bytes)
        {
            const long GB = 1024 * 1024 * 1024;
            const long MB = 1024 * 1024;
            const long KB = 1024;
            
            if (bytes >= GB)
                return $"{bytes / (double)GB:N2} GB";
            if (bytes >= MB)
                return $"{bytes / (double)MB:N2} MB";
            if (bytes >= KB)
                return $"{bytes / (double)KB:N2} KB";
            
            return $"{bytes} bytes";
        }
    }
}
