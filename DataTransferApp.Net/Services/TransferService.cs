using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DataTransferApp.Net.Models;

namespace DataTransferApp.Net.Services
{
    public class TransferService
    {
        private readonly AppSettings _settings;
        private readonly TransferDatabaseService? _databaseService;
        private readonly ComplianceRecordService? _complianceService;

        public TransferService(AppSettings settings)
        {
            _settings = settings;

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

        public static bool DriveHasContents(string drivePath)
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

        public static int GetTransferredFolderCount(string drivePath)
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

                // Transfer all files
                await TransferFilesAsync(folder.Files, destinationPath, progress, cancellationToken);

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

        public IList<RemovableDrive> GetRemovableDrives()
        {
            var drives = new List<RemovableDrive>();

            try
            {
                var allDrives = DriveInfo.GetDrives();

                foreach (var drive in allDrives)
                {
                    LoggingService.Debug($"Drive {drive.Name}: Type={drive.DriveType}, IsReady={drive.IsReady}");

                    if (drive.IsReady)
                    {
                        LoggingService.Debug($"  VolumeLabel='{drive.VolumeLabel}', FreeSpace={FormatFileSize(drive.AvailableFreeSpace)}, TotalSize={FormatFileSize(drive.TotalSize)}");
                    }
                    else
                    {
                        LoggingService.Debug("  Drive not ready");
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

                LoggingService.Debug($"Found {drives.Count} removable drive(s)");
            }
            catch (Exception ex)
            {
                LoggingService.Error("Error detecting removable drives", ex);
            }

            return drives;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarAnalyzer.CSharp", "S2325:Make 'CleanupRetentionAsync' a static method", Justification = "Service method should remain instance method for dependency injection")]
        public async Task CleanupRetentionAsync()
        {
#if DEBUG
            LoggingService.Info("DEBUG MODE: Retention cleanup simulated - no folders will be deleted.");

            // Synchronously pause the current thread for 5000 milliseconds (5 seconds)
            await Task.Delay(10000);

            LoggingService.Info("DEBUG MODE: Retention cleanup simulation complete.");
#else
            await Task.Run(() =>
            {
                try
                {
                    LoggingService.Info("Starting retention cleanup");
                    if (!Directory.Exists(_settings.RetentionDirectory))
                    {
                        LoggingService.Info("Retention directory does not exist, skipping cleanup");
                        return;
                    }

                    var retentionDir = new DirectoryInfo(_settings.RetentionDirectory);
                    var folders = retentionDir.GetDirectories()
                        .OrderBy(d => d.CreationTime)
                        .ToList();

                    LoggingService.Info($"Found {folders.Count} folders in retention directory");

                    var cutoffDate = DateTime.Now.AddDays(-_settings.RetentionDays);
                    var foldersToDelete = folders
                        .Where(f => f.CreationTime < cutoffDate)
                        .ToList();

                    LoggingService.Info($"Will delete {foldersToDelete.Count} old folders (older than {_settings.RetentionDays} days, cutoff: {cutoffDate})");

                    foreach (var folder in foldersToDelete)
                    {
                        DeleteRetentionFolder(folder);
                    }

                    LoggingService.Info("Retention cleanup completed");
                }
                catch (Exception ex)
                {
                    LoggingService.Error("Error during retention cleanup", ex);
                }
            });
#endif
        }

        private static void DeleteRetentionFolder(DirectoryInfo folder)
        {
            const int maxRetries = 3;
            const int retryDelayMs = 1000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    if (attempt == 1)
                    {
                        LoggingService.Info($"Deleting retention folder: {folder.Name} (created: {folder.CreationTime})");
                    }
                    else
                    {
                        LoggingService.Info($"Retry {attempt}/{maxRetries}: Deleting retention folder: {folder.Name}");
                    }

                    // For UNC paths, try to remove read-only attributes first
                    try
                    {
                        RemoveReadOnlyAttributes(folder);
                    }
                    catch
                    {
                        // Continue even if attribute removal fails
                    }

                    folder.Delete(true);
                    LoggingService.Info($"Successfully deleted retention folder: {folder.Name}");
                    return; // Success
                }
                catch (UnauthorizedAccessException ex)
                {
                    if (attempt == maxRetries)
                    {
                        LoggingService.Warning($"Access denied when deleting retention folder '{folder.Name}' after {maxRetries} attempts. The folder may be locked or you may lack permissions. Skipping.");
                        LoggingService.Debug($"Details: {ex.Message}");
                    }
                    else
                    {
                        Thread.Sleep(retryDelayMs);
                    }
                }
                catch (IOException ex) when (ex.Message.Contains("denied") || ex.Message.Contains("being used"))
                {
                    if (attempt == maxRetries)
                    {
                        LoggingService.Warning($"Unable to delete retention folder '{folder.Name}' - folder is locked or in use. Will retry on next cleanup.");
                        LoggingService.Debug($"Details: {ex.Message}");
                    }
                    else
                    {
                        Thread.Sleep(retryDelayMs);
                    }
                }
                catch (Exception ex)
                {
                    LoggingService.Error($"Unexpected error deleting retention folder: {folder.Name}", ex);
                    return; // Don't retry on unexpected errors
                }
            }
        }

        private static void RemoveReadOnlyAttributes(DirectoryInfo directory)
        {
            // Remove read-only attribute from directory
            if ((directory.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                directory.Attributes &= ~FileAttributes.ReadOnly;
            }

            // Recursively remove read-only from files
            foreach (var file in directory.GetFiles())
            {
                if ((file.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    file.Attributes &= ~FileAttributes.ReadOnly;
                }
            }

            // Recursively remove read-only from subdirectories
            foreach (var subDir in directory.GetDirectories())
            {
                RemoveReadOnlyAttributes(subDir);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarAnalyzer.CSharp", "S2325:Make 'ClearDriveAsync' a static method", Justification = "Service method should remain instance method for dependency injection")]
        public async Task ClearDriveAsync(string drivePath, IProgress<TransferProgress>? progress = null)
        {
            await Task.Run(() =>
            {
                try
                {
                    var (directories, files) = GetItemsToDelete(drivePath);
                    var totalItems = directories.Count + files.Length;
                    var completedItems = 0;
                    var skippedItems = 0;

                    ReportInitialProgress(progress, totalItems);

                    completedItems += DeleteDirectories(directories, progress, ref completedItems, totalItems, ref skippedItems);
                    completedItems += DeleteFiles(files, progress, ref completedItems, totalItems, ref skippedItems);

                    ReportFinalProgress(progress, totalItems);
                    LogClearResults(drivePath, completedItems, skippedItems);
                }
                catch (Exception ex)
                {
                    LoggingService.Error($"Error clearing drive: {drivePath}", ex);
                    throw;
                }
            });
        }

        private static (List<string> Directories, string[] Files) GetItemsToDelete(string drivePath)
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

            return (directories, files);
        }

        private static void ReportInitialProgress(IProgress<TransferProgress>? progress, int totalItems)
        {
            progress?.Report(new TransferProgress
            {
                CurrentFile = "Scanning drive...",
                CompletedFiles = 0,
                TotalFiles = totalItems,
                PercentComplete = 0
            });
        }

        private static int DeleteDirectories(List<string> directories, IProgress<TransferProgress>? progress, ref int completedItems, int totalItems, ref int skippedItems)
        {
            var deletedCount = 0;

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
                    deletedCount++;
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

            return deletedCount;
        }

        private static int DeleteFiles(string[] files, IProgress<TransferProgress>? progress, ref int completedItems, int totalItems, ref int skippedItems)
        {
            var deletedCount = 0;

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
                    deletedCount++;
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

            return deletedCount;
        }

        private static void ReportFinalProgress(IProgress<TransferProgress>? progress, int totalItems)
        {
            progress?.Report(new TransferProgress
            {
                CurrentFile = "Complete",
                CompletedFiles = totalItems,
                TotalFiles = totalItems,
                PercentComplete = 100
            });
        }

        private static void LogClearResults(string drivePath, int completedItems, int skippedItems)
        {
            if (skippedItems > 0)
            {
                LoggingService.Success($"Drive cleared: {drivePath} ({completedItems} items deleted, {skippedItems} items skipped)");
            }
            else
            {
                LoggingService.Success($"Drive cleared: {drivePath} ({completedItems} items deleted)");
            }
        }

        private static string GetSequencedPath(string destinationDrive, string folderName)
        {
            // Check if folder name already has a sequence (e.g., X50135_20260116_JTH_2)
            var parts = folderName.Split('_');
            string baseFolderName;
            int currentSequence = 0;

            // If there are 4 parts and the last part is numeric, it's already sequenced
            if (parts.Length == 4 && int.TryParse(parts[3], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int existingSeq))
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

        private static string FormatFileSize(long bytes)
        {
            const long GB = 1024 * 1024 * 1024;
            const long MB = 1024 * 1024;
            const long KB = 1024;

            if (bytes >= GB)
            {
                return $"{bytes / (double)GB:N2} GB";
            }

            if (bytes >= MB)
            {
                return $"{bytes / (double)MB:N2} MB";
            }

            if (bytes >= KB)
            {
                return $"{bytes / (double)KB:N2} KB";
            }

            return $"{bytes} bytes";
        }

        private async Task TransferFilesAsync(IEnumerable<FileData> files, string destinationPath, IProgress<TransferProgress>? progress, CancellationToken cancellationToken)
        {
            var fileList = files.ToList();
            var totalFiles = fileList.Count;
            var completedFiles = 0;

            foreach (var file in fileList)
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

                // Preserve original creation time
                Directory.SetCreationTime(retentionPath, Directory.GetCreationTime(sourcePath));

                LoggingService.Info($"Moved to retention: {folderName} -> {retentionPath}");
            }
            catch (Exception ex)
            {
                LoggingService.Error($"Failed to move folder to retention: {folderName}", ex);

                // Don't throw - transfer was successful, this is just cleanup
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

                // AppendSequence
                else
                {
                    return GetSequencedPath(destinationDrive, folderName);
                }
            }

            return basePath;
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
                HashAlgorithm hashAlgorithm = _settings.HashAlgorithm.ToUpperInvariant() switch
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
                    return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error($"Error calculating hash for: {filePath}", ex);
                return "error";
            }
        }
    }
}
