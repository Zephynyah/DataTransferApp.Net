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

        public static readonly DependencyProperty IsBlacklistedProperty =
            DependencyProperty.Register(
                nameof(IsBlacklisted),
                typeof(bool),
                typeof(AwesomeToolTip),
                new PropertyMetadata(false));
        public static readonly DependencyProperty IsCompessedProperty =
            DependencyProperty.Register(
                nameof(IsCompessed),
                typeof(bool),
                typeof(AwesomeToolTip),
                new PropertyMetadata(false));

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

        public bool IsCompessed
        {
            get => (bool)GetValue(IsCompessedProperty);
            set => SetValue(IsCompessedProperty, value);
        }

        public AwesomeToolTip()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}