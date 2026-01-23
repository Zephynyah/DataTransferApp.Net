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
        private readonly TransferDatabaseService? _databaseService;
        private readonly ComplianceRecordService? _complianceService;

        public TransferService(AppSettings settings)
        {
            _settings = settings;
            _archiveService = new ArchiveService();

            // Initialize database service
            try
            {
                var dbPath = string.IsNullOrWhiteSpace(_settings.TransferHistoryDatabasePath)
                    ? null
                    : _settings.TransferHistoryDatabasePath;
                _databaseService = new TransferDatabaseService(dbPath);
                LoggingService.Info("Transfer database service initialized");
            }
            catch (Exception ex)
            {
                LoggingService.Error("Failed to initialize transfer database service", ex);
            }

            // Initialize compliance service
            if (_settings.GenerateComplianceRecords)
            {
                _complianceService = new ComplianceRecordService(_settings);
                LoggingService.Info("Compliance record service initialized");
            }
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

                // Move original folder to retention directory
                await MoveToRetentionAsync(folder.FolderPath, folder.FolderName);
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

        private async Task MoveToRetentionAsync(string sourcePath, string folderName)
        {
            try
            {
                var retentionPath = Path.Combine(_settings.RetentionDirectory, folderName);

                // Create retention directory if it doesn't exist
                Directory.CreateDirectory(_settings.RetentionDirectory);

                // Handle existing folder in retention - delete old one and replace
                if (Directory.Exists(retentionPath))
                {
                    Directory.Delete(retentionPath, true);
                    LoggingService.Info($"Replaced existing retention folder: {folderName}");
                }

                // Move folder to retention
                await Task.Run(() => Directory.Move(sourcePath, retentionPath));

                LoggingService.Info($"Moved to retention: {folderName} -> {retentionPath}");
            }
            catch (Exception ex)
            {
                LoggingService.Error($"Failed to move folder to retention: {folderName}", ex);
                // Don't throw - transfer was successful, this is just cleanup
            }
        }

        public List<RemovableDrive> GetRemovableDrives()
        {
            var drives = new List<RemovableDrive>();

            try
            {
                var allDrives = DriveInfo.GetDrives();

                foreach (var drive in allDrives)
                {
                    LoggingService.Debug($"Drive {drive.Name}: Type={drive.DriveType}, IsReady={drive.IsReady}, VolumeLabel='{drive.VolumeLabel}'");

                    if (drive.IsReady)
                    {
                        LoggingService.Debug($"  FreeSpace={FormatFileSize(drive.AvailableFreeSpace)}, TotalSize={FormatFileSize(drive.TotalSize)}");
                    }


                    if (_settings.ExcludeDrives.Contains(drive.Name))
                    {
                        LoggingService.Debug($"   Drive {drive.Name} is excluded by settings");
                        continue;
                    }

                    if ((drive.DriveType == DriveType.Removable || drive.DriveType == DriveType.Fixed) && drive.IsReady && drive.AvailableFreeSpace > _settings.MinimumFreeSpaceGB * 1024 * 1024 * 1024)
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

        public async Task CleanupRetentionAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    if (!Directory.Exists(_settings.RetentionDirectory))
                    {
                        LoggingService.Info("Retention directory does not exist, skipping cleanup");
                        return;
                    }

                    var cutoffDate = DateTime.Now.AddDays(-_settings.RetentionDays);
                    var folders = Directory.GetDirectories(_settings.RetentionDirectory);
                    var deletedCount = 0;

                    foreach (var folder in folders)
                    {
                        var folderInfo = new DirectoryInfo(folder);
                        if (folderInfo.CreationTime < cutoffDate)
                        {
                            try
                            {
                                Directory.Delete(folder, true);
                                deletedCount++;
                                LoggingService.Info($"Deleted old retention folder: {folderInfo.Name} (Created: {folderInfo.CreationTime:yyyy-MM-dd})");
                            }
                            catch (Exception ex)
                            {
                                LoggingService.Error($"Failed to delete retention folder: {folderInfo.Name}", ex);
                            }
                        }
                    }

                    if (deletedCount > 0)
                    {
                        LoggingService.Success($"Retention cleanup completed: {deletedCount} folder(s) removed");
                    }
                    else
                    {
                        LoggingService.Info("Retention cleanup completed: No folders to remove");
                    }
                }
                catch (Exception ex)
                {
                    LoggingService.Error("Error during retention cleanup", ex);
                }
            });
        }

        public async Task ClearDriveAsync(string drivePath, IProgress<TransferProgress>? progress = null)
        {
            await Task.Run(() =>
            {
                try
                {
                    // System folders to skip
                    var systemFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        "$RECYCLE.BIN",
                        "System Volume Information",
                        "$Recycle.Bin",
                        "RECYCLER",
                        "$WinREAgent"
                    };

                    var directories = Directory.GetDirectories(drivePath)
                        .Where(d => !systemFolders.Contains(Path.GetFileName(d)))
                        .ToList();
                    var files = Directory.GetFiles(drivePath);
                    var totalItems = directories.Count + files.Length;
                    var completedItems = 0;
                    var skippedItems = 0;

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

                        try
                        {
                            Directory.Delete(dir, true);
                            completedItems++;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            LoggingService.Warning($"Access denied to folder: {dirName} (skipped)");
                            skippedItems++;
                        }
                        catch (IOException ex)
                        {
                            LoggingService.Warning($"Cannot delete folder: {dirName} - {ex.Message} (skipped)");
                            skippedItems++;
                        }
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

                        try
                        {
                            File.Delete(file);
                            completedItems++;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            LoggingService.Warning($"Access denied to file: {fileName} (skipped)");
                            skippedItems++;
                        }
                        catch (IOException ex)
                        {
                            LoggingService.Warning($"Cannot delete file: {fileName} - {ex.Message} (skipped)");
                            skippedItems++;
                        }
                    }

                    progress?.Report(new TransferProgress
                    {
                        CurrentFile = "Complete",
                        CompletedFiles = totalItems,
                        TotalFiles = totalItems,
                        PercentComplete = 100
                    });

                    if (skippedItems > 0)
                    {
                        LoggingService.Success($"Drive cleared: {drivePath} ({completedItems} items deleted, {skippedItems} items skipped)");
                    }
                    else
                    {
                        LoggingService.Success($"Drive cleared: {drivePath} ({completedItems} items deleted)");
                    }
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
                // Save to LiteDB database
                if (_databaseService != null)
                {
                    bool saved = _databaseService.AddTransfer(log);
                    if (saved)
                    {
                        LoggingService.Success($"Transfer record saved to database: {log.TransferInfo.FolderName}");
                    }
                    else
                    {
                        LoggingService.Error($"Failed to save transfer record to database: {log.TransferInfo.FolderName}");
                    }
                }
                else
                {
                    LoggingService.Warning("Database service is not initialized - transfer record not saved to database");
                }

                // Generate compliance record (replaces separate JSON logging)
                if (_settings.GenerateComplianceRecords && _complianceService != null)
                {
                    await _complianceService.GenerateComplianceRecordAsync(log);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error("Error saving transfer log", ex);
                throw; // Re-throw to ensure caller knows about the failure
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
                    "SHA1" => SHA1.Create(),
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
