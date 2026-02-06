using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataTransferApp.Net.Models;
using DataTransferApp.Net.Services;

namespace DataTransferApp.Net.ViewModels
{
    public partial class TransferHistoryViewModel : ViewModelBase
    {
        private readonly TransferHistoryService _historyService;

        [ObservableProperty]
        private ObservableCollection<TransferLog> _transfers = new();

        [ObservableProperty]
        private TransferLog? _selectedTransfer;

        [ObservableProperty]
        private ObservableCollection<TransferredFile> _transferFiles = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private int _totalTransfers;

        [ObservableProperty]
        private int _todayTransfers;

        [ObservableProperty]
        private int _thisWeekTransfers;

        [ObservableProperty]
        private int _totalFiles;

        [ObservableProperty]
        private string _statusMessage = "Ready";

        [ObservableProperty]
        private string _historyDbLocation;

        [ObservableProperty]
        private string _historyDbHealth = "Checking...";

        [ObservableProperty]
        private bool _isDeleteEnabled;

        [ObservableProperty]
        private bool _showTableView = false;

        public TransferHistoryViewModel(string? databasePath)
        {
            _historyService = new TransferHistoryService(databasePath);
            HistoryDbLocation = _historyService.GetDatabasePath();
            _ = CheckDbHealthAsync();
            _ = LoadTransfersAsync();
        }

        public string PageTitle { get; } = "Transfer History";

        public string PageDescription { get; } = "View all transferred folders and files";

        [RelayCommand]
        private async Task LoadTransfersAsync()
        {
            IsLoading = true;
            StatusMessage = "Loading transfer history...";

            try
            {
                var transfers = await _historyService.GetAllTransfersAsync();
                Transfers = new ObservableCollection<TransferLog>(transfers);

                var stats = await _historyService.GetTransferStatisticsAsync();
                TotalTransfers = stats["TotalTransfers"];
                TodayTransfers = stats["TodayTransfers"];
                ThisWeekTransfers = stats["ThisWeekTransfers"];
                TotalFiles = stats["TotalFiles"];

                StatusMessage = $"Loaded {TotalTransfers} transfers";

                // Automatically select the first item if any exist
                if (Transfers.Count > 0)
                {
                    SelectedTransfer = Transfers[0];
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading transfers: {ex.Message}";
                LoggingService.Error("Failed to load transfer history", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SearchTransfersAsync()
        {
            IsLoading = true;
            StatusMessage = "Searching...";

            try
            {
                var results = await _historyService.SearchTransfersAsync(SearchText);
                Transfers = new ObservableCollection<TransferLog>(results);
                StatusMessage = $"Found {results.Count} transfers";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Search error: {ex.Message}";
                LoggingService.Error("Failed to search transfers", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ViewTransferDetails()
        {
            if (SelectedTransfer != null)
            {
                TransferFiles = new ObservableCollection<TransferredFile>(SelectedTransfer.Files);
            }
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = string.Empty;
            _ = LoadTransfersAsync();
        }

        partial void OnSelectedTransferChanged(TransferLog? value)
        {
            if (value != null)
            {
                TransferFiles = new ObservableCollection<TransferredFile>(value.Files);
            }
            else
            {
                TransferFiles.Clear();
            }

            IsDeleteEnabled = value != null;
        }

        private async Task CheckDbHealthAsync()
        {
            try
            {
                await _historyService.GetTransferStatisticsAsync();
                HistoryDbHealth = "Healthy";
            }
            catch (Exception ex)
            {
                HistoryDbHealth = $"Error: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task DeleteTransferAsync()
        {
            if (SelectedTransfer == null)
            {
                return;
            }

            var result = System.Windows.MessageBox.Show(
                $"Are you sure you want to delete the transfer record for '{SelectedTransfer.TransferInfo.FolderName}'?",
                "Confirm Delete",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    var success = await _historyService.DeleteTransferAsync(SelectedTransfer.Id.ToString());
                    if (success)
                    {
                        Transfers.Remove(SelectedTransfer);
                        SelectedTransfer = Transfers.FirstOrDefault();
                        StatusMessage = "Transfer deleted successfully";
                    }
                    else
                    {
                        StatusMessage = "Failed to delete transfer";
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error deleting transfer: {ex.Message}";
                    LoggingService.Error("Failed to delete transfer", ex);
                }
            }
        }
    }
}