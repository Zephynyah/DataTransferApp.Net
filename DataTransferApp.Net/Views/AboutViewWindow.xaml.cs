using System.Windows;

namespace DataTransferApp.Net.Views
{
    public partial class AboutViewWindow : Window
    {
        public AboutViewWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
