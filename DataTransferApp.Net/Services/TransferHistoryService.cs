using DataTransferApp.Net.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataTransferApp.Net.Services
{
    /// <summary>
    /// Service for managing transfer history.
    /// Uses LiteDB database as primary storage with fallback to JSON files.
    /// </summary>
    public class TransferHistoryService : IDisposable
    {
        private readonly string _transferRecordsDirectory;
        private readonly TransferDatabaseService _databaseService;

        public TransferHistoryService(string transferRecordsDirectory, string? databasePath = null)
        {
            _transferRecordsDirectory = transferRecordsDirectory;
            _databaseService = new TransferDatabaseService(databasePath);
        }

        /// <summary>
        /// Gets all transfers from the database.
        /// </summary>
        public async Task<List<TransferLog>> GetAllTransfersAsync()
        {
            return await Task.Run(() => _databaseService.GetAllTransfers());
        }

        /// <summary>
        /// Searches for transfers matching the search term.
        /// </summary>
        public async Task<List<TransferLog>> SearchTransfersAsync(string searchTerm)
        {
            return await Task.Run(() => _databaseService.SearchTransfers(searchTerm));
        }

        /// <summary>
        /// Gets a transfer by its ID.
        /// </summary>
        public async Task<TransferLog?> GetTransferByIdAsync(string id)
        {
            return await Task.Run(() =>
            {
                if (LiteDB.ObjectId.TryParse(id, out var objectId))
                {
                    return _databaseService.GetTransferById(objectId);
                }
                return null;
            });
        }

        /// <summary>
        /// Gets transfers within a date range.
        /// </summary>
        public async Task<List<TransferLog>> GetTransfersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() => _databaseService.GetTransfersByDateRange(startDate, endDate));
        }

        /// <summary>
        /// Gets the most recent transfers.
        /// </summary>
        public async Task<List<TransferLog>> GetRecentTransfersAsync(int count = 10)
        {
            return await Task.Run(() => _databaseService.GetRecentTransfers(count));
        }

        /// <summary>
        /// Gets transfer statistics.
        /// </summary>
        public async Task<Dictionary<string, int>> GetTransferStatisticsAsync()
        {
            return await Task.Run(() => _databaseService.GetTransferStatistics());
        }

        /// <summary>
        /// Migrates existing JSON transfer logs to the database.
        /// </summary>
        public async Task<int> MigrateJsonLogsToDatabase()
        {
            var migratedCount = 0;

            try
            {
                if (!Directory.Exists(_transferRecordsDirectory))
                {
                    LoggingService.Warning("Transfer records directory does not exist");
                    return 0;
                }

                var jsonFiles = Directory.GetFiles(_transferRecordsDirectory, "*.json")
                    .OrderBy(f => File.GetCreationTime(f))
                    .ToList();

                LoggingService.Info($"Found {jsonFiles.Count} JSON transfer logs to migrate");

                var transfers = new List<TransferLog>();

                foreach (var jsonFile in jsonFiles)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(jsonFile);
                        var transfer = JsonSerializer.Deserialize<TransferLog>(json);

                        if (transfer != null)
                        {
                            transfers.Add(transfer);
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warning($"Failed to parse JSON log: {jsonFile} - {ex.Message}");
                    }
                }

                // Batch insert into database
                if (transfers.Count > 0)
                {
                    if (_databaseService.AddTransfers(transfers))
                    {
                        migratedCount = transfers.Count;
                        LoggingService.Success($"Successfully migrated {migratedCount} transfer records to database");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error("Error migrating JSON logs to database", ex);
            }

            return migratedCount;
        }

        /// <summary>
        /// Gets the database file path.
        /// </summary>
        public string GetDatabasePath()
        {
            return _databaseService.GetDatabasePath();
        }

        /// <summary>
        /// Cleans up old transfer records from database.
        /// </summary>
        public async Task<int> CleanupOldRecordsAsync(int retentionDays)
        {
            return await Task.Run(() => _databaseService.CleanupOldTransfers(retentionDays));
        }

        /// <summary>
        /// Optimizes the database.
        /// </summary>
        public async Task OptimizeDatabaseAsync()
        {
            await Task.Run(() => _databaseService.OptimizeDatabase());
        }

        public void Dispose()
        {
            _databaseService?.Dispose();
        }
    }
}
