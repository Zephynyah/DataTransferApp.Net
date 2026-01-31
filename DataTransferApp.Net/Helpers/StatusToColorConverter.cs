using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media; // For System.Windows.Media.Color and Brushes

namespace DataTransferApp.Net.Helpers
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                switch (status.ToLower())
                {
                    case "pass":
                        return Brushes.Green;
                    case "fail":
                        return Brushes.Red;
                    case "caution":
                        return Brushes.Yellow;
                    default:
                        return Brushes.Black; // Default brush for unknown status
                }
            }

            return Brushes.Gray; // Default brush if value is null or not a string
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not needed for this scenario (string to brush), but required by interface
            throw new NotSupportedException();
        }
    }
}