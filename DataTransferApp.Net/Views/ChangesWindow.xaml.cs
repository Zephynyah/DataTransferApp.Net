using System.IO;
using System.Windows;

namespace DataTransferApp.Net.Views
{
    public partial class ChangesWindow : Window
    {
        public ChangesWindow()
        {
            InitializeComponent();

            // Read and display the changelog
            LoadChangelog();
        }

        private void LoadChangelog()
        {
            try
            {
                // Get the path to the CHANGES.md file
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string changelogPath = Path.Combine(appDirectory, "CHANGES.md");

                // If not found in the same directory (for published app), try going up from project structure
                if (!File.Exists(changelogPath))
                {
                    // Try relative to the project root (for debug builds)
                    changelogPath = Path.Combine(appDirectory, "..", "..", "..", "..", "CHANGES.md");
                }

                if (!File.Exists(changelogPath))
                {
                    // Try another level up
                    changelogPath = Path.Combine(appDirectory, "..", "..", "..", "..", "..", "CHANGES.md");
                }

                if (File.Exists(changelogPath))
                {
                    string changelogContent = File.ReadAllText(changelogPath);
                    DataContext = new { MarkdownContent = changelogContent };
                }
                else
                {
                    // Fallback content if file not found
                    DataContext = new
                    {
                        MarkdownContent = "# Changelog\n\nChangelog file not found. Please check the application installation."
                    };
                }
            }
            catch (Exception ex)
            {
                // Fallback content on error
                DataContext = new
                {
                    MarkdownContent = $"# Changelog\n\nError loading changelog: {ex.Message}"
                };
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}