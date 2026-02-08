using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using DataTransferApp.Net.Helpers;
using FontAwesome.Sharp;

namespace DataTransferApp.Net.Controls
{

    /// <summary>
    /// Interaction logic for AwesomeDialog.xaml.
    /// </summary>
    public partial class AwesomeDialog : UserControl
    {
        public event EventHandler<DialogResultEventArgs>? OnResult; // True for confirm, false for cancel

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(AwesomeDialog),
            new PropertyMetadata("Dialog Title"));

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(AwesomeDialog),
            new PropertyMetadata("Your message here."));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
            nameof(Icon),
            typeof(IconChar),
            typeof(AwesomeDialog),
            new PropertyMetadata(IconChar.QuestionCircle));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }
        public IconChar Icon
        {
            get => (IconChar)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public AwesomeDialog()
        {
            InitializeComponent();

            // Handle visibility changes
            IsVisibleChanged += (s, e) =>
            {
                if ((bool)e.NewValue) // If Visibility == Visible
                {
                    var sb = (Storyboard)Resources["PopInAnimation"];
                    sb.Begin();

                    // Set focus to the dialog for keyboard support
                    _ = Dispatcher.BeginInvoke(
                        new Action(() =>
                        {
                            Focus();
                            Keyboard.Focus(this);
                        }),
                        System.Windows.Threading.DispatcherPriority.Input);
                }
            };

            // Keyboard support (Escape = Cancel, Enter = Confirm)
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    e.Handled = true;
                    Cancel_Click(this, new RoutedEventArgs());
                }
                else if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    Confirm_Click(this, new RoutedEventArgs());
                }
            };
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            AnimateOut(() => OnResult?.Invoke(this, new DialogResultEventArgs(true)));
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            AnimateOut(() => OnResult?.Invoke(this, new DialogResultEventArgs(false)));
        }

        private void OnOverlayClicked(object sender, MouseButtonEventArgs e)
        {
            AnimateOut(() => OnResult?.Invoke(this, new DialogResultEventArgs(false)));
        }

        private void AnimateOut(Action onComplete)
        {
            var sb = (Storyboard)Resources["PopOutAnimation"];
            sb.Completed += (s, e) =>
            {
                Visibility = Visibility.Collapsed;
                onComplete?.Invoke();
            };
            sb.Begin();
        }
    }
}