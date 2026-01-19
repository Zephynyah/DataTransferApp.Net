using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Readers;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataTransferApp.Net.Services
{
    public class ArchiveService
    {
        private static readonly string[] ArchiveExtensions = { ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".xz", ".mdzip", ".tar.gz", ".tar.xz", ".tar.bz2", ".tgz", ".tbz2", ".txz" };

        public bool IsArchive(string filePath)
        {
            var fileName = filePath.ToLower();
            
            // Check for compound extensions first (.tar.gz, .tar.xz, etc.)
            if (fileName.EndsWith(".tar.gz") || fileName.EndsWith(".tgz") ||
                fileName.EndsWith(".tar.xz") || fileName.EndsWith(".txz") ||
                fileName.EndsWith(".tar.bz2") || fileName.EndsWith(".tbz2"))
            {
                return true;
            }
            
            // Then check single extensions
            var ext = Path.GetExtension(filePath).ToLower();
            return ArchiveExtensions.Contains(ext);
        }

        public List<ArchiveEntry> GetArchiveContents(string archiveFilePath)
        {
            var entries = new List<ArchiveEntry>();

            try
            {
                using var archive = ArchiveFactory.Open(archiveFilePath);
                
                foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                {
                    entries.Add(new ArchiveEntry
                    {
                        Name = Path.GetFileName(entry.Key) ?? string.Empty,
                        Path = entry.Key ?? string.Empty,
                        Size = entry.Size,
                        CompressedSize = entry.CompressedSize,
                        Modified = entry.LastModifiedTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Unknown"
                    });
                }

                LoggingService.Info($"Retrieved {entries.Count} entries from archive: {Path.GetFileName(archiveFilePath)}");
            }
            catch (Exception ex)
            {
                LoggingService.Error($"Error reading archive '{archiveFilePath}'", ex);
                throw;
            }

            return entries;
        }

        public void ExtractArchive(string archiveFilePath, string destinationPath)
        {
            try
            {
                Directory.CreateDirectory(destinationPath);

                using var archive = ArchiveFactory.Open(archiveFilePath);
                
                foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                {
                    entry.WriteToDirectory(destinationPath, new ExtractionOptions
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }

                LoggingService.Success($"Extracted archive to: {destinationPath}");
            }
            catch (Exception ex)
            {
                LoggingService.Error($"Error extracting archive '{archiveFilePath}'", ex);
                throw;
            }
        }
    }

    public class ArchiveEntry
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public long Size { get; set; }
        public long CompressedSize { get; set; }
        public string Modified { get; set; } = string.Empty;

        public string SizeFormatted => FormatFileSize(Size);
        public string CompressedSizeFormatted => FormatFileSize(CompressedSize);

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
