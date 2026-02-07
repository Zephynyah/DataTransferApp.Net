using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DataTransferApp.Net.Models
{
    /// <summary>
    /// Represents file metadata and transfer status information for the data transfer application.
    /// </summary>
    public partial class FileData : ObservableObject
    {
        [ObservableProperty]
        private string _fileName = string.Empty;

        [ObservableProperty]
        private string _directoryPath = string.Empty;

        [ObservableProperty]
        private string _extension = string.Empty;

        [ObservableProperty]
        private long _size;

        [ObservableProperty]
        private DateTime _modified;

        [ObservableProperty]
        private string _fullPath = string.Empty;

        [ObservableProperty]
        private string _relativePath = string.Empty;

        [ObservableProperty]
        private string _status = "Ready";

        [ObservableProperty]
        private string? _hash;

        [ObservableProperty]
        private bool _isViewable;

        [ObservableProperty]
        private bool _isArchive;

        [ObservableProperty]
        private bool _isCompressed;

        [ObservableProperty]
        private bool _isBlacklisted;

        [ObservableProperty]
        private string? _errorMessage;

        [ObservableProperty]
        private string? _errorDetails;

        [ObservableProperty]
        private List<string> _recommendedActions = new List<string>();

        public string SizeFormatted => FormatFileSize(Size);

        /// <summary>
        /// Gets a value indicating whether this file has an error (blacklisted or other issues).
        /// </summary>
        public bool HasError => IsBlacklisted || !string.IsNullOrEmpty(ErrorMessage);

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
    }
}