using System.IO;
using System.Linq;
using System.Text;

namespace DataTransferApp.Net.Helpers
{
    public static class FileEncodingHelper
    {
        private const int SAMPLE_SIZE = 8192; // 8KB sample
        private const long MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB limit

        /// <summary>
        /// Determines if a file appears to be a text file that can be safely opened in a viewer
        /// by checking if it's ASCII-compatible and doesn't contain binary data patterns.
        /// </summary>
        public static bool IsTextFile(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);

                // Special case: .md files are always considered text (they're markdown)
                if (fileInfo.Extension.ToLower() == ".md")
                    return true;

                // Skip very large files
                if (fileInfo.Length > MAX_FILE_SIZE)
                    return false;

                // Skip empty files
                if (fileInfo.Length == 0)
                    return true;

                // Read only a sample of the file
                int bytesToRead = (int)Math.Min(SAMPLE_SIZE, fileInfo.Length);
                byte[] buffer = new byte[bytesToRead];

                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    int bytesRead = stream.Read(buffer, 0, bytesToRead);
                    return IsTextBuffer(buffer, bytesRead);
                }
            }
            catch
            {
                // If we can't read the file, assume it's not text
                return false;
            }
        }

        private static bool IsTextBuffer(byte[] buffer, int length)
        {
            // Check for BOM (Byte Order Mark)
            if (length >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
            {
                // UTF-8 BOM, skip it
                buffer = buffer.Skip(3).ToArray();
                length -= 3;
            }
            else if (length >= 2 && buffer[0] == 0xFF && buffer[1] == 0xFE)
            {
                // UTF-16 LE BOM, likely binary
                return false;
            }
            else if (length >= 2 && buffer[0] == 0xFE && buffer[1] == 0xFF)
            {
                // UTF-16 BE BOM, likely binary
                return false;
            }

            // Count null bytes (common in binary files)
            int nullByteCount = 0;
            int totalChars = 0;

            for (int i = 0; i < length; i++)
            {
                if (buffer[i] == 0)
                    nullByteCount++;

                // Check for non-ASCII characters
                if (buffer[i] > 127)
                {
                    // Allow some extended ASCII, but be conservative
                    if (buffer[i] < 160) // Control characters in extended range
                        return false;
                }

                totalChars++;
            }

            // If more than 10% null bytes, likely binary
            if (nullByteCount > totalChars * 0.1)
                return false;

            // Try to decode as UTF-8 and validate
            try
            {
                string sample = Encoding.UTF8.GetString(buffer, 0, length);

                // Very permissive validation: just ensure it's decodable and not mostly control chars
                int controlCharCount = sample.Count(c => c < 32 && c != 9 && c != 10 && c != 13 && c != 12 && c != 11);
                int sampleLength = sample.Length;

                // Allow up to 5% control characters (for binary safety)
                return controlCharCount <= sampleLength * 0.05;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Legacy method - kept for compatibility but IsTextFile is preferred
        /// </summary>
        public static bool IsAsciiFileDotNet6(string filePath)
        {
            try
            {
                // Only check small files
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > MAX_FILE_SIZE || fileInfo.Length == 0)
                    return false;

                string content = File.ReadAllText(filePath, Encoding.UTF8);
                return content.All(char.IsAscii);
            }
            catch
            {
                return false;
            }
        }
    }
}