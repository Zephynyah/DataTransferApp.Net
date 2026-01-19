using System;
using System.Globalization;
using System.Windows.Data;
using DataTransferApp.Net.Models;

namespace DataTransferApp.Net.Helpers
{
    /// <summary>
    /// Converter to provide icon path based on file type
    /// </summary>
    public class FileIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FileData file)
            {
                // Return geometry key for compressed files
                if (file.IsCompressed)
                {
                    return "M 2,2 L 14,2 L 14,14 L 2,14 Z M 4,4 L 12,4 L 12,12 L 4,12 Z M 6,6 L 10,6 M 6,8 L 10,8 M 6,10 L 10,10"; // Archive/zip icon
                }
                
                // Return geometry key for blacklisted files
                if (file.IsBlacklisted)
                {
                    return "M 8,1 L 15,8 L 8,15 L 1,8 Z M 8,4 L 8,9 M 8,11 L 8,12"; // Warning icon
                }
                
                // Default file icon
                return "M 6,2 L 14,2 L 14,16 L 2,16 L 2,6 Z M 6,2 L 6,6 L 2,6";
            }
            
            return "M 6,2 L 14,2 L 14,16 L 2,16 L 2,6 Z M 6,2 L 6,6 L 2,6"; // Default file icon
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
