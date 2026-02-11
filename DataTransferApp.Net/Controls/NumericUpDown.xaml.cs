using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DataTransferApp.Net.Controls;

/// <summary>
/// A custom numeric up/down control for WPF.
/// </summary>
public partial class NumericUpDown : UserControl
{
    private static readonly Regex _numericRegex = new Regex("[^0-9.-]+", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

    public NumericUpDown()
    {
        InitializeComponent();
    }

    #region Dependency Properties

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(decimal),
            typeof(NumericUpDown),
            new FrameworkPropertyMetadata(
                0m,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged,
                CoerceValue));

    public decimal Value
    {
        get => (decimal)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(
            nameof(Minimum),
            typeof(decimal),
            typeof(NumericUpDown),
            new PropertyMetadata(0m, OnMinMaxChanged));

    public decimal Minimum
    {
        get => (decimal)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
            nameof(Maximum),
            typeof(decimal),
            typeof(NumericUpDown),
            new PropertyMetadata(100m, OnMinMaxChanged));

    public decimal Maximum
    {
        get => (decimal)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly DependencyProperty IncrementProperty =
        DependencyProperty.Register(
            nameof(Increment),
            typeof(decimal),
            typeof(NumericUpDown),
            new PropertyMetadata(1m));

    public decimal Increment
    {
        get => (decimal)GetValue(IncrementProperty);
        set => SetValue(IncrementProperty, value);
    }

    public static readonly DependencyProperty DecimalPlacesProperty =
        DependencyProperty.Register(
            nameof(DecimalPlaces),
            typeof(int),
            typeof(NumericUpDown),
            new PropertyMetadata(0));

    public int DecimalPlaces
    {
        get => (int)GetValue(DecimalPlacesProperty);
        set => SetValue(DecimalPlacesProperty, value);
    }

    #endregion

    #region Property Changed Callbacks

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (NumericUpDown)d;
        control.CoerceValue(ValueProperty);
    }

    private static void OnMinMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (NumericUpDown)d;
        // Coerce the current value to ensure it's still within the new Min/Max bounds
        var currentValue = control.Value;
        control.Value = (decimal)CoerceValue(control, currentValue);
    }

    private static object CoerceValue(DependencyObject d, object baseValue)
    {
        var control = (NumericUpDown)d;
        var value = (decimal)baseValue;

        if (value < control.Minimum)
            return control.Minimum;
        if (value > control.Maximum)
            return control.Maximum;

        return Math.Round(value, control.DecimalPlaces);
    }

    #endregion

    #region Event Handlers

    private void UpButton_Click(object sender, RoutedEventArgs e)
    {
        if (Value + Increment <= Maximum)
        {
            Value += Increment;
        }
    }

    private void DownButton_Click(object sender, RoutedEventArgs e)
    {
        if (Value - Increment >= Minimum)
        {
            Value -= Increment;
        }
    }

    private void ValueTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Allow only numeric input
        e.Handled = _numericRegex.IsMatch(e.Text);
    }

    private void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Validate and update value
        if (sender is TextBox textBox && !string.IsNullOrEmpty(textBox.Text))
        {
            if (decimal.TryParse(textBox.Text, out decimal result))
            {
                Value = result;
            }
        }
    }

    #endregion

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        switch (e.Key)
        {
            case Key.Up:
                UpButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
                break;
            case Key.Down:
                DownButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
                break;
        }
    }
}
