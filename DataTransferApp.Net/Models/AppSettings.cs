using System;
using System.Collections.Generic;
using DataTransferApp.Net.Helpers;
using LiteDB;

namespace DataTransferApp.Net.Models
{
    public class AppSettings
    {
        public static string ApplicationVersion => VersionHelper.GetVersion();

        [BsonId]
        public int Id { get; set; } = 1;

        // Application Settings
        public string DataTransferAgent { get; set; } = Environment.UserName;

        // Directory Paths
        public string StagingDirectory { get; set; } = @"D:\Powershell\GUI\DTA\test-data\TransferStaging";

        public string RetentionDirectory { get; set; } = @"D:\Powershell\GUI\DTA\test-data\TransferRetention";

        public string TransferRecordsDirectory { get; set; } = @"D:\Powershell\GUI\DTA\test-data\TransferRecords";

        public int RetentionDays { get; set; } = 7;

        // Folder Naming
        public string FolderNameRegex { get; set; } = @"^[A-Za-z0-9]+_\d{8}_[A-Z]{2,10}(_\d+)?$";

        // File Extension Blacklist
        public IList<string> BlacklistedExtensions { get; set; } = new List<string>
        {
            ".exe", ".dll", ".msi", ".scr", ".com", ".pif", ".sys", ".drv"
        };

        // Dataset Whitelist
        public IList<string> WhiteListDatasets { get; set; } = new List<string>
        {
            "UG", "AETP", "PGP", "PAN"
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

        public IList<string> ExcludeDrives { get; set; } = new List<string> { "A:\\", "B:\\", "C:\\", "D:\\" };

        // Folder Exclusion
        public IList<string> ExcludedFolders { get; set; } = new List<string> { "New*", "MOVED", "ISSUES", "TEST" };

        // Application Logging
        public bool EnableFileLogging { get; set; } = true;

        public string LogLevel { get; set; } = "Info"; // Debug, Info, Warning, Error

        public string LogFormat { get; set; } = "TXT"; // JSON, CSV, TXT

        public int MaxLogSizeMB { get; set; } = 10;

        public int KeepLogFiles { get; set; } = 5;

        // Transfer History Database (LiteDB)
        public string TransferHistoryDatabasePath { get; set; } = string.Empty; // Empty = auto-select location

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
    }
}