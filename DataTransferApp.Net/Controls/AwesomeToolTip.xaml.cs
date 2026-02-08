using System.Windows;
using System.Windows.Controls;
using FontAwesome.Sharp;

namespace DataTransferApp.Net.Controls
{
    /// <summary>
    /// Interaction logic for AnimationToolTip.xaml.
    /// </summary>
    public partial class AwesomeToolTip : UserControl
    {
        // The "Title" Property
        public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(AwesomeToolTip),
            new PropertyMetadata("Default Title"));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        // The "Description" Property
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(AwesomeToolTip),
            new PropertyMetadata("Default description text."));

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
                nameof(Icon),
                typeof(IconChar),
                typeof(AwesomeToolTip),
                new PropertyMetadata(IconChar.InfoCircle));

        public IconChar Icon
        {
            get => (IconChar)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public AwesomeToolTip()
        {
            InitializeComponent();
        }
    }
}