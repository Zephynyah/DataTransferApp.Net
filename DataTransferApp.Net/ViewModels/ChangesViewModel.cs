using System.ComponentModel;
using System.IO;
using Markdig;
using DataTransferApp.Net.Helpers;
// Make sure to include any other necessary namespaces
using System.Diagnostics;


namespace DataTransferApp.Net.ViewModels
{
    public class ChangesViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private string _markdownContent = string.Empty;
        public string MarkdownContent
        {
            get => _markdownContent;
            set { _markdownContent = value; OnPropertyChanged(nameof(MarkdownContent)); }
        }

        public MarkdownPipeline Pipeline { get; }

        public ChangesViewModel()
        {
            // Enable advanced extensions for GitHub-style rendering
            Pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            LoadReadme();
        }

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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}