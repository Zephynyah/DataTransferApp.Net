using System.Windows;

namespace DataTransferApp.Net.Views
{
    /// <summary>
    /// Window for viewing file content with file name and path information.
    /// </summary>
    public partial class FileViewerWindow : Window
    {
        public FileViewerWindow(string fileName, string filePath, string content)
        {
            InitializeComponent();

            FileNameText.Text = fileName;
            FilePathText.Text = filePath;
            FileContentTextBox.Text = content;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}