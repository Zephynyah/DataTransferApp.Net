namespace DataTransferApp.Net.Helpers
{
    /// <summary>
    /// Helper class for formatting file sizes in human-readable format.
    /// </summary>
    public static class FileSizeHelper
    {
        /// <summary>
        /// Formats a file size in bytes to a human-readable string (GB, MB, KB, or B).
        /// </summary>
        /// <param name="bytes">The file size in bytes.</param>
        /// <returns>A formatted string representing the file size.</returns>
        public static string FormatFileSize(long bytes)
        {
            const long GB = AppConstants.BytesPerGB;
            const long MB = AppConstants.BytesPerMB;
            const long KB = AppConstants.BytesPerKB;

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

            return $"{bytes} B";
        }
    }
}
