using DataTransferApp.Net.Helpers;

namespace DataTransferApp.Net.Services
{
    public class ArchiveEntry
    {
        public string Name { get; set; } = string.Empty;

        public string Path { get; set; } = string.Empty;

        public long Size { get; set; }

        public long CompressedSize { get; set; }

        public string Modified { get; set; } = string.Empty;

        public string SizeFormatted => FileSizeHelper.FormatFileSize(Size);

        public string CompressedSizeFormatted => FileSizeHelper.FormatFileSize(CompressedSize);
    }
}