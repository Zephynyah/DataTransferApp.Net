using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DataTransferApp.Net.Helpers;

namespace DataTransferApp.Net.Models
{
    /// <summary>
    /// Represents folder data with audit results, file information, and transfer capabilities.
    /// </summary>
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

        // Individual audit statuses
        [ObservableProperty]
        private string _namingAuditStatus = "Not Audited";

        [ObservableProperty]
        private string _blacklistAuditStatus = "Not Audited";

        [ObservableProperty]
        private string _compressedAuditStatus = "Not Audited";

        [ObservableProperty]
        private string _datasetAuditStatus = "Not Audited";

        [ObservableProperty]
        private int _blacklistViolationCount = 0;

        [ObservableProperty]
        private int _compressedFileCount = 0;

        [ObservableProperty]
        private string _namingFailureReason = string.Empty;

        [ObservableProperty]
        private string _datasetFailureReason = string.Empty;

        [ObservableProperty]
        private string? _employeeId;

        [ObservableProperty]
        private string? _date;

        [ObservableProperty]
        private string? _dataset;

        [ObservableProperty]
        private string? _sequence;

        public string SizeFormatted => FileSizeHelper.FormatFileSize(TotalSize);

        public bool CanTransfer => AuditStatus == "Passed";
    }
}