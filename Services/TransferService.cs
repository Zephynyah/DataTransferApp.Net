using DataTransferApp.Net.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DataTransferApp.Net.Services
{
    public class TransferService
    {
        private readonly AppSettings _settings;
        private readonly ArchiveService _archiveService;

        public TransferService(AppSettings settings)
        {
            _settings = settings;
            _archiveService = new ArchiveService();
        }

        public bool DriveHasContents(string drivePath)
        {
            try
            {
                var directories = Directory.GetDirectories(drivePath);
                var files = Directory.GetFiles(drivePath);
                return directories.Length > 0 || files.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        public int GetTransferredFolderCount(string drivePath)
        {
            try
            {
                return Directory.GetDirectories(drivePath).Length;
            }
            catch
            {
                return 0;
            }
        }

        private string ResolveDestinationPath(string destinationDrive, string folderName)
        {
            var basePath = Path.Combine(destinationDrive, folderName);
            
            if (!Directory.Exists(basePath))
            {
                return basePath;
            }

            // Folder exists - check if contents are different
            if (_settings.AutoHandleConflicts)
            {
                if (_settings.ConflictResolution == "Skip")
                {
                    LoggingService.Info($"Skipping existing folder: {folderName}");
                    return basePath; // Return existing path, will skip in transfer
                }
                else if (_settings.ConflictResolution == "Overwrite")
                {
                    LoggingService.Info($"Overwriting existing folder: {folderName}");
                    return basePath;
                }
                else // AppendSequence
                {
                    return GetSequencedPath(destinationDrive, folderName);
                }
            }

            return basePath;
        }

        private string GetSequencedPath(string destinationDrive, string folderName)
        {
            // Check if folder name already has a sequence (e.g., X50135_20260116_JTH_2)
            var parts = folderName.Split('_');
            string baseFolderName;
            int currentSequence = 0;

            // If there are 4 parts and the last part is numeric, it's already sequenced
            if (parts.Length == 4 && int.TryParse(parts[3], out int existingSeq))
            {
                // Remove the existing sequence number
                baseFolderName = string.Join("_", parts.Take(3));
                currentSequence = existingSeq;
            }
            else
            {
                baseFolderName = folderName;
                currentSequence = 0;
            }

            // Start from the next sequence number
            var sequence = currentSequence + 1;
            var newPath = Path.Combine(destinationDrive, $"{baseFolderName}_{sequence}");

            // Keep incrementing until we find a non-existing path
            while (Directory.Exists(newPath))
            {
                sequence++;
                newPath = Path.Combine(destinationDrive, $"{baseFolderName}_{sequence}");
            }

            LoggingService.Info($"Conflict resolved: {folderName} -> {Path.GetFileName(newPath)}");
            return newPath;
        }

        public async Task<TransferResult> TransferFolderAsync(
            FolderData folder,
            string destinationDrive,
            IProgress<TransferProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var result = new TransferResult
            {
                Success = false,
                StartTime = DateTime.Now
            };

            try
            {
                var destinationPath = ResolveDestinationPath(destinationDrive, folder.FolderName);
                
                LoggingService.Info($"Starting transfer: {folder.FolderName} -> {destinationPath}");

                // Check if skipping due to conflict resolution
                if (_settings.AutoHandleConflicts && 
                    _settings.ConflictResolution == "Skip" && 
                    Directory.Exists(destinationPath))
                {
                    result.Success = true;
                    result.EndTime = DateTime.Now;
                    result.DestinationPath = destinationPath;
                    result.ErrorMessage = "Skipped - folder already exists";
                    LoggingService.Info($"Skipped transfer (already exists): {folder.FolderName}");
                    return result;
                }

                // Create destination directory
                Directory.CreateDirectory(destinationPath);

                var files = folder.Files.ToList();
                var totalFiles = files.Count;
                var completedFiles = 0;

                foreach (var file in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Copy file
                    var destFilePath = Path.Combine(destinationPath, file.RelativePath);
                    var destFileDir = Path.GetDirectoryName(destFilePath);
                    
                    if (!string.IsNullOrEmpty(destFileDir))
                    {
                        Directory.CreateDirectory(destFileDir);
                    }

                    await Task.Run(() => File.Copy(file.FullPath, destFilePath, true), cancellationToken);

                    // Calculate hash if enabled
                    if (_settings.CalculateFileHashes)
                    {
                        file.Hash = await CalculateFileHashAsync(file.FullPath, cancellationToken);
                    }

                    file.Status = "Transferred";
                    completedFiles++;

                    // Report progress
                    progress?.Report(new TransferProgress
                    {
                        CurrentFile = file.FileName,
                        CompletedFiles = completedFiles,
                        TotalFiles = totalFiles,
                        PercentComplete = (int)((completedFiles / (double)totalFiles) * 100)
                    });
                }

                // Create transfer log
                var transferLog = CreateTransferLog(folder, destinationPath);
                await SaveTransferLogAsync(transferLog);

                result.Success = true;
                result.EndTime = DateTime.Now;
                result.DestinationPath = destinationPath;
                result.TransferLog = transferLog;

                LoggingService.Success($"Transfer completed: {folder.FolderName}");
            }
            catch (OperationCanceledException)
            {
                result.ErrorMessage = "Transfer cancelled by user";
                LoggingService.Warning($"Transfer cancelled: {folder.FolderName}");
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                LoggingService.Error($"Transfer failed: {folder.FolderName}", ex);
            }

            return result;
        }

        public List<RemovableDrive> GetRemovableDrives()
        {
            var drives = new List<RemovableDrive>();

            try
            {
                var allDrives = DriveInfo.GetDrives();

                foreach (var drive in allDrives)
                {
                    if (drive.DriveType == DriveType.Removable &&
                        drive.IsReady &&
                        drive.AvailableFreeSpace > _settings.MinimumFreeSpaceGB * 1024 * 1024 * 1024 &&
                        !_settings.ExcludeDrives.Contains(drive.Name))
                    {
                        drives.Add(new RemovableDrive
                        {
                            DriveLetter = drive.Name,
                            VolumeName = string.IsNullOrEmpty(drive.VolumeLabel) ? "Removable Drive" : drive.VolumeLabel,
                            FreeSpace = drive.AvailableFreeSpace,
                            TotalSize = drive.TotalSize,
                            DisplayText = $"{drive.Name} - {drive.VolumeLabel} (Free: {FormatFileSize(drive.AvailableFreeSpace)})"
                        });
                    }
                }

                LoggingService.Info($"Found {drives.Count} removable drive(s)");
            }
            catch (Exception ex)
            {
                LoggingService.Error("Error detecting removable drives", ex);
            }

            return drives;
        }

        public async Task ClearDriveAsync(string drivePath, IProgress<TransferProgress>? progress = null)
        {
            await Task.Run(() =>
            {
                try
                {
                    var directories = Directory.GetDirectories(drivePath);
                    var files = Directory.GetFiles(drivePath);
                    var totalItems = directories.Length + files.Length;
                    var completedItems = 0;

                    progress?.Report(new TransferProgress
                    {
                        CurrentFile = "Scanning drive...",
                        CompletedFiles = 0,
                        TotalFiles = totalItems,
                        PercentComplete = 0
                    });

                    foreach (var dir in directories)
                    {
                        var dirName = Path.GetFileName(dir);
                        progress?.Report(new TransferProgress
                        {
                            CurrentFile = $"Deleting folder: {dirName}",
                            CompletedFiles = completedItems,
                            TotalFiles = totalItems,
                            PercentComplete = (int)((completedItems / (double)totalItems) * 100)
                        });

                        Directory.Delete(dir, true);
                        completedItems++;
                    }

                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileName(file);
                        progress?.Report(new TransferProgress
                        {
                            CurrentFile = $"Deleting file: {fileName}",
                            CompletedFiles = completedItems,
                            TotalFiles = totalItems,
                            PercentComplete = (int)((completedItems / (double)totalItems) * 100)
                        });

                        File.Delete(file);
                        completedItems++;
                    }

                    progress?.Report(new TransferProgress
                    {
                        CurrentFile = "Complete",
                        CompletedFiles = totalItems,
                        TotalFiles = totalItems,
                        PercentComplete = 100
                    });

                    LoggingService.Success($"Drive cleared: {drivePath}");
                }
                catch (Exception ex)
                {
                    LoggingService.Error($"Error clearing drive: {drivePath}", ex);
                    throw;
                }
            });
        }

        private TransferLog CreateTransferLog(FolderData folder, string destinationPath)
        {
            return new TransferLog
            {
                TransferInfo = new TransferInfo
                {
                    DTA = _settings.DataTransferAgent,
                    Date = DateTime.Now,
                    Employee = folder.EmployeeId ?? "Unknown",
                    Origin = folder.FolderPath,
                    Destination = destinationPath,
                    FolderName = folder.FolderName,
                    SourcePath = folder.FolderPath,
                    DestinationPath = destinationPath
                },
                Files = folder.Files.Select(f => new TransferredFile
                {
                    FileName = f.FileName,
                    Extension = f.Extension,
                    Size = f.Size,
                    Modified = f.Modified,
                    FileHash = f.Hash,
                    RelativePath = f.RelativePath,
                    Status = f.Status
                }).ToList(),
                Summary = new TransferSummary
                {
                    TotalFiles = folder.Files.Count,
                    TotalSize = folder.TotalSize,
                    TransferStarted = DateTime.Now,
                    TransferCompleted = DateTime.Now,
                    Status = "Completed"
                }
            };
        }

        private async Task SaveTransferLogAsync(TransferLog log)
        {
            try
            {
                var logDir = _settings.TransferLogsDirectory;
                Directory.CreateDirectory(logDir);

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"{log.TransferInfo.FolderName}_{timestamp}.json";
                var logPath = Path.Combine(logDir, fileName);

                var json = JsonSerializer.Serialize(log, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(logPath, json);

                LoggingService.Info($"Transfer log saved: {logPath}");
            }
            catch (Exception ex)
            {
                LoggingService.Error("Error saving transfer log", ex);
            }
        }

        private async Task<string> CalculateFileHashAsync(string filePath, CancellationToken cancellationToken)
        {
            try
            {
                HashAlgorithm hashAlgorithm = _settings.HashAlgorithm.ToUpper() switch
                {
                    "SHA512" => SHA512.Create(),
                    "MD5" => MD5.Create(),
                    _ => SHA256.Create()
                };

                using (hashAlgorithm)
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = await Task.Run(() => hashAlgorithm.ComputeHash(stream), cancellationToken);
                    return BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error($"Error calculating hash for: {filePath}", ex);
                return "error";
            }
        }

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

    public class TransferProgress
    {
        public string CurrentFile { get; set; } = string.Empty;
        public int CompletedFiles { get; set; }
        public int TotalFiles { get; set; }
        public int PercentComplete { get; set; }
    }

    public class TransferResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? DestinationPath { get; set; }
        public TransferLog? TransferLog { get; set; }
    }
}
