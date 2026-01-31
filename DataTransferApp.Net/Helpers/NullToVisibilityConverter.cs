using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DataTransferApp.Net.Helpers
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var invert = parameter != null;
            var str = value as string;
            if (str != null)
            {
                return string.IsNullOrEmpty(str)
                    ? (invert ? Visibility.Visible : Visibility.Collapsed)
                    : (invert ? Visibility.Collapsed : Visibility.Visible);
            }

            return value == null
                ? (invert ? Visibility.Visible : Visibility.Collapsed)
                : (invert ? Visibility.Collapsed : Visibility.Visible);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}