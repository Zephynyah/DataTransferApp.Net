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
                var destinationPath = Path.Combine(destinationDrive, folder.FolderName);
                
                LoggingService.Info($"Starting transfer: {folder.FolderName} -> {destinationPath}");

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

        public async Task ClearDriveAsync(string drivePath, IProgress<string>? progress = null)
        {
            await Task.Run(() =>
            {
                try
                {
                    progress?.Report($"Clearing drive: {drivePath}");

                    var directories = Directory.GetDirectories(drivePath);
                    foreach (var dir in directories)
                    {
                        progress?.Report($"Deleting: {Path.GetFileName(dir)}");
                        Directory.Delete(dir, true);
                    }

                    var files = Directory.GetFiles(drivePath);
                    foreach (var file in files)
                    {
                        progress?.Report($"Deleting: {Path.GetFileName(file)}");
                        File.Delete(file);
                    }

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
                using var sha256 = SHA256.Create();
                using var stream = File.OpenRead(filePath);
                var hash = await Task.Run(() => sha256.ComputeHash(stream), cancellationToken);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
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
