using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using DataTransferApp.Net.Helpers;
using Markdig;

namespace DataTransferApp.Net.ViewModels
{
    public class ChangesViewModel : ViewModelBase
    {
        private string _markdownContent = string.Empty;

        public string MarkdownContent
        {
            get => _markdownContent;
            set
            {
                _markdownContent = value;
                OnPropertyChanged(nameof(MarkdownContent));
            }
        }

        public ChangesViewModel()
        {
            // Enable advanced extensions for GitHub-style rendering
            Pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            LoadReadme();
        }

        public MarkdownPipeline Pipeline { get; }

        private void LoadReadme()
        {
            // Load CHANGELOG.md from embedded resources
            MarkdownContent = ResourceHelper.LoadEmbeddedResource("DataTransferApp.Net.Resources.CHANGELOG.md");
        }

        private void OpenHyperlink(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is string url && !string.IsNullOrEmpty(url))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
        }
    }
}