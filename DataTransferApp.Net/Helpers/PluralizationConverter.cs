using System;
using System.Globalization;
using System.Windows.Data;

namespace DataTransferApp.Net.Helpers
{
    public class PluralizationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int number && parameter is string format)
            {
                // Format should be "singular|plural" like "Day|Days"
                var parts = format.Split('|');
                if (parts.Length == 2)
                {
                    var unit = number == 1 ? parts[0] : parts[1];
                    return $"Policy is set for {number} {unit}";
                }
            }

            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}