using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DataTransferApp.Net.Helpers
{
    /// <summary>
    /// Converts audit status strings to colored brushes
    /// </summary>
    public class AuditStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                switch (status.ToLower())
                {
                    case "passed":
                    case "pass":
                        return new SolidColorBrush(Color.FromRgb(46, 204, 113)); // Green
                    
                    case "failed":
                    case "fail":
                        return new SolidColorBrush(Color.FromRgb(231, 76, 60)); // Red
                    
                    case "caution":
                    case "warning":
                        return new SolidColorBrush(Color.FromRgb(241, 196, 15)); // Yellow
                    
                    case "not audited":
                        return new SolidColorBrush(Color.FromRgb(149, 165, 166)); // Gray
                    
                    default:
                        return new SolidColorBrush(Color.FromRgb(52, 73, 94)); // Dark gray
                }
            }
            
            return new SolidColorBrush(Color.FromRgb(149, 165, 166)); // Default gray
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
