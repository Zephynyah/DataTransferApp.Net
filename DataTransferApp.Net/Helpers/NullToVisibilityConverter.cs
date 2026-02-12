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
                if (string.IsNullOrEmpty(str))
                {
                    return invert ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    return invert ? Visibility.Collapsed : Visibility.Visible;
                }
            }

            if (value == null)
            {
                return invert ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return invert ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}