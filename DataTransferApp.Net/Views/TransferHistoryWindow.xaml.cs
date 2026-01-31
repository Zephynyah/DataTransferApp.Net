using System.Windows;
using DataTransferApp.Net.Services;
using DataTransferApp.Net.ViewModels;

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