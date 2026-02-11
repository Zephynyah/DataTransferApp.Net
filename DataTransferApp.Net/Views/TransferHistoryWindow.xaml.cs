using System.Windows;
using DataTransferApp.Net.Services;
using DataTransferApp.Net.ViewModels;

namespace DataTransferApp.Net.Views
{
    /// <summary>
    /// Window for displaying and managing transfer history records.
    /// </summary>
    public partial class TransferHistoryWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransferHistoryWindow"/> class.
        /// </summary>
        /// <param name="databasePath">The path to the transfer history database.</param>
        public TransferHistoryWindow(string? databasePath)
        {
            InitializeComponent();
            DataContext = new TransferHistoryViewModel(databasePath);
        }
    }
}