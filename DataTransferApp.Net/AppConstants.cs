namespace DataTransferApp.Net
{
    /// <summary>
    /// Application-wide constants and reusable values.
    /// </summary>
    public static class AppConstants
    {
        /// <summary>
        /// Number of days to retain RoboSharp transfer logs specifically.
        /// Can be set differently from application logs if needed.
        /// </summary>
        public const int RoboSharpLogRetentionDays = 3;

        // Retry and Timeout Constants

        /// <summary>
        /// Default maximum number of retry attempts for operations.
        /// </summary>
        public const int DefaultMaxRetries = 3;

        /// <summary>
        /// Default retry delay in milliseconds for file operations.
        /// </summary>
        public const int DefaultRetryDelayMs = 1000;

        /// <summary>
        /// Default retry delay in milliseconds for database operations.
        /// </summary>
        public const int DatabaseRetryDelayMs = 500;

        /// <summary>
        /// Default base delay in seconds for exponential backoff retry logic.
        /// </summary>
        public const int DefaultBaseDelaySeconds = 5;

        // File Size Limits

        /// <summary>
        /// Maximum file size for text file reading operations (5MB).
        /// </summary>
        public const long MaxTextFileSizeBytes = 5 * 1024 * 1024;

        /// <summary>
        /// Maximum file size for encoding detection (10MB).
        /// </summary>
        public const long MaxEncodingDetectionFileSizeBytes = 10 * 1024 * 1024;

        /// <summary>
        /// Maximum file size limit for log files (10MB).
        /// </summary>
        public const long MaxLogFileSizeBytes = 10 * 1024 * 1024;

        /// <summary>
        /// Maximum number of lines to read from a text file before truncating.
        /// </summary>
        public const int MaxTextFileLines = 50000;

        // File Size Units

        /// <summary>
        /// Number of bytes in a kilobyte.
        /// </summary>
        public const long BytesPerKB = 1024;

        /// <summary>
        /// Number of bytes in a megabyte.
        /// </summary>
        public const long BytesPerMB = 1024 * 1024;

        /// <summary>
        /// Number of bytes in a gigabyte.
        /// </summary>
        public const long BytesPerGB = 1024 * 1024 * 1024;

        // Buffer Sizes

        /// <summary>
        /// Default buffer size for file encoding detection (8KB).
        /// </summary>
        public const int FileEncodingSampleSize = 8192;

        // Database Constants

        /// <summary>
        /// Name of the database collection for storing transfer logs.
        /// </summary>
        public const string TransfersCollectionName = "transfers";

        // File Extensions

        /// <summary>
        /// File extensions that are considered compressed/archive files.
        /// Used for audit warning detection and file classification.
        /// </summary>
        public static readonly string[] CompressedFileExtensions =
        {
            ".zip",
            ".rar",
            ".7z",
            ".gz",
            ".tar",
            ".bz2",
            ".xz",
            ".mdzip",
            ".tar.gz",
            ".tar.xz",
            ".tar.bz2",
            ".tgz",
            ".tbz2",
            ".txz"
        };

        /// <summary>
        /// Multi-part compressed file extensions that require special handling.
        /// These are checked as full filename suffixes, not just extensions.
        /// </summary>
        public static readonly string[] MultiPartCompressedExtensions =
        {
            ".tar.gz",
            ".tgz",
            ".tar.xz",
            ".txz",
            ".tar.bz2",
            ".tbz2"
        };

        /// <summary>
        /// File extensions that are considered viewable in the application.
        /// These files can be opened and displayed in the file viewer.
        /// </summary>
        public static readonly string[] ViewableExtensions =
        {
            ".txt", ".log", ".csv", ".xml", ".json", ".ps1", ".psm1", ".psd1",
            ".md", ".html", ".htm", ".css", ".js", ".ini", ".conf", ".config",
            ".sql", ".bat", ".cmd", ".sh", ".py", ".java", ".c", ".cpp", ".h",
            ".cs", ".vb", ".php", ".rb", ".pl", ".yml", ".yaml", ".cfg"
        };

        /// <summary>
        /// Path to the user's Application Data folder, used for storing app-specific data and logs.
        /// </summary>
        public static readonly string ApplicationDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    }
}
