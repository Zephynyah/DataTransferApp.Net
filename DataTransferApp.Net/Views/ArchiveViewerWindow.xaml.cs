using System.Collections.Generic;
using System.Windows;
using DataTransferApp.Net.Services;

namespace DataTransferApp.Net.Views
{
    public partial class ArchiveViewerWindow : Window
    {
        public ArchiveViewerWindow(string fileName, string filePath, List<ArchiveEntry> entries)
        {
            InitializeComponent();

            ArchiveNameText.Text = fileName;
            ArchivePathText.Text = filePath;
            StatusText.Text = $"Archive contains {entries.Count} file(s)";

            ArchiveDataGrid.ItemsSource = entries;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}