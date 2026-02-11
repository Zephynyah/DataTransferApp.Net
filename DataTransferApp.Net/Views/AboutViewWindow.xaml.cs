using System.Windows;

namespace DataTransferApp.Net.Views
{
    /// <summary>
    /// AboutViewWindow is a dialog window that displays information about the application.
    /// </summary>
    public partial class AboutViewWindow : Window
    {
        public AboutViewWindow(object dataContext)
        {
            InitializeComponent();

            DataContext = dataContext;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}