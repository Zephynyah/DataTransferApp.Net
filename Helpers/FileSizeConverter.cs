using System;
using System.Globalization;
using System.Windows.Data;

namespace DataTransferApp.Net.Helpers
{
    public class FileSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long bytes)
            {
                return FormatFileSize(bytes);
            }
            return "0 B";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static string FormatFileSize(long bytes)
        {
            const long GB = 1024L * 1024L * 1024L;
            const long MB = 1024L * 1024L;
            const long KB = 1024L;

            if (bytes >= GB)
                return $"{bytes / (double)GB:N2} GB";
            if (bytes >= MB)
                return $"{bytes / (double)MB:N2} MB";
            if (bytes >= KB)
                return $"{bytes / (double)KB:N2} KB";

            return $"{bytes} B";
        }
    }
}
