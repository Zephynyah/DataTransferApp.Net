using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DataTransferApp.Net.Models;

namespace DataTransferApp.Net.Helpers
{
    /// <summary>
    /// Converter to provide background color for DataGrid rows based on file status.
    /// </summary>
    public class FileRowBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FileData file)
            {
                // Blacklisted files get red background
                if (file.IsBlacklisted)
                {
                    return new SolidColorBrush(Color.FromArgb(100, 231, 76, 60)); // Light red with transparency
                }

                // Compressed files get yellow background
                if (file.IsCompressed)
                {
                    return new SolidColorBrush(Color.FromArgb(100, 241, 196, 15)); // Light yellow with transparency
                }
            }

            // Default - transparent (let AlternatingRowBackground show through)
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}