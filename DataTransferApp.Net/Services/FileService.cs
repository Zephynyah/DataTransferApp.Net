using System.IO;
using DataTransferApp.Net.Helpers;
using DataTransferApp.Net.Models;

namespace DataTransferApp.Net.Services
{
    public class FileService
    {
        public FileService()
        {
        }

        public static string ReadTextFile(string filePath, int maxLines = AppConstants.MaxTextFileLines)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                const long maxSize = AppConstants.MaxTextFileSizeBytes;

                if (fileInfo.Length > maxSize)
                {
                    var lines = File.ReadLines(filePath).Take(maxLines);
                    return string.Join(Environment.NewLine, lines) +
                           $"{Environment.NewLine}{Environment.NewLine}[File truncated - showing first {maxLines} lines]";
                }

                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                LoggingService.Error($"Error reading file: {filePath}", ex);
                throw;
            }
        }

        /// <summary>
        /// Enhanced file viewability check that combines extension filtering with content analysis.
        /// </summary>
        /// <param name="filePath">The full path to the file to check.</param>
        /// <param name="extension">The file extension to check.</param>
        /// <returns>True if the file can be viewed; otherwise, false.</returns>
        public static bool IsFileViewable(string filePath, string extension)
        {
            // Archives are always "viewable" because we can show their contents
            if (ArchiveService.IsArchive(extension))
            {
                return true;
            }

            // For known text extensions, verify content is actually text
            if (AppConstants.ViewableExtensions.Contains(extension.ToLowerInvariant()))
            {
                return FileEncodingHelper.IsTextFile(filePath);
            }

            // For unknown extensions, check if file is actually text-based
            return FileEncodingHelper.IsTextFile(filePath);
        }

        public async Task<IList<FolderData>> ScanStagingDirectoryAsync(string stagingPath)
        {
            return await Task.Run(() => ScanStagingDirectory(stagingPath));
        }

        public IList<FolderData> ScanStagingDirectory(string stagingPath)
        {
            var folders = new List<FolderData>();

            try
            {
                if (!Directory.Exists(stagingPath))
                {
                    LoggingService.Warning($"Staging directory does not exist: {stagingPath}");
                    return folders;
                }

                var directories = Directory.GetDirectories(stagingPath);

                // Get excluded folder patterns from settings
                var excludedPatterns = App.Settings?.ExcludedFolders ?? new List<string>();

                foreach (var dir in directories)
                {
                    try
                    {
                        var folderName = Path.GetFileName(dir);

                        // // Skip if folder is in exclusion list (case-insensitive)
                        // if (excludedFolders.Contains(folderName.ToLowerInvariant()))

                        // Skip if folder matches any exclusion pattern (supports wildcards)
                        if (Helpers.WildcardMatcher.IsMatch(folderName, excludedPatterns))
                        {
                            LoggingService.Info($"Skipping excluded folder: {folderName}");
                            continue;
                        }

                        var folderData = CreateFolderData(dir);
                        folders.Add(folderData);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Error($"Error scanning folder: {dir}", ex);
                    }
                }

                LoggingService.Info($"Scanned {folders.Count} folder(s) from staging directory");
            }
            catch (Exception ex)
            {
                LoggingService.Error($"Error scanning staging directory: {stagingPath}", ex);
            }

            return folders;
        }

        public FolderData CreateFolderData(string folderPath)
        {
            var folderName = Path.GetFileName(folderPath);
            var files = GetFolderFiles(folderPath);

            var folderData = new FolderData
            {
                FolderName = folderName,
                FolderPath = folderPath,
                FileCount = files.Count,
                TotalSize = files.Sum(f => f.Size),
                DateDiscovered = Directory.GetCreationTime(folderPath),
                Files = new System.Collections.ObjectModel.ObservableCollection<FileData>(files)
            };

            // Parse folder name
            var parts = folderName.Split('_');
            if (parts.Length >= 3)
            {
                folderData.EmployeeId = parts[0];
                folderData.Date = parts[1];
                folderData.Dataset = parts[2];
                folderData.Sequence = parts.Length > 3 ? parts[3] : null;
            }

            return folderData;
        }

        public IList<FileData> GetFolderFiles(string folderPath)
        {
            var fileDataList = new List<FileData>();

            try
            {
                var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        var ext = fileInfo.Extension.ToLowerInvariant();

                        var fileData = new FileData
                        {
                            FileName = fileInfo.Name,
                            DirectoryPath = GetRelativePath(fileInfo.DirectoryName, folderPath),
                            Extension = fileInfo.Extension,
                            Size = fileInfo.Length,
                            Modified = fileInfo.LastWriteTime,
                            FullPath = fileInfo.FullName,
                            RelativePath = file.Replace(folderPath, string.Empty).TrimStart(Path.DirectorySeparatorChar),
                            Status = "Ready",

                            // IsViewable = IsFileViewable(fileInfo.FullName, ext),
                            IsViewable = FileService.IsFileViewable(ext),
                            IsArchive = ArchiveService.IsArchive(file)
                        };

                        fileDataList.Add(fileData);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Error($"Error processing file: {file}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error($"Error getting folder files: {folderPath}", ex);
            }

            return fileDataList;
        }

        private static string GetRelativePath(string? directoryPath, string basePath)
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                return "\\";
            }

            var relativePath = directoryPath.Replace(basePath, string.Empty);
            return string.IsNullOrEmpty(relativePath) ? "\\" : relativePath.TrimEnd('\\') + "\\";
        }

        private static bool IsFileViewable(string extension)
        {
            return AppConstants.ViewableExtensions.Contains(extension.ToLowerInvariant()) || ArchiveService.IsArchive(extension);
        }
    }
}