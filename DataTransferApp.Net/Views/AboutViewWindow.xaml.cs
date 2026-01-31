using System.Windows;

namespace DataTransferApp.Net.Views
{
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