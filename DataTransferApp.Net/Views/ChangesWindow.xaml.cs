using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using DataTransferApp.Net.ViewModels;

namespace DataTransferApp.Net.Views
{
    /// <summary>
    /// Interaction logic for ChangesWindow.xaml
    /// </summary>
    public partial class ChangesWindow : Window
    {
        public ChangesWindow()
        {
            InitializeComponent();
            DataContext = new ChangesViewModel();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {

            // Thickness(0, 0, 0, 5):
            //-------------------------
            // Left: 0 (no margin/padding on the left side)
            // Top: 0 (no margin/padding on the top side)
            // Right: 0 (no margin/padding on the right side)
            // Bottom: 5 (5 units of margin/padding on the bottom side)
            //-------------------------
            // Customize the FlowDocument styles to reduce vertical gaps
            if (MarkdownViewer.Document != null)
            {
                foreach (var block in MarkdownViewer.Document.Blocks)
                {
                    // Reduce margins for paragraphs (text blocks in markdown) to tighten spacing between lines and sections
                    if (block is Paragraph para)
                    {
                        para.Margin = new Thickness(0, 0, 0, 5); // Reduce bottom margin
                        para.LineHeight = 1.2; // Adjust line height
                    }
                    // Remove margins for sections (major structural elements like headings) to eliminate extra spacing
                    else if (block is Section section)
                    {
                        section.Margin = new Thickness(0);
                    }
                    // Adjust margins for lists (bullet points and numbered lists in markdown) to compact list items
                    else if (block is List list)
                    {
                        list.Margin = new Thickness(0, 0, 0, 5);
                    }
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OpenHyperlink(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is string url && !string.IsNullOrEmpty(url))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
        }
    }
}