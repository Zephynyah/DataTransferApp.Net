using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;


namespace DataTransferApp.Net.Helpers
{
    public class ValueToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double numericValue)
            {
                if (numericValue > 100)
                    return Brushes.Red;
                else if (numericValue < 50)
                    return Brushes.Blue;
                else
                    return Brushes.Green;
            }
            return Brushes.Black; // Default color
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
