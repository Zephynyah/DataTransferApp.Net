using System;
using System.Globalization;
using System.Windows.Data;

namespace DataTransferApp.Net.Helpers;

/// <summary>
/// Converter for binding RadioButton IsChecked to a string property.
/// ConverterParameter specifies the value that should result in IsChecked=true.
/// </summary>
public class RadioButtonCheckedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
        {
            return false;
        }

        return value.ToString() == parameter.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked && parameter != null)
        {
            return parameter.ToString()!;
        }

        return Binding.DoNothing;
    }
}