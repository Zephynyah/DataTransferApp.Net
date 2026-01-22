using DataTransferApp.Net.Services;
using DataTransferApp.Net.ViewModels;
using System.Windows;

namespace DataTransferApp.Net.Views
{
    public partial class TransferHistoryWindow : Window
    {
        public TransferHistoryWindow(string? databasePath)
        {
            InitializeComponent();
            DataContext = new TransferHistoryViewModel(databasePath);
        }
    }
}
