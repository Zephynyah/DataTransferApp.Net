using DataTransferApp.Net.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DataTransferApp.Net.Services
{
    public class FileService
    {
        private readonly ArchiveService _archiveService;
        private static readonly string[] ViewableExtensions = {
            ".txt", ".log", ".csv", ".xml", ".json", ".ps1", ".psm1", ".psd1",
            ".md", ".html", ".htm", ".css", ".js", ".ini", ".conf", ".config",
            ".sql", ".bat", ".cmd", ".sh", ".py", ".java", ".c", ".cpp", ".h",
            ".cs", ".vb", ".php", ".rb", ".pl", ".yml", ".yaml", ".cfg"
        };

        public FileService()
        {
            _archiveService = new ArchiveService();
        }

        public async Task<List<FolderData>> ScanStagingDirectoryAsync(string stagingPath)
        {
            return await Task.Run(() => ScanStagingDirectory(stagingPath));
        }

        public List<FolderData> ScanStagingDirectory(string stagingPath)
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

                foreach (var dir in directories)
                {
                    try
                    {
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

        public List<FileData> GetFolderFiles(string folderPath)
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
                        var ext = fileInfo.Extension.ToLower();

                        var fileData = new FileData
                        {
                            FileName = fileInfo.Name,
                            DirectoryPath = GetRelativePath(fileInfo.DirectoryName, folderPath),
                            Extension = fileInfo.Extension,
                            Size = fileInfo.Length,
                            Modified = fileInfo.LastWriteTime,
                            FullPath = fileInfo.FullName,
                            RelativePath = file.Replace(folderPath, "").TrimStart(Path.DirectorySeparatorChar),
                            Status = "Ready",
                            IsViewable = IsFileViewable(ext),
                            IsArchive = _archiveService.IsArchive(file)
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

        public string ReadTextFile(string filePath, int maxLines = 50000)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                const long maxSize = 5 * 1024 * 1024; // 5MB

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

        private string GetRelativePath(string? directoryPath, string basePath)
        {
            if (string.IsNullOrEmpty(directoryPath))
                return "\\";

            var relativePath = directoryPath.Replace(basePath, "");
            return string.IsNullOrEmpty(relativePath) ? "\\" : relativePath.TrimEnd('\\') + "\\";
        }

        private bool IsFileViewable(string extension)
        {
            return ViewableExtensions.Contains(extension.ToLower()) || 
                   _archiveService.IsArchive(extension);
        }
    }
}
