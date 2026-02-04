using System;
using System.Globalization;
using System.Windows.Data;

namespace DataTransferApp.Net.Helpers
{
    public class BoolToWidthConverter : IValueConverter
    {
        public string TrueValue { get; set; } = "*";
        public string FalseValue { get; set; } = "0";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? TrueValue : FalseValue;
            }
            return FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}