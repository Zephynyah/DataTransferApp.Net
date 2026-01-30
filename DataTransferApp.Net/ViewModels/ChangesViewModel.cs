using System.ComponentModel;
using System.IO;
using Markdig;

namespace DataTransferApp.Net.ViewModels
{
    public class ChangesViewModel : INotifyPropertyChanged
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
            // Load your CHANGELOG.md file (ensure it's in the output directory or project root)
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "CHANGELOG.md");
            if (File.Exists(path))
            {
                MarkdownContent = File.ReadAllText(path);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}