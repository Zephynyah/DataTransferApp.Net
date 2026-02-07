using System.Windows;
using System.Windows.Controls;

namespace DataTransferApp.Net.Controls
{
    /// <summary>
    /// Interaction logic for AwesomeToolTip.xaml
    /// </summary>
    public partial class AwesomeToolTip : UserControl
    {
        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register(
                nameof(FileName),
                typeof(string),
                typeof(AwesomeToolTip),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty FullPathProperty =
            DependencyProperty.Register(
                nameof(FullPath),
                typeof(string),
                typeof(AwesomeToolTip),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty SizeFormattedProperty =
            DependencyProperty.Register(
                nameof(SizeFormatted),
                typeof(string),
                typeof(AwesomeToolTip),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(
                nameof(Description),
                typeof(string),
                typeof(AwesomeToolTip),
                new PropertyMetadata(string.Empty));

        public string FileName
        {
            get => (string)GetValue(FileNameProperty);
            set => SetValue(FileNameProperty, value);
        }

        public string FullPath
        {
            get => (string)GetValue(FullPathProperty);
            set => SetValue(FullPathProperty, value);
        }
        public string SizeFormatted
        {
            get => (string)GetValue(SizeFormattedProperty);
            set => SetValue(SizeFormattedProperty, value);
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