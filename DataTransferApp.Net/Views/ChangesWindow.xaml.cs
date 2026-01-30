using System.Windows;
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
    }
}