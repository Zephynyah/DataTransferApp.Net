using System.Windows;
using System.Windows.Controls;
using FontAwesome.Sharp;

namespace DataTransferApp.Net.Controls;

/// <summary>
/// A reusable label control with an icon and tooltip.
/// </summary>
public partial class LabelWithTooltip : UserControl
{
    public LabelWithTooltip()
    {
        InitializeComponent();
    }

    /// <summary>
    /// The FontAwesome icon to display.
    /// </summary>
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(IconChar),
            typeof(LabelWithTooltip),
            new PropertyMetadata(IconChar.InfoCircle));

    public IconChar Icon
    {
        get => (IconChar)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// The label text to display (e.g., "Staging Directory:").
    /// </summary>
    public static readonly DependencyProperty LabelTextProperty =
        DependencyProperty.Register(
            nameof(LabelText),
            typeof(string),
            typeof(LabelWithTooltip),
            new PropertyMetadata(string.Empty));

    public string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    /// <summary>
    /// The tooltip title.
    /// </summary>
    public static readonly DependencyProperty TooltipTitleProperty =
        DependencyProperty.Register(
            nameof(TooltipTitle),
            typeof(string),
            typeof(LabelWithTooltip),
            new PropertyMetadata(string.Empty));

    public string TooltipTitle
    {
        get => (string)GetValue(TooltipTitleProperty);
        set => SetValue(TooltipTitleProperty, value);
    }

    /// <summary>
    /// The tooltip description.
    /// </summary>
    public static readonly DependencyProperty TooltipDescriptionProperty =
        DependencyProperty.Register(
            nameof(TooltipDescription),
            typeof(string),
            typeof(LabelWithTooltip),
            new PropertyMetadata(string.Empty));

    public string TooltipDescription
    {
        get => (string)GetValue(TooltipDescriptionProperty);
        set => SetValue(TooltipDescriptionProperty, value);
    }
}
