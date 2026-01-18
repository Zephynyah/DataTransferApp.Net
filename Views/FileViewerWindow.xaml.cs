using System.Windows;

namespace DataTransferApp.Net.Views
{
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
