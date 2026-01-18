using LiteDB;
using System;
using System.Collections.Generic;

namespace DataTransferApp.Net.Models
{
    public class AppSettings
    {
        [BsonId]
        public int Id { get; set; } = 1;
        
        // Application Settings
        public string DataTransferAgent { get; set; } = Environment.UserName;
        public string ApplicationVersion { get; set; } = "2.0.0";
        
        // Directory Paths
        public string StagingDirectory { get; set; } = @"D:\TransferStaging";
        public string RetentionDirectory { get; set; } = @"D:\TransferRetention";
        public string TransferLogsDirectory { get; set; } = @"D:\TransferLogs";
        
        // Folder Naming
        public string FolderNameRegex { get; set; } = @"^[A-Za-z0-9]+_\d{8}_[A-Z]{3}(_\d+)?$";
        
        // File Extension Blacklist
        public List<string> BlacklistedExtensions { get; set; } = new()
        {
            ".exe", ".dll", ".bat", ".cmd", ".ps1", ".vbs", ".msi",
            ".scr", ".com", ".pif", ".sys", ".drv"
        };
        
        // Dataset Whitelist
        public List<string> WhiteListDatasets { get; set; } = new()
        {
            "HYG", "FAN", "PGH", "JTH", "CAN", "XAN", "RAN", "YAN",
            "BAN", "MAN", "SAN", "FRT"
        };
        
        // Audit Strategy
        public List<string> AuditStrategy { get; set; } = new()
        {
            "ValidateFolderName",
            "BlacklistExtensions",
            "WhitelistDatasets"
        };
        
        // Drive Detection
        public double MinimumFreeSpaceGB { get; set; } = 1.0;
        public List<string> ExcludeDrives { get; set; } = new() { "A:", "B:" };
        
        // Application Logging
        public bool EnableFileLogging { get; set; } = true;
        public string LogLevel { get; set; } = "Info"; // Debug, Info, Warning, Error
        public string LogFormat { get; set; } = "JSON"; // JSON, CSV, TXT
        public int MaxLogSizeMB { get; set; } = 10;
        public int KeepLogFiles { get; set; } = 5;
        
        // Transfer Logging
        public bool EnableTransferLogs { get; set; } = true;
        public bool LogTransferDetails { get; set; } = true;
        public bool SaveTransferSummary { get; set; } = true;
        public string TransferLogFormat { get; set; } = "JSON"; // JSON, CSV, TXT
        public bool IncludeFileHashes { get; set; } = true;
        public int KeepTransferLogs { get; set; } = 30; // Days
        
        // Transfer Settings
        public bool CalculateFileHashes { get; set; } = true;
        public string HashAlgorithm { get; set; } = "SHA256"; // SHA256, SHA512, MD5
        public bool EnableCompression { get; set; } = false;
        public int MaxConcurrentTransfers { get; set; } = 1;
        public bool AutoHandleConflicts { get; set; } = true;
        public string ConflictResolution { get; set; } = "AppendSequence"; // AppendSequence, Skip, Overwrite
        public bool PromptBeforeOverwrite { get; set; } = true;
        
        // UI Settings
        public double WindowWidth { get; set; } = 1400;
        public double WindowHeight { get; set; } = 1000;
        public bool ShowSuccessNotifications { get; set; } = true;
        
        // Last Updated
        public DateTime LastModified { get; set; } = DateTime.Now;
    }
}
