using System;
using System.Collections.Generic;
using LiteDB;

namespace DataTransferApp.Net.Models
{
    public class TransferLog
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.NewObjectId();

        public TransferInfo TransferInfo { get; set; } = new();

        public IList<TransferredFile> Files { get; set; } = new List<TransferredFile>();

        public TransferSummary Summary { get; set; } = new();
    }

    public class TransferInfo
    {
        public string DTA { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.Now;

        public string Employee { get; set; } = string.Empty;

        public string Origin { get; set; } = string.Empty;

        public string Destination { get; set; } = string.Empty;

        public string FolderName { get; set; } = string.Empty;

        public string SourcePath { get; set; } = string.Empty;

        public string DestinationPath { get; set; } = string.Empty;
    }

    public class TransferredFile
    {
        public string FileName { get; set; } = string.Empty;

        public string Extension { get; set; } = string.Empty;

        public long Size { get; set; }

        public DateTime Modified { get; set; }

        public string? FileHash { get; set; }

        public string RelativePath { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }

    public class TransferSummary
    {
        public int TotalFiles { get; set; }

        public long TotalSize { get; set; }

        public DateTime TransferStarted { get; set; }

        public DateTime TransferCompleted { get; set; }

        public string Status { get; set; } = string.Empty;
    }

    public class RemovableDrive
    {
        public string DriveLetter { get; set; } = string.Empty;

        public string VolumeName { get; set; } = string.Empty;

        public long FreeSpace { get; set; }

        public long TotalSize { get; set; }

        public string DisplayText { get; set; } = string.Empty;
    }
}