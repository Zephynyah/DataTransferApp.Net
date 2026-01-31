using System;
using System.Globalization;
using System.Windows.Data;
using DataTransferApp.Net.Models;
using FontAwesome.Sharp;

namespace DataTransferApp.Net.Helpers
{
    /// <summary>
    /// Converter to provide FontAwesome icon based on file type.
    /// </summary>
    public class FileToFontAwesomeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FileData file)
            {
                // Return FontAwesome icon for compressed files
                if (file.IsCompressed)
                {
                    return IconChar.Archive; // Archive/zip icon
                }

                // Return FontAwesome icon for blacklisted files
                if (file.IsBlacklisted)
                {
                    return IconChar.ExclamationTriangle; // Warning icon
                }

                // Default file icon
                return IconChar.File;
            }

            return IconChar.File; // Default file icon
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}