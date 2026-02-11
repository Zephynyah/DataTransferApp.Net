using System.Windows;
using System.Windows.Controls;
using FontAwesome.Sharp;

namespace DataTransferApp.Net.Controls;

/// <summary>
/// A settings item control with title, description, and a footer control.
/// Similar to Avalonia UI's SettingsExpanderItem.
/// </summary>
public partial class SettingsItem : UserControl
{
    public SettingsItem()
    {
        InitializeComponent();
    }

    /// <summary>
    /// The FontAwesome icon to display next to the title (optional).
    /// </summary>
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(IconChar),
            typeof(SettingsItem),
            new PropertyMetadata(IconChar.None));

    public IconChar Icon
    {
        get => (IconChar)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// The main title/label for the setting.
    /// </summary>
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(SettingsItem),
            new PropertyMetadata(string.Empty));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// The description/explanation for the setting.
    /// </summary>
    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(SettingsItem),
            new PropertyMetadata(string.Empty));

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// The control to display on the right side (e.g., CheckBox, TextBox, ComboBox).
    /// </summary>
    public static readonly DependencyProperty FooterProperty =
        DependencyProperty.Register(
            nameof(Footer),
            typeof(object),
            typeof(SettingsItem),
            new PropertyMetadata(null));

    public object Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }
}
