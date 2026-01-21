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
                    return "M19 9V20H5V9M19 9H5M19 9C19.5523 9 20 8.55228 20 8V5C20 4.44772 19.5523 4 19 4H5C4.44772 4 4 4.44772 4 5V8C4 8.55228 4.44772 9 5 9M10 13H14"; // Archive/zip icon
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
