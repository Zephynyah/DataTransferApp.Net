using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataTransferApp.Net.Models;
using DataTransferApp.Net.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DataTransferApp.Net.ViewModels
{
    public partial class TransferHistoryViewModel : ObservableObject
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

        public TransferHistoryViewModel(string transferLogsDirectory)
        {
            _historyService = new TransferHistoryService(transferLogsDirectory);
            _ = LoadTransfersAsync();
        }

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
        }
    }
}
