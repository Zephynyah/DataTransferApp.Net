using System.Globalization;
using System.Windows;
using System.Windows.Data;
using FontAwesome.Sharp;

namespace DataTransferApp.Net.Helpers;

/// <summary>
/// Converter that returns Collapsed when the IconChar is None, otherwise Visible.
/// </summary>
public class IconToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IconChar icon && icon != IconChar.None)
        {
            return Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("This converter only supports one-way binding.");
    }
}
