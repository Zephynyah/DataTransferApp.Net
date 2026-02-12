using System.IO;
using Serilog;
using Serilog.Events;

namespace DataTransferApp.Net.Services
{
    public static class LoggingService
    {
        private static readonly Lock _lock = new();
        private static ILogger? _logger;

        public static void Initialize(string logFilePath, LogEventLevel minLevel = LogEventLevel.Information)
        {
            using (_lock.EnterScope())
            {
                if (_logger != null)
                {
                    return;
                }

                // Ensure directory exists
                var logDir = Path.GetDirectoryName(logFilePath);
                if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                _logger = new LoggerConfiguration()
                    .MinimumLevel.Is(minLevel)
                    .WriteTo.File(
                        logFilePath,
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        fileSizeLimitBytes: AppConstants.MaxLogFileSizeBytes, // 10MB
                        retainedFileCountLimit: 5,
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .CreateLogger();
            }
        }

        public static void Debug(string message)
        {
            _logger?.Debug(message);
        }

        public static void Info(string message)
        {
            _logger?.Information(message);
        }

        public static void Warning(string message)
        {
            _logger?.Warning(message);
        }

        public static void Error(string message, Exception? exception = null)
        {
            if (exception != null)
            {
                _logger?.Error(exception, message);
            }
            else
            {
                _logger?.Error(message);
            }
        }

        public static void Success(string message)
        {
            _logger?.Information("✓ {Message}", message);
        }

        public static void Shutdown()
        {
            (_logger as IDisposable)?.Dispose();
        }

        public static LogEventLevel ParseLogLevel(string level)
        {
            return level.ToLowerInvariant() switch
            {
                "debug" => LogEventLevel.Debug,
                "info" or "information" => LogEventLevel.Information,
                "warning" or "warn" => LogEventLevel.Warning,
                "error" => LogEventLevel.Error,
                _ => LogEventLevel.Information
            };
        }

        /// <summary>
        /// Cleans up old log files in the specified directory.
        /// </summary>
        /// <param name="logDirectory">Directory containing log files to clean up.</param>
        /// <param name="retentionDays">Number of days to retain logs.</param>
        /// <param name="filePattern">File pattern to match (e.g., "*.log" or "app-*.log").</param>
        public static void CleanupOldLogs(string logDirectory, int retentionDays, string filePattern = "*.log")
        {
            try
            {
                if (!Directory.Exists(logDirectory))
                {
                    return;
                }

                var cutoffDate = DateTime.Now.AddDays(-retentionDays);
                var logFiles = Directory.GetFiles(logDirectory, filePattern);
                var deletedCount = 0;
                long totalSizeDeleted = 0;

                foreach (var logFile in logFiles)
                {
                    try
                    {
                        var fileInfo = new FileInfo(logFile);
                        if (fileInfo.LastWriteTime < cutoffDate)
                        {
                            var fileSize = fileInfo.Length;
                            File.Delete(logFile);
                            deletedCount++;
                            totalSizeDeleted += fileSize;
                            Debug($"Deleted old log file: {Path.GetFileName(logFile)} ({fileInfo.LastWriteTime:yyyy-MM-dd})");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue cleanup
                        Debug($"Failed to delete log file {Path.GetFileName(logFile)}: {ex.Message}");
                    }
                }

                if (deletedCount > 0)
                {
                    var sizeMB = totalSizeDeleted / (1024.0 * 1024.0);
                    Info($"Log cleanup: Deleted {deletedCount} old log file(s) ({sizeMB:F2} MB) older than {retentionDays} days");
                }
                else
                {
                    Debug($"Log cleanup: No old log files found (retention: {retentionDays} days)");
                }
            }
            catch (Exception ex)
            {
                Warning($"Error during log cleanup: {ex.Message}");
            }
        }
    }
}