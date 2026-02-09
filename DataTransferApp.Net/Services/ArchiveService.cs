using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace DataTransferApp.Net.Services
{
    public class ArchiveService
    {
        public static bool IsArchive(string filePath)
        {
            var fileName = filePath.ToLowerInvariant();

            // Check for compound extensions first (.tar.gz, .tar.xz, etc.)
            if (AppConstants.MultiPartCompressedExtensions.Any(ext => fileName.EndsWith(ext, StringComparison.Ordinal)))
            {
                return true;
            }

            // Then check single extensions
            var extension = Path.GetExtension(fileName);
            return AppConstants.CompressedFileExtensions.Contains(extension);
        }

        public IList<ArchiveEntry> GetArchiveContents(string archiveFilePath)
        {
            var entries = new List<ArchiveEntry>();

            try
            {
                var fileName = archiveFilePath.ToLowerInvariant();

                // For compound archives like .tar.gz, .tar.xz, use Reader approach
                if (AppConstants.MultiPartCompressedExtensions.Any(ext => fileName.EndsWith(ext, StringComparison.Ordinal)))
                {
                    using var stream = File.OpenRead(archiveFilePath);
                    using var reader = ReaderFactory.Open(stream);

                    while (reader.MoveToNextEntry())
                    {
                        if (!reader.Entry.IsDirectory)
                        {
                            entries.Add(new ArchiveEntry
                            {
                                Name = Path.GetFileName(reader.Entry.Key) ?? string.Empty,
                                Path = reader.Entry.Key ?? string.Empty,
                                Size = reader.Entry.Size,
                                CompressedSize = reader.Entry.CompressedSize,
                                Modified = reader.Entry.LastModifiedTime?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? "Unknown"
                            });
                        }
                    }
                }
                else
                {
                    // For regular archives, use ArchiveFactory
                    using var archive = ArchiveFactory.Open(archiveFilePath);

                    foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                    {
                        entries.Add(new ArchiveEntry
                        {
                            Name = Path.GetFileName(entry.Key) ?? string.Empty,
                            Path = entry.Key ?? string.Empty,
                            Size = entry.Size,
                            CompressedSize = entry.CompressedSize,
                            Modified = entry.LastModifiedTime?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? "Unknown"
                        });
                    }
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

                var fileName = archiveFilePath.ToLowerInvariant();

                // For compound archives like .tar.gz, .tar.xz, use Reader approach
                if (AppConstants.MultiPartCompressedExtensions.Any(ext => fileName.EndsWith(ext, StringComparison.Ordinal)))
                {
                    using var stream = File.OpenRead(archiveFilePath);
                    using var reader = ReaderFactory.Open(stream);

                    while (reader.MoveToNextEntry())
                    {
                        if (!reader.Entry.IsDirectory)
                        {
                            reader.WriteEntryToDirectory(destinationPath, new ExtractionOptions
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }
                    }
                }
                else
                {
                    // For regular archives, use ArchiveFactory
                    using var archive = ArchiveFactory.Open(archiveFilePath);

                    foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                    {
                        entry.WriteToDirectory(destinationPath, new ExtractionOptions
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
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
}