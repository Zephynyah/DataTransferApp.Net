using System.Globalization;
using System.Windows.Data;

namespace DataTransferApp.Net.Helpers;

/// <summary>
/// Converter that returns true (enabled) only when RoboSharpPresetMode is "Manual".
/// Used to disable performance controls when a preset is selected.
/// </summary>
public class PresetModeToEnabledConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string presetMode)
        {
            return presetMode.Equals("Manual", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("PresetModeToEnabledConverter does not support ConvertBack.");
    }
}