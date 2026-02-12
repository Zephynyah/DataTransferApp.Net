using System.Globalization;
using System.Windows.Data;

namespace DataTransferApp.Net.Helpers
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return InvertBoolean(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return InvertBoolean(value);
        }

        private static object InvertBoolean(object value)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }

            return false;
        }
    }
}