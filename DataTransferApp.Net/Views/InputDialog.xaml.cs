using System.Windows;

namespace DataTransferApp.Net.Views
{
    /// <summary>
    /// A reusable dialog window for collecting user input with customizable title and prompt.
    /// </summary>
    public partial class InputDialog : Window
    {
        public InputDialog(string title, string prompt)
        {
            InitializeComponent();
            Title = title;
            PromptText.Text = prompt;
        }

        public InputDialog(string title, string prompt, string defaultValue)
        {
            InitializeComponent();
            Title = title;
            PromptText.Text = prompt;
            InputTextBox.Text = defaultValue;
        }

        public string InputText => InputTextBox.Text;

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}