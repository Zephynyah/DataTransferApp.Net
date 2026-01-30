using System.Diagnostics;
using System.Windows;
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