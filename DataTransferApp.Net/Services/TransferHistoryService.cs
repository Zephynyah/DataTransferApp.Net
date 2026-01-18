using DataTransferApp.Net.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataTransferApp.Net.Services
{
    public class TransferHistoryService
    {
        private readonly string _transferLogsDirectory;

        public TransferHistoryService(string transferLogsDirectory)
        {
            _transferLogsDirectory = transferLogsDirectory;
        }

        public async Task<List<TransferLog>> GetAllTransfersAsync()
        {
            var transfers = new List<TransferLog>();

            try
            {
                if (!Directory.Exists(_transferLogsDirectory))
                {
                    return transfers;
                }

                var logFiles = Directory.GetFiles(_transferLogsDirectory, "*.json")
                    .OrderByDescending(f => File.GetCreationTime(f))
                    .ToList();

                foreach (var logFile in logFiles)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(logFile);
                        var transfer = JsonSerializer.Deserialize<TransferLog>(json);
                        
                        if (transfer != null)
                        {
                            transfers.Add(transfer);
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warning($"Failed to load transfer log: {logFile} - {ex.Message}");
                    }
                }

                LoggingService.Info($"Loaded {transfers.Count} transfer records");
            }
            catch (Exception ex)
            {
                LoggingService.Error("Error loading transfer history", ex);
            }

            return transfers;
        }

        public async Task<List<TransferLog>> SearchTransfersAsync(string searchTerm)
        {
            var allTransfers = await GetAllTransfersAsync();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return allTransfers;
            }

            searchTerm = searchTerm.ToLower();

            return allTransfers.Where(t =>
                t.TransferInfo.FolderName.ToLower().Contains(searchTerm) ||
                t.TransferInfo.Employee.ToLower().Contains(searchTerm) ||
                t.TransferInfo.DTA.ToLower().Contains(searchTerm) ||
                t.TransferInfo.Origin.ToLower().Contains(searchTerm) ||
                t.TransferInfo.Destination.ToLower().Contains(searchTerm)
            ).ToList();
        }

        public async Task<TransferLog?> GetTransferByIdAsync(string id)
        {
            var transfers = await GetAllTransfersAsync();
            return transfers.FirstOrDefault(t => t.Id.ToString() == id);
        }

        public async Task<List<TransferLog>> GetTransfersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var allTransfers = await GetAllTransfersAsync();

            return allTransfers.Where(t =>
                t.TransferInfo.Date >= startDate &&
                t.TransferInfo.Date <= endDate
            ).ToList();
        }

        public async Task<List<TransferLog>> GetRecentTransfersAsync(int count = 10)
        {
            var allTransfers = await GetAllTransfersAsync();
            return allTransfers.OrderByDescending(t => t.TransferInfo.Date)
                .Take(count)
                .ToList();
        }

        public async Task<Dictionary<string, int>> GetTransferStatisticsAsync()
        {
            var transfers = await GetAllTransfersAsync();

            return new Dictionary<string, int>
            {
                ["TotalTransfers"] = transfers.Count,
                ["TodayTransfers"] = transfers.Count(t => t.TransferInfo.Date.Date == DateTime.Today),
                ["ThisWeekTransfers"] = transfers.Count(t => t.TransferInfo.Date >= DateTime.Now.AddDays(-7)),
                ["ThisMonthTransfers"] = transfers.Count(t => t.TransferInfo.Date.Month == DateTime.Now.Month && 
                                                              t.TransferInfo.Date.Year == DateTime.Now.Year),
                ["TotalFiles"] = transfers.Sum(t => t.Summary.TotalFiles)
            };
        }
    }
}
