using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DataTransferApp.Net.Helpers;
using LiteDB;

namespace DataTransferApp.Net.Models
{
    public class AppSettings : INotifyDataErrorInfo
    {
        public static string ApplicationVersion => VersionHelper.GetVersion();

        [BsonId]
        public int Id { get; set; } = 1;

        // Application Settings
        public string DataTransferAgent { get; set; } = Environment.UserName;

        // Directory Paths
#if DEBUG
        private string _stagingDirectory = @"D:\Powershell\GUI\DTA\test-data\TransferStaging";
#else
        private string _stagingDirectory = @"\\Puszbf0a\GSC_FILE_TRANSFER";
#endif
        public string StagingDirectory
        {
            get => _stagingDirectory;
            set
            {
                _stagingDirectory = value;
                ValidateProperty(nameof(StagingDirectory), value);
            }
        }

#if DEBUG
        private string _retentionDirectory = @"D:\Powershell\GUI\DTA\test-data\TransferRetention";
#else
        private string _retentionDirectory = @"\\Puszbf0a\GSC_FILE_TRANSFER\Moved";
#endif
        public string RetentionDirectory
        {
            get => _retentionDirectory;
            set
            {
                _retentionDirectory = value;
                ValidateProperty(nameof(RetentionDirectory), value);
            }
        }

#if DEBUG
        private string _transferRecordsDirectory = @"D:\Powershell\GUI\DTA\test-data\TransferRecords";
#else
        private string _transferRecordsDirectory = @"\\Puszbf0a\GSC2\GSC_ACC\AFT\Collateral AFT Records";
#endif
        public string TransferRecordsDirectory
        {
            get => _transferRecordsDirectory;
            set
            {
                _transferRecordsDirectory = value;
                ValidateProperty(nameof(TransferRecordsDirectory), value);
            }
        }

        private int _retentionDays = 7;
        public int RetentionDays
        {
            get => _retentionDays;
            set
            {
                _retentionDays = value;
                ValidateProperty(nameof(RetentionDays), value);
            }
        }

        // Folder Naming
        public string FolderNameRegex { get; set; } = @"^[A-Za-z0-9]+_\d{8}_[A-Za-z]{2,10}(_\d+)?$";

        // File Extension Blacklist
        public IList<string> BlacklistedExtensions { get; set; } = new List<string>
        {
            ".exe", ".dll", ".msi", ".scr", ".com", ".pif", ".sys", ".drv"
        };

        // Dataset Whitelist
        public IList<string> WhiteListDatasets { get; set; } = new List<string>
        {
            "UG", "PGP"
        };

        // Audit Strategy
        public IList<string> AuditStrategy { get; set; } = new List<string>
        {
            "ValidateFolderName",
            "BlacklistExtensions",
            "WhitelistDatasets"
        };

        // Drive Detection
        public double MinimumFreeSpaceGB { get; set; } = 1.0;

        public IList<string> ExcludeDrives { get; set; } = new List<string>
        {
            "A:\\", "B:\\", "C:\\", "D:\\", "E:\\", "T:\\"
        };

        // Folder Exclusion
        public IList<string> ExcludedFolders { get; set; } = new List<string> { "New*", "MOVED", "ISSUES" };

        // Application Logging
        public bool EnableFileLogging { get; set; } = true;

        public string LogLevel { get; set; } = "Info"; // Debug, Info, Warning, Error

        public string LogFormat { get; set; } = "TXT"; // JSON, CSV, TXT

        public int MaxLogSizeMB { get; set; } = 10;

        public int KeepLogFiles { get; set; } = 5;

        // Transfer History Database (LiteDB)
#if DEBUG
        public string TransferHistoryDatabasePath { get; set; } = string.Empty; // Empty uses the default application data location
#else
        public string TransferHistoryDatabasePath { get; set; } = @"\\Puszbf0a\GSC2\GSC_ACC\AFT\Collateral AFT Records\TransferHistory.db";
#endif

        public bool UseSharedDatabaseLocation { get; set; } = true; // True = central location for all users

        public int KeepTransferRecords { get; set; } = 365; // Days to retain in database

        // Compliance Records (Primary Transfer Documentation)
        public bool GenerateComplianceRecords { get; set; } = true;

        public string ComplianceRecordFormat { get; set; } = "CSV"; // CSV, Excel, JSON

        public string ComplianceRecordType { get; set; } = "Standard"; // Standard, Comprehensive

        public string ComplianceSourceLocation { get; set; } = "Unclassified Corporate Network"; // Source location for compliance records

        public bool CalculateFileHashes { get; set; } = true;

        public string HashAlgorithm { get; set; } = "SHA1"; // SHA256, SHA512, SHA1, MD5

        // Transfer Settings
        public bool EnableCompression { get; set; } = false;

        public int MaxConcurrentTransfers { get; set; } = 1;

        public bool AutoHandleConflicts { get; set; } = true;

        public string ConflictResolution { get; set; } = "AppendSequence"; // AppendSequence, Skip, Overwrite

        public bool PromptBeforeOverwrite { get; set; } = true;

        // RoboSharp Transfer Engine Configuration
        public bool UseRoboSharp { get; set; } = false; // Feature flag - set to true to enable RoboSharp

        public bool UseMultithreadedCopy { get; set; } = true;

        public int RobocopyThreadCount { get; set; } = 8;

        public int RobocopyRetries { get; set; } = 5;

        public int RobocopyRetryWaitSeconds { get; set; } = 10;

        public bool UseRestartableMode { get; set; } = true;

        public bool UseBackupMode { get; set; } = true;

        public bool VerifyRobocopy { get; set; } = false;

        public int RobocopyBufferSizeKB { get; set; } = 128;

        public bool RobocopyDetailedLogging { get; set; } = false;

        public int RobocopyInterPacketGapMs { get; set; } = 0; // 0 = no throttling

        // UI Settings
        public double WindowWidth { get; set; } = 1400;

        public double WindowHeight { get; set; } = 1200;

        public bool ShowSuccessNotifications { get; set; } = true;

        public bool AutoAuditOnStartup { get; set; } = false;

        public bool ShowFolderAuditDetailsIcon { get; set; } = false;

        public bool ShowAuditSummaryAsCards { get; set; } = false;

        public string WindowStartupMode { get; set; } = "Normal"; // Normal, Maximized, Fullscreen

        // Last Updated
        public DateTime LastModified { get; set; } = DateTime.Now;

        #region INotifyDataErrorInfo Implementation

        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public System.Collections.IEnumerable GetErrors(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || !_errors.ContainsKey(propertyName))
                return Enumerable.Empty<string>();

            return _errors[propertyName];
        }

        public bool HasErrors => _errors.Any();

        private static void ValidateDirectoryPath(string? path, List<string> errors, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                errors.Add($"{fieldName} cannot be empty.");
                return;
            }

            try
            {
                // Check if it's a valid path format
                Path.GetFullPath(path);
            }
            catch
            {
                errors.Add($"{fieldName} contains invalid path characters.");
                return;
            }

            // Check if the path is absolute
            if (!Path.IsPathRooted(path))
            {
                errors.Add($"{fieldName} must be an absolute path.");
                return;
            }

            // Check if the drive exists
            var drive = Path.GetPathRoot(path);
            if (!string.IsNullOrEmpty(drive) && !Directory.Exists(drive))
            {
                errors.Add($"{fieldName} drive '{drive}' does not exist.");
            }
        }

        private static void ValidateRetentionDays(int? days, List<string> errors)
        {
            if (days == null)
            {
                errors.Add("Retention period is required.");
                return;
            }

            if (days < 0)
            {
                errors.Add("Retention period cannot be less than 0 days.");
            }
            else if (days > 3650)
            {
                // 10 years
                errors.Add("Retention period cannot exceed 3650 days (10 years).");
            }
        }

        private void ValidateProperty(string propertyName, object? value)
        {
            var errors = new List<string>();

            switch (propertyName)
            {
                case nameof(StagingDirectory):
                    ValidateDirectoryPath(value as string, errors, "Staging Directory");
                    break;
                case nameof(TransferRecordsDirectory):
                    ValidateDirectoryPath(value as string, errors, "Transfer Records Directory");
                    break;
                case nameof(RetentionDirectory):
                    ValidateDirectoryPath(value as string, errors, "Retention Directory");
                    break;
                case nameof(RetentionDays):
                    ValidateRetentionDays(value as int?, errors);
                    break;
            }

            if (errors.Any())
            {
                _errors[propertyName] = errors;
            }
            else
            {
                _errors.Remove(propertyName);
            }

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public void ValidateAllProperties()
        {
            ValidateProperty(nameof(StagingDirectory), StagingDirectory);
            ValidateProperty(nameof(TransferRecordsDirectory), TransferRecordsDirectory);
            ValidateProperty(nameof(RetentionDirectory), RetentionDirectory);
            ValidateProperty(nameof(RetentionDays), RetentionDays);
        }
        #endregion
    }
}
