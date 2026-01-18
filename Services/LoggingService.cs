using Serilog;
using Serilog.Events;
using System;
using System.IO;

namespace DataTransferApp.Net.Services
{
    public class LoggingService
    {
        private static ILogger? _logger;
        private static readonly object _lock = new();
        
        public static void Initialize(string logFilePath, LogEventLevel minLevel = LogEventLevel.Information)
        {
            lock (_lock)
            {
                if (_logger != null)
                    return;
                
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
                        fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
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
                _logger?.Error(exception, message);
            else
                _logger?.Error(message);
        }
        
        public static void Success(string message)
        {
            _logger?.Information($"✓ {message}");
        }
        
        public static void Dispose()
        {
            (_logger as IDisposable)?.Dispose();
        }
        
        public static LogEventLevel ParseLogLevel(string level)
        {
            return level.ToLower() switch
            {
                "debug" => LogEventLevel.Debug,
                "info" or "information" => LogEventLevel.Information,
                "warning" or "warn" => LogEventLevel.Warning,
                "error" => LogEventLevel.Error,
                _ => LogEventLevel.Information
            };
        }
    }
}
