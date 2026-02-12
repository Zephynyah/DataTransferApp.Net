using System.IO;
using DataTransferApp.Net.Models;
using LiteDB;

namespace DataTransferApp.Net.Services
{
    /// <summary>
    /// Service for managing transfer history using LiteDB.
    /// Provides centralized storage for all transfer records.
    /// Uses connection-per-operation pattern to avoid file locking issues.
    /// </summary>
    public class TransferDatabaseService : IDisposable
    {
        private readonly string _databasePath;
        private readonly string _connectionString;

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

            // Configure connection string with Shared mode to allow multiple concurrent accesses
            _connectionString = $"Filename={_databasePath};Mode=Shared";

            // Initialize indexes on first run
            InitializeDatabase();

            LoggingService.Info($"Transfer database initialized: {_databasePath}");
        }

        /// <summary>
        /// Adds a new transfer record to the database.
        /// </summary>
        /// <param name="transfer">The transfer record to add to the database.</param>
        /// <returns>True if the transfer was successfully added; otherwise false.</returns>
        public bool AddTransfer(TransferLog transfer)
        {
            try
            {
                LoggingService.Info($"Attempting to save transfer to database: {transfer.TransferInfo.FolderName}");
                LoggingService.Info($"Database path: {_databasePath}");
                LoggingService.Info($"Connection string: {_connectionString}");

                using var db = new LiteDatabase(_connectionString);
                var collection = db.GetCollection<TransferLog>("transfers");
                collection.Insert(transfer);

                LoggingService.Success($"Transfer record successfully added to database: {transfer.TransferInfo.FolderName}");
                return true;
            }
            catch (Exception ex)
            {
                LoggingService.Error($"Error adding transfer to database: {transfer.TransferInfo.FolderName}", ex);
                LoggingService.Error($"Exception type: {ex.GetType().Name}");
                LoggingService.Error($"Exception message: {ex.Message}");
                LoggingService.Error($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Adds multiple transfer records in a batch operation.
        /// </summary>
        /// <param name="transfers">The collection of transfer records to add to the database.</param>
        /// <returns>True if all transfers were successfully added; otherwise false.</returns>
        public bool AddTransfers(IEnumerable<TransferLog> transfers)
        {
            try
            {
                using var db = new LiteDatabase(_connectionString);
                var collection = db.GetCollection<TransferLog>("transfers");
                collection.InsertBulk(transfers);
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
        /// <returns>A list of all transfer records in the database.</returns>
        public IList<TransferLog> GetAllTransfers()
        {
            const int maxRetries = 3;
            const int delayMs = 500;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using var db = new LiteDatabase(_connectionString);
                    var collection = db.GetCollection<TransferLog>("transfers");
                    return collection.FindAll().ToList();
                }
                catch (IOException ex) when (ex.Message.Contains("being used by another process"))
                {
                    if (attempt < maxRetries)
                    {
                        // Only log debug message on retry attempts to reduce noise
                        LoggingService.Debug($"Database file locked, attempt {attempt}/{maxRetries}, retrying...");
                        System.Threading.Thread.Sleep(delayMs);
                    }
                    else
                    {
                        LoggingService.Warning($"Database file still locked after {maxRetries} attempts. Some data may be temporarily unavailable.");
                        LoggingService.Debug($"Details: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    LoggingService.Error("Error retrieving all transfers from database", ex);
                    return new List<TransferLog>();
                }
            }

            return new List<TransferLog>();
        }

        /// <summary>
        /// Retrieves a transfer record by its ID.
        /// </summary>
        /// <param name="id">The ObjectId of the transfer record to retrieve.</param>
        /// <returns>The transfer record if found; otherwise null.</returns>
        public TransferLog? GetTransferById(ObjectId id)
        {
            try
            {
                using var db = new LiteDatabase(_connectionString);
                var collection = db.GetCollection<TransferLog>("transfers");
                return collection.FindById(id);
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
        /// <param name="searchTerm">The search term to match against folder name, employee, DTA, origin, and destination.</param>
        /// <returns>A list of transfer records matching the search criteria.</returns>
        public IList<TransferLog> SearchTransfers(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return GetAllTransfers();
                }

                searchTerm = searchTerm.ToLowerInvariant();

                using var db = new LiteDatabase(_connectionString);
                var collection = db.GetCollection<TransferLog>("transfers");
                return collection.Find(t =>
                    t.TransferInfo.FolderName.ToLowerInvariant().Contains(searchTerm) ||
                    t.TransferInfo.Employee.ToLowerInvariant().Contains(searchTerm) ||
                    t.TransferInfo.DTA.ToLowerInvariant().Contains(searchTerm) ||
                    t.TransferInfo.Origin.ToLowerInvariant().Contains(searchTerm) ||
                    t.TransferInfo.Destination.ToLowerInvariant().Contains(searchTerm))
                .ToList();
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
        /// <param name="startDate">The start date of the range to retrieve transfers from.</param>
        /// <param name="endDate">The end date of the range to retrieve transfers from.</param>
        /// <returns>A list of transfer records within the specified date range.</returns>
        public IList<TransferLog> GetTransfersByDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var db = new LiteDatabase(_connectionString);
                var collection = db.GetCollection<TransferLog>("transfers");
                return collection.Find(t =>
                    t.TransferInfo.Date >= startDate &&
                    t.TransferInfo.Date <= endDate)
                .ToList();
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
        /// <param name="count">The maximum number of recent transfers to retrieve. Default is 10.</param>
        /// <returns>A list of the most recent transfer records.</returns>
        public IList<TransferLog> GetRecentTransfers(int count = 10)
        {
            try
            {
                using var db = new LiteDatabase(_connectionString);
                var collection = db.GetCollection<TransferLog>("transfers");
                return collection
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
        /// <param name="employeeId">The employee ID to filter transfers by.</param>
        /// <returns>A list of transfer records for the specified employee.</returns>
        public IList<TransferLog> GetTransfersByEmployee(string employeeId)
        {
            try
            {
                using var db = new LiteDatabase(_connectionString);
                var collection = db.GetCollection<TransferLog>("transfers");
                return collection.Find(t =>
                    t.TransferInfo.Employee == employeeId)
                .ToList();
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
        /// <param name="dta">The Data Transfer Agent name to filter transfers by.</param>
        /// <returns>A list of transfer records for the specified Data Transfer Agent.</returns>
        public IList<TransferLog> GetTransfersByDTA(string dta)
        {
            try
            {
                using var db = new LiteDatabase(_connectionString);
                var collection = db.GetCollection<TransferLog>("transfers");
                return collection.Find(t =>
                    t.TransferInfo.DTA == dta)
                .ToList();
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
        /// <returns>A dictionary containing transfer statistics including total, today, this week, and this month transfers.</returns>
        public IDictionary<string, int> GetTransferStatistics()
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
                return new Dictionary<string, int>
                {
                    ["TotalTransfers"] = 0,
                    ["TodayTransfers"] = 0,
                    ["ThisWeekTransfers"] = 0,
                    ["ThisMonthTransfers"] = 0,
                    ["TotalFiles"] = 0
                };
            }
        }

        /// <summary>
        /// Updates an existing transfer record.
        /// </summary>
        /// <param name="transfer">The transfer record to update in the database.</param>
        /// <returns>True if the transfer was successfully updated; otherwise false.</returns>
        public bool UpdateTransfer(TransferLog transfer)
        {
            try
            {
                using var db = new LiteDatabase(_connectionString);
                var collection = db.GetCollection<TransferLog>("transfers");
                return collection.Update(transfer);
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
        /// <param name="id">The ObjectId of the transfer record to delete.</param>
        /// <returns>True if the transfer was successfully deleted; otherwise false.</returns>
        public bool DeleteTransfer(ObjectId id)
        {
            const int maxRetries = 3;
            const int delayMs = 500;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using var db = new LiteDatabase(_connectionString);
                    var collection = db.GetCollection<TransferLog>("transfers");
                    return collection.Delete(id);
                }
                catch (IOException ex) when (ex.Message.Contains("being used by another process"))
                {
                    if (attempt < maxRetries)
                    {
                        LoggingService.Debug($"Database file locked during delete, attempt {attempt}/{maxRetries}, retrying...");
                        System.Threading.Thread.Sleep(delayMs);
                    }
                    else
                    {
                        LoggingService.Warning($"Database file still locked after {maxRetries} attempts during delete operation.");
                        LoggingService.Debug($"Details: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    LoggingService.Error($"Error deleting transfer from database: {id}", ex);
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Deletes old transfer records based on retention period.
        /// </summary>
        /// <param name="retentionDays">The number of days to retain transfer records. Records older than this will be deleted.</param>
        /// <returns>The number of transfer records that were deleted.</returns>
        public int CleanupOldTransfers(int retentionDays)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-retentionDays);
                using var db = new LiteDatabase(_connectionString);
                var collection = db.GetCollection<TransferLog>("transfers");
                var oldTransfers = collection.Find(t => t.TransferInfo.Date < cutoffDate);
                var count = oldTransfers.Count(transfer => collection.Delete(transfer.Id));

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
        /// <returns>The total number of transfer records in the database.</returns>
        public int GetTotalCount()
        {
            try
            {
                using var db = new LiteDatabase(_connectionString);
                var collection = db.GetCollection<TransferLog>("transfers");
                return collection.Count();
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
        /// <returns>The full path to the database file.</returns>
        public string GetDatabasePath() => _databasePath;

        /// <summary>
        /// Optimizes the database by shrinking and defragmenting.
        /// </summary>
        public void OptimizeDatabase()
        {
            try
            {
                using var db = new LiteDatabase(_connectionString);
                db.Rebuild();
                LoggingService.Info("Database optimized successfully");
            }
            catch (Exception ex)
            {
                LoggingService.Error("Error optimizing database", ex);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // No persistent connection to dispose with connection-per-operation pattern
            // No unmanaged resources to release
        }

        private static string ResolveDatabasePath(string? customPath)
        {
            if (!string.IsNullOrWhiteSpace(customPath))
            {
                // If it's a directory, append TransferHistory.db
                if (Directory.Exists(customPath))
                {
                    customPath = Path.Combine(customPath, "TransferHistory.db");
                    LoggingService.Info($"Directory provided, using: {customPath}");
                }

                // If no extension, assume it's a directory and append filename
                else if (!Path.HasExtension(customPath))
                {
                    customPath = Path.Combine(customPath, "TransferHistory.db");
                    LoggingService.Info($"No extension provided, using: {customPath}");
                }

                return customPath;
            }

            // Default: use a shared location or fall back to local
            var sharedPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "DataTransferApp",
                "TransferHistory.db");

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
                "TransferHistory.db");
        }

        private void InitializeDatabase()
        {
            try
            {
                using var db = new LiteDatabase(_connectionString);
                var collection = db.GetCollection<TransferLog>("transfers");
                collection.EnsureIndex(x => x.TransferInfo.Date);
                collection.EnsureIndex(x => x.TransferInfo.FolderName);
                collection.EnsureIndex(x => x.TransferInfo.DTA);
            }
            catch (Exception ex)
            {
                LoggingService.Error("Error initializing database indexes", ex);
            }
        }
    }
}