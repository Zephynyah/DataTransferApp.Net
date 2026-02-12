using System.Globalization;
using System.Text;
using System.Windows.Data;
using DataTransferApp.Net.Models;

namespace DataTransferApp.Net.Helpers
{
    /// <summary>
    /// Converter to generate detailed error tooltips for files with issues.
    /// Provides contextual information including file path, error type, and remediation hints.
    /// </summary>
    public class FileErrorTooltipConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not FileData file || (!file.HasError && !file.IsCompressed))
            {
                return null;
            }

            var tooltip = new StringBuilder();

            // File header
            AppendFileHeader(tooltip, file);

            // Error details
            AppendErrorDetails(tooltip, file);

            // Status
            AppendStatus(tooltip, file);

            return tooltip.ToString().TrimEnd();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static void AppendFileHeader(StringBuilder tooltip, FileData file)
        {
            tooltip.AppendLine($"📄 {file.FileName}");
            tooltip.AppendLine(new string('─', Math.Min(50, file.FileName.Length + 3)));
            tooltip.AppendLine($"Path: {file.FullPath}");
            tooltip.AppendLine($"Size: {file.SizeFormatted}");
            tooltip.AppendLine();
        }

        private static void AppendErrorDetails(StringBuilder tooltip, FileData file)
        {
            if (file.IsBlacklisted)
            {
                tooltip.AppendLine("❌ BLACKLISTED FILE");
                tooltip.AppendLine($"Extension '{file.Extension}' is not allowed for transfer.");
                tooltip.AppendLine();
                tooltip.AppendLine("💡 Recommended Actions:");
                tooltip.AppendLine("  • Convert to an allowed format");
                tooltip.AppendLine("  • Remove from transfer folder");
                tooltip.AppendLine("  • Request extension whitelist approval");
            }
            else if (file.IsCompressed)
            {
                tooltip.AppendLine("⚠️ COMPRESSED FILE DETECTED");
                tooltip.AppendLine($"Archive format detected: {file.Extension}");
                tooltip.AppendLine();
                tooltip.AppendLine("💡 Recommended Actions:");
                tooltip.AppendLine("  • Use the view button to inspect contents of compressed files.");
                tooltip.AppendLine("  • Verify archive contents with hash before transfer if possible.");
                tooltip.AppendLine("  • Ensure no nested archives");
            }
        }

        private static void AppendStatus(StringBuilder tooltip, FileData file)
        {
            if (!string.IsNullOrEmpty(file.Status) && file.Status != "Ready")
            {
                tooltip.AppendLine();
                tooltip.AppendLine($"Status: {file.Status}");
            }
        }
    }
}
