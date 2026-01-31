using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DataTransferApp.Net.Models;

namespace DataTransferApp.Net.Services
{
    /// <summary>
    /// Service for managing transfer history.
    /// Uses LiteDB database as primary storage.
    /// </summary>
    public class TransferHistoryService : IDisposable
    {
        private readonly TransferDatabaseService _databaseService;

        public TransferHistoryService(string? databasePath = null)
        {
            _databaseService = new TransferDatabaseService(databasePath);
        }

        /// <summary>
        /// Gets all transfers from the database.
        /// </summary>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        public async Task<IList<TransferLog>> GetAllTransfersAsync()
        {
            return await Task.Run(() => _databaseService.GetAllTransfers());
        }

        /// <summary>
        /// Searches for transfers matching the search term.
        /// </summary>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        public async Task<IList<TransferLog>> SearchTransfersAsync(string searchTerm)
        {
            return await Task.Run(() => _databaseService.SearchTransfers(searchTerm));
        }

        /// <summary>
        /// Gets a transfer by its ID.
        /// </summary>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        public async Task<TransferLog?> GetTransferByIdAsync(string id)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var objectId = new LiteDB.ObjectId(id);
                    return _databaseService.GetTransferById(objectId);
                }
                catch
                {
                    return null;
                }
            });
        }

        /// <summary>
        /// Gets transfers within a date range.
        /// </summary>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        public async Task<IList<TransferLog>> GetTransfersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() => _databaseService.GetTransfersByDateRange(startDate, endDate));
        }

        /// <summary>
        /// Gets the most recent transfers.
        /// </summary>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        public async Task<IList<TransferLog>> GetRecentTransfersAsync(int count = 10)
        {
            return await Task.Run(() => _databaseService.GetRecentTransfers(count));
        }

        /// <summary>
        /// Gets transfer statistics.
        /// </summary>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        public async Task<Dictionary<string, int>> GetTransferStatisticsAsync()
        {
            return await Task.Run(() => _databaseService.GetTransferStatistics());
        }

        /// <summary>
        /// Deletes a transfer by its ID.
        /// </summary>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        public async Task<bool> DeleteTransferAsync(string id)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var objectId = new LiteDB.ObjectId(id);
                    return _databaseService.DeleteTransfer(objectId);
                }
                catch
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// Migrates existing JSON transfer logs to the database.
        /// </summary>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        public async Task<int> MigrateJsonLogsToDatabase(string transferRecordsDirectory)
        {
            var migratedCount = 0;

            try
            {
                if (!Directory.Exists(transferRecordsDirectory))
                {
                    LoggingService.Warning("Transfer records directory does not exist");
                    return 0;
                }

                var jsonFiles = Directory.GetFiles(transferRecordsDirectory, "*.json")
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
        /// <returns></returns>
        public string GetDatabasePath()
        {
            return _databaseService.GetDatabasePath();
        }

        /// <summary>
        /// Cleans up old transfer records from database.
        /// </summary>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        public async Task<int> CleanupOldRecordsAsync(int retentionDays)
        {
            return await Task.Run(() => _databaseService.CleanupOldTransfers(retentionDays));
        }

        /// <summary>
        /// Optimizes the database.
        /// </summary>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
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