using System.Windows;
using System.Windows.Controls;

namespace DataTransferApp.Net.Controls
{
    /// <summary>
    /// Interaction logic for AwesomeToolTip.xaml
    /// </summary>
    public partial class AwesomeToolTip : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(AwesomeToolTip),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(
                nameof(Description),
                typeof(string),
                typeof(AwesomeToolTip),
                new PropertyMetadata(string.Empty));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public AwesomeToolTip()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}