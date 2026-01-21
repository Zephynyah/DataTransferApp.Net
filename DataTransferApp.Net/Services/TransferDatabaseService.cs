using DataTransferApp.Net.Models;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DataTransferApp.Net.Services
{
    /// <summary>
    /// Service for managing transfer history using LiteDB.
    /// Provides centralized storage for all transfer records.
    /// </summary>
    public class TransferDatabaseService : IDisposable
    {
        private readonly LiteDatabase _database;
        private readonly ILiteCollection<TransferLog> _transfersCollection;
        private readonly string _databasePath;

        public TransferDatabaseService(string? databasePath = null)
        {
            // Determine database path
            _databasePath = ResolveDatabasePath(databasePath);
            
            // Ensure directory exists
            var directory = Path.GetDirectoryName(_databasePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Initialize LiteDB
            _database = new LiteDatabase(_databasePath);
            _transfersCollection = _database.GetCollection<TransferLog>("transfers");
            
            // Create indexes for better query performance
            _transfersCollection.EnsureIndex(x => x.TransferInfo.Date);
            _transfersCollection.EnsureIndex(x => x.TransferInfo.FolderName);
            _transfersCollection.EnsureIndex(x => x.TransferInfo.Employee);
            _transfersCollection.EnsureIndex(x => x.TransferInfo.DTA);

            LoggingService.Info($"Transfer database initialized: {_databasePath}");
        }

        private string ResolveDatabasePath(string? customPath)
        {
            if (!string.IsNullOrWhiteSpace(customPath))
            {
                return customPath;
            }

            // Default: use a shared location or fall back to local
            var sharedPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "DataTransferApp",
                "TransferHistory.db"
            );

            try
            {
                // Test if we can write to shared location
                var testDir = Path.GetDirectoryName(sharedPath);
                if (!string.IsNullOrEmpty(testDir))
                {
                    Directory.CreateDirectory(testDir);
                    return sharedPath;
                }
            }
            catch
            {
                LoggingService.Warning("Cannot access shared database location, using local database");
            }

            // Fallback to user's local app data
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DataTransferApp",
                "TransferHistory.db"
            );
        }

        /// <summary>
        /// Adds a new transfer record to the database.
        /// </summary>
        public bool AddTransfer(TransferLog transfer)
        {
            try
            {
                _transfersCollection.Insert(transfer);
                LoggingService.Info($"Transfer record added to database: {transfer.TransferInfo.FolderName}");
                return true;
            }
            catch (Exception ex)
            {
                LoggingService.Error("Error adding transfer to database", ex);
                return false;
            }
        }

        /// <summary>
        /// Adds multiple transfer records in a batch operation.
        /// </summary>
        public bool AddTransfers(IEnumerable<TransferLog> transfers)
        {
            try
            {
                _transfersCollection.InsertBulk(transfers);
                LoggingService.Info($"Batch added {transfers.Count()} transfer records to database");
                return true;
            }
            catch (Exception ex)
            {
                LoggingService.Error("Error adding transfers batch to database", ex);
                return false;
            }
        }

        /// <summary>
        /// Retrieves all transfer records.
        /// </summary>
        public List<TransferLog> GetAllTransfers()
        {
            try
            {
                return _transfersCollection.FindAll().ToList();
            }
            catch (Exception ex)
            {
                LoggingService.Error("Error retrieving all transfers from database", ex);
                return new List<TransferLog>();
            }
        }

        /// <summary>
        /// Retrieves a transfer record by its ID.
        /// </summary>
        public TransferLog? GetTransferById(ObjectId id)
        {
            try
            {
                return _transfersCollection.FindById(id);
            }
            catch (Exception ex)
            {
                LoggingService.Error($"Error retrieving transfer by ID: {id}", ex);
                return null;
            }
        }

        /// <summary>
        /// Searches for transfers matching the search term.
        /// </summary>
        public List<TransferLog> SearchTransfers(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return GetAllTransfers();
                }

                searchTerm = searchTerm.ToLower();
                
                return _transfersCollection.Find(t =>
                    t.TransferInfo.FolderName.ToLower().Contains(searchTerm) ||
                    t.TransferInfo.Employee.ToLower().Contains(searchTerm) ||
                    t.TransferInfo.DTA.ToLower().Contains(searchTerm) ||
                    t.TransferInfo.Origin.ToLower().Contains(searchTerm) ||
                    t.TransferInfo.Destination.ToLower().Contains(searchTerm)
                ).ToList();
            }
            catch (Exception ex)
            {
                LoggingService.Error("Error searching transfers in database", ex);
                return new List<TransferLog>();
            }
        }

        /// <summary>
        /// Retrieves transfers within a date range.
        /// </summary>
        public List<TransferLog> GetTransfersByDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                return _transfersCollection.Find(t =>
                    t.TransferInfo.Date >= startDate &&
                    t.TransferInfo.Date <= endDate
                ).ToList();
            }
            catch (Exception ex)
            {
                LoggingService.Error("Error retrieving transfers by date range from database", ex);
                return new List<TransferLog>();
            }
        }

        /// <summary>
        /// Retrieves the most recent transfers.
        /// </summary>
        public List<TransferLog> GetRecentTransfers(int count = 10)
        {
            try
            {
                return _transfersCollection
                    .Find(Query.All(Query.Descending))
                    .OrderByDescending(t => t.TransferInfo.Date)
                    .Take(count)
                    .ToList();
            }
            catch (Exception ex)
            {
                LoggingService.Error("Error retrieving recent transfers from database", ex);
                return new List<TransferLog>();
            }
        }

        /// <summary>
        /// Retrieves transfers by employee ID.
        /// </summary>
        public List<TransferLog> GetTransfersByEmployee(string employeeId)
        {
            try
            {
                return _transfersCollection.Find(t =>
                    t.TransferInfo.Employee == employeeId
                ).ToList();
            }
            catch (Exception ex)
            {
                LoggingService.Error($"Error retrieving transfers for employee: {employeeId}", ex);
                return new List<TransferLog>();
            }
        }

        /// <summary>
        /// Retrieves transfers by Data Transfer Agent.
        /// </summary>
        public List<TransferLog> GetTransfersByDTA(string dta)
        {
            try
            {
                return _transfersCollection.Find(t =>
                    t.TransferInfo.DTA == dta
                ).ToList();
            }
            catch (Exception ex)
            {
                LoggingService.Error($"Error retrieving transfers for DTA: {dta}", ex);
                return new List<TransferLog>();
            }
        }

        /// <summary>
        /// Gets transfer statistics.
        /// </summary>
        public Dictionary<string, int> GetTransferStatistics()
        {
            try
            {
                var transfers = GetAllTransfers();
                var today = DateTime.Today;

                return new Dictionary<string, int>
                {
                    ["TotalTransfers"] = transfers.Count,
                    ["TodayTransfers"] = transfers.Count(t => t.TransferInfo.Date.Date == today),
                    ["ThisWeekTransfers"] = transfers.Count(t => t.TransferInfo.Date >= today.AddDays(-7)),
                    ["ThisMonthTransfers"] = transfers.Count(t =>
                        t.TransferInfo.Date.Month == today.Month &&
                        t.TransferInfo.Date.Year == today.Year),
                    ["TotalFiles"] = transfers.Sum(t => t.Summary.TotalFiles)
                };
            }
            catch (Exception ex)
            {
                LoggingService.Error("Error calculating transfer statistics", ex);
                return new Dictionary<string, int>();
            }
        }

        /// <summary>
        /// Updates an existing transfer record.
        /// </summary>
        public bool UpdateTransfer(TransferLog transfer)
        {
            try
            {
                return _transfersCollection.Update(transfer);
            }
            catch (Exception ex)
            {
                LoggingService.Error("Error updating transfer in database", ex);
                return false;
            }
        }

        /// <summary>
        /// Deletes a transfer record by ID.
        /// </summary>
        public bool DeleteTransfer(ObjectId id)
        {
            try
            {
                return _transfersCollection.Delete(id);
            }
            catch (Exception ex)
            {
                LoggingService.Error($"Error deleting transfer from database: {id}", ex);
                return false;
            }
        }

        /// <summary>
        /// Deletes old transfer records based on retention period.
        /// </summary>
        public int CleanupOldTransfers(int retentionDays)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-retentionDays);
                var oldTransfers = _transfersCollection.Find(t => t.TransferInfo.Date < cutoffDate);
                var count = 0;

                foreach (var transfer in oldTransfers)
                {
                    if (_transfersCollection.Delete(transfer.Id))
                    {
                        count++;
                    }
                }

                if (count > 0)
                {
                    LoggingService.Info($"Cleaned up {count} old transfer records (older than {retentionDays} days)");
                }

                return count;
            }
            catch (Exception ex)
            {
                LoggingService.Error("Error cleaning up old transfers", ex);
                return 0;
            }
        }

        /// <summary>
        /// Gets the total count of transfer records.
        /// </summary>
        public int GetTotalCount()
        {
            try
            {
                return _transfersCollection.Count();
            }
            catch (Exception ex)
            {
                LoggingService.Error("Error getting transfer count", ex);
                return 0;
            }
        }

        /// <summary>
        /// Gets the database file path.
        /// </summary>
        public string GetDatabasePath() => _databasePath;

        /// <summary>
        /// Optimizes the database by shrinking and defragmenting.
        /// </summary>
        public void OptimizeDatabase()
        {
            try
            {
                _database.Rebuild();
                LoggingService.Info("Database optimized successfully");
            }
            catch (Exception ex)
            {
                LoggingService.Error("Error optimizing database", ex);
            }
        }

        public void Dispose()
        {
            _database?.Dispose();
        }
    }
}
