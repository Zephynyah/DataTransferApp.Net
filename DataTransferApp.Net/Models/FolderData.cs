using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DataTransferApp.Net.Models
{
    public partial class FolderData : ObservableObject
    {
        [ObservableProperty]
        private string _folderName = string.Empty;
        
        [ObservableProperty]
        private string _folderPath = string.Empty;
        
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanTransfer))]
        [NotifyPropertyChangedFor(nameof(SizeFormatted))]
        private long _totalSize;
        
        public string SizeFormatted => FormatFileSize(TotalSize);
        
        [ObservableProperty]
        private int _fileCount;
        
        [ObservableProperty]
        private DateTime _dateDiscovered = DateTime.Now;
        
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanTransfer))]
        private string _auditStatus = "Not Audited"; // Not Audited, Passed, Failed
        
        [ObservableProperty]
        private ObservableCollection<FileData> _files = new();
        
        [ObservableProperty]
        private AuditResult? _auditResult;
        
        public bool CanTransfer => AuditStatus == "Passed";
        
        // Parsed from folder name
        [ObservableProperty]
        private string? _employeeId;
        
        [ObservableProperty]
        private string? _date;
        
        [ObservableProperty]
        private string? _dataset;
        
        [ObservableProperty]
        private string? _sequence;
        
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
}
