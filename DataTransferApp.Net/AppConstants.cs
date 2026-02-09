namespace DataTransferApp.Net
{
    /// <summary>
    /// Application-wide constants and reusable values.
    /// </summary>
    public static class AppConstants
    {
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
    }
}
