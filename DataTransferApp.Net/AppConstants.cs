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

        /// <summary>
        /// Path to the user's Application Data folder, used for storing app-specific data and logs.
        /// </summary>
        public static readonly string ApplicationDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

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
    }
}
