using System.Windows;
using System.Windows.Controls;

namespace DataTransferApp.Net.Controls
{
    /// <summary>
    /// Interaction logic for AwesomeListTooltip.xaml.
    /// </summary>
    public partial class AwesomeListTooltip : UserControl
    {
        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register(
                nameof(FileName),
                typeof(string),
                typeof(AwesomeListTooltip),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty FullPathProperty =
            DependencyProperty.Register(
                nameof(FullPath),
                typeof(string),
                typeof(AwesomeListTooltip),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty SizeFormattedProperty =
            DependencyProperty.Register(
                nameof(SizeFormatted),
                typeof(string),
                typeof(AwesomeListTooltip),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IsBlacklistedProperty =
            DependencyProperty.Register(
                nameof(IsBlacklisted),
                typeof(bool),
                typeof(AwesomeListTooltip),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsCompressedProperty =
            DependencyProperty.Register(
                nameof(IsCompressed),
                typeof(bool),
                typeof(AwesomeListTooltip),
                new PropertyMetadata(false));

        public static readonly DependencyProperty ExtensionProperty =
            DependencyProperty.Register(
                nameof(Extension),
                typeof(string),
                typeof(AwesomeListTooltip),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register(
                nameof(ErrorMessage),
                typeof(string),
                typeof(AwesomeListTooltip),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ErrorDetailsProperty =
            DependencyProperty.Register(
                nameof(ErrorDetails),
                typeof(string),
                typeof(AwesomeListTooltip),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty RecommendationsProperty =
            DependencyProperty.Register(
                nameof(Recommendations),
                typeof(IList<string>),
                typeof(AwesomeListTooltip),
                new PropertyMetadata(null));

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

        public bool IsBlacklisted
        {
            get => (bool)GetValue(IsBlacklistedProperty);
            set => SetValue(IsBlacklistedProperty, value);
        }

        public bool IsCompressed
        {
            get => (bool)GetValue(IsCompressedProperty);
            set => SetValue(IsCompressedProperty, value);
        }

        public string Extension
        {
            get => (string)GetValue(ExtensionProperty);
            set => SetValue(ExtensionProperty, value);
        }

        public string ErrorMessage
        {
            get => (string)GetValue(ErrorMessageProperty);
            set => SetValue(ErrorMessageProperty, value);
        }

        public string ErrorDetails
        {
            get => (string)GetValue(ErrorDetailsProperty);
            set => SetValue(ErrorDetailsProperty, value);
        }

        public IList<string> Recommendations
        {
            get => (IList<string>)GetValue(RecommendationsProperty);
            set => SetValue(RecommendationsProperty, value);
        }

        public AwesomeListTooltip()
        {
            InitializeComponent();
        }
    }
}